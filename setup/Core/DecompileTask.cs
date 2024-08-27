using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;
using AssemblyReference = ICSharpCode.Decompiler.Metadata.AssemblyReference;

namespace Terraria.ModLoader.Setup.Core
{
	public class DecompileTask : SetupOperation
	{
		public static readonly Version clientVersion = new("1.4.4.9");
		public static readonly Version serverVersion = new("1.4.4.9");

		// memoized
		private static readonly ConditionalWeakTable<PEFile, string?> assemblyTitleCache = new();

		private static readonly string[] knownAttributes = {
			nameof(AssemblyCompanyAttribute), nameof(AssemblyCopyrightAttribute), nameof(AssemblyTitleAttribute),
		};

		private readonly DecompilerSettings decompilerSettings;

		private readonly ProgramSettings programSettings;
		private readonly TerrariaExecutableSetter terrariaExecutableSetter;
		private readonly DecompileTaskParameters parameters;

		private ExtendedProjectDecompiler? projectDecompiler;

		public DecompileTask(DecompileTaskParameters parameters, IServiceProvider serviceProvider)
		{
			this.programSettings = serviceProvider.GetRequiredService<ProgramSettings>();
			this.terrariaExecutableSetter = serviceProvider.GetRequiredService<TerrariaExecutableSetter>();
			this.parameters = parameters with { SrcDir = PathUtils.SetCrossPlatformDirectorySeparators(parameters.SrcDir) };

			CSharpFormattingOptions formatting = FormattingOptionsFactory.CreateKRStyle();

			// Arrays should have a new line for every entry, since it's easier to insert values in patches that way.
			formatting.ArrayInitializerWrapping = Wrapping.WrapAlways;
			formatting.ArrayInitializerBraceStyle = BraceStyle.EndOfLine;

			// Force wrapping for chained calls for the same reason.
			// Hm, doesn't work.
			//formatting.ChainedMethodCallWrapping = Wrapping.WrapAlways;

			decompilerSettings = new DecompilerSettings(LanguageVersion.Latest) {
				RemoveDeadCode = true,
				CSharpFormattingOptions = formatting,

				// Switch expressions are not patching-friendly,
				// and do not even support expression bodies at this time:
				// https://github.com/dotnet/csharplang/issues/3037
				SwitchExpressions = false,
			};
		}

		public override async ValueTask<bool> ConfigurationPrompt(CancellationToken cancellationToken = default)
		{
			if (File.Exists(programSettings.TerrariaPath) && File.Exists(programSettings.TerrariaServerPath)) {
				return true;
			}

			if (programSettings.NoPrompts) {
				throw new InvalidOperationException($"Critical failure, can't find both {programSettings.TerrariaPath} and {programSettings.TerrariaServerPath}");
			}

			return await terrariaExecutableSetter.SelectAndSetTerrariaDirectory(cancellationToken).ConfigureAwait(false);
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = progress.StartTask("Decompiling Terraria...");
			taskProgress.ReportStatus("Deleting Old Src");
			if (Directory.Exists(parameters.SrcDir)) {
				Directory.Delete(parameters.SrcDir, true);
			}

			PEFile? clientModule = parameters.ServerOnly ? null : ReadModule(programSettings.TerrariaPath!, clientVersion, taskProgress);
			PEFile? serverModule = ReadModule(programSettings.TerrariaServerPath!, serverVersion, taskProgress);
			PEFile mainModule = (parameters.ServerOnly ? serverModule : clientModule)!;

			var embeddedAssemblyResolver = new EmbeddedAssemblyResolver(mainModule, mainModule.DetectTargetFrameworkId());

			projectDecompiler = new ExtendedProjectDecompiler(decompilerSettings, embeddedAssemblyResolver);

			var items = new List<WorkItem>();
			var files = new HashSet<string>();
			var resources = new HashSet<string>();
			var exclude = new List<string>();

			// Decompile embedded library sources directly into Terraria project. Treated the same as Terraria source
			string[] decompiledLibraries = ["ReLogic"];
			foreach (string lib in decompiledLibraries) {
				Resource libRes = mainModule.Resources.Single(r => r.Name.EndsWith(lib + ".dll"));
				AddEmbeddedLibrary(libRes, projectDecompiler.AssemblyResolver, items);
				exclude.Add(GetOutputPath(libRes.Name, mainModule));
			}

			if (!parameters.ServerOnly) {
				AddModule(clientModule!, projectDecompiler.AssemblyResolver, items, files, resources, exclude);
			}

			AddModule(serverModule, projectDecompiler.AssemblyResolver, items, files, resources, exclude, parameters.ServerOnly ? null : "SERVER");

			items.Add(WriteTerrariaProjectFile(mainModule, files, resources, decompiledLibraries));
			items.Add(WriteCommonConfigurationFile());

			await ExecuteParallel(items, taskProgress, maxDegreeOfParallelism: parameters.MaxDegreeOfParallelism, cancellationToken: cancellationToken).ConfigureAwait(false);
		}

		private void AddEmbeddedLibrary(
			Resource res,
			IAssemblyResolver resolver,
			List<WorkItem> items)
		{
			using Stream s = res.TryOpenStream()!;
			s.Position = 0;
			var module = new PEFile(res.Name, s, PEStreamOptions.PrefetchEntireImage);

			var files = new HashSet<string>();
			var resources = new HashSet<string>();
			AddModule(module, resolver, items, files, resources);
			items.Add(WriteProjectFile(module, "Library", files, resources, w => {
				// references
				w.WriteStartElement("ItemGroup");
				foreach (AssemblyReference r in module.AssemblyReferences.OrderBy(r => r.Name)) {
					if (r.Name == "mscorlib") {
						continue;
					}

					w.WriteStartElement("Reference");
					w.WriteAttributeString("Include", r.Name);
					w.WriteEndElement();
				}

				w.WriteEndElement(); // </ItemGroup>

				// TODO: resolve references to embedded terraria libraries with their HintPath
			}));
		}

		protected PEFile ReadModule(string path, Version version, ITaskProgress taskProgress)
		{
			bool usingVersionedPath = false;
			string versionedPath = path.Insert(path.LastIndexOf('.'), $"_v{version}");
			if (File.Exists(versionedPath)) {
				path = versionedPath;
				usingVersionedPath = true;
			}

			taskProgress.ReportStatus("Loading " + Path.GetFileName(path));
			using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var module = new PEFile(path, fileStream, PEStreamOptions.PrefetchEntireImage);
			var assemblyName = new AssemblyName(module.FullName);
			if (assemblyName.Version != version) {
				throw new Exception($"{assemblyName.Name} version {assemblyName.Version}. Expected {version}");
			}

			if (!usingVersionedPath) {
				taskProgress.ReportStatus("Backup up " + Path.GetFileName(path) + " to " + Path.GetFileName(versionedPath));
				File.Copy(path, versionedPath);
			}

			return module;
		}

		private static string GetAssemblyTitle(PEFile module)
		{
			if (!assemblyTitleCache.TryGetValue(module, out string? title)) {
				assemblyTitleCache.Add(module, title = GetCustomAttributes(module)[nameof(AssemblyTitleAttribute)]);
			}

			return title!;
		}

		private static bool IsCultureFile(string path)
		{
			if (!path.Contains('-')) {
				return false;
			}

			try {
				CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(path));
				return true;
			}
			catch (CultureNotFoundException) { }

			return false;
		}


		private static string GetOutputPath(string path, PEFile module)
		{
			if (path.EndsWith(".dll")) {
				AssemblyReference? asmRef = module.AssemblyReferences.SingleOrDefault(r => path.EndsWith(r.Name + ".dll"));
				if (asmRef != null) {
					path = Path.Combine(path.Substring(0, path.Length - asmRef.Name.Length - 5), asmRef.Name + ".dll");
				}
			}

			string? rootNamespace = GetAssemblyTitle(module);
			if (path.StartsWith(rootNamespace)) {
				path = path.Substring(rootNamespace.Length + 1);
			}

			path = path.Replace("Libraries.", "Libraries/"); // lets leave the folder structure in here alone
			path = path.Replace('\\', '/');

			// . to /
			int stopFolderzingAt = path.IndexOf('/');
			if (stopFolderzingAt < 0) {
				stopFolderzingAt = path.LastIndexOf('.');
			}

			path = new StringBuilder(path).Replace(".", "/", 0, stopFolderzingAt).ToString();

			// default lang files should be called Main
			if (IsCultureFile(path)) {
				path = path.Insert(path.LastIndexOf('.'), "/Main");
			}

			return path;
		}

		private IEnumerable<IGrouping<string, TypeDefinitionHandle>> GetCodeFiles(PEFile module)
		{
			MetadataReader metadata = module.Metadata;
			return module.Metadata.GetTopLevelTypeDefinitions()
				.Where(td => projectDecompiler!.IncludeTypeWhenDecompilingProject(module, td))
				.GroupBy(h => {
					TypeDefinition type = metadata.GetTypeDefinition(h);
					string path = WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Name)) + ".cs";
					if (!string.IsNullOrEmpty(metadata.GetString(type.Namespace))) {
						path = Path.Combine(WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Namespace)),
							path);
					}

					return GetOutputPath(path, module);
				}, StringComparer.OrdinalIgnoreCase);
		}

		private static IEnumerable<(string path, Resource r)> GetResourceFiles(PEFile module) =>
			module.Resources.Where(r => r.ResourceType == ResourceType.Embedded)
				.Select(res => (GetOutputPath(res.Name, module), res));

		private DecompilerTypeSystem AddModule(
			PEFile module,
			IAssemblyResolver resolver,
			List<WorkItem> items,
			ISet<string> sourceSet,
			ISet<string> resourceSet,
			ICollection<string>? exclude = null,
			string? conditional = null)
		{
			string projectDir = GetAssemblyTitle(module);
			List<IGrouping<string, TypeDefinitionHandle>> sources = GetCodeFiles(module).ToList();
			List<(string path, Resource r)> resources = GetResourceFiles(module).ToList();
			if (exclude != null) {
				sources.RemoveAll(src => exclude.Contains(src.Key));
				resources.RemoveAll(res => exclude.Contains(res.path));
			}

			var ts = new DecompilerTypeSystem(module, resolver, decompilerSettings);
			items.AddRange(sources
				.Where(src => sourceSet.Add(src.Key))
				.Select(src => DecompileSourceFile(ts, src, projectDir, conditional)));

			if (conditional != null && resources.Any(res => !resourceSet.Contains(res.path))) {
				throw new Exception($"Conditional ({conditional}) resources not supported");
			}

			items.AddRange(resources
				.Where(res => resourceSet.Add(res.path))
				.Select(res => ExtractResource(res.path, res.r, projectDir)));

			return ts;
		}

		private WorkItem ExtractResource(string name, Resource res, string projectDir) =>
			new WorkItem("Extracting: " + name, async ct => {
				string path = Path.Combine(parameters.SrcDir, projectDir, name);
				CreateParentDirectory(path);

				Stream? s = res.TryOpenStream();
				s!.Position = 0;
				using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
				await s.CopyToAsync(fs, ct).ConfigureAwait(false);
			});

		private CSharpDecompiler CreateDecompiler(DecompilerTypeSystem ts, CancellationToken cancellationToken = default)
		{
			var decompiler =
				new CSharpDecompiler(ts, projectDecompiler!.Settings) { CancellationToken = cancellationToken };
			decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers());
			decompiler.AstTransforms.Add(new RemoveCLSCompliantAttribute());
			return decompiler;
		}

		private WorkItem DecompileSourceFile(
			DecompilerTypeSystem ts,
			IGrouping<string, TypeDefinitionHandle> src,
			string projectName,
			string? conditional = null) =>
			new WorkItem("Decompiling: " + src.Key, async (updateStatus, cancellationToken) => {
				string path = Path.Combine(parameters.SrcDir, projectName, src.Key);
				CreateParentDirectory(path);

				using var w = new StringWriter();
				if (conditional != null) {
					await w.WriteLineAsync("#if " + conditional).ConfigureAwait(false);
				}

				CreateDecompiler(ts, cancellationToken)
					.DecompileTypes(src.ToArray())
					.AcceptVisitor(new CSharpOutputVisitor(w, projectDecompiler!.Settings.CSharpFormattingOptions));

				if (conditional != null) {
					await w.WriteLineAsync("#endif").ConfigureAwait(false);
				}

				string source = w.ToString();
				if (programSettings.FormatAfterDecompiling) {
					updateStatus("Formatting: " + src.Key);
					source = FormatTask.Format(source, true, cancellationToken);
				}

				await File.WriteAllTextAsync(path, source, cancellationToken).ConfigureAwait(false);
			});

		private WorkItem WriteTerrariaProjectFile(
			PEFile module,
			IEnumerable<string> sources,
			IEnumerable<string> resources,
			ICollection<string> decompiledLibraries) =>
			WriteProjectFile(module, "WinExe", sources, resources, w => {
				//configurations
				w.WriteStartElement("PropertyGroup");
				w.WriteAttributeString("Condition", "$(Configuration.Contains('Server'))");
				w.WriteElementString("OutputType", "Exe");
				w.WriteElementString("OutputName", "$(OutputName)Server");
				w.WriteEndElement(); // </PropertyGroup>

				// references
				w.WriteStartElement("ItemGroup");

				AssemblyReference[] references = module.AssemblyReferences.Where(r => r.Name != "mscorlib")
					.OrderBy(r => r.Name).ToArray();
				AssemblyReference[] projectReferences = decompiledLibraries != null
					? references.Where(r => decompiledLibraries.Contains(r.Name)).ToArray()
					: Array.Empty<AssemblyReference>();
				AssemblyReference[] normalReferences = references.Except(projectReferences).ToArray();

				foreach (AssemblyReference r in projectReferences) {
					w.WriteStartElement("ProjectReference");
					w.WriteAttributeString("Include", $"../{r.Name}/{r.Name}.csproj");
					w.WriteEndElement();
				}

				foreach (AssemblyReference r in projectReferences) {
					w.WriteStartElement("EmbeddedResource");
					w.WriteAttributeString("Include", $"../{r.Name}/bin/$(Configuration)/$(TargetFramework)/{r.Name}.dll");
					w.WriteElementString("LogicalName", $"Terraria.Libraries.{r.Name}.{r.Name}.dll");
					w.WriteEndElement();
				}

				foreach (AssemblyReference r in normalReferences) {
					w.WriteStartElement("Reference");
					w.WriteAttributeString("Include", r.Name);
					w.WriteEndElement();
				}

				w.WriteEndElement(); // </ItemGroup>
			});

		private WorkItem WriteProjectFile(
			PEFile module,
			string outputType,
			IEnumerable<string> sources,
			IEnumerable<string> resources,
			Action<XmlTextWriter> writeSpecificConfig)
		{
			string name = GetAssemblyTitle(module);
			string filename = name + ".csproj";
			return new WorkItem("Writing: " + filename, async _ => {
				string path = Path.Combine(parameters.SrcDir, name, filename);
				CreateParentDirectory(path);

				using var sw = new StreamWriter(path);
				using XmlTextWriter w = CreateXmlWriter(sw);
				w.Formatting = System.Xml.Formatting.Indented;
				w.WriteStartElement("Project");
				w.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

				w.WriteStartElement("Import");
				w.WriteAttributeString("Project", "../Configuration.targets");
				w.WriteEndElement(); // </Import>

				w.WriteStartElement("PropertyGroup");
				w.WriteElementString("OutputType", outputType);
				w.WriteElementString("Version", new AssemblyName(module.FullName).Version?.ToString());

				IDictionary<string, string?> attribs = GetCustomAttributes(module);
				w.WriteElementString("Company", attribs[nameof(AssemblyCompanyAttribute)]);
				w.WriteElementString("Copyright", attribs[nameof(AssemblyCopyrightAttribute)]);

				w.WriteElementString("RootNamespace", module.Name);
				w.WriteEndElement(); // </PropertyGroup>

				writeSpecificConfig(w);

				// resources
				w.WriteStartElement("ItemGroup");
				foreach (string r in ApplyWildcards(resources, sources.ToArray()).OrderBy(r => r)) {
					w.WriteStartElement("EmbeddedResource");
					w.WriteAttributeString("Include", r);
					w.WriteEndElement();
				}

				w.WriteEndElement(); // </ItemGroup>
				w.WriteEndElement(); // </Project>

				await sw.WriteAsync(Environment.NewLine).ConfigureAwait(false);
			});
		}

		private WorkItem WriteCommonConfigurationFile()
		{
			string filename = "Configuration.targets";
			return new WorkItem("Writing: " + filename, async _ => {
				string path = Path.Combine(parameters.SrcDir, filename);
				CreateParentDirectory(path);

				using var sw = new StreamWriter(path);
				using XmlTextWriter w = CreateXmlWriter(sw);
				w.Formatting = System.Xml.Formatting.Indented;
				w.WriteStartElement("Project");

				w.WriteStartElement("PropertyGroup");
				w.WriteElementString("TargetFramework", "net40");
				w.WriteElementString("Configurations", "Debug;Release;ServerDebug;ServerRelease");
				w.WriteElementString("AssemblySearchPaths", "$(AssemblySearchPaths);{GAC}");
				w.WriteElementString("PlatformTarget", "x86");
				w.WriteElementString("AllowUnsafeBlocks", "true");
				w.WriteElementString("Optimize", "true");
				w.WriteEndElement(); // </PropertyGroup>

				//configurations
				w.WriteStartElement("PropertyGroup");
				w.WriteAttributeString("Condition", "$(Configuration.Contains('Server'))");
				w.WriteElementString("DefineConstants", "$(DefineConstants);SERVER");
				w.WriteEndElement(); // </PropertyGroup>

				w.WriteStartElement("PropertyGroup");
				w.WriteAttributeString("Condition", "!$(Configuration.Contains('Server'))");
				w.WriteElementString("DefineConstants", "$(DefineConstants);CLIENT");
				w.WriteEndElement(); // </PropertyGroup>

				w.WriteStartElement("PropertyGroup");
				w.WriteAttributeString("Condition", "$(Configuration.Contains('Debug'))");
				w.WriteElementString("Optimize", "false");
				w.WriteElementString("DefineConstants", "$(DefineConstants);DEBUG");
				w.WriteEndElement(); // </PropertyGroup>

				w.WriteEndElement(); // </Project>

				await sw.WriteAsync(Environment.NewLine).ConfigureAwait(false);
			});
		}

		private static XmlTextWriter CreateXmlWriter(StreamWriter streamWriter) =>
			new(streamWriter) { Formatting = System.Xml.Formatting.Indented, IndentChar = '\t', Indentation = 1 };

		private IEnumerable<string> ApplyWildcards(IEnumerable<string> include, IReadOnlyList<string> exclude)
		{
			var wildpaths = new HashSet<string>();
			foreach (string path in include) {
				if (wildpaths.Any(path.StartsWith)) {
					continue;
				}

				string wpath = path;
				string cards = "";
				while (wpath.Contains('/')) {
					string parent = wpath.Substring(0, wpath.LastIndexOf('/'));
					if (exclude.Any(e => e.StartsWith(parent))) {
						break; //can't use parent as a wildcard
					}

					wpath = parent;
					if (cards.Length < 2) {
						cards += "*";
					}
				}

				if (wpath != path) {
					wildpaths.Add(wpath);
					yield return $"{wpath}/{cards}";
				}
				else {
					yield return path;
				}
			}
		}

		private static IDictionary<string, string?> GetCustomAttributes(PEFile module)
		{
			var dict = new Dictionary<string, string?>();

			MetadataReader reader = module.Reader.GetMetadataReader();
			IEnumerable<CustomAttribute> attribs =
				reader.GetAssemblyDefinition().GetCustomAttributes().Select(reader.GetCustomAttribute);
			foreach (CustomAttribute attrib in attribs) {
				MemberReference ctor = reader.GetMemberReference((MemberReferenceHandle)attrib.Constructor);
				string attrTypeName = reader.GetString(reader.GetTypeReference((TypeReferenceHandle)ctor.Parent).Name);
				if (!knownAttributes.Contains(attrTypeName)) {
					continue;
				}

				CustomAttributeValue<object?> value = attrib.DecodeValue(new IDGAFAttributeTypeProvider());
				dict[attrTypeName] = value.FixedArguments.Single().Value as string;
			}

			return dict;
		}

		private sealed class EmbeddedAssemblyResolver : IAssemblyResolver
		{
			private readonly UniversalAssemblyResolver _resolver;
			private readonly PEFile baseModule;
			private readonly Dictionary<string, PEFile?> cache = new();

			public EmbeddedAssemblyResolver(PEFile baseModule, string targetFramework)
			{
				this.baseModule = baseModule;
				_resolver = new UniversalAssemblyResolver(baseModule.FileName, true, targetFramework,
					streamOptions: PEStreamOptions.PrefetchMetadata);
				_resolver.AddSearchDirectory(Path.GetDirectoryName(baseModule.FileName));
			}

			public PEFile? Resolve(IAssemblyReference name)
			{
				lock (this) {
					if (cache.TryGetValue(name.FullName, out PEFile? module)) {
						return module;
					}

					//look in the base module's embedded resources
					string resName = name.Name + ".dll";
					Resource? res = baseModule.Resources.Where(r => r.ResourceType == ResourceType.Embedded)
						.SingleOrDefault(r => r.Name.EndsWith(resName));

					if (res != null) {
						module = new PEFile(res.Name, res.TryOpenStream()!);
					}

					module ??= _resolver.Resolve(name);

					cache[name.FullName] = module;
					return module;
				}
			}

			public PEFile? ResolveModule(PEFile mainModule, string moduleName)
				=> _resolver.ResolveModule(mainModule, moduleName);

			public async Task<PEFile?> ResolveAsync(IAssemblyReference reference)
				=> await Task.Run(() => Resolve(reference)).ConfigureAwait(false);

			public async Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
				=> await Task.Run(() => ResolveModule(mainModule, moduleName)).ConfigureAwait(false);
		}

		// What function does this serve..?
		private class ExtendedProjectDecompiler : WholeProjectDecompiler
		{
			public ExtendedProjectDecompiler(DecompilerSettings settings, IAssemblyResolver assemblyResolver)
				: base(settings, assemblyResolver, null, null)
			{
			}

			public new bool IncludeTypeWhenDecompilingProject(PEFile module, TypeDefinitionHandle type)
				=> base.IncludeTypeWhenDecompilingProject(module, type);
		}

		private class IDGAFAttributeTypeProvider : ICustomAttributeTypeProvider<object?>
		{
			public object? GetPrimitiveType(PrimitiveTypeCode typeCode) => null;
			public object GetSystemType() => throw new NotImplementedException();
			public object GetSZArrayType(object? elementType) => throw new NotImplementedException();

			public object GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) =>
				throw new NotImplementedException();

			public object GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) =>
				throw new NotImplementedException();

			public object GetTypeFromSerializedName(string name) => throw new NotImplementedException();
			public PrimitiveTypeCode GetUnderlyingEnumType(object? type) => throw new NotImplementedException();
			public bool IsSystemType(object? type) => throw new NotImplementedException();
		}
	}
}
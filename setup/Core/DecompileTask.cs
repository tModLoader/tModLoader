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
		private sealed class TerrariaAssemblyResolver : IAssemblyResolver
		{

			private readonly PEFile baseModule;
			private readonly UniversalAssemblyResolver _resolver;
			private readonly Dictionary<string, PEFile> cache = new();

			public TerrariaAssemblyResolver(PEFile baseModule, string targetFramework, IEnumerable<string> extraSearchDirs)
			{
				this.baseModule = baseModule;
				// pass mainAssemblyFileName: null so we can control the order of the search paths. We need to search the framework directory before the Terraria.exe folder on Mono platforms 
				_resolver = new UniversalAssemblyResolver(mainAssemblyFileName: null, throwOnError: true, targetFramework, streamOptions: PEStreamOptions.PrefetchMetadata);

				foreach (var dir in extraSearchDirs)
					_resolver.AddSearchDirectory(dir);

				_resolver.AddSearchDirectory(Path.GetDirectoryName(baseModule.FileName)!);
			}

			public PEFile? Resolve(IAssemblyReference name)
			{
				lock (this)
				{
					if (cache.TryGetValue(name.FullName, out var module))
						return module;

					//look in the base module's embedded resources
					var resName = name.Name + ".dll";
					var res = baseModule.Resources.Where(r => r.ResourceType == ResourceType.Embedded).SingleOrDefault(r => r.Name.EndsWith(resName));

					if (res != null)
						module = new PEFile(res.Name, res.TryOpenStream()!);

					module ??= _resolver.Resolve(name)!;

					cache[name.FullName] = module;
					return module;
				}
			}

			public PEFile? ResolveModule(PEFile mainModule, string moduleName)
				=> _resolver.ResolveModule(mainModule, moduleName)!;

			public Task<PEFile?> ResolveAsync(IAssemblyReference reference)
				=> Task.Run(() => Resolve(reference));

			public Task<PEFile?> ResolveModuleAsync(PEFile mainModule, string moduleName)
				=> Task.Run(() => ResolveModule(mainModule, moduleName));
		}

		// This exists solely to expose the IncludeTypeWhenDecompilingProject method, which is protected in WholeProjectDecompiler
		private class ExtendedProjectDecompiler : WholeProjectDecompiler
		{
			public ExtendedProjectDecompiler(DecompilerSettings settings, IAssemblyResolver assemblyResolver)
				: base(settings, assemblyResolver, assemblyReferenceClassifier: null, debugInfoProvider: null) { }

			public new bool IncludeTypeWhenDecompilingProject(PEFile module, TypeDefinitionHandle type)
				=> base.IncludeTypeWhenDecompilingProject(module, type);
		}

		private readonly ProgramSettings programSettings;
		private readonly TerrariaExecutableSetter terrariaExecutableSetter;
		private readonly TerrariaDecompileExecutableProvider terrariaDecompileExecutableProvider;
		private readonly DecompileTaskParameters parameters;

		private ExtendedProjectDecompiler? projectDecompiler;
		private readonly DecompilerSettings decompilerSettings;

		public DecompileTask(DecompileTaskParameters parameters, IServiceProvider serviceProvider)
		{
			programSettings = serviceProvider.GetRequiredService<ProgramSettings>();
			terrariaExecutableSetter = serviceProvider.GetRequiredService<TerrariaExecutableSetter>();
			terrariaDecompileExecutableProvider = serviceProvider.GetRequiredService<TerrariaDecompileExecutableProvider>();
			this.parameters = parameters with { SrcDir = PathUtils.WithUnixSeparators(parameters.SrcDir) };

			var formatting = FormattingOptionsFactory.CreateKRStyle();

			// Arrays should have a new line for every entry, since it's easier to insert values in patches that way.
			formatting.ArrayInitializerWrapping = Wrapping.WrapAlways;
			formatting.ArrayInitializerBraceStyle = BraceStyle.EndOfLine;

			// Force wrapping for chained calls for the same reason.
			// Hm, doesn't work.
			//formatting.ChainedMethodCallWrapping = Wrapping.WrapAlways;

			decompilerSettings = new(LanguageVersion.Latest) {
				RemoveDeadCode = true,
				CSharpFormattingOptions = formatting,

				// Switch expressions are not patching-friendly,
				// and do not even support expression bodies at this time:
				// https://github.com/dotnet/csharplang/issues/3037
				SwitchExpressions = false,
			};
		}

		public override async ValueTask ConfigurationPrompt(CancellationToken cancellationToken = default)
		{
			await terrariaExecutableSetter.FindAndSetTerrariaDirectoryIfNecessary(
				parameters.TerrariaSteamDirectory,
				parameters.TmlDevSteamDirectory,
				parameters.ValidateTerrariaSteamDirectory,
				cancellationToken);
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = progress.StartTask("Decompiling Terraria...");
			taskProgress.ReportStatus("Deleting Old Src");
			if (Directory.Exists(parameters.SrcDir))
				Directory.Delete(parameters.SrcDir, true);

			var clientModulePath = await terrariaDecompileExecutableProvider.RetrieveClientExecutable(parameters.DecryptionKey, cancellationToken);
			var clientModule = parameters.ServerOnly ? null : ReadModule(clientModulePath, taskProgress);

			var serverModulePath = await terrariaDecompileExecutableProvider.RetrieveServerExecutable(taskProgress, cancellationToken);
			var serverModule = ReadModule(serverModulePath, taskProgress);

			var mainModule = (parameters.ServerOnly ? serverModule : clientModule)!;

			var extraRefDirs = await terrariaDecompileExecutableProvider.RetrieveExtraReferences(taskProgress, cancellationToken);
			var assemblyResolver = new TerrariaAssemblyResolver(mainModule, mainModule.DetectTargetFrameworkId(), extraRefDirs);

			projectDecompiler = new ExtendedProjectDecompiler(decompilerSettings, assemblyResolver);

			var items = new List<WorkItem>();
			var files = new HashSet<string>();
			var resources = new HashSet<string>();
			var exclude = new List<string>();

			// Decompile embedded library sources directly into Terraria project. Treated the same as Terraria source
			var decompiledLibraries = new [] { "ReLogic" };
			foreach (var lib in decompiledLibraries) {
				var libRes = mainModule.Resources.Single(r => r.Name.EndsWith(lib+".dll"));
				AddEmbeddedLibrary(libRes, projectDecompiler.AssemblyResolver, items);
				exclude.Add(GetOutputPath(libRes.Name, mainModule));
			}

			if (!parameters.ServerOnly)
				AddModule(clientModule!, projectDecompiler.AssemblyResolver, items, files, resources, exclude);

			AddModule(serverModule, projectDecompiler.AssemblyResolver, items, files, resources, exclude, parameters.ServerOnly ? null : "SERVER");

			items.Add(WriteTerrariaProjectFile(mainModule, files, resources, decompiledLibraries));
			items.Add(WriteCommonConfigurationFile());

			await ExecuteParallel(items, taskProgress, maxDegreeOfParallelism: parameters.MaxDegreeOfParallelism, cancellationToken: cancellationToken);
		}

		private void AddEmbeddedLibrary(Resource res, IAssemblyResolver resolver, List<WorkItem> items)
		{
			using var s = res.TryOpenStream()!;
			s.Position = 0;
			var module = new PEFile(res.Name, s, PEStreamOptions.PrefetchEntireImage);

			var files = new HashSet<string>();
			var resources = new HashSet<string>();
			AddModule(module, resolver, items, files, resources);
			items.Add(WriteProjectFile(module, "Library", files, resources, w => {
				// references
				w.WriteStartElement("ItemGroup");
				foreach (var r in module.AssemblyReferences.OrderBy(r => r.Name)) {
					if (r.Name == "mscorlib") continue;

					w.WriteStartElement("Reference");
					w.WriteAttributeString("Include", r.Name);
					w.WriteEndElement();
				}
				w.WriteEndElement(); // </ItemGroup>

				// TODO: resolve references to embedded terraria libraries with their HintPath
			}));
		}

		protected PEFile ReadModule(string path, ITaskProgress taskProgress)
		{
			taskProgress.ReportStatus("Loading " + Path.GetFileName(path));
			using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return new PEFile(path, fileStream, PEStreamOptions.PrefetchEntireImage);
		}

		// memoized
		private static ConditionalWeakTable<PEFile, string> assemblyTitleCache = new ConditionalWeakTable<PEFile, string>();
		private static string GetAssemblyTitle(PEFile module)
		{
			if (!assemblyTitleCache.TryGetValue(module, out var title))
				assemblyTitleCache.Add(module, title = GetCustomAttributes(module)[nameof(AssemblyTitleAttribute)]);

			return title;
		}

		private static bool IsCultureFile(string path)
		{
			if (!path.Contains('-'))
				return false;

			try {
				var ci = CultureInfo.GetCultureInfo(Path.GetFileNameWithoutExtension(path));
				if (ci.ThreeLetterWindowsLanguageName == "ZZZ")
					return false; // https://stackoverflow.com/questions/35074033/invalid-cultureinfo-no-longer-throws-culturenotfoundexception

				return true;
			}
			catch (CultureNotFoundException) { }
			return false;
		}


		private static string GetOutputPath(string path, PEFile module)
		{
			if (path.EndsWith(".dll")) {
				var asmRef = module.AssemblyReferences.SingleOrDefault(r => path.EndsWith(r.Name + ".dll"));
				if (asmRef != null)
					path = Path.Combine(path.Substring(0, path.Length - asmRef.Name.Length - 5), asmRef.Name + ".dll");
			}

			var rootNamespace = GetAssemblyTitle(module);
			if (path.StartsWith(rootNamespace))
				path = path.Substring(rootNamespace.Length + 1);

			path = path.Replace("Libraries.", "Libraries/"); // lets leave the folder structure in here alone
			path = path.Replace('\\', '/');

			// . to /
			int stopFolderzingAt = path.IndexOf('/');
			if (stopFolderzingAt < 0)
				stopFolderzingAt = path.LastIndexOf('.');
			path = new StringBuilder(path).Replace(".", "/", 0, stopFolderzingAt).ToString();

			// default lang files should be called Main
			if (IsCultureFile(path))
				path = path.Insert(path.LastIndexOf('.'), "/Main");

			return path;
		}

		private IEnumerable<IGrouping<string, TypeDefinitionHandle>> GetCodeFiles(PEFile module)
		{
			var metadata = module.Metadata;
			return module.Metadata.GetTopLevelTypeDefinitions().Where(td => projectDecompiler!.IncludeTypeWhenDecompilingProject(module, td))
				.GroupBy(h =>
				{
					var type = metadata.GetTypeDefinition(h);
					var path = WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Name)) + ".cs";
					if (!string.IsNullOrEmpty(metadata.GetString(type.Namespace)))
						path = Path.Combine(WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Namespace)), path);
					return GetOutputPath(path, module);
				}, StringComparer.OrdinalIgnoreCase);
		}

		private static IEnumerable<(string path, Resource r)> GetResourceFiles(PEFile module)
		{
			return module.Resources.Where(r => r.ResourceType == ResourceType.Embedded).Select(res => (GetOutputPath(res.Name, module), res));
		}

		private DecompilerTypeSystem AddModule(PEFile module, IAssemblyResolver resolver, List<WorkItem> items, ISet<string> sourceSet, ISet<string> resourceSet, ICollection<string>? exclude = null, string? conditional = null)
		{
			var projectDir = GetAssemblyTitle(module);
			var sources = GetCodeFiles(module).ToList();
			var resources = GetResourceFiles(module).ToList();
			if (exclude != null) {
				sources.RemoveAll(src => exclude.Contains(src.Key));
				resources.RemoveAll(res => exclude.Contains(res.path));
			}

			var ts = new DecompilerTypeSystem(module, resolver, decompilerSettings);
			items.AddRange(sources
				.Where(src => sourceSet.Add(src.Key))
				.Select(src => DecompileSourceFile(ts, src, projectDir, conditional)));

			if (conditional != null && resources.Any(res => !resourceSet.Contains(res.path)))
				throw new Exception($"Conditional ({conditional}) resources not supported");

			items.AddRange(resources
				.Where(res => resourceSet.Add(res.path))
				.Select(res => ExtractResource(res.path, res.r, projectDir)));

			return ts;
		}

		private WorkItem ExtractResource(string name, Resource res, string projectDir)
		{
			return new WorkItem("Extracting: " + name, async ct =>
			{
				var path = Path.Combine(parameters.SrcDir, projectDir, name);
				CreateParentDirectory(path);

				var s = res.TryOpenStream()!;
				s.Position = 0;
				using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					await s.CopyToAsync(fs, ct);
			});
		}

		private CSharpDecompiler CreateDecompiler(DecompilerTypeSystem ts, CancellationToken cancellationToken = default)
		{
			var decompiler = new CSharpDecompiler(ts, projectDecompiler!.Settings)
			{
				CancellationToken = cancellationToken
			};
			decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers());
			decompiler.AstTransforms.Add(new RemoveCLSCompliantAttribute());
			return decompiler;
		}

		private WorkItem DecompileSourceFile(DecompilerTypeSystem ts, IGrouping<string, TypeDefinitionHandle> src, string projectName, string? conditional = null)
		{
			return new WorkItem("Decompiling: " + src.Key, async (updateStatus, cancellationToken) =>
			{
				var path = Path.Combine(parameters.SrcDir, projectName, src.Key);
				CreateParentDirectory(path);

				using (var w = new StringWriter())
				{
					if (conditional != null)
						w.WriteLine("#if "+conditional);

					CreateDecompiler(ts, cancellationToken)
						.DecompileTypes(src.ToArray())
						.AcceptVisitor(new CSharpOutputVisitor(w, projectDecompiler!.Settings.CSharpFormattingOptions));

					if (conditional != null)
						w.WriteLine("#endif");

					string source = w.ToString();
					if (programSettings.FormatAfterDecompiling) {
						updateStatus("Formatting: " + src.Key);
						source = FormatTask.Format(source, true, cancellationToken);
					}

					await File.WriteAllTextAsync(path, source, cancellationToken);
				}
			});
		}

		private WorkItem WriteTerrariaProjectFile(PEFile module, IEnumerable<string> sources, IEnumerable<string> resources, ICollection<string> decompiledLibraries)
		{
			return WriteProjectFile(module, "WinExe", sources, resources, w => {
				//configurations
				w.WriteStartElement("PropertyGroup");
				w.WriteAttributeString("Condition", "$(Configuration.Contains('Server'))");
				w.WriteElementString("OutputType", "Exe");
				w.WriteElementString("OutputName", "$(OutputName)Server");
				w.WriteEndElement(); // </PropertyGroup>

				// references
				w.WriteStartElement("ItemGroup");

				var references = module.AssemblyReferences.Where(r => r.Name != "mscorlib").OrderBy(r => r.Name).ToArray();
				var projectReferences = decompiledLibraries != null
					? references.Where(r => decompiledLibraries.Contains(r.Name)).ToArray()
					: Array.Empty<AssemblyReference>();
				var normalReferences = references.Except(projectReferences).ToArray();

				foreach (var r in projectReferences) {
					w.WriteStartElement("ProjectReference");
					w.WriteAttributeString("Include", $"../{r.Name}/{r.Name}.csproj");
					w.WriteEndElement();
				}

				foreach (var r in projectReferences) {
					w.WriteStartElement("EmbeddedResource");
					w.WriteAttributeString("Include", $"../{r.Name}/bin/$(Configuration)/$(TargetFramework)/{r.Name}.dll");
					w.WriteElementString("LogicalName", $"Terraria.Libraries.{r.Name}.{r.Name}.dll");
					w.WriteEndElement();
				}

				foreach (var r in normalReferences) {
					w.WriteStartElement("Reference");
					w.WriteAttributeString("Include", r.Name);
					w.WriteEndElement();
				}

				w.WriteEndElement(); // </ItemGroup>
			});
		}

		private WorkItem WriteProjectFile(PEFile module, string outputType, IEnumerable<string> sources, IEnumerable<string> resources, Action<XmlTextWriter> writeSpecificConfig)
		{
			var name = GetAssemblyTitle(module);
			var filename = name + ".csproj";
			return new WorkItem("Writing: " + filename, () =>
			{
				var path = Path.Combine(parameters.SrcDir, name, filename);
				CreateParentDirectory(path);

				using (var sw = new StreamWriter(path))
				using (var w = CreateXmlWriter(sw)) {
					w.Formatting = System.Xml.Formatting.Indented;
					w.WriteStartElement("Project");
					w.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

					w.WriteStartElement("Import");
					w.WriteAttributeString("Project", "../Configuration.targets");
					w.WriteEndElement(); // </Import>

					w.WriteStartElement("PropertyGroup");
					w.WriteElementString("OutputType", outputType);
					w.WriteElementString("Version", new AssemblyName(module.FullName).Version!.ToString());

					var attribs = GetCustomAttributes(module);
					w.WriteElementString("Company", attribs[nameof(AssemblyCompanyAttribute)]);
					w.WriteElementString("Copyright", attribs[nameof(AssemblyCopyrightAttribute)]);

					w.WriteElementString("RootNamespace", module.Name);
					w.WriteEndElement(); // </PropertyGroup>

					writeSpecificConfig(w);

					// resources
					w.WriteStartElement("ItemGroup");
					foreach (var r in ApplyWildcards(resources, sources.ToArray()).OrderBy(r => r)) {
						w.WriteStartElement("EmbeddedResource");
						w.WriteAttributeString("Include", r);
						w.WriteEndElement();
					}
					w.WriteEndElement(); // </ItemGroup>
					w.WriteEndElement(); // </Project>

					sw.Write(Environment.NewLine);
				}
			});
		}

		private WorkItem WriteCommonConfigurationFile()
		{
			var filename = "Configuration.targets";
			return new WorkItem("Writing: " + filename, () =>
			{
				var path = Path.Combine(parameters.SrcDir, filename);
				CreateParentDirectory(path);

				using (var sw = new StreamWriter(path))
				using (var w = CreateXmlWriter(sw)) {
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

					sw.Write(Environment.NewLine);
				}
			});
		}

		private static XmlTextWriter CreateXmlWriter(StreamWriter streamWriter)
		{
			return new XmlTextWriter(streamWriter) {
				Formatting = System.Xml.Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1,
			};
		}

		private IEnumerable<string> ApplyWildcards(IEnumerable<string> include, IReadOnlyList<string> exclude) {
			var wildpaths = new HashSet<string>();
			foreach (var path in include) {
				if (wildpaths.Any(path.StartsWith))
					continue;

				string wpath = path;
				string cards = "";
				while (wpath.Contains('/')) {
					var parent = wpath.Substring(0, wpath.LastIndexOf('/'));
					if (exclude.Any(e => e.StartsWith(parent)))
						break; //can't use parent as a wildcard

					wpath = parent;
					if (cards.Length < 2)
						cards += "*";
				}

				if (wpath != path) {
					wildpaths.Add(wpath);
					yield return $"{wpath}/{cards}";
				} else {
					yield return path;
				}
			}
		}

		private static string[] knownAttributes = {nameof(AssemblyCompanyAttribute), nameof(AssemblyCopyrightAttribute), nameof(AssemblyTitleAttribute) };
		private static IDictionary<string, string> GetCustomAttributes(PEFile module) {
			var dict = new Dictionary<string, string>();

			var reader = module.Reader.GetMetadataReader();
			var attribs = reader.GetAssemblyDefinition().GetCustomAttributes().Select(reader.GetCustomAttribute);
			foreach (var attrib in attribs) {
				var ctor = reader.GetMemberReference((MemberReferenceHandle)attrib.Constructor);
				var attrTypeName = reader.GetString(reader.GetTypeReference((TypeReferenceHandle)ctor.Parent).Name);
				if (!knownAttributes.Contains(attrTypeName))
					continue;

				var value = attrib.DecodeValue(new IDGAFAttributeTypeProvider());
				dict[attrTypeName] = (value.FixedArguments.Single().Value as string)!;
			}

			return dict;
		}

		private class IDGAFAttributeTypeProvider : ICustomAttributeTypeProvider<object>
		{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
			public object? GetPrimitiveType(PrimitiveTypeCode typeCode) => null;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
			public object GetSystemType() => throw new NotImplementedException();
			public object GetSZArrayType(object elementType) => throw new NotImplementedException();
			public object GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => throw new NotImplementedException();
			public object GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => throw new NotImplementedException();
			public object GetTypeFromSerializedName(string name) => throw new NotImplementedException();
			public PrimitiveTypeCode GetUnderlyingEnumType(object type) => throw new NotImplementedException();
			public bool IsSystemType(object type) => throw new NotImplementedException();
		}
	}
}
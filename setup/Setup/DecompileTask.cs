using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Terraria.ModLoader.Properties;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	public class DecompileTask : Task
	{
		private class EmbeddedAssemblyResolver : IAssemblyResolver
		{
			private readonly PEFile baseModule;
			private readonly UniversalAssemblyResolver _resolver;
			private readonly Dictionary<string, PEFile> cache = new Dictionary<string, PEFile>();

			public EmbeddedAssemblyResolver(PEFile baseModule, string targetFramework)
			{
				this.baseModule = baseModule;
				_resolver = new UniversalAssemblyResolver(baseModule.FileName, true, targetFramework, PEStreamOptions.PrefetchMetadata);
				_resolver.AddSearchDirectory(Path.GetDirectoryName(baseModule.FileName));
			}

			public PEFile Resolve(IAssemblyReference name)
			{
				lock (this)
				{
					if (cache.TryGetValue(name.FullName, out var module))
						return module;
					
					//look in the base module's embedded resources
					var resName = name.Name + ".dll";
					var res = baseModule.Resources.Where(r => r.ResourceType == ResourceType.Embedded).SingleOrDefault(r => r.Name.EndsWith(resName));
					if (!res.IsNil)
						module = new PEFile(res.Name, res.TryOpenStream());

					if (module == null)
						module = _resolver.Resolve(name);
					
					cache[name.FullName] = module;
					return module;
				}
			}

			public PEFile ResolveModule(PEFile mainModule, string moduleName) => _resolver.ResolveModule(mainModule, moduleName);
		}

		private class ExtendedProjectDecompiler : WholeProjectDecompiler
		{
			public new bool IncludeTypeWhenDecompilingProject(PEFile module, TypeDefinitionHandle type) => base.IncludeTypeWhenDecompilingProject(module, type);

			protected override bool IsGacAssembly(IAssemblyReference r, PEFile asm) => 
				asm.FileName.Contains("\\Microsoft.NET\\assembly\\");

			private static readonly MethodInfo _WriteProjectFile = typeof(WholeProjectDecompiler)
				.GetMethod("WriteProjectFile", BindingFlags.NonPublic | BindingFlags.Instance);

			public void WriteProjectFile(TextWriter writer, IEnumerable<Tuple<string, string>> files, PEFile module) =>
				_WriteProjectFile.Invoke(this, new object[] {writer, files, module});

		}

		private static readonly Guid clientGuid = new Guid("3996D5FA-6E59-4FE4-9F2B-40EEEF9645D5");
		private static readonly Guid serverGuid = new Guid("85BF1171-A0DC-4696-BFA4-D6E9DC4E0830");
		public static readonly Version clientVersion = new Version(Settings.Default.ClientVersion);
		public static readonly Version serverVersion = new Version(Settings.Default.ServerVersion);

		private readonly string srcDir;
		private readonly bool serverOnly;

		private ExtendedProjectDecompiler projectDecompiler;

		private readonly DecompilerSettings decompilerSettings = new DecompilerSettings(LanguageVersion.Latest)
		{
			//AlwaysUseBraces = false,
			RemoveDeadCode = true,
			CSharpFormattingOptions = FormattingOptionsFactory.CreateAllman()
		};

		public DecompileTask(ITaskInterface taskInterface, string srcDir, bool serverOnly = false) : base(taskInterface)
		{
			this.srcDir = srcDir;
			this.serverOnly = serverOnly;
		}

		public override bool ConfigurationDialog()
		{
			if (File.Exists(TerrariaPath) && File.Exists(TerrariaServerPath))
				return true;

			return (bool) taskInterface.Invoke(new Func<bool>(SelectTerrariaDialog));
		}

		public override void Run()
		{
			taskInterface.SetStatus("Deleting Old Src");
			if (Directory.Exists(srcDir))
				Directory.Delete(srcDir, true);
			
			var items = new List<WorkItem>();
			var files = new HashSet<string>();

			projectDecompiler = new ExtendedProjectDecompiler { Settings = decompilerSettings };
			IAssemblyResolver resolver = null;

			DecompilerTypeSystem cts = null;
			if (!serverOnly)
				cts = AddModule(items, files, ref resolver, TerrariaPath, clientVersion, clientGuid);

			var sts = AddModule(items, files, ref resolver, TerrariaServerPath, serverVersion, serverGuid);
			projectDecompiler.AssemblyResolver = resolver;

			items.Add(WriteAssemblyInfo(serverOnly ? sts : cts));

			ExecuteParallel(items, maxDegree: Settings.Default.SingleDecompileThread ? 1 : 0);
		}

		protected PEFile ReadModule(string path, Version version)
		{
			taskInterface.SetStatus("Loading " + Path.GetFileName(path));
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				var module = new PEFile(path, fileStream, PEStreamOptions.PrefetchEntireImage);
				var assemblyName = new AssemblyName(module.FullName);
				if (assemblyName.Version != version)
					throw new Exception($"{assemblyName.Name} version {assemblyName.Version}. Expected {version}");

				return module;
			}
		}

		private IEnumerable<IGrouping<string, TypeDefinitionHandle>> GetCodeFiles(PEFile module)
		{
			var metadata = module.Metadata;
			return module.Metadata.GetTopLevelTypeDefinitions().Where(td => projectDecompiler.IncludeTypeWhenDecompilingProject(module, td))
				.GroupBy(h =>
				{
					var type = metadata.GetTypeDefinition(h);
					var file = WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Name)) + ".cs";
					if (!string.IsNullOrEmpty(metadata.GetString(type.Namespace)))
						file = Path.Combine(WholeProjectDecompiler.CleanUpFileName(metadata.GetString(type.Namespace)), file);
					return file;
				}, StringComparer.OrdinalIgnoreCase);
		}

		private static IEnumerable<(string, Resource)> GetResourceFiles(PEFile module)
		{
			return module.Resources.Where(r => r.ResourceType == ResourceType.Embedded).Select(res =>
			{
				var path = res.Name;
				path = path.Replace("Terraria.Libraries.", "Terraria.Libraries\\");
				if (path.EndsWith(".dll"))
				{
					var asmRef = module.AssemblyReferences.SingleOrDefault(r => path.EndsWith(r.Name + ".dll"));
					if (asmRef != null)
						path = Path.Combine(path.Substring(0, path.Length - asmRef.Name.Length - 5), asmRef.Name + ".dll");
				}
				
				if (IsCultureFile(path))
					path = path.Insert(path.LastIndexOf('.'), ".Main");

				return (path, res);
			});
		}

		private static bool IsCultureFile(string path)
		{
			var fname = Path.GetFileNameWithoutExtension(path);
			var subext = Path.GetExtension(fname);
			if (!string.IsNullOrEmpty(subext))
			{
				try
				{
					CultureInfo.GetCultureInfo(subext.Substring(1));
					return true;
				}
				catch (CultureNotFoundException)
				{ }
			}

			return false;
		}

		private DecompilerTypeSystem AddModule(List<WorkItem> items, ISet<string> fileList, ref IAssemblyResolver resolver, 
			string path, Version version, Guid guid)
		{
			var module = ReadModule(path, version);
			var sources = GetCodeFiles(module).ToList();
			var resources = GetResourceFiles(module).ToList();

			items.Add(WriteProjectFile(module, guid, sources, resources));
			items.Add(WriteProjectUserFile(module, SteamDir));

			if (resolver == null)
				resolver = new EmbeddedAssemblyResolver(module, module.Reader.DetectTargetFrameworkId());

			var ts = new DecompilerTypeSystem(module, resolver, decompilerSettings);
			items.AddRange(sources
				.Where(src => fileList.Add(src.Key))
				.Select(src => DecompileSourceFile(ts, src)));

			items.AddRange(resources
				.Where(res => fileList.Add(res.Item1))
				.Select(res => ExtractResource(res.Item1, res.Item2)));

			return ts;
		}

		private WorkItem ExtractResource(string name, Resource res)
		{
			return new WorkItem("Extracting: " + name, () =>
			{
				var path = Path.Combine(srcDir, name);
				CreateParentDirectory(path);

				var s = res.TryOpenStream();
				s.Position = 0;
				using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					s.CopyTo(fs);
			});
		}

		private CSharpDecompiler CreateDecompiler(DecompilerTypeSystem ts)
		{
			var decompiler = new CSharpDecompiler(ts, projectDecompiler.Settings)
			{
				CancellationToken = taskInterface.CancellationToken
			};
			decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers());
			decompiler.AstTransforms.Add(new RemoveCLSCompliantAttribute());
			return decompiler;
		}

		private WorkItem DecompileSourceFile(DecompilerTypeSystem ts, IGrouping<string, TypeDefinitionHandle> src)
		{
			return new WorkItem("Decompiling: " + src.Key, () =>
			{
				var path = Path.Combine(srcDir, src.Key);
				CreateParentDirectory(path);

				using (var w = new StreamWriter(path))
				{
					CreateDecompiler(ts)
						.DecompileTypes(src.ToArray())
						.AcceptVisitor(new CSharpOutputVisitor(w, projectDecompiler.Settings.CSharpFormattingOptions));
				}
			});
		}

		private WorkItem WriteAssemblyInfo(DecompilerTypeSystem ts)
		{
			return new WorkItem("Decompiling: AssemblyInfo.cs", () =>
			{
				var path = Path.Combine(srcDir, "Properties/AssemblyInfo.cs");
				CreateParentDirectory(path);

				using (var w = new StreamWriter(path))
				{
					var decompiler = CreateDecompiler(ts);
					decompiler.AstTransforms.Add(new RemoveCompilerGeneratedAssemblyAttributes());
					decompiler.DecompileModuleAndAssemblyAttributes()
						.AcceptVisitor(new CSharpOutputVisitor(w, projectDecompiler.Settings.CSharpFormattingOptions));
				}
			});
		}

		private WorkItem WriteProjectFile(PEFile module, Guid guid,
			IEnumerable<IGrouping<string, TypeDefinitionHandle>> sources,
			IEnumerable<(string, Resource)> resources)
		{
			var name = Path.GetFileNameWithoutExtension(module.Name) + ".csproj";
			return new WorkItem("Writing: " + name, () =>
			{
				//flatten the file list
				var files = sources.Select(src => Tuple.Create("Compile", src.Key))
					.Concat(resources.Select(res => Tuple.Create("EmbeddedResource", res.Item1)))
					.Concat(new[] {Tuple.Create("Compile", "Properties/AssemblyInfo.cs")});

				//sort the assembly references
				var refs = module.AssemblyReferences.OrderBy(r => r.Name).ToArray();
				module.AssemblyReferences.Clear();
				foreach (var r in refs)
					module.AssemblyReferences.Add(r);

				projectDecompiler.ProjectGuid = guid;

				var path = Path.Combine(srcDir, name);
				CreateParentDirectory(path);

				using (var w = new StreamWriter(path))
					projectDecompiler.WriteProjectFile(w, files, module);

				//trailing newline
				using (var w = new StreamWriter(path, true))
					w.Write(Environment.NewLine);
			});
		}

		private WorkItem WriteProjectUserFile(PEFile module, string debugWorkingDir)
		{
			var name = Path.GetFileNameWithoutExtension(module.Name) + ".csproj.user";
			return new WorkItem("Writing: " + name, () =>
			{
				var path = Path.Combine(srcDir, name);
				CreateParentDirectory(path);

				using (var w = new StreamWriter(path))
				using (var xml = new XmlTextWriter(w))
				{
					xml.Formatting = Formatting.Indented;
					xml.WriteStartDocument();
					xml.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
					xml.WriteAttributeString("ToolsVersion", "4.0");
					xml.WriteStartElement("PropertyGroup");
					xml.WriteAttributeString("Condition", "'$(Configuration)' == 'Debug'");
					xml.WriteStartElement("StartWorkingDirectory");
					xml.WriteValue(debugWorkingDir);
					xml.WriteEndElement();
					xml.WriteEndElement();
					xml.WriteEndDocument();
				}
			});
		}
	}
}
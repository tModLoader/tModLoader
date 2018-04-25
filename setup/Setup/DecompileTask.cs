using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using Terraria.ModLoader.Properties;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	internal static class DecompileReflections
	{
		private static readonly MethodInfo _IncludeTypeWhenDecompilingProject = typeof(WholeProjectDecompiler)
			.GetMethod("IncludeTypeWhenDecompilingProject", BindingFlags.NonPublic | BindingFlags.Instance);

		public static bool IncludeTypeWhenDecompilingProject(this WholeProjectDecompiler decompiler, TypeDefinition type) =>
			(bool) _IncludeTypeWhenDecompilingProject.Invoke(decompiler, new object[] {type});

		private static readonly MethodInfo _WriteProjectFile = typeof(WholeProjectDecompiler)
			.GetMethod("WriteProjectFile", BindingFlags.NonPublic | BindingFlags.Instance);

		public static void WriteProjectFile(this WholeProjectDecompiler decompiler, TextWriter writer,
			IEnumerable<Tuple<string, string>> files, ModuleDefinition module) =>
			_WriteProjectFile.Invoke(decompiler, new object[] {writer, files, module});
	}

	public class DecompileTask : Task
	{
		private class EmbeddedAssemblyResolver : BaseAssemblyResolver
		{
			private readonly Dictionary<string, AssemblyDefinition> cache = new Dictionary<string, AssemblyDefinition>();
			public ModuleDefinition baseModule;

			public EmbeddedAssemblyResolver()
			{
				AddSearchDirectory(SteamDir);
			}

			public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
			{
				lock (this)
				{
					if (cache.TryGetValue(name.FullName, out var assemblyDefinition))
						return assemblyDefinition;

					//ignore references to other mscorlib versions, they are unneeded and produce namespace conflicts
					if (name.Name == "mscorlib" && name.Version.Major != 4)
						goto skip;

					//look in the base module's embedded resources
					if (baseModule != null)
					{
						var resName = name.Name + ".dll";
						var res =
							baseModule.Resources.OfType<EmbeddedResource>()
								.SingleOrDefault(r => r.Name.EndsWith(resName));
						if (res != null)
							assemblyDefinition = AssemblyDefinition.ReadAssembly(res.GetResourceStream(), parameters);
					}

					if (assemblyDefinition == null)
						assemblyDefinition = base.Resolve(name, parameters);

					skip:
					cache[name.FullName] = assemblyDefinition;
					return assemblyDefinition;
				}
			}
		}

		private static readonly Guid clientGuid = new Guid("3996D5FA-6E59-4FE4-9F2B-40EEEF9645D5");
		private static readonly Guid serverGuid = new Guid("85BF1171-A0DC-4696-BFA4-D6E9DC4E0830");
		public static readonly Version clientVersion = new Version(Settings.Default.ClientVersion);
		public static readonly Version serverVersion = new Version(Settings.Default.ServerVersion);

		private readonly string srcDir;
		private readonly bool serverOnly;

		private WholeProjectDecompiler projectDecompiler;

		public string FullSrcDir => Path.Combine(baseDir, srcDir);

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

			if (Directory.Exists(FullSrcDir))
				Directory.Delete(FullSrcDir, true);

			//var format = FormattingOptionsFactory.CreateKRStyle();
			var format = FormattingOptionsFactory.CreateAllman();

			projectDecompiler = new WholeProjectDecompiler
			{
				Settings = new DecompilerSettings(LanguageVersion.Latest)
				{
					//AlwaysUseBraces = false,
					CSharpFormattingOptions = format
				}
			};

			var items = new List<WorkItem>();
			var files = new HashSet<string>();
			DecompilerTypeSystem cts = null;
			if (!serverOnly)
				cts = AddModule(items, files, clientVersion, clientGuid);

			var sts = AddModule(items, files, serverVersion, serverGuid);

			items.Add(WriteAssemblyInfo(serverOnly ? sts : cts));

			ExecuteParallel(items, maxDegree: Settings.Default.SingleDecompileThread ? 1 : 0);
		}

		protected ModuleDefinition ReadModule(string modulePath, Version version)
		{
			taskInterface.SetStatus("Loading " + Path.GetFileName(modulePath));
			var resolver = new EmbeddedAssemblyResolver();
			var module = ModuleDefinition.ReadModule(modulePath,
				new ReaderParameters {AssemblyResolver = resolver});
			resolver.baseModule = module;

			if (module.Assembly.Name.Version != version)
				throw new Exception($"{module.Assembly.Name.Name} version {module.Assembly.Name.Version}. Expected {version}");

			return module;
		}

		private IEnumerable<IGrouping<string, TypeDefinition>> GetCodeFiles(ModuleDefinition module)
		{
			return module.Types.Where(projectDecompiler.IncludeTypeWhenDecompilingProject)
				.GroupBy(type =>
				{
					var file = WholeProjectDecompiler.CleanUpFileName(type.Name) + ".cs";
					if (!string.IsNullOrEmpty(type.Namespace))
						file = Path.Combine(WholeProjectDecompiler.CleanUpFileName(type.Namespace), file);
					return file;
				}, StringComparer.OrdinalIgnoreCase);
		}

		private static IEnumerable<(string, EmbeddedResource)> GetResourceFiles(ModuleDefinition module)
		{
			return module.Resources.OfType<EmbeddedResource>().Select(res =>
			{
				var path = res.Name;
				path = path.Replace("Terraria.Libraries.", "Terraria.Libraries\\");
				if (path.EndsWith(".dll"))
				{
					var asmRef = module.AssemblyReferences.SingleOrDefault(r => path.EndsWith(r.Name + ".dll"));
					if (asmRef != null)
						path = path.Substring(0, path.Length - asmRef.Name.Length - 5) +
						       Path.DirectorySeparatorChar + asmRef.Name + ".dll";
				}

				return (path, res);
			});
		}

		private DecompilerTypeSystem AddModule(List<WorkItem> items, ISet<string> fileList, Version version, Guid guid)
		{
			var module = ReadModule(TerrariaPath, version);
			var sources = GetCodeFiles(module).ToList();
			var resources = GetResourceFiles(module).ToList();

			items.Add(WriteProjectFile(module, guid, sources, resources));
			items.Add(WriteProjectUserFile(module, SteamDir));

			var ts = new DecompilerTypeSystem(module);
			items.AddRange(sources
				.Where(src => fileList.Add(src.Key))
				.Select(src => DecompileSourceFile(ts, src)));

			items.AddRange(resources
				.Where(res => fileList.Add(res.Item1))
				.Select(res => ExtractResource(res.Item1, res.Item2)));

			return ts;
		}

		private WorkItem ExtractResource(string name, EmbeddedResource res)
		{
			return new WorkItem("Extracting: " + name, () =>
			{
				var path = Path.Combine(FullSrcDir, name);
				CreateParentDirectory(path);

				var s = res.GetResourceStream();
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

		private WorkItem DecompileSourceFile(DecompilerTypeSystem ts, IGrouping<string, TypeDefinition> src)
		{
			return new WorkItem("Decompiling: " + src.Key, () =>
			{
				var path = Path.Combine(FullSrcDir, src.Key);
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
				var path = Path.Combine(FullSrcDir, Path.Combine("Properties", "AssemblyInfo.cs"));
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

		private WorkItem WriteProjectFile(ModuleDefinition module, Guid guid,
			IEnumerable<IGrouping<string, TypeDefinition>> sources,
			IEnumerable<(string, EmbeddedResource)> resources)
		{
			var name = Path.GetFileNameWithoutExtension(module.Name) + ".csproj";
			return new WorkItem("Writing: " + name, () =>
			{
				//flatten the file list
				var files = sources.Select(src => Tuple.Create("Compile", src.Key))
					.Concat(resources.Select(res => Tuple.Create("EmbeddedResource", res.Item1)))
					.Concat(new[] {Tuple.Create("Compile", Path.Combine("Properties", "AssemblyInfo.cs"))});

				//sort the assembly references
				var refs = module.AssemblyReferences.OrderBy(r => r.Name).ToArray();
				module.AssemblyReferences.Clear();
				foreach (var r in refs)
					module.AssemblyReferences.Add(r);

				projectDecompiler.ProjectGuid = guid;

				var path = Path.Combine(FullSrcDir, name);
				CreateParentDirectory(path);

				using (var w = new StreamWriter(path))
					projectDecompiler.WriteProjectFile(w, files, module);

				//trailing newline
				using (var w = new StreamWriter(path, true))
					w.Write(Environment.NewLine);
			});
		}

		private WorkItem WriteProjectUserFile(ModuleDefinition module, string debugWorkingDir)
		{
			var name = Path.GetFileNameWithoutExtension(module.Name) + ".csproj.user";
			return new WorkItem("Writing: " + name, () =>
			{
				var path = Path.Combine(FullSrcDir, name);
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
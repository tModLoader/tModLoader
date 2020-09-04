using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Terraria.ModLoader.Setup.Formatting;

namespace Terraria.ModLoader.Setup
{
	class IDFixerTask : RoslynTask
	{
		protected override string Status => "Fixing IDs";
		protected override int MaxDegreeOfParallelism => 2;

		public readonly string baseDir;
		public readonly string fixedDir;

		private Compilation compilation;
		internal static int totalFilesChanged;
		internal static int totalChanges;
		internal static Dictionary<string, IdDictionary> idDictionaries;

		public IDFixerTask(ITaskInterface taskInterface, string baseDir, string fixedDir) : base(taskInterface) {
			this.baseDir = PreparePath(baseDir);
			this.fixedDir = PreparePath(fixedDir);
		}

		public override bool ConfigurationDialog() {
			// This task is designed to occur after Terraria and before tModLoader patch stages
#if DEBUG
			projectPath = @".\setup\SyntaxRewriterTestProgram\SyntaxRewriterTestProgram.csproj";
#else
			projectPath = @".\src\Terraria\Terraria\Terraria.csproj";
#endif
			return true;
		}

		protected override void StartUp(MSBuildWorkspace workspace, Project project) {
			//taskInterface.SetStatus("Opening Terraria.sln");
			//_ = workspace.OpenSolutionAsync(@"D:\Documents\My Games\Terraria\Modding\tModLoader14\solutions\Terraria.sln").Result;

			//taskInterface.SetStatus("Opening Terraria.csproj");
			//Project project = workspace.CurrentSolution.Projects.Where(x => x.Name == "Terraria").First();
			//projectPath = project.FilePath;

			// Generate Compilation to later reflect ID fields and retrieve semantic model
			taskInterface.SetStatus("Generating Compilation");
			compilation = project.GetCompilationAsync().Result;

			taskInterface.SetStatus("Emit Assembly");
			var assemblyStream = new MemoryStream();
			var buildResult = compilation.Emit(assemblyStream);

			if (buildResult.Success == false)
				throw new Exception("Build failed. " + buildResult.Diagnostics.Count() + " errors: " + string.Join("", buildResult.Diagnostics.Select(o => "\n  " + o.ToString())));

			var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
			if (errors.Any()) {
				Console.WriteLine($"COMPILATION ERROR: {compilation.AssemblyName}: {errors.Count()} compilation errors: \n\t{string.Join("\n\t", errors.Where(e => false).Select(e => e.ToString()))}");
				taskInterface.SetStatus("Emit Fail");
			}
			else {
				Console.WriteLine($"Project {project.Name} compiled with no errors");
				taskInterface.SetStatus("Emit Success");
			}

			taskInterface.SetStatus("Loading IDs from Assembly");
			assemblyStream.Position = 0;
			using var peReader = new PEReader(assemblyStream);
			MetadataReader mr = peReader.GetMetadataReader();

			idDictionaries = new Dictionary<string, IdDictionary>();

			foreach (TypeDefinitionHandle typeHandle in mr.TypeDefinitions) {
				var typeDef = mr.GetTypeDefinition(typeHandle);

				string typeNamespace = mr.GetString(typeDef.Namespace);
				string typeName = mr.GetString(typeDef.Name);

				if (typeNamespace == "Terraria.ID") {
					Console.WriteLine($"{typeNamespace}.{typeName}");

					idDictionaries[typeName] = new IdDictionary(int.MaxValue);

					foreach (var fieldHandle in typeDef.GetFields()) {
						var fieldDef = mr.GetFieldDefinition(fieldHandle);

						string fieldName = mr.GetString(fieldDef.Name);

						if (fieldName == null) {
							continue;
						}

						var fieldDefaultValueHandle = fieldDef.GetDefaultValue();

						if (fieldDefaultValueHandle.IsNil) {
							continue;
						}

						var fieldDefaultValueConstant = mr.GetConstant(fieldDefaultValueHandle);

						//if (fieldDefaultValueConstant.TypeCode != ConstantTypeCode.Int16) {
						//	continue;
						//}

						var fieldDefaultValueReader = mr.GetBlobReader(fieldDefaultValueConstant.Value);
						int fieldDefaultValue = fieldDefaultValueConstant.TypeCode switch
						{
							ConstantTypeCode.Int16 => fieldDefaultValueReader.ReadInt16(),
							ConstantTypeCode.Int32 => fieldDefaultValueReader.ReadInt32(),
							ConstantTypeCode.SByte => fieldDefaultValueReader.ReadSByte(),
							ConstantTypeCode.Byte => fieldDefaultValueReader.ReadByte(),
							ConstantTypeCode.UInt16 => fieldDefaultValueReader.ReadUInt16(),
							ConstantTypeCode.UInt32 => (int)fieldDefaultValueReader.ReadUInt32(),
							_ => -1,
						};

						//Console.WriteLine($"{typeNamespace}.{typeName}.{fieldName} - {fieldDefaultValue}");

						// Should only happen in DustID
						if (fieldDefaultValue == -1 || idDictionaries[typeName].ContainsId(fieldDefaultValue)) {
							//Console.WriteLine($"Duplicate entry.");
							continue;
						}

						idDictionaries[typeName].Add(fieldName, fieldDefaultValue);
					}
				}
			}
			idDictionaries["NetmodeID"] = new IdDictionary(int.MaxValue);
			idDictionaries["NetmodeID"].Add("SinglePlayer", 0);
			idDictionaries["NetmodeID"].Add("MultiplayerClient", 1);
			idDictionaries["NetmodeID"].Add("Server", 2);

			totalChanges = 0;
			totalFilesChanged = 0;
		}

		protected override void FinalSteps() {
			taskInterface.SetStatus($"Finished ID fixes: {totalChanges} changes in {totalFilesChanged} files");
		}

		protected override async Task<Microsoft.CodeAnalysis.Document> Process(Microsoft.CodeAnalysis.Document doc) {
			if (!(await doc.GetSyntaxRootAsync() is SyntaxNode root))
				return doc;

			var tree = await doc.GetSyntaxTreeAsync();

			var model = compilation.GetSemanticModel(tree);

			IDFixerRewriter rewriter = new IDFixerRewriter(model);
			SyntaxNode newSource = rewriter.Visit(tree.GetRoot());

			if (newSource != root) {
				totalFilesChanged++;
				newSource = new AddIDUsingRewriter(model).Visit(newSource);
			}

			// TODO if needed: doc = doc.WithFilePath(doc.FilePath.Replace(baseDir, fixedDir));

			return doc.WithSyntaxRoot(newSource);
		}
	}
}

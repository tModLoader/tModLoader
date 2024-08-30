using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public abstract class RoslynTask : SetupOperation
	{
		private string projectPath;

		protected abstract string Status { get; }
		protected virtual int MaxDegreeOfParallelism => 0;

		public RoslynTask(ITaskInterface taskInterface) : base(taskInterface) { }

		public override bool ConfigurationDialog() => (bool)taskInterface.Invoke(new Func<bool>(() => {
			var dialog = new OpenFileDialog {
				FileName = projectPath,
				InitialDirectory = Path.GetDirectoryName(projectPath) ?? Path.GetFullPath("."),
				Filter = "C# Project|*.csproj",
				Title = "Select C# Project"
			};

			var result = dialog.ShowDialog();
			projectPath = dialog.FileName;
			return result == DialogResult.OK && File.Exists(projectPath);
		}));

		public override void Run() => RunAsync().GetAwaiter().GetResult();

		public async Task RunAsync() {
			using (var workspace = CreateWorkspace()) {
				// todo proper error log
				workspace.WorkspaceFailed += (o, e) => Console.Error.WriteLine(e.Diagnostic.Message);

				Console.WriteLine($"Loading project '{projectPath}'");

				// Attach progress reporter so we print projects as they are loaded.
				var project = await workspace.OpenProjectAsync(projectPath);
				var workItems = project.Documents.Select(doc => new WorkItem(Status+" "+doc.Name, async () => {
					var newDoc = await Process(doc);

					var before = await doc.GetTextAsync();
					var after = await newDoc.GetTextAsync();
					if (before != after)
						await File.WriteAllTextAsync(newDoc.FilePath, after.ToString());
				}));

				ExecuteParallel(workItems.ToList(), maxDegree: MaxDegreeOfParallelism);
			}
		}

		//protected virtual bool RequiresCodeAnalysis { get; }

		private static bool MSBuildFound = false;
		private MSBuildWorkspace CreateWorkspace() {
			//if (!RequiresCodeAnalysis)
			//	return new AdhocWorkspace();

			if (!MSBuildFound) {
				taskInterface.SetStatus("Finding MSBuild");
				var vsInst = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(inst => inst.Version).First();
				MSBuildLocator.RegisterInstance(vsInst);
				taskInterface.SetStatus($"Found MSBuild {vsInst.Version} at {vsInst.MSBuildPath}");
				MSBuildFound = true;
			}

			return MSBuildWorkspace.Create();
		}

		protected abstract Task<Document> Process(Document doc);

		private static Project adhocCSharpProject = new AdhocWorkspace().AddProject("", LanguageNames.CSharp);
		public static async Task<string> TransformSource(string source, CancellationToken cancel, Func<Document, CancellationToken, Task<Document>> transform) {
			var doc = adhocCSharpProject.AddDocument("", source);
			doc = await transform(doc, cancel).ConfigureAwait(false);
			return (await doc.GetTextAsync()).ToString();
		}
	}
}

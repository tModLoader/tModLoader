using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Terraria.ModLoader.Setup.Formatting;

namespace Terraria.ModLoader.Setup
{
	public partial class FormatTask : SetupOperation
	{
		private static AdhocWorkspace workspace = new AdhocWorkspace();
		static FormatTask() {
			FixRoslynFormatter.Apply();

			workspace.Options = workspace.Options
				.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);
		}

		public FormatTask(ITaskInterface taskInterface) : base(taskInterface) { }

		private static string projectPath; //persist across executions
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

		public override void Run() {
			var dir = Path.GetDirectoryName(projectPath); //just format all files in the directory
			var workItems = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
				.Select(path => new FileInfo(path))
				.OrderByDescending(f => f.Length)
				.Select(f => new WorkItem("Formatting: " + f.Name, () => FormatFile(f.FullName, taskInterface.CancellationToken, false)));


			ExecuteParallel(workItems.ToList());
		}

		public static void FormatFile(string path, CancellationToken cancellationToken, bool aggressive) {
			string source = File.ReadAllText(path);
			string formatted = Format(source, cancellationToken, aggressive);
			if (source != formatted)
				File.WriteAllText(path, formatted);
		}

		public static SyntaxNode Format(SyntaxNode node, CancellationToken cancellationToken, bool aggressive) {
			if (aggressive) {
				node = new NoNewlineBetweenFieldsRewriter().Visit(node);
				node = new RemoveBracesFromSingleStatementRewriter().Visit(node);
			}

			node = new AddVisualNewlinesRewriter().Visit(node);
			node = Formatter.Format(node, workspace, cancellationToken: cancellationToken);
			node = new CollectionInitializerFormatter().Visit(node);
			return node;
		}

		public static string Format(string source, CancellationToken cancellationToken, bool aggressive) {
			var tree = CSharpSyntaxTree.ParseText(source);
			return Format(tree.GetRoot(), cancellationToken, aggressive).ToFullString();
		}
	}
}

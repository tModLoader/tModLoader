using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Formatting;

namespace Terraria.ModLoader.Setup.Core
{
	public sealed class FormatTask : SetupOperation
	{
		private static readonly AdhocWorkspace Workspace = new();

		private readonly FormatTaskParameters parameters;

		static FormatTask() {
			var optionSet = Workspace.CurrentSolution.Options;

			// Essentials
			optionSet = optionSet
				.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true);

			// K&R
			optionSet = optionSet
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);

			// Fix switch indentation
			optionSet = optionSet
				.WithChangedOption(CSharpFormattingOptions.IndentSwitchCaseSection, true)
				.WithChangedOption(CSharpFormattingOptions.IndentSwitchCaseSectionWhenBlock, false);

			Workspace.TryApplyChanges(Workspace.CurrentSolution.WithOptions(optionSet));
		}

		public FormatTask(FormatTaskParameters parameters)
		{
			this.parameters = parameters;
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default) {
			using var taskProgress = progress.StartTask($"Formatting {Path.GetFileName(parameters.ProjectPath)}...");

			var dir = Path.GetDirectoryName(parameters.ProjectPath)!; //just format all files in the directory
			var workItems = Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
				.Select(path => new FileInfo(path))
				.OrderByDescending(f => f.Length)
				.Select(f => new WorkItem("Formatting: " + f.Name,
					ct => FormatFile(f.FullName, false, ct)));


			await ExecuteParallel(workItems.ToList(), taskProgress, cancellationToken: cancellationToken);
		}

		private static async ValueTask FormatFile(string path, bool aggressive, CancellationToken cancellationToken) {
			string source = await File.ReadAllTextAsync(path, cancellationToken);
			string formatted = Format(source, aggressive, cancellationToken);
			if (source != formatted)
				await File.WriteAllTextAsync(path, formatted, cancellationToken);
		}

		private static SyntaxNode Format(SyntaxNode node, bool aggressive, CancellationToken cancellationToken) {
			if (aggressive) {
				node = new NoNewlineBetweenFieldsRewriter().Visit(node);
				node = new RemoveBracesFromSingleStatementRewriter().Visit(node);
			}

			node = new AddVisualNewlinesRewriter().Visit(node)!;
			node = new FileScopedNamespaceRewriter().Visit(node)!;
			node = Formatter.Format(node, Workspace, cancellationToken: cancellationToken);
			node = new CollectionInitializerFormatter().Visit(node);
			return node;
		}

		public static string Format(string source, bool aggressive, CancellationToken cancellationToken) {
			SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(preprocessorSymbols: new[] { "SERVER" }));
			return Format(tree.GetRoot(), aggressive, cancellationToken).ToFullString();
		}
	}
}

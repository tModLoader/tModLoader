using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader.Setup.Formatting;

namespace Terraria.ModLoader.Setup
{
	public partial class FormatTask : RoslynTask
	{
		static FormatTask() {
			FixRoslynFormatter.Apply();
		}

		protected override string Status => "Formatting";
		//protected override int MaxDegreeOfParallelism => 1;

		public FormatTask(ITaskInterface taskInterface) : base(taskInterface) { }

		protected override async Task<Document> Process(Document doc) => await Format(doc, taskInterface.CancellationToken, false);

		public static async Task<Document> Format(Document doc, CancellationToken cancellationToken, bool agressive) {
			if (agressive) {
				doc = await Visit(doc, new NoNewlineBetweenFieldsRewriter());
				doc = await Visit(doc, new RemoveBracesFromSingleStatementRewriter());
			}

			doc = await Visit(doc, new AddVisualNewlinesRewriter());
			doc = await	RoslynFormat(doc, cancellationToken);
			doc = await Visit(doc, new CollectionInitializerFormatter());
			return doc;
		}

		private static async Task<Document> RoslynFormat(Document document, CancellationToken cancellationToken) {
			OptionSet options = await document.GetOptionsAsync();
			options = options.WithChangedOption(new OptionKey(FormattingOptions.UseTabs, LanguageNames.CSharp), true)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInProperties, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAccessors, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInControlBlocks, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousTypes, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, false)
				.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);

			return await Formatter.FormatAsync(document, options, cancellationToken);
		}
	}
}

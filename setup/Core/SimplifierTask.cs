using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Setup
{
	public class SimplifierTask : RoslynTask
	{
		protected override string Status => "Simplifying";
		protected override int MaxDegreeOfParallelism => 2;

		public SimplifierTask(ITaskInterface taskInterface) : base(taskInterface) { }

		protected override async Task<Document> Process(Document doc) {
			if (!(await doc.GetSyntaxRootAsync() is SyntaxNode root))
				return doc;

			root = root.WithAdditionalAnnotations(Simplifier.Annotation);
			return await Simplifier.ReduceAsync(doc.WithSyntaxRoot(root), cancellationToken: taskInterface.CancellationToken);
		}
	}
}

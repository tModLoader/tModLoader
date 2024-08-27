﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public sealed class SimplifierTask : RoslynTask
	{
		public SimplifierTask(IServiceProvider serviceProvider)
			: base(serviceProvider.GetRequiredService<ICSharpProjectSelectionPrompt>()) { }

		protected override string Status => "Simplifying";
		protected override int MaxDegreeOfParallelism => 2;

		protected override ITaskProgress GetTaskProgress(IProgress progress)
		{
			return progress.StartTask($"Simplifying {Path.GetFileName(ProjectPath)}...");
		}

		protected override async Task<Document> Process(Document doc, CancellationToken cancellationToken = default)
		{
			if (await doc.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not { } root) {
				return doc;
			}

			root = root.WithAdditionalAnnotations(Simplifier.Annotation);
			return await Simplifier.ReduceAsync(doc.WithSyntaxRoot(root), cancellationToken: cancellationToken).ConfigureAwait(false);
		}
	}
}
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public class CompositeTask : SetupOperation
	{
		private readonly SetupOperation[] tasks;
		private SetupOperation? failed;

		public CompositeTask(params SetupOperation[] tasks)
		{
			this.tasks = tasks;
		}

		public override async ValueTask ConfigurationPrompt(CancellationToken cancellationToken = default)
		{
			foreach (var task in tasks) {
				await task.ConfigurationPrompt(cancellationToken);
			}
		}

		public override bool Failed() => failed != null;

		public override bool Warnings() => tasks.Any(x => x.Warnings());

		public override void FinishedPrompt()
		{
			if (failed != null)
				failed.FinishedPrompt();
			else
				foreach (var task in tasks)
					task.FinishedPrompt();
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			foreach (var task in tasks) {
				await task.Run(progress, cancellationToken);
				if (task.Failed()) {
					failed = task;
					break;
				}
			}
		}
	}
}

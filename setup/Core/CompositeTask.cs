using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

public class CompositeTask : SetupOperation
{
	private readonly SetupOperation[] tasks;
	private SetupOperation? failed;

	public CompositeTask(params SetupOperation[] tasks)
	{
		this.tasks = tasks;
	}

	public override async ValueTask<bool> ConfigurationPrompt(CancellationToken cancellationToken = default)
	{
		foreach (var task in tasks) {
			bool result = await task.ConfigurationPrompt(cancellationToken).ConfigureAwait(false);

			if (!result) {
				return false;
			}
		}

		return true;
	}

	public override bool Failed()
	{
		return failed != null;
	}

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
			await task.Run(progress, cancellationToken).ConfigureAwait(false);
			if (task.Failed()) {
				failed = task;
			}
		}
	}
}
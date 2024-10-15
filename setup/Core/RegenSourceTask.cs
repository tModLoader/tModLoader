using DiffPatch;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public class RegenSourceTask : CompositeTask
	{
		private readonly ProgramSettings programSettings;
		private readonly IUserPrompt userPrompt;

		public RegenSourceTask(IServiceProvider serviceProvider) : base(GetOperations(serviceProvider))
		{
			programSettings = serviceProvider.GetRequiredService<ProgramSettings>();
			userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
		}

		public override async ValueTask<bool> StartupWarning()
		{
			if (programSettings.NoPrompts) {
				return true;
			}

			if (programSettings.PatchMode == Patcher.Mode.FUZZY && !await userPrompt.Prompt(
				    "Strict Patch Mode",
				    "Patch mode will be reset from fuzzy to offset.",
				    PromptOptions.OKCancel)) {
				return false;
			}

			return await userPrompt.Prompt(
				"Ready for Setup",
				"Any changes in /src will be lost.",
				PromptOptions.OKCancel);
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			if (programSettings.PatchMode == Patcher.Mode.FUZZY) {
				programSettings.PatchMode = Patcher.Mode.OFFSET;
				programSettings.Save();
			}

			await base.Run(progress, cancellationToken);
		}

		private static SetupOperation[] GetOperations(IServiceProvider serviceProvider)
		{
			ProgramSettings programSettings = serviceProvider.GetRequiredService<ProgramSettings>();

			return [
				new PatchTask(PatchTaskParameters.ForTerraria(programSettings), serviceProvider),
				new PatchTask(PatchTaskParameters.ForTerrariaNetCore(programSettings), serviceProvider),
				new PatchTask(PatchTaskParameters.ForTModLoader(programSettings), serviceProvider),
			];
		}
	}
}
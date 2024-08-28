using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core
{
	public sealed class SetupTask : CompositeTask
	{
		private readonly IUserPrompt userPrompt;
		private readonly ProgramSettings programSettings;

		public SetupTask(
			DecompileTaskParameters decompileTaskParameters,
			IServiceProvider serviceProvider)
			: base(new DecompileTask(decompileTaskParameters, serviceProvider), new RegenSourceTask(serviceProvider))
		{
			userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
			programSettings = serviceProvider.GetRequiredService<ProgramSettings>();
		}

		public override bool StartupWarning()
		{
			if (programSettings.NoPrompts) {
				return true;
			}

			return userPrompt.Prompt(
				"Ready for Setup",
				"Any changes in /src will be lost.",
				PromptOptions.OKCancel);
		}
	}
}
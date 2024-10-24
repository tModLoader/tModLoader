using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Setup.GUI;

public class UserPrompt : IUserPrompt
{
	public async Task<bool> Prompt(string caption, string text, PromptOptions options, PromptSeverity severity = PromptSeverity.Information)
	{
		IMsBox<string> messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams {
			Icon = GetMessageBoxIcon(severity),
			ButtonDefinitions = [
				new ButtonDefinition { Name = options == PromptOptions.RetryCancel ? "Retry" : "Ok" },
				new ButtonDefinition { Name = "Cancel", IsCancel = true },
			],
			ContentTitle = caption,
			ContentMessage = text,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
		});

		string result = await messageBox.ShowAsync();

		return result != "Cancel";
	}

	public async Task Inform(string caption, string text, PromptSeverity severity = PromptSeverity.Information)
	{
		IMsBox<ButtonResult> messageBox = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.Ok, GetMessageBoxIcon(severity));

		await messageBox.ShowAsync();
	}

	private static Icon GetMessageBoxIcon(PromptSeverity severity)
	{
		return severity switch {
			PromptSeverity.Information => Icon.Info,
			PromptSeverity.Warning => Icon.Warning,
			PromptSeverity.Error => Icon.Error,
			_ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
		};
	}
}
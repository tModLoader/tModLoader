using System.Windows.Forms;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.GUI;

public class UserPrompt : IUserPrompt
{
	public bool Prompt(
		string caption,
		string text,
		PromptOptions options,
		PromptSeverity severity = PromptSeverity.Information)
	{
		return MessageBox.Show(
			text,
			caption,
			GetButtons(options)) == DialogResult.OK;
	}

	public void Inform(string caption, string text, PromptSeverity severity = PromptSeverity.Information)
	{
		MessageBox.Show(
			text,
			caption,
			MessageBoxButtons.OK,
			GetIcon(severity));
	}

	private static MessageBoxButtons GetButtons(PromptOptions options)
	{
		return options switch {
			PromptOptions.OKCancel => MessageBoxButtons.OKCancel,
			PromptOptions.RetryCancel => MessageBoxButtons.RetryCancel,
			_ => MessageBoxButtons.OK,
		};
	}

	private static MessageBoxIcon GetIcon(PromptSeverity severity)
	{
		return severity switch {
			PromptSeverity.Information => MessageBoxIcon.Information,
			PromptSeverity.Warning => MessageBoxIcon.Warning,
			PromptSeverity.Error => MessageBoxIcon.Error,
			_ => MessageBoxIcon.None,
		};
	}
}
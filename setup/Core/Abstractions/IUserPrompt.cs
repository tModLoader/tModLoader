namespace Terraria.ModLoader.Setup.Core.Abstractions;

/// <summary>
///		Represents a type for showing a prompt to the user.
/// </summary>
public interface IUserPrompt
{
	/// <summary>
	///		Shows a prompt to the user that can be confirmed or declined.
	/// </summary>
	/// <param name="caption">The caption.</param>
	/// <param name="text">The text.</param>
	/// <param name="options">Options value that describes which options are shown to the user.</param>
	/// <param name="severity">An optional severity.</param>
	/// <returns><see langword="true"/> if the user confirmed the prompt; otherwise <see langword="false"/></returns>
	bool Prompt(
		string caption,
		string text,
		PromptOptions options,
		PromptSeverity severity = PromptSeverity.Information);

	/// <summary>
	///		Shows an informational message to the user.
	/// </summary>
	/// <param name="caption">The caption.</param>
	/// <param name="text">The text.</param>
	/// <param name="severity">An optional severity.</param>
	void Inform(string caption, string text, PromptSeverity severity = PromptSeverity.Information);
}
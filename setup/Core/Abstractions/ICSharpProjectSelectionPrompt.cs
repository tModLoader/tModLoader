namespace Terraria.ModLoader.Setup.Core.Abstractions;

/// <summary>
///     Represents a type for prompting the user for a .csproj file.
/// </summary>
public interface ICSharpProjectSelectionPrompt
{
	/// <summary>
	///		Prompts for a .csproj file and returns the path to the file that was selected by the user or
	///		<see langword="null"/> if no valid selection was made.
	/// </summary>
	/// <param name="currentProjectPath">The current project path if one has been previously selected. Used as default.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Path to the file that was selected by the user or <see langword="null"/> if no valid selection was made.</returns>
	Task<string?> Prompt(string? currentProjectPath, CancellationToken cancellationToken = default);
}
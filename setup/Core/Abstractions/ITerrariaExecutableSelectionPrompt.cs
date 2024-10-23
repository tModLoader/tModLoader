namespace Terraria.ModLoader.Setup.Core.Abstractions;

/// <summary>
///     Represents a type for prompting the user for a Terraria.exe file.
/// </summary>
public interface ITerrariaExecutableSelectionPrompt
{
	/// <summary>
	///		Prompts for a Terraria.exe file and returns the path to the file that was selected by the user.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Path to the file that was selected by the user</returns>
	Task<string> Prompt(CancellationToken cancellationToken = default);
}
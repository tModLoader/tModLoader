using DiffPatch;

namespace Terraria.ModLoader.Setup.Core.Abstractions;

/// <summary>
///     Represents a type for showing a patch review component.
/// </summary>
public interface IPatchReviewer
{
	/// <summary>
	///     Displays the patch review component.
	/// </summary>
	/// <param name="results">The file patcher results.</param>
	/// <param name="commonBasePath">The common base path.</param>
	void Show(IReadOnlyCollection<FilePatcher> results, string? commonBasePath = null);
}
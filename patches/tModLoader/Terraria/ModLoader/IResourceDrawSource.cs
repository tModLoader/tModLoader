using Terraria.GameContent.UI.ResourceSets;

#nullable enable

namespace Terraria.ModLoader
{
	public interface IResourceDrawSource
	{
		string? Context { get; }

		IPlayerResourcesDisplaySet DisplaySet { get; }
	}
}

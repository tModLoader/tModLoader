using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;
public sealed class UnloadedPrefix : ModPrefix
{
	public override LocalizedText DisplayName => Language.GetText("Mods.ModLoader.Unloaded");
	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		UnloadedGlobalItem unloadedGlobalItem = item.GetGlobalItem<UnloadedGlobalItem>();
		yield return new TooltipLine("UnloadedPrefix", $"{unloadedGlobalItem.modPrefixMod}/{unloadedGlobalItem.modPrefixName}") {
			IsModifier = true,
			OverrideColor = new(215, 123, 186)
		};
	}
}

using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.Default;
public sealed class UnloadedPrefix : ModPrefix
{
	public override LocalizedText DisplayName => LocalizedText.Empty;
	public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		UnloadedGlobalItem unloadedGlobalItem = item.GetGlobalItem<UnloadedGlobalItem>();
		yield return new TooltipLine("UnloadedPrefix", this.GetLocalization("Tooltip").Format($"{unloadedGlobalItem.ModPrefixMod}/{unloadedGlobalItem.ModPrefixName}")) {
			IsModifier = true,
			OverrideColor = new(215, 123, 186) // This is the color used in our unloaded textures
		};
	}
}

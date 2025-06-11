using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent.Creative;
using Terraria.Localization;

#pragma warning disable IDE0057 // Use range operator

namespace Terraria.ModLoader.Default.Patreon;

internal abstract class PatreonItem : ModLoaderModItem
{
	public string InternalSetName { get; set; }
	public string SetSuffix { get; set; }
	public override LocalizedText Tooltip => LocalizedText.Empty;

	public PatreonItem()
	{
		InternalSetName = GetType().Name;

		int lastUnderscoreIndex = InternalSetName.LastIndexOf('_');

		if (lastUnderscoreIndex != -1) {
			InternalSetName = InternalSetName.Substring(0, lastUnderscoreIndex);
		}

		SetSuffix = InternalSetName.EndsWith('s') ? "'" : "'s";
	}

	public override void SetStaticDefaults()
	{
		/*
		string displayName = Name.Replace('_', ' ');

		displayName = displayName.Insert(Name.IndexOf('_'), SetSuffix);

		DisplayName.SetDefault(displayName);
		*/
	}

	public override void SetDefaults()
	{
		Item.rare = 9;
		Item.vanity = true;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		var line = new TooltipLine(Mod, "PatreonThanks", Language.GetTextValue("tModLoader.PatreonSetTooltip")) {
			OverrideColor = Color.Aquamarine
		};
		tooltips.Add(line);
	}
}

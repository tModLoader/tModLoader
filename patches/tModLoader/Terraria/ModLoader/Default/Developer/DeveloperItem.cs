using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;

namespace Terraria.ModLoader.Default.Developer;

internal abstract class DeveloperItem : ModLoaderModItem
{
	public virtual string TooltipBrief { get; }
	public virtual string SetSuffix => "'s";

	public string InternalSetName => GetType().Name.Split('_')[0];

	public override void SetStaticDefaults()
	{
		/*
		string displayName = Name.Replace('_', ' ');
		displayName = displayName.Insert(displayName.IndexOf(' '), SetSuffix);
		DisplayName.SetDefault(displayName);
		*/
	}

	public override void SetDefaults()
	{
		Item.rare = 11;
		Item.vanity = true;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		var line = new TooltipLine(Mod, "DeveloperSetNote", $"{TooltipBrief}Developer Item") {
			OverrideColor = Color.OrangeRed
		};
		tooltips.Add(line);
	}
}

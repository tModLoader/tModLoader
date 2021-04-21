using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default.Developer
{
	internal abstract class DeveloperItem : ModLoaderModItem
	{
		public virtual string TooltipBrief { get; }
		public virtual string SetSuffix => "'s";

		public string InternalSetName => GetType().Name.Split('_')[0];

		public override void SetStaticDefaults() {
			var displayName = Name.Replace('_', ' ');
			displayName.Insert(displayName.IndexOf(' '), SetSuffix);
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults() {
			Item.rare = 11;
			Item.vanity = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			var line = new TooltipLine(Mod, "DeveloperSetNote", $"{TooltipBrief}Developer Item") {
				overrideColor = Color.OrangeRed
			};
			tooltips.Add(line);
		}
	}
}

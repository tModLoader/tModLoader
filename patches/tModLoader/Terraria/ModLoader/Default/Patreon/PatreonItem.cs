using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
	internal abstract class PatreonItem : ModLoaderModItem
	{
		public virtual string SetSuffix => "'s";

		public string InternalSetName => GetType().Name.Split('_')[0];

		public override void SetStaticDefaults() {
			var displayName = Name.Replace('_', ' ');
			displayName.Insert(displayName.IndexOf(' '), SetSuffix);
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults() {
			Item.rare = 9;
			Item.vanity = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			var line = new TooltipLine(Mod, "PatreonThanks", Language.GetTextValue("tModLoader.PatreonSetTooltip")) {
				overrideColor = Color.Aquamarine
			};
			tooltips.Add(line);
		}
	}
}

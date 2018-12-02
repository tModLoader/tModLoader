using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
	internal abstract class PatreonItem : ModItem
	{
		// Make sure the name and classname prefix match exactly.
		public abstract string SetName { get; }
		public abstract EquipType ItemEquipType { get; }
		public virtual string SetSuffix => "'s";

		protected string EquipTypeSuffix
			=> Enum.GetName(typeof(EquipType), ItemEquipType);

		public override string Texture => $"ModLoader/Patreon.{SetName}_{EquipTypeSuffix}";

		public override void SetStaticDefaults() {
			string displayName =
				EquipTypeSuffix != null
					? $"{SetName}{SetSuffix} {EquipTypeSuffix}"
					: "ITEM NAME ERROR";
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults() {
			item.rare = 9;
			item.vanity = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			var line = new TooltipLine(mod, "PatreonThanks", Language.GetTextValue("tModLoader.PatreonSetTooltip")) {
				overrideColor = Color.Aquamarine
			};
			tooltips.Add(line);
		}
	}
}

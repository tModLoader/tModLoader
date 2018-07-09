using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
	internal abstract class PatreonItem : ModItem
	{
		// Make sure the name and classname prefix match exactly.
		public abstract string PatreonName { get; }
		public abstract EquipType PatreonEquipType { get; }

		// Returns the appropriate suffix for the item, by EquipType
		private string GetEquipTypeSuffix()
		{
			switch (PatreonEquipType)
			{
				case EquipType.Head:
					return "Head";
				case EquipType.Body:
					return "Body";
				case EquipType.Legs:
					return "Legs";
				case EquipType.Wings:
					return "Wings";
			}

			return null;
		}

		public override string Texture => $"ModLoader/Patreon.{PatreonName}_{GetEquipTypeSuffix()}";

		public override void SetStaticDefaults()
		{
			string suffix = GetEquipTypeSuffix();
			string displayName =
				suffix != null
					? $"{PatreonName}'s {suffix}"
					: "ITEM NAME ERROR"; // Should never happen, but who knows
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults()
		{
			item.rare = 9;
			item.vanity = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var line = new TooltipLine(mod, "PatreonThanks", Language.GetTextValue("tModLoader.PatreonSetTooltip"));
			line.overrideColor = Color.Aquamarine;
			tooltips.Add(line);
		}
	}
}

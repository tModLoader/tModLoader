using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default.Developer
{
	internal abstract class DeveloperItem : ModItem
	{
		public virtual string TooltipBrief { get; }
		public abstract string SetName { get; }
		public abstract EquipType ItemEquipType { get; }
		public virtual string SetSuffix => "'s";

		protected string EquipTypeSuffix
			=> Enum.GetName(typeof(EquipType), ItemEquipType);

		public override string Texture => $"ModLoader/Developer.{SetName}_{EquipTypeSuffix}";

		public override void SetStaticDefaults()
		{
			string displayName =
				EquipTypeSuffix != null
				? $"{SetName}{SetSuffix} {EquipTypeSuffix}"
				: "ITEM NAME ERROR";
			DisplayName.SetDefault(displayName);
		}

		public override void SetDefaults()
		{
			item.rare = 11;
			item.vanity = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			var line = new TooltipLine(mod, "DeveloperSetNote", $"{TooltipBrief}Developer Item")
			{
				overrideColor = Color.OrangeRed
			};
			tooltips.Add(line);
		}
	}
}

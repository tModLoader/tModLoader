﻿using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class dinidini_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 28;
			Item.height = 20;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	internal class dinidini_Body : PatreonItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			ArmorIDs.Body.Sets.HidesTopSkin[Mod.GetEquipSlot(GetType().Name, EquipType.Body)] = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 28;
			Item.height = 24;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class dinidini_Legs : PatreonItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			ArmorIDs.Legs.Sets.OverridesLegs[Mod.GetEquipSlot(GetType().Name, EquipType.Legs)] = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 22;
			Item.height = 18;
		}
	}

	[AutoloadEquip(EquipType.Wings)]
	internal class dinidini_Wings : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.vanity = false;
			Item.width = 24;
			Item.height = 8;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.wingTimeMax = 150;
		}
	}
}

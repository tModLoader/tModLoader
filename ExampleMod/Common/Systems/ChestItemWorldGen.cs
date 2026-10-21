using System.Collections.Generic;
using ExampleMod.Content.Items.Mounts;
using ExampleMod.Content.Pets.ExampleLightPet;
using Terraria;
using Terraria.GameContent.ChestLootRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	// This class showcases declarative loot and chest-content rules.
	public class ChestItemWorldGen : ModSystem
	{
		private bool _bossChestRewardAdded;

		// Keep each entry's Condition visible: vanilla rates include conditional branches, not aggregate chest odds.
		public static List<ChestDropRateInfo> GetFrozenChestDropRates()
			=> ChestLootDatabase.GetDropRates(ChestID.Frozen);

		public override void PostSetupContent() {
			RegisterChestLoot();
		}

		private static void RegisterChestLoot() {
			// Add rules to the chest table; they run after vanilla fills chests during world generation.
			ChestLoot frozenChestLoot = ChestLootDatabase.For(ChestID.Frozen);
			frozenChestLoot.ReplaceItem(ItemID.IceTorch, ItemID.PinkJellyfishJar);
			frozenChestLoot.RemoveItem(ItemID.Snowball, 5);
			frozenChestLoot.AddCommon(ModContent.ItemType<ExampleMountItem>(), chanceDenominator: 3);

			ChestLoot goldChestLoot = ChestLootDatabase.For(ChestID.Gold);
			// Conditions are checked during generation and retained in drop-rate queries, like NPC loot.
			goldChestLoot.AddByCondition(Condition.InExpertMode,
				ChestLoot.Common(ModContent.ItemType<ExampleLightPetItem>(), chanceDenominator: 3));

			// Multiple conditions use AND semantics and are evaluated for each generated chest.
			goldChestLoot.AddByCondition(
				ChestLoot.Common(ItemID.GoldCoin),
				new ChestLootCondition(Condition.InExpertMode),
				new ChestLootCondition(Condition.NotRemixWorld));
			goldChestLoot.AddOneFromOptions(2,
				ModContent.ItemType<ExampleMountItem>(),
				ModContent.ItemType<ExampleLightPetItem>());

			ChestLoot shadowChestLoot = ChestLootDatabase.For(ChestID.LockedShadow);
			// Content edits run in registration order: clear first, then add the replacement loot.
			shadowChestLoot.ClearChestContents();
			shadowChestLoot.AddCommon(ModContent.ItemType<ExampleMountItem>());
			shadowChestLoot.AddCommon(ModContent.ItemType<ExampleLightPetItem>());
			shadowChestLoot.AddCommon(ItemID.PinkJellyfishJar, stackMin: 2, stackMax: 2);
		}

		public override void ClearWorld() {
			_bossChestRewardAdded = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (_bossChestRewardAdded)
				tag["bossChestRewardAdded"] = true;
		}

		public override void LoadWorldData(TagCompound tag) {
			_bossChestRewardAdded = tag.ContainsKey("bossChestRewardAdded");
		}

		// Boss progression happens after world generation, so check it in a world-update hook.
		public override void PostUpdateWorld() {
			if (Main.netMode == NetmodeID.MultiplayerClient || _bossChestRewardAdded || !NPC.downedBoss1)
				return;

			for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++) {
				Chest chest = Main.chest[chestIndex];
				if (chest == null || chest.type != ChestID.LockedShadow)
					continue;

				// This runtime progression reward is separate from the world-generation loot table.
				if (chest.AddItem(ModContent.ItemType<ExampleLightPetItem>())) {
					_bossChestRewardAdded = true;
					break;
				}
			}
		}
	}
}

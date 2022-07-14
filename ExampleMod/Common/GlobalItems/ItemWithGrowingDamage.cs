using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.GlobalItems
{
	internal class ItemWithGrowingDamage : GlobalItem
	{
		public int experience;
		public int level;
		public static int experiencePerLevel = 100;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			//Apply this GlobalItem to all swords
			return entity.DamageType == DamageClass.Melee && entity.noMelee == false && entity.useStyle == ItemUseStyleID.Swing;
		}

		public override GlobalItem Clone(Item item, Item itemClone) {
			//Overriding clone is required for GlobalItems that set InstancePerEntity to true.
			//It is expremely important to include complex fields and properties such as arrays, lists and dictionaries in the Clone method.
			//If they are not included, they will act like static fields or properties.
			//It is not required to include simple fields and properties such as int, float, bool.
			ItemWithGrowingDamage clone = (ItemWithGrowingDamage)base.Clone(item, itemClone);
			return clone;
		}

		public override void LoadData(Item item, TagCompound tag) {
			experience = tag.Get<int>("experience");//Load experience tag
			item.value += experience * 5;
			UpdateLevel();
		}
		public override void SaveData(Item item, TagCompound tag) {
			tag["experience"] = experience;//Save experience tag
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			//The sword gains experience when damaging an npc.
			int xp = damage;
			experience += xp;
			item.value += xp * 5;
			UpdateLevel();
		}
		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			//Gain 1% multiplicative damage for every level on the weapon.
			damage *= 1f + (float)level / 100f;
		}

		public void UpdateLevel() {
			level = experience / experiencePerLevel;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (experience > 0) {
				UpdateLevel();
				tooltips.Add(new TooltipLine(Mod, "level", $"Level: {level}") { OverrideColor = Color.LightGreen });
				string levelString = $" ({(level + 1) * experiencePerLevel - experience} to next level)";
				tooltips.Add(new TooltipLine(Mod, "experience", $"Experience: {experience}{levelString}") { OverrideColor = Color.White });
			}
		}
		public override void OnCreate(Item item, ItemCreationContext context, List<Item> consumedItems) {
			if(context is RecipeCreationContext) {
				foreach (Item ingredient in consumedItems) {
					if (ingredient.TryGetGlobalItem(out ItemWithGrowingDamage ingredientGlobal)) {
						//Transfer all experience from consumed items to the crafted item.
						experience += ingredientGlobal.experience;
						UpdateLevel();
					}
				}
			}
		}

		//Currently not used in ExampleMod, but functions correctly
		public static void SpawnCoins(Item item) {
			int coinValue = item.GetGlobalItem<ItemWithGrowingDamage>().experience;
			int valuePerCoin = 1000000;//Starting with value of 1 platinum coin.
			for (int i = 3; i >= 0; i--) {
				int coins = coinValue / valuePerCoin;
				coinValue %= valuePerCoin;
				valuePerCoin /= 100;
				if (coins > 0)
					Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), ItemID.CopperCoin + i, coins);
			}
		}
	}
}

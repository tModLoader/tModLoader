using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using System.IO;
using ExampleMod.Content.NPCs;

namespace ExampleMod.Common.GlobalItems
{
	public class WeaponWithGrowingDamage : GlobalItem
	{
		//Related to GlobalProjectile: ProjectileWithGrowingDamage

		public int experience;
		public static int experiencePerLevel = 100;
		private int lastBonusValue;
		private int stack = int.MinValue;
		public int level => experience / experiencePerLevel;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {//Apply to all weapons
			//Exclude items like Eye of Cthulhu shield
			if (entity.accessory)
				return false;

			if (entity.type is ItemID.CoinGun or ItemID.Snowball)
				return true;
			
			return entity.damage > 0 && entity.ammo == 0;
		}
		public override void SetDefaults(Item item) {
			if (item.type == ItemID.Snowball) {
				experience = 1;
				UpdateValue(item);
			}
		}
		public override void LoadData(Item item, TagCompound tag) {
			int xp = tag.Get<int>("experience");//Load experience tag
			GainExperience(item, xp, true);
		}

		public override void SaveData(Item item, TagCompound tag) {
			tag["experience"] = experience;//Save experience tag
		}

		public override void NetSend(Item item, BinaryWriter writer) {
			writer.Write(experience);
		}

		public override void NetReceive(Item item, BinaryReader reader) {
			int xp = reader.ReadInt32();
			GainExperience(item, xp, true);
		}
		
		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			OnHitNPCGeneral(item, player, target, damage, knockBack, crit);
		}

		public void OnHitNPCGeneral(Item item, Player player, NPC target, int damage, float knockBack, bool crit, Projectile projectile = null) {
			//The weapon gains experience when hitting an npc.
			int xp = damage;
			if (projectile != null)
				xp /= 2;

			GainExperience(item, xp);
		}

		public void GainExperience(Item item, int xp, bool setXp = false) {
			if (setXp) {
				experience = xp;
			}
			else {
				experience += xp;
			}

			UpdateValue(item);
		}

		private void UpdateValue(Item item, int stackChange = 0) {
			//The goal of UpdateValue is to give the item bonus value based on it's experience.
			//With stackable items such as Throwing Knives, the item.value is multiplied by the item.stack.  This is not something that is desired for this example.

			//Example: UpdateValue() is called when a stack is combined.
			//		The original item.stack was 10, the new item.stack is 100.
			//		The original experience was 40, the current experience is 40.
			//		The original extra coins from selling would be 
			//
			//	Previous Values:
			//			
			//			lastBonusValue: 200
			//			stack: 10
			//	New Values:
			//			

			if (stack == int.MinValue)
				stack = item.stack + stackChange;

			if (CheckStackZero(item))
				return;

			int lastBonusValuePerItem = lastBonusValue / stack;//lastBonusValue : 200, stack: 10, lastBonusValuePerItem = 20
			stack = item.stack + stackChange;//item.stack = 10, stackChange = 90, so the new stack will be 100.  stack = 100

			if (CheckStackZero(item))
				return;

			int newBonusValue = experience * 5;//experience: 40, newBonusValue = 200
			int newBonusValuePerItem = newBonusValue / stack;//newBonusValue: 200, stack: 100, newBonusValuePerItem = 2
			item.value += newBonusValuePerItem - lastBonusValuePerItem;//newBonusValuePerItem: 2, lastBonusValuePerItem, 20, item.value += -18
			lastBonusValue = newBonusValue;//newBonusValue: 200, lastBonusValue = 200
		}

		private bool CheckStackZero(Item item) {
			if (stack <= 0) {
				item.value = ContentSamples.ItemsByType[item.type].value;
				lastBonusValue = 0;
				return true;
			}

			return false;
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			//Gain 1% multiplicative damage for every level on the weapon.
			damage *= 1f + (float)level / 100f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (experience > 0) {
				tooltips.Add(new TooltipLine(Mod, "level", $"Level: {level}") { OverrideColor = Color.LightGreen });
				string levelString = $" ({(level + 1) * experiencePerLevel - experience} to next level)";
				tooltips.Add(new TooltipLine(Mod, "experience", $"Experience: {experience}{levelString}") { OverrideColor = Color.White });
			}
		}

		public override void OnCreate(Item item, ItemCreationContext context) {
			if (context is RecipeCreationContext rContext) {
				foreach (Item ingredient in rContext.ConsumedItems) {
					if (ingredient.TryGetGlobalItem(out WeaponWithGrowingDamage ingredientGlobal)) {
						//Transfer all experience from consumed items to the crafted item.
						GainExperience(item, ingredientGlobal.experience);
					}
				}
			}
		}

		public override void OnStack(Item item1, Item item2, int numberToBeTransfered) {
			if (!item1.TryGetGlobalItem(out WeaponWithGrowingDamage weapon1) || !item2.TryGetGlobalItem(out WeaponWithGrowingDamage weapon2))
				return;

			if (item1.stack == 0)
				weapon1.experience = 0;
			
			//Transfer experience and value to item1.
			weapon1.experience += weapon2.experience;//Works
			//experience += weapon2.experience;//Doesn't work
			weapon1.UpdateValue(item1, numberToBeTransfered);

			if (item2.stack > numberToBeTransfered) {
				//Prevent duplicating the experience by clearing it on item2 if item2 will still exist.
				weapon2.experience = 0;
				weapon2.UpdateValue(item2, -numberToBeTransfered);
			}
		}
	}
	public class SlowBallShop : GlobalNPC
	{
		public override void SetupShop(int type, Chest shop, ref int nextSlot) {
			if (type != ModContent.NPCType<ExamplePerson>())
				return;

			shop.item[nextSlot].SetDefaults(ItemID.Snowball);
			if (shop.item[nextSlot].TryGetGlobalItem(out WeaponWithGrowingDamage weapon))
				weapon.experience *= 2;

			nextSlot++;
		}
	}
}

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
using Terraria.Utilities;

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

		public override void OnCreate(Item item, ItemCreationContext context) {
			if (context is RecipeCreationContext rContext) {
				foreach (Item ingredient in rContext.ConsumedItems) {
					if (ingredient.TryGetGlobalItem(out ItemWithGrowingDamage ingredientGlobal)) {
						//Transfer all experience from consumed items to the crafted item.
						experience += ingredientGlobal.experience;
						UpdateLevel();
					}
					item.value += experience * 5;
				}
			}
		}
	}
}

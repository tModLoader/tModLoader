using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using System.IO;

namespace ExampleMod.Common.GlobalItems
{
	internal class ItemWithGrowingDamage : GlobalItem
	{
		public int experience;
		public static int experiencePerLevel = 100;
		public int level => experience / experiencePerLevel;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			//Apply this GlobalItem to all swords
			return entity.DamageType == DamageClass.Melee && entity.noMelee == false && entity.useStyle == ItemUseStyleID.Swing;
		}

		public override void LoadData(Item item, TagCompound tag) {
			int xp = tag.Get<int>("experience");//Load experience tag
			GainExperience(item, xp);
		}

		public override void SaveData(Item item, TagCompound tag) {
			tag["experience"] = experience;//Save experience tag
		}

		public override void NetSend(Item item, BinaryWriter writer) {
			writer.Write(experience);
		}

		public override void NetReceive(Item item, BinaryReader reader) {
			int xp = reader.ReadInt32();
			GainExperience(item, xp);
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			//The sword gains experience when damaging an npc.
			int xp = damage;
			GainExperience(item, xp);
		}

		public void GainExperience(Item item, int xp) {
			experience += xp;
			item.value += xp * 5;
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
					if (ingredient.TryGetGlobalItem(out ItemWithGrowingDamage ingredientGlobal)) {
						//Transfer all experience from consumed items to the crafted item.
						GainExperience(item, ingredientGlobal.experience);
					}
				}
			}
		}
	}
}

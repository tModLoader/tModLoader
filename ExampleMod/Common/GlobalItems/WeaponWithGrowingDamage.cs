using ExampleMod.Common.Configs;
using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

// Related to GlobalProjectile: ProjectileWithGrowingDamage
namespace ExampleMod.Common.GlobalItems
{
	public class WeaponWithGrowingDamage : GlobalItem
	{
		public int experience;
		public static int experiencePerLevel = 100;
		private int bonusValuePerItem;
		public int level => experience / experiencePerLevel;
		public static LocalizedText LevelText { get; private set; }
		public static LocalizedText ExperienceText { get; private set; }

		public override bool InstancePerEntity => true;

		public override bool IsLoadingEnabled(Mod mod) {
			// To experiment with this example, you'll need to enable it in the config.
			return ModContent.GetInstance<ExampleModConfig>().WeaponWithGrowingDamageToggle;
		}

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			// Apply to weapons
			return lateInstantiation && entity.damage > 0;
		}

		public override void SetStaticDefaults() {
			LevelText = Mod.GetLocalization($"{nameof(WeaponWithGrowingDamage)}.Level");
			ExperienceText = Mod.GetLocalization($"{nameof(WeaponWithGrowingDamage)}.Experience");
		}

		public override void LoadData(Item item, TagCompound tag) {
			experience = 0;
			GainExperience(item, tag.Get<int>("experience")); // Load experience tag
		}

		public override void SaveData(Item item, TagCompound tag) {
			tag["experience"] = experience; // Save experience tag
		}

		public override void NetSend(Item item, BinaryWriter writer) {
			writer.Write(experience);
		}

		public override void NetReceive(Item item, BinaryReader reader) {
			experience = 0;
			GainExperience(item, reader.ReadInt32());
		}

		public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			OnHitNPCGeneral(player, target, hit, item);
		}

		public void OnHitNPCGeneral(Player player, NPC target, NPC.HitInfo hit, Item item = null, Projectile projectile = null) {
			// The weapon gains experience when hitting an npc.
			int xp = hit.Damage;
			if (projectile != null) {
				xp /= 2;
			}

			GainExperience(item, xp);
		}

		public void GainExperience(Item item, int xp) {
			experience += xp;

			UpdateValue(item);
		}

		public void UpdateValue(Item item, int stackChange = 0) {
			if (item == null) {
				return;
			}

			item.value -= bonusValuePerItem;
			int stack = item.stack + stackChange;
			if (stack == 0) {
				bonusValuePerItem = 0;
			}
			else {
				bonusValuePerItem = experience * 5 / stack;
			}

			item.value += bonusValuePerItem;
		}

		public override void UpdateInventory(Item item, Player player) {
			UpdateValue(item);
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			// Gain 1% multiplicative damage for every level on the weapon.
			damage *= 1f + (float)level / 100f;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (experience > 0) {
				tooltips.Add(new TooltipLine(Mod, "level", LevelText.Format(level)) { OverrideColor = Color.LightGreen });
				int nextLevelExperience = (level + 1) * experiencePerLevel - experience;
				tooltips.Add(new TooltipLine(Mod, "experience", ExperienceText.Format(experience, nextLevelExperience)) { OverrideColor = Color.White });
			}
		}

		public override void OnCreated(Item item, ItemCreationContext context) {
			if (item.type == ItemID.Snowball) {
				GainExperience(item, item.stack); // snowballs come with 1xp, for testing :)
			}

			if (context is RecipeItemCreationContext rContext) {
				foreach (Item ingredient in rContext.ConsumedItems) {
					if (ingredient.TryGetGlobalItem(out WeaponWithGrowingDamage ingredientGlobal)) {
						// Transfer all experience from consumed items to the crafted item.
						GainExperience(item, ingredientGlobal.experience);
					}
				}
			}
		}

		public override void OnStack(Item destination, Item source, int numToTransfer) {
			if (!source.TryGetGlobalItem(out WeaponWithGrowingDamage weapon2)) {
				return;
			}

			TransferExperience(destination, source, weapon2, numToTransfer);
		}

		public override void SplitStack(Item destination, Item source, int numToTransfer) {
			if (!source.TryGetGlobalItem(out WeaponWithGrowingDamage weapon2)) {
				return;
			}

			// Prevent duplicating the experience on the new item, increase, which is a clone of decrease.  experience should not be cloned, so set it to 0.
			experience = 0;

			TransferExperience(destination, source, weapon2, numToTransfer);
		}

		private void TransferExperience(Item destination, Item source, WeaponWithGrowingDamage weapon2, int numToTransfer) {
			// Transfer experience and value to increase.
			experience += weapon2.experience;
			UpdateValue(destination, numToTransfer);

			if (source.stack > numToTransfer) {
				// Prevent duplicating the experience by clearing it on decrease if decrease will still exist.
				weapon2.experience = 0;
				weapon2.UpdateValue(source, -numToTransfer);
			}
		}
	}

	public class DoubleXPSnowBallInExamplePersonShop : GlobalNPC
	{
		public override bool IsLoadingEnabled(Mod mod) {
			// To experiment with this example, you'll need to enable it in the config.
			return ModContent.GetInstance<ExampleModConfig>().WeaponWithGrowingDamageToggle;
		}

		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType != ModContent.NPCType<ExamplePerson>()) {
				return;
			}

			var snowball = new Item(ItemID.Snowball);
			if (snowball.TryGetGlobalItem(out WeaponWithGrowingDamage weapon)) {
				weapon.GainExperience(snowball, 2);
			}
			shop.Add(snowball);
		}
	}
}

using ExampleMod.Content.Biomes;
using ExampleMod.Content.Buffs;
using ExampleMod.Content.Items.Armor;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleCostumePlayer : ModPlayer
	{
		// These 5 relate to ExampleCostume.
		public bool BlockyAccessoryPrevious;
		public bool BlockyAccessory;
		public bool BlockyHideVanity;
		public bool BlockyForceVanity;
		public bool BlockyPower;

		public override void ResetEffects() {
			BlockyAccessoryPrevious = BlockyAccessory;
			BlockyAccessory = BlockyHideVanity = BlockyForceVanity = BlockyPower = false;
		}

		public override void UpdateVisibleVanityAccessories() {
			for (int n = 13; n < 18 + Player.GetAmountOfExtraAccessorySlotsToShow(); n++) {
				Item item = Player.armor[n];
				if (item.type == ModContent.ItemType<ExampleCostume>()) {
					BlockyHideVanity = false;
					BlockyForceVanity = true;
				}
			}
		}

		public override void UpdateEquips() {
			// Make sure this condition is the same as the condition in the Buff to remove itself. We do this here instead of in ModItem.UpdateAccessory in case we want future upgraded items to set blockyAccessory
			if (Player.townNPCs >= 1 && BlockyAccessory) {
				Player.AddBuff(ModContent.BuffType<Blocky>(), 60);
			}
		}

		public override void FrameEffects() {
			// TODO: Need new hook, FrameEffects doesn't run while paused.
			if ((BlockyPower || BlockyForceVanity) && !BlockyHideVanity) {
				var exampleCostume = ModContent.GetInstance<ExampleCostume>();
				Player.head = EquipLoader.GetEquipSlot(Mod, exampleCostume.Name, EquipType.Head);
				Player.body = EquipLoader.GetEquipSlot(Mod, exampleCostume.Name, EquipType.Body);
				Player.legs = EquipLoader.GetEquipSlot(Mod, exampleCostume.Name, EquipType.Legs);

				// Use the alternative equipment textures by calling them through their internal name.
				if (Player.wet) {
					Player.head = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Head);
					Player.body = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Body);
					Player.legs = EquipLoader.GetEquipSlot(Mod, "BlockyAlt", EquipType.Legs);
				}
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if ((BlockyPower || BlockyForceVanity) && !BlockyHideVanity) {
				Player.headRotation = Player.velocity.Y * Player.direction * 0.1f;
				Player.headRotation = Utils.Clamp(Player.headRotation, -0.3f, 0.3f);
				if (Player.InModBiome<ExampleSurfaceBiome>()) {
					Player.headRotation = (float)Main.time * 0.1f * Player.direction;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
			if (BlockyAccessory) {
				playSound = false;
			}

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
			if (BlockyAccessory) {
				SoundEngine.PlaySound(SoundID.Frog, Player.position);
			}
		}
	}
}
using ExampleMod.Common.Biomes;
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
		public bool blockyAccessoryPrevious;
		public bool blockyAccessory;
		public bool blockyHideVanity;
		public bool blockyForceVanity;
		public bool blockyPower;

		public override void ResetEffects() {
			blockyAccessoryPrevious = blockyAccessory;
			blockyAccessory = blockyHideVanity = blockyForceVanity = blockyPower = false;
		}

		public override void UpdateVanityAccessories() {
			for (int n = 13; n < 18 + Player.GetAmountOfExtraAccessorySlotsToShow(); n++) {
				Item item = Player.armor[n];
				if (item.type == ModContent.ItemType<ExampleCostume>()) {
					blockyHideVanity = false;
					blockyForceVanity = true;
				}
			}
		}

		public override void UpdateEquips() {
			// Make sure this condition is the same as the condition in the Buff to remove itself. We do this here instead of in ModItem.UpdateAccessory in case we want future upgraded items to set blockyAccessory
			if (Player.townNPCs >= 1 && blockyAccessory) {
				Player.AddBuff(ModContent.BuffType<Blocky>(), 60);
			}
		}

		public override void FrameEffects() {
			if ((blockyPower || blockyForceVanity) && !blockyHideVanity) {
				var exampleCostume = ModContent.GetInstance<ExampleCostume>();
				Player.head = exampleCostume.EquipHeadSlot;
				Player.body = exampleCostume.EquipBodySlot;
				Player.legs = exampleCostume.EquipLegsSlot;
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if ((blockyPower || blockyForceVanity) && !blockyHideVanity) {
				Player.headRotation = Player.velocity.Y * Player.direction * 0.1f;
				Player.headRotation = Utils.Clamp(Player.headRotation, -0.3f, 0.3f);
				if (ModContent.GetInstance<ExampleBiome>().IsBiomeActive(Player)) {
					Player.headRotation = (float)Main.time * 0.1f * Player.direction;
				}
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			if (blockyAccessory) {
				playSound = false;
			}

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (blockyAccessory) {
				SoundEngine.PlaySound(SoundID.Zombie, Player.position, 13);
			}
		}
	}
}
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
		// These 6 relate to ExampleCostume.
		public bool BlockyAccessoryPrevious;
		public bool BlockyAccessory;             // If true, an accessory granting potential effects is equipped
		public bool BlockyHideVanity;            // If true, the item is in a hidden accessory slot
		public bool BlockyForceVanity;           //	If true, the vanity is forced because the item is in a vanity slot, not the stats.
		public bool BlockyPower;                 // If true, the stats boosts are applied
		public bool BlockyVanityEffects => BlockyForceVanity || (BlockyPower && !BlockyHideVanity); // This helper property controls if the audio and visual effects of the vanity should be applied.

		public override void ResetEffects() {
			BlockyAccessoryPrevious = BlockyAccessory;
			BlockyAccessory = BlockyHideVanity = BlockyForceVanity = BlockyPower = false;
		}

		public override void UpdateEquips() {
			// Make sure this condition is the same as the condition in the Buff to remove itself. We do this here instead of in ModItem.UpdateAccessory in case we want future upgraded items to set blockyAccessory
			if (Player.townNPCs >= 1 && BlockyAccessory) {
				Player.AddBuff(ModContent.BuffType<Blocky>(), 60);
			}
		}

		public override void FrameEffects() {
			// TODO: Need new hook, FrameEffects doesn't run while paused.
			if (BlockyVanityEffects) {
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
			if (BlockyVanityEffects) {
				Player.headRotation = Player.velocity.Y * Player.direction * 0.1f;
				Player.headRotation = Utils.Clamp(Player.headRotation, -0.3f, 0.3f);
				if (Player.InModBiome<ExampleSurfaceBiome>()) {
					Player.headRotation = (float)Main.time * 0.1f * Player.direction;
				}
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (BlockyVanityEffects) {
				modifiers.DisableSound();
			}
		}

		public override void OnHurt(Player.HurtInfo info) {
			if (BlockyVanityEffects) {
				// SoundID.Frog is actually SoundType.Ambient, so we need to change it to play at the correct SoundType.Sound master volume.
				SoundEngine.PlaySound(SoundID.Frog with { Type = SoundType.Sound }, Player.position);
			}
		}
	}
}
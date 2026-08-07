using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExampleVoiceAccessory : ModItem
	{
		// Here we define which sounds we want to use.
		public SoundStyle ExampleVoiceAccessoryHurtSound = new($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact") { PitchVariance = 0.1f };
		public SoundStyle ExampleVoiceAccessoryRareHurtSound = new($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact") { Pitch = 1f };
		public SoundStyle ExampleVoiceAccessoryDeathSound = SoundID.StatueMimicLaugh;

		public override void SetStaticDefaults() {
			// This marks this item as voice change item which causes naturally spawned slimes that carry this item to have triple HP.
			ItemID.Sets.IsAVoiceChangeItem[Type] = true;
			// This lets this item be randomly generated in chest loot and lets slimes in Skyblock spawn carrying with this item.
			ItemID.Sets.VoiceChangeItemForChestLootAndSlimes.Add(Type);
		}
		public override void SetDefaults() {
			// DefaultToVoiceOverrideAccessory sets a number of things for us such as the use style, use time, vanity, and accessory.
			// The important thing for this is Item.voiceSlot = Type
			Item.DefaultToVoiceOverrideAccessory(Type);
			/*
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = null;
			Item.useTurn = false;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.vanity = true;
			Item.voiceSlot = Type;
			*/
			Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));
		}

		// Here is where we can define our hurt sound. This optional. If you only want a hurt sound, don't override this hook.
		public override bool PlayerHurtSoundOverride(Entity entity) {
			// Rarely play a different sound like the Chicken Charm.
			if (Main.rand.NextBool(5)) {
				SoundEngine.PlaySound(ExampleVoiceAccessoryRareHurtSound, entity.position);
			}
			else {
				SoundEngine.PlaySound(ExampleVoiceAccessoryHurtSound, entity.position);
			}

			return true; // Return true to prevent vanilla from playing other sounds.
		}

		// Here is where we can define our death sound. This optional. If you only want a death sound, don't override this hook.
		// Note: The hurt sound will still play in addition to the death sound.
		public override bool PlayerDeathSoundOverride(Entity entity) {
			SoundEngine.PlaySound(ExampleVoiceAccessoryDeathSound, entity.position);
			return true;
		}
	}
}

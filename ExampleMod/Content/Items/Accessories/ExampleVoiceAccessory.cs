using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// [AutoloadEquip(EquipType.Voice)]
	public class ExampleVoiceAccessory : ModItem
	{
		public SoundStyle ExampleVoiceAccessoryHurtSound = new($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact") { PitchVariance = 0.1f };
		public SoundStyle ExampleVoiceAccessoryRareHurtSound = new($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact") { Pitch = 1f };
		public SoundStyle ExampleVoiceAccessoryDeathSound = SoundID.StatueMimicLaugh;

		public override void SetStaticDefaults() {
			ItemID.Sets.IsAVoiceChangeItem[Type] = true; 
		}
		public override void SetDefaults() {
			//Item.DefaultToVoiceOverrideAccessory(Type);
			// Item.voiceSlot = voiceOverrideID;
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
			Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));
		}

		public override bool PlayerHurtSoundOverride(Entity entity) {
			if (Main.rand.NextBool(5)) {
				SoundEngine.PlaySound(ExampleVoiceAccessoryRareHurtSound, entity.position);
			}
			else {
				SoundEngine.PlaySound(ExampleVoiceAccessoryHurtSound, entity.position);
			}

			return true;
		}

		public override bool PlayerDeathSoundOverride(Entity entity) {
			SoundEngine.PlaySound(ExampleVoiceAccessoryDeathSound, entity.position);
			return true;
		}
	}

	public class SpawnBlueSlimeWithExVoiceAccessory : ModItem
	{
		public override string Texture => "Terraria/Images/Item_1";
		public override void SetDefaults() {
			Item.width = 16;
			Item.height = 16;
			Item.useTime = 1;
			Item.useAnimation = 1;

			Item.useStyle = ItemUseStyleID.Shoot;
		}
		public override bool? UseItem(Player player) {
			NPC.NewNPC(new EntitySource_BossSpawn(player), (int)player.position.X, (int)player.position.Y, NPCID.BlueSlime, 0, default, ModContent.ItemType<ExampleVoiceAccessory>());
			return base.UseItem(player);
		}
	}

	public class ExampleVoiceAccessoryPlayer : ModPlayer
	{
		/*
		public SoundStyle ExampleVoiceAccessoryHurtSound = new($"{nameof(ExampleMod)}/Assets/Sounds/Items/BananaImpact");
		public SoundStyle ExampleVoiceAccessoryDeathSound = SoundID.StatueMimicLaugh;

		public override void OnHurt(Player.HurtInfo info) {
			if (!info.SoundDisabled) {
				if (Player.dead) {
					SoundEngine.PlaySound(ExampleVoiceAccessoryDeathSound, Player.position);
				}
				else {
					SoundEngine.PlaySound(ExampleVoiceAccessoryHurtSound, Player.position);
				}
			}
		}
		*/
		public override void PostUpdate() {
			Main.NewText($"Player.voiceOverride {Player.voiceOverride}");
		}
	}
}

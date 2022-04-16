using ExampleMod.Common.Players;
using ExampleMod.Content.Dusts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// This and several other classes show off using EquipTextures to do a Merfolk or Werewolf effect.
	// Typically Armor items are automatically paired with an EquipTexture, but we can manually use EquipTextures to achieve more unique effects.
	// There is code for this effect in many places, look in the following files for the full implementation:
	// NPCs.ExamplePerson drops this item when killed
	// Content.Items.Armor.ExampleCostume (below) is the accessory item that sets ExampleCostumePlayer values. Note that this item does not have EquipTypes set. This is a vital difference and key to our approach.
	// Content.Items.Armor.BlockyHead (below) is an EquipTexture class. It spawns dust when active.
	// ExampleCostume.Load() shows calling AddEquipTexture 3 times with appropriate parameters. This is how we register EquipTexture manually instead of the automatic pairing of ModItem and EquipTexture that other equipment uses.
	// Buffs.Blocky is the Buff that is shown while in Blocky mode. The buff is responsible for the actual stat effects of the costume. It also needs to remove itself when not near town npcs.
	// ExampleCostumePlayer has 5 bools. They manage the visibility and other things related to this effect.
	// ExampleCostumePlayer.ResetEffects resets those bool, except blockyAccessoryPrevious which is special because of the order of hooks.
	// ExampleCostumePlayer.UpdateVanityAccessories is responsible for forcing the visual effect of our costume if the item is in a vanity slot. Note that ModItem.UpdateVanitySet can't be used for this because it is called too late.
	// ExampleCostumePlayer.UpdateEquips is responsible for applying the Blocky buff to the player if the conditions are met and the accessory is equipped.
	// ExampleCostumePlayer.FrameEffects is most important. It overrides the drawn equipment slots and sets them to our Blocky EquipTextures.
	// ExampleCostumePlayer.ModifyDrawInfo is for some fun effects for our costume.
	// Remember that the visuals and the effects of Costumes must be kept separate. Follow this example for best results.
	public class ExampleCostume : ModItem
	{
		public override void Load() {
			// The code below runs only if we're not loading on a server
			if (Main.netMode != NetmodeID.Server) {
				// Add equip textures
				Mod.AddEquipTexture(new BlockyHead(), this, EquipType.Head, $"{Texture}_{EquipType.Head}");
				Mod.AddEquipTexture(new EquipTexture(), this, EquipType.Body, $"{Texture}_{EquipType.Body}");
				// The below 2 lines are equivalent
				//Mod.AddEquipTexture(new EquipTexture(), EquipType.Legs, $"{Texture}_{EquipType.Legs}");
				Mod.AddEquipTexture(this, EquipType.Legs, $"{Texture}_{EquipType.Legs}");
			}
		}

		// Called in SetStaticDefaults
		private void SetupDrawing() {
			int equipSlotHead = Mod.GetEquipSlot(Name, EquipType.Head);
			int equipSlotBody = Mod.GetEquipSlot(Name, EquipType.Body);
			int equipSlotLegs = Mod.GetEquipSlot(Name, EquipType.Legs);

			ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
			ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
			ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
			ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Charm of Example");
			Tooltip.SetDefault("Turns the holder into Blocky near town NPC");

			SetupDrawing();
		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.buyPrice(gold: 15);
			Item.rare = ItemRarityID.Pink;
			Item.canBePlacedInVanityRegardlessOfConditions = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			var p = player.GetModPlayer<ExampleCostumePlayer>();
			p.BlockyAccessory = true;
			p.BlockyHideVanity = hideVisual;
		}

		// TODO: Fix this once new hook prior to FrameEffects added.
		// Required so UpdateVanitySet gets called in EquipTextures
		public override bool IsVanitySet(int head, int body, int legs) => true;
	}

	public class BlockyHead : EquipTexture
	{
		public override void UpdateVanitySet(Player player) {
			if (Main.rand.NextBool(20)) {
				Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<Sparkle>());
			}
		}
	}
}
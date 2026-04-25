using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content;

/// <summary>
/// Example NPC interaction that gives the player an item when clicked.
/// </summary>
public class ExampleGiftInteraction : ModNPCInteraction
{
	public override int ForNPCType => NPCID.Guide; // Applies to the Guide

	public override string GetText() => "Give me a gift";

	public override void Interact()
	{
		// Give the player an ExampleItem
		var entitySource = Main.LocalPlayer.GetSource_GiftOrReward("Example Gift");
		Item.NewItem(entitySource, Main.LocalPlayer.Center, ModContent.ItemType<Items.ExampleItem>(), 1);

		// Show a message
		Main.NewText("The Guide gave you an Example Item!", 255, 200, 100);

		// Play a sound
		Terraria.Audio.SoundEngine.PlaySound(SoundID.Coins);
	}
}
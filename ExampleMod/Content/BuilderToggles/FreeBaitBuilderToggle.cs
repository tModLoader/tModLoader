using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.BuilderToggles;

// This example shows almost all BuilderToggle hooks.
// More like a "button" than a "toggle", which left clicking allows you to select bait type,
// while right clicking gives you free baits of the selected type.
// Custom drawing is contained in this example to handle frame changes.
public class FreeBaitBuilderToggle : BuilderToggle
{
	public static LocalizedText NameText { get; private set; }

	public override string HoverTexture => Texture;

	public override bool Active() => !Main.LocalPlayer.HeldItem.IsAir && Main.LocalPlayer.HeldItem.fishingPole > 0;

	public override int NumberOfStates => 4;

	// Sorted after Torch God toggle because that would be cool.
	public override Position OrderPosition => new After(TorchBiome);

	public override bool OnLeftClick(ref SoundStyle? sound) {
		// Change the click sound.
		// If you don't want a sound to play, set sound to null.
		sound = SoundID.DrumTomHigh;
		return true;
	}

	public override void OnRightClick() {
		// Give the player free baits when right clicked.
		SoundEngine.PlaySound(Main.rand.NextBool(2) ? SoundID.DrumCymbal1 : SoundID.DrumCymbal2);
		int itemType = CurrentState switch {
			0 => ItemID.ApprenticeBait,
			1 => ItemID.JourneymanBait,
			2 => ItemID.MasterBait,
			3 => ItemID.TruffleWorm,
			_ => throw new ArgumentOutOfRangeException()
		};

		Main.LocalPlayer.QuickSpawnItem(new EntitySource_Gift(Main.LocalPlayer), itemType, 10);
	}

	// Use custom drawing to handle frame changes.
	public override bool Draw(SpriteBatch spriteBatch, ref Texture2D texture, ref Vector2 position, ref Rectangle frame, ref Color color, ref float scale, ref SpriteEffects spriteEffects) {
		frame = texture.Frame(4, 2, CurrentState % 4);
		return true;
	}

	// Truffle Worm has a unique hover texture.
	public override bool DrawHover(SpriteBatch spriteBatch, ref Texture2D texture, ref Vector2 position, ref Rectangle frame,
		ref Color color, ref float scale, ref SpriteEffects spriteEffects) {
		frame = texture.Frame(4, 2, CurrentState is 3 ? 1 : 0, 1);
		return true;
	}

	public override void SetStaticDefaults() {
		NameText = this.GetLocalization(nameof(NameText));
	}

	public override string DisplayValue() {
		return NameText.Value + CurrentState switch {
			0 => Lang.GetItemNameValue(ItemID.ApprenticeBait),
			1 => Lang.GetItemNameValue(ItemID.JourneymanBait),
			2 => Lang.GetItemNameValue(ItemID.MasterBait),
			3 => Lang.GetItemNameValue(ItemID.TruffleWorm),
			_ => "Unknown (How did you get here?)"
		};
	}
}
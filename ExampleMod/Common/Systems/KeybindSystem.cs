using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// Acts as a container for keybinds registered by this mod.
	// See Common/Players/ExampleKeybindPlayer for usage.
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind RandomBuffKeybind { get; private set; }
		private ChromaHotkeyPainter.PaintKey randomBuffPaintKey;

		public override void Load() {
			// Registers a new keybind
			// We localize keybinds by adding a Mods.{ModName}.Keybind.{KeybindName} entry to our localization files. The actual text displayed to English users is in en-US.hjson
			RandomBuffKeybind = KeybindLoader.RegisterKeybind(Mod, "RandomBuff", "P");
		}

		// Please see ExampleMod.cs' Unload() method for a detailed explanation of the unloading process.
		public override void Unload() {
			// Not required if your AssemblyLoadContext is unloading properly, but nulling out static fields can help you figure out what's keeping it loaded.
			RandomBuffKeybind = null;
		}

		public override void PostUpdateTime() {
			// Issue, not updating _keys unless go to keybind menu.
			// TODO: Add PaintKey to ModKeybind? Add fullName to ModKeybind?
			randomBuffPaintKey = Main.ChromaPainter._keys["ExampleMod/RandomBuff"];

			/*
			if (Main.LocalPlayer.direction == -1)
				_healKey.SetSolid(Color.White);
			else
				_healKey.SetSolid(Color.DarkRed);
			*/

			if (Main.LocalPlayer.controlJump)
				randomBuffPaintKey.SetAlert(Color.Green, Color.Purple, 10, 10);
		}
	}
}

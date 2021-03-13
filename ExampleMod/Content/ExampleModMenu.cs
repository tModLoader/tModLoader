using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	public class ExampleModMenu : ModMenu
	{
		private const string menuAssetPath = "ExampleMod/Assets/Textures/Menu"; // Creates a constant variable representing the texture path, so we don't have to write it out multiple times

		private int _globalButtonOffset;
		private int _ticksHovered;

		public override Asset<Texture2D> Logo => base.Logo;

		public override Asset<Texture2D> SunTexture => ModContent.GetTexture($"{menuAssetPath}/ExampleSun");

		public override Asset<Texture2D> MoonTexture => ModContent.GetTexture($"{menuAssetPath}/ExampliumMoon");

		/*public override int Music => Mod.GetSoundSlot(SoundType.Music, ""); TODO: Reimplement music loading */

		/*public override ModSurfaceBgStyle MenuBackgroundStyle => Mod.GetSurfaceBgStyle(""); TODO: Reimplement backgrounds */

		public override string DisplayName => "Example ModMenu";

		public override void OnSelected() {
			SoundEngine.PlaySound(SoundID.Thunder); // Plays a thunder sound when this ModMenu is selected
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			drawColor = Main.DiscoColor; // Changes the draw color of the logo
			return true;
		}

		// This hook allows you to modify the buttons on the main menu
		// These changes only take place if your menu is loaded
		public override void ModifyMenuButtons(List<MenuButton> buttons) {
			// If there are no MenuButtons in the list, return, since there's nothing to do
			if (buttons.Count == 0)
				return;

			// Color the first button in the list rainbow
			// This will always be applied no matter what menu mode the user is on (menu modes are not the same as ModMenus)
			buttons.First().color = Main.DiscoColor;

			// Use a foreach loop to modify all buttons in the list
			foreach (MenuButton button in buttons) {
				// Forcefully set the yOffsetPos of every button to _globalButtonOffset, which is modified through our ExampleButton below
				button.yOffsetPos = _globalButtonOffset;
			}

			// Make sure we're on the title screen menu
			// If we aren't, then return and don't execute the code below
			if (Main.menuMode != MenuID.Title)
				return;

			MenuButton exampleButton = new MenuButton(Mod, $"{Mod.Name}:ExampleButton", "Example Button!") {
				color = Color.Goldenrod, // Change the default text color to GoldenRod
				onHover = ExampleButtonOnHover, // Invoke this method when the button is being hovered on
				onLeftClick = ExampleButtonOnLeftClick, // Invoke this method when the button is clicked with the left mouse button
				readonlyText = false, // Make sure the button is interactable
				unhoverableText = false, // Make sure you can hover over the button
				scale = 0.5f // Adjust the scale to make it considerably smaller
			};
				
			// Add our newly-created button to the end of the list
			// This will allow our button to actually appear
			buttons.Add(exampleButton);
		}

		private void ExampleButtonOnHover() {
			// Increment _ticksHovered every tick
			_ticksHovered++;

			// Every ten ticks this button is hovered on, increment _globalButtonOffset by 1
			if (_ticksHovered % 10 == 0)
				_globalButtonOffset++;
		}

		private void ExampleButtonOnLeftClick() {
			// When the button is left-clicked, reset _ticksHovered and _globalButtonOffset, resetting buttons to their normal position
			_ticksHovered = 0;
			_globalButtonOffset = 0;
		}
	}
}

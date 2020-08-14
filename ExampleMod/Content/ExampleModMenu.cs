using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Audio;
using Terraria.ID;

namespace ExampleMod.Content
{
	public class ExampleModMenu : ModMenu
	{
		private const string menuAssetPath = "ExampleMod/Assets/Textures/Menu"; // Creates a constant variable representing the texture path, so we don't have to write it out multiple times

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
	}
}

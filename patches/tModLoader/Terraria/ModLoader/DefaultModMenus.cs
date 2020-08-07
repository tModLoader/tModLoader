using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is the default modmenu - the one that tML uses and the default one upon entering the game for the first time.
	/// </summary>
	internal class MenutML : ModMenu
	{
		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			logoScale = 0.84f; // Preserves the old scale of this logo from before ModMenu was implemented.
			return true;
		}
	}

	/// <summary>
	/// The Journey's End theme converted into a ModMenu, so that it better fits with the new system.
	/// </summary>
	internal class MenuJourneysEnd : ModMenu
	{
		public override string NameOnMenu => "Journey's End";

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			Color color2 = new Color((byte)(drawColor.R * (Main.LogoA / 255f)), (byte)(drawColor.G * (Main.LogoA / 255f)), (byte)(drawColor.B * (Main.LogoA / 255f)), (byte)(drawColor.A * (Main.LogoA / 255f)));
			Color color3 = new Color((byte)(drawColor.R * (Main.LogoB / 255f)), (byte)(drawColor.G * (Main.LogoB / 255f)), (byte)(drawColor.B * (Main.LogoB / 255f)), (byte)(drawColor.A * (Main.LogoB / 255f)));
			spriteBatch.Draw(TextureAssets.Logo.Value, logoDrawCenter, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color2, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			spriteBatch.Draw(TextureAssets.Logo2.Value, logoDrawCenter, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color3, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			return false;
		}
	}

	/// <summary>
	/// The Terraria 1.3.5.3 theme converted into a ModMenu, so that it better fits with the new system.
	/// </summary>
	internal class MenuOldVanilla : ModMenu
	{
		public override bool IsAvailable => Main.instance.playOldTile;

		public override string NameOnMenu => "Terraria 1.3.5.3";

		public override int Music => MusicID.Title;

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
			Color color2 = new Color((byte)(drawColor.R * (Main.LogoA / 255f)), (byte)(drawColor.G * (Main.LogoA / 255f)), (byte)(drawColor.B * (Main.LogoA / 255f)), (byte)(drawColor.A * (Main.LogoA / 255f)));
			Color color3 = new Color((byte)(drawColor.R * (Main.LogoB / 255f)), (byte)(drawColor.G * (Main.LogoB / 255f)), (byte)(drawColor.B * (Main.LogoB / 255f)), (byte)(drawColor.A * (Main.LogoB / 255f)));
			spriteBatch.Draw(TextureAssets.Logo3.Value, logoDrawCenter, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color2, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			spriteBatch.Draw(TextureAssets.Logo4.Value, logoDrawCenter, new Rectangle(0, 0, TextureAssets.Logo.Width(), TextureAssets.Logo.Height()), color3, logoRotation, new Vector2(TextureAssets.Logo.Width() / 2, TextureAssets.Logo.Height() / 2), logoScale, SpriteEffects.None, 0f);
			return false;
		}
	}
}

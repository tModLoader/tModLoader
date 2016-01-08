using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public struct PlayerDrawInfo
	{
		public Player drawPlayer;
		public Vector2 position;
		public float shadow;
		public Vector2 itemLocation;
		public bool drawHands;
		public bool drawArms;
		public bool drawHeldProjInFrontOfHeldItemAndBody;
		public bool drawHair;
		public bool drawAltHair;
		public int hairShader;
		public int headArmorShader;
		public int bodyArmorShader;
		public int legArmorShader;
		public int handOnShader;
		public int handOffShader;
		public int backShader;
		public int frontShader;
		public int shoeShader;
		public int waistShader;
		public int shieldShader;
		public int neckShader;
		public int faceShader;
		public int balloonShader;
		public int wingShader;
		public int carpetShader;
		public Color hairColor;
		public Color eyeWhiteColor;
		public Color eyeColor;
		public Color faceColor;
		public Color bodyColor;
		public Color legColor;
		public Color shirtColor;
		public Color underShirtColor;
		public Color pantsColor;
		public Color shoeColor;
		public Color upperArmorColor;
		public Color middleArmorColor;
		public Color mountColor;
		public Color lowerArmorColor;
		public int headGlowMask;
		public int bodyGlowMask;
		public int armGlowMask;
		public int legGlowMask;
		public Color headGlowMaskColor;
		public Color bodyGlowMaskColor;
		public Color armGlowMaskColor;
		public Color legGlowMaskColor;
		public SpriteEffects spriteEffects;
		public Vector2 headOrigin;
		public Vector2 bodyOrigin;
		public Vector2 legOrigin;
	}

	public struct PlayerHeadDrawInfo
	{
		public SpriteBatch spriteBatch;
		public Player drawPlayer;
		public float alpha;
		public float scale;
		public short hairShader;
		public int armorShader;
		public Color eyeWhiteColor;
		public Color eyeColor;
		public Color hairColor;
		public Color skinColor;
		public Color armorColor;
		public SpriteEffects spriteEffects;
		public Vector2 drawOrigin;
		public bool drawHair;
		public bool drawAltHair;
	}
}

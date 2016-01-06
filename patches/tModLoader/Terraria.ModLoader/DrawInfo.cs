using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	public struct PlayerDrawInfo
	{
		public Player drawPlayer;
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader
{
	/// <summary>
	/// A struct that contains information that may help with PlayerLayer drawing.
	/// </summary>
	public struct PlayerDrawInfo
	{
		/// <summary>
		/// The player that is being drawn.
		/// </summary>
		public Player drawPlayer;
		/// <summary>
		/// The position the player should be drawn in. Use this; do not use drawPlayer.position.
		/// </summary>
		public Vector2 position;
		/// <summary>
		/// The transparency of the player, where 0f is fully opaque and 1f is fully transparent.
		/// </summary>
		public float shadow;
		/// <summary>
		/// Similar to Player.itemLocation, but takes PlayerDrawInfo.position into account.
		/// </summary>
		public Vector2 itemLocation;
		/// <summary>
		/// Whether or not the player's hands underneath the armor should be drawn.
		/// </summary>
		public bool drawHands;
		/// <summary>
		/// Whether or not the player's arms underneath the armor should be drawn.
		/// </summary>
		public bool drawArms;
		/// <summary>
		/// Whether or not the held projectile is drawn in front of or behind the held item and arms.
		/// </summary>
		public bool drawHeldProjInFrontOfHeldItemAndBody;
		/// <summary>
		/// Whether or not the player's hair is drawn.
		/// </summary>
		public bool drawHair;
		/// <summary>
		/// Whether or not the player's alternate (hat) hair is drawn.
		/// </summary>
		public bool drawAltHair;
		/// <summary>
		/// The ID of the shader (dye) on the player's hair.
		/// </summary>
		public int hairShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's head armor.
		/// </summary>
		public int headArmorShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's body armor.
		/// </summary>
		public int bodyArmorShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's leg armor.
		/// </summary>
		public int legArmorShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's hand on accessory.
		/// </summary>
		public int handOnShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's hand off accessory.
		/// </summary>
		public int handOffShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's back accessory.
		/// </summary>
		public int backShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's front accessory.
		/// </summary>
		public int frontShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's shoe accessory.
		/// </summary>
		public int shoeShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's waist accessory.
		/// </summary>
		public int waistShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's shield accessory.
		/// </summary>
		public int shieldShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's neck accessory.
		/// </summary>
		public int neckShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's face accessory.
		/// </summary>
		public int faceShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's balloon accessory.
		/// </summary>
		public int balloonShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's wings.
		/// </summary>
		public int wingShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's magic carpet.
		/// </summary>
		public int carpetShader;
		/// <summary>
		/// The color of the player's hair, with lighting and transparency taken into account.
		/// </summary>
		public Color hairColor;
		/// <summary>
		/// The color of the whites of the player's eyes, with lighting and transparency taken into account.
		/// </summary>
		public Color eyeWhiteColor;
		/// <summary>
		/// The color of the player's eyes, with lighting and transparency taken into account.
		/// </summary>
		public Color eyeColor;
		/// <summary>
		/// The color of the player's face, with lighting and transparency taken into account.
		/// </summary>
		public Color faceColor;
		/// <summary>
		/// The color of the player's body skin, with lighting and transparency taken into account.
		/// </summary>
		public Color bodyColor;
		/// <summary>
		/// The color of the player's leg skin, with lighting and transparency taken into account.
		/// </summary>
		public Color legColor;
		/// <summary>
		/// The color of the player's shirt, with lighting and transparency taken into account.
		/// </summary>
		public Color shirtColor;
		/// <summary>
		/// The color of the player's under-shirt, with lighting and transparency taken into account.
		/// </summary>
		public Color underShirtColor;
		/// <summary>
		/// The color of the player's pants, with lighting and transparency taken into account.
		/// </summary>
		public Color pantsColor;
		/// <summary>
		/// The color of the player's shoes, with lighting and transparency taken into account.
		/// </summary>
		public Color shoeColor;
		/// <summary>
		/// The color of all armor and accessories on the upper third of the player, with lighting and transparency taken into account.
		/// </summary>
		public Color upperArmorColor;
		/// <summary>
		/// The color of all armor and accessories on the middle third of the player, with lighting and transparency taken into account.
		/// </summary>
		public Color middleArmorColor;
		/// <summary>
		/// The color of the player's mount, with lighting and transparency taken into account.
		/// </summary>
		public Color mountColor;
		/// <summary>
		/// The color of all armor and accessories on the lower third of the player, with lighting and transparency taken into account.
		/// </summary>
		public Color lowerArmorColor;
		/// <summary>
		/// The ID of the glow-mask on the player's head.
		/// </summary>
		public int headGlowMask;
		/// <summary>
		/// The ID of the glow-mask on the player's body.
		/// </summary>
		public int bodyGlowMask;
		/// <summary>
		/// The ID of the glow-mask on the player's arms.
		/// </summary>
		public int armGlowMask;
		/// <summary>
		/// The ID of the glow-mask on the player's legs.
		/// </summary>
		public int legGlowMask;
		/// <summary>
		/// The color of the glow-mask on the player's head.
		/// </summary>
		public Color headGlowMaskColor;
		/// <summary>
		/// The color of the glow-mask on the player's body.
		/// </summary>
		public Color bodyGlowMaskColor;
		/// <summary>
		/// The color of the glow-mask on the player's arms.
		/// </summary>
		public Color armGlowMaskColor;
		/// <summary>
		/// The color of the glow-mask on the player's legs.
		/// </summary>
		public Color legGlowMaskColor;
		/// <summary>
		/// The SpriteEffects that should be used to draw the player (how the sprite should be flipped).
		/// </summary>
		public SpriteEffects spriteEffects;
		/// <summary>
		/// The point around which the player's head texture rotates.
		/// </summary>
		public Vector2 headOrigin;
		/// <summary>
		/// The point around which the player's body texture rotates.
		/// </summary>
		public Vector2 bodyOrigin;
		/// <summary>
		/// The point around which the player's leg texture rotates.
		/// </summary>
		public Vector2 legOrigin;
	}

	/// <summary>
	/// A struct that contains information that may help with PlayerHeadLayer drawing.
	/// </summary>
	public struct PlayerHeadDrawInfo
	{
		/// <summary>
		/// The SpriteBatch object that should be used to do all the drawing. This is the same as Main.spriteBatch.
		/// </summary>
		public SpriteBatch spriteBatch;
		/// <summary>
		/// The player whose head is being drawn.
		/// </summary>
		public Player drawPlayer;
		/// <summary>
		/// The transparency in which the player should be drawn. 0 means fully transparent, while 1 means fully opaque.
		/// </summary>
		public float alpha;
		/// <summary>
		/// The scale on the size in which the player should be drawn.
		/// </summary>
		public float scale;
		/// <summary>
		/// The ID of the shader (dye) on the player's hair.
		/// </summary>
		public short hairShader;
		/// <summary>
		/// The ID of the shader (dye) on the player's head armor.
		/// </summary>
		public int armorShader;
		/// <summary>
		/// The color of the whites of the player's eyes. Alpha has already been taken into account.
		/// </summary>
		public Color eyeWhiteColor;
		/// <summary>
		/// The color of the player's eyes. Alpha has already been taken into account.
		/// </summary>
		public Color eyeColor;
		/// <summary>
		/// The color of the player's hair. Alpha has already been taken into account.
		/// </summary>
		public Color hairColor;
		/// <summary>
		/// The color of the player's skin. Alpha has already been taken into account.
		/// </summary>
		public Color skinColor;
		/// <summary>
		/// The color the player's armor should be shaded in. Alpha has already been taken into account.
		/// </summary>
		public Color armorColor;
		/// <summary>
		/// The SpriteEffects that should be used to draw the player. (SpriteEffects.None or SpriteEffects.FlipHorizontal)
		/// </summary>
		public SpriteEffects spriteEffects;
		/// <summary>
		/// The point on the player's texture around which everything should be rotated.
		/// </summary>
		public Vector2 drawOrigin;
		/// <summary>
		/// Whether the player's hair texture should be drawn.
		/// </summary>
		public bool drawHair;
		/// <summary>
		/// Whether the player's alternate (hat) hair texture should be drawn.
		/// </summary>
		public bool drawAltHair;
	}
}

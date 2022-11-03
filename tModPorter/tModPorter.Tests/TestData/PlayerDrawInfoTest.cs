using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

public class PlayerDrawInfoTest
{
	void Method() {
		PlayerDrawInfo info = default;
		Vector2 position = info.position;
		Vector2 itemLocation = info.itemLocation;
		bool drawHands = info.drawHands; // Negated, the non-negated version (missingHand) is internal
		bool drawArms = info.drawArms; // Negated, the non-negated version (missingArm) is internal
		bool drawHeldProjInFrontOfHeldItemAndBody = info.drawHeldProjInFrontOfHeldItemAndBody;
		bool drawHair = info.drawHair;
		bool drawAltHair = info.drawAltHair;
		//int hairShader = info.hairShader; // Can't be reasonably ported, hairDyePacked has a different representation and needs (un)packing
		int headArmorShader = info.headArmorShader;
		int bodyArmorShader = info.bodyArmorShader;
		int legArmorShader = info.legArmorShader;
		int handOnShader = info.handOnShader;
		int handOffShader = info.handOffShader;
		int backShader = info.backShader;
		int frontShader = info.frontShader;
		int shoeShader = info.shoeShader;
		int waistShader = info.waistShader;
		int shieldShader = info.shieldShader;
		int neckShader = info.neckShader;
		int faceShader = info.faceShader;
		int balloonShader = info.balloonShader;
		int wingShader = info.wingShader;
		int carpetShader = info.carpetShader;

		Color hairColor = info.hairColor;
		Color eyeWhiteColor = info.eyeWhiteColor;
		Color eyeColor = info.eyeColor;
		Color faceColor = info.faceColor;
		Color bodyColor = info.bodyColor;
		Color legColor = info.legColor;
		Color shirtColor = info.shirtColor;
		Color underShirtColor = info.underShirtColor;
		Color pantsColor = info.pantsColor;
		Color shoeColor = info.shoeColor;
		Color upperArmorColor = info.upperArmorColor;
		Color middleArmorColor = info.middleArmorColor;
		Color mountColor = info.mountColor;
		Color lowerArmorColor = info.lowerArmorColor;

		int legGlowMask = info.legGlowMask;
		Color legGlowMaskColor = info.legGlowMaskColor;
		SpriteEffects spriteEffects = info.spriteEffects;
		Vector2 headOrigin = info.headOrigin;
		Vector2 bodyOrigin = info.bodyOrigin;
		Vector2 legOrigin = info.legOrigin;
	}

	void ListTest(ref PlayerDrawInfo info) {
		Main.playerDrawDust.Add(1);
		Main.playerDrawGore.Add(2);
	}
}
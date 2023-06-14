using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

public class PlayerDrawInfoTest
{
	void Method() {
		PlayerDrawSet info = default;
		Vector2 position = info.Position;
		Vector2 itemLocation = info.ItemLocation;
		// not-yet-implemented
		bool drawHands = !info.armorHidesHands; // Negated, the non-negated version (missingHand) is internal
		bool drawArms = !info.armorHidesArms; // Negated, the non-negated version (missingArm) is internal
		// instead-expect
#if COMPILE_ERROR
		bool drawHands = info.drawHands; // Negated, the non-negated version (missingHand) is internal
		bool drawArms = info.drawArms; // Negated, the non-negated version (missingArm) is internal
#endif		
		bool drawHeldProjInFrontOfHeldItemAndBody = info.heldProjOverHand;
		bool drawHair = info.fullHair;
		bool drawAltHair = info.hatHair;
		//int hairShader = info.hairShader; // Can't be reasonably ported, hairDyePacked has a different representation and needs (un)packing
		int headArmorShader = info.cHead;
		int bodyArmorShader = info.cBody;
		int legArmorShader = info.cLegs;
		int handOnShader = info.cHandOn;
		int handOffShader = info.cHandOff;
		int backShader = info.cBack;
		int frontShader = info.cFront;
		int shoeShader = info.cShoe;
		int waistShader = info.cWaist;
		int shieldShader = info.cShield;
		int neckShader = info.cNeck;
		int faceShader = info.cFace;
		int balloonShader = info.cBalloon;
		int wingShader = info.cWings;
		int carpetShader = info.cCarpet;

		Color hairColor = info.colorHair;
		Color eyeWhiteColor = info.colorEyeWhites;
		Color eyeColor = info.colorEyes;
		Color faceColor = info.colorHead;
		Color bodyColor = info.colorBodySkin;
		Color legColor = info.colorLegs;
		Color shirtColor = info.colorShirt;
		Color underShirtColor = info.colorUnderShirt;
		Color pantsColor = info.colorPants;
		Color shoeColor = info.colorShoes;
		Color upperArmorColor = info.colorArmorHead;
		Color middleArmorColor = info.colorArmorBody;
		Color mountColor = info.colorMount;
		Color lowerArmorColor = info.colorArmorLegs;

		int legGlowMask = info.legsGlowMask;
		Color legGlowMaskColor = info.legsGlowColor;
		SpriteEffects spriteEffects = info.playerEffect;
		Vector2 headOrigin = info.headVect;
		Vector2 bodyOrigin = info.bodyVect;
		Vector2 legOrigin = info.legVect;
	}

	void ListTest(ref PlayerDrawSet info) {
		// not-yet-implemented
		info.DustCache.Add(1);
		info.GoreCache.Add(2);
		// instead-expect
#if COMPILE_ERROR
		Main.playerDrawDust.Add(1);
		Main.playerDrawGore.Add(2);
#endif	
	}
}
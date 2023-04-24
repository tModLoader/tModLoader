using Terraria;
using Terraria.ModLoader; 

public class ModWallTest : ModWall
{
	void Method() {
#if COMPILE_ERROR
		ItemDrop/* tModPorter Note: Removed. No longer needed */ = 1;
		ItemDrop/* tModPorter Note: Removed. No longer needed */ = 12;
#endif
		DustType = 0;
#if COMPILE_ERROR
		// not-yet-implemented
		HitSound/* tModPorter Suggestion: Use a SoundStyle here */ = 0;
		// instead-expect
		HitSound = 1;
		soundStyle/* tModPorter Note: Removed. Integrate into HitSound */ = 0;
#endif
	}
}
using Terraria;
using Terraria.ModLoader; 

public class ModWallTest : ModWall
{
	void Method() {
		ItemDropOverride = 1;
		ItemDropOverride = 12;
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
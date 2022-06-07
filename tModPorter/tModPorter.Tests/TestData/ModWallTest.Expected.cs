using Terraria;
using Terraria.ModLoader; 

public class ModWallTest : ModWall
{
	void Method() {
		ItemDrop = 1;
		DustType = 0;
#if COMPILE_ERROR
		HitSound /* Suggestion: Use a SoundStyle here */ = 0;
		soundStyle /* Suggestion: Utilize HitSound */ = 0;
#endif
	}
}
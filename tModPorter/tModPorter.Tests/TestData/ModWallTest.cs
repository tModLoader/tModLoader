using Terraria;
using Terraria.ModLoader; 

public class ModWallTest : ModWall
{
	void Method() {
		drop = 1;
		dustType = 0;
#if COMPILE_ERROR
		soundType = 1;
		soundStyle = 0;
#endif
	}
}
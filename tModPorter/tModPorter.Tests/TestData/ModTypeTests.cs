using Terraria.ModLoader;
using System.Collections.Generic;

// General ModType tests that cover a particular refactor for better overview
// Also contain lesser used ModTypes
public class ModTypeModItemTest : ModItem
{
	void Method(ref string s) {
		var a = mod;
	}

	// Most "ModTypes" had this Autoload variant in 1.3
	public override bool Autoload(ref string name) {
#if COMPILE_ERROR
		Method(ref name);
		name = "n1";
		name = "n2";
#endif
		return true;
	}
}

// Below are exceptions to the Autoload rule
public class ModTypeModBuffTest : ModBuff
{
	public override bool Autoload(ref string name, ref string texture) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModDustTest : ModDust
{
	public override bool Autoload(ref string name, ref string texture) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModTileTest : ModTile
{
	public override bool Autoload(ref string name, ref string texture)
	{
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWallTest : ModWall
{
	public override bool Autoload(ref string name, ref string texture) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWaterfallStyleTest : ModWaterfallStyle
{
	void Method() {
		int type = Type;
	}

	public override bool Autoload(ref string name, ref string texture) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWaterStyleTest : ModWaterStyle
{
	void Method() {
		int type = Type;
    }

	public override bool Autoload(ref string name, ref string texture, ref string blockTexture) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
		blockTexture = "b1";
		blockTexture = "b2";
#endif
		return true;
	}

	// Mandatory
	public override int ChooseWaterfallStyle() { return 0; }

	public override int GetSplashDust() { return 0; }

	public override int GetDropletGore() { return 0; }
}

public class ModTypeModMountDataTest : ModMountData
{
	public override bool Autoload(ref string name, ref string texture, IDictionary<MountTextureType, string> extraTextures) {
#if COMPILE_ERROR
		name = "n1";
		name = "n2";
		texture = "t1";
		texture = "t2";
		extraTextures = null;
		extraTextures = null;
#endif
		return true;
	}
}

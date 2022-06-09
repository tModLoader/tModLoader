using Terraria.ModLoader;
using System.Collections.Generic;

// General ModType tests that cover a particular refactor for better overview
// Also contain lesser used ModTypes
public class ModTypeModItemTest : ModItem
{
	void Method(ref string s) {
		var a = Mod;
	}

	// Most "ModTypes" had this Autoload variant in 1.3
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		Method(ref name/* name is defined by overridable Name property */);
		name = "n1";
		name = "n2";
#endif
		return true;
	}
}

// Below are exceptions to the Autoload rule
public class ModTypeModBuffTest : ModBuff
{
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModDustTest : ModDust
{
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModTileTest : ModTile
{
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWallTest : ModWall
{
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWaterfallStyleTest : ModWaterfallStyle
{
	void Method() {
		int type = Slot;
	}

	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
#endif
		return true;
	}
}

public class ModTypeModWaterStyleTest : ModWaterStyle
{
	void Method() {
		int type = Slot;
    }

	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
		blockTexture/* blockTexture is defined by overridable BlockTexture property */ = "b1";
		blockTexture = "b2";
#endif
		return true;
	}

	// Mandatory
	public override int ChooseWaterfallStyle() { return 0; }

	public override int GetSplashDust() { return 0; }

	public override int GetDropletGore() { return 0; }
}

public class ModTypeModMountDataTest : ModMount
{
	public override bool IsLoadingEnabled(Mod mod)/* tModPorter Suggestion: If you return false for the purposes of manual loading, use the [Autoload(false)] attribute on your class instead */ {
#if COMPILE_ERROR
		name/* name is defined by overridable Name property */ = "n1";
		name = "n2";
		texture/* texture is defined by overridable Texture property */ = "t1";
		texture = "t2";
		extraTextures/* Use GetExtraTexture hook inplace of extraTextures */ = null;
		extraTextures = null;
#endif
		return true;
	}
}
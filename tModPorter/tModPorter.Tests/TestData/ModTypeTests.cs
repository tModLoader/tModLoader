using Terraria.ModLoader;
using System.Collections.Generic;

// General ModType tests that cover a particular refactor for better overview
// Also contain lesser used ModTypes
public class ModTypeModItemTest : ModItem
{
	void Method() {
		var a = mod;
	}

	// Most "ModTypes" had this Autoload variant in 1.3
	public override bool Autoload(ref string name) { return true; /* Empty */ }
}

// Below are exceptions to the Autoload rule
public class ModTypeModBuffTest : ModBuff
{
	public override bool Autoload(ref string name, ref string texture) { return true; /* Empty */ }
}

public class ModTypeModDustTest : ModDust
{
	public override bool Autoload(ref string name, ref string texture) { return true; /* Empty */ }
}

public class ModTypeModTileTest : ModTile
{
	public override bool Autoload(ref string name, ref string texture) { return true; /* Empty */ }
}

public class ModTypeModWallTest : ModWall
{
	public override bool Autoload(ref string name, ref string texture) { return true; /* Empty */ }
}

public class ModTypeModWaterfallStyleTest : ModWaterfallStyle
{
	void Method() {
		int type = Type;
	}

	public override bool Autoload(ref string name, ref string texture) { return true; /* Empty */ }
}

public class ModTypeModWaterStyleTest : ModWaterStyle
{
	void Method() {
		int type = Type;
    }

	public override bool Autoload(ref string name, ref string texture, ref string blockTexture) { return true; /* Empty */ } //TODO BlockTexture => Texture + "_Block"

	// Mandatory
	public override int ChooseWaterfallStyle() { return 0; }

	public override int GetSplashDust() { return 0; }

	public override int GetDropletGore() { return 0; }
}

public class ModTypeModMountDataTest : ModMountData
{
	public override bool Autoload(ref string name, ref string texture, IDictionary<MountTextureType, string> extraTextures) { return true; /* Empty */ } //TODO GetExtraTexture hook
}

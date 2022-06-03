using Terraria.ModLoader;
using System.Collections.Generic;

// General ModType tests that cover a particular refactor for better overview
// Also contain lesser used ModTypes
public class ModTypeModItemTest : ModItem
{
	void Method() {
		var a = Mod;
	}

	// Most "ModTypes" had this Autoload variant in 1.3
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property */ { return true; /* Empty */ }
}

// Below are exceptions to the Autoload rule
public class ModTypeModBuffTest : ModBuff
{
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture */ { return true; /* Empty */ }
}

public class ModTypeModDustTest : ModDust
{
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture */  { return true; /* Empty */ }
}

public class ModTypeModTileTest : ModTile
{
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture */  { return true; /* Empty */ }
}

public class ModTypeModWallTest : ModWall
{
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture */  { return true; /* Empty */ }
}

public class ModTypeModWaterfallStyleTest : ModWaterfallStyle
{
	void Method() {
		int type = Slot;
	}

	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture */ { return true; /* Empty */ }
}

public class ModTypeModWaterStyleTest : ModWaterStyle
{
	void Method() {
		int type = Slot;
    }

	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture and blockTexture */ { return true; /* Empty */ }

	// Mandatory
	public override int ChooseWaterfallStyle() { return 0; }

	public override int GetSplashDust() { return 0; }

	public override int GetDropletGore() { return 0; }
}

public class ModTypeModMountDataTest : ModMount
{
	public override bool IsLoadingEnabled(Mod mod) /* Suggestion: Use Mod.AddContent(instance) in Mod.Load, name is defined by overridable Name property, same with texture. Use GetExtraTexture hook inplace of extraTextures */ { return true; /* Empty */ }
}

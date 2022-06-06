using System;
using Terraria.ModLoader;

public class ModGoreTest : ModGore
{
	public void IdentifierTest() {
		Console.Write(UpdateType);
	}

#if COMPILE_ERROR
	public override bool DrawBehind(Gore gore)/* Suggestion: Removed. Use GoreID.Sets.DrawBehind[Type] in SetStaticDefaults */ { return false; }
#endif
}
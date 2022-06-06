using System;
using Terraria;
using Terraria.ModLoader;

public class ModGoreTest : ModGore
{
	public void IdentifierTest() {
		Console.Write(updateType);
	}

#if COMPILE_ERROR
	public override bool DrawBehind(Gore gore) { return false; }
#endif
}
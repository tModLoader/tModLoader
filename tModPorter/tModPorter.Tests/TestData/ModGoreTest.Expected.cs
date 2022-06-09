using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

public class ModGoreTest : ModGore
{
	public void IdentifierTest() {
		Console.Write(UpdateType);
	}

	public override void OnSpawn(Gore gore, IEntitySource source) { /* Empty */ }

#if COMPILE_ERROR
	public override bool DrawBehind(Gore gore)/* tModPorter Note: Removed. Use GoreID.Sets.DrawBehind[Type] in SetStaticDefaults */ { return false; }
#endif
}
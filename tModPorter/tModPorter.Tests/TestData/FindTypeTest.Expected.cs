using Terraria.ModLoader;

public abstract class FindTypeTest : Mod
{
	void ViaMemberAccess(Mod mod) {
		int a = mod.Find<ModBuff>("BuffClass").Type;
		int b = mod.Find<ModDust>("DustClass").Type;
		int c = mod.Find<ModItem>("ItemClass").Type;
		int d = mod.Find<ModMount>("MountClass").Type;
		int e = mod.Find<ModNPC>("NPCClass").Type;
		int f = mod.Find<ModPrefix>("PrefixClass").Type;
		int g = mod.Find<ModProjectile>("ProjectileClass").Type;
		int h = mod.Find<ModTileEntity>("TileEntityClass").Type;
		int i = mod.Find<ModTile>("TileClass").Type;
		int j = mod.Find<ModWall>("WallClass").Type;

		// not-yet-implemented
		int k = mod.Find<ModGore>("GoreTextureOrClass").Type;
		int l = ModContent.Find<ModGore>("ModName/GoreTextureOrClass").Type;
#if COMPILE_ERROR
		int m = mod.GetGoreSlot("DoesNotStartWith'Gores/'String")/* tModPorter Note: Removed. Replacement is Mod.Find<ModGore>("NameWithout'Gores/'").Type */;
		int n = ModGore.GetGoreSlot("DoesNotStartWith'ModName/Gores/'String")/* tModPorter Note: Removed. Replacement is ModContent.Find<ModGore>("ModName/NameWithout'Gores/'").Type */;

		int o = ModContent.GoreType<GoreClass>();
#endif
		// instead-expect
#if COMPILE_ERROR
		int k = mod.GetGoreSlot("Gores/GoreTextureOrClass");
		int l = ModGore.GetGoreSlot("ModName/Gores/GoreTextureOrClass");
		int m = mod.GetGoreSlot("DoesNotStartWith'Gores/'String");
		int n = ModGore.GetGoreSlot("DoesNotStartWith'ModName/Gores/'String");

		int o = mod.GetGoreSlot<GoreClass>();
#endif
	}

	int ViaIdentifier() => Find<ModBuff>("BuffClass").Type;

	int? ViaConditionalAccess(Mod mod) => mod?.Find<ModBuff>("BuffClass").Type;
}
// not-yet-implemented
using Microsoft.Xna.Framework;
using Terraria;

#if COMPILE_ERROR
public class IEntitySourceTest
{
	void Method() {
		NPC.NewNPC(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, 1, 2, 3, 4, 5, 6, 7, 8, 9);

		Gore.NewGore(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4);
		Gore.NewGoreDirect(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4);
		Gore.NewGorePerfect(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4);

		Item.NewItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Rectangle(), 1, 2, false, 4, false, false); // rect
		Item.NewItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, 1, 2, 3, 4, 5, 6, false, 8, false, false); // full coords
		Item.NewItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos
		Item.NewItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos +width/height
		Item.NewItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4, false, 6, false, false); // vec2 both

		Player player = new Player();
		player.QuickSpawnItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, 1, 2);
		player.QuickSpawnItem(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Item(), 2);

		Projectile.NewProjectile(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8); // vec2 both
		Projectile.NewProjectile(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10); // full coords
		Projectile.NewProjectileDirect(/* tModPorter Suggestion: player/npc/projectile.GetSource_... */, new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8);
	}
}

public class ModTileWithEntitySource : ModTile
{
	public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 32, ItemID.StoneBlock);
	}

	public override bool Drop(int i, int j) {
		Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 64, 32, ItemID.StoneBlock);
	}
}

public class ModWallWithEntitySource : ModWall
{
	public override bool Drop(int i, int j) {
		Projectile.NewProjectile(new EntitySource_TileBreak(i, j), new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8);
	}
}
#endif
using Microsoft.Xna.Framework;
using Terraria;

public class IEntitySourceTest
{
	void Method() {
		NPC.NewNPC(1, 2, 3, 4, 5, 6, 7, 8, 9);

		Gore.NewGore(new Vector2(), new Vector2(), 3, 4);
		Gore.NewGoreDirect(new Vector2(), new Vector2(), 3, 4);
		Gore.NewGorePerfect(new Vector2(), new Vector2(), 3, 4);

		Item.NewItem(new Rectangle(), 1, 2, false, 4, false, false); // rect
		Item.NewItem(1, 2, 3, 4, 5, 6, false, 8, false, false); // full coords
		Item.NewItem(new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos
		Item.NewItem(new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos +width/height
		Item.NewItem(new Vector2(), new Vector2(), 3, 4, false, 6, false, false); // vec2 both

		Player player = new Player();
		player.QuickSpawnItem(1, 2);
		player.QuickSpawnItem(new Item(), 2);

		Projectile.NewProjectile(new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8); // vec2 both
		Projectile.NewProjectile(1, 2, 3, 4, 5, 6, 7, 8, 9, 10); // full coords
		Projectile.NewProjectileDirect(new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8);
	}
}

public class ModTileWithEntitySource : ModTile
{
	public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		Item.NewItem(i * 16, j * 16, 64, 32, ItemID.StoneBlock);
	}

	public override bool Drop(int i, int j) {
		Item.NewItem(i * 16, j * 16, 64, 32, ItemID.StoneBlock);
	}
}

public class ModWallWithEntitySource : ModWall
{
	public override bool Drop(int i, int j) {
		Projectile.NewProjectile(new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8);
	}
}
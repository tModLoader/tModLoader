using Microsoft.Xna.Framework;
using Terraria;

public class IEntitySourceTest
{
	void Method() {
		NPC.NewNPC(null, 1, 2, 3, 4, 5, 6, 7, 8, 9);

		Gore.NewGore(null, new Vector2(), new Vector2(), 3, 4);
		Gore.NewGoreDirect(null, new Vector2(), new Vector2(), 3, 4);
		Gore.NewGorePerfect(null, new Vector2(), new Vector2(), 3, 4);

		Item.NewItem(null, new Rectangle(), 1, 2, false, 4, false, false); // rect
		Item.NewItem(null, 1, 2, 3, 4, 5, 6, false, 8, false, false); // full coords
		Item.NewItem(null, new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos
		Item.NewItem(null, new Vector2(), 2, 3, 4, 5, false, 7, false, false); // vec2 pos +width/height
		Item.NewItem(null, new Vector2(), new Vector2(), 3, 4, false, 6, false, false); // vec2 both

		Player player = new Player();
		player.QuickSpawnItem(null, 1, 2);
		player.QuickSpawnItem(null, new Item(), 2);
		player.QuickSpawnClonedItem(null, new Item(), 2);

		Projectile.NewProjectile(null, new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8); // vec2 both
		Projectile.NewProjectile(null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10); // full coords
		Projectile.NewProjectileDirect(null, new Vector2(), new Vector2(), 3, 4, 5, 6, 7, 8);
	}
}

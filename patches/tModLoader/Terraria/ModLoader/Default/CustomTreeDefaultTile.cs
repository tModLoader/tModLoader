using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader.Default;

public class CustomTreeDefaultTile : CustomTreeTile
{
	public override string Name => Tree.Name + "Tile";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		AddMapEntry(new Color(151, 107, 75), Tree.GetLocalization("TileMapName", Tree.PrettyPrintName));
	}

	public override bool CreateDust(int x, int y, ref int type)
	{
		type = DustID.WoodFurniture;
		return true;
	}

	public override IEnumerable<Item> GetItemDrops(int x, int y)
	{
		int woodCount = 1;
		int acornCount = 0;

		if (TreeTileInfo.GetInfo(x, y).IsLeafy)
			acornCount++;

		int num = Player.FindClosest(new Vector2(x * 16, y * 16), 16, 16);
		int axe = Main.player[num].inventory[Main.player[num].selectedItem].axe;
		if (WorldGen.genRand.Next(100) < axe || Main.rand.Next(3) == 0)
			woodCount++;

		Item wood = new();
		wood.SetDefaults(ItemID.Wood);
		wood.stack = woodCount;

		if (acornCount == 0) {
			return new[] { wood };
		}
		else {
			Item acorn = new();
			acorn.SetDefaults(ItemID.Acorn);
			acorn.stack = acornCount;

			return new[] { wood, acorn };
		}
	}
}

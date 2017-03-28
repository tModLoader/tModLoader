using Terraria.ModLoader;

namespace ExampleMod.Items.Placeable
{
	class ScoreBoard : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Scoreboard";
			item.useStyle = 1;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 999;
			item.consumable = true;
			item.createTile = mod.TileType<Tiles.ScoreBoard>();
			item.width = 28;
			item.height = 28;
		}
	}
}

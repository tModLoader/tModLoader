using System;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace Terraria.ModLoader
{
	//todo: further documentation
	public class MapLegend
	{
		//change type of Lang.mapLegend to this class
		private LocalizedText[] legend;

		public MapLegend(int size)
		{
			legend = new LocalizedText[size];
		}

		public int Length
		{
			get
			{
				return legend.Length;
			}
		}

		internal void Resize(int newSize)
		{
			Array.Resize(ref legend, newSize);
		}

		public LocalizedText this[int i]
		{
			get
			{
				return legend[i];
			}
			set
			{
				legend[i] = value;
			}
		}
		//in Terraria.Main.DrawInventory replace
		//  Lang.mapLegend[MapHelper.TileToLookup(Main.recipe[Main.availableRecipe[num60]].requiredTile[num62], 0)]
		//  with Lang.mapLegend.FromType(Main.recipe[Main.availableRecipe[num60]].requiredTile[num62])
		//in Terraria.Main.DrawInfoAccs replace Lang.mapLegend[MapHelper.TileToLookup(Main.player[Main.myPlayer].bestOre, 0)]
		//  with Lang.mapLegend.FromType(Main.player[Main.myPlayer].bestOre)
		public LocalizedText FromType(int type)
		{
			return this[MapHelper.TileToLookup(type, 0)];
		}
		//in Terraria.Main.DrawMap replace text = Lang.mapLegend[type]; with
		//  text = Lang.mapLegend.FromTile(Main.Map[num91, num92], num91, num92);
		public LocalizedText FromTile(MapTile mapTile, int x, int y)
		{
			LocalizedText name = legend[mapTile.Type];
			if (MapLoader.nameFuncs.ContainsKey(mapTile.Type))
			{
				name = MapLoader.nameFuncs[mapTile.Type](name, x, y);
			}
			return name;
		}
	}
}

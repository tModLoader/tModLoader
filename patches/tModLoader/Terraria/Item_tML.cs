using Microsoft.Xna.Framework;
using System;
using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable
	{
		public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

		public static bool[] materialCache = new bool[3930];

		public TagCompound SerializeData()
		{
			return ItemIO.Save(this);
		}

		internal static void PopulateMaterialCache()
		{
			for (int i = 0; i < Recipe.numRecipes; i++)
			{
				int num = 0;
				while (Main.recipe[i].requiredItem[num].type > 0)
				{
					materialCache[Main.recipe[i].requiredItem[num].type] = true;
					num++;
				}
			}
			foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values)
			{
				foreach (var item in recipeGroup.ValidItems)
				{
					materialCache[item] = true;
				}
			}
			materialCache[71] = false;
			materialCache[72] = false;
			materialCache[73] = false;
			materialCache[74] = false;
		}

		public static int NewItem(Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
		{
			return Item.NewItem(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
		}

		public static int NewItem(Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
		{
			return Item.NewItem((int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
		}
	}
}
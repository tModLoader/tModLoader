using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable
	{
		public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

		public TagCompound SerializeData() => ItemIO.Save(this);

		/// <summary>
		/// The custom damage type of this Item. Assign to ModContent.DamageClassType<T>().
		/// </summary>
		public ModDamageClass DamageType { get; set; }

		internal static void PopulateMaterialCache() {
			for (int i = 0; i < Recipe.numRecipes; i++) {
				int num = 0;
				while (Main.recipe[i].requiredItem[num].type > 0) {
					ItemID.Sets.IsAMaterial[Main.recipe[i].requiredItem[num].type] = true;
					num++;
				}
			}

			foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values) {
				foreach (var item in recipeGroup.ValidItems) {
					ItemID.Sets.IsAMaterial[item] = true;
				}
			}

			ItemID.Sets.IsAMaterial[71] = false;
			ItemID.Sets.IsAMaterial[72] = false;
			ItemID.Sets.IsAMaterial[73] = false;
			ItemID.Sets.IsAMaterial[74] = false;
		}

		public static int NewItem(Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false) => 
			Item.NewItem(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
		public static int NewItem(Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false) => 
			NewItem((int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
	}
}
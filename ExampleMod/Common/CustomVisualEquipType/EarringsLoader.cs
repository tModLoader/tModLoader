using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.CustomVisualEquipType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomVisualEquipType/README.md

	/// <summary>
	/// EarringsLoader manages loading earring equipment textures for each item using this custom EquipType. 
	/// </summary>
	public class EarringsLoader : ModSystem
	{
		/// <summary> Maps an item type to it's corresponding earring equipment texture. </summary>
		internal static readonly Dictionary<int, Asset<Texture2D>> earringItemToTexture = new();

		public override void ResizeArrays() {
			// After all content is loaded, autoload earring equipment textures
			for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
				ModItem modItem = ItemLoader.GetItem(i);
				if (modItem.GetType().GetAttribute<AutoloadEquip_EarringAttribute>() != null) {
					AddEarringTexture(modItem.Type, $"{modItem.Texture}_Earrings");
				}
			}

			// Add an earring equipment texture to a vanilla item.
			AddEarringTexture(ItemID.AnglerEarring, "ExampleMod/Common/CustomVisualEquipType/AnglerEarring_Earrings");
		}

		public static void AddEarringTexture(int type, string texture) {
			var asset = ModContent.Request<Texture2D>(texture);
			earringItemToTexture[type] = asset;
		}
	}
}

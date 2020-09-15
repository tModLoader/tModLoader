using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	class ExampleNetworkedItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Networked Example Item");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.maxStack = 999;
			item.value = 1000; // Makes the item worth 1 gold.
			item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			//This creates a new ModRecipe, associated with the mod that this content piece comes from.
			CreateRecipe(1)
				.AddIngredient<ExampleItem>()
				//When you're done, call this to register the recipe.
				.Register();
		}
		
        public override void NetSend(BinaryWriter writer) {
			writer.Write("HI");
		}
 
        public override void NetRecieve(BinaryReader reader) 
		{
			var test = reader.ReadString();
			ExampleNetworkModWorld.table["test"] = test;
			
		}
 
	}
	class ExampleNetworkModWorld: ModWorld 
	{
		public static Dictionary<string, object> table;

		public override void Initialize() {
			base.Initialize();
			table = new Dictionary<string, object>();
		}
	}
}
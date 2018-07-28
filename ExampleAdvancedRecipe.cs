using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	//ModRecipe class is useful class that can help us adding custom recipe requirements other than materials
	//In this example my recipe will need specific npc nearby and Eye of Cthulhu defeated
	public class ExampleAdvancedRecipe : ModRecipe
	{
		public int NeededNPCType;

		//Range of npc search
		private const int Range = 480; //30 tiles -> 30 * 16

		//In constructor (necessary thing), i'll add argument where we will specify npc needed
		//Mod argument is required here, because ModRecipe itself need it
		//that's why we have ":base(mod)" here to satisfy constructor of ModRecipe
		public ExampleAdvancedRecipe(Mod mod, int NeededNPC) : base(mod)
		{
			NeededNPCType = NeededNPC;
		}

		//RecipeAvalible is our goal here, in here we check our custom requirements
		//Also, RecipeAvalible is called on client, so we can use here Main.LocalPlayer without problems
		public override bool RecipeAvailable()
		{
			//We will use this bool to determine is there npc nearby
			bool foundNPC = false;

			//First we check is EoC defeated, if no, we will return false, so recipe won't be avaliable
			if (!NPC.downedBoss1) return false;

			//If EoC is defated we will try find out is there is required npc nearby player
			foreach (NPC npc in Main.npc)
			{
				//If npc isn't active or isn't our needed type, we will skip iteration
				if (!npc.active && npc.type != NeededNPCType) continue;
				//Otherwise we will compare positions
				if(Vector2.Distance(Main.LocalPlayer.Center, npc.Center) <= Range)
				{
					foundNPC = true;
					break;
				}
			}

			//We don't need to check is there EoC defeated, because if it wouldn't, code would end earlier
			return foundNPC;
		}

		public override void OnCraft(Item item)
		{
			//And here a little surprise
			Main.LocalPlayer.AddBuff(BuffID.OnFire, 120);
		}
	}

	//Here's the item where we will add our recipe
	public class AdvancedRecipeItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Advanced Recipe Test Item");
		}
		public override string Texture
		{
			get
			{
				return "ExampleMod/Items/ExampleItem";
			}
		}

		public override void SetDefaults()
		{
			item.width = 26;
			item.height = 26;
		}

		//Applying our custom recipe type
		public override void AddRecipes()
		{
			ExampleAdvancedRecipe recipe = new ExampleAdvancedRecipe(mod, NPCID.Guide);
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

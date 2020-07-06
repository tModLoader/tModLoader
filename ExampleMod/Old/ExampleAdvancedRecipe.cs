using Microsoft.Xna.Framework;
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
		public ExampleAdvancedRecipe(Mod mod, int NeededNPC) : base(mod) {
			NeededNPCType = NeededNPC;
		}

		//RecipeAvailable is our goal here, in here we check our custom requirements
		//Also, RecipeAvailable is called on client, so we can use here Main.LocalPlayer without problems
		public override bool RecipeAvailable() {
			//We will use this bool to determine is there is needed npc nearby
			bool foundNPC = false;
			//First we check does EoC was defeated, if no, we will return false, so recipe won't be available
			if (!NPC.downedBoss1) {
				return false;
			}
			//If EoC was defeated we will try find out is there is required npc nearby player
			foreach (NPC npc in Main.npc) {
				//If npc isn't active or isn't our needed type, we will skip iteration
				if (!npc.active || npc.type != NeededNPCType) {
					continue;
				}
				//Otherwise we will compare positions
				if (Main.LocalPlayer.DistanceSQ(npc.Center) <= Range * Range) {
					foundNPC = true;
					break;
				}
			}
			//We don't need to check does EoC was defeated, because if it wasn't, code would return earlier
			return foundNPC;
		}

		//OnCraft is called when we create item
		public override void OnCraft(Item item) {
			//And here a little surprise
			Main.LocalPlayer.AddBuff(BuffID.OnFire, 120);
		}
	}

	//Here's the item where we will add our recipe
	public class AdvancedRecipeItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Advanced Recipe Test Item");
			Tooltip.SetDefault("You need help with creating this!");
		}

		public override string Texture => "ExampleMod/Items/ExampleItem";

		public override void SetDefaults() {
			item.width = 26;
			item.height = 26;
			item.rare = ItemRarityID.Blue;
		}

		//Using our custom recipe type
		public override void AddRecipes() {
			ExampleAdvancedRecipe recipe = new ExampleAdvancedRecipe(mod, NPCID.Guide);
			recipe.AddIngredient(ItemID.DirtBlock, 5);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}

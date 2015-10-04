using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public static class RecipeHelper
	{
		public static void AddBossRecipes(Mod mod)
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.SuspiciousLookingEye, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.WormFood, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.BloodySpine, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.Abeemination, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.GuideVoodooDoll);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalEye, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalWorm, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.MechanicalSkull, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "BossItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(ItemID.LihzahrdPowerCell, 20);
			recipe.AddRecipe();
		}

		public static void AddTestRecipes(Mod mod)
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.NebulaHelmet);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.NebulaBreastplate);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.NebulaLeggings);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.CelestialCuffs);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.ManaFlower);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.CharmofMyths);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.StarVeil);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.WormScarf);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.LunarFlareBook);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.LastPrism);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.WingsSolar);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.AnkhShield);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.FrostsparkBoots);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.SuperManaPotion, 99);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.SpectreHood);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.SpectreRobe);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.SpectrePants);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.BubbleGun);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.GoldenShower);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.RazorbladeTyphoon);
			recipe.AddRecipe();
		}
	}
}
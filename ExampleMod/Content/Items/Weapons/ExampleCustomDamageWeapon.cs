using ExampleMod.Content.DamageClasses;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleCustomDamageWeapon : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleSword"; // TODO: remove when sprite is made for this

		public override void SetDefaults() {
			item.DamageType = ModContent.GetInstance<ExampleDamageClass>(); // Makes our item use our custom damage type.
			item.width = 40;
			item.height = 40;
			item.useStyle = ItemUseStyleID.Swing;
			item.useTime = 30;
			item.useAnimation = 30;
			item.autoReuse = true;
			item.damage = 70;
			item.knockBack = 4;
			item.crit = 6;
			item.value = Item.buyPrice(gold: 1);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(20)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}

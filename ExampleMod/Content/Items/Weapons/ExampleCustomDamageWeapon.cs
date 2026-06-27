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
			Item.DamageType = ModContent.GetInstance<ExampleDamageClass>(); // Makes our item use our custom damage type.
			Item.width = 40;
			Item.height = 40;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.autoReuse = true;
			Item.damage = 70;
			Item.knockBack = 4;
			// Item.mana = 5; // ExampleDamageClass weapons should get magic and melee prefixes, but since this weapon doesn't have a mana cost all magic prefixes except Deranged can't be applied.
			Item.crit = 6;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(20)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}

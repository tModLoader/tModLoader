using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
    public class ExampleBoomerang : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Boomerang");
        }

        public override void SetDefaults()
        {
            item.melee = true; //Makes the item do melee damage
            item.useStyle = ItemUseStyleID.SwingThrow; //Makes the player swing
            item.noUseGraphic = true; //Turns off the boomerang from showing in the player's hand

            item.shoot = ModContent.ProjectileType<ExampleBoomerangProj>(); //Shoots the boomerang

            item.autoReuse = true; //Determines whether the player can hold left click to continue throwing or not, set to false to make them not able to
            item.useTime = 10; //Sets how fast the player can switch to another item after throwing, or how fast they can throw another boomerang after throwing the previous
            item.useAnimation = 10; //Sets how fast the player's hand finishes its throwing animation in ticks(60 ticks in a second)
            item.value = Item.buyPrice(0, 10, 0, 0); //Sets price(platinum, gold, silver, copper)
            item.rare = ItemRarityID.Blue; //Sets rarity
            item.damage = 100; //Sets damage
            item.knockBack = 4f; //Sets knockback
            item.width = 18; //Sets width of item
            item.height = 32; //Sets height of item
            item.UseSound = SoundID.Item1; //Sets the sound to the default thrown sound
            item.shootSpeed = 10f; //Sets how fast/far the boomerang is thrown, increase for more range and decrease for less
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[item.shoot] < 1; //Ensures there are no more than 1 boomerang out at once, you can set this to 2 or 3 if you want more boomerangs to be able to be thrown at once
        }
    }
}
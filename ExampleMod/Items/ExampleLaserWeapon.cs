using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
    public class ExampleLaserWeapon: ModItem
    {
        public override void SetDefaults()
        {
            item.name = "Example Laser Weapon";
            item.damage = 40;
            item.toolTip = "Shoot a laser beam that can eliminate everything...";
            item.noMelee = true;
            item.magic = true;
            item.channel = true;                            //Channel so that you can held the weapon
            item.mana = 5;
            item.rare = 5;
            item.width = 28;
            item.height = 30;
            item.useTime = 30;
            item.useStyle = 5;
            item.shootSpeed = 15;
            item.useAnimation = 30;                         //Speed is not important here
            item.shoot = mod.ProjectileType("ExampleLaser");
            item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "ExampleItem", 10);
            recipe.AddTile(null, "ExampleWorkbench");
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}

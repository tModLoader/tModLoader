﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	public class ExamplePie : ModItem
    {
        public override void SetStaticDefaults()
        {

            DisplayName.SetDefault("Example Pie");
            //Using references to language keys allow the tooltip to be easily translated
            //Listed below are some keys that you may find useful for making a food item
            //MinorStats, MediumStats, MajorStats, TipsyStats
            Tooltip.SetDefault("{$CommonItemTooltip.MinorStats}\n'Who knew examples could taste good'");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;

            //This is to show the correct frame in the inventory
            //The MaxValue argument is for the animation speed, we want it to be stuck on frame 1
            //Setting it to max value will cause it to take 414 days to reach the next frame
            //No one is going to have game open that long so this is fine
            //The second argument is the number of frames, which is 3
            //The first frame is the inventory texture, the second frame is the holding texture,
            //and the third frame is the placed texture
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

            //This allows you to change the color of the crumbs that are created when you eat.
            //The numbers are RGB (Red, Green, and Blue) values which range from 0 to 255.
            //Most foods have 3 crumb colors, but you can use more or less if you desire.
            //Depending on if you are making solid or liquid food switch out FoodParticleColors
            //with DrinkParticleColors. The difference is that food particles fly outwards
            //whereas drink particles fall straight down and are slightly transparent
            ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
                new Color(249, 230, 136),
                new Color(152, 93, 95),
                new Color(174, 192, 192)
                };

            ItemID.Sets.IsFood[Type] = true; //This allows it to be placed on a plate and held correctly
        }

        public override void SetDefaults()
        {

            Item.CloneDefaults(ItemID.ApplePie); //Makes this item use the same attributes as Apple Pie.

            Item.buffType = BuffID.WellFed; //If you desire, the buff type and duration can be changed.
            Item.buffTime = 36000; //To get time in seconds, multiply the time (in seconds) you want by 60

        }

        //If you want multiple buffs, you can apply them with this method
        public override bool ConsumeItem(Player player)
        {
            player.AddBuff(BuffID.SugarRush, 3600);
            return true;
        }

        //Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ExampleItem>()
                .AddIngredient(ItemID.Apple, 3)
                .AddTile<Tiles.Furniture.ExampleWorkbench>()
                .Register();
        }
    }
}
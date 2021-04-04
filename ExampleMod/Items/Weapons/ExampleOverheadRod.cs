using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using ExampleMod.Projectiles.Minions;
using ExampleMod.Buffs;
using ExampleMod.Tiles;
 
namespace ExampleMod.Items.Weapons
{
    public class ExampleOverheadRod : ModItem
    {
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Spirit Rod");
			Tooltip.SetDefault(@"Summons a spirit of light minion above your head.
Only one minion can be summoned at a time");
		}
        public override void SetDefaults() { 
            item.mana = 20; //how much mana it uses.
			item.damage = 98; //the damage of the weapon. 
            item.width = 40; //in pixels.   
            item.height = 40; //in pixels.    
            item.useTime = 25;   
            item.useAnimation = 25;    
            item.useStyle = 1; //swings over your head.
            item.noMelee = true; //its swinging does not damage enemies.
            item.knockBack = 12f;
            item.rare = ItemRarityID.Lime;  
            item.UseSound = SoundID.Item44; //44 is the summon minion sound.
            item.autoReuse = false; //does not have autoswing.  
            item.shoot = mod.ProjectileType("ExampleOverheadMinion");  
            item.summon = true;    
            item.value = Item.sellPrice(silver: 500); //sells for 500 silver.
            item.buffType = ModContent.BuffType<Buffs.ExampleOverheadMinionBuff>();
        }
        public override bool CanUseItem(Player player) {
            return !player.GetModPlayer<ExampleOverheadPlayer>().exampleOverheadMinion; //if overheadMinion variable is active (which is activated from the buff it gives you), can't use this item.
        }
 
        public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            player.AddBuff(item.buffType, 2); //adds the minion buff.
            Vector2 SPos = Main.screenPosition + new Vector2((float)Main.mouseX, (float)Main.mouseY);  //only 1 minion can be summoned at a time.
            position = SPos;
            for (int l = 0; l < Main.projectile.Length; l++) {                                                                  
                Projectile proj = Main.projectile[l];
                if (proj.active && proj.type == item.shoot && proj.owner == player.whoAmI) {
                    proj.active = false;
                }
            }
            return true;
        }
        public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod); //adds a recipe
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>(), 100);
			recipe.AddTile(ModContent.TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
    }
}
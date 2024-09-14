using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Liquids
{
	public class ExampleLiquid : ModLiquid
	{
		public static bool[] NpcTurnsIntoKingsSlimeOnContact = NPCID.Sets.Factory.CreateBoolSet();
		public static bool[] ItemTurnIntoSlimeOnContact = ItemID.Sets.Factory.CreateBoolSet();
		public static bool[] ProjectileExplodeOnContact = ProjectileID.Sets.Factory.CreateBoolSet();

		public override void SetStaticDefaults() {
			NpcTurnsIntoKingsSlimeOnContact[NPCID.BlueSlime] = true;
			ItemTurnIntoSlimeOnContact[ItemID.Gel] = true;
			ItemTurnIntoSlimeOnContact[ItemID.PinkGel] = true;
			ProjectileExplodeOnContact[ProjectileID.WaterBolt] = true;
			ProjectileExplodeOnContact[ProjectileID.BallofFire] = true;

			WaterfallLength = 16;
			DefaultOpacity = 0.6f;
			AddMapEntry(Color.Purple, Language.GetText("Mods.ExampleMod.Liquids.ExampleLiquid.MapEntry"));
		}

		public override void PreUpdate(int x, int y) {
			Tile tile = Main.tile[x, y];

			if (y < 200 && tile.LiquidAmount > 0) {
				byte b = 2;
				if (tile.LiquidAmount < b)
					b = tile.LiquidAmount;

				tile.LiquidAmount -= b;
			}
		}

		/// <summary>
		/// Various examples of what you can do with entity that collide with your liquid
		/// </summary>
		/// <param name="entity">Player/Item/Projectile/NPC</param>
		/// <param name="collisionType">Enter/Stay/Exit</param>
		public override void OnEntityCollision(Entity entity, CollisionType collisionType) {
			// We do a switch to do pattern matching for each type of entity we need to do something with
			switch (entity) {
				case Player player when collisionType == CollisionType.Enter: {
					// We make it so player get the cursed debuff when it enter, stay or exit the liquid
					player.AddBuff(BuffID.Cursed, 1800);
					break;
					}
				case Player player when collisionType == CollisionType.Exit: {
					CombatText.NewText(player.getRect(), Color.White, "Let that sink out!", true);
					return;
				}
				// We check if the item can turn into a slime and the item is entering the liquid
				case Item item when ItemTurnIntoSlimeOnContact[item.type] && collisionType == CollisionType.Enter: {
					// Then delete the item
					item.TurnToAir(true);
					// Then Spawn a king slime where the gel was
					NPC.NewNPC(new AEntitySource_Tile((int)(item.position.X / 16), (int)(item.position.Y / 16), ""),
						(int)item.position.X, (int)item.position.Y, NPCID.BlueSlime);
					// Then play the roar sound to accompany the new kind slime spawn
					SoundEngine.PlaySound(SoundID.Roar);
					
					// And then return to exit out the function
					return;
				}
				case NPC npc when NpcTurnsIntoKingsSlimeOnContact[npc.type]: {
					// We check if the npc is a slime and is entering the liquid
					if (collisionType == CollisionType.Enter) {
						// Then spawn a bunch of smoke dust for visual effect 
						for (int i = 0; i < 10; i++) {
							int num7 = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (float)(npc.height / 2) - 8f), npc.width + 12, 24, DustID.Smoke);
							Main.dust[num7].velocity.Y -= 4f;
							Main.dust[num7].velocity.X *= 2.5f;
							Main.dust[num7].scale *= 0.8f;
							Main.dust[num7].alpha = 100;
							Main.dust[num7].noGravity = true;
						}
						// Then transform the NPC into king slime
						npc.SetDefaults(NPCID.KingSlime);
						// Then play the roar sound to accompany the new kind slime spawn
						SoundEngine.PlaySound(SoundID.Roar, npc.position);
						// And then return to exit out the functionm
						
					}
					return;
				}
				case Projectile projectile when ProjectileExplodeOnContact[projectile.type] && collisionType == CollisionType.Enter:
					projectile.Kill();
					for (int i = 0; i < 50; i++) {
						int num7 = Dust.NewDust(new Vector2(entity.position.X - 6f, entity.position.Y + (float)(entity.height / 2) - 8f), entity.width + 12, 48, DustID.Smoke);
						Main.dust[num7].velocity.Y -= Main.rand.Next(-4, 4);
						Main.dust[num7].velocity.X *= 2.5f;
						Main.dust[num7].scale *= 1.5f;
						Main.dust[num7].alpha = 100;
						Main.dust[num7].noGravity = true;
					}
					return;
			}

			if (collisionType == CollisionType.Exit) {
				CombatText.NewText(entity.Hitbox, Color.White, "Example of exit collision!", true);
			}

			// If none of the condition in the switch case are meant, spawn a bunch of particle
			if (collisionType == CollisionType.Enter) {
				
				for (int i = 0; i < 10; i++) {
					int num7 = Dust.NewDust(new Vector2(entity.position.X - 6f, entity.position.Y + (float)(entity.height / 2) - 8f), entity.width + 12, 24, ModContent.DustType<ExampleAdvancedDust>());
					Main.dust[num7].velocity.Y -= 4f;
					Main.dust[num7].velocity.X *= 2.5f;
					Main.dust[num7].scale *= 0.8f;
					Main.dust[num7].alpha = 100;
					Main.dust[num7].noGravity = true;
				}
				// Finally, we play the classic splash sound
				SoundEngine.PlaySound(SoundID.Splash, entity.position);
			}
		}

		public override void Merge(int otherLiquid, bool[] liquidNearby, ref int liquidMergeTileType, ref int liquidMergeType) {
			liquidMergeTileType = 0;

			if (otherLiquid == ModContent.GetInstance<ExampleLiquid2>().Type) {
				liquidMergeTileType = TileID.LunarOre;
			}
			else if (otherLiquid == LiquidID.Shimmer) {
				liquidMergeTileType = TileID.ShimmerBlock;
			}

			liquidMergeType = 4;
		}
	}
}

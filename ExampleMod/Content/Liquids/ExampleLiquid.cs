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
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
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
		/// An example of what you can do when a player enter your custom liquids, in this case it give the cursed debuff to the player
		/// </summary>
		/// <param name="npc"></param>
		public override void OnPlayerEnterCollide(Player player) {
			player.AddBuff(BuffID.Cursed, 1800);
			for (int i = 0; i < 10; i++) {
				int num7 = Dust.NewDust(new Vector2(player.position.X - 6f, player.position.Y + (float)(player.height / 2) - 8f), player.width + 12, 24, DustID.Smoke);
				Main.dust[num7].velocity.Y -= 4f;
				Main.dust[num7].velocity.X *= 2.5f;
				Main.dust[num7].scale *= 0.8f;
				Main.dust[num7].alpha = 100;
				Main.dust[num7].noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.ForceRoar, player.position);
		}

		/// <summary>
		/// An example of what you can do when a NPC enter your custom liquids, in this case it can transform a slime into a king slime or just spit out some particle
		/// </summary>
		/// <param name="npc"></param>
		public override void OnNPCEnterCollide(NPC npc) {
			if (npc.type is NPCID.BlueSlime) {
				for (int i = 0; i < 10; i++) {
					int num7 = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (float)(npc.height / 2) - 8f), npc.width + 12, 24, DustID.Smoke);
					Main.dust[num7].velocity.Y -= 4f;
					Main.dust[num7].velocity.X *= 2.5f;
					Main.dust[num7].scale *= 0.8f;
					Main.dust[num7].alpha = 100;
					Main.dust[num7].noGravity = true;
					npc.SetDefaults(NPCID.KingSlime);
				}
				SoundEngine.PlaySound(SoundID.Roar, npc.position);
				return;
			}
			SoundEngine.PlaySound(SoundID.Splash, npc.position);
		}

		/// <summary>
		/// An example of what you can do when an items enter your custom liquids, in this case it can transform a dirt block into an Example item or just spit out some particle
		/// </summary>
		/// <param name="item"></param>
		public override void OnItemEnterCollide(Item item) {
			switch (item.type)
			{
				case ItemID.DirtBlock:
				{
					for (int i = 0; i < 10; i++) {
						int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.FireworkFountain_Red);
						Main.dust[num7].velocity.Y -= 4f;
						Main.dust[num7].velocity.X *= 2.5f;
						Main.dust[num7].scale *= 0.8f;
						Main.dust[num7].alpha = 100;
						Main.dust[num7].noGravity = true;
					}
				

					SoundEngine.PlaySound(SoundID.DD2_GoblinBomb, item.position);

					item.SetDefaults(ModContent.ItemType<ExampleItem>());
					break;
				}
				case ItemID.Gel:
					for (int i = 0; i < 20; i++) {
						int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.Bee);
						Main.dust[num7].velocity.Y -= 4f;
						Main.dust[num7].velocity.X *= 2.5f;
						Main.dust[num7].scale *= 0.8f;
						Main.dust[num7].alpha = 100;
						Main.dust[num7].noGravity = true;
					}
					item.TurnToAir();
					NPC.NewNPC(new AEntitySource_Tile((int)(item.position.X / 16), (int)(item.position.Y / 16), ""),
						(int)item.position.X, (int)item.position.Y, NPCID.KingSlime);
					SoundEngine.PlaySound(SoundID.Splash, item.position);
					break;
				default:
				{
					for (int i = 0; i < 20; i++) {
						int num7 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, DustID.Bee);
						Main.dust[num7].velocity.Y -= 4f;
						Main.dust[num7].velocity.X *= 2.5f;
						Main.dust[num7].scale *= 0.8f;
						Main.dust[num7].alpha = 100;
						Main.dust[num7].noGravity = true;
					}
					item.TurnToAir();
					NPC.NewNPC(new AEntitySource_Tile((int)(item.position.X / 16), (int)(item.position.Y / 16), ""),
						(int)item.position.X, (int)item.position.Y, NPCID.KingSlime);
					SoundEngine.PlaySound(SoundID.Splash, item.position);
					break;
				}
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

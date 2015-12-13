using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.NPCs.PuritySpirit;

namespace ExampleMod
{
	public class ExamplePlayer : ModPlayer
	{
		private const int saveVersion = 0;
		public int score = 0;
		public bool eFlames = false;
		public bool elementShield = false;
		public int elementShields = 0;
		private int elementShieldTimer = 0;
		public int elementShieldPos = 0;
		public int voidMonolith = 0;
		public int heroLives = 0;
		public int constantDamage = 0;
		public float percentDamage = 0f;
		public float defenseEffect = -1f;

		public override void ResetEffects()
		{
			eFlames = false;
			elementShield = false;
			if (voidMonolith > 0)
			{
				voidMonolith--; //this is a very bad hack until I create ModWorld
			}
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			writer.Write(score);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			score = reader.ReadInt32();
		}

		public override void SetupStartInventory(IList<Item> items)
		{
			Item item = new Item();
			item.SetDefaults(mod.ItemType("ExampleItem"));
			item.stack = 5;
			items.Add(item);
		}

		public override void UpdateBiomeVisuals()
		{
			bool usePurity = NPC.AnyNPCs(mod.NPCType("PuritySpirit"));
			player.ManageSpecialBiomeVisuals("ExampleMod:PuritySpirit", usePurity);
			bool useVoidMonolith = voidMonolith > 0 && !usePurity && !NPC.AnyNPCs(NPCID.MoonLordCore);
			player.ManageSpecialBiomeVisuals("ExampleMod:MonolithVoid", useVoidMonolith, player.Center);
		}

		public override void UpdateBadLifeRegen()
		{
			if (eFlames)
			{
				if (player.lifeRegen > 0)
				{
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				player.lifeRegen -= 16;
			}
		}

		public override void PreUpdateBuffs()
		{
			if (heroLives > 0)
			{
				bool flag = false;
				for (int k = 0; k < 200; k++)
				{
					NPC npc = Main.npc[k];
					if (npc.active && npc.type == mod.NPCType("PuritySpirit"))
					{
						flag = true;
						PuritySpiritTeleport(npc);
						break;
					}
				}
				if (!flag)
				{
					heroLives = 0;
				}
				if (heroLives == 1)
				{
					player.AddBuff(mod.BuffType("HeroOne"), 2);
				}
				else if (heroLives == 2)
				{
					player.AddBuff(mod.BuffType("HeroTwo"), 2);
				}
				else if (heroLives == 3)
				{
					player.AddBuff(mod.BuffType("HeroThree"), 3);
				}
			}
		}

		private void PuritySpiritTeleport(NPC npc)
		{
			int halfWidth = PuritySpirit.arenaWidth / 2;
			int halfHeight = PuritySpirit.arenaHeight / 2;
			Vector2 newPosition = player.position;
			if (player.position.X <= npc.Center.X - halfWidth)
			{
				newPosition.X = npc.Center.X + halfWidth - player.width - 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height))
				{
					newPosition.X -= 16f;
				}
			}
			else if (player.position.X + player.width >= npc.Center.X + halfWidth)
			{
				newPosition.X = npc.Center.X - halfWidth + 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height))
				{
					newPosition.X += 16f;
				}
			}
			else if (player.position.Y <= npc.Center.Y - halfHeight)
			{
				newPosition.Y = npc.Center.Y + halfHeight - player.height - 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height))
				{
					newPosition.Y -= 16f;
				}
			}
			else if (player.position.Y + player.height >= npc.Center.Y + halfHeight)
			{
				newPosition.Y = npc.Center.Y - halfHeight + 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height))
				{
					newPosition.Y += 16f;
				}
			}
			if (newPosition != player.position)
			{
				player.Teleport(newPosition, 1, 0);
				NetMessage.SendData(65, -1, -1, "", 0, player.whoAmI, newPosition.X, newPosition.Y, 1, 0, 0);
				PuritySpiritDebuff();
			}
		}

		private void PuritySpiritDebuff()
		{
			if (Main.rand.Next(2) == 0)
			{
				switch (Main.rand.Next(5))
				{
					case 0:
						player.AddBuff(BuffID.Darkness, 1800);
						break;
					case 1:
						player.AddBuff(BuffID.Cursed, 900);
						break;
					case 2:
						player.AddBuff(BuffID.Confused, 1800);
						break;
					case 3:
						player.AddBuff(BuffID.Slow, 1800);
						break;
					case 4:
						player.AddBuff(BuffID.Silenced, 900);
						break;
				}
			}
			else
			{
			}
		}

		public override void PostUpdateEquips()
		{
			if (elementShield)
			{
				if (elementShields > 0)
				{
					elementShieldTimer--;
					if (elementShieldTimer < 0)
					{
						elementShields--;
						elementShieldTimer = 600;
					}
				}
			}
			else
			{
				elementShields = 0;
				elementShieldTimer = 0;
			}
			elementShieldPos++;
			elementShieldPos %= 300;
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref string deathText)
		{
			if (constantDamage > 0 || percentDamage > 0f)
			{
				int damageFromPercent = (int)(player.statLifeMax2 * percentDamage);
				damage = Math.Max(constantDamage, damageFromPercent);
				customDamage = true;
			}
			else if (defenseEffect >= 0f)
			{
				if (Main.expertMode)
				{
					defenseEffect *= 1.5f;
				}
				damage -= (int)(player.statDefense * defenseEffect);
				if (damage < 0)
				{
					damage = 1;
				}
				customDamage = true;
			}
			constantDamage = 0;
			percentDamage = 0f;
			defenseEffect = -1f;
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref deathText);
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			if (elementShield && damage > 1.0)
			{
				if (elementShields < 6)
				{
					int k;
					bool flag = false;
					for (k = 3; k < 8 + player.extraAccessorySlots; k++)
					{
						if (player.armor[k].type == mod.ItemType("SixColorShield"))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						Projectile.NewProjectile(player.Center.X, player.Center.Y, 0f, 0f, mod.ProjectileType("ElementShield"), player.GetWeaponDamage(player.armor[k]), player.GetWeaponKnockback(player.armor[k], 2f), player.whoAmI, elementShields++);
					}
				}
				elementShieldTimer = 600;
			}
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref string deathText)
		{
			if (heroLives > 0)
			{
				heroLives--;
				if (heroLives > 0)
				{
					player.statLife = player.statLifeMax2;
					player.HealEffect(player.statLifeMax2);
					player.immune = true;
					player.immuneTime = player.longInvince ? 180 : 120;
					for (int k = 0; k < player.hurtCooldowns.Length; k++)
					{
						player.hurtCooldowns[k] = player.longInvince ? 180 : 120;
					}
					Main.PlaySound(2, (int)player.position.X, (int)player.position.Y, 29);
					return false;
				}
			}
			return true;
		}

		public override void AnglerQuestReward(float quality, List<Item> rewardItems)
		{
			if (voidMonolith > 0)
			{
				Item sticky = new Item();
				sticky.SetDefaults(ItemID.StickyDynamite);
				sticky.stack = 4;
				rewardItems.Add(sticky);
			}
			foreach (Item item in rewardItems)
			{
				if (item.type == ItemID.GoldCoin)
				{
					int stack = item.stack;
					item.SetDefaults(ItemID.PlatinumCoin);
					item.stack = stack;
				}
			}
		}

		public override void CatchFish(Item fishingRod, Item bait, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
		{
			if (junk)
			{
				return;
			}
			if (player.HasBuff(BuffID.TwinEyesMinion) > -1 && liquidType == 0 && Main.rand.Next(3) == 0)
			{
				caughtType = mod.ItemType("SparklingSphere");
			}
			if (player.gravDir == -1f && questFish == mod.ItemType("ExampleQuestFish") && Main.rand.Next(2) == 0)
			{
				caughtType = mod.ItemType("ExampleQuestFish");
			}
		}

		public override void GetFishingLevel(Item fishingRod, Item bait, ref int fishingLevel)
		{
			if (player.HasBuff(mod.BuffType("CarMount")) > -1)
			{
				fishingLevel = (int)(fishingLevel * 1.1f);
			}
		}

		public override void GetDyeTraderReward(List<int> dyeItemIDsPool)
		{
			if (player.HasBuff(BuffID.UFOMount) > -1)
			{
				dyeItemIDsPool.Clear();
				dyeItemIDsPool.Add(ItemID.MartianArmorDye);
			}
		}
	}
}

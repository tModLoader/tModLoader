using ExampleMod.Buffs;
using ExampleMod.Dusts;
using ExampleMod.Items;
using ExampleMod.Items.Abomination;
using ExampleMod.NPCs;
using ExampleMod.NPCs.PuritySpirit;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod
{
	// ModPlayer classes provide a way to attach data to Players and act on that data. ExamplePlayer has a lot of functionality related to 
	// several effects and items in ExampleMod. See SimpleModPlayer for a very simple example of how ModPlayer classes work.
	public class ExamplePlayer : ModPlayer
	{
		public int score;
		public bool eFlames;
		public bool elementShield;
		public int elementShields;
		private int elementShieldTimer;
		public int elementShieldPos;
		public int lockTime;
		public bool voidMonolith = false;
		public int heroLives;
		public int reviveTime;
		public int constantDamage;
		public float percentDamage;
		public float defenseEffect = -1f;
		public bool badHeal;
		public int healHurt;
		public bool nullified;
		public int purityDebuffCooldown;
		public bool purityMinion;
		public bool exampleShield;
		public bool infinity;
		public bool strongBeesUpgrade;
		public bool manaHeart;
		public int manaHeartCounter;
		public bool nonStopParty; // The value of this bool can't be calculated by other clients automatically since it is set in ExampleUI. This bool is synced by SendClientChanges.
		public bool examplePersonGiftReceived;

		public override void ResetEffects() {
			eFlames = false;
			elementShield = false;
			constantDamage = 0;
			percentDamage = 0f;
			defenseEffect = -1f;
			badHeal = false;
			healHurt = 0;
			nullified = false;
			purityMinion = false;
			exampleShield = false;
			infinity = false;
			strongBeesUpgrade = false;
			if (!manaHeart) {
				manaHeartCounter = 0;
			}
			manaHeart = false;
		}

		public override void OnEnterWorld(Player player) {
			// We can refresh UI using OnEnterWorld. OnEnterWorld happens after Load, so nonStopParty is the correct value.
			GetInstance<ExampleMod>().ExampleUI.ExampleButton.HoverText = "SendClientChanges Example: Non-Stop Party " + (nonStopParty ? "On" : "Off");
		}

		// In MP, other clients need accurate information about your player or else bugs happen.
		// clientClone, SyncPlayer, and SendClientChanges, ensure that information is correct.
		// We only need to do this for data that is changed by code not executed by all clients, 
		// or data that needs to be shared while joining a world.
		// For example, examplePet doesn't need to be synced because all clients know that the player is wearing the ExamplePet item in an equipment slot. 
		// The examplePet bool is set for that player on every clients computer independently (via the Buff.Update), keeping that data in sync.
		// ExampleLifeFruits, however might be out of sync. For example, when joining a server, we need to share the exampleLifeFruits variable with all other clients.
		// In addition, in ExampleUI we have a button that toggles "Non-Stop Party". We need to sync this whenever it changes.
		public override void clientClone(ModPlayer clientClone) {
			ExamplePlayer clone = clientClone as ExamplePlayer;
			// Here we would make a backup clone of values that are only correct on the local players Player instance.
			// Some examples would be RPG stats from a GUI, Hotkey states, and Extra Item Slots
			clone.nonStopParty = nonStopParty;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)ExampleModMessageType.ExamplePlayerSyncPlayer);
			packet.Write((byte)player.whoAmI);
			packet.Write(nonStopParty); // While we sync nonStopParty in SendClientChanges, we still need to send it here as well so newly joining players will receive the correct value.
			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			// Here we would sync something like an RPG stat whenever the player changes it.
			ExamplePlayer clone = clientPlayer as ExamplePlayer;
			if (clone.nonStopParty != nonStopParty) {
				// Send a Mod Packet with the changes.
				var packet = mod.GetPacket();
				packet.Write((byte)ExampleModMessageType.NonStopPartyChanged);
				packet.Write((byte)player.whoAmI);
				packet.Write(nonStopParty);
				packet.Send();
			}
		}

		public override void UpdateDead() {
			eFlames = false;
			badHeal = false;
		}

		public override TagCompound Save() {
			// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
			return new TagCompound {
				// {"somethingelse", somethingelse}, // To save more data, add additional lines
				{"score", score},
				{"nonStopParty", nonStopParty},
				{nameof(examplePersonGiftReceived), examplePersonGiftReceived},
			};
			//note that C# 6.0 supports indexer initializers
			//return new TagCompound {
			//	["score"] = score
			//};
		}

		public override void Load(TagCompound tag) {
			score = tag.GetInt("score");
			// nonStopParty was added after the initial ExampleMod release. Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound#mod-version-updates for information about how to handle version updates in your mod without messing up current users of your mod.
			nonStopParty = tag.GetBool("nonStopParty");
			examplePersonGiftReceived = tag.GetBool(nameof(examplePersonGiftReceived));
		}

		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath) {
			Item item = new Item();
			item.SetDefaults(ItemType<ExampleItem>());
			item.stack = 5;
			items.Add(item);
		}

		public override void UpdateBiomeVisuals() {
			bool usePurity = NPC.AnyNPCs(NPCType<PuritySpirit>());
			player.ManageSpecialBiomeVisuals("ExampleMod:PuritySpirit", usePurity);
			bool useVoidMonolith = voidMonolith && !usePurity && !NPC.AnyNPCs(NPCID.MoonLordCore);
			player.ManageSpecialBiomeVisuals("ExampleMod:MonolithVoid", useVoidMonolith, player.Center);
		}

		public override void UpdateBadLifeRegen() {
			if (eFlames) {
				// These lines zero out any positive lifeRegen. This is expected for all bad life regeneration effects.
				if (player.lifeRegen > 0) {
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				// lifeRegen is measured in 1/2 life per second. Therefore, this effect causes 8 life lost per second.
				player.lifeRegen -= 16;
			}
			if (healHurt > 0) {
				if (player.lifeRegen > 0) {
					player.lifeRegen = 0;
				}
				player.lifeRegenTime = 0;
				player.lifeRegen -= 120 * healHurt;
			}
		}

		public override void PreUpdateBuffs() {
			if (heroLives > 0) {
				bool flag = false;
				for (int k = 0; k < 200; k++) {
					NPC npc = Main.npc[k];
					if (npc.active && npc.type == NPCType<PuritySpirit>()) {
						flag = true;
						PuritySpiritTeleport(npc);
						break;
					}
				}
				if (!flag) {
					heroLives = 0;
				}
				if (heroLives == 1) {
					player.AddBuff(BuffType<Buffs.HeroOne>(), 2);
				}
				else if (heroLives == 2) {
					player.AddBuff(BuffType<Buffs.HeroTwo>(), 2);
				}
				else if (heroLives == 3) {
					player.AddBuff(BuffType<Buffs.HeroThree>(), 3);
				}
			}
			if (purityDebuffCooldown > 0) {
				purityDebuffCooldown--;
			}
		}

		private void PuritySpiritTeleport(NPC npc) {
			int halfWidth = PuritySpirit.arenaWidth / 2;
			int halfHeight = PuritySpirit.arenaHeight / 2;
			Vector2 newPosition = player.position;
			if (player.position.X <= npc.Center.X - halfWidth) {
				newPosition.X = npc.Center.X + halfWidth - player.width - 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height)) {
					newPosition.X -= 16f;
				}
			}
			else if (player.position.X + player.width >= npc.Center.X + halfWidth) {
				newPosition.X = npc.Center.X - halfWidth + 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height)) {
					newPosition.X += 16f;
				}
			}
			else if (player.position.Y <= npc.Center.Y - halfHeight) {
				newPosition.Y = npc.Center.Y + halfHeight - player.height - 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height)) {
					newPosition.Y -= 16f;
				}
			}
			else if (player.position.Y + player.height >= npc.Center.Y + halfHeight) {
				newPosition.Y = npc.Center.Y - halfHeight + 1;
				while (Collision.SolidCollision(newPosition, player.width, player.height)) {
					newPosition.Y += 16f;
				}
			}
			if (newPosition != player.position) {
				player.Teleport(newPosition, 1, 0);
				NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, newPosition.X, newPosition.Y, 1, 0, 0);
				PuritySpiritDebuff();
			}
		}

		public void PuritySpiritDebuff() {
			bool flag = true;
			if (Main.rand.NextBool()) {
				flag = false;
				for (int k = 0; k < 2; k++) {
					int buffType;
					int buffTime;
					switch (Main.rand.Next(5)) {
						case 0:
							buffType = BuffID.Darkness;
							buffTime = 1800;
							break;
						case 1:
							buffType = BuffID.Cursed;
							buffTime = 900;
							break;
						case 2:
							buffType = BuffID.Confused;
							buffTime = 1800;
							break;
						case 3:
							buffType = BuffID.Slow;
							buffTime = 1800;
							break;
						default:
							buffType = BuffID.Silenced;
							buffTime = 900;
							break;
					}
					if (!player.buffImmune[buffType]) {
						player.AddBuff(buffType, buffTime);
						return;
					}
				}
			}
			if (flag || Main.expertMode || Main.rand.NextBool()) {
				player.AddBuff(BuffType<Buffs.Undead>(), 1800, false);
			}
			for (int k = 0; k < 25; k++) {
				Dust.NewDust(player.position, player.width, player.height, DustType<Dusts.Negative>(), 0f, -1f, 0, default(Color), 2f);
			}
		}

		public override void PostUpdateBuffs() {
			if (nullified) {
				Nullify();
			}
		}

		public override void PostUpdateEquips() {
			if (nullified) {
				Nullify();
			}
			if (elementShield) {
				if (elementShields > 0) {
					elementShieldTimer--;
					if (elementShieldTimer < 0) {
						elementShields--;
						elementShieldTimer = 600;
					}
				}
			}
			else {
				elementShields = 0;
				elementShieldTimer = 0;
			}
			elementShieldPos++;
			elementShieldPos %= 300;
		}

		public override void PostUpdateMiscEffects() {
			if (lockTime > 0) {
				lockTime--;
			}
			if (reviveTime > 0) {
				reviveTime--;
			}
		}

		public override void FrameEffects() {
			if (nullified) {
				Nullify();
			}
		}

		private void Nullify() {
			player.ResetEffects();
			player.head = -1;
			player.body = -1;
			player.legs = -1;
			player.handon = -1;
			player.handoff = -1;
			player.back = -1;
			player.front = -1;
			player.shoe = -1;
			player.waist = -1;
			player.shield = -1;
			player.neck = -1;
			player.face = -1;
			player.balloon = -1;
			nullified = true;
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			if (constantDamage > 0 || percentDamage > 0f) {
				int damageFromPercent = (int)(player.statLifeMax2 * percentDamage);
				damage = Math.Max(constantDamage, damageFromPercent);
				customDamage = true;
			}
			else if (defenseEffect >= 0f) {
				if (Main.expertMode) {
					defenseEffect *= 1.5f;
				}
				damage -= (int)(player.statDefense * defenseEffect);
				if (damage < 0) {
					damage = 1;
				}
				customDamage = true;
			}
			constantDamage = 0;
			percentDamage = 0f;
			defenseEffect = -1f;
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (elementShield && damage > 1.0) {
				if (elementShields < 6) {
					int k;
					bool flag = false;

					for (k = 3; k < 8 + player.extraAccessorySlots; k++) {
						if (player.armor[k].type == ItemType<SixColorShield>()) {
							flag = true;
							break;
						}
					}

					if (flag) {
						Projectile.NewProjectile(player.Center.X, player.Center.Y, 0f, 0f, ProjectileType<ElementShield>(), player.GetWeaponDamage(player.armor[k]), player.GetWeaponKnockback(player.armor[k], 2f), player.whoAmI, elementShields++);
					}
				}

				elementShieldTimer = 600;
			}

			if (heroLives > 0) {
				for (int k = 0; k < 200; k++) {
					NPC npc = Main.npc[k];

					if (npc.active && npc.type == NPCType<PuritySpirit>()) {
						PuritySpirit modNPC = (PuritySpirit)npc.modNPC;

						if (modNPC.attack >= 0) {
							double proportion = damage / player.statLifeMax2;

							if (proportion > 1.0) {
								proportion = 1.0;
							}

							modNPC.attackWeights[modNPC.attack] += (int)(proportion * 400);

							if (modNPC.attackWeights[modNPC.attack] > PuritySpirit.maxAttackWeight) {
								modNPC.attackWeights[modNPC.attack] = PuritySpirit.maxAttackWeight;
							}

							if (nullified && modNPC.attack != 2) {
								modNPC.attackWeights[2] += (int)(proportion * 200);
								if (modNPC.attackWeights[2] > PuritySpirit.maxAttackWeight) {
									modNPC.attackWeights[2] = PuritySpirit.maxAttackWeight;
								}
							}
						}
					}
				}
			}

			if (player.mount._mountSpecificData is Mounts.Car.CarSpecificData balloons) {
				if (balloons.count > 0) {
					balloons.count--;

					SoundEngine.PlaySound(SoundID.Item38, player.position);
				}
			}
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			if (heroLives > 0) {
				heroLives--;

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = mod.GetPacket();

					packet.Write((byte)ExampleModMessageType.HeroLives);
					packet.Write(player.whoAmI);
					packet.Write(heroLives);

					packet.Send();
				}

				if (heroLives > 0) {
					player.statLife = player.statLifeMax2;
					player.HealEffect(player.statLifeMax2);
					player.immune = true;
					player.immuneTime = player.longInvince ? 180 : 120;

					for (int k = 0; k < player.hurtCooldowns.Length; k++) {
						player.hurtCooldowns[k] = player.longInvince ? 180 : 120;
					}

					SoundEngine.PlaySound(SoundID.Item29, player.position);

					reviveTime = 60;

					return false;
				}
			}

			if (healHurt > 0 && damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8) {
				damageSource = PlayerDeathReason.ByCustomReason(" was dissolved by holy powers");
			}

			return true;
		}

		public override float UseTimeMultiplier(Item item) {
			return exampleShield ? 1.5f : 1f;
		}

		public override void OnConsumeMana(Item item, int manaConsumed) {
			if (!manaHeart) {
				return;
			}

			manaHeartCounter += manaConsumed;

			if (manaHeartCounter < 200) {
				return;
			}

			if (Main.netMode != NetmodeID.Server) {
				SoundEngine.PlaySound(SoundID.Item4, player.position);

				player.statLife += 20;

				if (Main.myPlayer == player.whoAmI) {
					player.HealEffect(20, true);
				}

				if (player.statLife > player.statLifeMax2) {
					player.statLife = player.statLifeMax2;
				}
			}

			manaHeartCounter -= 200;
		}

		public override void AnglerQuestReward(float quality, List<Item> rewardItems) {
			if (voidMonolith) {
				Item sticky = new Item();
				sticky.SetDefaults(ItemID.StickyDynamite);
				sticky.stack = 4;
				rewardItems.Add(sticky);
			}

			foreach (Item item in rewardItems) {
				if (item.type == ItemID.GoldCoin) {
					int stack = item.stack;
					item.SetDefaults(ItemID.PlatinumCoin);
					item.stack = stack;
				}
			}
		}

		public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel) {
			if (player.FindBuffIndex(BuffType<CarMount>()) > -1) {
				fishingLevel = (int)(fishingLevel * 1.1f);
			}
		}

		public override void GetDyeTraderReward(List<int> dyeItemIDsPool) {
			if (player.FindBuffIndex(BuffID.UFOMount) > -1) {
				dyeItemIDsPool.Clear();
				dyeItemIDsPool.Add(ItemID.MartianArmorDye);
			}
		}

		public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			/*if (eFlames) {
				if (Main.rand.NextBool(4) && drawInfo.shadow == 0f) {
					int dust = Dust.NewDust(drawInfo.position - new Vector2(2f, 2f), player.width + 4, player.height + 4, DustType<EtherealFlame>(), player.velocity.X * 0.4f, player.velocity.Y * 0.4f, 100, default(Color), 3f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 1.8f;
					Main.dust[dust].velocity.Y -= 0.5f;

					Main.playerDrawDust.Add(dust);
				}

				r *= 0.1f;
				g *= 0.2f;
				b *= 0.7f;
				fullBright = true;
			}

			if (nonStopParty && drawInfo.shadow == 0f && Main.rand.NextBool(6)) { // checking shadow == 0 helps avoid spawning extra dust because of extra shadow draws.
				int dustIndex = Dust.NewDust(drawInfo.position + new Vector2(drawInfo.drawPlayer.width / 2 - 2, -30), 4, 4, 219, 0f, 0f, 100, default(Color), 1f);
				
				Dust dust = Main.dust[dustIndex];
				dust.velocity.X = Main.rand.NextFloat(-1f, 1f);
				dust.velocity.Y = Main.rand.NextFloat(-3, -1.5f);
				dust.scale = 1f + Main.rand.NextFloat(-.030f, .031f);
				
				Main.playerDrawDust.Add(dustIndex);
			}*/
		}

		/*public static readonly PlayerLayer MiscEffectsBack = new PlayerLayer("ExampleMod", "MiscEffectsBack", PlayerLayer.MiscEffectsBack, delegate (PlayerDrawInfo drawInfo) {
			if (drawInfo.shadow != 0f) {
				return;
			}
			
			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("ExampleMod");
			ExamplePlayer modPlayer = drawPlayer.GetModPlayer<ExamplePlayer>();
			
			if (modPlayer.reviveTime > 0) {
				Texture2D texture = mod.GetTexture("NPCs/PuritySpirit/Revive");
				
				int drawX = (int)(drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X);
				int drawY = (int)(drawInfo.position.Y + drawPlayer.height / 4f - 60f + modPlayer.reviveTime - Main.screenPosition.Y);
				
				DrawData data = new DrawData(texture, new Vector2(drawX, drawY), null, Color.White * (modPlayer.reviveTime / 60f), 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), 1f, SpriteEffects.None, 0);
				
				Main.playerDrawData.Add(data);
			}
		});
		public static readonly PlayerLayer MiscEffects = new PlayerLayer("ExampleMod", "MiscEffects", PlayerLayer.MiscEffectsFront, delegate (PlayerDrawInfo drawInfo) {
			if (drawInfo.shadow != 0f) {
				return;
			}

			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("ExampleMod");
			ExamplePlayer modPlayer = drawPlayer.GetModPlayer<ExamplePlayer>();

			if (modPlayer.lockTime > 0) {
				int frame = 2;

				if (modPlayer.lockTime > 50) {
					frame = 0;
				}
				else if (modPlayer.lockTime > 40) {
					frame = 1;
				}

				Texture2D texture = mod.GetTexture("NPCs/Lock");
				
				int frameSize = texture.Height / 3;
				int drawX = (int)(drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X);
				int drawY = (int)(drawInfo.position.Y + drawPlayer.height / 2f - Main.screenPosition.Y);
				
				DrawData data = new DrawData(texture, new Vector2(drawX, drawY), new Rectangle(0, frameSize * frame, texture.Width, frameSize), Lighting.GetColor((int)((drawInfo.position.X + drawPlayer.width / 2f) / 16f), (int)((drawInfo.position.Y + drawPlayer.height / 2f) / 16f)), 0f, new Vector2(texture.Width / 2f, frameSize / 2f), 1f, SpriteEffects.None, 0);
				
				Main.playerDrawData.Add(data);
			}

			if (modPlayer.badHeal) {
				Texture2D texture = mod.GetTexture("Buffs/Skull");
				int drawX = (int)(drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X);
				int drawY = (int)(drawInfo.position.Y - 4f - Main.screenPosition.Y);

				DrawData data = new DrawData(texture, new Vector2(drawX, drawY), null, Lighting.GetColor((int)((drawInfo.position.X + drawPlayer.width / 2f) / 16f), (int)((drawInfo.position.Y - 4f - texture.Height / 2f) / 16f)), 0f, new Vector2(texture.Width / 2f, texture.Height), 1f, SpriteEffects.None, 0);
				
				Main.playerDrawData.Add(data);

				for (int k = 0; k < 2; k++) {
					int dust = Dust.NewDust(new Vector2(drawInfo.position.X + drawPlayer.width / 2f - texture.Width / 2f, drawInfo.position.Y - 4f - texture.Height), texture.Width, texture.Height, DustType<Smoke>(), 0f, 0f, 0, Color.Black);
					
					Main.dust[dust].velocity += drawPlayer.velocity * 0.25f;

					Main.playerDrawDust.Add(dust);
				}
			}
		});*/

		/*public override void ModifyDrawLayers(List<PlayerLayer> layers) {
			MiscEffectsBack.visible = true;

			layers.Insert(0, MiscEffectsBack);

			MiscEffects.visible = true;
			layers.Add(MiscEffects);
		}*/

		public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText) {
			if (nurse.life != nurse.lifeMax) {
				chatText = "Sorry, I'm hurt, you'll have to wait. Ouch!";
				return false;
			}

			return base.ModifyNurseHeal(nurse, ref health, ref removeDebuffs, ref chatText);
		}

		public override void PostBuyItem(NPC vendor, Item[] shop, Item item) {
			// Here we use PostBuyItem to limit the player to only buying 1 item from the ExamplePersonFreeGiftList by removing items from the shop.
			if (vendor.type == NPCType<ExamplePerson>() && item.GetGlobalItem<ExampleInstancedGlobalItem>().examplePersonFreeGift) {
				examplePersonGiftReceived = true;
				foreach (var shopItem in shop) {
					if (!shopItem.IsAir && shopItem.GetGlobalItem<ExampleInstancedGlobalItem>().examplePersonFreeGift) {
						shopItem.TurnToAir();
					}
				}
			}
		}

		public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
			// Here we use PostSellItem to let the player buy a different item from the ExamplePersonFreeGiftList when the player sells the item back.
			if (vendor.type == NPCType<ExamplePerson>() && (GetInstance<ExampleConfigServer>().ExamplePersonFreeGiftList?.Any(x => x.Type == item.type) ?? false)) {
				examplePersonGiftReceived = false;
				item.TurnToAir();
				Main.NewText("You are returning your free gift? Come back in a second and I'll show you the free gifts again.");
			}
		}
	}
}

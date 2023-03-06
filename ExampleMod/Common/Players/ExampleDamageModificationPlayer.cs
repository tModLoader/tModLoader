using ExampleMod.Content.Buffs;
using ExampleMod.Content.Items.Accessories;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	internal class ExampleDamageModificationPlayer : ModPlayer
	{
		public float AdditiveCritDamageBonus;

		// These 3 fields relate to the Example Dodge. Example Dodge is modeled after the dodge ability of the Hallowed armor set bonus.
		// exampleDodge indicates if the player actively has the ability to dodge the next attack. This is set by ExampleDodgeBuff, which in this example is applied by the HitModifiersShowcase weapon. The buff is only applied if exampleDodgeCooldown is 0 and will be cleared automatically if an attack is dodged or if the player is no longer holding HitModifiersShowcase.
		public bool exampleDodge; // TODO: Example of custom player render
		// Used to add a delay between Example Dodge being consumed and the next time the dodge buff can be aquired.
		public int exampleDodgeCooldown;
		// Controls the intensity of the visual effect of the dodge.
		public int exampleDodgeVisualCounter;

		// If this player has an accessory which gives this effect and has enough health
		public bool hasAbsorbTeamDamageEffect;
		// If the player is currently in range of a player with hasAbsorbTeamDamageEffect
		public bool defendedByAbsorbTeamDamageEffect;

		public override void ResetEffects() {
			AdditiveCritDamageBonus = 0f;

			exampleDodge = false;
			if (exampleDodgeCooldown > 0)
				exampleDodgeCooldown--;

			hasAbsorbTeamDamageEffect = false;
			defendedByAbsorbTeamDamageEffect = false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if(AdditiveCritDamageBonus > 0) {
				modifiers.CritDamage += AdditiveCritDamageBonus;
			}
		}

		public override void PostUpdateEquips() {
			// If the conditions for the player having the buff are no longer true, remove the buff.
			// This could could techinically go in ExampleDodgeBuff.Update, but typically these effects are given by armor or accessories, so showing this example here is more useful.
			if(exampleDodge && Player.HeldItem.type != ModContent.ItemType<HitModifiersShowcase>()) {
				Player.ClearBuff(ModContent.BuffType<ExampleDodgeBuff>());
			}
			
			// exampleDodgeVisualCounter should be updated here, not in DrawEffects, to work properly
			exampleDodgeVisualCounter = Math.Clamp(exampleDodgeVisualCounter + (exampleDodge ? 1 : -1), 0, 30);
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			// exampleDodgeVisualCounter helps fade the color effect in and out.
			if (exampleDodgeVisualCounter > 0) {
				g = Math.Max(0, g - exampleDodgeVisualCounter * 0.03f);
			}
		}

		public override bool ConsumableDodge(PlayerDeathReason damageSource, int cooldownCounter) {
			if (exampleDodge) {
				ExampleDodgeEffects();
				return true;
			}
			return base.ConsumableDodge(damageSource, cooldownCounter);
		}

		// ExampleDodgeEffects() will be called from ConsumableDodge and HandleExampleDodgeMessage to sync the effect.
		public void ExampleDodgeEffects() {
			Player.SetImmuneTimeForAllTypes(Player.longInvince ? 120 : 80);

			// Some sound and visual effects
			for (int i = 0; i < 50; i++) {
				Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
				Dust d = Dust.NewDustPerfect(Player.Center + speed * 16, DustID.BlueCrystalShard, speed * 5, Scale: 1.5f);
				d.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.5f });

			// The visual and sound effects happen on all clients, but the code below only runs for the dodging player 
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			// Clearing the buff and assigning the cooldown time
			Player.ClearBuff(ModContent.BuffType<ExampleDodgeBuff>());
			exampleDodgeCooldown = 120; // 3 second cooldown before the buff can be given again.

			if (Main.netMode != NetmodeID.SinglePlayer) {
				SendExampleDodgeMessage(Player.whoAmI);
			}
		}

		public static void HandleExampleDodgeMessage(BinaryReader reader, int whoAmI) {
			int player = reader.ReadByte();
			if (Main.netMode == NetmodeID.Server) {
				player = whoAmI;
			}

			Main.player[player].GetModPlayer<ExampleDamageModificationPlayer>().ExampleDodgeEffects();

			if (Main.netMode == NetmodeID.Server) {
				// If the server receives this message, it sends it to all other clients to sync the effects.
				SendExampleDodgeMessage(player);
			}
		}

		public static void SendExampleDodgeMessage(int whoAmI) {
			// This code is called by both the initial 
			ModPacket packet = ModContent.GetInstance<ExampleMod>().GetPacket();
			packet.Write((byte)ExampleMod.MessageType.ExampleDodge);
			packet.Write((byte)whoAmI);
			packet.Send(ignoreClient: whoAmI);
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (defendedByAbsorbTeamDamageEffect && Player.whoAmI == Main.myPlayer && TeammateHasPalidinShieldAndCanTakeDamage()) {
				modifiers.FinalDamage *= 1f - AbsorbTeamDamageBuff.TeamDamageAbsorptionPercent;
			}
		}

		private bool TeammateHasPalidinShieldAndCanTakeDamage() {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player otherPlayer = Main.player[i];
				if (i != Main.myPlayer && IsCandidateForAbsorbingMyDamage(otherPlayer)) {
					return true;
				}
			}
			return false;
		}

		private bool IsCandidateForAbsorbingMyDamage(Player otherPlayer) {
			return otherPlayer.active && !otherPlayer.dead && !otherPlayer.immune && otherPlayer.GetModPlayer<ExampleDamageModificationPlayer>().hasAbsorbTeamDamageEffect && otherPlayer.team == Player.team && otherPlayer.statLife > otherPlayer.statLifeMax2 * AbsorbTeamDamageAccessory.DamageAbsorptionAbilityLifeThreshold;
		}

		public override void OnHurt(Player.HurtInfo info) {
			// On Hurt is used in this example to act upon another player being hurt.
			Player localPlayer = Main.player[Main.myPlayer];
			if (defendedByAbsorbTeamDamageEffect && Player.whoAmI != Main.myPlayer && localPlayer.GetModPlayer<ExampleDamageModificationPlayer>().hasAbsorbTeamDamageEffect && localPlayer.team == Player.team && Player.team != 0) {
				// This code finds the closest player wearing AbsorbTeamDamageAccessory. 
				float distance = localPlayer.Distance(Player.Center);
				bool localPlayerIsClosestPaladinShiledWearer = distance < 800f;
				if (localPlayerIsClosestPaladinShiledWearer) {
					for (int i = 0; i < Main.maxPlayers; i++) {
						Player otherPlayer = Main.player[i];
						if (i != Main.myPlayer && IsCandidateForAbsorbingMyDamage(otherPlayer)) {
							float otherPlayerDistance = otherPlayer.Distance(Player.Center);
							if (distance > otherPlayerDistance || (distance == otherPlayerDistance && i < Main.myPlayer)) {
								localPlayerIsClosestPaladinShiledWearer = false;
								break;
							}
						}
					}
				}

				// If the closest player wearing AbsorbTeamDamageAccessory happens to be the local player, then this code will run, hurting the player.
				if (localPlayerIsClosestPaladinShiledWearer) {
					// The intention of AbsorbTeamDamageAccessory is to transfer 30% of damage taken by teammates to the wearer.
					// In ModifiedHurt, we reduce the damage by 30%. The resulting reduced damage is passed to OnHurt, where the player wearing AbsorbTeamDamageAccessory hurts themselves.
					// Since OnHurt is provided with the damage already reduced by 30%, we need to reverse the math to determine how much the damage was originally reduced by
					// Working throught the math, the amount of damage that was reduced is equal to: damage * (percent / (1 - percent))
					float percent = AbsorbTeamDamageBuff.TeamDamageAbsorptionPercent;
					int damage = (int)(info.Damage * (percent / (1 - percent)));
					localPlayer.Hurt(PlayerDeathReason.LegacyEmpty(), damage, 0);
				}
			}
		}
	}
}

using ExampleMod.Common.Players;
using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	/// <summary>
	/// This item can help conceptualize various damage modification concepts. <br/>
	/// The Item.damage of this weapon is 100 so the math is easy to follow. Damage variation is disabled for all modes except the 1st mode for the same reason. <br/>
	/// When testing this weapon the first time, it is recommended to disable other mods and to remove all damage boosting accessories, as they will complicate the math being taught. <br/>
	/// Testing against <see cref="NPCID.BlueArmoredBonesNoPants"/> is recommended as it has high defense (50), good knockback resistance, and enough health for a few hits. Having 50 defense makes the math for defense and armor penetration easy to follow.
	/// <br/>
	/// The math taught in this example also assumes the player is in a normal world. <br/> 
	/// Use right click to switch modes.<br/>
	/// This example is purely for demonstation purposes only, it will not work in multiplayer. This should also not be considered correct code for a working dual-use weapon. <br/>
	/// </summary>
	public class HitModifiersShowcase : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleSword";

		private const int numberOfModes = 8;
		private int mode = 0;

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;

			Item.DamageType = DamageClass.Melee;
			Item.damage = 100;
			Item.knockBack = 5;
			Item.crit = 10;
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)mode);
		}

		public override void NetReceive(BinaryReader reader) {
			mode = reader.ReadByte();
		}

		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				mode++;
				if (mode >= numberOfModes) {
					mode = 0;
				}
				Main.NewText($"Switching to mode #{mode}: {GetMessageForMode()}");
				// This line will trigger NetSend to be called at the end of this game update, allowing the changes to useStyle to be in sync. 
				Item.NetStateChanged();
			}
			else {
				Main.NewText($"Mode #{mode}: {GetMessageForMode()}");
			}
			return true;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (!Main.rand.NextBool(3))
				return;
			Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Firework_Red + mode);
		}

		private string GetMessageForMode() {
			switch (mode) {
				case 0:
					return "Normal damage behavior";
				case 1:
					return "Damage variation disabled";
				case 2:
					return "50% extra knockback";
				case 3:
					return "200% extra critical hit damage";
				case 4:
					return "10 extra armor penetration. Test against high defense enemy";
				case 5:
					// This is similar to the Lightning Aura and Flymeal weapon effects
					return "50% extra armor penetration. Ignores 50% of enemy defense";
				case 6:
					return "Will apply ExampleDefenseDebuff, reducing defense by 25%";
				case 7:
					return "On hit, gives player ExampleDodgeBuff to dodge the next hit";

			}
			return "Unknown mode";
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			// These effects modify the hit itself, so they need to be in this method.
			if (mode != 0) {
				modifiers.DamageVariationScale *= 0f;
			}
			if (mode == 2) {
				modifiers.Knockback += .5f;
			}
			else if (mode == 3) {
				modifiers.CritDamage += 2f; // Default crit is 100% more than a normal hit, so with this in effect, crits should deal 4x damage
			}
			else if (mode == 4) {
				modifiers.ArmorPenetration += 10f;
			}
			else if (mode == 5) {
				modifiers.ScalingArmorPenetration += 0.5f;
			}

			// Below is an example of using ModifyHitInfo to alter the final value of damage, between Modify and OnHit hooks.
			// This 'backdoor' is a replacmenet for the old style of modifiers which allowed modifying the damage via `ref`
			// Please only use this if absolutely necessary, as multiple mods freely altering the damage results will create incompatible or unintutive player experiences.
			//
			// For example, the effect below could be better implemented by checking `player.GetWeaponDamage(Item)` and adding to FinalDamage.Base, SourceDamage.Base, SourceDamage.Flat or FlatBonusDamage
			/*
			modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) => {
				if (hitInfo.Damage > 10) {
					hitInfo.Damage += 5;
				}
			};
			*/
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			// These effects act on a hit happening, so they should go here.
			// Buffs added locally are automatically synced to the server and other players in multiplayer
			if (mode == 6) {
				target.AddBuff(ModContent.BuffType<ExampleDefenseDebuff>(), 600);
			}
			else if (mode == 7) {
				var damageModificationPlayer = player.GetModPlayer<ExampleDamageModificationPlayer>();
				if (damageModificationPlayer.exampleDodgeCooldown == 0) {
					player.AddBuff(ModContent.BuffType<ExampleDodgeBuff>(), 1800);
				}
			}
		}

		// Due to the differences in pvp damage calculations, only some of the effects of this weapon work in pvp.
		public override void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) {
			// Unlike the effects in OnHitPvp, these specific effects need to run on all clients to keep things in sync, so there is no check for local player.
			if (mode == 2) {
				modifiers.Knockback += .5f;
			}
			else if (mode == 4) {
				modifiers.ArmorPenetration += 10f;
			}
			else if (mode == 5) {
				modifiers.ScalingArmorPenetration += 0.5f;
			}
		}

		public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) {
			// These effects of this weapon should only run on the player damaging another, this check does that.
			if (player != Main.LocalPlayer)
				return;

			if (mode == 6) {
				// This AddBuff is not quiet because it is affecting another player. This allows it to broadcast to all players that the target has a buff. (Main.pvpBuff must be set to true for other players to be able to give buffs to a player)
				// Note that in PvP, it is possible to attack a player and see them take damage, but by the time the hit message arrives on the target client, they may have recharged a dodge. In this case, the target will not actually take damage, and their health will appear to restore. Because the attacking player applies the debuff, the target will receive the debuff regardless
				target.AddBuff(ModContent.BuffType<ExampleDefenseDebuff>(), 600, quiet: false);
			}
			else if (mode == 7) {
				var damageModificationPlayer = player.GetModPlayer<ExampleDamageModificationPlayer>();
				if (damageModificationPlayer.exampleDodgeCooldown == 0) {
					player.AddBuff(ModContent.BuffType<ExampleDodgeBuff>(), 1800);
				}
			}
		}
	}
}

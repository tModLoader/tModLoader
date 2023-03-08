using ExampleMod.Common.Players;
using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	/// <summary>
	/// This item can help conceptualize various damage modification concepts. <br/>
	/// The Item.damage of this weapon is 100 so the math is easy to follow. Damage variation is disabled for all modes except the 1st mode for the same reason. <br/>
	/// When testing this weapon the first time, it is recommended to disable other mods and to remove all damage boosting accessories, as they will complicate the math being taught. <br/>
	/// Testing against <see cref="NPCID.ArmoredSkeleton"/> is recommended as it has good defense, some knockback resistance, and enough health for a few hits.
	/// <br/>
	/// The math taught in this example also assumes the player is in a normal world. <br/> 
	/// Use right click to switch modes.<br/>
	/// This example is purely for demonstation purposes only, it will not work in multiplayer. This should also not be considered correct code for a working dual-use weapon. <br/>
	/// </summary>
	public class HitModifiersShowcase : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleSword";

		private const int numberOfModes = 7;
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

		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				mode++;
				if (mode >= numberOfModes) {
					mode = 0;
				}
				Main.NewText($"Switching to m #{mode}: {GetMessageForMode()}");
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
					return "Will apply ExampleDefenseDebuff, reducing defense by 25%";
				case 6:
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
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			// These effects act on a hit happening, so they should go here.
			// Buffs added locally are automatically synced to the server and other players in multiplayer
			if (mode == 5) {
				target.AddBuff(ModContent.BuffType<ExampleDefenseDebuff>(), 600);
			}
			else if (mode == 6) {
				var damageModificationPlayer = player.GetModPlayer<ExampleDamageModificationPlayer>();
				if (damageModificationPlayer.exampleDodgeCooldown == 0) {
					player.AddBuff(ModContent.BuffType<ExampleDodgeBuff>(), 1800);
				}
			}
		}
	}
}

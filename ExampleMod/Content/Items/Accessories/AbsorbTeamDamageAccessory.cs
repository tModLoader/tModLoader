using ExampleMod.Common.Players;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ExampleMod.Content.Buffs;
using Terraria.Localization;

namespace ExampleMod.Content.Items.Accessories
{
	/// <summary>
	/// AbsorbTeamDamageAccessory mimics the unique effect of the Paladin's Shield item.
	/// This example showcases some advanced interplay between accessories, buffs, and ModPlayer hooks.
	/// Of particular note is how this accessory gives other players a buff and how a player might act on another player being hit.
	/// </summary>
	[AutoloadEquip(EquipType.Shield)]
	public class AbsorbTeamDamageAccessory : ModItem
	{
		public static readonly int DamageAbsorptionAbilityLifeThresholdPercent = 50;
		public static float DamageAbsorptionAbilityLifeThreshold => DamageAbsorptionAbilityLifeThresholdPercent / 100f;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AbsorbTeamDamageBuff.TeamDamageAbsorption, DamageAbsorptionAbilityLifeThresholdPercent);

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.defense = 6;
			Item.value = Item.buyPrice(0, 30, 0, 0); 
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// player.noKnockback = true; could be used here if this accessory prevented knockback.

			if (player.statLife > player.statLifeMax2 * DamageAbsorptionAbilityLifeThreshold) {
				// The wearer only has this effect if health is high enough.
				player.GetModPlayer<ExampleDamageModificationPlayer>().hasAbsorbTeamDamageEffect = true;

				// Remember that UpdateAccessory runs for all players on all clients. This code gives the local player a buff if the player wearing this accessory is within 50 tiles and is on the same team.
				if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0) {
					Player LocalPlayer = Main.player[Main.myPlayer];
					if (LocalPlayer.team == player.team && player.team != 0) {
						float distanceInWorldCooridinates = player.Distance(LocalPlayer.Center);
						// 50 tiles is 800 world units. (50 * 16 == 800)
						if (distanceInWorldCooridinates < 800f) {
							LocalPlayer.AddBuff(ModContent.BuffType<AbsorbTeamDamageBuff>(), 20);
						}
					}
				}
			}
		}
	}
}

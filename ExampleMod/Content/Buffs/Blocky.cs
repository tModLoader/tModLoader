using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Placeable;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class Blocky : ModBuff
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blocky");
			Description.SetDefault("Jumping power is increased");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			ExampleCostumePlayer p = player.GetModPlayer<ExampleCostumePlayer>();

			// We use blockyAccessoryPrevious here instead of blockyAccessory because UpdateBuffs happens before UpdateEquips but after ResetEffects.
			if (player.townNPCs >= 1 && p.BlockyAccessoryPrevious) {
				p.BlockyPower = true;

				if (Main.myPlayer == player.whoAmI && Main.time % 1000 == 0) {
					player.QuickSpawnItem(player.GetSource_Buff(buffIndex), ModContent.ItemType<ExampleBlock>());
				}

				player.jumpSpeedBoost += 4.8f;
				player.extraFall += 45;

				// Some other effects:
				//player.lifeRegen++;
				//player.meleeCrit += 2;
				//player.meleeDamage += 0.051f;
				//player.meleeSpeed += 0.051f;
				//player.statDefense += 3;
				//player.moveSpeed += 0.05f;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}

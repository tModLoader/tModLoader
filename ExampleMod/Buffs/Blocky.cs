using ExampleMod.Items.Placeable;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Buffs
{
	public class Blocky : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Blocky");
			Description.SetDefault("Jumping power is increased");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			canBeCleared = false;
		}

		public override void Update(Player player, ref int buffIndex) {
			ExamplePlayer p = player.GetModPlayer<ExamplePlayer>();

			// We use blockyAccessoryPrevious here instead of blockyAccessory because UpdateBuffs happens before UpdateEquips but after ResetEffects.
			if (player.townNPCs >= 1 && p.blockyAccessoryPrevious) {
				p.blockyPower = true;
				if (Main.myPlayer == player.whoAmI && Main.time % 1000 == 0) {
					player.QuickSpawnItem(ItemType<ExampleBlock>());
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

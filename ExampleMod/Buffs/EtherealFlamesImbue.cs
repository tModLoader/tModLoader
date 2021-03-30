using ExampleMod.Items.Placeable;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class EtherealFlamesImbue : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Weapon Imbue: Ethereal Flames");
			Description.SetDefault("Melee attacks Inflict ethereal flames");
		}

		public override void Update(Player player, ref int buffIndex) {
			ExamplePlayer p = player.GetModPlayer<ExamplePlayer>(); //Important line because we can use it to get stuff from the ExamplePlayer class
			p.eFlamesImbue = true; //this sets the bool in ExamplePlayer that detects if you have the imbue buff to true.
		}
	}
}

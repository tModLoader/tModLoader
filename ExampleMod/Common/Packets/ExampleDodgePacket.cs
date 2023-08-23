using ExampleMod.Common.Players;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader.Packets;

namespace ExampleMod.Common.Packets;

[NetPacket(typeof(ExampleMod))]
public partial struct ExampleDodgePacket
{
	public byte PlayerWhoAmI { get; set; }

	public void HandlePacket(int sender) {
		if (Main.netMode == NetmodeID.Server) {
			PlayerWhoAmI = (byte)sender;
		}

		Main.player[PlayerWhoAmI].GetModPlayer<ExampleDamageModificationPlayer>().ExampleDodgeEffects();
	}
}

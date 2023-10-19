﻿using ExampleMod.Common.Players;
using Terraria;
using Terraria.ModLoader.Packets;

namespace ExampleMod.Common.Packets;

[NetPacket(typeof(ExampleMod))]
public partial struct ExampleStatIncreasePlayerPacket
{
	public byte PlayerWhoAmI { get; set; }
	public byte PlayerExampleLifeFruits { get; set; }
	public byte PlayerExampleManaCrystals { get; set; }

	public void HandlePacket(int sender) {
		ExampleStatIncreasePlayer statIncreasePlayer = Main.player[PlayerWhoAmI].GetModPlayer<ExampleStatIncreasePlayer>();

		statIncreasePlayer.exampleLifeFruits = PlayerExampleLifeFruits;
		statIncreasePlayer.exampleManaCrystals = PlayerExampleManaCrystals;

		// Forward the changes to the other clients
		SendToAllPlayers(ignoreClient: PlayerWhoAmI);
	}
}

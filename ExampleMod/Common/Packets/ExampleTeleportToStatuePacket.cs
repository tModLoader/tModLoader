using ExampleMod.Content.NPCs;
using Terraria;
using Terraria.ModLoader.Packets;

namespace ExampleMod.Common.Packets;

[NetPacket(typeof(ExampleMod))]
public partial struct ExampleTeleportToStatuePacket
{
	public byte NpcWhoAmI { get; set; }

	public readonly void HandlePacket(int sender) {
		if (Main.npc[NpcWhoAmI].ModNPC is ExamplePerson person && person.NPC.active) {
			person.StatueTeleport();
		}
	}
}

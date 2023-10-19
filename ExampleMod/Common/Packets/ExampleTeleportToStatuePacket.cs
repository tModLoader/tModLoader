using ExampleMod.Content.NPCs;
using Terraria.ModLoader.Packets;

namespace ExampleMod.Common.Packets;

[NetPacket(typeof(ExampleMod))]
public partial struct ExampleTeleportToStatuePacket
{
	public ExamplePerson Person { get; set; }

	public readonly void HandlePacket(int sender) {
		if (Person is not null && Person.NPC.active) {
			Person.StatueTeleport();
		}
	}
}

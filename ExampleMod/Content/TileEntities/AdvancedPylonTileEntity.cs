using ExampleMod.Content.Tiles;
using System.IO;
using Terraria;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.TileEntities
{
	/// <summary>
	/// This TileEntity is used in direct tandem with <seealso cref="ExamplePylonTileAdvanced"/> in order to grant more flexibility than
	/// vanilla's normal pylon TileEntity (AKA <seealso cref="TETeleportationPylon"/>) using the <seealso cref="TEModdedPylon"></seealso> class
	/// that is built into tML itself.
	/// <para>
	/// The main example shown here is having a Pylon that is only active at completely random intervals.
	/// </para>
	/// </summary>
	public class AdvancedPylonTileEntity : TEModdedPylon
	{
		// This is the main crux of this TileEntity; its pylon functionality will only work when this boolean is true.
		public bool isActive;

		public override void NetSend(BinaryWriter writer) {
			// We want to make sure that our data is synced properly across clients and server.
			// NetSend is called whenever a TileEntitySharing message is sent, so the game will handle this automatically for us,
			// granted that we send a message when we need to.
			writer.WriteFlags(isActive);
		}

		public override void NetReceive(BinaryReader reader) {
			reader.ReadFlags(out isActive);
		}

		public override void Update() {
			// Update is only ever called on the Server or in SinglePlayer, so our randomness will be in that frame of reference
			// Every tick, there will be a 1/180 chance that the active state of this pylon will swap (ON to OFF or vice versa)
			if (!Main.rand.NextBool(180)) {
				return;
			}

			// Granted that the check passes, we change the active state, and if this is on the server, we sync it with the server:
			isActive = !isActive;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
			}
		}
	}
}

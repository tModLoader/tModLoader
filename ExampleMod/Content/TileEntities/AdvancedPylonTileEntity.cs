using ExampleMod.Content.Tiles;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.TileEntities
{
	/// <summary>
	/// This TileEntity is used in direct tandem with <seealso cref="ExamplePylonTileAdvanced"/> in order to grant more flexibility than
	/// vanilla's normal pylon TileEntity (AKA <seealso cref="TETeleportationPylon"/>) using the <seealso cref="IPylonTileEntity"/> interface
	/// along-side other tile functions.
	/// <para>
	/// The main example shown here is having a Pylon that is only active at completely random intervals.
	/// </para>
	/// </summary>
	public class AdvancedPylonTileEntity : ModTileEntity, IPylonTileEntity //This interface is EXTREMELY important! It allows these tile entities to be included with the TETeleportationPylon entities, do not forget it!
	{
		//This is the main crux of this TileEntity; its pylon functionality will only work when this boolean is true.
		public bool isActive;

		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Framing.GetTileSafely(x, y);

			//This check is only if your tile has ONE style. If you plan on adding more styles, make sure to use modular arithmetic to ensure things are valid.
			//For example, if your pylon tile has two styles and is 2 tiles wide, you should make sure tile.TileFrameX % 36 == 0. (assuming 18 pixels per tile)
			return tile.HasTile && tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}


		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			//This hook is called on the CLIENT of whoever placed the tile; thus we need to sync the data with the server and inform everyone this entity was placed
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 3);

				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
			}

			//Then, after that, actually place the Tile Entity, making sure we account for the origin displacement:
			Point16 tileOrigin = ModContent.GetInstance<ExamplePylonTileAdvanced>().tileOrigin;
			int placedEntityID = Place(i - tileOrigin.X, j - tileOrigin.Y);
			if (placedEntityID != -1) {
				//Place returning -1 means that the placement failed; granted that is doesn't, however, we need to tell the pylon system to update, which is done like so:
				Main.PylonSystem.RequestImmediateUpdate(); //This line is very important, don't forget it, or these entities won't register with vanilla's pylon system!
			}

			return placedEntityID;
		}

		public override void OnNetPlace() {
			//This hook is only ever called on the server; its purpose is to give more freedom in terms of syncing FROM the server to clients, which we take advantage of
			//by making sure to sync whenever this hook is called:
			NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
		}

		public override void NetSend(BinaryWriter writer) {
			//We want to make sure that our data is synced properly across clients and server.
			//NetSend is called whenever a TileEntitySharing message is sent, so the game will handle this automatically for us,
			//granted that we send a message when we need to.
			writer.Write(isActive);
		}

		public override void NetReceive(BinaryReader reader) {
			isActive = reader.ReadBoolean();
		}

		public override void Update() {
			//Update is only ever called on the Server or in SinglePlayer, so our randomness will be in that frame of reference
			//Every tick, there will be a 1/180 chance that the active state of this pylon will swap (ON to OFF or vice versa)
			if (!Main.rand.NextBool(180)) {
				return;
			}

			//Granted that the check passes, we change the active state, and if this is on the server, we sync it with the server:
			isActive = !isActive;
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}
	}
}

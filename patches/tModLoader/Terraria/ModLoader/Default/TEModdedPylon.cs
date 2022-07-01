using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	/// <summary>
	/// This is a TML provided Tile Entity that acts extremely similar to vanilla's pylon TEs. If you plan
	/// to make a pylon tile that closely resembles (or is identical to) vanilla's pylon TEs, you should use
	/// this.
	/// </summary>
	public class TEModdedPylon : ModTileEntity, IPylonTileEntity
	{

		public override void NetPlaceEntityAttempt(int x, int y) {
			if (!GetModPylonFromCoords(x, y, out ModPylon pylon)) {
				RejectPlacementFromNet(x, y);
				return;
			}

			int ID = Place(x, y, pylon.Mod);
			NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: x, number3: y);
		}

		public bool TryGetModPylon(out ModPylon modPylon) => GetModPylonFromCoords(Position.X, Position.Y, out modPylon);

		private static void RejectPlacementFromNet(int x, int y) {
			WorldGen.KillTile(x, y);
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileManipulation, number2: x, number3: y);
			}
		}

		public int Place(int x, int y, Mod mod) {
			int newID = Place(x, y);

			(ByID[newID] as TEModdedPylon).Mod = mod;

			Main.PylonSystem.RequestImmediateUpdate();
			return newID;
		}

		private new int Place(int x, int y) {
			TEModdedPylon moddedPylon = new TEModdedPylon();
			moddedPylon.Position = new Point16(x, y);
			moddedPylon.ID = AssignNewID();
			moddedPylon.type = (byte)Type;

			lock (EntityCreationLock) {
				ByID[moddedPylon.ID] = moddedPylon;
				ByPosition[moddedPylon.Position] = moddedPylon;
			}

			return moddedPylon.ID;
		}

		public new void Kill(int x, int y) {
			if (!ByPosition.TryGetValue(new Point16(x, y), out TileEntity value) || value.type != Type) {
				return;
			}

			lock (EntityCreationLock) {
				ByID.Remove(value.ID);
				ByPosition.Remove(new Point16(x, y));
			}

			Main.PylonSystem.RequestImmediateUpdate();
		}

		public override string ToString() => Position.X + "x  " + Position.Y + "y";

		public override bool IsTileValidForEntity(int x, int y) {
			return Main.tile[x, y].active() && TileID.Sets.CountsAsPylon.Contains(Main.tile[x, y].type) && Main.tile[x, y].frameY == 0 && Main.tile[x, y].frameX % 54 == 0;
		}

		public int PlacementPreviewHook_AfterPlacement(int x, int y, int type, int style = 0, int direction = 1, int alternate = 0) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, x - 1, y - 3, 3, 4);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: x + -1, number2: y + -3, number3: Type);
				return -1;
			}

			return Place(x + -1, y + -3, TileLoader.GetTile(type).Mod);
		}

		public int PlacementPreviewHook_CheckIfCanPlace(int x, int y, int type, int style = 0, int direction = 1, int alternate = 0) {
			ModPylon pylon = TileLoader.GetTile(type) as ModPylon;

			if (PylonLoader.PreCanPlacePylon(x, y, type, pylon.PylonType) is bool value)
				return value ? 0 : 1;
			
			return pylon.CanPlacePylon() ? 0 : 1;
		}

		public static bool GetModPylonFromCoords(int x, int y, out ModPylon modPylon) {
			modPylon = null;
			Tile tile = Main.tile[x, y];
			if (tile != null && tile.active() && tile.type >= TileID.Count) {
				modPylon = TileLoader.GetTile(tile.type) as ModPylon;
				return true;
			}

			return false;
		}
	}
}

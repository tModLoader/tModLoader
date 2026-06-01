using System.Linq;
using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader.Default;

/// <summary>
/// This is a TML provided Tile Entity that acts extremely similar to vanilla's pylon TEs by default. If you plan
/// to make a pylon tile in any capacity, you must extend this TE at least once.
/// </summary>
public abstract class TEModdedPylon : ModTileEntity, IPylonTileEntity
{

	public override void NetPlaceEntityAttempt(int x, int y)
	{
		if (!GetModPylonFromCoords(x, y, out ModPylon pylon)) {
			RejectPlacementFromNet(x, y);
			return;
		}

		bool canPlace = PylonLoader.PreCanPlacePylon(x, y, pylon.Type, pylon.PylonType) ?? pylon.CanPlacePylon();
		if (!canPlace) {
			RejectPlacementFromNet(x, y);
			return;
		}

		int ID = Place(x, y);
		ModTileEntity newEntity = (ModTileEntity)ByID[ID];
		newEntity.OnNetPlace();
		NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: x, number3: y);
	}

	public bool TryGetModPylon(out ModPylon modPylon) => GetModPylonFromCoords(Position.X, Position.Y, out modPylon);

	private static void RejectPlacementFromNet(int x, int y)
	{
		WorldGen.KillTile(x, y);
		if (Main.netMode == NetmodeID.Server) {
			NetMessage.SendData(MessageID.TileManipulation, number2: x, number3: y);
		}
	}

	// Acts exactly like vanilla's TE does the placing process, except we tack on updating the pylon system.
	public new int Place(int i, int j)
	{
		int ID = base.Place(i, j);

		Main.PylonSystem.RequestImmediateUpdate();
		return ID;
	}

	// Acts exactly like vanilla's TE does the killing process, except we tack on updating the pylon system.
	public new void Kill(int x, int y)
	{
		base.Kill(x, y);

		Main.PylonSystem.RequestImmediateUpdate();
	}

	public override string ToString() => Position.X + "x  " + Position.Y + "y";

	public override bool IsTileValidForEntity(int x, int y)
	{
		// This is the default check that vanilla does for vanilla pylons. Feel free to override this if you use a differently sized pylon, or use a multi-framed pylon.
		TileObjectData tileData = TileObjectData.GetTileData(Main.tile[x, y]);
		return Main.tile[x, y].active() && TileID.Sets.CountsAsPylon.Contains(Main.tile[x, y].type) && Main.tile[x, y].frameY == 0 && Main.tile[x, y].frameX % tileData.CoordinateFullWidth == 0;
	}

	public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
	{
		TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
		int topLeftX = i - tileData.Origin.X;
		int topLeftY = j - tileData.Origin.Y;

		if (Main.netMode == NetmodeID.MultiplayerClient) {
			NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, tileData.Width, tileData.Height);
			NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
			return -1;
		}

		return Place(topLeftX,  topLeftY);
	}

	public int PlacementPreviewHook_CheckIfCanPlace(int x, int y, int type, int style = 0, int direction = 1, int alternate = 0)
	{
		ModPylon pylon = TileLoader.GetTile(type) as ModPylon;

		if (PylonLoader.PreCanPlacePylon(x, y, type, pylon.PylonType) is bool value)
			return value ? 0 : 1;

		return pylon.CanPlacePylon() ? 0 : 1;
	}

	public static bool GetModPylonFromCoords(int x, int y, out ModPylon modPylon)
	{
		Tile tile = Main.tile[x, y];
		if (tile.active() && TileLoader.GetTile(tile.type) is ModPylon p) {
			modPylon = p;
			return true;
		}

		modPylon = null;
		return false;
	}
}

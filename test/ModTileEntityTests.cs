using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

[TestClass]
public class ModTileEntityTests
{
	private sealed class UpdatingTileEntity : ModTileEntity
	{
		public override bool IsTileValidForEntity(int x, int y) => true;
	}

	private sealed class NonUpdatingTileEntity : ModTileEntity
	{
		public NonUpdatingTileEntity()
		{
			RequiresUpdates = false;
		}

		public override bool IsTileValidForEntity(int x, int y) => true;
	}

	[TestInitialize]
	public void ClearTileEntities()
	{
		TileEntity.Clear();
	}

	[TestCleanup]
	public void CleanupTileEntities()
	{
		TileEntity.Clear();
	}

	[TestMethod]
	public void PlacementRegistersUpdatingTileEntity()
	{
		var template = new UpdatingTileEntity();
		int id = template.Place(10, 20);
		TileEntity placed = TileEntity.ByID[id];

		CollectionAssert.Contains(TileEntity.UpdateEntities, placed);

		template.Kill(10, 20);

		CollectionAssert.DoesNotContain(TileEntity.UpdateEntities, placed);
	}

	[TestMethod]
	public void ConstructorCanDisableUpdates()
	{
		var template = new NonUpdatingTileEntity();
		int id = template.Place(10, 20);

		CollectionAssert.DoesNotContain(TileEntity.UpdateEntities, TileEntity.ByID[id]);
	}
}

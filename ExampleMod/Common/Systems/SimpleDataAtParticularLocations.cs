using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

/// <summary>
/// This Example illustrates a solution for storing Small-Sparse-Simple data at locations. The definitions of those are as follows:
/// Small/Large - < 10 locations are actively using the data per frame is small, > 10 is large. Use UNRELEASEDSYSTEM1 to do large-X-simple data.
/// Sparse/Filled - Sparse is that not all locations will have data, typically less than 60% in the world will have data. Use UNRELEASEDSYSTEM1 to do large-X-simple data.
/// Simple/Complex - Sorta arbitrary. Simple data will not contain methods, nor complicated functionality, and typically is just basic data types. Use TileEntities if working with complex data.
/// </summary>

///			Some other common use cases not exampled:
/// Getting data for a particular tile type your mod added, that was placed in world:
///		Trigger fetch of data using adjTiles[type]. If data is ordered, use appropriate version of PosData.Lookup. If data is not ordered, you will likely need to find via enumeration.
///		If it is unordered additions, you may elect to build myMap yourself OR attempt to insert the data so it remains ordered. The latter will lead to better post-event performance.
///	Clustering data to achieve sparsity:
///		If your application has multiple repeat static data in a row, you should elect to use Clustered mode in the builder to compress it. Note that you should NOT use PosData.LookupExact in this case.


// Future TODO: Improve documentation.
namespace ExampleMod.Common.Systems
{
	// Saving and loading requires TagCompounds, a guide exists on the wiki: https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound
	public class SimpleDataAtParticularLocations : ModSystem
	{
		// Create our map. Uses generics for whatever type you want of the data to store.
		public PosData<byte>[] myMap;

		// Next, we ensure we initialize the map on world load to an empty map.
		public override void ClearWorld() {
			myMap = [];
		}

		// We save our data sets using TagCompounds.
		// NOTE: The tag instance provided here is always empty by default.
		public override void SaveWorldData(TagCompound tag) {
			if (myMap.Length != 0) {
				tag["myMap"] = myMap.Select(info => new TagCompound {
					["pos"] = info.pos,
					["data"] = info.value
				}).ToList();
			}
		}

		// We load our data sets using the provided TagCompound. Should mirror SaveWorldData()
		public override void LoadWorldData(TagCompound tag) {

			List<PosData<byte>> list = new List<PosData<byte>>();
			foreach (var entry in tag.GetList<TagCompound>("myMap")) {
				list.Add(new PosData<byte>(
					entry.GetInt("pos"),
					entry.Get<byte>("data")
				));
			}
			myMap = list.ToArray();
		}

		// We define what we want to generate as additional location data, for this example, in PostWorldGen.
		// We will create a simple column of byte data going down the horizontal center of the world that we will later use in PreUpdateWorld.
		public override void PostWorldGen() {
			var builder = new PosData<byte>.OrderedSparseLookupBuilder(compressEqualValues: false);

			int xCenter = Main.maxTilesX / 2;

			for (int y = 0; y < Main.maxTilesY; y++) {
				builder.Add(
					xCenter, y, // The locations
					(byte)(y % 255) // The data we want to store at the location
				);
			}

			myMap = builder.Build();
		}

		// We call our custom method after testing that our map isn't empty - this ensures safe-loading on previous generated worlds!
		public override void PreUpdateWorld() {
			if (myMap.Length == 0) {
				return;
			}
			foreach (var player in Main.ActivePlayers) {
				UpdateFromNearestInMap(player);
			}
		}

		// We use the column at world center to paint nearby tiles based on the player's proximity to the nearest entry in the map.
		// In this case, the nearest entry should correspond to the player's depth.
		public void UpdateFromNearestInMap(Player player) {
			// Get player position in tile coordinates
			Point z = player.position.ToTileCoordinates();
			// Search for an entry within 32 tiles of our player
			if (PosData.NearbySearchOrderedPosMap(myMap, z, 32, out var entry)) {
				// If found, we grab the data from the corresponding output index
				var data = entry.value;

				// We then proceed to paint a 5x2 area around the player position with our locational custom values.
				for (int i = -2; i < 3; i++) {
					for (int j = 0; j < 2; j++) {
						Tile tile = Main.tile[z.X + i, z.Y + j];
						if (tile.HasTile) {
							tile.TileColor = data;
						}
					}
				}
			}
		}
	}
}
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

//Future TODO: Improve documentation.
namespace ExampleMod.Common.Systems
{
	//Saving and loading requires TagCompounds, a guide exists on the wiki: https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound
	public class SimpleDataAtParticularLocations : ModSystem
	{
		// Create the two main data sets - the array that will hold our map during run time, and the list containing our data.
		public PosIndexer.PosIndex[] myMap;
		public List<byte> myData;

		// Next, we ensure we initialize the data sets on world load.
		public override void OnWorldLoad() {
			myMap = new PosIndexer.PosIndex[0];
			myData = new List<byte>();
		}

		// We save our data sets using TagCompounds.
		public override TagCompound SaveWorldData() {
			TagCompound tag;
			if (myMap.Length == 0) {
				tag = null;
			} 
			else {
				tag = new TagCompound {
					["myMap"] = myMap.Select(info => new TagCompound {
						["pos"] = info.posID,
						["index"] = info.indexID
					}).ToList(),
					["myData"] = myData
				};
			}
			// We discard our datasets after saving, because we don't need them anymore.
			myMap = null;
			myData = null;

			return tag;
		}

		// We load our data sets using the provided TagCompound. Should mirror SaveWorldData()
		public override void LoadWorldData(TagCompound tag) {
			myData = tag.GetList<byte>("myData").ToList();

			List<PosIndexer.PosIndex> list = new List<PosIndexer.PosIndex>();
			foreach (var entry in tag.GetList<TagCompound>("myMap")) {
				list.Add(new PosIndexer.PosIndex {
					posID = entry.GetInt("pos"),
					indexID = entry.Get<ushort>("index")
				});
			}
			myMap = list.ToArray();
		}
		
		// We define what we want to generate as additional location data, for this example, in PostWorldGen. 
		// We will create a simple column of byte data going down the horizontal center of the world that we will later use in PreUpdateWorld.
		public override void PostWorldGen() {
			myData = new List<byte>();
			int xCenter = Main.maxTilesX / 2;
			List<PosIndexer.PosIndex> list = new List<PosIndexer.PosIndex>();
			for (int y = 0; y < Main.maxTilesY ; y++) {
				PosIndexer.MapPosToInfo(list, (ushort)myData.Count, xCenter, y);
				
				myData.Add((byte)(y % 255));
			}
			myMap = list.ToArray();
		}

		// We call our custom method after testing that our map isn't empty - this ensures safe-loading on previous generated worlds!
		public override void PreUpdateWorld() {
			if (myMap.Length == 0) {
				return;
			}
			foreach (var player in Main.player) {
				if (player.active) {
					UpdateFromNearestInMap(player);
				}
			}
		}

		// We use the column at world center to paint nearby tiles based on the player's proximity to the nearest entry in the map.
		// In this case, the nearest entry should correspond to the player's depth.
		public void UpdateFromNearestInMap(Player player) {
			// Get player position in tile coordinates
			Point z = player.position.ToTileCoordinates();
			// Search for an entry within 32 tiles of our player
			if (PosIndexer.NearbyBinarySearchPosMap(myMap, z, 32, out int mapIndex)) {
				// If found, we grab the entry from the corresponding output index
				var entry = myMap[mapIndex];

				// We then proceed to paint a 3x3 area around the player position with our locational custom values.
				for (int i = -1; i < 2; i++) {
					for (int j = -1; j < 2; j++) {
						Tile tile = Main.tile[z.X + i, z.Y + j];
						if (tile.active()) {
							tile.color(myData[entry.indexID]);
						}
					}
				}
			}
		}
	}
}

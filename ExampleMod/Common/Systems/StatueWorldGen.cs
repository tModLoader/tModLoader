using ExampleMod.Content.Tiles;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ExampleMod.Common.Systems
{
	// This is a simple example of generating statues in worlds.
	public class StatueWorldGen : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
			int resetIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Reset"));

			if (resetIndex == -1) {
				return;
			}

			tasks.Insert(resetIndex + 1, new PassLegacy("Example Mod Statue Setup", (progress, configuration) => {
				progress.Message = "Adding ExampleMod Statue";

				// The vanilla game has an array of statue types that we'll be adding ours to.
				var statueList = GenVars.statueList;
				int startIndex = statueList.Length; // Save the original length of the vanilla list to use later.

				// This is an array of statues we want to add to worldgen.
				// Set shouldBeWired to true to make the statue spawn with a pressure plate wired to it (like traps are).
				(int type, bool shouldBeWired, ushort placeStyle)[] statueTypesToAdd = {
					(ModContent.TileType<ExampleStatue>(), false, 0),
				};

				// Make space in the statueList array.
				Array.Resize(ref statueList, statueList.Length + statueTypesToAdd.Length);

				// And then add Point16s of (TileID, PlaceStyle) to it.
				for (int i = 0; i < statueTypesToAdd.Length; i++) {
					int arrayIndex = startIndex + i;
					(int statueType, bool shouldBeWired, ushort placeStyle) = statueTypesToAdd[i];

					statueList[arrayIndex] = new Point16(statueType, placeStyle);

					if (shouldBeWired) {
						GenVars.StatuesWithTraps.Add(arrayIndex);
					}
				}
			}));
		}
	}
}
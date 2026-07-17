using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	// This ModSystem demonstrates adding extra options to the world creation menu without using IL edits or On hooks.
	// The selected options are saved into the world header so they can also be displayed in the world select menu.
	//
	// Important details for custom world creation options:
	// - Every mod can Add options to the lists. After all mods finish ModifyWorldCreationMenuOptions, tModLoader assigns
	//   WorldCreationMenuOption.Type automatically in the final displayed list order, so multiple mods won't reuse the same TML ID.
	//   For example, if 3 mods add custom difficulties, they will automatically become GameModeID.TML, GameModeID.TML + 1,
	//   and GameModeID.TML + 2 in their final list order. The same idea applies to custom evils using WorldEvilID.TML.
	// - Difficulty option IDs start at GameModeID.TML and evil option IDs start at WorldEvilID.TML.
	// - Use option.Type inside OnSelected instead of hardcoding GameModeID.Expert or WorldEvilID.Crimson if the option
	//   represents a truly custom mode. Hardcoded vanilla IDs should only be used when intentionally selecting vanilla behavior.
	// - PreviewValue is separate from Type. It controls which vanilla preview art is shown in the world creation panel.
	//   Use byte.MaxValue for a truly custom preview that does not draw any vanilla difficulty or evil preview layer.
	// - IconTexturePath controls the small button icon. PreviewTexturePath and PreviewTexturePath2 can provide custom 76x76 preview layers.
	// - The world creation UI wraps options automatically when enough mods add options.
	public class ExampleWorldCreationMenuSystem : ModSystem
	{
		private const string HeaderKeyDifficulty = "ExampleDifficulty";
		private const string HeaderKeyEvil = "ExampleEvil";
		private const string HeaderKeySize = "ExampleSize";
		private const string ExampleDifficultyValue = "exampleDifficulty";
		private const string ExampleEvilValue = "exampleEvil";
		private const string ExampleSizeValue = "exampleSize";

		private static bool creatingExampleDifficultyWorld;
		private static bool creatingExampleEvilWorld;
		private static bool creatingExampleSizeWorld;

		public override void ModifyWorldCreationMenuOptions(ref List<WorldCreationMenuOption> sizeOptions, ref List<WorldCreationMenuOption> difficultyOptions, ref List<WorldCreationMenuOption> evilOptions) {
			WorldCreationMenuOption exampleSize = null;
			exampleSize = new WorldCreationMenuOption(
				"ExampleMod:ExampleSize",
				Mod.GetLocalization("WorldCreation.ExampleSize.DisplayName"),
				Mod.GetLocalization("WorldCreation.ExampleSize.Description"),
				Color.Gold,
				"ExampleMod/Assets/Textures/WorldCreation/IconSizeExample",
				() => {
					// This example intentionally uses the Large world's actual dimensions until custom dimensions are supported.
					WorldGen.SetWorldSize(0);
					creatingExampleSizeWorld = true;
				},
				() => creatingExampleSizeWorld,
				2,
				() => creatingExampleSizeWorld = false,
				previewTexturePath: "ExampleMod/Assets/Textures/WorldCreation/PreviewSizeExample"
			);
			sizeOptions.Add(exampleSize);

			// This local variable is captured by the OnSelected lambda. It is safe because Type is assigned by tModLoader
			// immediately after all mods have finished modifying the option list, before the player can click the button.
			WorldCreationMenuOption exampleDifficulty = null;
			exampleDifficulty = new WorldCreationMenuOption(
				"ExampleMod:ExampleDifficulty",
				Mod.GetLocalization("WorldCreation.ExampleDifficulty.DisplayName"),
				Mod.GetLocalization("WorldCreation.ExampleDifficulty.Description"),
				Color.Orange,
				"ExampleMod/Assets/Textures/WorldCreation/IconDifficultyExample",
				() => {
					// Use the automatically assigned TML range ID instead of reusing GameModeID.Expert, preventing conflicts between mods.
					// If another mod adds a difficulty before this one, this Type automatically moves back by 1.
					Main.GameMode = exampleDifficulty.Type;
					creatingExampleDifficultyWorld = true;
				},
				() => creatingExampleDifficultyWorld,
				// byte.MaxValue means this is not a vanilla difficulty preview. The 2 custom preview layers fully replace vanilla difficulty art.
				byte.MaxValue,
				() => creatingExampleDifficultyWorld = false,
				previewTexturePath: "ExampleMod/Assets/Textures/WorldCreation/PreviewDifficultyExample1",
				previewTexturePath2: "ExampleMod/Assets/Textures/WorldCreation/PreviewDifficultyExample2"
			);
			difficultyOptions.Add(exampleDifficulty);

			WorldCreationMenuOption exampleEvil = null;
			exampleEvil = new WorldCreationMenuOption(
				"ExampleMod:ExampleEvil",
				Mod.GetLocalization("WorldCreation.ExampleEvil.DisplayName"),
				Mod.GetLocalization("WorldCreation.ExampleEvil.Description"),
				Color.ForestGreen,
				"ExampleMod/Assets/Textures/WorldCreation/IconDifficultyExample",
				() => {
					// Use the automatically assigned TML range ID instead of reusing WorldEvilID.Crimson, preventing conflicts between mods.
					// Custom worldgen code can later check the saved header flag or WorldGen.WorldGenParam_Evil >= WorldEvilID.TML.
					WorldGen.WorldGenParam_Evil = exampleEvil.Type;
					creatingExampleEvilWorld = true;
				},
				() => creatingExampleEvilWorld,
				// byte.MaxValue means this is not a vanilla evil preview. The custom preview layer replaces Corruption/Crimson art.
				byte.MaxValue,
				() => creatingExampleEvilWorld = false,
				previewTexturePath: "ExampleMod/Assets/Textures/WorldCreation/PreviewEvilExample"
			);
			evilOptions.Add(exampleEvil);
		}

		public override void SaveWorldHeader(TagCompound tag) {
			if (creatingExampleSizeWorld && WorldGen.GetWorldSize() == 2)
				tag[HeaderKeySize] = ExampleSizeValue;

			if (creatingExampleDifficultyWorld && Main.GameMode >= GameModeID.TML)
				tag[HeaderKeyDifficulty] = ExampleDifficultyValue;

			if (creatingExampleEvilWorld && WorldGen.WorldGenParam_Evil >= WorldEvilID.TML)
				tag[HeaderKeyEvil] = ExampleEvilValue;
		}

		// SaveWorldHeader data is available in the world select menu without loading the world.
		// This is the recommended way to show custom difficulty text and custom world icons for these options.
		public override void ModifyWorldListDisplay(WorldFileData worldData, ref string difficultyText, ref Color difficultyColor, List<Asset<Texture2D>> icons) {
			if (!worldData.TryGetHeaderData(this, out TagCompound tag))
				return;

			if (tag.GetString(HeaderKeySize) == ExampleSizeValue) {
				// The example currently generates using Large dimensions, but the world select menu displays the selected custom size preset.
				worldData._worldSizeName = Mod.GetLocalization("WorldCreation.ExampleSize.DisplayName");
			}

			if (tag.GetString(HeaderKeyDifficulty) == ExampleDifficultyValue) {
				difficultyText = Mod.GetLocalization("WorldCreation.ExampleDifficulty.DisplayName").Value;
				difficultyColor = Color.Orange;
			}

			if (icons != null && tag.GetString(HeaderKeyEvil) == ExampleEvilValue) {
				icons.Clear();
				icons.Add(ModContent.Request<Texture2D>("ExampleMod/Assets/Textures/WorldCreation/IconExample", AssetRequestMode.ImmediateLoad));
			}
		}

	}
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.States;

//TODO: UIWorldSelect.TML.cs is almost the exact same thing. Please make these files share code.
public partial class UICharacterSelect : UIState
{
	// Added by TML.
	private static bool _currentlyMigratingFiles;

	// Individual

	private void AddIndividualPlayerMigrationButtons()
	{
		UIExpandablePanel playerMigrationPanel = new UIExpandablePanel();
		_playerList.Add(playerMigrationPanel);

		var playerMigrationPanelTitle = new UIText(Language.GetTextValue("tModLoader.MigrateIndividualPlayersHeader"));
		playerMigrationPanelTitle.Top.Set(4, 0);
		playerMigrationPanel.Append(playerMigrationPanelTitle);

		ModLoader.Config.UI.NestedUIList migratePlayerList = new ModLoader.Config.UI.NestedUIList();
		migratePlayerList.Width.Set(-22, 1f);
		migratePlayerList.Left.Set(0, 0f);
		migratePlayerList.Top.Set(30, 0);
		migratePlayerList.MinHeight.Set(300, 0f);
		migratePlayerList.ListPadding = 5f;
		playerMigrationPanel.VisibleWhenExpanded.Add(migratePlayerList);

		UIScrollbar scrollbar = new UIScrollbar();
		scrollbar.SetView(100f, 1000f);
		scrollbar.Height.Set(-42f, 1f);
		scrollbar.Top.Set(36f, 0f);
		scrollbar.Left.Pixels -= 0;
		scrollbar.HAlign = 1f;
		migratePlayerList.SetScrollbar(scrollbar);
		playerMigrationPanel.VisibleWhenExpanded.Add(scrollbar);

		// TODO: Do we need to do extra work for .plr files that have been renamed? Is that valid?
		// TODO: We could probably support cloud players as well, if we tried.
		// Vanilla and 1.3 paths are defaults, 1.4 TML paths are relative to current savepath.
		var otherPaths = new (string path, string message, int stabilityLevel)[] {
			(path: Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), "Players"), "Click to copy \"{0}\" over from Terraria", 0),
			(path: Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), "ModLoader", "Players"), "Click to copy \"{0}\" over from 1.3 tModLoader", 0),
			(path: Path.Combine(Main.SavePath, "..", Program.ReleaseFolder, "Players"), "Click to copy \"{0}\" over from 1.4-stable", 1),
			(path: Path.Combine(Main.SavePath, "..", Program.PreviewFolder, "Players"), "Click to copy \"{0}\" over from 1.4-preview", 2),
			(path: Path.Combine(Main.SavePath, "..", Program.DevFolder, "Players"), "Click to copy \"{0}\" over from 1.4-dev", 3),
		};

		int currentStabilityLevel = BuildInfo.Purpose switch {
			BuildInfo.BuildPurpose.Stable => 1,
			BuildInfo.BuildPurpose.Preview => 2,
			_ or BuildInfo.BuildPurpose.Dev => 3,
		};

		foreach (var (otherSaveFolderPath, message, stabilityLevel) in otherPaths) {
			if (stabilityLevel == currentStabilityLevel)
				continue;

			if (!Directory.Exists(otherSaveFolderPath))
				continue;

			string[] files = Directory.GetFiles(otherSaveFolderPath, "*.plr");
			int num2 = Math.Min(1000, files.Length);

			for (int i = 0; i < num2; i++) {
				string playerInThisPlayersPath = Path.Combine(Main.PlayerPath, Path.GetFileName(files[i]));

				if (File.Exists(playerInThisPlayersPath) && File.GetLastWriteTime(playerInThisPlayersPath) == File.GetLastWriteTime(files[i])) {
					continue;
				}

				PlayerFileData fileData = Player.GetFileData(files[i], cloudSave: false);

				if (fileData == null) {
					continue;
				}

				var migrateIndividualPlayerPanel = new UIPanel();
				migrateIndividualPlayerPanel.Width.Set(0, 1);
				migrateIndividualPlayerPanel.Height.Set(50, 0);

				float left = 0;

				if (stabilityLevel > currentStabilityLevel) {
					// TODO: Not necessarily newer...
					var warningImage = new UIHoverImage(UICommon.ButtonErrorTexture, "This player is from a newer tModLoader, it might not work") {
						Left = { Pixels = left },
						Top = { Pixels = 3 }
					};

					migrateIndividualPlayerPanel.Append(warningImage);

					left += warningImage.Width.Pixels + 6;
				}

				var playerWithSameName = Main.PlayerList.FirstOrDefault(x => x.Name == fileData.Name);

				if (playerWithSameName != null) {
					var warningImage = new UIHoverImage(UICommon.ButtonExclamationTexture, "A player with this name exists, it will be overwritten") {
						Left = { Pixels = left },
						Top = { Pixels = 3 }
					};

					migrateIndividualPlayerPanel.Append(warningImage);

					left += warningImage.Width.Pixels + 6;

					if (File.GetLastWriteTime(playerWithSameName.Path) > File.GetLastWriteTime(files[i])) {
						warningImage = new UIHoverImage(UICommon.ButtonExclamationTexture, "The existing player was last played more recently") {
							Left = { Pixels = left },
							Top = { Pixels = 3 }
						};

						migrateIndividualPlayerPanel.Append(warningImage);

						left += warningImage.Width.Pixels + 6;
					}
				}

				var migrateIndividualPlayerText = new UIText(string.Format(message, fileData.Name));

				migrateIndividualPlayerText.Width.Set(-left, 1);
				migrateIndividualPlayerText.Left.Set(left, 0);
				migrateIndividualPlayerText.Height.Set(0, 1);
				migrateIndividualPlayerText.OnLeftClick += (a, b) => {
					if (_currentlyMigratingFiles) {
						return;
					}

					_currentlyMigratingFiles = true;
					migrateIndividualPlayerText.SetText(Language.GetText("tModLoader.MigratingWorldsText"));

					Task.Factory.StartNew(
						() => ExecuteIndividualPlayerMigration(fileData, otherSaveFolderPath),
						TaskCreationOptions.PreferFairness
					);
				};

				migrateIndividualPlayerPanel.Append(migrateIndividualPlayerText);

				migratePlayerList.Add(migrateIndividualPlayerPanel);
			}
		}
	}

	private static void ExecuteIndividualPlayerMigration(PlayerFileData fileData, string otherSaveFolderPath)
	{
		try {
			// Delete existing player files of the same name
			string playerFileName = Path.GetFileNameWithoutExtension(fileData.Path);
			var playerFiles = Directory.GetFiles(Main.PlayerPath, $"{playerFileName}.*")
				.Where(s => s.EndsWith(".plr") || s.EndsWith(".tplr") || s.EndsWith(".bak"));

			foreach (string existingPlayerFile in playerFiles) {
				File.Delete(existingPlayerFile);
			}

			string existingPlayerMapPath = Path.Combine(Main.PlayerPath, Path.GetFileNameWithoutExtension(fileData.Path));

			if (Directory.Exists(existingPlayerMapPath)) {
				Directory.Delete(existingPlayerMapPath, true);
			}

			var otherPlayerFiles = Directory.GetFiles(otherSaveFolderPath, $"{playerFileName}.*")
				.Where(s => s.EndsWith(".plr") || s.EndsWith(".tplr") || s.EndsWith(".bak"));

			foreach (string otherPlayerFile in otherPlayerFiles) {
				File.Copy(otherPlayerFile, Path.Combine(Main.PlayerPath, Path.GetFileName(otherPlayerFile)), true);
			}

			// Copy map files
			string playerMapPath = Path.Combine(otherSaveFolderPath, Path.GetFileNameWithoutExtension(fileData.Path));

			if (Directory.Exists(playerMapPath)) {
				FileUtilities.CopyFolder(playerMapPath, existingPlayerMapPath);
			}
		}
		catch (Exception e) {
			Logging.tML.Error(Language.GetText("tModLoader.MigratePlayersException"), e);
		}

		_currentlyMigratingFiles = false;
		Main.menuMode = 1;
	}

	// Automatic

	private void AddAutomaticPlayerMigrationButtons()
	{
		string vanillaPlayersPath = Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), "Players");

		if (!Directory.Exists(vanillaPlayersPath) || !Directory.GetFiles(vanillaPlayersPath, "*.plr").Any()) {
			return;
		}

		var autoMigrateButton = new UIPanel();

		autoMigrateButton.Width.Set(0, 1);
		autoMigrateButton.Height.Set(50, 0);

		var migrateText = new UIText(!_currentlyMigratingFiles
			? Language.GetText("tModLoader.MigratePlayersText")
			// Use of world text here is intentional.
			: Language.GetText("tModLoader.MigratingWorldsText"));

		autoMigrateButton.OnLeftClick += (a, b) => {
			if (_currentlyMigratingFiles)
				return;

			_currentlyMigratingFiles = true;

			migrateText.SetText(Language.GetText("tModLoader.MigratingWorldsText"));

			Task.Factory.StartNew(
				() => ExecuteAutomaticPlayerMigration(vanillaPlayersPath),
				TaskCreationOptions.PreferFairness
			);
		};

		autoMigrateButton.Append(migrateText);

		_playerList.Add(autoMigrateButton);

		var noPlayersMessage = new UIText(Language.GetTextValue("tModLoader.MigratePlayersMessage", Program.SaveFolderName));
		noPlayersMessage.Width.Set(0, 1);
		noPlayersMessage.Height.Set(300, 0);
		noPlayersMessage.MarginTop = 20f;
		noPlayersMessage.OnLeftClick += (a, b) => {
			Utils.OpenFolder(Main.PlayerPath);
			Utils.OpenFolder(vanillaPlayersPath);
		};

		_playerList.Add(noPlayersMessage);
	}

	private static void ExecuteAutomaticPlayerMigration(string vanillaPlayersPath)
	{
		var vanillaPlayerFiles = Directory.GetFiles(vanillaPlayersPath, "*.*")
			.Where(s => s.EndsWith(".plr") || s.EndsWith(".tplr") || s.EndsWith(".bak"));

		foreach (string file in vanillaPlayerFiles) {
			File.Copy(file, Path.Combine(Main.PlayerPath, Path.GetFileName(file)), true); // old .bak files might exist, need to have overwrite
		}

		// Copy map files
		foreach (string mapDir in Directory.GetDirectories(vanillaPlayersPath)) {
			var mapFiles = Directory.GetFiles(mapDir, "*.*")
				.Where(s => s.EndsWith(".map") || s.EndsWith(".tmap"));

			try {
				foreach (string mapFile in mapFiles) {
					string mapFileDir = Path.Combine(Main.PlayerPath, Directory.GetParent(mapFile).Name);
					Directory.CreateDirectory(mapFileDir);
					File.Copy(mapFile, Path.Combine(mapFileDir, Path.GetFileName(mapFile)), true);
				}
			}
			catch (Exception e) {
				Logging.tML.Error(Language.GetText("tModLoader.MigratePlayersException"), e);
			}
		}

		_currentlyMigratingFiles = false;
		Main.menuMode = 1;
	}
}

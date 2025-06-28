using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.States;

//TODO: UICharacterSelect.TML.cs is almost the exact same thing. Please make these files share code.
partial class UIWorldSelect
{
	// Added by TML.
	private static bool _currentlyMigratingFiles;

	// Individual
	private static UIExpandablePanel _migrationPanel;
	private static ModLoader.Config.UI.NestedUIList migrateWorldList;
	private static bool migratableWorldsLoaded = false;

	private void InitializeMigrationPanel()
	{
		_migrationPanel = new UIExpandablePanel();
		_migrationPanel.OnExpanded += _migrationPanel_OnExpanded;
		//_worldList.Add(_migrationPanel);

		var playerMigrationPanelTitle = new UIText(Language.GetTextValue("tModLoader.MigrateIndividualWorldsHeader"));
		playerMigrationPanelTitle.Top.Set(4, 0);
		_migrationPanel.Append(playerMigrationPanelTitle);

		migrateWorldList = new ModLoader.Config.UI.NestedUIList();
		migrateWorldList.Width.Set(-22, 1f);
		migrateWorldList.Left.Set(0, 0f);
		migrateWorldList.Top.Set(30, 0);
		migrateWorldList.MinHeight.Set(300, 0f);
		migrateWorldList.ListPadding = 5f;
		_migrationPanel.VisibleWhenExpanded.Add(migrateWorldList);

		UIScrollbar scrollbar = new UIScrollbar();
		scrollbar.SetView(100f, 1000f);
		scrollbar.Height.Set(-42f, 1f);
		scrollbar.Top.Set(36f, 0f);
		scrollbar.Left.Pixels -= 0;
		scrollbar.HAlign = 1f;
		migrateWorldList.SetScrollbar(scrollbar);
		//DataListElement.Append(scrollbar);

		_migrationPanel.VisibleWhenExpanded.Add(scrollbar);
	}

	private void ActivateMigrationPanel()
	{
		migrateWorldList.Clear();
		migratableWorldsLoaded = false;
		_migrationPanel.Collapse();
	}

	private void _migrationPanel_OnExpanded()
	{
		if (migratableWorldsLoaded)
			return;
		migratableWorldsLoaded = true;
		LoadMigratableWorlds();
	}

	private void LoadMigratableWorlds()
	{
		// TODO: Do we need to do extra work for .wld files that have been renamed? Is that valid?
		// Vanilla and 1.3 paths are defaults, 1.4 TML paths are relative to current savepath.
		var otherPaths = FileUtilities.GetAlternateSavePathFiles("Worlds");

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

			string[] files = Directory.GetFiles(otherSaveFolderPath, "*.wld");
			int num2 = Math.Min(1000, files.Length);

			for (int i = 0; i < num2; i++) {
				string worldInThisWorldsPath = Path.Combine(Main.WorldPath, Path.GetFileName(files[i]));

				if (File.Exists(worldInThisWorldsPath) && File.GetLastWriteTime(worldInThisWorldsPath) == File.GetLastWriteTime(files[i]))
					continue;

				WorldFileData fileData = WorldFile.GetAllMetadata(files[i], cloudSave: false);

				if (fileData == null)
					continue;

				var migrateIndividualWorldPanel = new UIPanel();
				migrateIndividualWorldPanel.Width.Set(0, 1);
				migrateIndividualWorldPanel.Height.Set(50, 0);

				float left = 0;

				if (stabilityLevel > currentStabilityLevel) {
					// TODO: Not necessarily newer...
					var warningImage = new UIHoverImage(UICommon.ButtonErrorTexture, Language.GetTextValue("tModLoader.WorldFromNewerTModMightNotWork")) {
						UseTooltipMouseText = true,
						Left = { Pixels = left },
						Top = { Pixels = 3 }
					};

					migrateIndividualWorldPanel.Append(warningImage);

					left += warningImage.Width.Pixels + 6;
				}

				var worldWithSameName = Main.WorldList.FirstOrDefault(x => x.Name == fileData.Name);

				if (worldWithSameName != null) {
					var warningImage = new UIHoverImage(UICommon.ButtonExclamationTexture, Language.GetTextValue("tModLoader.WorldWithThisNameExistsWillBeOverwritten")) {
						UseTooltipMouseText = true,
						Left = { Pixels = left },
						Top = { Pixels = 3 }
					};

					migrateIndividualWorldPanel.Append(warningImage);

					left += warningImage.Width.Pixels + 6;

					if (File.GetLastWriteTime(worldWithSameName.Path) > File.GetLastWriteTime(files[i])) {
						warningImage = new UIHoverImage(UICommon.ButtonExclamationTexture, Language.GetTextValue("tModLoader.ExistingWorldPlayedMoreRecently")) {
							UseTooltipMouseText = true,
							Left = { Pixels = left },
							Top = { Pixels = 3 }
						};

						migrateIndividualWorldPanel.Append(warningImage);

						left += warningImage.Width.Pixels + 6;
					}
				}

				var migrateIndividualWorldText = new UIText(string.Format(message, fileData.Name));

				migrateIndividualWorldText.Width.Set(-left, 1);
				migrateIndividualWorldText.Left.Set(left, 0);
				migrateIndividualWorldText.Height.Set(0, 1);

				migrateIndividualWorldText.OnLeftClick += (a, b) => {
					if (_currentlyMigratingFiles)
						return;

					_currentlyMigratingFiles = true;
					migrateIndividualWorldText.SetText(Language.GetText("tModLoader.MigratingWorldsText"));

					Task.Factory.StartNew(
						() => ExecuteIndividualWorldMigration(fileData, otherSaveFolderPath),
						TaskCreationOptions.PreferFairness
					);
				};

				migrateIndividualWorldPanel.Append(migrateIndividualWorldText);

				migrateWorldList.Add(migrateIndividualWorldPanel);
			}
		}
	}

	private static void ExecuteIndividualWorldMigration(WorldFileData fileData, string otherSaveFolderPath)
	{
		try {
			// Delete existing world files of the same name
			string worldFileName = Path.GetFileNameWithoutExtension(fileData.Path);
			var worldFiles = Directory.GetFiles(Main.WorldPath, $"{worldFileName}.*")
				.Where(s => s.EndsWith(".wld") || s.EndsWith(".twld") || s.EndsWith(".bak"));

			foreach (string existingWorldFile in worldFiles) {
				File.Delete(existingWorldFile);
			}

			var otherWorldFiles = Directory.GetFiles(otherSaveFolderPath, $"{worldFileName}.*")
				.Where(s => s.EndsWith(".wld") || s.EndsWith(".twld") || s.EndsWith(".bak"));

			foreach (string otherWorldFile in otherWorldFiles) {
				File.Copy(otherWorldFile, Path.Combine(Main.WorldPath, Path.GetFileName(otherWorldFile)), true);
			}
		}
		catch (Exception e) {
			Logging.tML.Error(Language.GetText("tModLoader.MigratePlayersException"), e);
		}

		_currentlyMigratingFiles = false;
		Main.menuMode = 6;
	}

	// Automatic

	private void AddAutomaticWorldMigrationButtons()
	{
		string vanillaWorldsPath = Path.Combine(ReLogic.OS.Platform.Get<ReLogic.OS.IPathService>().GetStoragePath("Terraria"), "Worlds");

		if (!Directory.Exists(vanillaWorldsPath) || !Directory.GetFiles(vanillaWorldsPath, "*.wld").Any())
			return;

		var autoMigrateButton = new UIPanel();
		autoMigrateButton.Width.Set(0, 1);
		autoMigrateButton.Height.Set(50, 0);

		var migrateText = new UIText(!_currentlyMigratingFiles
			? Language.GetText("tModLoader.MigrateWorldsText")
			: Language.GetText("tModLoader.MigratingWorldsText"));

		autoMigrateButton.OnLeftClick += (a, b) => {
			if (_currentlyMigratingFiles)
				return;

			_currentlyMigratingFiles = true;
			migrateText.SetText(Language.GetText("tModLoader.MigratingWorldsText"));

			Task.Factory.StartNew(
				() => ExecuteAutomaticWorldMigration(vanillaWorldsPath),
				TaskCreationOptions.PreferFairness
			);
		};

		autoMigrateButton.Append(migrateText);

		_worldList.Add(autoMigrateButton);

		var noWorldsMessage = new UIText(Language.GetTextValue("tModLoader.MigrateWorldsMessage", Program.SaveFolderName));
		noWorldsMessage.Width.Set(0, 1);
		noWorldsMessage.Height.Set(300, 0);
		noWorldsMessage.MarginTop = 20f;
		noWorldsMessage.OnLeftClick += (a, b) => {
			Utils.OpenFolder(Main.WorldPath);
			Utils.OpenFolder(vanillaWorldsPath);
		};

		_worldList.Add(noWorldsMessage);
	}

	private static void ExecuteAutomaticWorldMigration(string vanillaWorldsPath)
	{
		var vanillaWorldFiles = Directory.GetFiles(vanillaWorldsPath, "*.*")
			.Where(s => s.EndsWith(".wld") || s.EndsWith(".twld") || s.EndsWith(".bak"));

		foreach (string file in vanillaWorldFiles) {
			File.Copy(file, Path.Combine(Main.WorldPath, Path.GetFileName(file)), true);
		}

		_currentlyMigratingFiles = false;
		Main.menuMode = 6;
	}
}

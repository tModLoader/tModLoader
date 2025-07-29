using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.Elements;

public partial class UIWorldListItem : AWorldListItem
{
	private ulong _fileSize;
	private Asset<Texture2D> _configTexture;
	UIText warningLabel; // top right label.

	private void InitializeTmlFields(WorldFileData data)
	{
		_fileSize = (ulong)FileUtilities.GetFileSize(data.Path, data.IsCloudSave);
	}

	private void LoadTmlTextures()
	{
		_configTexture = ModLoader.UI.UICommon.ButtonConfigTexture;
	}

	private void AddTmlElements(WorldFileData data, ref float offset)
	{
		/*
		if (ConfigManager.Configs.Count > 0) {
			UIImageButton configButton = new UIImageButton(_configTexture);
			configButton.VAlign = 1f;
			configButton.Left.Set(offset, 0f);
			configButton.OnClick += new UIElement.MouseEvent(ConfigButtonClick);
			configButton.OnMouseOver += new UIElement.MouseEvent(ConfigMouseOver);
			configButton.OnMouseOut += new UIElement.MouseEvent(ButtonMouseOut);
			Append(configButton);
			offset += 24f;
		}
		*/

		warningLabel = new UIText("", 1f, false) {
			VAlign = 0f,
			HAlign = 1f
		};

		float topRightButtonsLeftPixels = 0f;
		warningLabel.Top.Set(3f, 0f);

		Append(warningLabel);

		// TODO: Mostly duplicate code with UICharacterListItem, need to find good place for shared method
		// TODO: Should we also show a separate button for mods that were present during worldgen but aren't enabled?
		if (data.usedMods != null) {
			string[] currentModNames = ModLoader.ModLoader.Mods.Select(m => m.Name).ToArray();
			var missingMods = data.usedMods.Except(currentModNames).Select(ModOrganizer.GetDisplayNameCleanFromLocalModsOrDefaultToModName).ToList();
			var newMods = currentModNames.Except(new[] { "ModLoader" }).Except(data.usedMods).Select(ModOrganizer.GetDisplayNameCleanFromLocalModsOrDefaultToModName).ToList();
			bool checkModPack = System.IO.Path.GetFileNameWithoutExtension(ModOrganizer.ModPackActive) != data.modPack;

			if (checkModPack || missingMods.Count > 0 || newMods.Count > 0) {
				UIImageButton modListWarning = new UIImageButton(UICommon.ButtonErrorTexture) {
					VAlign = 0f,
					HAlign = 1f,
					Top = new StyleDimension(-2, 0),
					Left = new StyleDimension(topRightButtonsLeftPixels, 0)
				};
				topRightButtonsLeftPixels -= 24;

				System.Text.StringBuilder fullSB = new System.Text.StringBuilder(Language.GetTextValue("tModLoader.ModsDifferentSinceLastPlay"));
				System.Text.StringBuilder shortSB = new System.Text.StringBuilder();

				string Separator()
					=> shortSB.Length != 0 ? "; " : null;

				if (checkModPack) {
					string pack = data.modPack;
					if (string.IsNullOrEmpty(pack))
						pack = "None";

					shortSB.Append(Separator() + Language.GetTextValue("tModLoader.ModPackMismatch", pack));
					fullSB.Append("\n" + Language.GetTextValue("tModLoader.ModPackMismatch", pack));
				}

				if (missingMods.Count > 0) {
					shortSB.Append(Separator() + (missingMods.Count > 1 ? Language.GetTextValue("tModLoader.MissingXMods", missingMods.Count) : Language.GetTextValue("tModLoader.Missing1Mod")));
					fullSB.Append("\n" + Language.GetTextValue("tModLoader.MissingModsListing", string.Join("\n", missingMods.Select(x => "- " + x))));
				}

				if (newMods.Count > 0) {
					shortSB.Append(Separator() + (newMods.Count > 1 ? Language.GetTextValue("tModLoader.NewXMods", newMods.Count) : Language.GetTextValue("tModLoader.New1Mod")));
					fullSB.Append("\n" + Language.GetTextValue("tModLoader.NewModsListing", string.Join("\n", newMods.Select(x => "- " + x))));
				}

				if (shortSB.Length != 0) {
					shortSB.Append('.');
				}

				string warning = shortSB.ToString();
				string fullWarning = fullSB.ToString();

				modListWarning.OnMouseOver += (a, b) => warningLabel.SetText(warning);
				modListWarning.OnMouseOut += (a, b) => warningLabel.SetText("");
				modListWarning.OnLeftClick += (a, b) => {
					Interface.infoMessage.Show(fullWarning, 888, Main._worldSelectMenu);
				};

				Append(modListWarning);
			}
		}

		if (data.ModSaveErrors.Any()) {
			UIImageButton modSaveErrorWarning = new UIImageButton(UICommon.ButtonErrorTexture) {
				VAlign = 0f,
				HAlign = 1f,
				Top = new StyleDimension(-2, 0),
				Left = new StyleDimension(topRightButtonsLeftPixels, 0)
			};
			topRightButtonsLeftPixels -= 24;

			string warning = Language.GetTextValue("tModLoader.ViewSaveErrorMessage");
			string fullError = Utils.CreateSaveErrorMessage("tModLoader.WorldCustomDataSaveFail", data.ModSaveErrors, doubleNewline: true).ToString();
			modSaveErrorWarning.OnMouseOver += (a, b) => warningLabel.SetText(warning);
			modSaveErrorWarning.OnMouseOut += (a, b) => warningLabel.SetText("");
			modSaveErrorWarning.OnLeftClick += (a, b) => {
				Interface.infoMessage.Show(fullError, 888, Main._worldSelectMenu);
			};

			Append(modSaveErrorWarning);
		}

		warningLabel.Left.Set(topRightButtonsLeftPixels - 6, 0f);
	}

	internal static Action PlayReload()
	{
		// Main.ActivePlayerFileData gets cleared during reload
		string path = Main.ActivePlayerFileData.Path;
		bool isCloudSave = Main.ActivePlayerFileData.IsCloudSave;

		return () => {
			// Re-select the current player
			Player.GetFileData(path, isCloudSave).SetAsActive();
			WorldGen.playWorld();
		};
	}

	private void ConfigMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		_buttonLabel.SetText("Edit World Config");
	}

	private void ConfigButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{

	}
}

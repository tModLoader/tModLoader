using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.Utilities;

namespace Terraria.GameContent.UI.Elements;

partial class UICharacterListItem
{
	private Asset<Texture2D> _errorTexture;
	private Asset<Texture2D> _configTexture;
	private ulong _fileSize;

	private void InitializeTmlFields(PlayerFileData data)
	{
		_errorTexture = UICommon.ButtonErrorTexture;
		_configTexture = UICommon.ButtonConfigTexture;
		_fileSize = (ulong)FileUtilities.GetFileSize(data.Path, data.IsCloudSave);
	}

	private void AddTmlElements(PlayerFileData data)
	{
		if (data.customDataFail != null) {
			var errorButton = new UIImageButton(_errorTexture) {
				VAlign = 1f,
				HAlign = 1f
			};

			errorButton.Left.Set(-24f, 0f);
			errorButton.OnLeftClick += new MouseEvent(ErrorButtonClick);
			errorButton.OnMouseOver += new MouseEvent(ErrorMouseOver);
			errorButton.OnMouseOut += new MouseEvent(DeleteMouseOut);

			Append(errorButton);
		}

		if (data.Player.usedMods != null) {
			string[] currentModNames = ModLoader.ModLoader.Mods.Select(m => m.Name).ToArray();
			var missingMods = data.Player.usedMods.Except(currentModNames).ToList();
			var newMods = currentModNames.Except(new[] { "ModLoader" }).Except(data.Player.usedMods).ToList();
			bool checkModPack = System.IO.Path.GetFileNameWithoutExtension(ModLoader.Core.ModOrganizer.ModPackActive) != data.Player.modPack;

			if (checkModPack || missingMods.Count > 0 || newMods.Count > 0) {
				UIText warningLabel = new UIText("", 1f, false) {
					VAlign = 0f,
					HAlign = 1f
				};

				warningLabel.Left.Set(-30f, 0f);
				warningLabel.Top.Set(3f, 0f);

				Append(warningLabel);

				UIImageButton modListWarning = new UIImageButton(_errorTexture) {
					VAlign = 0f,
					HAlign = 1f
				};

				modListWarning.Top.Set(-2f, 0f);

				System.Text.StringBuilder fullSB = new System.Text.StringBuilder(Language.GetTextValue("tModLoader.ModsDifferentSinceLastPlay"));
				System.Text.StringBuilder shortSB = new System.Text.StringBuilder();

				string Separator()
					=> shortSB.Length != 0 ? "; " : null;

				if (checkModPack) {
					string pack = data.Player.modPack;
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
					Interface.infoMessage.Show(fullWarning, 888, Main._characterSelectMenu);
				};

				Append(modListWarning);
			}
		}

		/*
		int buttonLabelLeft = 80;

		if (ConfigManager.Configs.Count > 0) {
			UIImageButton configButton = new UIImageButton(this._configTexture);
			configButton.VAlign = 1f;
			configButton.Left.Set(76, 0f);
			configButton.OnLeftClick += new UIElement.MouseEvent(this.ConfigButtonClick);
			configButton.OnMouseOver += new UIElement.MouseEvent(this.ConfigMouseOver);
			configButton.OnMouseOut += new UIElement.MouseEvent(this.ButtonMouseOut);
			base.Append(configButton);
		
			buttonLabelLeft += 24;
		}
		*/
	}

	private void ErrorMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		_deleteButtonLabel.SetText(_data.customDataFail.modName + " Error");
	}

	private void ConfigMouseOver(UIMouseEvent evt, UIElement listeningElement)
	{
		_buttonLabel.SetText("Edit Player Config");
	}

	private void ErrorButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{
		Interface.infoMessage.Show(Language.GetTextValue("tModLoader.PlayerCustomDataFail") + "\n\n" + _data.customDataFail.InnerException, 888, Main._characterSelectMenu);
	}

	private void ConfigButtonClick(UIMouseEvent evt, UIElement listeningElement)
	{
	}
}

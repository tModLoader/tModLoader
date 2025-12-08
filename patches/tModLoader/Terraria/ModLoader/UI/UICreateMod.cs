using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

public class UICreateMod : UIState, IHaveBackButtonCommand
{
	private UIElement _baseElement;
	private UITextPanel<string> _messagePanel;
	private UIFocusInputTextField _modName;
	private UIFocusInputTextField _modDisplayName;
	private UIFocusInputTextField _modAuthor;
	private UIFocusInputTextField _basicSword;
	public UIState PreviousUIState { get; set; }

	public override void OnInitialize()
	{
		_baseElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f
		};
		Append(_baseElement);

		var mainPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -110, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
			PaddingTop = 0f
		};
		_baseElement.Append(mainPanel);

		var uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15);
		_baseElement.Append(uITextPanel);

		_messagePanel = new UITextPanel<string>(Language.GetTextValue("")) {
			Width = { Percent = 1f },
			Height = { Pixels = 25 },
			VAlign = 1f,
			Top = { Pixels = -20 }
		};
		// Appended dynamically

		var buttonBack = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
			Width = { Pixels = -10, Percent = 0.5f },
			Height = { Pixels = 25 },
			VAlign = 1f,
			Top = { Pixels = -65 }
		}.WithFadedMouseOver();
		buttonBack.OnLeftClick += BackClick;
		_baseElement.Append(buttonBack);

		var buttonCreate = new UITextPanel<string>(Language.GetTextValue("LegacyMenu.28")); // Create
		buttonCreate.CopyStyle(buttonBack);
		buttonCreate.HAlign = 1f;
		buttonCreate.WithFadedMouseOver();
		buttonCreate.OnLeftClick += OKClick;
		_baseElement.Append(buttonCreate);

		float top = 16;
		_modName = createAndAppendTextInputWithLabel(Language.GetTextValue("tModLoader.CreateModName"), Language.GetTextValue("tModLoader.ModConfigTypeHere"));
		_modName.OnTextChange += (a, b) => { _modName.SetText(_modName.CurrentString.Replace(" ", "")); };
		_modDisplayName = createAndAppendTextInputWithLabel(Language.GetTextValue("tModLoader.CreateModDisplayName"), Language.GetTextValue("tModLoader.ModConfigTypeHere"));
		_modAuthor = createAndAppendTextInputWithLabel(Language.GetTextValue("tModLoader.CreateModAuthor"), Language.GetTextValue("tModLoader.ModConfigTypeHere"));
		_basicSword = createAndAppendTextInputWithLabel(Language.GetTextValue("tModLoader.CreateModBasicSword"), Language.GetTextValue("tModLoader.CreateModBasicSwordHint"));
		_modName.OnTab += (a, b) => _modDisplayName.Focused = true;
		_modDisplayName.OnTab += (a, b) => _modAuthor.Focused = true;
		_modAuthor.OnTab += (a, b) => _basicSword.Focused = true;
		_basicSword.OnTab += (a, b) => _modName.Focused = true;

		UIFocusInputTextField createAndAppendTextInputWithLabel(string label, string hint)
		{
			var panel = new UIPanel();
			panel.SetPadding(0);
			panel.Width.Set(0, 1f);
			panel.Height.Set(40, 0f);
			panel.Top.Set(top, 0f);
			top += 46;

			var modNameText = new UIText(label) {
				Left = { Pixels = 10 },
				Top = { Pixels = 10 }
			};

			panel.Append(modNameText);

			var textBoxBackground = new UIPanel();
			textBoxBackground.SetPadding(0);
			textBoxBackground.Top.Set(6f, 0f);
			textBoxBackground.Left.Set(0, .5f);
			textBoxBackground.Width.Set(0, .5f);
			textBoxBackground.Height.Set(30, 0f);
			panel.Append(textBoxBackground);

			var uIInputTextField = new UIFocusInputTextField(hint) {
				UnfocusOnTab = true
			};
			uIInputTextField.Top.Set(5, 0f);
			uIInputTextField.Left.Set(10, 0f);
			uIInputTextField.Width.Set(-20, 1f);
			uIInputTextField.Height.Set(20, 0);
			textBoxBackground.Append(uIInputTextField);

			mainPanel.Append(panel);

			return uIInputTextField;
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		_modName.SetText("");
		_basicSword.SetText("");
		_modDisplayName.SetText("");
		_modAuthor.SetText("");
		_messagePanel.SetText("");
		_modName.Focused = true;
	}

	private string lastKnownMessage = "";
	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (lastKnownMessage != _messagePanel.Text) {
			lastKnownMessage = _messagePanel.Text;
			if (string.IsNullOrEmpty(_messagePanel.Text)) {
				_baseElement.RemoveChild(_messagePanel);
			}
			else {

				_baseElement.Append(_messagePanel);
			}
		}
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
	}

	public void HandleBackButtonUsage()
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		Main.menuMode = Interface.modSourcesID;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}

	private void OKClick(UIMouseEvent evt, UIElement listeningElement)
	{
		try {
			string modNameTrimmed = _modName.CurrentString.Trim();
			string basicSwordTrimmed = _basicSword.CurrentString.Trim();
			string sourceFolder = Path.Combine(ModCompile.ModSourcePath, modNameTrimmed);
			var provider = CodeDomProvider.CreateProvider("C#");

			if (Directory.Exists(sourceFolder))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModFolderAlreadyExists"));
			else if (!provider.IsValidIdentifier(modNameTrimmed))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModNameInvalid"));
			else if (modNameTrimmed.Equals("Mod", StringComparison.InvariantCultureIgnoreCase) || modNameTrimmed.Equals("ModLoader", StringComparison.InvariantCultureIgnoreCase) || modNameTrimmed.Equals("tModLoader", StringComparison.InvariantCultureIgnoreCase))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModNameReserved"));
			else if (!string.IsNullOrEmpty(basicSwordTrimmed) && !provider.IsValidIdentifier(basicSwordTrimmed))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModBasicSwordInvalid"));
			else if (string.IsNullOrWhiteSpace(_modDisplayName.CurrentString))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModDisplayNameEmpty"));
			else if (string.IsNullOrWhiteSpace(_modAuthor.CurrentString))
				_messagePanel.SetText(Language.GetTextValue("tModLoader.CreateModAuthorEmpty"));
			else {
				Directory.CreateDirectory(sourceFolder);

				SourceManagement.WriteModTemplate(sourceFolder, GetModTemplateArguments());

				Utils.OpenFolder(sourceFolder);
				SoundEngine.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
			}
		}
		catch (Exception e) {
			Logging.tML.Error(e);
			_messagePanel.SetText("There was an issue. Check client.log");
		}
	}

	private SourceManagement.TemplateParameters GetModTemplateArguments()
	{
		var result = new SourceManagement.TemplateParameters {
			ModName = _modName.CurrentString.Trim(),
			ModDisplayName = _modDisplayName.CurrentString,
			ModAuthor = _modAuthor.CurrentString.Trim(),
			ModVersion = "0.1",
			ItemName = _basicSword.CurrentString.Trim(),
		};
		result.ItemDisplayName = Regex.Replace(result.ItemName, "([A-Z])", " $1").Trim();

		return result;
	}
}

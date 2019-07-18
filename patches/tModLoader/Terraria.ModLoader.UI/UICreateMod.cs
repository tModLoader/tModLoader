using System;
using System.CodeDom.Compiler;
using System.IO;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public class UICreateMod : UIState {
		private UITextPanel<string> _messagePanel;
		private UIFocusInputTextField _modName;
		private UIFocusInputTextField _modDiplayName;
		private UIFocusInputTextField _modAuthor;
		private UIFocusInputTextField _basicSword;

		public override void OnInitialize() {
			var uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};
			Append(uIElement);

			var mainPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.MainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(mainPanel);

			var uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15);
			uIElement.Append(uITextPanel);

			_messagePanel = new UITextPanel<string>(Language.GetTextValue("")) {
				Width = { Percent = 1f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			};
			uIElement.Append(_messagePanel);

			var buttonBack = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			buttonBack.OnClick += BackClick;
			uIElement.Append(buttonBack);

			var buttonCreate = new UITextPanel<string>(Language.GetTextValue("LegacyMenu.28")); // Create
			buttonCreate.CopyStyle(buttonBack);
			buttonCreate.HAlign = 1f;
			buttonCreate.WithFadedMouseOver();
			buttonCreate.OnClick += OKClick;
			uIElement.Append(buttonCreate);

			float top = 16;
			_modName = createAndAppendTextInputWithLabel("ModName (no spaces)", "Type here");
			_modName.OnTextChange += (a, b) => { _modName.SetText(_modName.CurrentString.Replace(" ", "")); };
			_modDiplayName = createAndAppendTextInputWithLabel("Mod DisplayName", "Type here");
			_modAuthor = createAndAppendTextInputWithLabel("Mod Author", "Type here");
			_basicSword = createAndAppendTextInputWithLabel("BasicSword (no spaces)", "Leave Blank to Skip");
			_modName.OnTab += (a, b) => _modDiplayName.Focused = true;
			_modDiplayName.OnTab += (a, b) => _modAuthor.Focused = true;
			_modAuthor.OnTab += (a, b) => _basicSword.Focused = true;
			_basicSword.OnTab += (a, b) => _modName.Focused = true;

			UIFocusInputTextField createAndAppendTextInputWithLabel(string label, string hint) {
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

		public override void OnActivate() {
			base.OnActivate();
			_modName.SetText("");
			_modDiplayName.SetText("");
			_modAuthor.SetText("");
			_messagePanel.SetText("");
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modSourcesID;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			string modNameTrimmed = _modName.CurrentString.Trim();
			string basicSwordTrimmed = _basicSword.CurrentString.Trim();
			string sourceFolder = Path.Combine(ModCompile.ModSourcePath, modNameTrimmed);
			var provider = CodeDomProvider.CreateProvider("C#");
			if (Directory.Exists(sourceFolder))
				_messagePanel.SetText("Folder already exists");
			else if (!provider.IsValidIdentifier(modNameTrimmed))
				_messagePanel.SetText("ModName is invalid C# identifier. Remove spaces.");
			else if (!string.IsNullOrEmpty(basicSwordTrimmed) && !provider.IsValidIdentifier(basicSwordTrimmed))
				_messagePanel.SetText("BasicSword is invalid C# identifier. Remove spaces.");
			else if (string.IsNullOrWhiteSpace(_modDiplayName.CurrentString))
				_messagePanel.SetText("DisplayName can't be empty");
			else if (string.IsNullOrWhiteSpace(_modAuthor.CurrentString))
				_messagePanel.SetText("Author can't be empty");
			else if (string.IsNullOrWhiteSpace(_modAuthor.CurrentString))
				_messagePanel.SetText("Author can't be empty");
			else {
				Main.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
				Directory.CreateDirectory(sourceFolder);

				// TODO: verbatim line endings, localization.
				File.WriteAllText(Path.Combine(sourceFolder, "build.txt"), GetModBuild());
				File.WriteAllText(Path.Combine(sourceFolder, "description.txt"), GetModDescription());
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.cs"), GetModClass(modNameTrimmed));
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.csproj"), GetModCsproj(modNameTrimmed));
				string propertiesFolder = Path.Combine(sourceFolder, "Properties");
				Directory.CreateDirectory(propertiesFolder);
				File.WriteAllText(Path.Combine(propertiesFolder, $"launchSettings.json"), GetLaunchSettings());
				if (!string.IsNullOrEmpty(basicSwordTrimmed)) {
					string itemsFolder = Path.Combine(sourceFolder, "Items");
					Directory.CreateDirectory(itemsFolder);
					File.WriteAllText(Path.Combine(itemsFolder, $"{basicSwordTrimmed}.cs"), GetBasicSword(modNameTrimmed, basicSwordTrimmed));
					File.WriteAllBytes(Path.Combine(itemsFolder, $"{basicSwordTrimmed}.png"), ExampleSwordPNG);
				}
			}
		}

		// TODO Let's embed all these files
		private string GetModBuild() {
			return $"displayName = {_modDiplayName.CurrentString}" +
				$"{Environment.NewLine}author = {_modAuthor.CurrentString}" +
				$"{Environment.NewLine}version = 0.1";
		}

		private string GetModDescription() {
			return $"{_modDiplayName.CurrentString} is a pretty cool mod, it does...this. Modify this file with a description of your mod.";
		}

		private string GetModClass(string modNameTrimmed) {
			return
$@"using Terraria.ModLoader;

namespace {modNameTrimmed}
{{
	public class {modNameTrimmed} : Mod
	{{
		public {modNameTrimmed}()
		{{
		}}
	}}
}}";
		}

		internal string GetModCsproj(string modNameTrimmed) {
			return
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\references\tModLoader.targets"" />
  <PropertyGroup>
    <AssemblyName>{modNameTrimmed}</AssemblyName>
    <TargetFramework>net45</TargetFramework>
    <PlatformTarget>x86</PlatformTarget>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  <Target Name=""BuildMod"" AfterTargets=""Build"">
    <Exec Command=""&quot;$(tMLBuildServerPath)&quot; -build $(ProjectDir) -eac $(TargetPath) -define $(DefineConstants) -unsafe $(AllowUnsafeBlocks)"" />
  </Target>
</Project>";
		}

		internal string GetLaunchSettings() {
			return
$@"{{
  ""profiles"": {{
    ""Terraria"": {{
      ""commandName"": ""Executable"",
      ""executablePath"": ""$(tMLPath)"",
      ""workingDirectory"": ""$(TerrariaSteamPath)""
    }},
    ""TerrariaServer"": {{
      ""commandName"": ""Executable"",
      ""executablePath"": ""$(tMLServerPath)"",
      ""workingDirectory"": ""$(TerrariaSteamPath)""
    }}
  }}
}}";
		}

		internal string GetBasicSword(string modNameTrimmed, string basicSwordName) {
			return
$@"using Terraria.ID;
using Terraria.ModLoader;

namespace {modNameTrimmed}.Items
{{
	public class {basicSwordName} : ModItem
	{{
		public override void SetStaticDefaults() 
		{{
			// DisplayName.SetDefault(""{basicSwordName}""); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
			Tooltip.SetDefault(""This is a basic modded sword."");
		}}

		public override void SetDefaults() 
		{{
			item.damage = 50;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 2;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}}

		public override void AddRecipes() 
		{{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}}
	}}
}}";
		}

		// TODO: Why not embed this texture?
		private readonly byte[] ExampleSwordPNG = {
			0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
			0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x28,
			0x08, 0x06, 0x00, 0x00, 0x00, 0x8C, 0xFE, 0xB8, 0x6D, 0x00, 0x00, 0x00,
			0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xAE, 0xCE, 0x1C, 0xE9, 0x00, 0x00,
			0x00, 0x06, 0x62, 0x4B, 0x47, 0x44, 0x00, 0xFF, 0x00, 0xFF, 0x00, 0xFF,
			0xA0, 0xBD, 0xA7, 0x93, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73,
			0x00, 0x00, 0x0B, 0x13, 0x00, 0x00, 0x0B, 0x13, 0x01, 0x00, 0x9A, 0x9C,
			0x18, 0x00, 0x00, 0x00, 0x07, 0x74, 0x49, 0x4D, 0x45, 0x07, 0xDF, 0x07,
			0x08, 0x02, 0x3A, 0x04, 0x79, 0x68, 0x3E, 0xAB, 0x00, 0x00, 0x00, 0x1D,
			0x69, 0x54, 0x58, 0x74, 0x43, 0x6F, 0x6D, 0x6D, 0x65, 0x6E, 0x74, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x64, 0x20,
			0x77, 0x69, 0x74, 0x68, 0x20, 0x47, 0x49, 0x4D, 0x50, 0x64, 0x2E, 0x65,
			0x07, 0x00, 0x00, 0x00, 0xBB, 0x49, 0x44, 0x41, 0x54, 0x58, 0xC3, 0xED,
			0xD6, 0xC9, 0x0D, 0xC4, 0x20, 0x0C, 0x05, 0xD0, 0xEF, 0x28, 0x55, 0xB8,
			0x26, 0x44, 0x1D, 0xD4, 0x87, 0xA8, 0x89, 0x36, 0x32, 0x97, 0x44, 0x62,
			0x22, 0x26, 0x8B, 0x08, 0x0C, 0x0E, 0xF6, 0xD1, 0xE6, 0xF4, 0xE4, 0x05,
			0x42, 0x27, 0xC1, 0xCC, 0x4B, 0x2E, 0x3F, 0xA1, 0xF3, 0xA0, 0x5E, 0xE4,
			0xBC, 0xF7, 0x00, 0x80, 0x18, 0x23, 0x00, 0xC0, 0x39, 0x27, 0x43, 0x70,
			0xEE, 0x4D, 0x8E, 0x99, 0xB5, 0x07, 0x8B, 0xE4, 0xAC, 0xB5, 0x5B, 0x9E,
			0x54, 0xB0, 0x44, 0x4E, 0x7B, 0xB0, 0x54, 0x4E, 0x05, 0x4B, 0xE5, 0xC6,
			0x15, 0x7C, 0x4A, 0x6E, 0x3C, 0xC1, 0xA7, 0xE5, 0xC6, 0x11, 0xAC, 0x25,
			0xF7, 0x7E, 0xC1, 0xDA, 0x72, 0xF2, 0x04, 0xF7, 0x3F, 0xD9, 0x4D, 0xE4,
			0x5F, 0x72, 0x22, 0x05, 0x97, 0x5C, 0x2D, 0x11, 0x6A, 0x2A, 0x27, 0x57,
			0xD0, 0x18, 0x03, 0x00, 0x08, 0x21, 0x7C, 0x3D, 0x6C, 0x2D, 0x27, 0x46,
			0x70, 0x4E, 0xA6, 0x96, 0x56, 0xB9, 0xAC, 0x64, 0x6B, 0x39, 0xF9, 0x97,
			0xE4, 0xAC, 0x27, 0x6B, 0xCB, 0xC9, 0x15, 0xFC, 0x25, 0x97, 0x91, 0xA4,
			0xA3, 0x8B, 0xA3, 0x7B, 0x30, 0x99, 0x5E, 0xBA, 0x52, 0x1F, 0x57, 0xF0,
			0xAE, 0xCC, 0xFE, 0x66, 0xAB, 0x60, 0x6B, 0x91, 0xD7, 0x09, 0x7E, 0x00,
			0x64, 0x8C, 0xC4, 0xAB, 0x0D, 0xC4, 0xCA, 0x08, 0x00, 0x00, 0x00, 0x00,
			0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
		};

	}
}

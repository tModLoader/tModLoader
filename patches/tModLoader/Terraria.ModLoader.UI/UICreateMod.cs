using System;
using System.CodeDom.Compiler;
using System.IO;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	class UICreateMod : UIState
	{
		UITextPanel<string> messagePanel;
		UIFocusInputTextField modName;
		UIFocusInputTextField modDiplayName;
		UIFocusInputTextField modAuthor;

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
				BackgroundColor = UICommon.mainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(mainPanel);

			var uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBCreateMod"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15);
			uIElement.Append(uITextPanel);

			messagePanel = new UITextPanel<string>(Language.GetTextValue("")) {
				Width = { Pixels = -10, Percent = 1f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			};
			uIElement.Append(messagePanel);

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
			modName = createAndAppendTextInputWithLabel("ModName (no spaces)", "Type here");
			modDiplayName = createAndAppendTextInputWithLabel("Mod DisplayName", "Type here");
			modAuthor = createAndAppendTextInputWithLabel("Mod Author", "Type here");
			// TODO: Starting Item checkbox

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

				var uIInputTextField = new UIFocusInputTextField(hint);
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
			modName.SetText("");
			modDiplayName.SetText("");
			modAuthor.SetText("");
			messagePanel.SetText("");
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modSourcesID;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			string modNameTrimmed = modName.currentString.Trim();
			var provider = CodeDomProvider.CreateProvider("C#");
			if (!provider.IsValidIdentifier(modNameTrimmed))
				messagePanel.SetText("ModName is invalid C# identifier. Remove spaces.");
			else if (string.IsNullOrWhiteSpace(modDiplayName.currentString))
				messagePanel.SetText("DisplayName can't be empty");
			else if (string.IsNullOrWhiteSpace(modAuthor.currentString))
				messagePanel.SetText("Author can't be empty");
			else if (string.IsNullOrWhiteSpace(modAuthor.currentString))
				messagePanel.SetText("Author can't be empty");
			else {
				Main.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
				string sourceFolder = ModCompile.ModSourcePath + Path.DirectorySeparatorChar + modNameTrimmed;
				Directory.CreateDirectory(sourceFolder);

				// TODO: Simple ModItem and PNG, verbatim line endings, localization.
				File.WriteAllText(Path.Combine(sourceFolder, "build.txt"), $"displayName = {modDiplayName.currentString}{Environment.NewLine}author = {modAuthor.currentString}version = 0.1");
				File.WriteAllText(Path.Combine(sourceFolder, "desciption.txt"), $"{modDiplayName.currentString} is a pretty cool mod, it does...this. Modify this file with a description of your mod.");
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.cs"), $@"using Terraria.ModLoader;

namespace {modNameTrimmed}
{{
	class {modNameTrimmed} : Mod
	{{
		public {modNameTrimmed}()
		{{
		}}
	}}
}}");
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.csproj"), $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <Import Project=""..\..\references\tModLoader.targets"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{{8298EAB6-0586-4BDA-9483-83624B66B13A}}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>{modNameTrimmed}</RootNamespace>
    <AssemblyName>{modNameTrimmed}</AssemblyName>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <PlatformTarget>x86</PlatformTarget>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' Or '$(Configuration)|$(Platform)' == 'ServerDebug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""**\*.cs"" Exclude=""obj\**\*.*"" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include=""System"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <PropertyGroup>
    <PostBuildEvent>""$(tMLServerPath)"" -build ""$(ProjectDir)\"" -eac ""$(TargetPath)""</PostBuildEvent>
  </PropertyGroup>
</Project>");
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.csproj.user"), $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""..\..\references\tModLoader.targets"" />
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'"">
    <StartAction>Program</StartAction>
    <StartProgram>$(tMLPath)</StartProgram>
    <StartWorkingDirectory>$(TerrariaSteamPath)</StartWorkingDirectory>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(Platform)' == 'ServerDebug|AnyCPU'"">
    <StartAction>Program</StartAction>
    <StartProgram>$(tMLServerPath)</StartProgram>
    <StartWorkingDirectory>$(TerrariaSteamPath)</StartWorkingDirectory>
  </PropertyGroup>
</Project>
");
			}
		}
	}
}

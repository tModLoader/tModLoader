using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDeveloperModeHelp : UIState
	{
		private UIPanel backPanel;
		private UIImage refAssemDirectDlButton;
		private UITextPanel<string> bottomButton;
		private bool allChecksSatisfied;

		public override void OnInitialize() {
			UIElement area = new UIElement {
				Width = new StyleDimension(0f, 0.5f),
				Top = new StyleDimension(200f, 0f),
				Height = new StyleDimension(-240f, 1f),
				HAlign = 0.5f
			};

			backPanel = new UIPanel {
				Width = new StyleDimension(0f, 1f),
				Height = new StyleDimension(-90f, 1f),
				BackgroundColor = new Color(33, 43, 79) * 0.8f
			};
			area.Append(backPanel);

			UITextPanel<string> heading = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuEnableDeveloperMode"), 0.8f, true) {
				HAlign = 0.5f,
				Top = new StyleDimension(-45f, 0f),
				BackgroundColor = new Color(73, 94, 171)
			};
			heading.SetPadding(15f);
			area.Append(heading);

			bottomButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 0.7f, true) {
				Width = new StyleDimension(0, 0.5f),
				Height = new StyleDimension(50f, 0f),
				HAlign = 0.5f,
				VAlign = 1f,
				Top = new StyleDimension(-30f, 0f)
			};
			bottomButton.OnMouseOver += UICommon.FadedMouseOver;
			bottomButton.OnMouseOut += UICommon.FadedMouseOut;
			bottomButton.OnClick += BackClick;
			area.Append(bottomButton);

			Append(area);
		}

		public override void OnActivate() {
			backPanel.RemoveAllChildren();

			int i = 0;
			UIMessageBox AddMessageBox(bool check, string text) {
				var msgBox = new UIMessageBox(text) {
					Width = new StyleDimension(0, 1f),
					Height = new StyleDimension(0, .2f),
					Top = new StyleDimension(0, (i++) / 4f + 0.05f)
				};
				backPanel.Append(msgBox);
				return msgBox;
			}

			UITextPanel<string> AddButton(UIElement elem, string text, Action clickAction) {
				var button = new UITextPanel<string>(text) {
					Top = new StyleDimension(-2, 0),
					Left = new StyleDimension(-2, 0),
					HAlign = 1,
					VAlign = 1
				};
				button.OnMouseOver += UICommon.FadedMouseOver;
				button.OnMouseOut += UICommon.FadedMouseOut;
				button.OnClick += (evt, _) => clickAction();
				elem.Append(button);
				return button;
			}

			bool dotNetCheck = ModCompile.DotNet46Check(out var dotNetMsg);
			var dotNetMsgBox = AddMessageBox(dotNetCheck, Language.GetTextValue(dotNetMsg, $"{FrameworkVersion.Framework} {FrameworkVersion.Version}"));
			if (!dotNetCheck) {
				AddButton(dotNetMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadDotNet);
			}

			bool modCompileCheck = ModCompile.ModCompileVersionCheck(out var modCompileMsg);
			var modCompileMsgBox = AddMessageBox(modCompileCheck, Language.GetTextValue(modCompileMsg));
#if !DEBUG
			if (!modCompileCheck)
				AddButton(modCompileMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadModCompile);
#endif

			bool refAssemCheck = ModCompile.ReferenceAssembliesCheck(out var refAssemMsg);
			var refAssemMsgBox = AddMessageBox(refAssemCheck, Language.GetTextValue(refAssemMsg));
			if (!refAssemCheck) {
				var vsButton = AddButton(refAssemMsgBox, Language.GetTextValue("tModLoader.DMVisualStudio"), DevelopingWithVisualStudio);

				var icon = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));
				refAssemDirectDlButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.DMReferenceAssembliesDownload")) {
					Left = new StyleDimension(-1, 0f),
					Top = new StyleDimension(-1, 0f),
					VAlign = 1,
				};
				refAssemDirectDlButton.OnClick += (evt, _) => DirectDownloadRefAssemblies();
				refAssemMsgBox.Append(refAssemDirectDlButton);
			}

			var tutorialMsgBox = AddMessageBox(true, Language.GetTextValue("tModLoader.DMTutorialWelcome"));
			AddButton(tutorialMsgBox, Language.GetTextValue("tModLoader.DMTutorial"), OpenTutorial);

			allChecksSatisfied = dotNetCheck && modCompileCheck && refAssemCheck;
			bottomButton.SetText(allChecksSatisfied ?
				Language.GetTextValue("tModLoader.Continue") :
				Language.GetTextValue("UI.Back"));
		}

		private void DevelopingWithVisualStudio() {
			Process.Start("https://github.com/blushiemagic/tModLoader/wiki/Developing-with-Visual-Studio");
		}

		private void OpenTutorial() {
			Process.Start("https://github.com/blushiemagic/tModLoader/wiki/Basic-tModLoader-Modding-Guide");
		}

		private void DownloadDotNet() {
			Process.Start("https://www.microsoft.com/net/download/thank-you/net472");
		}

		private void DownloadModCompile() {
			Main.PlaySound(SoundID.MenuOpen);
			// TODO: Replace with https://github.com/blushiemagic/tModLoader/releases/download/v0.10.1.5/ModCompile.zip for releases
			string url = "https://www.dropbox.com/s/cf9bdrw273whv97/ModCompileTest.zip?dl=1";
			string file = Path.Combine(ModCompile.modCompileDir, $"ModCompile_{ModLoader.versionedName}.zip");
			Directory.CreateDirectory(ModCompile.modCompileDir);
			DownloadFile("ModCompile", url, file, () => DeleteFilesAndUnzip(file));
		}

		private void DirectDownloadRefAssemblies() {
			Main.PlaySound(SoundID.MenuOpen);
			//TODO: Replace with centrally hosted link
			string url = "https://www.dropbox.com/s/ddz854nqsckbn75/v4.5%20Reference%20Assemblies.zip?dl=1";
			string folder = Path.Combine(ModCompile.modCompileDir, "v4.5 Reference Assemblies");
			string file = Path.Combine(folder, "v4.5 Reference Assemblies.zip");
			Directory.CreateDirectory(folder);
			DownloadFile("v4.5 Reference Assemblies", url, file, () => DeleteFilesAndUnzip(file));
		}

		private void DeleteFilesAndUnzip(string zipFile, bool deleteFiles = false) {
			string folder = Path.GetDirectoryName(zipFile);
			Directory.CreateDirectory(folder);
			if (deleteFiles) {
				foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles()) {
					if (file.Name != Path.GetFileName(zipFile)) {
						file.Delete();
					}
				}
			}

			using (ZipFile zip = ZipFile.Read(zipFile)) {
				zip.ExtractAll(folder, ExtractExistingFileAction.OverwriteSilently);
			}

			File.Delete(zipFile);
			Main.menuMode = Interface.developerModeHelpID;
		}

		private void DownloadFile(string name, string url, string file, Action downloadModCompileComplete) {
			Interface.downloadFile.SetDownloading(name, url, file, downloadModCompileComplete);
			Main.menuMode = Interface.downloadFileID;
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			if (allChecksSatisfied) {
				Main.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
			}
			else {
				Main.PlaySound(SoundID.MenuClose);
				Main.menuMode = 0;
			}
		}
	}
}

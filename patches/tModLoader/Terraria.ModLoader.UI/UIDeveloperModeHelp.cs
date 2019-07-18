using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.Utilities;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.UI
{
	internal class UIDeveloperModeHelp : UIState
	{
		private UIPanel _backPanel;
		private UIImage _refAssemDirectDlButton;
		private UITextPanel<string> _bottomButton;
		private bool _allChecksSatisfied;
		private bool _updateRequired;

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 60 },
				Height = { Pixels = -60, Percent = 1f },
				HAlign = 0.5f
			};

			_backPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -70, Percent = 1f },
				BackgroundColor = UICommon.MainPanelBackground
			}.WithPadding(6);
			area.Append(_backPanel);

			var heading = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuEnableDeveloperMode"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -45 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15);
			area.Append(heading);

			_bottomButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 0.7f, true) {
				Width = { Percent = 0.5f },
				Height = { Pixels = 50 },
				HAlign = 0.5f,
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			_bottomButton.OnClick += BackClick;
			area.Append(_bottomButton);

			Append(area);
		}

		public override void OnActivate() {
			_updateRequired = true;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!_updateRequired) return;

			_updateRequired = false;
			_backPanel.RemoveAllChildren();

			int i = 0;

			UIMessageBox AddMessageBox(string text) {
				var msgBox = new UIMessageBox(text) {
					Width = { Percent = 1f },
					Height = { Percent = .2f },
					Top = { Percent = (i++) / 4f + 0.05f },
				}.WithPadding(6);
				_backPanel.Append(msgBox);
				msgBox.Activate();
				return msgBox;
			}

			void AddButton(UIElement elem, string text, Action clickAction) {
				var button = new UITextPanel<string>(text) {
					Top = { Pixels = -2 },
					Left = { Pixels = -2 },
					HAlign = 1,
					VAlign = 1
				}.WithFadedMouseOver();
				button.OnClick += (evt, _) => clickAction();
				button.Activate();
				elem.Append(button);
			}

			bool frameworkCheck = ModCompile.RoslynCompatibleFrameworkCheck(out var dotNetMsg);
			if (monoStartScriptsUpdated)
				dotNetMsg = Language.GetTextValue("tModLoader.DMScriptsRequireRestart");

			var dotNetMsgBox = AddMessageBox(dotNetMsg);
			if (!frameworkCheck && !monoStartScriptsUpdated) {
				if (ModCompile.systemMonoSuitable)
					AddButton(dotNetMsgBox, Language.GetTextValue("tModLoader.DMUpdateScripts"), UpdateMonoStartScripts);
				else if (FrameworkVersion.Framework == Framework.Mono)
					AddButton(dotNetMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadMono);
				else
					AddButton(dotNetMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadDotNet);
			}

			bool modCompileCheck = ModCompile.ModCompileVersionCheck(out var modCompileMsg);
			if (!modCompileCheck && !ModBrowser.UIModBrowser.PlatformSupportsTls12)
				modCompileMsg = "tModLoader.DMUpdateMonoToDownloadModCompile";
			var modCompileMsgBox = AddMessageBox(Language.GetTextValue(modCompileMsg));
#if !DEBUG
			if (!modCompileCheck && ModBrowser.UIModBrowser.PlatformSupportsTls12)
				AddButton(modCompileMsgBox, Language.GetTextValue("tModLoader.MBDownload"), DownloadModCompile);
#endif

			bool refAssemCheck = ModCompile.ReferenceAssembliesCheck(out var refAssemMsg);
			var refAssemMsgBox = AddMessageBox(refAssemMsg);

			if (!refAssemCheck) {
				if (ModCompile.PlatformSupportsVisualStudio)
					AddButton(refAssemMsgBox, Language.GetTextValue("tModLoader.DMVisualStudio"), DevelopingWithVisualStudio);

				var icon = UICommon.ButtonExclamationTexture;
				_refAssemDirectDlButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.DMReferenceAssembliesDownload")) {
					Left = { Pixels = -1 },
					Top = { Pixels = -1 },
					VAlign = 1,
				};
				_refAssemDirectDlButton.OnClick += (evt, _) => DirectDownloadRefAssemblies();
				refAssemMsgBox.Append(_refAssemDirectDlButton);
			}

			var tutorialMsgBox = AddMessageBox(Language.GetTextValue("tModLoader.DMTutorialWelcome"));
			AddButton(tutorialMsgBox, Language.GetTextValue("tModLoader.DMTutorial"), OpenTutorial);

			_allChecksSatisfied = frameworkCheck && modCompileCheck && refAssemCheck;
			_bottomButton.SetText(_allChecksSatisfied ? Language.GetTextValue("tModLoader.Continue") : Language.GetTextValue("UI.Back"));
		}

		private void DevelopingWithVisualStudio() {
			Process.Start("https://github.com/tModLoader/tModLoader/wiki/Developing-with-Visual-Studio");
		}

		private void OpenTutorial() {
			Process.Start("https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide");
		}

		private void DownloadDotNet() {
			Process.Start("https://www.microsoft.com/net/download/thank-you/net472");
		}

		private void DownloadMono() {
			Process.Start("https://www.mono-project.com/download/stable/");
		}

		private static bool monoStartScriptsUpdated;
		private void UpdateMonoStartScripts() {
			try {
				// upgrade start scripts to system mono
				foreach (var monoPath in new[] { "tModLoader", "tModLoaderServer" })
					File.Copy("tModLoader-mono", monoPath, true);

				// vanilla start scripts need to be upgraded to copy back the sys/ folder
				var kickPaths = new List<string> { "TerrariaServer" };
				if (!File.ReadAllText("Terraria").Contains("forwarder"))
					kickPaths.Add("Terraria");

				foreach (var kickPath in kickPaths)
					File.Copy("tModLoader-kick", kickPath, true);

				monoStartScriptsUpdated = true;
				_updateRequired = true;
			}
			catch (Exception e) {
				Logging.tML.Error(e);
				Interface.errorMessage.Show("Failed to copy mono start scripts\n" + e, Interface.developerModeHelpID);
			}
		}

		private void DownloadModCompile() {
			Main.PlaySound(SoundID.MenuOpen);
			// download the ModCompile for the platform we don't have
			string url = $"https://github.com/tModLoader/tModLoader/releases/download/{ModLoader.versionTag}/ModCompile_{(PlatformUtilities.IsXNA ? "FNA" : "XNA")}.zip";
			string file = Path.Combine(ModCompile.modCompileDir, $"ModCompile_{ModLoader.versionedName}.zip");
			Directory.CreateDirectory(ModCompile.modCompileDir);
			Interface.downloadProgress.OnDownloadsComplete += () => {
				Main.menuMode = Interface.developerModeHelpID;
			};
			DownloadFileFromUrl("ModCompile", url, file, () => {
				try {
					Extract(file);
					var currentEXEFilename = Process.GetCurrentProcess().ProcessName;
					string originalXMLFile = Path.Combine(ModCompile.modCompileDir, "Terraria.xml");
					string correctXMLFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{currentEXEFilename}.xml");
					File.Copy(originalXMLFile, correctXMLFile, true);
					File.Delete(originalXMLFile);
					string originalPDBFilename = ReLogic.OS.Platform.IsWindows ? "tModLoader.pdb" : (ReLogic.OS.Platform.IsLinux ? "tModLoader_Linux.pdb" : "tModLoader_Mac.pdb");
					string originalPDBFile = Path.Combine(ModCompile.modCompileDir, originalPDBFilename);
					string correctPDBFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"{currentEXEFilename}.pdb");
					File.Copy(originalPDBFile, correctPDBFile, true);
					File.Delete(originalPDBFile);
					_updateRequired = true;
				}
				catch (Exception e) {
					Logging.tML.Error($"Problem during extracting of mod compile files for", e);
				}
			});
		}

		private void DirectDownloadRefAssemblies() {
			Main.PlaySound(SoundID.MenuOpen);
			const string url = "https://tmodloader.net/dl/ext/v45ReferenceAssemblies.zip"; // This never changes, maybe put it on 0.11 release only and leave it out of other release uploads.
			string folder = Path.Combine(ModCompile.modCompileDir, "v4.5 Reference Assemblies");
			string file = Path.Combine(folder, "v4.5 Reference Assemblies.zip");
			Directory.CreateDirectory(folder);

			Interface.downloadProgress.OnDownloadsComplete += () => {
				Main.menuMode = Interface.developerModeHelpID;
			};

			DownloadFileFromUrl("v4.5 Reference Assemblies", url, file, () => {
				try {
					Extract(file);
				}
				catch (Exception e) {
					Logging.tML.Error($"Problem during extracting of reference assembly files for", e);
				}
			});
		}

		private void Extract(string zipFile, bool deleteFiles = false) {
			string folder = Path.GetDirectoryName(zipFile);
			Directory.CreateDirectory(folder);
			if (deleteFiles) {
				foreach (FileInfo file in new DirectoryInfo(folder).EnumerateFiles()) {
					if (file.Name != Path.GetFileName(zipFile))
						file.Delete();
				}
			}

			using (var zip = ZipFile.Read(zipFile))
				zip.ExtractAll(folder, ExtractExistingFileAction.OverwriteSilently);

			File.Delete(zipFile);
		}

		private void DownloadFileFromUrl(string name, string url, string file, Action downloadModCompileComplete) {
			var downloadFile = new DownloadFile(url, file, name);
			downloadFile.OnComplete += downloadModCompileComplete;
			Interface.downloadProgress.gotoMenu = Interface.developerModeHelpID;
			Interface.downloadProgress.OnCancel += () => {
				Interface.developerModeHelp._updateRequired = true;
			};
			Interface.downloadProgress.HandleDownloads(downloadFile);
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			if (_allChecksSatisfied) {
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
using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO how is this different to UIInfoMessage?
	internal class UIUpdateMessage : UIState
	{
		private readonly UIMessageBox _message = new UIMessageBox("");
		private UIElement _area;
		private UITextPanel<string> _autoUpdateButton;
		private int _gotoMenu;
		private string _url;
		private string _autoUpdateUrl;

		public override void OnInitialize() {
			_area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			_message.Width.Percent = 1f;
			_message.Height.Percent = 0.8f;
			_message.HAlign = 0.5f;
			_area.Append(_message);

			var button = new UITextPanel<string>("Ignore", 0.7f, true) {
				Width = { Pixels = -10, Percent = 1f / 3f },
				Height = { Pixels = 50 },
				VAlign = 1f,
				Top = { Pixels = -30 }
			};
			button.WithFadedMouseOver();
			button.OnClick += IgnoreClick;
			_area.Append(button);

			var button2 = new UITextPanel<string>("Download", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 0.5f;
			button2.WithFadedMouseOver();
			button2.OnClick += OpenURL;
			_area.Append(button2);

			_autoUpdateButton = new UITextPanel<string>("Auto Update", 0.7f, true);
			_autoUpdateButton.CopyStyle(button);
			_autoUpdateButton.HAlign = 1f;
			_autoUpdateButton.WithFadedMouseOver();
			_autoUpdateButton.OnClick += AutoUpdate;

			Append(_area);
		}

		public override void OnActivate() {
			base.OnActivate();

#if WINDOWS
			_area.AddOrRemoveChild(_autoUpdateButton, !string.IsNullOrEmpty(_autoUpdateUrl));
#endif
		}

		internal void SetMessage(string text) {
			_message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu) {
			_gotoMenu = gotoMenu;
		}

		internal void SetURL(string url) {
			_url = url;
		}

		internal void SetAutoUpdateURL(string autoUpdateURL) {
			_autoUpdateUrl = autoUpdateURL;
		}

		private void IgnoreClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Main.menuMode = _gotoMenu;
		}

		private void OpenURL(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Process.Start(_url);
		}

		// Windows only. AutoUpdate will download the the latest zip, extract it, then launch a script that waits for this exe to finish
		// The script then replaces this exe and then launches tModLoader again.
		private void AutoUpdate(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);

			string currentExecutingFilePath = Assembly.GetExecutingAssembly().Location;
			string installDirectory = Path.GetDirectoryName(currentExecutingFilePath);
			string autoUpdateFilePath = Path.Combine(installDirectory, Path.GetFileNameWithoutExtension(currentExecutingFilePath) + "_AutoUpdate.exe");
			string zipFileName = Path.GetFileName(new Uri(_autoUpdateUrl).LocalPath);
			string zipFilePath = Path.Combine(installDirectory, zipFileName);

			Logging.tML.Info("AutoUpdate started");
			Logging.tML.Info($"AutoUpdate Paths: currentExecutingFilePath {currentExecutingFilePath}, installDirectory {installDirectory}, autoUpdateFilePath {autoUpdateFilePath}, zipFileName {zipFileName}, zipFilePath {zipFilePath}, autoUpdateURL {_autoUpdateUrl}");
			var downloadFile = new DownloadFile(_autoUpdateUrl, zipFilePath, $"Auto update: {zipFileName}");
			downloadFile.OnComplete += () => {
				OnAutoUpdateDownloadComplete(zipFilePath, autoUpdateFilePath, installDirectory, currentExecutingFilePath);
			};
			Interface.downloadProgress.gotoMenu = Interface.modBrowserID;
			Interface.downloadProgress.HandleDownloads(downloadFile);
		}

		private void OnAutoUpdateDownloadComplete(string zipFilePath, string autoUpdateFilePath, string installDirectory, string currentExecutingFilePath) {
			try {
				using (var zip = ZipFile.Read(zipFilePath))
					for (int i = 0; i < zip.Count; i++) {
						var current = zip[i];
						if (current.FileName == "Terraria.exe") {
							current.FileName = Path.GetFileName(autoUpdateFilePath);
							Logging.tML.Info($"Saving AutoUpdate Terraria.exe to {current.FileName}");
						}
						current.Extract(ExtractExistingFileAction.OverwriteSilently);
					}
				File.Delete(zipFilePath);

				// Self deleting bat file techinque found here: https://stackoverflow.com/a/20333575
				string autoUpdateScript = Path.Combine(installDirectory, "AutoUpdate.bat");
				File.WriteAllText(autoUpdateScript, @"@ECHO OFF
if [%1]==[] goto usage
if [%2]==[] goto usage

ECHO Auto-update in progress. Waiting for tModLoader to close...
:LOOP
Timeout /T 5 /Nobreak
copy %2 %1 /Y
IF ERRORLEVEL 1 (
  ECHO %1 is still running, waiting for tModLoader to close...
  Timeout /T 5 /Nobreak
  GOTO LOOP
) ELSE (
  GOTO CONTINUE
)

:CONTINUE
echo Successfully updated, tModLoader will restart now.
del %2
start """" %1
(goto) 2>nul & del ""%~f0""
exit /B 0

:usage
echo Please do not run this file manually
exit /B 1");
				Process.Start(autoUpdateScript, $"\"{currentExecutingFilePath}\" \"{autoUpdateFilePath}\"");

				Main.menuMode = Interface.exitID; // Environment.Exit(0); // Exit on main thread to avoid crash
				Interface.downloadProgress.gotoMenu = Interface.exitID;
			}
			catch (Exception e) {
				Logging.tML.Error($"Problem during autoupdate", e);
			}
		}
	}
}

using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
		private UIMessageBox message = new UIMessageBox("");
		private UIElement area;
		private UITextPanel<string> autoUpdateButton;
		private int gotoMenu = 0;
		private string url;
		private string autoUpdateURL;

		public override void OnInitialize() {
			area = new UIElement {
				Width = { Percent = 0.8f },
				Top = { Pixels = 200 },
				Height = { Pixels = -240, Percent = 1f },
				HAlign = 0.5f
			};

			message.Width.Percent = 1f;
			message.Height.Percent = 0.8f;
			message.HAlign = 0.5f;
			area.Append(message);

			var button = new UITextPanel<string>("Ignore", 0.7f, true) {
				Width = { Pixels = -10, Percent = 1f / 3f },
				Height = { Pixels = 50 },
				VAlign = 1f,
				Top = { Pixels = -30 }
			};
			button.WithFadedMouseOver();
			button.OnClick += IgnoreClick;
			area.Append(button);

			var button2 = new UITextPanel<string>("Download", 0.7f, true);
			button2.CopyStyle(button);
			button2.HAlign = 0.5f;
			button2.WithFadedMouseOver();
			button2.OnClick += OpenURL;
			area.Append(button2);

			autoUpdateButton = new UITextPanel<string>("Auto Update", 0.7f, true);
			autoUpdateButton.CopyStyle(button);
			autoUpdateButton.HAlign = 1f;
			autoUpdateButton.WithFadedMouseOver();
			autoUpdateButton.OnClick += AutoUpdate;

			Append(area);
		}

		public override void OnActivate() {
			base.OnActivate();

#if WINDOWS
			area.AddOrRemoveChild(autoUpdateButton, !string.IsNullOrEmpty(autoUpdateURL));
#endif
		}

		internal void SetMessage(string text) {
			message.SetText(text);
		}

		internal void SetGotoMenu(int gotoMenu) {
			this.gotoMenu = gotoMenu;
		}

		internal void SetURL(string url) {
			this.url = url;
		}
		internal void SetAutoUpdateURL(string autoUpdateURL) {
			this.autoUpdateURL = autoUpdateURL;
		}

		private void IgnoreClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Main.menuMode = gotoMenu;
		}

		private void OpenURL(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Process.Start(url);
		}

		// Windows only. AutoUpdate will download the the latest zip, extract it, then launch a script that waits for this exe to finish
		// The script then replaces this exe and then launches tModLoader again.
		private void AutoUpdate(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);

			string currentExecutingFilePath = Assembly.GetExecutingAssembly().Location;
			string installDirectory = Path.GetDirectoryName(currentExecutingFilePath);
			string autoUpdateFilePath = Path.Combine(installDirectory, Path.GetFileNameWithoutExtension(currentExecutingFilePath) + "_AutoUpdate.exe");
			string zipFileName = Path.GetFileName(new Uri(autoUpdateURL).LocalPath);
			string zipFilePath = Path.Combine(installDirectory, zipFileName);

			Logging.tML.Info($"AutoUpdate started");
			Logging.tML.Info($"AutoUpdate Paths: currentExecutingFilePath {currentExecutingFilePath}, installDirectory {installDirectory}, autoUpdateFilePath {autoUpdateFilePath}, zipFileName {zipFileName}, zipFilePath {zipFilePath}, autoUpdateURL {autoUpdateURL}");
			Interface.downloadManager.OnQueueProcessed = () => { };
			Interface.downloadManager.EnqueueRequest(
				new HttpDownloadRequest($"Auto Update: {zipFileName}", zipFilePath, () => (HttpWebRequest)WebRequest.Create(autoUpdateURL), onComplete: () => {
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

						// TODO: Auto delete this file somewhere.
						string autoUpdateScript = Path.Combine(installDirectory, "AutoUpdate.bat");
						File.WriteAllText(autoUpdateScript, @"@ECHO OFF
if [%1]==[] goto usage
if [%2]==[] goto usage

:LOOP
copy %2 %1 /Y
echo Exit Code is %errorlevel%
IF ERRORLEVEL 1 (
  ECHO %1 is still running
  Timeout /T 5 /Nobreak
  GOTO LOOP
) ELSE (
  GOTO CONTINUE
)

:CONTINUE
echo Successfully updated
exit /B 0

:usage
echo Please do not run this file manually
exit /B 1");
						Process.Start(autoUpdateScript, $"\"{currentExecutingFilePath}\" \"{autoUpdateFilePath}\"");

						Main.menuMode = Interface.exitID; // Environment.Exit(0); // Exit on main thread to avoid crash
					}
					catch (Exception e) {
						Logging.tML.Error($"Problem during autoupdate", e);
					}
				}, onCancel: () => Main.menuMode = Interface.modBrowserID));
			Main.menuMode = Interface.downloadManagerID;
		}
	}
}

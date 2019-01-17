using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO: downloads and web exceptions need logging
	//TODO: merge all progress/download UIs
	internal class UIDownloadMods : UIState
	{
		private UILoadProgress loadProgress;
		private string name;
		private Action cancelAction;
		private Queue<UIModDownloadItem> modsToDownload = new Queue<UIModDownloadItem>();
		private List<string> missingMods = new List<string>();
		private WebClient client;
		private UIModDownloadItem currentDownload;

		public override void OnInitialize() {
			loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(loadProgress);

			var cancel = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true) {
				VAlign = 0.5f,
				HAlign = 0.5f,
				Top = { Pixels = 170 }
			}.WithFadedMouseOver();
			cancel.OnClick += CancelClick;
			Append(cancel);
		}

		public override void OnActivate() {
			loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", name + ": ???"));
			loadProgress.SetProgress(0f);
			if (UIModBrowser.PlatformSupportsTls12) // Needed for downloads from Github
			{
				ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; // SecurityProtocolType.Tls12
			}
			if (modsToDownload != null && modsToDownload.Count > 0) {
				client = new WebClient();
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
				SetCancel(client.CancelAsync);
				client.DownloadProgressChanged += Client_DownloadProgressChanged;
				client.DownloadFileCompleted += Client_DownloadFileCompleted;
				currentDownload = modsToDownload.Dequeue();
				loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", $"{name}: {currentDownload.displayname}"));
				client.DownloadFileAsync(new Uri(currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else {
				Interface.modBrowser.ClearItems();
				Main.menuMode = Interface.modBrowserID;
				if (missingMods.Count > 0) {
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", String.Join(",", missingMods)), Interface.modBrowserID);
				}
			}
		}

		//public override void Update(GameTime gameTime)
		//{
		//	if (modsToDownload == null || modsToDownload.Count == 0)
		//		Main.menuMode = Interface.modBrowserID;
		//}

		private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
			//Main.menuMode = Interface.modBrowserID;
			if (e.Error != null) {
				if (e.Cancelled) {
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
				}
				else {
					var errorKey = GetHttpStatusCode(e.Error) == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
					Interface.errorMessage.Show(Language.GetTextValue("tModLoader."+errorKey), 0);
				}
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else if (!e.Cancelled) {
				// Downloaded OK
				File.Copy(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod", ModLoader.ModPath + Path.DirectorySeparatorChar + currentDownload.mod + ".tmod", true);
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				if (!currentDownload.update) {
					Interface.modBrowser.aNewModDownloaded = true;
				}
				else {
					Interface.modBrowser.aModUpdated = true;
				}
				if (ModLoader.autoReloadAndEnableModsLeavingModBrowser) {
					ModLoader.EnableMod(currentDownload.mod);
				}

				// Start next download
				if (modsToDownload.Count != 0) {
					currentDownload = modsToDownload.Dequeue();
					loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", $"{name}: {currentDownload.displayname}"));
					loadProgress.SetProgress(0f);
					client.DownloadFileAsync(new Uri(currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				}
				else {
					client.Dispose();
					client = null;
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
					if (missingMods.Count > 0) {
						Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", String.Join(",", missingMods)), Interface.modsMenuID);
					}
				}
			}
			else {
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			SetProgress(e);
		}

		internal void SetDownloading(string name) {
			this.name = name;
		}

		public void SetCancel(Action cancelAction) {
			this.cancelAction = cancelAction;
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);
		internal void SetProgress(long count, long len) {
			//loadProgress?.SetText("Downloading: " + name + " -- " + count+"/" + len);
			loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			cancelAction?.Invoke();
		}

		internal void SetModsToDownload(List<string> specialModPackFilter, List<UIModDownloadItem> items) {
			modsToDownload.Clear();
			missingMods.Clear();
			foreach (var desiredMod in specialModPackFilter) {
				var mod = items.FirstOrDefault(x => x.mod == desiredMod) ?? null;
				if (mod == null) {
					missingMods.Add(desiredMod);
				}
				else {
					if (mod.installed != null && !mod.update) {
						// skip mods that are already installed and don't have an update
					}
					else {
						modsToDownload.Enqueue(mod);
					}
				}
			}
		}

		private HttpStatusCode GetHttpStatusCode(System.Exception err) {
			if (err is WebException) {
				WebException we = (WebException)err;
				if (we.Response is HttpWebResponse) {
					HttpWebResponse response = (HttpWebResponse)we.Response;
					return response.StatusCode;
				}
			}
			return 0;
		}
	}
}

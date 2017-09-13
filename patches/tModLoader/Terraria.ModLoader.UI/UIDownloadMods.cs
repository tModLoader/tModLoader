using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System.Linq;
using Terraria.ID;
using System.IO;
using System.Net.Security;

namespace Terraria.ModLoader.UI
{
	internal class UIDownloadMods : UIState
	{
		private UILoadProgress loadProgress;
		private string name;
		private Action cancelAction;
		private Queue<UIModDownloadItem> modsToDownload = new Queue<UIModDownloadItem>();
		private List<string> missingMods = new List<string>();
		private WebClient client;
		private UIModDownloadItem currentDownload;

		public override void OnInitialize()
		{
			loadProgress = new UILoadProgress();
			loadProgress.Width.Set(0f, 0.8f);
			loadProgress.MaxWidth.Set(600f, 0f);
			loadProgress.Height.Set(150f, 0f);
			loadProgress.HAlign = 0.5f;
			loadProgress.VAlign = 0.5f;
			loadProgress.Top.Set(10f, 0f);
			base.Append(loadProgress);

			var cancel = new UITextPanel<string>("Cancel", 0.75f, true);
			cancel.VAlign = 0.5f;
			cancel.HAlign = 0.5f;
			cancel.Top.Set(170f, 0f);
			cancel.OnMouseOver += UICommon.FadedMouseOver;
			cancel.OnMouseOut += UICommon.FadedMouseOut;
			cancel.OnClick += CancelClick;
			base.Append(cancel);
		}

		public override void OnActivate()
		{
			loadProgress.SetText($"Downloading: {name}: ???");
			loadProgress.SetProgress(0f);

			if (modsToDownload != null && modsToDownload.Count > 0)
			{
				client = new WebClient();
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
				SetCancel(client.CancelAsync);
				client.DownloadProgressChanged += Client_DownloadProgressChanged;
				client.DownloadFileCompleted += Client_DownloadFileCompleted;
				currentDownload = modsToDownload.Dequeue();
				loadProgress.SetText($"Downloading: {name}: {currentDownload.displayname}");
				client.DownloadFileAsync(new Uri(currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else
			{
				Interface.modBrowser.ClearItems();
				Main.menuMode = Interface.modBrowserID;
				if (missingMods.Count > 0)
				{
					Interface.infoMessage.SetMessage("The following mods were not found on the Mod Browser: " + String.Join(",", missingMods));
					Interface.infoMessage.SetGotoMenu(Interface.modBrowserID);
					Main.menuMode = Interface.infoMessageID;
				}
			}
		}

		//public override void Update(GameTime gameTime)
		//{
		//	if (modsToDownload == null || modsToDownload.Count == 0)
		//		Main.menuMode = Interface.modBrowserID;
		//}

		private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			//Main.menuMode = Interface.modBrowserID;
			if (e.Error != null)
			{
				if (e.Cancelled)
				{
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
				}
				else
				{
					HttpStatusCode httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
					{
						Interface.errorMessage.SetMessage("The Mod Browser server has exceeded its daily bandwidth allotment. Please consult this mod's homepage for an alternate download or try again later.");
						Interface.errorMessage.SetGotoMenu(0);
						Interface.errorMessage.SetFile(ErrorLogger.LogPath);
						Main.gameMenu = true;
						Main.menuMode = Interface.errorMessageID;
					}
					else
					{
						Interface.errorMessage.SetMessage("Unknown Mod Browser Error. Try again later.");
						Interface.errorMessage.SetGotoMenu(0);
						Interface.errorMessage.SetFile(ErrorLogger.LogPath);
						Main.gameMenu = true;
						Main.menuMode = Interface.errorMessageID;
					}
				}
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
			else if (!e.Cancelled)
			{
				// Downloaded OK
				File.Copy(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod", ModLoader.ModPath + Path.DirectorySeparatorChar + currentDownload.mod + ".tmod", true);
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				if (!currentDownload.update)
				{
					Interface.modBrowser.aNewModDownloaded = true;
				}
				else
				{
					Interface.modBrowser.aModUpdated = true;
				}

				// Start next download
				if (modsToDownload.Count != 0)
				{
					currentDownload = modsToDownload.Dequeue();
					loadProgress.SetText($"Downloading: {name}: {currentDownload.displayname}");
					loadProgress.SetProgress(0f);
					client.DownloadFileAsync(new Uri(currentDownload.download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
				}
				else
				{
					client.Dispose();
					client = null;
					Interface.modBrowser.ClearItems();
					Main.menuMode = Interface.modBrowserID;
					if (missingMods.Count > 0)
					{
						Interface.infoMessage.SetMessage("The following mods were not found on the Mod Browser: " + String.Join(",", missingMods));
						Interface.infoMessage.SetGotoMenu(Interface.modsMenuID);
						Main.menuMode = Interface.infoMessageID;
					}
				}
			}
			else
			{
				File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
			}
		}

		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			SetProgress(e);
		}

		internal void SetDownloading(string name)
		{
			this.name = name;
		}

		public void SetCancel(Action cancelAction)
		{
			this.cancelAction = cancelAction;
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e) => SetProgress(e.BytesReceived, e.TotalBytesToReceive);
		internal void SetProgress(long count, long len)
		{
			//loadProgress?.SetText("Downloading: " + name + " -- " + count+"/" + len);
			loadProgress?.SetProgress((float)count / len);
		}

		private void CancelClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			cancelAction?.Invoke();
		}

		internal void SetModsToDownload(List<string> specialModPackFilter, List<UIModDownloadItem> items)
		{
			modsToDownload.Clear();
			missingMods.Clear();
			foreach (var desiredMod in specialModPackFilter)
			{
				var mod = items.FirstOrDefault(x => x.mod == desiredMod) ?? null;
				if (mod == null)
					missingMods.Add(desiredMod);
				else
				{
					if (mod.installed != null && !mod.update)
					{
						// skip mods that are already installed and don't have an update
					}
					else
						modsToDownload.Enqueue(mod);
				}
			}
		}

		private HttpStatusCode GetHttpStatusCode(System.Exception err)
		{
			if (err is WebException)
			{
				WebException we = (WebException)err;
				if (we.Response is HttpWebResponse)
				{
					HttpWebResponse response = (HttpWebResponse)we.Response;
					return response.StatusCode;
				}
			}
			return 0;
		}
	}
}

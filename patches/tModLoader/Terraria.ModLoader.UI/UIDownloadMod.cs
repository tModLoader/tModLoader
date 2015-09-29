using System;
using System.IO;
using System.Net;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDownloadMod : UIState
	{
		private UILoadProgress loadProgress;

		public override void OnInitialize()
		{
			loadProgress = new UILoadProgress();
			SetDownloading();
			loadProgress.Width.Set(0f, 0.8f);
			loadProgress.MaxWidth.Set(600f, 0f);
			loadProgress.Height.Set(150f, 0f);
			loadProgress.HAlign = 0.5f;
			loadProgress.VAlign = 0.5f;
			loadProgress.Top.Set(10f, 0f);
			base.Append(loadProgress);
		}

		public override void OnActivate()
		{
			SetDownloading();
			loadProgress.SetProgress(0f);
		}

		internal void SetDownloading()
		{
			loadProgress.SetText("Downloading: " + Interface.modBrowser.selectedItem.mod);
		}

		internal void SetProgress(DownloadProgressChangedEventArgs e)
		{
			loadProgress.SetText("Downloading: " + Interface.modBrowser.selectedItem.mod);// + " -- " + e.BytesReceived+"/" + e.TotalBytesToReceive);
			loadProgress.SetProgress((float)e.ProgressPercentage / 100f);
		}
	}
}

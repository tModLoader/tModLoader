using System;
using System.Net;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDownloadMod : UIState
	{
		private UILoadProgress loadProgress;
		private string name;
		private Action cancelAction;

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
			
			var cancel = new UITextPanel<string>(Language.GetTextValue("UI.Cancel"), 0.75f, true);
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
			loadProgress.SetText(Language.GetTextValue("tModLoader.MBDownloadingMod", name));
			loadProgress.SetProgress(0f);
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
			Main.PlaySound(10);
			cancelAction();
		}
	}
}

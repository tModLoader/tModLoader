using System;
using System.Net;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class UIUploadModProgress : UIProgress
	{
		public override void OnActivate() {
			gotoMenu = Interface.modSourcesID;
		}

		internal void SetDownloading(string name)
			=> DisplayText = $"Uploading: {name}";

		public void SetCancel(Action cancelAction)
			=> OnCancel += cancelAction;

		internal void SetProgress(UploadProgressChangedEventArgs e)
			=> SetProgress(e.BytesSent, e.TotalBytesToSend);

		internal void SetProgress(long count, long len) {
			Progress = (float)count / len;
		}
	}
}

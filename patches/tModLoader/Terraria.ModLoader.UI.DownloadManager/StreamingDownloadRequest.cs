using System;
using System.IO;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.UI.DownloadManager
{
	/// <summary>
	/// Responsible for a streaming download, handled by <see cref="ModNet"/>
	/// </summary>
	internal class StreamingDownloadRequest : DownloadRequest
	{
		public long DownloadingLength { get; internal set; }
		public ModNet.ModHeader ModHeader { get; internal set; }
		public FileStream FileStream { get; internal set; }

		public StreamingDownloadRequest(string displayText, string outputFilePath,
			Action<DownloadRequest> onBufferUpdate = null, Action<DownloadRequest> onComplete = null,
			Action<DownloadRequest> onCancel = null, Action<DownloadRequest> onFinish = null,
			object customData = null)
			: base(displayText, outputFilePath, onBufferUpdate, onComplete, onCancel, onFinish, customData) {

		}

		public override bool SetupRequest(CancellationToken cancellationToken) {
			cancellationToken.Register(Cancel);
			return true;
		}

		public bool Receive(BinaryReader reader) {
			try {
				var bytes = reader.ReadBytes((int)Math.Min(DownloadingLength - FileStream.Position, ModNet.CHUNK_SIZE));
				FileStream.Write(bytes, 0, bytes.Length);
				OnBufferUpdate?.Invoke(this);
				return FileStream.Position == DownloadingLength;
			}
			catch (Exception e) {
				Cancel();
				ShowError(e);
				return false;
			}
		}

		public void Complete() {
			try {
				FileStream.Close();
				var mod = new TmodFile(ModHeader.path);
				mod.Read();
				mod.Close();
				OnComplete?.Invoke(this);
				Completed = true;

				if (!ModHeader.Matches(mod))
					throw new Exception(Language.GetTextValue("tModLoader.MPErrorModHashMismatch"));

				if (ModHeader.signed && !mod.ValidModBrowserSignature)
					throw new Exception(Language.GetTextValue("tModLoader.MPErrorModNotSigned"));

				ModLoader.EnableMod(mod.name);
				OnFinish?.Invoke(this);
			}
			catch (Exception e) {
				Cancel();
				ShowError(e);
			}
		}

		public void Cancel() {
			try {
				FileStream?.Close();
				File.Delete(ModHeader.path);
			}
			catch (Exception e) {
				Logging.tML.Error("Problem during download sync when receiving mod during closing of the filestream ", e);
			}

			OnCancel?.Invoke(this);
		}

		private void ShowError(Exception e) {
			var msg = Language.GetTextValue("tModLoader.MPErrorModDownloadError", ModHeader.name);
			Logging.tML.Error(msg, e);
			Interface.errorMessage.Show(msg + e, 0);
			Netplay.disconnect = true;
		}
	}
}

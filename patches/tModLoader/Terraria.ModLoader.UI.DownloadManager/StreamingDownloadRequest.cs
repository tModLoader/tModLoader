using System;
using System.IO;
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

		public StreamingDownloadRequest(string displayText, string outputFilePath, long downloadingLength, ModNet.ModHeader modHeader,
			object customData = null, Action<double> onUpdateProgress = null, Action onCancel = null, Action onComplete = null)
			: base(displayText, outputFilePath, customData, onUpdateProgress, onCancel, onComplete) {

			DownloadingLength = downloadingLength;
			ModHeader = modHeader;
		}

		private int _currentIndex;

		public bool Receive(BinaryReader reader) {
			try {
				byte[] bytes = reader.ReadBytes((int)Math.Min(DownloadingLength - FileStream.Position, ModNet.CHUNK_SIZE));
				FileStream.Write(bytes, 0, bytes.Length);
				_currentIndex += bytes.Length;
				UpdateProgress(_currentIndex / (double)DownloadingLength);
				return FileStream.Position == DownloadingLength;
			}
			catch (Exception e) {
				Cancel();
				ShowError(e);
				return false;
			}
		}

		public override void Execute() {
			UpdateProgress(0);
			ModLoader.GetMod(ModHeader.name)?.Close();
		}

		public override void Complete() {
			Success = true;
			base.Complete();

			try {
				var mod = new TmodFile(ModHeader.path);
				using (mod.Open()) { }

				if (!ModHeader.Matches(mod))
					throw new Exception(Language.GetTextValue("tModLoader.MPErrorModHashMismatch"));

				if (ModHeader.signed && !mod.ValidModBrowserSignature)
					throw new Exception(Language.GetTextValue("tModLoader.MPErrorModNotSigned"));

				ModLoader.EnableMod(mod.name);
			}
			catch (Exception e) {
				Cancel();
				ShowError(e);
			}
		}

		public override void Cancel() {
			Netplay.disconnect = true;
		}

		private void ShowError(Exception e) {
			var msg = Language.GetTextValue("tModLoader.MPErrorModDownloadError", ModHeader.name);
			Logging.tML.Error(msg, e);
			Interface.errorMessage.Show(msg + e, 0);
		}
	}
}

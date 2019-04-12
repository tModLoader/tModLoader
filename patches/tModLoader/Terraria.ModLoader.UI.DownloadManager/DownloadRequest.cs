using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal abstract class DownloadRequest
	{
		public readonly string DisplayText;
		public readonly string OutputFilePath;


		public object CustomData { get; internal set; }

		protected DownloadRequest(string displayText, string outputFilePath) {
			DisplayText = displayText;
			OutputFilePath = outputFilePath;
		}

		public virtual bool SetupRequest(CancellationToken cancellationToken) => true;
	}
}

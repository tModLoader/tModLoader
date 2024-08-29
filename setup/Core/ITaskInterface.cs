using System;
using System.Threading;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public interface ITaskInterface : IWin32Window
	{
		void SetMaxProgress(int max);
		void SetStatus(string status);
		void SetProgress(int progress);
		CancellationToken CancellationToken { get; }

		object Invoke(Delegate action);
		IAsyncResult BeginInvoke(Delegate action);
	}
}

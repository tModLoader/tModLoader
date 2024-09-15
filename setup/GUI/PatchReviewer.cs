#if WINDOWS
using System.Collections.Generic;
using Avalonia.Threading;
using DiffPatch;
using PatchReviewer;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Setup.GUI.Avalonia;

public class PatchReviewer : IPatchReviewer
{
	public void Show(IReadOnlyCollection<FilePatcher> results, string? commonBasePath = null)
	{
		Dispatcher.UIThread.Invoke(() => {
			ReviewWindow reviewWindow = new(results, commonBasePath) { AutoHeaders = true };
			reviewWindow.ShowDialog();
		});
	}
}
#endif
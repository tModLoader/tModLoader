using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader.UI;
internal class UIStartServer : UIProgress
{
	private int modCount;

	private string stageText;

	private CancellationTokenSource _cts;

	public override void OnActivate()
	{
		base.OnActivate();

		_cts = new CancellationTokenSource();

		OnCancel += () => {
			if (Main.tServer != null) {
				try {
					Main.tServer.Kill();
					Main.tServer = null;
				}
				catch {
				}
			}
			ServerClientIPC.ShutdownServer();
		};

		ServerClientIPC.Run(_cts.Token);
		DisplayText = "Starting server...";
		SubProgressText = "Waiting for server";
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		ServerClientIPC.ShutdownServer();
		_cts?.Dispose();
		_cts = null;
	}

	public void SetLoadStage(string stageText, int modCount = -1)
	{
		this.stageText = stageText;
		this.modCount = modCount;
		if (modCount < 0)
			SubProgressText = Language.GetTextValue(stageText);
		Progress = 0;
		SubProgressText = "";
	}

	public void SetCurrentMod(int i, string displayName, string version)
	{
		var display = $"{displayName} v{version}";
		SubProgressText = Language.GetTextValue(stageText, display);
		Progress = i / (float)modCount;
	}
}

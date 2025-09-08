using log4net.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader.UI;

internal class UIBuildMod : UIProgress, ModCompile.IBuildStatus
{
	private CancellationTokenSource _cts;
	private int numProgressItems;

	public void SetProgress(int i, int n = -1)
	{
		if (n >= 0) numProgressItems = n;
		Progress = i / (float)numProgressItems;
	}

	public void SetStatus(string msg)
	{
		Logging.tML.Info(msg);
		DisplayText = msg;
	}

	public void LogCompilerLine(string msg, Level level)
	{
		Logging.tML.Logger.Log(null, level, msg, null);
	}

	internal void Build(string mod, bool reload)
		=> Build(mc => mc.Build(mod), reload);

	internal void BuildAll(bool reload)
		=> Build(mc => mc.BuildAll(), reload);

	internal void Build(Action<ModCompile> buildAction, bool reload)
	{
		Main.menuMode = Interface.buildModID;
		Task.Run(() => BuildMod(buildAction, reload));
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
		_cancelButton.Remove();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		_cts = new CancellationTokenSource();
		OnCancel += () => {
			_cts.Cancel();
		};
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		_cts?.Dispose();
		_cts = null;
	}

	private Task BuildMod(Action<ModCompile> buildAction, bool reload)
	{
		while (_progressBar == null || _cts == null)
			Task.Delay(1); // wait for the UI to init

		try {
			// TODO propagate _cts and check for cancellation during build process:
			// _cts.Token.ThrowIfCancellationRequested();
			buildAction(new ModCompile(this));
			Main.menuMode = reload ? Interface.reloadModsID : Interface.modSourcesID;
		}
		catch (OperationCanceledException e) {
			Logging.tML.Info("Mod building was cancelled.");
			return Task.FromResult(false);
		}
		catch (Exception e) {
			Logging.tML.Error(e.Message, e);

			var mod = e.Data.Contains("mod") ? e.Data["mod"] : null;
			var msg = Language.GetTextValue("tModLoader.BuildError", mod ?? "");

			Action retry = null;
			if (e is BuildException)
			{
				msg += $"\n{e.Message}\n\n{e.InnerException?.ToString() ?? ""}";
				retry = () => Interface.buildMod.Build(buildAction, reload);
			}
			else
				msg += $"\n\n{e}";

			if (e.Data.Contains("showTModPorterHint")) {
				msg += $"\n{"Some of these errors can be fixed automatically by running tModPorter from the Mod Sources menu."}";
			}

			Interface.errorMessage.Show(msg, Interface.modSourcesID, webHelpURL: e.HelpLink, retryAction: retry);
			return Task.FromResult(false);
		}
		return Task.FromResult(true);
	}
}

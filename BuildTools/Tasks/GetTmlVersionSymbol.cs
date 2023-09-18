using System;
using System.IO;
using Microsoft.Build.Framework;

namespace tModLoader.BuildTools.Tasks;

public class GetTmlVersionSymbol : TaskBase
{
	/// <summary>
	/// The path to tModLoader's assembly file.
	/// </summary>
	[Required]
	public string TmlDllPath { get; set; } = string.Empty;

	[Output]
	public string TmlVersionSymbol { get; set; } = string.Empty;

	protected override void Run()
	{
		if (!File.Exists(TmlDllPath)) {
			Log.LogError("tModLoader assembly file does not exist. (Path: {0})", TmlDllPath);
			return;
		}

		Version version = SavePathLocator.GetTmlVersion(TmlDllPath);
		TmlVersionSymbol = $"TML_{version.Major}_{version.Minor:D2}";
	}
}
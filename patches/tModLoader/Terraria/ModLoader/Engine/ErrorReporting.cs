using System;
using System.IO;
using System.Threading;
using Terraria.Localization;

namespace Terraria.ModLoader.Engine;

/// <summary>
/// This class handles displaying errors that require a OS-provided modal message box. Fatal errors and errors that happen in situations where a suitable place to display an error doesn't exist (such as when initially loading).
/// </summary>
internal class ErrorReporting
{
	internal static void MessageBoxShow(string message, bool fatal = false)
	{
		var title = ModLoader.versionedName + (fatal ? " Fatal Error" : " Error");

		string logDir = Path.GetFullPath(Logging.LogDir);
		var logFileName = Logging.LogPath == null ? "Natives.log" : Path.GetFileName(Logging.LogPath);

		string logHint = Language.GetTextValue("tModLoader.LogPathHint", logFileName, logDir);
		if (Language.ActiveCulture == null) // Simple backup approach in case error happens before localization is loaded
			logHint = $"A {logFileName} file containing error information has been generated in\n{logDir}\n(You will need to share this file if asking for help)";

		message += "\n\n" + logHint;

		try {
			// always write to console. Ideal for headless servers
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Out.WriteLine(title + "\n" + message);
			SDL2.SDL.SDL_ShowSimpleMessageBox(SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, title, message, IntPtr.Zero);
		}
		catch { }
	}

	public static void FatalExit(string message)
	{
		if (Logging.LogPath == null)
			Console.Error.WriteLine(message); // Writes to Natives.log
		else
			Logging.tML.Fatal(message);

		TerrariaSteamClient.Shutdown();
		MessageBoxShow(message, fatal: true);
		Environment.Exit(1);
	}

	public static void FatalExit(string message, Exception e)
	{
		try {
			if (SDL2.SDL.SDL_GetError() is string error && !string.IsNullOrWhiteSpace(error))
				message += "\n\nSDL Error: " + error;
		}
		catch { }

		if (e.HelpLink != null) {
			try {
				Utils.OpenToURL(e.HelpLink);
			}
			catch { }
		}

		string tip = null;
		if (e is OutOfMemoryException)
			tip = Language.GetTextValue("tModLoader.OutOfMemoryHint");
		else if (e is InvalidOperationException || e is NullReferenceException || e is IndexOutOfRangeException || e is ArgumentNullException)
			tip = Language.GetTextValue("tModLoader.ModExceptionHint");
		else if (e is IOException && e.Message.Contains("cloud file provider")) {
			if (string.IsNullOrEmpty(e.HelpLink))
				e.HelpLink = "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ#save-data-file-issues";
			tip = Language.GetTextValue("tModLoader.OneDriveHint");
			if (Language.ActiveCulture == null) // This error typically happens before localization is loaded, so fallback to english text.
				tip = "Tip: Try installing/enabling OneDrive. Right click your Documents folder and enable \"Always save on this device\"";
		}
		else if (e is SynchronizationLockException)
			tip = Language.GetTextValue("tModLoader.AntivirusHint");
		else if (e is TypeInitializationException)
			tip = Language.GetTextValue("tModLoader.TypeInitializationHint");

		if (e.HelpLink != null) {
			try {
				Utils.OpenToURL(e.HelpLink);
			}
			catch { }
		}

		if (tip != null)
			message += "\n\n" + tip;

		message += "\n\n" + e;

		FatalExit(message);
	}

	/// <summary>
	/// Shows an OS-provided modal message box displaying a message and a number of buttons and returns the button index of the user-selected option. The first option will be mapped to return key and the last option to escape key. The options are displayed from right to left in order.
	/// </summary>
	/// <returns></returns>
	internal static int ShowMessageBoxWithChoices(string title, string message, string[] buttonLabels)
	{
		SDL2.SDL.SDL_MessageBoxButtonData[] buttons = new SDL2.SDL.SDL_MessageBoxButtonData[buttonLabels.Length];
		for (int i = 0; i < buttonLabels.Length; i++) {
			buttons[i] = new SDL2.SDL.SDL_MessageBoxButtonData() { flags = 0, buttonid = i, text = buttonLabels[i] };
			if (i == 0)
				buttons[i].flags = SDL2.SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT;
			else if (i == buttonLabels.Length - 1)
				buttons[i].flags = SDL2.SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT;
		}
		SDL2.SDL.SDL_MessageBoxData messageBoxData = new SDL2.SDL.SDL_MessageBoxData() {
			flags = SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION,
			window = IntPtr.Zero,
			title = title,
			message = message,
			numbuttons = buttons.Length,
			buttons = buttons,
			colorScheme = null
		};
		int buttonID;
		if (SDL2.SDL.SDL_ShowMessageBox(ref messageBoxData, out buttonID) < 0) {
			Logging.tML.Info("ShowMessageBoxWithChoices: Error displaying message box");
		}
		if (buttonID == -1) {
			Logging.tML.Info("ShowMessageBoxWithChoices: No selection");
		}
		return buttonID;
	}

	/// <summary> Various error codes to show in Visual Studio. Mainly used to cross reference with source code. Subject to change if more granular error codes are needed. </summary>
	internal enum TMLErrorCode
	{
		TML001, // Other exception
		TML002, // Any BuildException
		TML003, // tMod file in use
	}

	/// <summary> Writes an error to stderr using the <see href="https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-diagnostic-format-for-tasks?view=vs-2022">"MSBuild and Visual Studio format for diagnostic messages"</see>.
	/// <para/> This means the error will show up in "Error List" in VS directly. </summary>
	internal static void LogStandardDiagnosticError(string message, TMLErrorCode errorCode, bool error = true, string origin = "tModLoader", string subCategory = "Mod Build")
	{
		Console.Error.WriteLine($"{origin}: {subCategory} {(error ? "error" : "warning")} {errorCode}: {message}");
	}
}
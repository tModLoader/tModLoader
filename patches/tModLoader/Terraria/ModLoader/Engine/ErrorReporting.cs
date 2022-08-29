using System;
using System.IO;
using System.Threading;
using Terraria.Localization;

namespace Terraria.ModLoader.Engine
{
	internal class ErrorReporting
	{
		private static void MessageBoxShow(string message) {
			var title = ModLoader.versionedName + " Fatal Error";

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

		public static void FatalExit(string message) {
			if (Logging.LogPath == null)
				Console.Error.WriteLine(message); // Writes to Natives.log
			else
				Logging.tML.Fatal(message);

			TerrariaSteamClient.Shutdown();
			MessageBoxShow(message);
			Environment.Exit(1);
		}

		public static void FatalExit(string message, Exception e) {
			if (e.HelpLink != null) {
				try {
					Utils.OpenToURL(e.HelpLink);
				} catch { }
			}

			string tip = null;
			if (e is OutOfMemoryException)
				tip = Language.GetTextValue("tModLoader.OutOfMemoryHint");
			else if (e is InvalidOperationException || e is NullReferenceException || e is IndexOutOfRangeException || e is ArgumentNullException)
				tip = Language.GetTextValue("tModLoader.ModExceptionHint");
			else if (e is IOException && e.Message.Contains("cloud file provider"))
				tip = Language.GetTextValue("tModLoader.OneDriveHint");
			else if (e is SynchronizationLockException)
				tip = Language.GetTextValue("tModLoader.AntivirusHint");
			else if (e is TypeInitializationException)
				tip = Language.GetTextValue("tModLoader.TypeInitializationHint");

			if (tip != null)
				message += "\n\n" + tip;

			message += "\n\n" + e;

			FatalExit(message);
		}
	}
}

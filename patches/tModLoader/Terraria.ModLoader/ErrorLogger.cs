using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ErrorLogger
	{
		public static readonly string LogPath = Path.Combine(Main.SavePath, "Logs");
		internal static string CompileErrorPath = Path.Combine(LogPath, "Compile Errors.txt");
		private static readonly string[] buildDllLines =
		{
            "Must have either All.dll or both of Windows.dll and Mono.dll",
			"All.dll must not have any references to Microsoft.Xna.Framework or FNA",
			"Windows.dll must reference the windows Terraria.exe and Microsoft.Xna.Framework.dll",
			"Mono.dll must reference a non-windows Terraria.exe and FNA.dll"
		};

		internal static void LogModReferenceError(Exception e, string modName)
		{
			Directory.CreateDirectory(LogPath);
			var errorText = "Mod reference " + modName + " " + e;
			File.WriteAllText(CompileErrorPath, errorText);
			Console.WriteLine(errorText);

			Interface.errorMessage.SetMessage(errorText);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(CompileErrorPath);
		}

	    internal static void LogBuildFileError(Exception e, string filePath) {
            Directory.CreateDirectory(LogPath);
            var errorText = "Failed to load " + filePath + Environment.NewLine + e;
            File.WriteAllText(CompileErrorPath, errorText);
            Console.WriteLine(errorText);

            Interface.errorMessage.SetMessage(errorText);
            Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
            Interface.errorMessage.SetFile(CompileErrorPath);
        }

		internal static void LogCompileErrors(CompilerErrorCollection errors)
		{
			string errorHeader = "An error ocurred while compiling a mod." + Environment.NewLine + Environment.NewLine;
			Console.WriteLine(errorHeader);
			Directory.CreateDirectory(LogPath);
			CompilerError displayError = null;
			using (var writer = File.CreateText(CompileErrorPath))
			{
				foreach (CompilerError error in errors)
				{
					writer.WriteLine(error + Environment.NewLine);
					Console.WriteLine(error + Environment.NewLine);
					if (!error.IsWarning && displayError == null)
						displayError = error;
				}
			}
			Interface.errorMessage.SetMessage(errorHeader + displayError);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(CompileErrorPath);
		}

		internal static void LogDllBuildError(string modDir)
		{
			Directory.CreateDirectory(LogPath);
			var errorText = "Missing dll files for " + Path.GetFileName(modDir) + Environment.NewLine + Environment.NewLine +
			                string.Join(Environment.NewLine, buildDllLines);
			File.WriteAllText(CompileErrorPath, errorText);
            Console.WriteLine(errorText);

            Interface.errorMessage.SetMessage(errorText);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(CompileErrorPath);
		}

		internal static void LogDependencyError(string error)
		{
			Directory.CreateDirectory(LogPath);
            string file = Path.Combine(LogPath, "Loading Errors.txt");
            File.WriteAllText(file, error);
            Console.WriteLine(error);

            Interface.errorMessage.SetMessage(error);
			Interface.errorMessage.SetGotoMenu(Interface.reloadModsID);
			Interface.errorMessage.SetFile(file);
		}

		internal static void LogLoadingError(string modFile, Version modBuildVersion, Exception e)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
				Exception inner = e.InnerException;
				while (inner != null)
				{
					writer.WriteLine();
					writer.WriteLine("Inner Exception:");
					writer.WriteLine(inner.Message);
					writer.WriteLine(inner.StackTrace);
					inner = inner.InnerException;
				}
			}
			string message = "An error occurred while loading " + modFile;
			if (modBuildVersion != ModLoader.version)
			{
				message += "\nIt has been detected that this mod was built for tModLoader v" + modBuildVersion;
				message += "\nHowever, you are using " + ModLoader.versionedName;
			}
			if (modFile == "recipes")
			{
				message += "\nThe offending mod should have been automatically disabled.";
			}
			else
			{
				message += "\nThis mod has automatically been disabled.";
			}
			message += "\n\n" + e.Message + "\n" + e.StackTrace;
			if (Main.dedServ)
			{
				Console.WriteLine(message);
			}
			Interface.errorMessage.SetMessage(message);
			Interface.errorMessage.SetGotoMenu(Interface.reloadModsID);
			Interface.errorMessage.SetFile(file);
		}
		//add try catch to Terraria.WorldGen.worldGenCallBack
		//add try catch to Terraria.WorldGen.playWorldCallBack
		//add try catch to Terraria.Main.Update
		//add try catch to Terraria.Main.Draw
		internal static void LogException(Exception e)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Runtime Error.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
				Exception inner = e.InnerException;
				while (inner != null)
				{
					writer.WriteLine();
					writer.WriteLine("Inner Exception:");
					writer.WriteLine(inner.Message);
					writer.WriteLine(inner.StackTrace);
					inner = inner.InnerException;
				}
			}
			Interface.errorMessage.SetMessage("The game has crashed!\n\n" + e.Message + "\n" + e.StackTrace);
			Interface.errorMessage.SetGotoMenu(0);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		internal static void LogModBrowserException(Exception e)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Runtime Error.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
				Exception inner = e.InnerException;
				while (inner != null)
				{
					writer.WriteLine();
					writer.WriteLine("Inner Exception:");
					writer.WriteLine(inner.Message);
					writer.WriteLine(inner.StackTrace);
					inner = inner.InnerException;
				}
			}
			Interface.errorMessage.SetMessage("The game has crashed accessing Web Resources!\n\n" + e.Message + "\n" + e.StackTrace);
			Interface.errorMessage.SetGotoMenu(0);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		internal static void LogModPublish(string message)
		{
			string file = LogPath + Path.DirectorySeparatorChar + "Network Error.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(message);
			}
			Interface.errorMessage.SetMessage("The Mod Browser server response:\n\n" + message);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		internal static void LogModUnPublish(string message)
		{
			string file = LogPath + Path.DirectorySeparatorChar + "Network Error.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(message);
			}
			Interface.errorMessage.SetMessage("The Mod Browser server response:\n\n" + message);
			Interface.errorMessage.SetGotoMenu(Interface.managePublishedID);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

        public static void Log(string message)
		{
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
			{
				writer.WriteLine(message);
			}
		}

		public static void ClearLog()
		{
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.CreateText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
			{
			}
		}
	}
}

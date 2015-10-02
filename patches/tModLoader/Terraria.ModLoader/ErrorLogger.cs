using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ErrorLogger
	{
		public static readonly string LogPath = Main.SavePath + Path.DirectorySeparatorChar + "Logs";
		private static readonly string[] buildDllLines = new string[]
		{
			"Must have either All.dll or both of Windows.dll and Other.dll",
			"All.dll must not have any references to Microsoft.Xna.Framework or FNA",
			"Windows.dll must reference the windows Terraria.exe and Microsoft.Xna.Framework.dll",
			"Other.dll must reference a non-windows Terraria.exe and FNA.dll"
		};

		internal static void LogModReferenceError(string reference)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
			string message = "Mod reference " + reference + " was not found.";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(message);
			}
			Interface.errorMessage.SetMessage(message);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(file);
		}

		internal static void LogCompileErrors(CompilerErrorCollection errors)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				foreach (CompilerError error in errors)
				{
					writer.WriteLine(error.ToString());
					writer.WriteLine();
				}
			}
			string errorHeader = "An error ocurred while compiling a mod." + Environment.NewLine + Environment.NewLine;
			Interface.errorMessage.SetMessage(errorHeader + errors[0]);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(file);
		}

		internal static void LogDllBuildError(string modDir)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
			string errorText = "";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine("Missing dll files for " + Path.GetFileName(modDir));
				errorText += "Missing dll files for " + Path.GetFileName(modDir) + Environment.NewLine;
				writer.WriteLine();
				errorText += Environment.NewLine;
				foreach (string line in buildDllLines)
				{
					writer.WriteLine(line);
					errorText += line + Environment.NewLine;
				}
			}
			Interface.errorMessage.SetMessage(errorText);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(file);
		}

		internal static void LogMissingLoadReference(IList<TmodFile> mods)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
			string message = "The following mods were missing mod dependencies. They have been automatically disabled.\n";
			foreach (TmodFile modFile in mods)
			{
				message += Path.GetFileNameWithoutExtension(modFile.Name) + "\n";
			}
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.Write(message);
			}
			Interface.errorMessage.SetMessage(message);
			Interface.errorMessage.SetGotoMenu(Interface.reloadModsID);
			Interface.errorMessage.SetFile(file);
		}

		internal static void LogLoadingError(string modFile, Exception e)
		{
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
			using (StreamWriter writer = File.CreateText(file))
			{
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
			}
			string message = "An error occurred while loading " + Path.GetFileNameWithoutExtension(modFile);
			if (modFile == "recipes")
			{
				message += "\nThe offending mod should have been automatically disabled.";
			}
			else
			{
				message += "\nThis mod has automatically been disabled.";
			}
			message += "\n\n" + e.Message + "\n" + e.StackTrace;
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

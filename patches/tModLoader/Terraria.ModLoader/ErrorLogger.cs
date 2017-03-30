using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using System.Reflection;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This class consists of functions that write error messages to text files for you to read. It also lets you write logs to text files.
	/// </summary>
	public static class ErrorLogger
	{
		/// <summary>
		/// The file path to which logs are written and stored.
		/// </summary>
		public static readonly string LogPath = Path.Combine(Main.SavePath, "Logs");
		internal static string CompileErrorPath = Path.Combine(LogPath, "Compile Errors.txt");
		private static readonly string[] buildDllLines =
		{
			"Must have either All.dll or both of Windows.dll and Mono.dll",
			"All.dll must not have any references to Microsoft.Xna.Framework or FNA",
			"Windows.dll must reference the windows Terraria.exe and Microsoft.Xna.Framework.dll",
			"Mono.dll must reference a non-windows Terraria.exe and FNA.dll"
		};

		internal static void LogBuildError(string errorText)
		{
			Directory.CreateDirectory(LogPath);
			File.WriteAllText(CompileErrorPath, errorText);
			Console.WriteLine(errorText);

			Interface.errorMessage.SetMessage(errorText);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Interface.errorMessage.SetFile(CompileErrorPath);
		}

		internal static void LogCompileErrors(CompilerErrorCollection errors, bool forWindows)
		{
			string errorHeader = "An error ocurred while compiling a mod." + Environment.NewLine + Environment.NewLine;
			string badInstallHint = "";
			if (!forWindows && ModLoader.windows)
			{
				badInstallHint = "It is likely that you didn't install correctly. Make sure you installed the ModCompile folder as well." + Environment.NewLine + Environment.NewLine;
			}
			Console.WriteLine(errorHeader + badInstallHint);
			Directory.CreateDirectory(LogPath);
			CompilerError displayError = null;
			using (var writer = File.CreateText(CompileErrorPath))
			{
				foreach (CompilerError error in errors)
				{
					writer.WriteLine(error + Environment.NewLine);
					Console.WriteLine(error);
					if (!error.IsWarning && displayError == null)
						displayError = error;
				}
			}
			Interface.errorMessage.SetMessage(errorHeader + badInstallHint + displayError);
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

		internal static void LogLoadingError(string modFile, Version modBuildVersion, Exception e, bool recipes = false)
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
			string message;
			if (recipes)
			{
				message = "An error occurred while adding recipes for " + modFile;
			}
			else
			{
				message = "An error occurred while loading " + modFile;
			}
			if (modBuildVersion != ModLoader.version)
			{
				message += "\nIt has been detected that this mod was built for tModLoader v" + modBuildVersion;
				message += "\nHowever, you are using " + ModLoader.versionedName;
			}
			message += "\nThis mod has automatically been disabled.";
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
		internal static void LogException(Exception e, string msg = "The game has crashed!")
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
			Interface.errorMessage.SetMessage(msg + "\n\n" + e.Message + "\n" + e.StackTrace);
			Interface.errorMessage.SetGotoMenu(0);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		internal static void LogModBrowserException(Exception e) => LogException(e, "The game has crashed accessing Web Resources!");

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

		internal static void LogMissingMods(string msg)
		{
			Directory.CreateDirectory(LogPath);
			string file = Path.Combine(LogPath, "Missing Mods.txt");
			File.WriteAllText(file, msg);
			Console.WriteLine(msg);

			Interface.errorMessage.SetMessage(msg);
			Interface.errorMessage.SetGotoMenu(0);
			Interface.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = Interface.errorMessageID;
		}

		/// <summary>
		/// You can use this method for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder.
		/// </summary>
		public static void Log(string message)
		{
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
			{
				writer.WriteLine(message);
			}
		}

		/// <summary>
		/// Allows you to log an object for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder. 
		/// </summary>
		/// <param name="param">The object to be logged.</param>
		/// <param name="alternateOutput">If true, the object's data will be manually retrieved and logged. If false, the object's ToString method is logged.</param>
		public static void Log(object param, bool alternateOutput = false)
		{
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
			{
				if (!alternateOutput)
				{
					writer.WriteLine(param.ToString());
				}
				else
				{
					writer.WriteLine("Object type: " + param.GetType());
					foreach (PropertyInfo property in param.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						writer.Write("PROPERTY " + property.Name + " = " + property.GetValue(param, null) + "\n");
					}

					foreach (FieldInfo field in param.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						writer.Write("FIELD " + field.Name + " = " + (field.GetValue(param).ToString() != "" ? field.GetValue(param) : "(Field value not found)") + "\n");
					}

					foreach (MethodInfo method in param.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						writer.Write("METHOD " + method.Name + "\n");
					}

					int temp = 0;

					foreach (ConstructorInfo constructor in param.GetType().GetConstructors(BindingFlags.Public | BindingFlags.NonPublic))
					{
						temp++;
						writer.Write("CONSTRUCTOR " + temp + " : " + constructor.Name + "\n");
					}
				}
			}
		}

		/// <summary>
		/// Deletes all text in the Logs.txt file.
		/// </summary>
		public static void ClearLog()
		{
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.CreateText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
			{
			}
		}
	}
}

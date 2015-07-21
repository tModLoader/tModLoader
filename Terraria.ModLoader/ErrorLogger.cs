using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Terraria.ModLoader {
public static class ErrorLogger
{
    public static readonly string LogPath = Main.SavePath + Path.DirectorySeparatorChar + "Logs";

    internal static void LogCompileErrors(CompilerErrorCollection errors)
    {
        Directory.CreateDirectory(LogPath);
        string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
        StreamWriter writer = File.CreateText(file);
        foreach (CompilerError error in errors)
        {
            writer.WriteLine(error.ToString());
            writer.WriteLine();
        }
        writer.Close();
        Interface.errorMessage.SetMessage("An error occurred while compiling a mod.\n\n" + errors[0]);
        Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
        Interface.errorMessage.SetFile(file);
    }

    internal static void LogLoadingError(string modFile, Exception e)
    {
        Directory.CreateDirectory(LogPath);
        string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
        StreamWriter writer = File.CreateText(file);
        writer.WriteLine(e.Message);
        writer.WriteLine(e.StackTrace);
        writer.Close();
        string message = "An error occurred while loading " + Path.GetFileNameWithoutExtension(modFile);
        if (modFile != "recipes")
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
        StreamWriter writer = File.CreateText(file);
        writer.WriteLine(e.Message);
        writer.WriteLine(e.StackTrace);
        writer.Close();
        Interface.errorMessage.SetMessage("The game has crashed!\n\n" + e.Message + "\n" + e.StackTrace);
        Interface.errorMessage.SetGotoMenu(0);
        Interface.errorMessage.SetFile(file);
        Main.gameMenu = true;
        Main.menuMode = Interface.errorMessageID;
    }

    public static void Log(string message)
    {
        Directory.CreateDirectory(LogPath);
        StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt");
        writer.WriteLine(message);
        writer.Close();
    }

    public static void ClearLog()
    {
        Directory.CreateDirectory(LogPath);
        StreamWriter writer = File.CreateText(LogPath + Path.DirectorySeparatorChar + "Logs.txt");
        writer.Close();
    }
}}

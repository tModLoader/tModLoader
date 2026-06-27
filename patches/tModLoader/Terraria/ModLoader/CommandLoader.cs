using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader;

//todo: further documentation
/// <summary>
/// This serves as the central class from which ModCommand functions are supported and carried out.
/// </summary>
public static class CommandLoader
{
	internal static readonly IDictionary<string, List<ModCommand>> Commands = new Dictionary<string, List<ModCommand>>(StringComparer.OrdinalIgnoreCase);

	public static bool Matches(CommandType commandType, CommandType callerType)
	{
		if ((commandType & CommandType.World) != 0)
			if (Main.netMode == 2)
				commandType |= CommandType.Server;
			else if (Main.netMode == 0)
				commandType |= CommandType.Chat;

		return (callerType & commandType) != 0;
	}

	internal static void Add(ModCommand cmd)
	{
		if (!Commands.TryGetValue(cmd.Command, out List<ModCommand> cmdList))
			Commands.Add(cmd.Command, cmdList = new List<ModCommand>());

		cmdList.Add(cmd);
	}

	internal static void Unload()
	{
		Commands.Clear();
	}
	/// <summary>
	/// Finds a command by name. Handles mod prefixing. Replies with error messages.
	/// </summary>
	/// <param name="caller">Handles relaying the results of the command and narrows down the search by CommandType context</param>
	/// <param name="name">The name of the command to retrieve</param>
	/// <param name="mc">The found command, or null if an error was encountered.</param>
	/// <returns>True if a ModCommand was found, or an error message was replied. False if the command is unrecognized.</returns>
	internal static bool GetCommand(CommandCaller caller, string name, out ModCommand mc)
	{
		string modName = null;
		if (name.Contains(':')) {
			var split = name.Split(':');
			modName = split[0];
			name = split[1];
		}

		mc = null;

		if (!Commands.TryGetValue(name, out List<ModCommand> cmdList))
			return false;

		cmdList = cmdList.Where(c => Matches(c.Type, caller.CommandType)).ToList();
		if (cmdList.Count == 0)
			return false;

		if (modName != null) {
			if (!ModLoader.TryGetMod(modName, out var mod)) {
				caller.Reply("Unknown Mod: " + modName, Color.Red);
			}
			else {
				mc = cmdList.SingleOrDefault(c => c.Mod == mod);
				if (mc == null)
					caller.Reply("Mod: " + modName + " does not have a " + name + " command.", Color.Red);
			}
		}
		else if (cmdList.Count > 1) {
			caller.Reply("Multiple definitions of command /" + name + ". Try:", Color.Red);
			foreach (var c in cmdList)
				caller.Reply(c.Mod.Name + ":" + c.Command, Color.LawnGreen);
		}
		else {
			mc = cmdList[0];
		}
		return true;
	}

	internal static bool HandleCommand(string input, CommandCaller caller)
	{
		int sep = input.IndexOf(" ");
		string name = (sep >= 0 ? input.Substring(0, sep) : input).ToLower();

		if (caller.CommandType != CommandType.Console) {
			if (name == "" || name[0] != '/')
				return false;

			name = name.Substring(1);
		}

		if (!GetCommand(caller, name, out ModCommand mc))
			return false;

		if (mc == null)//error in command name (multiple commands or missing mod etc)
			return true;

		if (!mc.IsCaseSensitive)
			input = input.ToLower();

		var args = input.TrimEnd().Split(' ');
		args = args.Skip(1).ToArray();

		try {
			mc.Action(caller, input, args);
		}
		catch (Exception e) {
			var ue = e as UsageException;
			if (ue?.msg != null)
				caller.Reply(ue.msg, ue.color);
			else
				caller.Reply("Usage: " + mc.Usage, Color.Red);
		}
		return true;
	}

	public static List<Tuple<string, string>> GetHelp(CommandType type)
	{
		var list = new List<Tuple<string, string>>();
		foreach (var entry in Commands) {
			var cmdList = entry.Value.Where(mc => Matches(mc.Type, type)).ToList();
			foreach (var mc in cmdList) {
				string cmd = mc.Command;
				if (cmdList.Count > 1)
					cmd = mc.Mod.Name + ":" + cmd;

				list.Add(Tuple.Create(cmd, mc.Description));
			}
		}
		return list;
	}
}

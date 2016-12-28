using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader
{
	public static class CommandManager
	{
		internal static readonly IDictionary<string, List<ModCommand>> ChatCommands = new Dictionary<string, List<ModCommand>>();
		internal static readonly IDictionary<string, List<ModCommand>> ServerCommands = new Dictionary<string, List<ModCommand>>();

		internal static List<ModCommand> GetCommandList(CommandType type, string cmd)
		{
			List<ModCommand> cmdList;
			if (type == CommandType.Chat)
				ChatCommands.TryGetValue(cmd, out cmdList);
			else
				ServerCommands.TryGetValue(cmd, out cmdList);
			return cmdList;
		}

		internal static void Add(ModCommand cmd)
		{
			var cmdList = GetCommandList(cmd.Type, cmd.Command);
			if (cmdList != null)
				cmdList.Add(cmd);
			else if (cmd.Type == CommandType.Chat)
				ChatCommands.Add(cmd.Command, new List<ModCommand>() {cmd});
			else
				ServerCommands.Add(cmd.Command, new List<ModCommand>() {cmd});
		}

		internal static void Unload()
		{
			ChatCommands.Clear();
			ServerCommands.Clear();
		}

		internal static string[] Parse(string text) => text.Substring(1).Split(' ');

		internal static void PrintUsage(ModCommand mc) => Main.NewText("Usage: " + mc.Usage);

		internal static void ProcessInput(string input, CommandType type, ref bool show)
		{
			var cmd = Parse(input);
			if (input.Length <= 0) return;
			var cmdList = GetCommandList(type, cmd[0]);
			if (cmdList == null) return;
			var args = cmd.Skip(1).ToArray();
			var mc = cmdList[0];

			show = mc.Show;
			if (cmdList.Count > 1)
				Main.NewText("Error: Multiple definitions of command /" + mc.Command);
			else if (!mc.VerifyArguments(args))
				PrintUsage(mc);
			else
			{
					try
					{
						mc.Action(args);
					}
					catch
					{
						PrintUsage(mc);
					}
			}
		}
	}
}

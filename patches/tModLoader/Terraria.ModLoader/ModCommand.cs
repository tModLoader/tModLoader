using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	[Flags]
	public enum CommandType
	{
		Chat = 1, //client command
		Server = 2, //server command
		Console = 4, //console command
		World = 8 //singleplayer ? Chat : Server
	}

	public interface CommandCaller
	{
		CommandType CommandType { get; }
		Player Player { get; }
		void Reply(string text, Color color = default(Color));
	}

	public abstract class ModCommand
	{
		public Mod Mod { get; internal set; }
		public string Name { get; internal set; }
		public abstract string Command { get; }
		public abstract CommandType Type { get; }
		public virtual string Usage => "/" + Command;
		public virtual string Description => "";
		public virtual bool Autoload(ref string name) => Mod.Properties.Autoload;
		public abstract void Action(CommandCaller caller, string input, string[] args);
	}

	public class UsageException : Exception
	{
		internal string msg;
		internal Color color = Color.Red;

		public UsageException() { }

		public UsageException(string msg)
		{
			this.msg = msg;
		}

		public UsageException(string msg, Color color)
		{
			this.msg = msg;
			this.color = color;
		}
	}

	internal class ChatCommandCaller : CommandCaller
	{
		public CommandType CommandType => CommandType.Chat;
		public Player Player => Main.player[Main.myPlayer];

		public void Reply(string text, Color color = default(Color))
		{
			if (color == default(Color))
				color = Color.White;
			foreach (var line in text.Split('\n'))
				Main.NewText(line, color.R, color.G, color.B);
		} 
	}

	internal class PlayerCommandCaller : CommandCaller
	{
		public PlayerCommandCaller(Player player)
		{
			Player = player;
		}
		public CommandType CommandType => CommandType.Server;

		public Player Player { get; }

		public void Reply(string text, Color color = default(Color))
		{
			if (color == default(Color))
				color = Color.White;
			foreach (var line in text.Split('\n'))
				NetMessage.SendData(25, Player.whoAmI, -1, line, 255, color.R, color.G, color.B);
		}
	}

	internal class ConsoleCommandCaller : CommandCaller
	{
		public CommandType CommandType => CommandType.Console;
		public Player Player => null;

		public void Reply(string text, Color color = default(Color))
		{
			foreach (var line in text.Split('\n'))
				Console.WriteLine(line);
		}
	}
}

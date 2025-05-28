using Microsoft.Xna.Framework;
using System;
using Terraria.Chat;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>A flag enum representing context where this command operates.</summary>
[Flags]
public enum CommandType
{
	/// <summary>Command can be used in Chat in SP and MP.</summary>
	Chat = 1,
	/// <summary>Command is executed by server in MP.</summary>
	Server = 2,
	/// <summary>Command can be used in server console during MP.</summary>
	Console = 4,
	/// <summary>Command can be used in Chat in SP and MP, but executes on the Server in MP. (SinglePlayer ? Chat : Server)</summary>
	World = 8
}

public interface CommandCaller
{
	CommandType CommandType { get; }

	/// <summary>
	/// The Player object corresponding to the Player that invoked this command. Use this when the Player is needed. Don't use Main.LocalPlayer because that would be incorrect for various CommandTypes.
	/// </summary>
	Player Player { get; }

	/// <summary>
	/// Use this to respond to the Player that invoked this command. This method handles writing to the console, writing to chat, or sending messages over the network for you depending on the CommandType used. Avoid using Main.NewText, Console.WriteLine, or NetMessage.SendChatMessageToClient directly because the logic would change depending on CommandType.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="color"></param>
	void Reply(string text, Color color = default(Color));
}

/// <summary>
/// This class represents a chat or console command. Use the CommandType to specify the scope of the command.
/// </summary>
public abstract class ModCommand : ModType
{
	/// <summary>The desired text to trigger this command.</summary>
	public abstract string Command { get; }
	/// <summary>A flag enum representing context where this command operates.</summary>
	public abstract CommandType Type { get; }
	/// <summary>A short usage explanation for this command.</summary>
	public virtual string Usage => "/" + Command;
	/// <summary>A short description of this command.</summary>
	public virtual string Description => "";
	///<summary>If false (default), the arguments to <see cref="Action"/> will be provided as lowercase.</summary>
	public virtual bool IsCaseSensitive => false;

	protected override sealed void Register()
	{
		CommandLoader.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>The code that is executed when the command is triggered.</summary>
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
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(line), color, Player.whoAmI);
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

using System;
using ReLogic.Reflection;

namespace Terraria.ID;

/// <summary>
/// Correspond to <see cref="Main.netMode"/> values.
/// </summary>
public static class NetmodeID
{
	public const int SinglePlayer = 0;
	public const int MultiplayerClient = 1;
	public const int Server = 2;
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(NetmodeID), typeof(int)); // Added By TML
}

#if DEBUG
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader.Packets;

namespace Terraria.ModLoader.Default;

internal sealed class DebugNetPacketTestCommand : ModCommand
{
	public override CommandType Type => CommandType.Chat;

	public override string Command => "/debugpackettest";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		byte[] array = new byte[sizeof(int) * 8];
		Main.rand.NextBytes(array);

		new DebugNetPacketTestPacket(array).SendToServer();

		var span = MemoryMarshal.Cast<byte, int>(array);
		for (int i = 0; i < span.Length; i++) {
			Main.NewText($"I{i}: {span[i]}");
		}
	}
}

[NetPacket(typeof(ModLoaderMod))]
internal partial record struct DebugNetPacketTestPacket(int[] Array)
{
	public DebugNetPacketTestPacket(ReadOnlySpan<byte> array) : this(MemoryMarshal.Cast<byte, int>(array).ToArray())
	{
	}

	public void HandlePacket(int sender)
	{
		for (int i = 0; i < Array.Length; i++) {
			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"O{i}: {Array[i]}"), Color.White);
		}
	}
}
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader;

[TestClass]
[DoNotParallelize]
public class PacketFramingTests
{
	private static string originalSavePath;

	[ClassInitialize]
	public static void InitializeClass(TestContext context) {
		originalSavePath = Program.SavePath;
		// Main's static initialization constructs paths even in a headless test.
		Program.SavePath ??= context.TestRunDirectory;
	}

	[ClassCleanup]
	public static void CleanupClass() => Program.SavePath = originalSavePath;

	private MessageBuffer[] originalBuffers;
	private RemoteClient[] originalClients;
	private bool originalDisconnect;
	private bool originalDedicatedServer;
	private int originalNetMode;
	private Hook dispatchHook;
	private bool stateReplaced;
	private readonly Dictionary<MessageBuffer, List<byte[]>> packets = new();
	private int dispatches;
	private delegate void Dispatch(MessageBuffer self, int offset, int length, out int messageType);

	[TestInitialize]
	public void Initialize() {
		originalBuffers = NetMessage.buffer;
		originalClients = Netplay.Clients;
		originalDisconnect = Netplay.Disconnect;
		originalDedicatedServer = Main.dedServ;
		originalNetMode = Main.netMode;
		NetMessage.buffer = new MessageBuffer[257];
		NetMessage.buffer[0] = new();
		NetMessage.buffer[1] = new();
		NetMessage.buffer[256] = new();
		Netplay.Clients = new RemoteClient[] { new() { Id = 0 }, new() { Id = 1 } };
		stateReplaced = true;
		foreach (var buffer in NetMessage.buffer.Where(buffer => buffer != null)) {
			buffer.ResetReader();
			packets.Add(buffer, new());
		}
		Netplay.Disconnect = false;
		Main.dedServ = true;
		Main.netMode = 2;
		// Exercise the checkout's real framing and termination code. Only payload
		// dispatch is replaced, avoiding world/content initialization and sockets.
		dispatchHook = new Hook(typeof(MessageBuffer).GetMethod(nameof(MessageBuffer.GetData)), (Dispatch)RecordPacket);
	}

	private void RecordPacket(MessageBuffer self, int offset, int length, out int messageType) {
		messageType = self.readBuffer[offset];
		// CheckBytes catches this exception. Bound the original zero-length loop
		// so removing the production guard causes an assertion failure, not a hang.
		if (++dispatches > 100)
			throw new InvalidOperationException("Non-progressing parser exceeded dispatch budget");
		if (length > 0)
			packets[self].Add(self.readBuffer.AsSpan(offset, length).ToArray());
	}

	[TestCleanup]
	public void Cleanup() {
		dispatchHook?.Dispose();
		if (!stateReplaced)
			return;
		foreach (var buffer in NetMessage.buffer.Where(buffer => buffer != null))
			buffer.reader?.Dispose();
		NetMessage.buffer = originalBuffers;
		Netplay.Clients = originalClients;
		Netplay.Disconnect = originalDisconnect;
		Main.dedServ = originalDedicatedServer;
		Main.netMode = originalNetMode;
	}

	private static void Feed(byte[] bytes, int slot = 0) {
		var buffer = NetMessage.buffer[slot];
		bytes.CopyTo(buffer.readBuffer, buffer.totalData);
		buffer.totalData += bytes.Length;
		buffer.checkBytes = true;
		NetMessage.CheckBytes(slot);
	}

	private static void Rejected(int slot = 0) {
		Assert.IsTrue(Netplay.Clients[slot].PendingTermination, "connection not terminated");
		Assert.IsTrue(Netplay.Clients[slot].PendingTerminationApproved, "termination not approved");
		Assert.IsTrue(NetMessage.buffer[slot].totalData == 0, "bad bytes retained");
		Assert.IsTrue(!NetMessage.buffer[slot].checkBytes, "buffer still pending");
	}

	[DataTestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	public void RejectInvalidLengthBeforeDispatch(int length) {
		Feed(new byte[] { (byte)length, 0, 0, 0 });
		Assert.IsTrue(dispatches == 0, "invalid packet dispatched " + dispatches + " times");
		Rejected();
	}

	[TestMethod]
	public void CapturedSmbProbe() {
		Feed(Convert.FromHexString("000000A4FF534D4272000000000801400000000000000000000000000000400600000100008100025043204E4554574F524B2050524F4752414D20312E3000024D4943524F534F4654204E4554574F524B5320312E303300024D4943524F4654204E4554574F524B5320332E3000024C414E4D414E312E3000024C4D312E3258303032000253616D626100024E54204C414E4D414E20312E3000024E54204C4D20302E313200"));
		Assert.IsTrue(dispatches == 0, "SMB probe reached message dispatcher");
		Rejected();
	}

	[TestMethod]
	public void SplitLengthHeader() {
		Feed(new byte[] { 3 });
		Assert.IsTrue(NetMessage.buffer[0].totalData == 1, "partial header lost");
		Feed(new byte[] { 0, 42 });
		Assert.IsTrue(packets[NetMessage.buffer[0]].Count == 1, "packet missing");
	}

	[TestMethod]
	public void SplitPayload() {
		Feed(new byte[] { 5, 0, 42 });
		Assert.IsTrue(dispatches == 0, "incomplete packet dispatched");
		Feed(new byte[] { 7, 8 });
		Assert.IsTrue(packets[NetMessage.buffer[0]].Single().SequenceEqual(new byte[] { 42, 7, 8 }), "payload changed");
	}

	[TestMethod]
	public void CoalescedPackets() {
		Feed(new byte[] { 3, 0, 42, 4, 0, 43, 99 });
		Assert.IsTrue(packets[NetMessage.buffer[0]].Count == 2, "packet count");
		Assert.IsTrue(NetMessage.buffer[0].totalData == 0, "bytes retained");
	}

	[TestMethod]
	public void CompletePacketFollowedByPartial() {
		Feed(new byte[] { 3, 0, 42, 4, 0, 43 });
		Assert.IsTrue(packets[NetMessage.buffer[0]].Count == 1, "packet count");
		Assert.IsTrue(NetMessage.buffer[0].totalData == 3, "partial lost");
		Feed(new byte[] { 99 });
		Assert.IsTrue(packets[NetMessage.buffer[0]][1].SequenceEqual(new byte[] { 43, 99 }), "reassembly wrong");
	}

	[TestMethod]
	public void BadFrameAfterValidFrame() {
		Feed(new byte[] { 3, 0, 42, 0, 0, 0 });
		Assert.IsTrue(dispatches == 1, "invalid frame dispatched");
		Rejected();
	}

	[TestMethod]
	public void AnotherConnectionRemainsUsable() {
		Feed(new byte[] { 0, 0, 0 });
		Feed(new byte[] { 3, 0, 42 }, 1);
		Assert.IsTrue(packets[NetMessage.buffer[1]].Count == 1, "other client blocked");
		Assert.IsTrue(!Netplay.Clients[1].PendingTermination, "other client terminated");
	}

	[DataTestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(2)]
	public void ClientRejectsMalformedServerFrame(int length) {
		Main.dedServ = false;
		Main.netMode = 1;
		Feed(new byte[] { (byte)length, 0, 0 }, 256);
		Assert.AreEqual(0, dispatches, "invalid server frame dispatched");
		Assert.IsTrue(Netplay.Disconnect, "client did not disconnect");
		Assert.AreEqual(0, NetMessage.buffer[256].totalData, "bytes retained");
		Assert.IsFalse(NetMessage.buffer[256].checkBytes, "buffer still pending");
	}

	[TestMethod]
	public void ValidFrameDoesNotDisconnect() {
		Feed(new byte[] { 3, 0, 42 });
		Assert.IsTrue(!Netplay.Clients[0].PendingTermination && !Netplay.Disconnect, "valid connection disconnected");
	}
}

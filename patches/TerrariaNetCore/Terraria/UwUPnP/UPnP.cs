using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria;

namespace UwUPnP;

public enum Protocol
{
	Unknown = 0,
	TCP = 1,
	UDP = 2
}

/// <summary>
/// A simple UPnP library
/// See: https://github.com/Rartrin/UwUPnP
/// </summary>
public static class UPnP
{
	private static bool gatewayNotYetRequested = true;
	private static bool searching = false;
	private static Gateway defaultGateway = null;

	private static Gateway Gateway {
		get {
			if (gatewayNotYetRequested) {
				gatewayNotYetRequested = false;
				FindGateway();
			}

			// bool is expected to be atomic, strangely not optimized out...
			// probably a locking mechanism might work better?
			while (searching) {
				Thread.Sleep(1);
			}

			return defaultGateway;
		}
	}

	public static bool IsAvailable => Gateway is not null;

	public static IPAddress ExternalIP => Gateway?.ExternalIPAddress;
	public static IPAddress LocalIP => Gateway?.InternalClient;

	public static void Open(Protocol protocol, ushort externalPort, ushort? internalPort = null, string description = null) =>
		Gateway?.AddPortMapping(externalPort, protocol, internalPort, description);

	public static void Close(Protocol protocol, ushort externalPort) =>
		Gateway?.DeletePortMapping(externalPort, protocol);

	public static bool IsOpen(Protocol protocol, ushort externalPort) =>
		Gateway?.SpecificPortMappingExists(externalPort, protocol) ?? false;

	public static Dictionary<string, string> GetGenericPortMappingEntry(int portMappingIndex) =>
		Gateway?.GetGenericPortMappingEntry(portMappingIndex);

	private static void FindGateway()
	{
		searching = true;
		List<Task> listeners = new List<Task>();

		foreach (var ip in GetLocalIPs()) {
			listeners.Add(Task.Run(() => StartListener(ip)));
		}

		Task.WhenAll(listeners).ContinueWith(t => searching = false);
	}

	private static void StartListener(IPAddress ip)
	{
		if (Gateway.TryNew(ip, out Gateway gateway)) {
			Interlocked.CompareExchange(ref defaultGateway, gateway, null);
			searching = false;
		}
	}

	private static IEnumerable<IPAddress> GetLocalIPs() =>
		NetworkInterface.GetAllNetworkInterfaces().Where(IsValidInterface).SelectMany(GetValidNetworkIPs);

	// TODO: Filter out virtual/sub-interfaces (like for VMs).
	private static bool IsValidInterface(NetworkInterface network) =>
		network.OperationalStatus == OperationalStatus.Up
		&& network.NetworkInterfaceType != NetworkInterfaceType.Loopback
		&& network.NetworkInterfaceType != NetworkInterfaceType.Ppp;

	private static IEnumerable<IPAddress> GetValidNetworkIPs(NetworkInterface network) =>
		network.GetIPProperties().UnicastAddresses
		.Select(a => a.Address)
		.Where(a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6);
}
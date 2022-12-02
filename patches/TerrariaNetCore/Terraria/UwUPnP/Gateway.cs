using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;

namespace UwUPnP;

// UPnP Specifications http://upnp.org/specs/gw/UPnP-gw-WANIPConnection-v1-Service.pdf
// Helpful overview http://www.upnp-hacks.org/igd.html
// Port tester https://www.yougetsignal.com/tools/open-ports/

internal sealed class Gateway
{
	public IPAddress InternalClient { get; }

	private readonly string serviceType = null;
	private readonly string controlURL = null;

	// Allow user to set this env var in case they have some weird
	// hardware that only accepts LF-seperated SSDP header lines (this
	// breaks the spec, but it may well happen still).
	private static readonly string ssdpLineSep = Environment.GetEnvironmentVariable("SSDP_HEADER_USE_LF") == "1" ? "\n" : "\r\n";

	private Gateway(IPAddress ip, string data)
	{
		InternalClient = ip;

		string location = GetLocation(data);

		(serviceType, controlURL) = GetInfo(location);
	}

	private static readonly string[] searchMessageTypes = new[] {
		"urn:schemas-upnp-org:device:InternetGatewayDevice:1",
		"urn:schemas-upnp-org:service:WANIPConnection:1",
		"urn:schemas-upnp-org:service:WANPPPConnection:1"
	};

	public static bool TryNew(IPAddress ip, out Gateway gateway)
	{
		IPEndPoint endPoint = IPEndPoint.Parse("239.255.255.250:1900");
		// Sockets are disposable so wrap them
		using var socket = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp) {
			ReceiveTimeout = 3000,
			SendTimeout = 3000
		};
		socket.Bind(new IPEndPoint(ip, 0));

		byte[] buffer = new byte[0x600];

		foreach (string type in searchMessageTypes) {
			string request = string.Join(
				ssdpLineSep,

				"M-SEARCH * HTTP/1.1",
				"HOST: 239.255.255.250:1900",
				$"ST: {type}",
				"MAN: \"ssdp:discover\"",
				"MX: 2",
				"", "" // End with double newlines
			);
			byte[] req = Encoding.ASCII.GetBytes(request);

			try {
				socket.SendTo(req, endPoint);
			}
			catch (SocketException) {
				gateway = null;
				return false;
			}

			const int maxReceives = 20;
			int receivedCount = 0;

			//   Naughty devices on the network may respond to all SSDP
			// discover requests, even though they don't match the
			// requested ST! (Philips Hue does this, which is diabolical).
			//
			//   We may receive a lot of responses, and it's worth reading
			// them all to try and find a useful one - set a limit in
			// case they don't stop coming to prevent infinite looping.
			for (int i = 0; i < maxReceives; i++) {
				try {
					receivedCount = socket.Receive(buffer);
				}
				catch (SocketException) {
					// Timeout, so probably no more responses to wait for
					//  - proceed to next ST to try.
					break;
				}

				try {
					gateway = new Gateway(ip, Encoding.ASCII.GetString(buffer, 0, receivedCount));
					return true;
				}
				catch {
					//   Invalid gateway, keep reading further responses
					// (or go to next ST to try).
					gateway = null;
					continue;
				}
			}
		}

		gateway = null;
		return false;
	}

	private static string GetLocation(string data)
	{
		var lines = data.Split('\n').Select(l=>l.Trim()).Where(l=>l.Length>0);

		foreach (string line in lines) {
			if (line.StartsWith("HTTP/1.") || line.StartsWith("NOTIFY *"))
				continue;

			int colonIndex = line.IndexOf(':');

			if (colonIndex < 0)
				continue;

			string name = line[..colonIndex];
			string val = line.Length >= name.Length ? line[(colonIndex + 1)..].Trim() : null;

			if (name.ToLowerInvariant() == "location") {
				// finds the first slash after http://
				if (val.IndexOf('/', 7) == -1)
					throw new Exception("Unsupported Gateway");

				return val;
			}
		}
		throw new Exception("Unsupported Gateway");
	}

	private static (string serviceType, string controlURL) GetInfo(string location)
	{
		XDocument doc = XDocument.Load(location);
		var services =  doc.Descendants().Where(d => d.Name.LocalName == "service");

		(string serviceType, string controlURL) ret = (null,null);

		foreach (XElement service in services) {
			string serviceType = null;
			string controlURL = null;

			foreach (var node in service.Nodes()) {
				if (node is not XElement ele || ele.FirstNode is not XText n) {
					continue;
				}

				switch(ele.Name.LocalName.Trim().ToLowerInvariant()) {
					case "servicetype": serviceType = n.Value.Trim(); break;
					case "controlurl": controlURL = n.Value.Trim(); break;
				}
			}

			if (serviceType is not null && controlURL is not null) {
				if (serviceType.ToLowerInvariant().Contains(":wanipconnection:")
				|| serviceType.ToLowerInvariant().Contains(":wanpppconnection:")) {
					ret.serviceType = serviceType;
					ret.controlURL = controlURL;
				}
			}
		}

		if (ret.controlURL is null) {
			throw new Exception("Unsupported Gateway");
		}

		if (!ret.controlURL.StartsWith('/')) {
			ret.controlURL = "/" + ret.controlURL;
		}

		int slash = location.IndexOf('/', 7); // finds the first slash after http://

		ret.controlURL = location[0..slash] + ret.controlURL;

		return ret;
	}

	private static string BuildArgString((string Key, object Value) arg) => $"<{arg.Key}>{arg.Value}</{arg.Key}>";

	private Dictionary<string, string> RunCommand(string action, params (string Key, object Value)[] args)
	{
		string requestData = GetRequestData(action, args);

		HttpWebRequest request = SendRequest(action, requestData);

		return GetResponse(request);
	}

	private string GetRequestData(string action, (string Key, object Value)[] args) => string.Concat
	(
		"<?xml version=\"1.0\"?>\n",
		"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">",
			"<SOAP-ENV:Body>",
				$"<m:{action} xmlns:m=\"{serviceType}\">",
					string.Concat(args.Select(BuildArgString)),
				$"</m:{action}>",
			"</SOAP-ENV:Body>",
		"</SOAP-ENV:Envelope>"
	);

	private HttpWebRequest SendRequest(string action, string requestData)
	{
		byte[] data = Encoding.ASCII.GetBytes(requestData);

		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(controlURL);

		request.Method = "POST";
		request.ContentType = "text/xml";
		request.ContentLength = data.Length;
		request.Headers.Add("SOAPAction", $"\"{serviceType}#{action}\"");

		using Stream requestStream = request.GetRequestStream();
		requestStream.Write(data);

		return request;
	}

	private static Dictionary<string, string> GetResponse(HttpWebRequest request)
	{
		Dictionary<string, string> ret = new Dictionary<string, string>();

		try {
			using HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			if (response.StatusCode != HttpStatusCode.OK)
				return ret;

			XDocument doc = XDocument.Load(response.GetResponseStream());
			foreach (XNode node in doc.DescendantNodes()) {
				if (node is XElement ele && ele.FirstNode is XText txt) {
					ret[ele.Name.LocalName] = txt.Value;
				}
			}
		}
		catch { }

		if (ret.TryGetValue("errorCode", out string errorCode)) {
			throw new Exception(errorCode);
		}

		return ret;
	}

	public IPAddress ExternalIPAddress => RunCommand("GetExternalIPAddress").TryGetValue("NewExternalIPAddress", out string ret) ? IPAddress.Parse(ret) : null;

	public bool SpecificPortMappingExists(ushort externalPort, Protocol protocol) => RunCommand("GetSpecificPortMappingEntry",
		("NewRemoteHost", ""),
		("NewExternalPort", externalPort),
		("NewProtocol", protocol)
	).ContainsKey("NewInternalPort");

	public void AddPortMapping(ushort externalPort, Protocol protocol, ushort? internalPort = null, string description = null) => RunCommand("AddPortMapping",
		("NewRemoteHost", ""),
		("NewExternalPort", externalPort),
		("NewProtocol", protocol),
		("NewInternalClient", InternalClient),
		("NewInternalPort", internalPort??externalPort),
		("NewEnabled", 1),
		("NewPortMappingDescription", description??"UwUPnP"),
		("NewLeaseDuration", 0)
	);

	public void DeletePortMapping(ushort externalPort, Protocol protocol) => RunCommand("DeletePortMapping",
		("NewRemoteHost", ""),
		("NewExternalPort", externalPort),
		("NewProtocol", protocol)
	);

	/// <summary>2.4.14.GetGenericPortMappingEntry</summary>
	public Dictionary<string,string> GetGenericPortMappingEntry(int portMappingIndex) => RunCommand("GetGenericPortMappingEntry",
		("NewPortMappingIndex", portMappingIndex)
	);
}
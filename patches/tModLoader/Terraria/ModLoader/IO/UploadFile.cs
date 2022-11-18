using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Terraria.ModLoader.IO;

public class UploadFile
{
	public UploadFile()
	{
		ContentType = "application/octet-stream";
	}

	public string Name { get; set; }

	public string Filename { get; set; }

	public string ContentType { get; set; }

	public byte[] Content { get; set; }

	public static byte[] UploadFiles(string address, IEnumerable<UploadFile> files, NameValueCollection values)
	{
		var request = WebRequest.Create(address);
		request.Method = "POST";
		var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
		request.ContentType = "multipart/form-data; boundary=" + boundary;
		boundary = "--" + boundary;
		using (var requestStream = request.GetRequestStream()) {
			WriteValues(requestStream, values, boundary);
			WriteFiles(requestStream, files, boundary);
			var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
			requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
		}
		using (var response = request.GetResponse())
		using (var responseStream = response.GetResponseStream())
		using (var stream = new MemoryStream()) {
			responseStream.CopyTo(stream);
			return stream.ToArray();
		}
	}

	public static byte[] GetUploadFilesRequestData(IEnumerable<UploadFile> files, NameValueCollection values, string boundary)
	{
		boundary = "--" + boundary;
		using (var requestStream = new MemoryStream()) {
			WriteValues(requestStream, values, boundary);
			WriteFiles(requestStream, files, boundary);
			var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
			requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
			return requestStream.ToArray();
		}
	}

	private static void WriteValues(Stream requestStream, NameValueCollection values, string boundary)
	{
		if (values == null)
			return;

		// Write the values
		foreach (string name in values.Keys) {
			var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
			requestStream.Write(buffer, 0, buffer.Length);
			buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
			requestStream.Write(buffer, 0, buffer.Length);
			buffer = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
			requestStream.Write(buffer, 0, buffer.Length);
		}
	}

	private static void WriteFiles(Stream requestStream, IEnumerable<UploadFile> files, string boundary)
	{
		if (files == null)
			return;

		// Write the files
		foreach (var file in files) {
			var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
			requestStream.Write(buffer, 0, buffer.Length);
			buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine));
			requestStream.Write(buffer, 0, buffer.Length);
			buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
			requestStream.Write(buffer, 0, buffer.Length);
			requestStream.Write(file.Content, 0, file.Content.Length);
			buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
			requestStream.Write(buffer, 0, buffer.Length);
		}
	}
}

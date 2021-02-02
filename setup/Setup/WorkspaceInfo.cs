using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Terraria.ModLoader.Setup
{
	[XmlRoot("PropertyGroup")]
	public struct WorkspaceInfo
	{
		public string BranchName { get; set; }
		public string CommitSHA { get; set; }
		public string TerrariaSteamPath { get; set; }

		[XmlElement("tModLoaderSteamPath")]
		public string TModLoaderSteamPath { get; set; }

		public static bool Load(string filePath, out WorkspaceInfo result) {
			result = default;

			if (!File.Exists(filePath)) {
				return false;
			}

			try {
				using var xmlReader = XmlReader.Create(filePath);
				var xml = XDocument.Load(xmlReader);

				//Removes all namespace attributes. Without this, XElement will add one and screw up deserialization below.
				foreach (XElement e in xml.Descendants()) {
					e.Name = e.Name.LocalName;
				}

				var project = xml.Elements().FirstOrDefault(e => e.Name.LocalName == "Project");
				var propertyGroup = project?.Elements().FirstOrDefault(e => e.Name.LocalName == "PropertyGroup");

				if (project == null || propertyGroup == null) {
					return false;
				}

				var serializer = new XmlSerializer(typeof(WorkspaceInfo));
				using var elementReader = propertyGroup.CreateReader(ReaderOptions.OmitDuplicateNamespaces);

				result = (WorkspaceInfo)serializer.Deserialize(elementReader);

				return true;
			}
			catch {
				return false;
			}
		}

		public static void Save(WorkspaceInfo value, string filePath) {
			var serializer = new XmlSerializer(typeof(WorkspaceInfo));

			using var stringWriter = new StringWriter();
			using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings() {
				OmitXmlDeclaration = true,
				Indent = true
			});

			serializer.Serialize(xmlWriter, value, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));

			string text = Regex.Replace(stringWriter.ToString(), @"^", "  $&", RegexOptions.Multiline);

			text =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
{text}
</Project>";

			if (!File.Exists(filePath) || File.ReadAllText(filePath) != text) {
				File.WriteAllText(filePath, text);
			}
		}
	}
}

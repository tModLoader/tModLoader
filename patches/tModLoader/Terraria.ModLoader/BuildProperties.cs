using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	internal class BuildProperties
	{
		internal string modBuildVersion = "";
		internal string[] dllReferences = new string[0];
		internal string[] modReferences = new string[0];
		internal string author = "";
		internal string version = "";
		internal string displayName = "";
		internal bool noCompile = false;
		internal bool hideCode = true;
		internal bool hideResources = true;
		internal bool includeSource = false;
		internal string homepage = "";
		internal string description = "";

		internal static BuildProperties ReadBuildFile(string modDir)
		{
			string propertiesFile = modDir + Path.DirectorySeparatorChar + "build.txt";
			string descriptionfile = modDir + Path.DirectorySeparatorChar + "description.txt";
			BuildProperties properties = new BuildProperties();
			if (!File.Exists(propertiesFile))
			{
				return properties;
			}
			if (File.Exists(descriptionfile))
			{
				properties.description = File.ReadAllText(descriptionfile);
			}
			string[] lines = File.ReadAllLines(propertiesFile);
			foreach (string line in lines)
			{
				if (line.Length == 0)
				{
					continue;
				}
				int split = line.IndexOf('=');
				string property = line.Substring(0, split).Trim();
				string value = line.Substring(split + 1).Trim();
				if (value.Length == 0)
				{
					continue;
				}
				switch (property)
				{
					case "dllReferences":
						string[] dllReferences = value.Split(',');
						for (int k = 0; k < dllReferences.Length; k++)
						{
							string dllReference = dllReferences[k].Trim();
							if (dllReference.Length > 0)
							{
								dllReferences[k] = dllReference;
							}
						}
						properties.dllReferences = dllReferences;
						break;
					case "modReferences":
						string[] modReferences = value.Split(',');
						for (int k = 0; k < modReferences.Length; k++)
						{
							string modReference = modReferences[k].Trim();
							if (modReference.Length > 0)
							{
								modReferences[k] = modReference;
							}
						}
						properties.modReferences = modReferences;
						break;
					case "author":
						properties.author = value;
						break;
					case "version":
						properties.version = value;
						break;
					case "displayName":
						properties.displayName = value;
						break;
					case "homepage":
						properties.homepage = value;
						break;
					case "noCompile":
						properties.noCompile = value.ToLower() == "true";
						break;
					case "hideCode":
						properties.hideCode = value.ToLower() != "false";
						break;
					case "hideResources":
						properties.hideResources = value.ToLower() != "false";
						break;
					case "includeSource":
						properties.includeSource = value.ToLower() == "true";
						break;
				}
			}
			return properties;
		}

		internal byte[] ToBytes()
		{
			byte[] data;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(memoryStream))
				{
					if (dllReferences.Length > 0)
					{
						writer.Write("dllReferences");
						foreach (string reference in dllReferences)
						{
							writer.Write(reference);
						}
						writer.Write("");
					}
					if (modReferences.Length > 0)
					{
						writer.Write("modReferences");
						foreach (string reference in modReferences)
						{
							writer.Write(reference);
						}
						writer.Write("");
					}
					if (author.Length > 0)
					{
						writer.Write("author");
						writer.Write(author);
					}
					if (version.Length > 0)
					{
						writer.Write("version");
						writer.Write(version);
					}
					if (displayName.Length > 0)
					{
						writer.Write("displayName");
						writer.Write(displayName);
					}
					if (homepage.Length > 0)
					{
						writer.Write("homepage");
						writer.Write(homepage);
					}
					if (description.Length > 0)
					{
						writer.Write("description");
						writer.Write(description);
					}
					if (noCompile)
					{
						writer.Write("noCompile");
					}
					if (!hideCode)
					{
						writer.Write("!hideCode");
					}
					if (!hideResources)
					{
						writer.Write("!hideResources");
					}
					if (includeSource)
					{
						writer.Write("includeSource");
					}
					writer.Write("");
					writer.Flush();
					data = memoryStream.ToArray();
				}
			}
			return data;
		}

		internal static BuildProperties ReadModFile(TmodFile modFile)
		{
			BuildProperties properties = new BuildProperties();
			byte[] data;
			using (MemoryStream memoryStream = new MemoryStream(modFile.GetFile("Info")))
			{
				using (BinaryReader reader = new BinaryReader(memoryStream))
				{
					properties.modBuildVersion = reader.ReadString();
					data = reader.ReadBytes(reader.ReadInt32());
				}
			}
			if (data.Length == 0)
			{
				return properties;
			}
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				using (BinaryReader reader = new BinaryReader(memoryStream))
				{
					for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString())
					{
						if (tag == "dllReferences")
						{
							List<string> dllReferences = new List<string>();
							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString())
							{
								dllReferences.Add(reference);
							}
							properties.dllReferences = dllReferences.ToArray();
						}
						if (tag == "modReferences")
						{
							List<string> modReferences = new List<string>();
							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString())
							{
								modReferences.Add(reference);
							}
							properties.modReferences = modReferences.ToArray();
						}
						if (tag == "author")
						{
							properties.author = reader.ReadString();
						}
						if (tag == "version")
						{
							properties.version = reader.ReadString();
						}
						if (tag == "displayName")
						{
							properties.displayName = reader.ReadString();
						}
						if (tag == "homepage")
						{
							properties.homepage = reader.ReadString();
						}
						if (tag == "description")
						{
							properties.description = reader.ReadString();
						}
						if (tag == "noCompile")
						{
							properties.noCompile = true;
						}
						if (tag == "!hideCode")
						{
							properties.hideCode = false;
						}
						if (tag == "!hideResources")
						{
							properties.hideResources = false;
						}
						if (tag == "includeSource")
						{
							properties.includeSource = true;
						}
					}
				}
			}
			return properties;
		}
	}
}

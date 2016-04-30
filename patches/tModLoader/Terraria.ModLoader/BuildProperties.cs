using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	internal class BuildProperties
	{
		internal string[] dllReferences = new string[0];
		internal string[] modReferences = new string[0];
		internal string[] buildIgnores = new string[0];
		internal string author = "";
		internal Version version = new Version(1, 0);
		internal string displayName = "";
		internal bool noCompile = false;
		internal bool hideCode = true;
		internal bool hideResources = true;
		internal bool includeSource = false;
		internal bool includePDB = false;
		internal bool editAndContinue = false;
		internal int languageVersion = 4;
		internal string homepage = "";
		internal string description = "";
		internal ModSide side;

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
						properties.version = new Version(value);
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
					case "includePDB":
						properties.includePDB = value.ToLower() == "true";
						break;
					case "buildIgnore":
						string[] buildIgnores = value.Split(',');
						for (int k = 0; k < buildIgnores.Length; k++)
						{
							string buildIgnore = buildIgnores[k].Trim();
							if (buildIgnore.Length > 0)
							{
								buildIgnores[k] = buildIgnore;
							}
						}
						properties.buildIgnores = buildIgnores;
						break;
					case "languageVersion":
						if (!int.TryParse(value, out properties.languageVersion))
							throw new Exception("languageVersion not an int: "+value);

						if (properties.languageVersion < 4 || properties.languageVersion > 6)
							throw new Exception("languageVersion ("+properties.languageVersion+") must be between 4 and 6");
						break;
					case "side":
						if (!ModSide.TryParse(value, true, out properties.side))
							throw new Exception("side is not one of (Both, Client, Server, NoSync): "+value);
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
					writer.Write("version");
					writer.Write(version.ToString());
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
					if (includePDB)
					{
						writer.Write("includePDB");
					}
					if (editAndContinue)
					{
						writer.Write("editAndContinue");
					}
					if (side != ModSide.Both)
					{
						writer.Write("side");
						writer.Write((byte)side);
					}
					writer.Write("");
				}
				data = memoryStream.ToArray();
			}
			return data;
		}

		internal static BuildProperties ReadModFile(TmodFile modFile)
		{
			BuildProperties properties = new BuildProperties();
			byte[] data = modFile.GetFile("Info");
			
			if (data.Length == 0)
				return properties;
			
			using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
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
						properties.version = new Version(reader.ReadString());
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
					if (tag == "includePDB")
					{
						properties.includePDB = true;
					}
					if (tag == "editAndContinue")
					{
						properties.editAndContinue = true;
					}
					if (tag == "side")
					{
						properties.side = (ModSide) reader.ReadByte();
					}
				}
			}
			return properties;
		}

		internal bool ignoreFile(string resource)
		{
			return this.buildIgnores.Any(fileMask => FitsMask(resource, fileMask));
		}

		private bool FitsMask(string fileName, string fileMask)
		{
			string pattern =
				'^' +
				Regex.Escape(fileMask.Replace(".", "__DOT__")
								 .Replace("*", "__STAR__")
								 .Replace("?", "__QM__"))
					 .Replace("__DOT__", "[.]")
					 .Replace("__STAR__", ".*")
					 .Replace("__QM__", ".")
				+ '$';
			return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
		}
	}
}

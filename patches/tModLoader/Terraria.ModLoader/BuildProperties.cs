using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	//todo: further documentation
	internal class BuildProperties
	{
		internal struct ModReference
		{
			public string mod;
			public Version target;

			public ModReference(string mod, Version target)
			{
				this.mod = mod;
				this.target = target;
			}

			public override string ToString() => target == null ? mod : mod + '@' + target;

			public static ModReference Parse(string spec)
			{
				var split = spec.Split('@');
				if (split.Length == 1)
					return new ModReference(split[0], null);

				if (split.Length > 2)
					throw new Exception("Invalid mod reference: " + spec);

				try
				{
					return new ModReference(split[0], new Version(split[1]));
				}
				catch
				{
					throw new Exception("Invalid mod reference: " + spec);
				}
			}
		}

		internal string[] dllReferences = new string[0];
		internal ModReference[] modReferences = new ModReference[0];
		internal ModReference[] weakReferences = new ModReference[0];
		//this mod will load after any mods in this list
		//sortAfter includes (mod|weak)References that are not in sortBefore
		internal string[] sortAfter = new string[0];
		//this mod will load before any mods in this list
		internal string[] sortBefore = new string[0];
		internal string[] buildIgnores = new string[0];
		internal string author = "";
		internal Version version = new Version(1, 0);
		internal string displayName = "";
		internal bool noCompile = false;
		internal bool hideCode = false;
		internal bool hideResources = false;
		internal bool includeSource = false;
		internal bool includePDB = false;
		internal bool editAndContinue = false;
		internal int languageVersion = 4;
		internal string homepage = "";
		internal string description = "";
		internal ModSide side;

		public IEnumerable<ModReference> Refs(bool includeWeak) =>
			includeWeak ? modReferences.Concat(weakReferences) : modReferences;

		public IEnumerable<string> RefNames(bool includeWeak) => Refs(includeWeak).Select(dep => dep.mod);

		private static IEnumerable<string> ReadList(string value)
			=> value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

		private static IEnumerable<string> ReadList(BinaryReader reader)
		{
			var list = new List<string>();
			for (string item = reader.ReadString(); item.Length > 0; item = reader.ReadString())
				list.Add(item);

			return list;
		}

		private static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
		{
			foreach (var item in list)
				writer.Write(item.ToString());

			writer.Write("");
		}

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
						properties.dllReferences = ReadList(value).ToArray();
						break;
					case "modReferences":
						properties.modReferences = ReadList(value).Select(ModReference.Parse).ToArray();
						break;
					case "weakReferences":
						properties.weakReferences = ReadList(value).Select(ModReference.Parse).ToArray();
						break;
					case "sortBefore":
						properties.sortAfter = ReadList(value).ToArray();
						break;
					case "sortAfter":
						properties.sortBefore = ReadList(value).ToArray();
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
						properties.hideCode = value.ToLower() == "true";
						break;
					case "hideResources":
						properties.hideResources = value.ToLower() == "true";
						break;
					case "includeSource":
						properties.includeSource = value.ToLower() == "true";
						break;
					case "includePDB":
						properties.includePDB = value.ToLower() == "true";
						break;
					case "buildIgnore":
						properties.buildIgnores = value.Split(',').Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)).Where(s => s.Length > 0).ToArray();
						break;
					case "languageVersion":
						if (!int.TryParse(value, out properties.languageVersion))
							throw new Exception("languageVersion not an int: " + value);

						if (properties.languageVersion < 4 || properties.languageVersion > 6)
							throw new Exception("languageVersion (" + properties.languageVersion + ") must be between 4 and 6");
						break;
					case "side":
						if (!Enum.TryParse(value, true, out properties.side))
							throw new Exception("side is not one of (Both, Client, Server, NoSync): " + value);
						break;
				}
			}

			var refs = properties.RefNames(true).ToList();
			if (refs.Count != refs.Distinct().Count())
				throw new Exception("Duplicate mod/weak reference");

			//add (mod|weak)References that are not in sortBefore to sortAfter
			properties.sortAfter = properties.RefNames(true).Where(dep => !properties.sortBefore.Contains(dep))
				.Concat(properties.sortAfter).Distinct().ToArray();

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
						WriteList(dllReferences, writer);
					}
					if (modReferences.Length > 0)
					{
						writer.Write("modReferences");
						WriteList(modReferences, writer);
					}
					if (weakReferences.Length > 0)
					{
						writer.Write("weakReferences");
						WriteList(weakReferences, writer);
					}
					if (sortAfter.Length > 0)
					{
						writer.Write("sortAfter");
						WriteList(sortAfter, writer);
					}
					if (sortBefore.Length > 0)
					{
						writer.Write("sortBefore");
						WriteList(sortBefore, writer);
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
						properties.dllReferences = ReadList(reader).ToArray();
					}
					if (tag == "modReferences")
					{
						properties.modReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
					}
					if (tag == "weakReferences")
					{
						properties.weakReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
					}
					if (tag == "sortAfter")
					{
						properties.sortAfter = ReadList(reader).ToArray();
					}
					if (tag == "sortBefore")
					{
						properties.sortBefore = ReadList(reader).ToArray();
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
						properties.side = (ModSide)reader.ReadByte();
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

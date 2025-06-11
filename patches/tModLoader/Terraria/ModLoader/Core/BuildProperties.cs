using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Core;

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

			try {
				return new ModReference(split[0], new Version(split[1]));
			}
			catch {
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
	internal string eacPath = "";
	// This .tmod was built against a beta release, preventing publishing.
	internal bool beta = false;
	internal Version buildVersion = BuildInfo.tMLVersion;
	internal string homepage = "";
	internal string description = "";
	internal ModSide side;
	internal bool playableOnPreview = true;
	internal bool translationMod = false;
	internal string modSource = "";

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
		if (!File.Exists(propertiesFile)) {
			return properties;
		}
		if (File.Exists(descriptionfile)) {
			properties.description = File.ReadAllText(descriptionfile);
		}
		foreach (string line in File.ReadAllLines(propertiesFile)) {
			if (string.IsNullOrWhiteSpace(line)) {
				continue;
			}
			int split = line.IndexOf('=');
			if (split < 0)
				continue; // lines without an '=' are ignored
			string property = line.Substring(0, split).Trim();
			string value = line.Substring(split + 1).Trim();
			if (value.Length == 0) {
				continue;
			}
			switch (property) {
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
					properties.sortBefore = ReadList(value).ToArray();
					break;
				case "sortAfter":
					properties.sortAfter = ReadList(value).ToArray();
					break;
				case "author":
					properties.author = value;
					break;
				case "version":
					if (Version.TryParse(value, out Version result)) {
						properties.version = result;
					}
					else {
						Logging.tML.Error($"The version found in {propertiesFile}, \"{value}\", is not a valid version number. Read the \"version\" section of https://github.com/tModLoader/tModLoader/wiki/build.txt#available-properties for more info on correct version numbers.");
					}
					break;
				case "displayName":
					properties.displayName = value;
					break;
				case "homepage":
					properties.homepage = value;
					break;
				case "noCompile":
					properties.noCompile = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "playableOnPreview":
					properties.playableOnPreview = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "translationMod":
					properties.translationMod = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "hideCode":
					properties.hideCode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "hideResources":
					properties.hideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "includeSource":
					properties.includeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
					break;
				case "buildIgnore":
					properties.buildIgnores = value.Split(',').Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)).Where(s => s.Length > 0).ToArray();
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

		if (properties.dllReferences.Intersect(properties.modReferences.Select(x => x.mod)).Any())
			throw new Exception("dllReferences contains duplicate of modReferences");

		//add (mod|weak)References that are not in sortBefore to sortAfter
		properties.sortAfter = properties.RefNames(true).Where(dep => !properties.sortBefore.Contains(dep))
			.Concat(properties.sortAfter).Distinct().ToArray();

		// Interpolate description values
		ModCompile.UpdateSubstitutedDescriptionValues(ref properties.description, properties.version.ToString(), properties.homepage);

		return properties;
	}

	internal byte[] ToBytes()
	{
		byte[] data;
		using (MemoryStream memoryStream = new MemoryStream()) {
			using (BinaryWriter writer = new BinaryWriter(memoryStream)) {
				if (dllReferences.Length > 0) {
					writer.Write("dllReferences");
					WriteList(dllReferences, writer);
				}
				if (modReferences.Length > 0) {
					writer.Write("modReferences");
					WriteList(modReferences, writer);
				}
				if (weakReferences.Length > 0) {
					writer.Write("weakReferences");
					WriteList(weakReferences, writer);
				}
				if (sortAfter.Length > 0) {
					writer.Write("sortAfter");
					WriteList(sortAfter, writer);
				}
				if (sortBefore.Length > 0) {
					writer.Write("sortBefore");
					WriteList(sortBefore, writer);
				}
				if (author.Length > 0) {
					writer.Write("author");
					writer.Write(author);
				}
				writer.Write("version");
				writer.Write(version.ToString());
				if (displayName.Length > 0) {
					writer.Write("displayName");
					writer.Write(displayName);
				}
				if (homepage.Length > 0) {
					writer.Write("homepage");
					writer.Write(homepage);
				}
				if (description.Length > 0) {
					writer.Write("description");
					writer.Write(description);
				}
				if (noCompile) {
					writer.Write("noCompile");
				}
				if (!playableOnPreview) {
					writer.Write("!playableOnPreview");
				}
				if (translationMod) {
					writer.Write("translationMod");
				}
				if (!hideCode) {
					writer.Write("!hideCode");
				}
				if (!hideResources) {
					writer.Write("!hideResources");
				}
				if (includeSource) {
					writer.Write("includeSource");
				}
				if (eacPath.Length > 0) {
					writer.Write("eacPath");
					writer.Write(eacPath);
				}
				if (side != ModSide.Both) {
					writer.Write("side");
					writer.Write((byte)side);
				}
				if (modSource.Length > 0) {
					writer.Write("modSource");
					writer.Write(modSource);
				}

				writer.Write("buildVersion");
				writer.Write(buildVersion.ToString());

				writer.Write("");
			}
			data = memoryStream.ToArray();
		}
		return data;
	}

	internal static BuildProperties ReadModFile(TmodFile modFile)
	{
		return ReadFromStream(modFile.GetStream("Info"));
	}

	internal static BuildProperties ReadFromStream(Stream stream)
	{
		BuildProperties properties = new BuildProperties();
		// While the intended defaults for these are false, Info will only have !hideCode and !hideResources entries, so this is necessary.
		properties.hideCode = true;
		properties.hideResources = true;
		using (var reader = new BinaryReader(stream)) {
			for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString()) {
				if (tag == "dllReferences") {
					properties.dllReferences = ReadList(reader).ToArray();
				}
				if (tag == "modReferences") {
					properties.modReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
				}
				if (tag == "weakReferences") {
					properties.weakReferences = ReadList(reader).Select(ModReference.Parse).ToArray();
				}
				if (tag == "sortAfter") {
					properties.sortAfter = ReadList(reader).ToArray();
				}
				if (tag == "sortBefore") {
					properties.sortBefore = ReadList(reader).ToArray();
				}
				if (tag == "author") {
					properties.author = reader.ReadString();
				}
				if (tag == "version") {
					properties.version = new Version(reader.ReadString());
				}
				if (tag == "displayName") {
					properties.displayName = reader.ReadString();
				}
				if (tag == "homepage") {
					properties.homepage = reader.ReadString();
				}
				if (tag == "description") {
					properties.description = reader.ReadString();
				}
				if (tag == "noCompile") {
					properties.noCompile = true;
				}
				if (tag == "!playableOnPreview") {
					properties.playableOnPreview = false;
				}
				if (tag == "translationMod") {
					properties.translationMod = true;
				}
				if (tag == "!hideCode") {
					properties.hideCode = false;
				}
				if (tag == "!hideResources") {
					properties.hideResources = false;
				}
				if (tag == "includeSource") {
					properties.includeSource = true;
				}
				if (tag == "eacPath") {
					properties.eacPath = reader.ReadString();
				}
				if (tag == "side") {
					properties.side = (ModSide)reader.ReadByte();
				}
				if (tag == "buildVersion") {
					properties.buildVersion = new Version(reader.ReadString());
				}
				if (tag == "modSource") {
					properties.modSource = reader.ReadString();
				}
			}
		}
		return properties;
	}

	internal static void InfoToBuildTxt(Stream src, Stream dst)
	{
		BuildProperties properties = ReadFromStream(src);
		var sb = new StringBuilder();
		if (properties.displayName.Length > 0)
			sb.AppendLine($"displayName = {properties.displayName}");
		if (properties.author.Length > 0)
			sb.AppendLine($"author = {properties.author}");
		sb.AppendLine($"version = {properties.version}");
		if (properties.homepage.Length > 0)
			sb.AppendLine($"homepage = {properties.homepage}");
		if (properties.dllReferences.Length > 0)
			sb.AppendLine($"dllReferences = {string.Join(", ", properties.dllReferences)}");
		if (properties.modReferences.Length > 0)
			sb.AppendLine($"modReferences = {string.Join(", ", properties.modReferences)}");
		if (properties.weakReferences.Length > 0)
			sb.AppendLine($"weakReferences = {string.Join(", ", properties.weakReferences)}");
		if (properties.noCompile)
			sb.AppendLine($"noCompile = true");
		if (properties.hideCode)
			sb.AppendLine($"hideCode = true");
		if (properties.hideResources)
			sb.AppendLine($"hideResources = true");
		if (properties.includeSource)
			sb.AppendLine($"includeSource = true");
		if (!properties.playableOnPreview)
			sb.AppendLine($"playableOnPreview = false");
		if (properties.translationMod)
			sb.AppendLine($"translationMod = true");
		// buildIgnores isn't preserved in Info, but it doesn't matter with extraction since the ignored files won't be present anyway.
		// if (properties.buildIgnores.Length > 0)
		//	sb.AppendLine($"buildIgnores = {string.Join(", ", properties.buildIgnores)}");
		if (properties.side != ModSide.Both)
			sb.AppendLine($"side = {properties.side}");
		if (properties.sortAfter.Length > 0)
			sb.AppendLine($"sortAfter = {string.Join(", ", properties.sortAfter)}");
		if (properties.sortBefore.Length > 0)
			sb.AppendLine($"sortBefore = {string.Join(", ", properties.sortBefore)}");
		var bytes = Encoding.UTF8.GetBytes(sb.ToString());
		dst.Write(bytes, 0, bytes.Length);
	}

	internal bool ignoreFile(string resource) => buildIgnores.Any(fileMask => FitsMask(resource, fileMask));

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

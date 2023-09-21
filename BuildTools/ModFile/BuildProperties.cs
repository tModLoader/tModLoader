using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace tModPackager.ModFile;

public class BuildProperties
{
	public List<string> DllReferences = new();
	public List<ModReference> ModReferences = new();
	public List<ModReference> WeakReferences = new();
	public string[] SortAfter = Array.Empty<string>();
	public string[] SortBefore = Array.Empty<string>();
	public string[] BuildIgnores = Array.Empty<string>();
	public string Author = string.Empty;
	public Version Version = new(1, 0);
	public string DisplayName = "";
	public bool NoCompile = false;
	public bool HideResources = true;
	public bool IncludeSource = false;
	public bool PlayableOnPreview = true;
	public bool TranslationMod = false;
	public string EacPath = string.Empty;
	public string Homepage = "";
	public string Description = "";
	public ModSide Side = ModSide.Both;

	public IEnumerable<ModReference> Refs(bool includeWeak) => includeWeak ? ModReferences.Concat(WeakReferences) : ModReferences;

	public IEnumerable<string> RefNames(bool includeWeak) => Refs(includeWeak).Select(dep => dep.mod);

	public static IEnumerable<string> ReadList(string value) => value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

	public static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
	{
		foreach (var item in list)
			writer.Write(item!.ToString()!);

		writer.Write("");
	}

	public static BuildProperties ReadTaskItems(IEnumerable<ITaskItem> taskItems)
	{
		BuildProperties properties = new();

		foreach (ITaskItem property in taskItems) {
			string propertyName = char.ToLowerInvariant(property.ItemSpec[0]) + property.ItemSpec.Substring(1);
			string propertyValue = property.GetMetadata("Value");

			ProcessProperty(properties, propertyName, propertyValue);
		}

		VerifyRefs(properties.RefNames(true).ToList());
		properties.SortAfter = properties.GetDistinctRefs();

		return properties;
	}

	public static BuildProperties ReadBuildInfo(string buildFile)
	{
		BuildProperties properties = new BuildProperties();

		foreach (string line in File.ReadAllLines(buildFile)) {
			if (string.IsNullOrWhiteSpace(line))
				continue;

			int split = line.IndexOf('=');
			string property = line.Substring(0, split).Trim();
			string value = line.Substring(split + 1).Trim();
			if (value.Length == 0)
				continue;

			ProcessProperty(properties, property, value);
		}

		VerifyRefs(properties.RefNames(true).ToList());
		properties.SortAfter = properties.GetDistinctRefs();

		return properties;
	}

	private static void ProcessProperty(BuildProperties properties, string property, string value)
	{
		switch (property) {
			case "dllReferences":
				properties.DllReferences = ReadList(value).ToList();
				break;
			case "modReferences":
				properties.ModReferences = ReadList(value).Select(ModReference.Parse).ToList();
				break;
			case "weakReferences":
				properties.WeakReferences = ReadList(value).Select(ModReference.Parse).ToList();
				break;
			case "sortBefore":
				properties.SortBefore = ReadList(value).ToArray();
				break;
			case "sortAfter":
				properties.SortAfter = ReadList(value).ToArray();
				break;
			case "author":
				properties.Author = value;
				break;
			case "version":
				properties.Version = new Version(value);
				break;
			case "displayName":
				properties.DisplayName = value;
				break;
			case "homepage":
				properties.Homepage = value;
				break;
			case "noCompile":
				properties.NoCompile = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "hideResources":
				properties.HideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "includeSource":
				properties.IncludeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "playableOnPreview":
				properties.PlayableOnPreview = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "translationMod":
				properties.TranslationMod = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "buildIgnore":
				properties.BuildIgnores = value.Split(',')
					.Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar))
					.Where(s => s.Length > 0)
					.ToArray();
				break;
			case "side":
				if (!Enum.TryParse(value, true, out properties.Side))
					throw new ArgumentException("Side is not one of (Both, Client, Server, NoSync): " + value);
				break;
		}
	}

	internal void AddDllReference(string name)
	{
		DllReferences.Add(name);
	}

	internal void AddModReference(string modName, bool weak)
	{
		if (weak)
			WeakReferences.Add(ModReference.Parse(modName));
		else
			ModReferences.Add(ModReference.Parse(modName));
	}

	internal byte[] ToBytes(string buildVersion)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);

		if (DllReferences.Count > 0) {
			writer.Write("dllReferences");
			WriteList(DllReferences, writer);
		}

		if (ModReferences.Count > 0) {
			writer.Write("modReferences");
			WriteList(ModReferences, writer);
		}

		if (WeakReferences.Count > 0) {
			writer.Write("weakReferences");
			WriteList(WeakReferences, writer);
		}

		if (SortAfter.Length > 0) {
			writer.Write("sortAfter");
			WriteList(SortAfter, writer);
		}

		if (SortBefore.Length > 0) {
			writer.Write("sortBefore");
			WriteList(SortBefore, writer);
		}

		if (Author.Length > 0) {
			writer.Write("author");
			writer.Write(Author);
		}

		writer.Write("version");
		writer.Write(Version.ToString());
		if (DisplayName.Length > 0) {
			writer.Write("displayName");
			writer.Write(DisplayName);
		}

		if (Homepage.Length > 0) {
			writer.Write("homepage");
			writer.Write(Homepage);
		}

		if (Description.Length > 0) {
			writer.Write("description");
			writer.Write(Description);
		}

		if (NoCompile) {
			writer.Write("noCompile");
		}

		if (!HideResources) {
			writer.Write("!hideResources");
		}

		if (IncludeSource) {
			writer.Write("includeSource");
		}

		if (!PlayableOnPreview) {
			writer.Write("!playableOnPreview");
		}

		if (TranslationMod) {
			writer.Write("translationMod");
		}

		if (EacPath.Length > 0) {
			writer.Write("eacPath");
			writer.Write(EacPath);
		}

		if (Side != ModSide.Both) {
			writer.Write("side");
			writer.Write((byte)Side);
		}

		writer.Write("buildVersion");
		writer.Write(buildVersion);

		writer.Write("");
		return memoryStream.ToArray();
	}

	internal bool IgnoreFile(string resource) =>
		BuildIgnores.Any(fileMask => FitsMask(resource, fileMask)) || DllReferences.Contains("lib/" + Path.GetFileName(resource));

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

	private static void VerifyRefs(List<string> refs)
	{
		if (refs.Distinct().Count() != refs.Count) throw new DuplicateNameException("Duplicate mod or weak references.");
	}

	// Adds (mod|weak)References that are not in sortBefore to sortAfter
	public string[] GetDistinctRefs() => RefNames(true)
		.Where(dep => !SortBefore.Contains(dep))
		.Concat(SortAfter).Distinct().ToArray();
}
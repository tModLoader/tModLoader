using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace tModLoader.BuildTools.ModFile;

public class BuildProperties
{
	private List<string> _dllReferences = new();
	private List<ModReference> _modReferences = new();
	private List<ModReference> _weakReferences = new();
	private string[] _sortAfter = Array.Empty<string>();
	private string[] _sortBefore = Array.Empty<string>();
	private string[] _buildIgnores = Array.Empty<string>();
	private string _author = "";
	public Version Version = new(1, 0);
	private string _displayName = "";
	private bool _noCompile = false;
	private bool _hideCode = false;
	private bool _hideResources = false;
	private bool _includeSource = false;

	private string _eacPath = "";

	// This .tmod was built against a beta release, preventing publishing.
	internal bool Beta = false;
	private string _homepage = "";
	private string _description = "";
	private ModSide _side;

	public IEnumerable<ModReference> Refs(bool includeWeak) =>
		includeWeak ? _modReferences.Concat(_weakReferences) : _modReferences;

	public IEnumerable<string> RefNames(bool includeWeak) => Refs(includeWeak).Select(dep => dep.mod);

	private static IEnumerable<string> ReadList(string value)
		=> value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);

	private static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer) {
		foreach (var item in list)
			writer.Write(item!.ToString());

		writer.Write("");
	}

	internal static BuildProperties ReadTaskItems(IEnumerable<ITaskItem> taskItems) {
		BuildProperties properties = new BuildProperties();

		foreach (ITaskItem property in taskItems) {
			string propertyName = property.ItemSpec;
			string propertyValue = property.GetMetadata("Value");

			// Make the first letter lowercase
			propertyName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

			ProcessProperty(properties, propertyName, propertyValue);
		}

		var refs = properties.RefNames(true).ToList();
		if (refs.Count != refs.Distinct().Count())
			throw new Exception("Duplicate mod/weak reference");

		//add (mod|weak)References that are not in sortBefore to sortAfter
		properties._sortAfter = properties.RefNames(true).Where(dep => !properties._sortBefore.Contains(dep))
			.Concat(properties._sortAfter).Distinct().ToArray();

		return properties;
	}

	internal static BuildProperties ReadBuildInfo(string buildFile) {
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

		var refs = properties.RefNames(true).ToList();
		if (refs.Count != refs.Distinct().Count())
			throw new Exception("Duplicate mod/weak reference");

		//add (mod|weak)References that are not in sortBefore to sortAfter
		properties._sortAfter = properties.RefNames(true).Where(dep => !properties._sortBefore.Contains(dep))
			.Concat(properties._sortAfter).Distinct().ToArray();

		return properties;
	}

	private static void ProcessProperty(BuildProperties properties, string property, string value) {
		switch (property) {
			case "dllReferences":
				properties._dllReferences = ReadList(value).ToList();
				break;
			case "modReferences":
				properties._modReferences = ReadList(value).Select(ModReference.Parse).ToList();
				break;
			case "weakReferences":
				properties._weakReferences = ReadList(value).Select(ModReference.Parse).ToList();
				break;
			case "sortBefore":
				properties._sortBefore = ReadList(value).ToArray();
				break;
			case "sortAfter":
				properties._sortAfter = ReadList(value).ToArray();
				break;
			case "author":
				properties._author = value;
				break;
			case "version":
				properties.Version = new Version(value);
				break;
			case "displayName":
				properties._displayName = value;
				break;
			case "homepage":
				properties._homepage = value;
				break;
			case "noCompile":
				properties._noCompile = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "hideCode":
				properties._hideCode = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "hideResources":
				properties._hideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "includeSource":
				properties._includeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
				break;
			case "buildIgnore":
				properties._buildIgnores = value.Split(',').Select(s => s.Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar))
					.Where(s => s.Length > 0).ToArray();
				break;
			case "side":
				if (!Enum.TryParse(value, true, out properties._side))
					throw new Exception("Side is not one of (Both, Client, Server, NoSync): " + value);
				break;
		}
	}

	internal void SetDescription(string description) {
		_description = description;
	}

	internal void AddDllReference(string name) {
		_dllReferences.Add(name);
	}

	internal void AddModReference(string modName, bool weak) {
		if (weak)
			_weakReferences.Add(ModReference.Parse(modName));
		else
			_modReferences.Add(ModReference.Parse(modName));
	}

	internal byte[] ToBytes(Version buildVersion) {
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);

		if (_dllReferences.Count > 0) {
			writer.Write("dllReferences");
			WriteList(_dllReferences, writer);
		}
		if (_modReferences.Count > 0) {
			writer.Write("modReferences");
			WriteList(_modReferences, writer);
		}
		if (_weakReferences.Count > 0) {
			writer.Write("weakReferences");
			WriteList(_weakReferences, writer);
		}
		if (_sortAfter.Length > 0) {
			writer.Write("sortAfter");
			WriteList(_sortAfter, writer);
		}
		if (_sortBefore.Length > 0) {
			writer.Write("sortBefore");
			WriteList(_sortBefore, writer);
		}
		if (_author.Length > 0) {
			writer.Write("author");
			writer.Write(_author);
		}
		writer.Write("version");
		writer.Write(Version.ToString());
		if (_displayName.Length > 0) {
			writer.Write("displayName");
			writer.Write(_displayName);
		}
		if (_homepage.Length > 0) {
			writer.Write("homepage");
			writer.Write(_homepage);
		}
		if (_description.Length > 0) {
			writer.Write("description");
			writer.Write(_description);
		}
		if (_noCompile) {
			writer.Write("noCompile");
		}
		if (!_hideCode) {
			writer.Write("!hideCode");
		}
		if (!_hideResources) {
			writer.Write("!hideResources");
		}
		if (_includeSource) {
			writer.Write("includeSource");
		}
		if (_eacPath.Length > 0) {
			writer.Write("eacPath");
			writer.Write(_eacPath);
		}
		if (_side != ModSide.Both) {
			writer.Write("side");
			writer.Write((byte)_side);
		}

		writer.Write("buildVersion");
		writer.Write(buildVersion.ToString());

		writer.Write("");
		byte[] data = memoryStream.ToArray();
		return data;
	}

	internal bool IgnoreFile(string resource) => _buildIgnores.Any(fileMask => FitsMask(resource, fileMask));

	private bool FitsMask(string fileName, string fileMask) {
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

	public override string ToString() => $"{nameof(_dllReferences)}: {_dllReferences.Count}, {nameof(_modReferences)}: {_modReferences.Count}, {nameof(_weakReferences)}: {_weakReferences.Count}, {nameof(_sortAfter)}: {_sortAfter.Length}, {nameof(_sortBefore)}: {_sortBefore.Length}, {nameof(_buildIgnores)}: {_buildIgnores.Length}, {nameof(_author)}: {_author}, {nameof(Version)}: {Version}, {nameof(_displayName)}: {_displayName}, {nameof(_noCompile)}: {_noCompile}, {nameof(_hideCode)}: {_hideCode}, {nameof(_hideResources)}: {_hideResources}, {nameof(_includeSource)}: {_includeSource}, {nameof(_eacPath)}: {_eacPath}, {nameof(Beta)}: {Beta}, {nameof(_homepage)}: {_homepage}, {nameof(_description)}: {_description}, {nameof(_side)}: {_side}";
}
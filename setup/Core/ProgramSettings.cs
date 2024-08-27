using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using DiffPatch;

namespace Terraria.ModLoader.Setup.Core;

public sealed class ProgramSettings
{
	private static readonly JsonSerializerOptions FormatJsonSerializerOptions =
		new() { WriteIndented = true, TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

	private readonly string jsonFilePath;

	internal ProgramSettings(string jsonFilePath)
	{
		this.jsonFilePath = jsonFilePath;
	}

	public string LogsDir { get; set; } = Path.Combine("setup", "logs");

	public string? TerrariaSteamDir { get; set; }

	public string? TMLDevSteamDir { get; set; }

	public DateTime? TerrariaDiffCutoff { get; set; }

	public DateTime? TerrariaNetCoreDiffCutoff { get; set; }

	public DateTime? TModLoaderDiffCutoff { get; set; }

	public Patcher.Mode PatchMode { get; set; }

	public bool FormatAfterDecompiling { get; set; } = true;

	[JsonIgnore]
	public bool NoPrompts { get; set; }

	[JsonIgnore]
	public string? TerrariaPath => string.IsNullOrEmpty(TerrariaSteamDir)
		? null
		: Path.Combine(TerrariaSteamDir, "Terraria.exe");

	[JsonIgnore]
	public string? TerrariaServerPath => string.IsNullOrEmpty(TerrariaSteamDir)
		? null
		: Path.Combine(TerrariaSteamDir, "TerrariaServer.exe");

	public void Save()
	{
		string json = JsonSerializer.Serialize(this, FormatJsonSerializerOptions);
		File.WriteAllText(jsonFilePath, json);
	}

	public static ProgramSettings InitializeSettingsFile(string jsonFilePath)
	{
		ProgramSettings programSettings = new ProgramSettings(jsonFilePath);
		programSettings.Save();

		return programSettings;
	}
}
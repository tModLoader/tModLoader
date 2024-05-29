#if NETCORE
using Basic.Reference.Assemblies;
using log4net.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Core;

// TODO further documentation
// TODO too many inner classes
internal class ModCompile
{
	public interface IBuildStatus
	{
		void SetProgress(int i, int n = -1);
		void SetStatus(string msg);
		void LogCompilerLine(string msg, Level level);
	}

	private class ConsoleBuildStatus : IBuildStatus
	{
		public void SetProgress(int i, int n) { }

		public void SetStatus(string msg) => Console.WriteLine(msg);

		public void LogCompilerLine(string msg, Level level) =>
			((level == Level.Error) ? Console.Error : Console.Out).WriteLine(msg);
	}

	private class BuildingMod : LocalMod
	{
		public string path;

		public BuildingMod(TmodFile modFile, BuildProperties properties, string path) : base(ModLocation.Local, modFile, properties)
		{
			this.path = path;
		}
	}

	public static readonly string ModSourcePath = Path.Combine(Program.SavePathShared, "ModSources");

	internal static string[] FindModSources()
	{
		Directory.CreateDirectory(ModSourcePath);
		return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => {
			var directory = new DirectoryInfo(dir);
			return directory.Name[0] != '.' && directory.Name != "ModAssemblies" && directory.Name != "Mod Libraries";
		}).ToArray();
	}

	// Silence exception reporting in the chat unless actively modding.
	public static bool activelyModding;

	public static bool DeveloperMode => Debugger.IsAttached || Directory.Exists(ModSourcePath) && FindModSources().Length > 0;

	private static readonly Regex ErrorRegex = new(@"^\s*(\d+) Error\(s\)", RegexOptions.Multiline | RegexOptions.Compiled);
	private static readonly Regex WarningRegex = new(@"^\s*(\d+) Warning\(s\)", RegexOptions.Multiline | RegexOptions.Compiled);
	private static readonly Regex ErrorMessageRegex = new(@"error \w*:", RegexOptions.Compiled);
	private static readonly string tMLDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	private static readonly string oldModReferencesPath = Path.Combine(Program.SavePath, "references");
	private static readonly string modTargetsPath = Path.Combine(ModSourcePath, "tModLoader.targets");
	private static readonly string tMLModTargetsPath = Path.Combine(tMLDir, "tMLMod.targets");
	private static bool referencesUpdated = false;
	internal static void UpdateReferencesFolder()
	{
		if (referencesUpdated)
			return;

		try {
			if (Directory.Exists(oldModReferencesPath))
				Directory.Delete(oldModReferencesPath, true);
		} catch (Exception e) {
			Logging.tML.Error("Failed to delete old /references dir", e);
		}

		UpdateFileContents(modTargetsPath,
$@"<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""{SecurityElement.Escape(tMLModTargetsPath)}"" />
</Project>");

		referencesUpdated = true;
	}

	private static void UpdateFileContents(string path, string contents) {
		Directory.CreateDirectory(Path.GetDirectoryName(path));

		byte[] bytes = Encoding.UTF8.GetBytes(contents);
		if (!File.Exists(path) || !Enumerable.SequenceEqual(bytes, File.ReadAllBytes(path)))
			File.WriteAllBytes(path, bytes);
	}

	internal static IList<string> sourceExtensions = new List<string> { ".csproj", ".cs", ".sln" };

	private IBuildStatus status;
	public ModCompile(IBuildStatus status)
	{
		this.status = status;

		// *gasp*, side-effects
		activelyModding = true;
		Logging.ResetPastExceptions();
	}

	internal static void BuildModCommandLine(string modFolder)
	{
		UpdateReferencesFolder();

		// TODO: Build works even without build.txt or even a correct folder...
		LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);
		Lang.InitializeLegacyLocalization();

		// Once we get to this point, the application is guaranteed to exit
		try {
			new ModCompile(new ConsoleBuildStatus()).Build(modFolder);
		}
		catch (BuildException e) {
			Console.Error.WriteLine("Error: " + e.Message);
			if (e.InnerException != null)
				Console.Error.WriteLine(e.InnerException);
			Environment.Exit(1);
		}
		catch (Exception e) {
			Console.Error.WriteLine(e);
			Environment.Exit(1);
		}

		Social.Steam.WorkshopSocialModule.SteamCMDPublishPreparer(modFolder);

		// Mod was built with success, exit code 0 indicates success.
		Environment.Exit(0);
	}

	internal void Build(string modFolder) => Build(ReadBuildInfo(modFolder));

	private BuildingMod ReadBuildInfo(string modFolder)
	{
		if (modFolder.EndsWith("\\") || modFolder.EndsWith("/")) modFolder = modFolder.Substring(0, modFolder.Length - 1);
		string modName = Path.GetFileName(modFolder);
		status.SetStatus(Language.GetTextValue("tModLoader.ReadingProperties", modName));

		string file = Path.Combine(ModLoader.ModPath, modName + ".tmod");
		var modFile = new TmodFile(file, modName);
		return new BuildingMod(modFile, null, modFolder);
	}

	private void Build(BuildingMod mod)
	{
		try {
			status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));

			string csprojFile = Path.Combine(mod.path, mod.Name);
			csprojFile = Path.ChangeExtension(csprojFile, ".csproj");
			if (!File.Exists(csprojFile)) {
				throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorMissingCsproj"));
			}

			if (ModLoader.TryGetMod(mod.Name, out var loadedMod)) {
				loadedMod.Close();
			}

			string outputPath = mod.modFile.path;
			Process process = new() {
				StartInfo = new ProcessStartInfo {
					FileName = UIModSources.GetSystemDotnetPath() ?? "dotnet",
					Arguments = $"build --no-incremental -c Release -v q -p:OutputTmodPath=\"{outputPath}\"",
					WorkingDirectory = mod.path,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
				},
			};

			// Force locale to be in English.
			// Needed because of how we are getting the error and warning count.
			process.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en-US";

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			string stderr = process.StandardError.ReadToEnd();
			process.WaitForExit(1000 * 60); // Wait up to a minute for the process to end


			if (!process.HasExited || process.ExitCode != 0) {
				Logging.tML.Debug("Complete build output:\n" + output);
				Logging.tML.Debug("Stderr:\n" + stderr);

				Match errorMatch = ErrorRegex.Match(output);
				Match warningMatch = WarningRegex.Match(output);
				string numErrors = errorMatch.Success ? errorMatch.Groups[1].Value : "?";
				string numWarnings = warningMatch.Success ? warningMatch.Groups[1].Value : "?";

				string firstError = output.Split('\n').FirstOrDefault(line => ErrorMessageRegex.IsMatch(line), "N/A");

				throw new BuildException(Language.GetTextValue("tModLoader.CompileError", mod.Name + ".dll", numErrors, numWarnings) + $"\nError: {firstError}");
			}

			// Enable mods here to update mod menu UI
			ModLoader.EnableMod(mod.Name);
			// TODO: This should probably enable dependencies recursively as well. They will load properly, but right now the UI does not show them as loaded.
			LocalizationLoader.HandleModBuilt(mod.Name);
		}
		catch (Exception e) {
			e.Data["mod"] = mod.Name;
			throw;
		}
	}

	internal static void UpdateSubstitutedDescriptionValues(ref string description, string modVersion, string homepage)
	{
		// Language.GetText returns the given key if it can't be found, this way we can use LocalizedText.FormatWith
		// This allows us to use substitution keys such as {ModVersion}
		description = Language.GetText(description).FormatWith(new {
			ModVersion = modVersion,
			ModHomepage = homepage,
			tMLVersion = BuildInfo.tMLVersion.MajorMinor().ToString(),
			tMLBuildPurpose = BuildInfo.Purpose.ToString(),
		});
	}
}
#endif

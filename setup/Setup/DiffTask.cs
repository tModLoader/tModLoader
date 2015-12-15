using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;

namespace Terraria.ModLoader.Setup
{
	public class DiffTask : Task
	{
		public static string[] extensions = { ".cs", ".csproj", ".ico", ".resx", ".png" };
		public static string[] excluded = { "bin" + Path.DirectorySeparatorChar, "obj" + Path.DirectorySeparatorChar };

		public readonly string baseDir;
		public readonly string srcDir;
		public readonly string patchDir;
		public readonly CSharpFormattingOptions format;

		public string FullBaseDir { get { return Path.Combine(Program.baseDir, baseDir); } }
		public string FullSrcDir { get { return Path.Combine(Program.baseDir, srcDir); } }
		public string FullPatchDir { get { return Path.Combine(Program.baseDir, patchDir); } }

		public static ProgramSetting<DateTime> MergedDiffCutoff = new ProgramSetting<DateTime>("MergedDiffCutoff");
		public static ProgramSetting<DateTime> TerrariaDiffCutoff = new ProgramSetting<DateTime>("TerrariaDiffCutoff");
		public static ProgramSetting<DateTime> tModLoaderDiffCutoff = new ProgramSetting<DateTime>("tModLoaderDiffCutoff");
		public int stepNumber;

		public DiffTask(ITaskInterface taskInterface, string baseDir, string srcDir, string patchDir, int stepNumber,
			CSharpFormattingOptions format = null)
			: base(taskInterface)
		{
			this.baseDir = baseDir;
			this.srcDir = srcDir;
			this.patchDir = patchDir;
			this.format = format;
			this.stepNumber = stepNumber;
		}

		public override void Run()
		{
			List<string> patchesFiles = new List<string>();// (Directory.EnumerateFiles(FullPatchDir, "*", SearchOption.AllDirectories));
			foreach (var file in Directory.EnumerateFiles(FullPatchDir, "*", SearchOption.AllDirectories))
			{
				patchesFiles.Add(RelPath(FullPatchDir, file));
			}
			//if (Directory.Exists(FullPatchDir))
			//	Directory.Delete(FullPatchDir, true);

			var files = Directory.EnumerateFiles(FullSrcDir, "*", SearchOption.AllDirectories).Where(f => extensions.Any(f.EndsWith));
			var items = new List<WorkItem>();

			foreach (var file in files)
			{
				var relPath = RelPath(FullSrcDir, file);
				patchesFiles.Remove(relPath);
				patchesFiles.Remove(relPath + ".patch");
				if (excluded.Any(relPath.StartsWith))
					continue;

				//bool skip = false;
				if ((stepNumber == 0 && (File.GetLastWriteTime(file) < MergedDiffCutoff.Get())) ||
					(stepNumber == 1 && (File.GetLastWriteTime(file) < TerrariaDiffCutoff.Get())) ||
					(stepNumber == 2 && (File.GetLastWriteTime(file) < tModLoaderDiffCutoff.Get())))
					continue;
				//skip = true;

				//if (!skip)
				items.Add(File.Exists(Path.Combine(FullBaseDir, relPath))
					? new WorkItem("Creating Diff: " + relPath, () => Diff(relPath))
					: new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(FullPatchDir, relPath))));
			}

			ExecuteParallel(items);

			taskInterface.SetStatus("Deleting Unnessesary Patches");

			foreach (string file in patchesFiles)
			{
				File.Delete(file);
			}

			switch (stepNumber)
			{
				case 0:
					MergedDiffCutoff.Set(DateTime.Now);
					break;
				case 1:
					TerrariaDiffCutoff.Set(DateTime.Now);
					break;
				case 2:
					tModLoaderDiffCutoff.Set(DateTime.Now);
					break;
			}
		}

		private void Diff(string relPath)
		{
			var srcFile = Path.Combine(FullSrcDir, relPath);
			var baseFile = Path.Combine(FullBaseDir, relPath);

			string temp = null;
			if (srcFile.EndsWith(".cs") && format != null)
			{
				temp = Path.GetTempPath() + Guid.NewGuid() + ".cs";
				File.WriteAllText(temp, FormatTask.FormatCode(File.ReadAllText(baseFile), format, taskInterface.CancellationToken()));
				baseFile = temp;
			}

			var patch = CallDiff(baseFile, srcFile, Path.Combine(baseDir, relPath), Path.Combine(srcDir, relPath));

			if (temp != null)
				File.Delete(temp);

			if (patch == "\r\n")
				return;

			var patchFile = Path.Combine(FullPatchDir, relPath + ".patch");
			CreateParentDirectory(patchFile);
			File.WriteAllText(patchFile, patch);
		}

		private string CallDiff(string baseFile, string srcFile, string baseName, string srcName)
		{
			var output = new StringBuilder();
			Program.RunCmd(Program.toolsDir, Path.Combine(Program.toolsDir, "py.exe"),
				string.Format("diff.py {0} {1} {2} {3}", baseFile, srcFile, baseName, srcName),
				s => output.Append(s), _ => { });

			return output.ToString();
		}

	}
}
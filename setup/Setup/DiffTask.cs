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
        public static string[] extensions = {".cs", ".csproj", ".ico", ".resx", ".png"};
        public static string[] excluded = {"bin" + Path.DirectorySeparatorChar, "obj" + Path.DirectorySeparatorChar};

        public readonly string baseDir;
        public readonly string srcDir;
        public readonly string patchDir;
        public readonly CSharpFormattingOptions format;

        public string FullBaseDir { get { return Path.Combine(Program.baseDir, baseDir); } }
        public string FullSrcDir { get { return Path.Combine(Program.baseDir, srcDir); } }
        public string FullPatchDir { get { return Path.Combine(Program.baseDir, patchDir); } }

        public DiffTask(ITaskInterface taskInterface, string baseDir, string srcDir, string patchDir,
            CSharpFormattingOptions format = null)
            : base(taskInterface) {
            this.baseDir = baseDir;
            this.srcDir = srcDir;
            this.patchDir = patchDir;
            this.format = format;
        }

        public override void Run() {
            taskInterface.SetStatus("Deleting Old Patches");

            if (Directory.Exists(FullPatchDir))
                Directory.Delete(FullPatchDir, true);

            var files = Directory.EnumerateFiles(FullSrcDir, "*", SearchOption.AllDirectories).Where(f => extensions.Any(f.EndsWith));
            var items = new List<WorkItem>();

            foreach (var file in files) {
                var relPath = RelPath(FullSrcDir, file);
                if (excluded.Any(relPath.StartsWith))
                    continue;

                items.Add(File.Exists(Path.Combine(FullBaseDir, relPath))
                    ? new WorkItem("Creating Diff: " + relPath, () => Diff(relPath))
                    : new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(FullPatchDir, relPath))));
            }

            ExecuteParallel(items);
        }

        private void Diff(string relPath) {
            var srcFile = Path.Combine(FullSrcDir, relPath);
            var baseFile = Path.Combine(FullBaseDir, relPath);

            string temp = null;
            if (srcFile.EndsWith(".cs") && format != null) {
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

        private string CallDiff(string baseFile, string srcFile, string baseName, string srcName) {
            var output = new StringBuilder();
			Program.RunCmd(Program.toolsDir, Path.Combine(Program.toolsDir, "py.exe"),
                string.Format("diff.py {0} {1} {2} {3}", baseFile, srcFile, baseName, srcName), 
                s => output.Append(s), _ => {});
            
            return output.ToString();
        }
    }
}
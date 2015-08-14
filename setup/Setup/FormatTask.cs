using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;

namespace Terraria.ModLoader.Setup
{
    class FormatTask : Task
    {
        public static CSharpFormattingOptions tModLoaderFormat = FormattingOptionsFactory.CreateAllman();

        public static string FormatCode(string text, CSharpFormattingOptions options, CancellationToken ct) {
            var formatter = new CSharpFormatter(options) { FormattingMode = FormattingMode.Intrusive };
            text = text.Replace("\r\n\r\n", "\r\n");

            var doc = new StringBuilderDocument(text);
            var syntaxTree = SyntaxTree.Parse(doc, doc.FileName, null, ct);
            formatter.AnalyzeFormatting(doc, syntaxTree, ct).ApplyChanges();

            return doc.Text;
        }

        public static void Format(string path, CSharpFormattingOptions options, CancellationToken ct) {
            File.WriteAllText(path, FormatCode(File.ReadAllText(path), options, ct));
        }

        private static string directory = "";

        public readonly CSharpFormattingOptions format;
        private List<string> files = new List<string>(); 

        public FormatTask(ITaskInterface taskInterface, CSharpFormattingOptions format) : base(taskInterface) {
            this.format = format;
        }

        public override bool ConfigurationDialog() {
            var form = new SelectFilesForm(directory, "C# Source Files (*.cs)|*.cs");
            var res = (DialogResult)taskInterface.Invoke(new Func<DialogResult>(() => form.ShowDialog(taskInterface)));

            if (res != DialogResult.OK)
                return false;

            directory = form.GetDirectory();
            foreach (var path in form.GetPaths()) {
                if (Directory.Exists(path))
                    foreach (var file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
                        files.Add(file);
                else if (File.Exists(path) && path.EndsWith(".cs"))
                    files.Add(path);
            }

            return true;
        }

        public override void Run() {
            var items = files.Select(file => new WorkItem(
                "Formatting: " + Path.GetFileName(file),
                () => Format(file, format, taskInterface.CancellationToken()))).ToList();

            ExecuteParallel(items);
        }
    }
}

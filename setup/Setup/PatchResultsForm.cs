using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public partial class PatchResultsForm : Form
	{
		private List<PatchTask.PatchResults> list;

		private PatchTask.PatchResults file;
		private Patcher.Result result;

		public PatchResultsForm(IEnumerable<PatchTask.PatchResults> results) {
			list = results.OrderBy(r => r.relPath).ToList();
			InitializeComponent();

			foreach (var file in list) {
				var node = new TreeNode(file.relPath) {
					BackColor = ErrColor(file.ErrorLevel()),
					Tag = file
				};
				treeView.Nodes.Add(node);

				node.Nodes.AddRange(file.results.Select(r =>
					new TreeNode(file.Summary(r)) {
						BackColor = ErrColor(file.ErrorLevel(r), r),
						Tag = r
					}
				).ToArray());
			}
		}

		private Color ErrColor(int errorLevel, Patcher.Result result = null) {
			switch (errorLevel) {
				case 0:
					if (result != null && result.mode == Patcher.Mode.EXACT)
						return Color.ForestGreen;
					if (result != null && result.mode == Patcher.Mode.OFFSET)
						return Color.GreenYellow;
					return default(Color);
				case 1://fuzzy
					return Color.LightSkyBlue;
				case 2:
					return Color.Orange;
				case 3:
					return Color.OrangeRed;
				case 4:
					return Color.Red;
			}
			throw new ArgumentException("Error Level: "+errorLevel);
		}

		private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			new object();
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e) {
			if (e.Node.Tag is PatchTask.PatchResults)
				SetFile((PatchTask.PatchResults)e.Node.Tag);
			else if (e.Node.Tag is Patcher.Result)
				SetResult((PatchTask.PatchResults)e.Node.Parent.Tag, (Patcher.Result)e.Node.Tag);
		}

		private void SetFile(PatchTask.PatchResults file) {
			this.file = file;
			textBoxTitle.Text = file.relPath;
			textBoxTitle.BackColor = ErrColor(file.ErrorLevel());
			buttonOpenPatch.Enabled = false;
			tabControl.Hide();
		}

		private void SetResult(PatchTask.PatchResults file, Patcher.Result result) {
			this.file = file;
			this.result = result;
			textBoxTitle.Text = file.relPath + " " + file.Summary(result);
			textBoxTitle.BackColor = ErrColor(file.ErrorLevel(result), result);
			buttonOpenPatch.Enabled = true;
			tabControl.Show();
			UpdatePanes();
		}

		private void UpdatePanes() {
			if (result.success && result.mode == Patcher.Mode.FUZZY) {
				string origHtml, fuzzyHtml;
				FormatAppliedPatch(result.patch.ToString().Split('\n').Select(l => l.TrimEnd('\r')).ToArray(),
					result.appliedPatch.ToString().Split('\n').Select(l => l.TrimEnd('\r')).ToArray(),
					result.fuzzyDiff, 0,
					out origHtml, out fuzzyHtml);

				patchDisplayPanel.SetContent(
					"Original", Color.FromArgb(0xFF, 0x80, 0x80), origHtml,
					"Fuzzy", Color.FromArgb(0x80, 0xFF, 0x80), fuzzyHtml);
			}
			else {
				patchDisplayPanel.SetSingleContent("Patch", Color.LightGray, FormatPatch(result.patch));
			}

			if (!result.success) {
				appliedDisplayPanel.SetContent("Failed", Color.Red, "", "Failed", Color.Red, "");
				return;
			}

			string beforeHtml, afterHtml;
			FormatAppliedPatch(file.beforeLines, file.afterLines, result.appliedPatch, 40, out beforeHtml, out afterHtml);

			appliedDisplayPanel.SetContent(
				"Before", Color.FromArgb(0xFF, 0x80, 0x80), beforeHtml, 
				"After", Color.FromArgb(0x80, 0xFF, 0x80), afterHtml);

			appliedDisplayPanel.ScrollToOnLoad("bounds");
		}

		private static string Escape(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

		private static string FormatLine(string s, string divClass = null, string preClass = null) {
			divClass = divClass == null ? "" : $" class=\"{divClass}\"";
			preClass = preClass == null ? "" : $" class=\"{preClass}\"";
			return string.IsNullOrEmpty(s) ? 
				$"<div{divClass}><br/></div>" : 
				$"<div{divClass}><pre{preClass}>{Escape(s)}</pre></div>";
		}

		private static string FormatPatch(Patcher.Patch patch) {
			var diffOpClasses = new Dictionary<Patcher.Operation, string> {
				{ Patcher.Operation.EQUAL, "none" },
				{ Patcher.Operation.INSERT, "insert" },
				{ Patcher.Operation.DELETE, "delete" }
			};
			var lines = new List<string>();
			lines.Add(FormatLine(patch.Header, null, "header"));
			lines.AddRange(patch.diffs.Select(diff =>
				FormatLine(diff.ToString(), null, diffOpClasses[diff.op])));

			return PaneHTML(Enumerable.Range(1, lines.Count).Select(i => i.ToString()), lines);
		}

		private class NumberedLines
		{
			public List<string> lineNumbers = new List<string>();
			public List<string> lines = new List<string>();

			public void Add(int? lineNo, string line) {
				lineNumbers.Add((lineNo+1)?.ToString() ?? "");
				lines.Add(line);
			}

			public void Insert(int i, int? lineNo, string line) {
				lineNumbers.Insert(i, (lineNo + 1)?.ToString() ?? "");
				lines.Insert(i, line);
			}
		}

		private static void FormatAppliedPatch(string[] lineArr1, string[] lineArr2, Patcher.Patch patch, 
			int extraContext, out string html1, out string html2) {
			//insert lines
			//make the assumption that removals come before additions
			int loc1 = patch.start1, loc2 = patch.start2;
			var lines1 = new NumberedLines();
			var lines2 = new NumberedLines();
			
			//last match point
			int mp1 = loc1, mp2 = loc2;
			foreach (var diff in patch.diffs) {
				switch (diff.op) {
					case Patcher.Operation.EQUAL:
						//match sublines
						int num1 = loc1 - mp1;
						int num2 = loc2 - mp2;

						for (; num1 < num2; num1++)
							lines1.Add(null, FormatLine("", "missing"));
						for (; num2 < num1; num2++)
							lines2.Add(null, FormatLine("", "missing"));

						mp1 = loc1;
						mp2 = loc2;
						lines1.Add(loc1, FormatLine(lineArr1[loc1++]));
						lines2.Add(loc2, FormatLine(lineArr2[loc2++]));
						break;
					case Patcher.Operation.DELETE:
						lines1.Add(loc1, FormatLine(lineArr1[loc1++], "delete"));
						break;
					case Patcher.Operation.INSERT:
						lines2.Add(loc2, FormatLine(lineArr2[loc2++], "insert"));
						break;
				}
			}

			//add context
			if (extraContext > 0) {
				lines1.lines[0] = "<div id=\"bounds\">" + lines1.lines[0];
				lines2.lines[0] = "<div id=\"bounds\">" + lines2.lines[0];
				lines1.lines[lines1.lines.Count - 1] = lines1.lines[lines1.lines.Count - 1] + "</div>";
				lines2.lines[lines2.lines.Count - 1] = lines2.lines[lines2.lines.Count - 1] + "</div>";
			}

			int pre1 = patch.start1-1, pre2 = patch.start2-1;
			for (int i = 0; i < extraContext; i++) {
				if (loc1 < lineArr1.Length || loc2 < lineArr2.Length) {
					if (loc1 < lineArr1.Length)
						lines1.Add(loc1, FormatLine(lineArr1[loc1++]));
					else
						lines1.Add(null, FormatLine("", "missing"));

					if (loc2 < lineArr2.Length)
						lines2.Add(loc2, FormatLine(lineArr2[loc2++]));
					else
						lines2.Add(null, FormatLine("", "missing"));
				}

				if (pre1 >= 0 || pre2 >= 0) {
					if (pre1 >= 0)
						lines1.Insert(0, pre1, FormatLine(lineArr1[pre1--]));
					else
						lines1.Insert(0, null, FormatLine("", "missing"));

					if (pre2 >= 0)
						lines2.Insert(0, pre2, FormatLine(lineArr2[pre2--]));
					else
						lines1.Insert(0, null, FormatLine("", "missing"));
				}
			}
			
			html1 = PaneHTML(lines1.lineNumbers, lines1.lines);
			html2 = PaneHTML(lines2.lineNumbers, lines2.lines);
		}

		private static string PaneHTML(IEnumerable<string> lineNumbers, IEnumerable<string> lines) {
			var head = @"<head>
<style type=""text/css"">
<!--
body { 
	font-family: ""Courier New"";
	font-size: 10pt;
    margin: 0 !important;
    padding: 0 !important;
}
table {
	border-collapse: collapse;
	width: 100%;
}
pre {
	margin: 0;
	display: inline;
}
td {
	font-size: 10pt;
	padding: 0;
	margin: 0;
}
.lineno {
	width: 50px;
	padding: 0 4 0 4;
	text-align: right;
	background: #F0F0F0;
}
.header { color: #FF0000; }
.delete { background: #FFA0A0; }
.insert { background: #A0FFA0; }
.missing { background: #C0C0C0; }
#bounds {
    border: solid 2px;
	border-left: none;
	margin: -2px 0 -2px 0;
}
-- >
</style>
</head>";

			return @"<html>
" + head + @"
<body>
<table>
<td class=""lineno""><pre style=""display: inline;"">" + string.Join(Environment.NewLine, lineNumbers) + @"</pre></td>
<td>" + string.Join(Environment.NewLine, lines) + @"</td>
</table>
</body>
</html>";
		}

		private void buttonOpenFile_Click(object sender, EventArgs e) {
			Process.Start(new ProcessStartInfo {
				FileName = "TortoiseGitMerge",
				Arguments = $"/base:\"{file.basePath}\" /mine:\"{file.srcPath}\" /basename:Base /minename:Patched",
				UseShellExecute = true
			});
		}

		private void buttonOpenPatch_Click(object sender, EventArgs e) {
			Process.Start(new ProcessStartInfo {
				FileName = "TortoiseGitUDiff",
				Arguments = $"\"{file.patchPath}\"",
				UseShellExecute = true
			});
		}
	}
}

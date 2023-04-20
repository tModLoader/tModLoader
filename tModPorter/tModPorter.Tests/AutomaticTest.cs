using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NUnit.Framework;

namespace tModPorter.Tests;

public class AutomaticTest {
	public static string TestModPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../TestData/TestData.csproj"));
	private static VisualStudioInstance instance = MSBuildLocator.RegisterDefaults();

	[OneTimeSetUp]
	public async Task Setup() {
		await LoadProject();
	}

	[TestCaseSource(nameof(GetTestCases))]
	public async Task RewriteCode(Document doc) {
		RemoveIfExists(Path.ChangeExtension(doc.FilePath!, ".Out.cs"));

		while (true) {
			var pDoc = doc;
			doc = await tModPorter.RewriteOnce(doc);
			if (doc == pDoc)
				break;
		}

		await AssertFixed(doc);
	}

	private static void RemoveIfExists(string path) {
		if (File.Exists(path))
			File.Delete(path);
	}

	[Test]
	public async Task ExpectedModCompiles() {
		using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

		var proj = await workspace.OpenProjectAsync(TestModPath[..^".csproj".Length] + "Expected.csproj");
		var comp = (await proj.GetCompilationAsync())!;
		using var peStream = new MemoryStream();
		var result = comp.Emit(peStream);
		foreach (var diag in result.Diagnostics) {
			if (diag.Severity == DiagnosticSeverity.Error)
				TestContext.Error.WriteLine(diag);
			else
				TestContext.Out.WriteLine(diag);
		}

		if (!result.Success) {
			Assert.Fail("Compilation Failed");
		}
	}

	[Test]
	public async Task ProjectWideRefactor() {
		var baseProject = await LoadProject();
		var testCaseDocs = baseProject.Documents.Where(doc => doc.FilePath!.Replace('\\', '/').Contains("/ProjectWide/")).ToArray();
		foreach (var doc in testCaseDocs) {
			RemoveIfExists(Path.ChangeExtension(doc.FilePath!, ".Out.cs"));
		}


		int pass = 0;
		var log = (ProgressUpdate update) => {
			if (update is not ProgressUpdate.Progress progress) {
				TestContext.Out.WriteLine(update);
				return;
			}

			int p = pass;
			if (progress.Pass > pass && Interlocked.CompareExchange(ref pass, progress.Pass, p) == p) {
				pass = progress.Pass;
				TestContext.Out.WriteLine("Pass " + pass);
			}
		};


		var project = await new tModPorter(dryRun: true).Process(baseProject, log);
		TestContext.Out.WriteLine("Complete. Comparing files...");

		var updatedDocs = project.GetChanges(baseProject).GetChangedDocuments().ToHashSet();
		Assert.Multiple(async () => {
			foreach (var testDoc in testCaseDocs) {
				if (!updatedDocs.Contains(testDoc.Id))
					Assert.Fail($"{testDoc.Name}: No content change!");

				await AssertFixed(project.GetDocument(testDoc.Id)!);
				TestContext.Out.WriteLine("Success: " + testDoc.Name);
			}
		});
	}

	private static async Task AssertFixed(Document doc) {
		var result = (await doc.GetTextAsync()).ToString().ReplaceLineEndings().TrimEnd();

		string fixedFilePath = Path.ChangeExtension(doc.FilePath!, ".Expected.cs");
		Assert.True(File.Exists(fixedFilePath), $"File '{fixedFilePath}' doesn't exist.");

		string fixedContent = StripNotYetImplemented(await File.ReadAllLinesAsync(fixedFilePath));
		if (doc.Project == _project) {
			if (fixedContent.Length == 0)
				return;

			Assert.Fail($"{doc.Name}: No content change!");
		}

		var outFilePath = Path.ChangeExtension(doc.FilePath!, ".Out.cs");
		if (fixedContent != result)
			await File.WriteAllTextAsync(outFilePath, result);
		else if (File.Exists(outFilePath))
			File.Delete(outFilePath);

		FileAssert.Equal(doc.Name, fixedContent, result);
	}

	private static string StripNotYetImplemented(string[] lines) {
		StringBuilder sb = new StringBuilder();
		bool ignoring = false;
		foreach (var line in lines) {
			var s = line.Trim();
			if (s == "// not-yet-implemented") {
				ignoring = true;
			}
			else if (s == "// instead-expect") {
				ignoring = false;
			}
			else if (!ignoring && !s.StartsWith('#')) {
				sb.AppendLine(line);
			}
		}
		return sb.ToString().TrimEnd();
	}

	private static Project? _project;
	private static async Task<Project> LoadProject() {
		if (_project is not null) return _project;

		using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
		workspace.WorkspaceFailed += (o, e) => {
			if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure && !e.Diagnostic.ToString().Contains("This mismatch may cause runtime failures"))
				throw new Exception(e.Diagnostic.ToString());

			Console.Error.WriteLine(e.Diagnostic.ToString());
		};

		if (!File.Exists(TestModPath)) {
			throw new FileNotFoundException("TestData.csproj not found.");
		}

		_project = await workspace.OpenProjectAsync(TestModPath);
		return _project;
	}

	public static IEnumerable<TestCaseData> GetTestCases() {
		return LoadProject().GetAwaiter().GetResult().Documents
			.Where(d => FilterTestCasePath(d.FilePath!.Replace('\\', '/')))
			.Select(d => new TestCaseData(d).SetArgDisplayNames(Path.GetFileName(d.FilePath)!))
			.ToArray();
	}

	private static bool FilterTestCasePath(string path) => !path.Contains("/Common/") && !path.Contains("/ProjectWide/") && !path.Contains("/obj/");
}
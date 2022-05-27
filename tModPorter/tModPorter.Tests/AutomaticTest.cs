using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		doc = await tModPorter.Rewrite(doc);
		if (doc.Project == _project)
			Assert.Fail("No content change!");

		await AssertFixed(doc);
	}

	[Test]
	public async Task FixedModCompiles() {
		using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

		var proj = await workspace.OpenProjectAsync(TestModPath[..^".csproj".Length] + "Fixed.csproj");
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

		var baseProject = await LoadProject();
		var project = await new tModPorter(dryRun: true).Process(baseProject, log);
		TestContext.Out.WriteLine("Complete. Comparing files...");

		var updatedDocs = project.GetChanges(baseProject).GetChangedDocuments().ToHashSet();
		Assert.Multiple(async () => {
			foreach (var doc in project.Documents) {
				if (!doc.FilePath!.Replace('\\', '/').Contains("/ProjectWide/"))
					continue;

				if (!updatedDocs.Contains(doc.Id))
					Assert.Fail($"{doc.Name}: No content change!");

				await AssertFixed(doc);
				TestContext.Out.WriteLine("Success: " + doc.Name);
			}
		});
	}

	private static async Task AssertFixed(Document doc) {
		var result = (await doc.GetTextAsync()).ToString();
		await File.WriteAllTextAsync(Path.ChangeExtension(doc.FilePath!, ".Out.cs"), result);

		string fixedFilePath = Path.ChangeExtension(doc.FilePath!, ".Fix.cs");
		Assert.True(File.Exists(fixedFilePath), $"File '{fixedFilePath}' doesn't exist.");

		string fixedContent = await File.ReadAllTextAsync(fixedFilePath);
		FileAssert.Equal(doc.Name, fixedContent, result);
	}

	private static Project? _project;
	private static async Task<Project> LoadProject() {
		if (_project is not null) return _project;

		using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
		workspace.WorkspaceFailed += (o, e) => {
			if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
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
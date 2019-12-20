using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Terraria.ModLoader
{
	public static class RoslynWrapper
	{
		public static CompilerErrorCollection Compile(string name, string outputPath, string[] references, string[] files, string[] preprocessorSymbols, bool includePdb, bool allowUnsafe) {
			var pdbPath = Path.ChangeExtension(outputPath, "pdb");

			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
				assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default,
				optimizationLevel: preprocessorSymbols.Contains("DEBUG") ? OptimizationLevel.Debug : OptimizationLevel.Release,
				allowUnsafe: allowUnsafe);

			var parseOptions = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessorSymbols);

			bool mono = Type.GetType("Mono.Runtime") != null;
			var emitOptions = new EmitOptions(debugInformationFormat: mono ? DebugInformationFormat.PortablePdb : DebugInformationFormat.Pdb);

			var refs = references.Select(s => MetadataReference.CreateFromFile(s));
			var src = files.Select(f => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(f), parseOptions, f, Encoding.UTF8));
			var comp = CSharpCompilation.Create(name, src, refs, options);

			EmitResult results;
			using (var peStream = File.OpenWrite(outputPath))
			using (var pdbStream = includePdb ? File.OpenWrite(pdbPath) : null) {
				results = comp.Emit(peStream, pdbStream, options: emitOptions);
			}

			var errors = new CompilerErrorCollection();
			foreach (var d in results.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning)) {
				var loc = d.Location.GetLineSpan();
				errors.Add(new CompilerError {
					ErrorNumber = d.Id,
					IsWarning = d.Severity == DiagnosticSeverity.Warning,
					ErrorText = d.GetMessage(),
					FileName = loc.Path ?? "",
					Line = loc.StartLinePosition.Line+1,
					Column = loc.StartLinePosition.Character
				});
			}

			return errors;
		}
	}
}

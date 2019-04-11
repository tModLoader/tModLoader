using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace Terraria.ModLoader
{
	public class RoslynWrapper
	{
		public static CompilerResults Compile(CompilerParameters args, string[] files) {
			var name = Path.GetFileNameWithoutExtension(args.OutputAssembly);
			var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithOptimizationLevel(args.IncludeDebugInformation ? OptimizationLevel.Debug : OptimizationLevel.Release)
				.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
			var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);

			var refs = args.ReferencedAssemblies.Cast<string>().Select(s => MetadataReference.CreateFromFile(s));
			var src = files.Select(f => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(f), parseOptions, f, Encoding.UTF8));
			var comp = CSharpCompilation.Create(name, src, refs, options);

			var res = comp.Emit(args.OutputAssembly, args.IncludeDebugInformation ? Path.ChangeExtension(args.OutputAssembly, "pdb") : null);
			var cRes = new CompilerResults(args.TempFiles);
			foreach (var d in res.Diagnostics) {
				if (d.Severity != DiagnosticSeverity.Error)
					continue;

				var loc = d.Location.GetLineSpan();
				var pos = loc.StartLinePosition;
				cRes.Errors.Add(new CompilerError(loc.Path ?? "", pos.Line, pos.Character, d.Id, d.GetMessage()));
			}

			return cRes;
		}

		/*
		 * Roslyn outputs pdb files that are incompatible with cecil modified assemblies (which we use for reload support)
		 * Mono.Cecil can parse the roslyn debug info, and then output a compatible pdb file and binary using the windows API
		 * 
		 * In addition, some remapping is required to use extension methods on mono.
		 */
		public static void PostProcess(string assemblyPath, bool mono, bool includeSymbols) {
			if (!mono && !includeSymbols)
				return; //nothing to do

			var asm = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters {
				ReadSymbols = includeSymbols,
				ReadWrite = true
			});

			if (mono)
				FixExtensionMethods(asm);

			using (asm) {
				asm.Write(new WriterParameters { 
					WriteSymbols = includeSymbols,
					SymbolWriterProvider = (includeSymbols && mono) ? new MdbWriterProvider() : null
				});
			}
		}

		// Extension methods are marked with an attribute which is located in mscorlib on .NET but in System.Core on Mono
		// Find all extension attributes and change their assembly references
		private static void FixExtensionMethods(AssemblyDefinition asm) {
			AssemblyNameReference SystemCoreRef = null;
			AssemblyNameReference GetOrAddSystemCore(ModuleDefinition module) {
				if (SystemCoreRef != null)
					return SystemCoreRef;

				var assemblyRef = module.AssemblyReferences.SingleOrDefault(r => r.Name == "System.Core");
				if (assemblyRef == null) {
					//System.Linq.Enumerable is in System.Core
					var name = System.Reflection.Assembly.GetAssembly(typeof(Enumerable)).GetName();
					assemblyRef = new AssemblyNameReference(name.Name, name.Version) {
						Culture = name.CultureInfo.Name,
						PublicKeyToken = name.GetPublicKeyToken(),
						HashAlgorithm = (AssemblyHashAlgorithm)name.HashAlgorithm
					};
					module.AssemblyReferences.Add(assemblyRef);
				}
				return assemblyRef;
			}

			foreach (var module in asm.Modules)
				foreach (var type in module.Types)
					foreach (var met in type.Methods)
						foreach (var attr in met.CustomAttributes)
							if (attr.AttributeType.FullName == "System.Runtime.CompilerServices.ExtensionAttribute")
								attr.AttributeType.Scope = GetOrAddSystemCore(module);
		}
	}
}

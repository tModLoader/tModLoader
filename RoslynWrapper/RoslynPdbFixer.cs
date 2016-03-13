using System.IO;
using Mono.Cecil;

namespace Terraria.ModLoader
{
    public class RoslynPdbFixer
    {
        /// <summary>
        /// Roslyn outputs pdb files that are incompatible with cecil modified assemblies (which we use for reload support)
        /// Mono.Cecil can parse the roslyn debug info, and then output a compatible pdb file and binary using the windows API
        /// </summary>
        public static void Fix(string dll) {
            var asm = AssemblyDefinition.ReadAssembly(dll);
            asm.MainModule.ReadSymbols();
            var fstream = File.Create(dll);
            asm.Write(fstream, new WriterParameters { WriteSymbols = true });
            fstream.Close();
        }
    }
}

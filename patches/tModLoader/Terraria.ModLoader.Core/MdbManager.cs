using Mono.Cecil;
using Mono.Cecil.Mdb;
using Mono.CompilerServices.SymbolWriter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Core
{
	internal class MdbManager
	{
		private class MdbModuleEntry
		{
			public readonly ModuleDefinition module;
			public readonly MdbReader mdbReader;

			public MdbModuleEntry(ModuleDefinition module, Stream symbolStream) {
				this.module = module;
				mdbReader = new MdbReader(module, MonoSymbolFile.ReadSymbolFile(symbolStream));
			}
		}

		private static readonly IDictionary<Guid, MdbModuleEntry> moduleDict = new Dictionary<Guid, MdbModuleEntry>();
		public static void RegisterMdb(ModuleDefinition module, byte[] mdb) {
			moduleDict[module.Mvid] = new MdbModuleEntry(module, new MemoryStream(mdb));
		}

		private static readonly FieldInfo f_fileName = Logging.f_fileName;
		private static readonly FieldInfo f_lineNo = typeof(StackFrame).GetField("lineNumber", BindingFlags.Instance | BindingFlags.NonPublic);
		public static void Symbolize(StackFrame[] frames) {
			if (frames == null)
				return;

			foreach (var frame in frames) {
				var fileName = frame.GetFileName();
				if (fileName != null && fileName[0] != '<')
					continue;
				
				var method = frame.GetMethod();
				var module = method?.DeclaringType?.Module;
				if (module == null)
					continue;

				if (!TryResolveLocation(module.ModuleVersionId, method.MetadataToken, frame.GetILOffset(), out fileName, out var line))
					fileName = module.Name;
				
				f_fileName.SetValue(frame, fileName);
				f_lineNo.SetValue(frame, line);
			}
		}

		private static bool TryResolveLocation(Guid mvid, int mdToken, int ilOffset, out string fileName, out int line)
		{
			fileName = "";
			line = -1;
			
			if (!moduleDict.TryGetValue(mvid, out var moduleEntry))
				return false;

			if (!(moduleEntry.module.LookupToken(mdToken) is MethodReference mRef))
				return false;
			
			var debugInfo = moduleEntry.mdbReader.Read(mRef.Resolve());
			if (!debugInfo.HasSequencePoints)
				return false;

			var prev = debugInfo.SequencePoints.OrderBy(sp => sp.Offset).LastOrDefault(sp => sp.Offset <= ilOffset);
			if (prev == null)
				return false;

			fileName = prev.Document.Url;
			line = prev.StartLine;
			return true;
		}
	}
}

using System;
using System.CodeDom.Compiler;

namespace Terraria.ModLoader.Exceptions
{
	internal class BuildException : Exception
	{
		public CompilerErrorCollection compileErrors;

		public BuildException(string message) : base(message) { }

		public BuildException(string message, Exception innerException) : base(message, innerException) { }
	}
}

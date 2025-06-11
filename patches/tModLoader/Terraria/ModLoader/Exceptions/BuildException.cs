using System;
using System.CodeDom.Compiler;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader.Exceptions;

internal class BuildException : Exception
{
	public CompilerErrorCollection compileErrors;
	public ErrorReporting.TMLErrorCode errorCode = ErrorReporting.TMLErrorCode.TML002;

	public BuildException(string message, ErrorReporting.TMLErrorCode errorCode = ErrorReporting.TMLErrorCode.TML002) : base(message) {
		this.errorCode = errorCode;
	}

	public BuildException(string message, Exception innerException, ErrorReporting.TMLErrorCode errorCode = ErrorReporting.TMLErrorCode.TML002) : base(message, innerException) {
		this.errorCode = errorCode;
	}
}

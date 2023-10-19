using System.CodeDom.Compiler;
using System.IO;

namespace tModLoader.SourceGenerators;

public sealed class IndentedStringBuilder : IndentedTextWriter
{
	public IndentedStringBuilder() : base(new StringWriter(), "\t")
	{
	}

	public sealed override string ToString()
	{
		var stringWriter = (StringWriter)InnerWriter;
		var stringBuilder = stringWriter.GetStringBuilder();
		stringWriter.Close();
		return stringBuilder.ToString();
	}
}

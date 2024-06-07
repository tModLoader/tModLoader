using System.CodeDom.Compiler;
using System.IO;

namespace tModLoader.Analyzers.SourceGenerator;

public sealed class IndentedStringBuilder : IndentedTextWriter
{
	public readonly ref struct ScopeBlock
	{
		private readonly IndentedStringBuilder writer;

		public ScopeBlock(IndentedStringBuilder writer)
		{
			this.writer = writer;

			writer.WriteLine('{');
			writer.Indent++;
		}

		public void Dispose()
		{
			writer.Indent--;
			writer.WriteLine('}');
		}
	}

	public IndentedStringBuilder() : base(new StringWriter(), "\t") {
	}

	public sealed override string ToString() {
		var stringWriter = (StringWriter)InnerWriter;
		var stringBuilder = stringWriter.GetStringBuilder();
		stringWriter.Close();
		return stringBuilder.ToString();
	}

	public ScopeBlock DoScope()
	{
		return new ScopeBlock(this);
	}
}

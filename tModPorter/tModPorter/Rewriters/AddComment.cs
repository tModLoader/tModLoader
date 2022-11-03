using Microsoft.CodeAnalysis;

namespace tModPorter.Rewriters
{
	public record AddComment(string comment)
	{
		public T Apply<T>(T t) where T : SyntaxNode => t.WithBlockComment(comment);

		public static AddComment Comment(string comment) => new(comment);
		public static AddComment Removed(string comment) => Comment(("Note: Removed. " + comment).TrimEnd());
	}
}

namespace tModPorter.Rewriters;

public enum RewriterType {
	None,
	AnonymousMethod,
	Assignment,
	Identifier,
	Invocation,
	MemberAccess,
	Method,
	UsingDirective,
}
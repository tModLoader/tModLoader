using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Exceptions;

public class MultipleException : AggregateException
{
	public static readonly string DefaultMessage = "Multiple errors occurred.";

	private readonly string _message;

	public MultipleException(IEnumerable<Exception> exceptions) : this(DefaultMessage, exceptions) { }

	public MultipleException(string message, IEnumerable<Exception> exceptions) : base(exceptions)
	{
		_message = message;
	}


	public override string Message => _message;
}

using System;

namespace ReLogic.Content;

internal class ContentRejectionAssetReaderException : IRejectionReason
{
	private readonly Exception e;

	public ContentRejectionAssetReaderException(Exception e)
	{
		this.e = e;
	}

	public string GetReason() => e.ToString();
}
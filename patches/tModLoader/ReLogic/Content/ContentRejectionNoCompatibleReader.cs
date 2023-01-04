namespace ReLogic.Content;

public class ContentRejectionNoCompatibleReader : IRejectionReason
{
	private readonly string reason;

	public ContentRejectionNoCompatibleReader(string extension, string[] supportedExtensions)
	{
		reason = $"Files of type '{extension}' cannot be read. Supported extensions are: {string.Join(" ", supportedExtensions)}";
	}

	public string GetReason() => reason;
}

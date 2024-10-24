using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Setup.GUI.Services;

public interface IFilesService
{
	public Task<IStorageFile?> OpenFile(
		string title,
		IReadOnlyList<FilePickerFileType>? filters = null,
		string? initialDirectory = null,
		string? initialFileName = null);

	public Task<IStorageFolder?> OpenFolder(
		string title,
		string? initialDirectory = null);
}
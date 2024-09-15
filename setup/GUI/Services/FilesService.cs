using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Setup.GUI.Avalonia.Services;

public sealed class FilesService : IFilesService
{
	private readonly IStorageProvider storageProvider;

	public FilesService(IStorageProvider storageProvider)
	{
		this.storageProvider = storageProvider;
	}

	public async Task<IStorageFile?> OpenFile(
		string title,
		IReadOnlyList<FilePickerFileType>? filters = null,
		string? initialDirectory = null,
		string? initialFileName = null)
	{
		var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
		{
			Title = title,
			AllowMultiple = false,
			SuggestedStartLocation = initialDirectory != null ? await storageProvider.TryGetFolderFromPathAsync(initialDirectory) : null,
			FileTypeFilter = filters,
			SuggestedFileName = initialFileName,
		});

		return files.Count >= 1 ? files[0] : null;
	}

	public async Task<IStorageFolder?> OpenFolder(string title, string? initialDirectory = null)
	{
		IReadOnlyList<IStorageFolder> picked = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
			Title = title,
			AllowMultiple = false,
			SuggestedStartLocation = initialDirectory != null ? await storageProvider.TryGetFolderFromPathAsync(initialDirectory) : null,
		});

		return picked.Count >= 1 ? picked[0] : null;
	}
}
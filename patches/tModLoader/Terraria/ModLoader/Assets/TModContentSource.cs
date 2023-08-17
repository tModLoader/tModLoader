using System;
using System.IO;
using System.Linq;
using ReLogic.Content.Sources;
using Terraria.Initializers;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.Assets;

public class TModContentSource : ContentSource
{
	private readonly TmodFile file;

	public TModContentSource(TmodFile file)
	{
		this.file = file ?? throw new ArgumentNullException(nameof(file));

		// Skip loading assets if this is a dedicated server
		if (Main.dedServ)
			return;

		// Filter assets based on the current reader set. Custom mod asset readers will need to be added before content sources are initialized
		// Unfortunately this means that if a reader is missing, the asset will be missing, causing a misleading error message, but there's little
		// we can do about that while still supporting multiple files with the same extension. Unless we provided a hardcoded exclusion for .cs files...
		SetAssetNames(file
			.Select(fileEntry => fileEntry.Name)
			.Where(name => AssetInitializer.assetReaderCollection.TryGetReader(Path.GetExtension(name), out _)));
	}

	public override Stream OpenStream(string assetName) => file.GetStream(assetName, newFileStream: true); //todo, might be sloww
}

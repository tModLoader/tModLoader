using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tModLoaderTests")]
namespace Terraria.ModLoader.Core
{
	internal class LocalMod
	{
		public readonly TmodFile modFile;
		public readonly BuildProperties properties;
		public DateTime lastModified;

		public string Name => modFile.name;
		public string DisplayName => string.IsNullOrEmpty(properties.displayName) ? Name : properties.displayName;
		public Version tModLoaderVersion => properties.buildVersion;

		public bool Enabled {
			get => ModLoader.IsEnabled(Name);
			set => ModLoader.SetModEnabled(Name, value);
		}

		public override string ToString() => Name;

		public LocalMod(TmodFile modFile, BuildProperties properties) {
			this.modFile = modFile;
			this.properties = properties;
		}

		public LocalMod(TmodFile modFile) : this(modFile, BuildProperties.ReadModFile(modFile)) {
		}
	}
}

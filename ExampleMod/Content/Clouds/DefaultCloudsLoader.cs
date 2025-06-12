using Terraria.ModLoader;

namespace ExampleMod.Content.Clouds
{
	// By default, image files in a "Clouds" folder will autoload as clouds. We use this class to manually load ExampleCloud.png before autoloading happens to register the cloud with custom parameters instead.
	public class DefaultCloudsLoader : ILoadable
	{
		public void Load(Mod mod) {
			// Registers a new simple cloud. See Content/Clouds/AdvancedExampleCloud.cs for a cloud with custom logic.
			CloudLoader.AddCloudFromTexture(mod, "ExampleMod/Content/Clouds/ExampleCloud", spawnChance: 0.1f, rareCloud: false);
		}

		public void Unload() {
		}
	}
}

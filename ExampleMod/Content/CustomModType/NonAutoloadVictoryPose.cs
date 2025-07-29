using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// This class doesn't autoload because it has a non-default constructor, it is loaded manually 2 times in NonAutoloadVictoryPoseLoader.Load
	public class NonAutoloadVictoryPose : ModVictoryPose
	{
		public class NonAutoloadVictoryPoseLoader : ILoadable
		{
			public void Load(Mod mod) {
				// Manually load additional ModVictoryPose from this mod.
				mod.AddContent(new NonAutoloadVictoryPose("ShortPose", 120));
				mod.AddContent(new NonAutoloadVictoryPose("LongPose", 180));
			}

			public void Unload() {
			}
		}

		private readonly string nameOverride;
		private readonly int duration;

		// The internal name of ModTypes must be unique for a given mod. We override Name to provide a custom internal name for this content because the default value, the classname, would not be unique.
		public override string Name => nameOverride;

		public override string Texture => "ExampleMod/Content/Items/ExampleTooltipsItem"; // A texture with 4 smileys

		public NonAutoloadVictoryPose(string name, int duration) {
			this.nameOverride = name;
			this.duration = duration;
		}

		public override void SetStaticDefaults() {
			PoseTime = duration;

			VictoryPoseID.Sets.NonBoss[Type] = true;
		}

		public override Rectangle? GetTextureFrame(Asset<Texture2D> texture) {
			// Randomly choose one of the 4 faces in the texture.
			return texture.Frame(1, 4, 0, Main.rand.Next(4));
		}

		public override void OnStartPose(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Main.blockMouse = true;
			}
		}

		public override void Update(Player player) {
			if (player.itemAnimation == 0) {
				player.bodyFrame.Y = player.bodyFrame.Height; // 1 Hand up
			}
		}
	}
}

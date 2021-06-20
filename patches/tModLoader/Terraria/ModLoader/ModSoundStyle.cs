using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	public class ModSoundStyle : SoundStyle
	{
		public readonly string SoundPath;
		public readonly int Variations;

		private Asset<SoundEffect>[] variants;
		private Asset<SoundEffect> effect;

		public override bool IsTrackable => true;

		public ModSoundStyle(string soundPath, int variations = 0, SoundType type = SoundType.Sound, float volume = 1.0f, float pitch = 0f, float pitchVariance = 0f) : base(volume, pitch, pitchVariance, type) {
			SoundPath = soundPath;
			Variations = variations;
			if (variations > 0)
				variants = new Asset<SoundEffect>[variations];
		}

		public override SoundEffect GetRandomSound() {
			Asset<SoundEffect> asset;
			if (Variations == 0) {
				asset = effect ??= ModContent.Request<SoundEffect>(SoundPath, AssetRequestMode.ImmediateLoad);
			}
			else {
				int variant = Main.rand.Next(Variations);
				asset = variants[variant] ??= ModContent.Request<SoundEffect>(SoundPath + (variant + 1), AssetRequestMode.ImmediateLoad);
			}

			return asset.Value;
		}
	}
}

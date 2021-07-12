using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;

namespace Terraria.Audio
{
	public partial class SoundStyle
	{
		public float Pitch { get; private set; }

		public virtual SoundEffectInstance Play(Vector2? position) {
			SlotId slot;

			if (position.HasValue)
				slot = SoundEngine.PlayTrackedSound(this, position.Value);
			else
				slot = SoundEngine.PlayTrackedSound(this);

			return slot.IsValid ? SoundEngine.GetActiveSound(slot)?.Sound : null;
		}

		protected static void CalculateVolumeAndPan(Vector2 position, float maxDistance, out float volume, out float pan) {
			Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

			volume = MathHelper.Clamp(1f - (Vector2.Distance(position, screenCenter) / maxDistance), 0f, 1f);
			pan = MathHelper.Clamp((position.X - screenCenter.X) / (Main.screenWidth * 0.5f), -1f, 1f);
		}
	}
}

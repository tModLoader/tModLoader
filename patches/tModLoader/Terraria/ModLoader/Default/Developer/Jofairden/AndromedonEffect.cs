using System;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal sealed class AndromedonEffect : ICloneable
	{
		public bool HasSetBonus;
		public bool HasAura => _auraTime > 0;
		public float LayerStrength;
		public float ShaderStrength;

		private int _lastLife = -1;
		internal int _auraTime;

		private int? _headSlot;
		private int? _bodySlot;
		private int? _legSlot;

		public void ResetEffects() {
			HasSetBonus = false;
		}

		public void UpdateDead() {
			HasSetBonus = false;
			_auraTime = 0;
		}

		public void UpdateEffects(Player player) {
			if (!HasAura) {
				if (ShaderStrength > 0f) {
					ShaderStrength -= 0.02f;
				}
			}
			else {
				if (ShaderStrength <= 1f) {
					ShaderStrength += 0.02f;
				}
				else {
					_auraTime--;
				}
			}

			if (!HasSetBonus) {
				if (LayerStrength > 0) {
					LayerStrength -= 0.02f;
				}
			}
			else {
				if (LayerStrength <= 1) {
					LayerStrength += 0.02f;
				}
			}

			if (ShaderStrength > 0f) {
				Lighting.AddLight(
					player.Center,
					Main.DiscoColor.ToVector3() * LayerStrength * ((float)Main.time % 2) * (float)Math.Abs(Math.Log10(Main.essScale * 0.75f)));
			}
		}

		public void UpdateAura(Player player) {
			if (_lastLife <= -1) {
				_lastLife = player.statLife;
			}
			int diff = _lastLife - player.statLife;
			if (diff >= 0.1f * player.statLifeMax2) {
				_auraTime = 300 + diff;
			}
			_lastLife = player.statLife;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}
}

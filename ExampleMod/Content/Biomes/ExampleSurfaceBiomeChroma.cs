using Microsoft.Xna.Framework;
using ReLogic.Peripherals.RGB;
using System;
using Terraria;
using Terraria.GameContent.RGB;
using Terraria.Initializers;
using Terraria.ModLoader;

namespace ExampleMod.Content.Biomes
{
	internal class ExampleSurfaceBiomeChroma : ILoadable
	{
		public static readonly ChromaCondition ExampleSurfaceCondition = new CommonConditions.SimpleCondition((Player player) => ModContent.GetInstance<ExampleSurfaceBiome>().IsBiomeActive(player));

		void ILoadable.Load(Mod mod) {
			// TODO: Properly load and unload. Test reload.
			// TODO: Probably integrate this into ModSceneEffect/ModBiome, but still allow it to be done outside of that.
			ChromaInitializer.RegisterShader("ExampleMod/ExampleSurfaceBiome", new DungeonShaderClone(), ExampleSurfaceCondition, ShaderLayer.Background);
			// Do shaderLayers overwrite each other, or overlap each other?
		}

		void ILoadable.Unload() {
		}
	}

	public class DungeonShaderClone : ChromaShader
	{
		private readonly Vector4 _backgroundColor = new Color(5, 5, 5).ToVector4();
		private readonly Vector4 _spiritTrailColor = new Color(6, 51, 222).ToVector4();
		private readonly Vector4 _spiritColor = Color.White.ToVector4();

		[RgbProcessor(new EffectDetailLevel[] {
			EffectDetailLevel.Low,
			EffectDetailLevel.High
		})]
		private void ProcessHighDetail(RgbDevice device, Fragment fragment, EffectDetailLevel quality, float time) {
			// TODO: document what this is doing. No idea where the randomness is coming from.
			for (int i = 0; i < fragment.Count; i++) {
				Point gridPositionOfIndex = fragment.GetGridPositionOfIndex(i);
				Vector2 canvasPositionOfIndex = fragment.GetCanvasPositionOfIndex(i);
				float num = ((NoiseHelper.GetStaticNoise(gridPositionOfIndex.Y) * 10f + time /** 4*/) % 10f - (canvasPositionOfIndex.X + 2f)) * 0.5f;
				Vector4 vector = _backgroundColor;
				if (num > 0f) {
					float num2 = Math.Max(0f, 1.2f - num);
					float amount = MathHelper.Clamp(num2 * num2 * num2, 0f, 1f);
					if (num < 0.2f)
						num2 = num / 0.2f;

					Vector4 value = Vector4.Lerp(_spiritTrailColor, _spiritColor, amount);
					vector = Vector4.Lerp(vector, value, num2);
				}

				fragment.SetColor(i, vector);
			}
		}
	}
}

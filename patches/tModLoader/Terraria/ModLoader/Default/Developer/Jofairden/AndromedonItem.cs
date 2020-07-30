using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal abstract class AndromedonItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";
		public sealed override string SetName => "PowerRanger";
		public const int ShaderNumSegments = 8;
		public const int ShaderDrawOffset = 2;

		private static int? ShaderId;

		public sealed override void SetStaticDefaults() {
			DisplayName.SetDefault($"Andromedon {EquipTypeSuffix}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}

		protected static Vector2 GetDrawOffset(int i) {
			return new Vector2(0, ShaderDrawOffset).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);

			//var halfDist = ShaderDrawOffset / 2;
			//var offY = halfDist + halfDist * i % halfDist;
			//return new Vector2(0, offY).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);
		}

		private static void BeginShaderBatch(SpriteBatch batch) {
			batch.End();
			RasterizerState rasterizerState = Main.LocalPlayer.gravDir == 1f ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
			batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public static DrawDataInfo GetHeadDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X + drawPlayer.width / 2f - drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X),
							  (int)(drawInfo.position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y))
						  + drawPlayer.headPosition
						  + drawInfo.headOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.headOrigin,
				Rotation = drawPlayer.headRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetBodyDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f))
						  + drawPlayer.bodyPosition
						  + drawInfo.bodyOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.bodyOrigin,
				Rotation = drawPlayer.bodyRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetLegDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f))
						  + drawPlayer.legPosition
						  + drawInfo.legOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.legFrame,
				Origin = drawInfo.legOrigin,
				Rotation = drawPlayer.legRotation,
				Texture = texture
			};
		}

		public static PlayerLayer CreateShaderLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc) {
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) => {
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis) {
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);
				Player drawPlayer = drawInfo.drawPlayer;
				var devPlayer = DeveloperPlayer.GetPlayer(drawPlayer);
				SpriteEffects effects = SpriteEffects.None;
				if (drawPlayer.direction == -1) {
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1) {
					effects |= SpriteEffects.FlipVertically;
				}

				var data = new DrawData(
					drawDataInfo.Texture,
					drawDataInfo.Position,
					drawDataInfo.Frame,
					Color.White * Main.essScale * devPlayer.AndromedonEffect.LayerStrength * devPlayer.AndromedonEffect.ShaderStrength,
					drawDataInfo.Rotation,
					drawDataInfo.Origin,
					1f,
					effects,
					0);

				BeginShaderBatch(Main.spriteBatch);
				ShaderId ??= GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye);
				GameShaders.Armor.Apply(ShaderId.Value, drawPlayer, data);
				var centerPos = data.position;

				for (int i = 0; i < ShaderNumSegments; i++) {
					data.position = centerPos + GetDrawOffset(i);
					data.Draw(Main.spriteBatch);
				}

				data.position = centerPos;
			});
		}

		public static PlayerLayer CreateGlowLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc) {
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) => {
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis) {
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);
				Player drawPlayer = drawInfo.drawPlayer;
				var devPlayer = DeveloperPlayer.GetPlayer(drawPlayer);
				SpriteEffects effects = SpriteEffects.None;

				if (drawPlayer.direction == -1) {
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1) {
					effects |= SpriteEffects.FlipVertically;
				}

				var data = new DrawData(
					drawDataInfo.Texture,
					drawDataInfo.Position,
					drawDataInfo.Frame,
					Color.White * Main.essScale * devPlayer.AndromedonEffect.LayerStrength,
					drawDataInfo.Rotation,
					drawDataInfo.Origin,
					1f,
					effects,
					0);

				if (devPlayer.AndromedonEffect.HasAura) {
					ShaderId ??= GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye);
					data.shader = ShaderId.Value;
				}

				throw new NotImplementedException(); //TODO: Reimplement //Main.playerDrawData.Add(data);
			});
		}
	}
}

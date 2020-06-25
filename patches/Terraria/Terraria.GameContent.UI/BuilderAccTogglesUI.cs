using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI.Gamepad;

namespace Terraria.GameContent.UI
{
	public class BuilderAccTogglesUI
	{
		public delegate bool GetIsAvailablemethod(Player player);

		public delegate void PerformClickMethod(Player player);

		public interface IBuilderAccToggle
		{
			void Draw(SpriteBatch spriteBatch, Vector2 iconAnchorPositionOnScreen, Player targetPlayer);

			bool IsAvailableFor(Player targetPlayer);
		}

		private class CommonBuilderAccToggle : IBuilderAccToggle
		{
			public delegate bool GetToggleStateMethod(Player player);

			public Asset<Texture2D> Texture;
			public SpriteFrame Frame = new SpriteFrame(1, 1);
			public SpriteFrame? FrameOverrideWhenOff;
			public Asset<Texture2D> HoverOutlineTexture;
			public SpriteFrame? HoverOutlineFrame;
			private string _hoverWhenEnabledKey;
			private string _hoverWhenDisabledKey;
			private GetToggleStateMethod _toggleStateProvider;
			private PerformClickMethod _performClickMethod;
			private GetIsAvailablemethod _getIsAvailableMethod;

			public CommonBuilderAccToggle(GetIsAvailablemethod isAvailableMethod, GetToggleStateMethod toggleStateProvider, PerformClickMethod clickMethod, string hoverWhenEnabledKey, string hoverWhenDisabledKey) {
				_toggleStateProvider = toggleStateProvider;
				_performClickMethod = clickMethod;
				_getIsAvailableMethod = isAvailableMethod;
				_hoverWhenEnabledKey = hoverWhenEnabledKey;
				_hoverWhenDisabledKey = hoverWhenDisabledKey;
			}

			public void Draw(SpriteBatch spriteBatch, Vector2 iconAnchorPositionOnScreen, Player targetPlayer) {
				Rectangle rectangle = Utils.CenteredRectangle(iconAnchorPositionOnScreen, new Vector2(14f, 14f));
				Point value = Main.MouseScreen.ToPoint();
				Texture2D value2 = Texture.Value;
				bool num = rectangle.Contains(value) && !PlayerInput.IgnoreMouseInterface;
				bool flag = _toggleStateProvider(targetPlayer);
				Color color = flag ? Color.White : Color.Gray;
				if (num && Main.mouseLeft && Main.mouseLeftRelease) {
					_performClickMethod(targetPlayer);
					SoundEngine.PlaySound(12);
					Main.mouseLeftRelease = false;
				}

				Rectangle sourceRectangle = Frame.GetSourceRectangle(value2);
				if (!flag && FrameOverrideWhenOff.HasValue)
					sourceRectangle = FrameOverrideWhenOff.Value.GetSourceRectangle(value2);

				spriteBatch.Draw(value2, iconAnchorPositionOnScreen, sourceRectangle, color, 0f, sourceRectangle.Size() / 2f, 1f, SpriteEffects.None, 0f);
				if (num) {
					targetPlayer.mouseInterface = true;
					string textValue = Language.GetTextValue(flag ? _hoverWhenEnabledKey : _hoverWhenDisabledKey);
					Main.instance.MouseText(textValue, 0, 0);
					Main.mouseText = true;
					Texture2D value3 = HoverOutlineTexture.Value;
					Rectangle rectangle2 = HoverOutlineFrame.HasValue ? HoverOutlineFrame.Value.GetSourceRectangle(value3) : value3.Frame();
					spriteBatch.Draw(value3, iconAnchorPositionOnScreen, rectangle2, Main.OurFavoriteColor, 0f, rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
				}
			}

			public bool IsAvailableFor(Player targetPlayer) => _getIsAvailableMethod(targetPlayer);
		}

		public enum WireToggleState
		{
			Bright,
			Normal,
			Faded,
			Count
		}

		private class WireBuilderAccToggle : IBuilderAccToggle
		{
			public delegate WireToggleState GetStateMethod(Player player);

			public SpriteFrame Frame = new SpriteFrame(1, 1);
			public Asset<Texture2D> Texture;
			private string _nameKey;
			private GetStateMethod _stateProvider;
			private PerformClickMethod _performClickMethod;
			private GetIsAvailablemethod _getIsAvailableMethod;

			public WireBuilderAccToggle(GetIsAvailablemethod isAvailableMethod, GetStateMethod stateProvider, PerformClickMethod clickMethod, string nameKey) {
				_stateProvider = stateProvider;
				_performClickMethod = clickMethod;
				_getIsAvailableMethod = isAvailableMethod;
				_nameKey = nameKey;
			}

			public void Draw(SpriteBatch spriteBatch, Vector2 iconAnchorPositionOnScreen, Player targetPlayer) {
				Rectangle rectangle = Utils.CenteredRectangle(iconAnchorPositionOnScreen, new Vector2(14f, 14f));
				Point value = Main.MouseScreen.ToPoint();
				Texture2D value2 = Texture.Value;
				bool flag = rectangle.Contains(value) && !PlayerInput.IgnoreMouseInterface;
				WireToggleState wireToggleState = _stateProvider(targetPlayer);
				Color color = Color.White;
				switch (wireToggleState) {
					case WireToggleState.Bright:
						color = Color.White;
						break;
					case WireToggleState.Normal:
						color = Color.Gray;
						break;
					case WireToggleState.Faded:
						color = Color.Gray * 0.66f;
						break;
				}

				if (flag) {
					targetPlayer.mouseInterface = true;
					string arg = "";
					switch (wireToggleState) {
						case WireToggleState.Bright:
							arg = Language.GetTextValue("GameUI.Bright");
							break;
						case WireToggleState.Normal:
							arg = Language.GetTextValue("GameUI.Normal");
							break;
						case WireToggleState.Faded:
							arg = Language.GetTextValue("GameUI.Faded");
							break;
					}

					string cursorText = $"{Language.GetTextValue(_nameKey)}: {arg}";
					Main.instance.MouseText(cursorText, 0, 0);
					Main.mouseText = true;
					Texture2D value3 = TextureAssets.InfoIcon[13].Value;
					spriteBatch.Draw(value3, iconAnchorPositionOnScreen, null, Main.OurFavoriteColor, 0f, value3.Size() / 2f, 1f, SpriteEffects.None, 0f);
				}

				if (flag && Main.mouseLeft && Main.mouseLeftRelease) {
					_performClickMethod(targetPlayer);
					SoundEngine.PlaySound(12);
					Main.mouseLeftRelease = false;
				}

				Rectangle sourceRectangle = Frame.GetSourceRectangle(value2);
				spriteBatch.Draw(value2, iconAnchorPositionOnScreen, sourceRectangle, color, 0f, sourceRectangle.Size() / 2f, 1f, SpriteEffects.None, 0f);
			}

			public bool IsAvailableFor(Player targetPlayer) => _getIsAvailableMethod(targetPlayer);
		}

		private const int AccEnabledValue = 0;
		private const int AccDisabledValue = 1;
		private const int AccIndex_RulerLine = 0;
		private const int AccIndex_RulerGrid = 1;
		private const int AccIndex_AutoPaint = 2;
		private const int AccIndex_AutoActuator = 3;
		private const int AccIndex_RedWiresVisibility = 4;
		private const int AccIndex_BlueWiresVisibility = 5;
		private const int AccIndex_GreenWiresVisibility = 6;
		private const int AccIndex_YellowWiresVisibility = 7;
		private const int AccIndex_MasterWiresVisibility = 8;
		private const int AccIndex_ActuatorsVisibility = 9;
		private const int AccIndex_BlockSwap = 10;
		private List<IBuilderAccToggle> _toggles = new List<IBuilderAccToggle>();

		private static bool CanShowWires(Player player) => player.InfoAccMechShowWires;

		private static void CycleWireToggleState(Player player, int accIndex) {
			player.builderAccStatus[accIndex]++;
			if (player.builderAccStatus[accIndex] >= 3)
				player.builderAccStatus[accIndex] = 0;
		}

		private void LazyInit() {
			byte rowToUse = 1;
			byte rows = 2;
			byte columns = 10;
			SpriteFrame spriteFrame = new SpriteFrame(columns, rows);
			Asset<Texture2D> builderAcc = TextureAssets.BuilderAcc;
			Asset<Texture2D> hoverOutlineTexture = TextureAssets.InfoIcon[13];
			SpriteFrame spriteFrame2 = new SpriteFrame(1, 1);
			spriteFrame2.PaddingX = 0;
			spriteFrame2.PaddingY = 0;
			SpriteFrame value = spriteFrame2;
			_toggles.Clear();
			List<IBuilderAccToggle> toggles = _toggles;
			List<IBuilderAccToggle> list = new List<IBuilderAccToggle>();
			CommonBuilderAccToggle obj = new CommonBuilderAccToggle((Player player) => true, (Player player) => player.builderAccStatus[10] == 0, delegate (Player player) {
				player.builderAccStatus[10] = ((player.builderAccStatus[10] == 0) ? 1 : 0);
			}, "GameUI.BlockReplacerOn", "GameUI.BlockReplacerOff") {
				Texture = TextureAssets.blockReplaceIcon[0],
				HoverOutlineTexture = TextureAssets.blockReplaceIcon[0]
			};

			spriteFrame2 = (obj.Frame = new SpriteFrame(3, 1, 0, 0) {
				PaddingY = 0
			});

			spriteFrame2 = new SpriteFrame(3, 1, 1, 0) {
				PaddingY = 0
			};

			obj.FrameOverrideWhenOff = spriteFrame2;
			spriteFrame2 = new SpriteFrame(3, 1, 2, 0) {
				PaddingY = 0
			};

			obj.HoverOutlineFrame = spriteFrame2;
			list.Add(obj);
			list.Add(new CommonBuilderAccToggle((Player player) => player.rulerLine, (Player player) => player.builderAccStatus[0] == 0, delegate (Player player) {
				player.builderAccStatus[0] = ((player.builderAccStatus[0] == 0) ? 1 : 0);
			}, "GameUI.RulerOn", "GameUI.RulerOff") {
				Texture = builderAcc,
				HoverOutlineTexture = hoverOutlineTexture,
				Frame = spriteFrame.With(0, rowToUse),
				HoverOutlineFrame = value
			});

			list.Add(new CommonBuilderAccToggle((Player player) => player.rulerGrid, (Player player) => player.builderAccStatus[1] == 0, delegate (Player player) {
				player.builderAccStatus[1] = ((player.builderAccStatus[1] == 0) ? 1 : 0);
			}, "GameUI.MechanicalRulerOn", "GameUI.MechanicalRulerOff") {
				Texture = builderAcc,
				HoverOutlineTexture = hoverOutlineTexture,
				Frame = spriteFrame.With(1, rowToUse),
				HoverOutlineFrame = value
			});

			list.Add(new CommonBuilderAccToggle((Player player) => player.autoPaint, (Player player) => player.builderAccStatus[2] == 0, delegate (Player player) {
				player.builderAccStatus[2] = ((player.builderAccStatus[2] == 0) ? 1 : 0);
			}, "GameUI.PaintSprayerOn", "GameUI.PaintSprayerOff") {
				Texture = builderAcc,
				HoverOutlineTexture = hoverOutlineTexture,
				Frame = spriteFrame.With(3, rowToUse),
				HoverOutlineFrame = value
			});

			list.Add(new CommonBuilderAccToggle((Player player) => player.autoActuator, (Player player) => player.builderAccStatus[3] == 0, delegate (Player player) {
				player.builderAccStatus[3] = ((player.builderAccStatus[3] == 0) ? 1 : 0);
			}, "GameUI.ActuationDeviceOn", "GameUI.ActuationDeviceOff") {
				Texture = builderAcc,
				HoverOutlineTexture = hoverOutlineTexture,
				Frame = spriteFrame.With(2, rowToUse),
				HoverOutlineFrame = value
			});

			list.Add(new WireBuilderAccToggle(CanShowWires, (Player player) => (WireToggleState)player.builderAccStatus[4], delegate (Player player) {
				CycleWireToggleState(player, 4);
			}, "Game.RedWires") {
				Texture = builderAcc,
				Frame = spriteFrame.With(4, rowToUse)
			});

			list.Add(new WireBuilderAccToggle(CanShowWires, (Player player) => (WireToggleState)player.builderAccStatus[5], delegate (Player player) {
				CycleWireToggleState(player, 5);
			}, "Game.BlueWires") {
				Texture = builderAcc,
				Frame = spriteFrame.With(5, rowToUse)
			});

			list.Add(new WireBuilderAccToggle(CanShowWires, (Player player) => (WireToggleState)player.builderAccStatus[6], delegate (Player player) {
				CycleWireToggleState(player, 6);
			}, "Game.GreenWires") {
				Texture = builderAcc,
				Frame = spriteFrame.With(6, rowToUse)
			});

			list.Add(new WireBuilderAccToggle(CanShowWires, (Player player) => (WireToggleState)player.builderAccStatus[7], delegate (Player player) {
				CycleWireToggleState(player, 7);
			}, "Game.YellowWires") {
				Texture = builderAcc,
				Frame = spriteFrame.With(7, rowToUse)
			});

			list.Add(new WireBuilderAccToggle(CanShowWires, (Player player) => (WireToggleState)player.builderAccStatus[9], delegate (Player player) {
				CycleWireToggleState(player, 9);
			}, "Game.Actuators") {
				Texture = builderAcc,
				Frame = spriteFrame.With(9, rowToUse)
			});

			list.Add(new CommonBuilderAccToggle((Player player) => player.InfoAccMechShowWires, (Player player) => player.builderAccStatus[8] == 0, delegate (Player player) {
				player.builderAccStatus[8] = ((player.builderAccStatus[8] == 0) ? 1 : 0);
			}, "GameUI.WireModeForced", "GameUI.WireModeNormal") {
				Texture = builderAcc,
				HoverOutlineTexture = hoverOutlineTexture,
				Frame = spriteFrame.With(8, rowToUse),
				HoverOutlineFrame = value
			});

			toggles.AddRange(list);
		}

		public void Draw(SpriteBatch spriteBatch) {
			Vector2 vector = new Vector2(10f, 54f);
			Vector2 vector2 = new Vector2(0f, 24f);
			if (_toggles.Count == 0)
				LazyInit();

			Player localPlayer = Main.LocalPlayer;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < _toggles.Count; i++) {
				if (_toggles[i].IsAvailableFor(localPlayer))
					num2++;
			}

			if (num2 >= 10)
				vector += new Vector2(0f, -24f);

			for (int j = 0; j < _toggles.Count; j++) {
				IBuilderAccToggle builderAccToggle = _toggles[j];
				if (builderAccToggle.IsAvailableFor(localPlayer)) {
					builderAccToggle.Draw(spriteBatch, vector, localPlayer);
					UILinkPointNavigator.SetPosition(6000 + num, vector);
					num++;
					vector += vector2;
				}
			}
		}
	}
}

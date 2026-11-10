using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

/// <summary>
/// A text panel that supports various states that change colors, sounds, and tooltip text.
/// <br/><br/> Do not set <see cref="UIButton.BackgroundColor"/> or <see cref="UIButton.BorderColor"/> because they will be overriden.
/// </summary>
public class UIButton : UIMarqueeTextPanel
{
	public struct State()
	{
		public Color PanelColor = UICommon.DefaultUIBlueMouseOver;
		public Color BorderColor = UICommon.DefaultUIBorder;

		public Color? HoverPanelColor = null;
		public Color? HoverBorderColor = null;

		public SoundStyle? HoverSound = null;
		public SoundStyle? ClickSound = null;

		public object HoverText = null;
		public bool TooltipText = false;

		public static State Default = new();
		public static State DefaultWithHover = Default with {
			HoverPanelColor = UICommon.DefaultUIBlue,
			HoverBorderColor = UICommon.DefaultUIBorderMouseOver,
			HoverSound = SoundID.MenuTick,
		};
	}

	public Func<string> GetCurrentState;
	public State CurrentState { get; private set; }

	private Dictionary<string, State> _states = [];
	private string _firstState = null;

	public UIButton(object text, float textScaleMax = 1, bool large = false) : base(text, textScaleMax, large)
	{
		GetCurrentState = () => _firstState;
	}

	public void AddState(string name, State state)
	{
		_firstState ??= name;
		_states.Add(name, state);
	}

	private void UpdateState()
	{
		if (_states.Count <= 0)
			throw new InvalidOperationException("No states have been added using AddState()");

		var currentStateName = GetCurrentState();
		CurrentState = _states.TryGetValue(currentStateName, out var state)
			? state
			: throw new InvalidOperationException($"{currentStateName} is not a valid state");

		SetPanelColors();
	}

	private void SetPanelColors()
	{
		BackgroundColor = IsMouseHovering && CurrentState.HoverPanelColor != null
			? CurrentState.HoverPanelColor.Value
			: CurrentState.PanelColor;

		BorderColor = IsMouseHovering && CurrentState.HoverBorderColor != null
			? CurrentState.HoverBorderColor.Value
			: CurrentState.BorderColor;
	}

	public override void OnActivate()
	{
		UpdateState();
	}

	public override void Update(GameTime gameTime)
	{
		UpdateState();
		base.Update(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		SetPanelColors();
		base.Draw(spriteBatch);

		if (!IsMouseHovering)
			return;

		string text = CurrentState.HoverText?.ToString();
		if (string.IsNullOrEmpty(text))
			return;

		if (CurrentState.TooltipText)
			UICommon.TooltipMouseText(text);
		else
			Main.instance.MouseText(text);
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);

		if (CurrentState.HoverSound != null)
			SoundEngine.PlaySound(CurrentState.HoverSound.Value);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);

		if (CurrentState.ClickSound != null)
			SoundEngine.PlaySound(CurrentState.ClickSound.Value);
	}
}
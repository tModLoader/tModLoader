
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIProgressBar : UIPanel
{
	private string _cachedText = "";

	public string DisplayText {
		get => _textPanel?.Text ?? _cachedText;
		set {
			if (_textPanel == null) _cachedText = value;
			else _textPanel.SetText(value ?? _textPanel.Text);
		}
	}

	public float Progress { get; private set; } = 0f;

	private UIAutoScaleTextTextPanel<string> _textPanel;

	public override void OnInitialize()
	{
		_textPanel = new UIAutoScaleTextTextPanel<string>(_cachedText ?? "", 1f, true) {
			Top = { Pixels = 10 },
			HAlign = 0.5f,
			Width = { Percent = 1 },
			Height = { Pixels = 60 },
			DrawPanel = false
		};
		Append(_textPanel);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		// Sometimes the element gets initialized before the cache is set
		if (!string.IsNullOrEmpty(_cachedText) && _textPanel != null) {
			_textPanel.SetText(_cachedText);
			_cachedText = string.Empty;
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle space = GetInnerDimensions();
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)space.X + 10, (int)space.Y + (int)space.Height / 2 + 20, (int)space.Width - 20, 10), new Rectangle(0, 0, 1, 1), new Color(0, 0, 70));
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)space.X + 10, (int)space.Y + (int)space.Height / 2 + 20, (int)((space.Width - 20) * Progress), 10), new Rectangle(0, 0, 1, 1), new Color(200, 200, 70));
	}

	public void UpdateProgress(float value) => Progress = value;
}

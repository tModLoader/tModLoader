using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIExpandablePanel : UIPanel
{
	private bool pendingChanges;
	private bool expanded = false;
	private float defaultHeight = 40;

	private UIHoverImage expandButton;
	protected Asset<Texture2D> CollapsedTexture { get; set; } = UICommon.ButtonCollapsedTexture;
	protected Asset<Texture2D> ExpandedTexture { get; set; } = UICommon.ButtonExpandedTexture;

	public List<UIElement> VisibleWhenExpanded = new();

	public event Action OnExpanded;
	public event Action OnCollapsed;

	public UIExpandablePanel()
	{
		Width.Set(0f, 1f);
		Height.Set(defaultHeight, 0f);

		SetPadding(6);

		expandButton = new UIHoverImage(CollapsedTexture, Language.GetTextValue("tModLoader.ModConfigExpand"));
		expandButton.UseTooltipMouseText = true;
		expandButton.Top.Set(3, 0f); // 10, -25: 4, -52
		expandButton.Left.Set(-25, 1f);
		expandButton.OnLeftClick += (a, b) => {
			expanded = !expanded;
			pendingChanges = true;
		};
		Append(expandButton);
	}

	public void Collapse()
	{
		if (expanded) {
			expanded = false;
			pendingChanges = true;
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!pendingChanges)
			return;
		pendingChanges = false;

		float newHeight = defaultHeight;

		if (expanded) {
			foreach (var item in VisibleWhenExpanded) {
				Append(item);
				var innerDimensions = item.GetInnerDimensions();
				if (innerDimensions.Height > newHeight)
					newHeight = 30 + innerDimensions.Height + PaddingBottom + PaddingTop;
			}
			expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigCollapse");
			expandButton.SetImage(ExpandedTexture);
			OnExpanded?.Invoke();
		}
		else {
			foreach (var item in VisibleWhenExpanded) {
				RemoveChild(item);
			}
			OnCollapsed?.Invoke();
			expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigExpand");
			expandButton.SetImage(CollapsedTexture);
		}

		Height.Set(newHeight, 0f);
	}
}

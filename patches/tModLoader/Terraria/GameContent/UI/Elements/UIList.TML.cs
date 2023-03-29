using System.Collections;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.GameContent.UI.Elements;

public partial class UIList : UIElement, IEnumerable<UIElement>, IEnumerable
{
	public float ViewPosition {
		get => _scrollbar.ViewPosition;
		set => _scrollbar.ViewPosition = value;
	}

	public virtual void AddRange(IEnumerable<UIElement> items)
	{
		foreach (var item in items) {
			_items.Add(item);
			_innerList.Append(item);
		}

		UpdateOrder();
		_innerList.Recalculate();
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		PlayerInput.LockVanillaMouseScroll("ModLoader/UIList");
	}
}
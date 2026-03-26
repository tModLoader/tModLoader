using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.GameContent.UI.Elements;

/// <summary>
/// A scrollable list element. Typically paired with a <see cref="UIScrollbar"/>.
/// <para/> To add elements to the list, use <see cref="Add(UIElement)"/> (or <see cref="AddRange(IEnumerable{UIElement})"/>) rather than <see cref="UIElement.Append(UIElement)"/>.
/// <para/> If the ordering of list elements is inconsistent, either override <see cref="UIElement.CompareTo(object)"/> on the elements of the list or assign a custom sort delegate to <see cref="ManualSortMethod"/>. If elements are added in order, you can use an empty sort method to not do any sorting to preserve the original order: <c>myList.ManualSortMethod = (e) => { };</c>
/// </summary>
public partial class UIList : UIElement, IEnumerable<UIElement>, IEnumerable
{
	public float ViewPosition {
		get => _scrollbar.ViewPosition;
		set => _scrollbar.ViewPosition = value;
	}

	/// <summary>
	/// Similar to <see cref="Add(UIElement)"/>, but adds many elements at once, which is more efficient.
	/// </summary>
	public virtual void AddRange(IEnumerable<UIElement> items)
	{
		_items.AddRange(items);
		foreach (var item in items) {
			_innerList.Append(item);
		}

		UpdateOrder();
		_innerList.Recalculate();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (IsMouseHovering)
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIList");
	}
}
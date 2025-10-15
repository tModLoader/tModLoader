using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Terraria.ModLoader.UI.Elements;

// TODO: wow that's a lot of redundant this.
// TODO: This is an almost exact copy of UIList, the only meaningful difference is RecalculateChildren, but that might change if we implement additional features.
/// <summary>
/// Similar to <see cref="UIList"/> except the elements are arranged in a grid in normal reading order.
/// <para/> <b>UIList docs:</b>
/// <inheritdoc cref="UIList"/>
/// </summary>
public class UIGrid : UIElement, IEnumerable<UIElement>, IEnumerable
{
	public delegate bool ElementSearchMethod(UIElement element);

	private class UIInnerList : UIElement
	{
		public override bool ContainsPoint(Vector2 point) => true;

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			Vector2 position = base.Parent.GetDimensions().Position();
			Vector2 dimensions = new Vector2(base.Parent.GetDimensions().Width, base.Parent.GetDimensions().Height);
			foreach (UIElement element in Elements) {
				Vector2 position2 = element.GetDimensions().Position();
				Vector2 dimensions2 = new Vector2(element.GetDimensions().Width, element.GetDimensions().Height);
				if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
					element.Draw(spriteBatch);
			}
		}

		public override Rectangle GetViewCullingArea() => base.Parent.GetDimensions().ToRectangle();
	}

	public List<UIElement> _items = new List<UIElement>();
	protected UIScrollbar _scrollbar;
	internal UIElement _innerList = new UIInnerList();
	private float _innerListHeight;
	public float ListPadding = 5f;
	public Action<List<UIElement>> ManualSortMethod;

	public int Count => _items.Count;

	// todo, vertical/horizontal orientation, left to right, etc?
	public UIGrid()
	{
		_innerList.OverflowHidden = false;
		_innerList.Width.Set(0f, 1f);
		_innerList.Height.Set(0f, 1f);
		OverflowHidden = true;
		Append(_innerList);
	}

	public float GetTotalHeight() => _innerListHeight;

	/// <inheritdoc cref="UIList.Goto(UIList.ElementSearchMethod, bool)"/>
	public void Goto(ElementSearchMethod searchMethod, bool center = false)
	{
		var innerDimensionHeight = GetInnerDimensions().Height;
		for (int i = 0; i < _items.Count; i++) {
			var item = _items[i];
			if (searchMethod(item)) {
				_scrollbar.ViewPosition = item.Top.Pixels;
				if (center) {
					_scrollbar.ViewPosition = item.Top.Pixels - innerDimensionHeight / 2 + item.GetOuterDimensions().Height / 2;
				}
				return;
			}
		}
	}

	/// <inheritdoc cref="UIList.Add(UIElement)"/>
	public virtual void Add(UIElement item)
	{
		_items.Add(item);
		_innerList.Append(item);
		UpdateOrder();
		_innerList.Recalculate();
	}

	/// <inheritdoc cref="UIList.AddRange(IEnumerable{UIElement})"/>
	public virtual void AddRange(IEnumerable<UIElement> items)
	{
		_items.AddRange(items);
		foreach (var item in items) {
			_innerList.Append(item);
		}

		UpdateOrder();
		_innerList.Recalculate();
	}

	public virtual bool Remove(UIElement item)
	{
		_innerList.RemoveChild(item);
		UpdateOrder();
		return _items.Remove(item);
	}

	public virtual void Clear()
	{
		_innerList.RemoveAllChildren();
		_items.Clear();
	}

	public override void Recalculate()
	{
		base.Recalculate();
		UpdateScrollbar();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (IsMouseHovering)
			PlayerInput.LockVanillaMouseScroll("ModLoader/UIList");
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		base.ScrollWheel(evt);
		if (_scrollbar != null)
			_scrollbar.ViewPosition -= evt.ScrollWheelValue;
	}

	public override void RecalculateChildren()
	{
		float availableWidth = GetInnerDimensions().Width;
		base.RecalculateChildren();
		float top = 0f;
		float left = 0f;
		float maxRowHeight = 0f;
		for (int i = 0; i < _items.Count; i++) {
			var item = _items[i];
			var outerDimensions = item.GetOuterDimensions();
			if (left + outerDimensions.Width > availableWidth && left > 0) {
				top += maxRowHeight + ListPadding;
				left = 0;
				maxRowHeight = 0;
			}
			maxRowHeight = Math.Max(maxRowHeight, outerDimensions.Height);
			item.Left.Set(left, 0f);
			left += outerDimensions.Width + ListPadding;
			item.Top.Set(top, 0f);
		}
		_innerListHeight = top + maxRowHeight;
	}

	private void UpdateScrollbar()
	{
		if (_scrollbar != null) {
			float height = GetInnerDimensions().Height;
			_scrollbar.SetView(height, _innerListHeight);
		}
	}

	public void SetScrollbar(UIScrollbar scrollbar)
	{
		_scrollbar = scrollbar;
		UpdateScrollbar();
	}

	public void UpdateOrder()
	{
		if (ManualSortMethod != null)
			ManualSortMethod(_items);
		else
			_items.Sort(SortMethod);

		UpdateScrollbar();
	}

	public int SortMethod(UIElement item1, UIElement item2) => item1.CompareTo(item2);

	public override List<SnapPoint> GetSnapPoints()
	{
		List<SnapPoint> list = new List<SnapPoint>();
		if (GetSnapPoint(out var point))
			list.Add(point);

		foreach (UIElement item in _items) {
			list.AddRange(item.GetSnapPoints());
		}

		return list;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (_scrollbar != null)
			_innerList.Top.Set(0f - _scrollbar.GetValue(), 0f);

		// Recalculate(); // Might change existing behavior to add this in.
	}

	public IEnumerator<UIElement> GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
}

class NestedUIGrid : UIGrid
{
	public NestedUIGrid()
	{
	}

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		if (this._scrollbar != null)
		{
			float oldpos = this._scrollbar.ViewPosition;
			this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
			if (oldpos == _scrollbar.ViewPosition)
			{
				base.ScrollWheel(evt);
			}
		}
		else
		{
			base.ScrollWheel(evt);
		}
	}
}

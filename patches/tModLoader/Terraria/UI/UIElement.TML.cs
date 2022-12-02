namespace Terraria.UI;

partial class UIElement
{
	public event MouseEvent OnMiddleMouseDown;
	public event MouseEvent OnMiddleMouseUp;
	public event MouseEvent OnMiddleClick;
	public event MouseEvent OnMiddleDoubleClick;
	public event MouseEvent OnXButton1MouseDown;
	public event MouseEvent OnXButton1MouseUp;
	public event MouseEvent OnXButton1Click;
	public event MouseEvent OnXButton1DoubleClick;
	public event MouseEvent OnXButton2MouseDown;
	public event MouseEvent OnXButton2MouseUp;
	public event MouseEvent OnXButton2Click;
	public event MouseEvent OnXButton2DoubleClick;

	public bool HasChild(UIElement child) => Elements.Contains(child);

	public virtual void MiddleMouseDown(UIMouseEvent evt)
	{
		OnMiddleMouseDown?.Invoke(evt, this);
		Parent?.MiddleMouseDown(evt);
	}

	public virtual void MiddleMouseUp(UIMouseEvent evt)
	{
		OnMiddleMouseUp?.Invoke(evt, this);
		Parent?.MiddleMouseUp(evt);
	}

	public virtual void MiddleClick(UIMouseEvent evt)
	{
		OnMiddleClick?.Invoke(evt, this);
		Parent?.MiddleClick(evt);
	}

	public virtual void MiddleDoubleClick(UIMouseEvent evt)
	{
		OnMiddleDoubleClick?.Invoke(evt, this);
		Parent?.MiddleDoubleClick(evt);
	}

	public virtual void XButton1MouseDown(UIMouseEvent evt)
	{
		OnXButton1MouseDown?.Invoke(evt, this);
		Parent?.XButton1MouseDown(evt);
	}

	public virtual void XButton1MouseUp(UIMouseEvent evt)
	{
		OnXButton1MouseUp?.Invoke(evt, this);
		Parent?.XButton1MouseUp(evt);
	}

	public virtual void XButton1Click(UIMouseEvent evt)
	{
		OnXButton1Click?.Invoke(evt, this);
		Parent?.XButton1Click(evt);
	}

	public virtual void XButton1DoubleClick(UIMouseEvent evt)
	{
		OnXButton1DoubleClick?.Invoke(evt, this);
		Parent?.XButton1DoubleClick(evt);
	}

	public virtual void XButton2MouseDown(UIMouseEvent evt)
	{
		OnXButton2MouseDown?.Invoke(evt, this);
		Parent?.XButton2MouseDown(evt);
	}

	public virtual void XButton2MouseUp(UIMouseEvent evt)
	{
		OnXButton2MouseUp?.Invoke(evt, this);
		Parent?.XButton2MouseUp(evt);
	}

	public virtual void XButton2Click(UIMouseEvent evt)
	{
		OnXButton2Click?.Invoke(evt, this);
		Parent?.XButton2Click(evt);
	}

	public virtual void XButton2DoubleClick(UIMouseEvent evt)
	{
		OnXButton2DoubleClick?.Invoke(evt, this);
		Parent?.XButton2DoubleClick(evt);
	}
}

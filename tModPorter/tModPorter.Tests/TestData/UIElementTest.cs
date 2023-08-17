using Terraria.UI;

public class UIElementTest : UIElement
{
	public void M()
	{
		OnMouseDown += (evt, e) => { };
		OnMouseUp += (evt, e) => { };
		OnClick += (evt, e) => { };
		OnDoubleClick += (evt, e) => { };
	}

	public override void MouseDown(UIMouseEvent evt)
	{
		base.MouseDown(evt);
	}

	public override void MouseUp(UIMouseEvent evt)
	{
		base.MouseUp(evt);
	}

	public override void Click(UIMouseEvent evt)
	{
		base.Click(evt);
	}

	public override void DoubleClick(UIMouseEvent evt)
	{
		base.DoubleClick(evt);
	}
}

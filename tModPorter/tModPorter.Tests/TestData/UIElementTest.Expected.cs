using Terraria.UI;

public class UIElementTest : UIElement
{
	public void M()
	{
		OnLeftMouseDown += (evt, e) => { };
		OnLeftMouseUp += (evt, e) => { };
		OnLeftClick += (evt, e) => { };
		OnLeftDoubleClick += (evt, e) => { };
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);
	}

	public override void LeftMouseUp(UIMouseEvent evt)
	{
		base.LeftMouseUp(evt);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);
	}

	public override void LeftDoubleClick(UIMouseEvent evt)
	{
		base.LeftDoubleClick(evt);
	}
}
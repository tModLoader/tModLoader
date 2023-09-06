using System;

namespace Terraria.ModLoader.UI.Config.Elements;
public class UIHeaderElement : UIConfigElement
{
	public string Header;

	private UIHeading _header;

	public override bool FitsType(Type type) => false;

	public override void OnInitialize()
	{
		base.OnInitialize();

		SetPadding(4f);

		DrawLabel = false;
		DrawPanel = false;
		DrawTooltip = false;

		_header = new UIHeading(Header);
		Append(_header);
	}

	public override void Recalculate()
	{
		base.Recalculate();
		_header?.SetText(Header);
	}
}

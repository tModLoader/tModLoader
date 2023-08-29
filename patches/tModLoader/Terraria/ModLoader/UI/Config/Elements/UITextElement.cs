using System;

namespace Terraria.ModLoader.UI.Config.Elements;
public class UITextElement : UIConfigElement
{
	public object Text;
	public object Tooltip;

	public override bool FitsType(Type type) => false;

	public override string GetLabel()
	{
		return Text?.ToString() ?? "";
	}

	public override string GetTooltip()
	{
		return Tooltip?.ToString() ?? "";
	}
}

using System;

namespace Terraria.ModLoader.Config.UI.Elements;
public class UITextElement : UIConfigElement
{
	public object Text;

	public override bool FitsType(Type type) => false;

	public override void CreateUI() { }

	public override void RefreshUI() { }

	public override string GetLabel()
	{
		return Text?.ToString() ?? "";
	}
}

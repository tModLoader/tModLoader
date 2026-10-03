using System;
using System.Collections.Generic;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal class StringOptionElement : RangeElement
{
	private Func<string> getValue;
	private Func<int> getIndex;
	private Action<int> setValue;
	private string[] options;

	public IList<string> StringList { get; set; }

	public override int NumberTicks => options.Length;
	public override float TickIncrement => 1f / (options.Length - 1);

	protected override float Proportion {
		get => getIndex() / (float)(options.Length - 1);
		set => setValue((int)(Math.Round(value * (options.Length - 1))));
	}

	public override void OnBind()
	{
		base.OnBind();

		StringList = (IList<string>)List;

		var optionsAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(MemberInfo, Item, StringList);

		options = optionsAttribute.OptionLabels;

		TextDisplayFunction = () => MemberInfo.Name + ": " + getValue();
		getValue = () => DefaultGetValue();
		getIndex = () => DefaultGetIndex();
		setValue = (int value) => DefaultSetValue(value);

		if (StringList != null) {
			getValue = () => this.StringList[Index];
			setValue = (int value) => { StringList[Index] = options[value]; Interface.modConfig.SetPendingChanges(); };
			TextDisplayFunction = () => Index + 1 + ": " + StringList[Index];
		}

		if (Label != null) {
			TextDisplayFunction = () => Label + ": " + getValue();
		}
	}

	private void DefaultSetValue(int index)
	{
		if (!MemberInfo.CanWrite)
			return;

		MemberInfo.SetValue(Item, options[index]);
		Interface.modConfig.SetPendingChanges();
	}

	private string DefaultGetValue()
	{
		return (string)MemberInfo.GetValue(Item);
	}

	private int DefaultGetIndex()
	{
		return Array.IndexOf(options, getValue());
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	class StringOptionElement : RangeElement
	{
		private Func<string> _GetValue;
		private Func<int> _GetIndex;
		private Action<int> _SetValue;
		string[] options;
		public IList<string> stringList;

		public override int NumberTicks => options.Length;
		public override float TickIncrement => 1f / (options.Length - 1);

		protected override float Proportion {
			get => _GetIndex() / (float)(options.Length - 1);
			set => _SetValue((int)(Math.Round(value * (options.Length - 1))));
		}

		public override void OnBind() {
			base.OnBind();
			stringList = (IList<string>)list;
			OptionStringsAttribute optionsAttribute = ConfigManager.GetCustomAttribute<OptionStringsAttribute>(memberInfo, item, stringList);
			options = optionsAttribute.optionLabels;

			TextDisplayFunction = () => memberInfo.Name + ": " + _GetValue();
			_GetValue = () => DefaultGetValue();
			_GetIndex = () => DefaultGetIndex();
			_SetValue = (int value) => DefaultSetValue(value);

			if (stringList != null)
			{
				_GetValue = () => this.stringList[index];
				_SetValue = (int value) => { stringList[index] = options[value]; Interface.modConfig.SetPendingChanges(); };
				TextDisplayFunction = () => index + 1 + ": " + stringList[index];
			}

			if (labelAttribute != null)
			{
				TextDisplayFunction = () => labelAttribute.Label + ": " + _GetValue();
			}
		}

		void DefaultSetValue(int index)
		{
			if (!memberInfo.CanWrite) return;
			memberInfo.SetValue(item, options[index]);
			Interface.modConfig.SetPendingChanges();
		}

		string DefaultGetValue()
		{
			return (string)memberInfo.GetValue(item);
		}

		int DefaultGetIndex()
		{
			return Array.IndexOf(options, _GetValue());
		}
	}
}

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Graphics;

namespace Terraria.ModLoader.Config.UI
{
	internal abstract class ISetElementWrapper
	{
		internal virtual object Value => null;
	}

	internal class SetElementWrapper<V> : ISetElementWrapper
	{
		private object set;

		private V _value;
		public V value
		{
			get { return _value; }
			set
			{
				var removeMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove");
				var addMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");
				var containsMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Contains");
				if ((bool)containsMethod.Invoke(set, new object[] { value }))
				{
				}
				else
				{
					removeMethod.Invoke(set, new object[] { _value });
					_value = value;
					addMethod.Invoke(set, new object[] { _value });
				}

				//if (set.Contains(value))
				//{

				//}
				//else
				//{
				//	set.Remove(_value);
				//	_value = value;
				//	set.Add(_value, _value);
				//}
			}
		}
		internal override object Value => value;

		public SetElementWrapper(V value, object set)
		{
			this.set = set;
			this._value = value;
		}
	}

	internal class SetElement : ConfigElement
	{
		private object data;
		private Type setType;
		private int sliderIDStart;
		private NestedUIList dataList;

		public List<ISetElementWrapper> dataWrapperList;

		float scale = 1f;

		public SetElement(PropertyFieldWrapper memberInfo, object item, ref int sliderIDInPage) : base(memberInfo, item, null)
		{
			MaxHeight.Set(300, 0f);

			drawLabel = false;

			sliderIDStart = sliderIDInPage;
			sliderIDInPage += 10000;

			string name = memberInfo.Name;
			if (labelAttribute != null)
			{
				name = labelAttribute.Label;
			}

			UISortableElement sortedContainer = new UISortableElement(-1);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.HAlign = 0.5f;
			var text = new UIText(name);
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			sortedContainer.Append(text);
			Append(sortedContainer);
			//sortedContainer.OverflowHidden = true;

			UIPanel panel = new UIPanel();
			panel.Width.Set(-20f, 1f);
			panel.Left.Set(20f, 0f);
			panel.Top.Set(30f, 0f);
			panel.Height.Set(-60, 1f);
			Append(panel);
			//panel.OverflowHidden = true;

			dataList = new NestedUIList();
			dataList.Width.Set(-20, 1f);
			dataList.Left.Set(0, 0f);
			dataList.Height.Set(0, 1f);
			dataList.ListPadding = 5f;
			panel.Append(dataList);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0f, 1f);
			scrollbar.Top.Set(0f, 0f);
			scrollbar.Left.Pixels += 8;
			scrollbar.HAlign = 1f;
			dataList.SetScrollbar(scrollbar);
			panel.Append(scrollbar);

			setType = memberInfo.Type.GetGenericArguments()[0];
			data = memberInfo.GetValue(item);
			SetupList();

			sortedContainer = new UISortableElement(int.MaxValue);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.Top.Set(-30f, 1f);
			sortedContainer.HAlign = 0.5f;
			text = new UIText("Click To Add");
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			text.OnClick += (a, b) =>
			{
				Main.PlaySound(21);

				var addMethod = data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");

				DefaultListValueAttribute defaultListValueAttribute = ConfigManager.GetCustomAttribute<DefaultListValueAttribute>(memberInfo, null, null);
				if (defaultListValueAttribute != null)
				{
					//((SortedSet)data).Add(defaultListValueAttribute.defaultValue);
					addMethod.Invoke(data, new object[] { defaultListValueAttribute.defaultValue });
				}
				else
				{
					//((IList)data).Add(ConfigManager.AlternateCreateInstance(listType));
					addMethod.Invoke(data, new object[] { ConfigManager.AlternateCreateInstance(setType) });
				}

				//try
				//{
				//	((IDictionary)data).Add(ConfigManager.AlternateCreateInstance(keyType), ConfigManager.AlternateCreateInstance(valueType));
				//}
				//catch (Exception e)
				//{
				//	Interface.modConfig.SetMessage("Error: " + e.Message, Color.Red);
				//}

				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};
			sortedContainer.Append(text);
			Append(sortedContainer);

			UIImageButton upButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png")));
			upButton.Top.Set(40, 0f);
			upButton.Left.Set(0, 0f);
			upButton.OnClick += (a, b) =>
			{
				scale = Math.Min(2f, scale + 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement)
				{
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(upButton);

			UIImageButton downButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png")));
			downButton.Top.Set(52, 0f);
			downButton.Left.Set(0, 0f);
			downButton.OnClick += (a, b) =>
			{
				scale = Math.Max(1f, scale - 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement)
				{
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(downButton);
		}

		private void SetupList()
		{
			Type itemType = memberInfo.Type.GetGenericArguments()[0];
			int sliderID = sliderIDStart;
			dataList.Clear();
			var deleteButtonTexture = TextureManager.Load("Images/UI/ButtonDelete");
			int top = 0;
			dataWrapperList = new List<ISetElementWrapper>();

			Type genericType = typeof(SetElementWrapper<>).MakeGenericType(itemType);

			var valuesEnumerator = ((IEnumerable)data).GetEnumerator();

			int i = 0;
			while (valuesEnumerator.MoveNext())
			{
				ISetElementWrapper proxy = (ISetElementWrapper)Activator.CreateInstance(genericType,
					new object[] { valuesEnumerator.Current, (object)data });
				dataWrapperList.Add(proxy);

				var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList()[0];
				int index = i;
				var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, this, ref sliderID, dataWrapperList, genericType, i);
				wrapped.Item2.Left.Pixels += 24;
				wrapped.Item2.Width.Pixels -= 24;

				// Add delete button.
				UIImageButton deleteButton = new UIImageButton(deleteButtonTexture);
				deleteButton.VAlign = 0.5f;

				// fix delete.
				object o = valuesEnumerator.Current; // needed for closure?
				deleteButton.OnClick += (a, b) =>
				{
					var removeMethod = data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove");
					removeMethod.Invoke(data, new object[] { o });
					//((IDictionary)data).Remove(o);
					SetupList();
					Interface.modConfig.SetPendingChanges();
				};
				wrapped.Item1.Append(deleteButton);

				i++;
			}
			dataList.RecalculateChildren();
			float h = dataList.GetTotalHeight();
			MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
			Recalculate();
			if (Parent != null && Parent is UISortableElement)
			{
				Parent.Height.Pixels = GetOuterDimensions().Height;
			}
		}

		public override void Recalculate()
		{
			base.Recalculate();
			float h = dataList.GetTotalHeight() * scale;
			Height.Set(h, 0f);
		}
	}
}

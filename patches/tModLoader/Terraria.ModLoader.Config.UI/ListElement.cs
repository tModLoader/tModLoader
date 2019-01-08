using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class ListElement : ConfigElement
	{
		// does not apply?
		//public override int NumberTicks => 0;
		//public override float TickIncrement => 0;

		private object data;
		private List<object> dataAsList;
		private Type listType;
		//private int sliderIDStart;
		private NestedUIList dataList;

		float scale = 1f;

		public ListElement(PropertyFieldWrapper memberInfo, object item) : base(memberInfo, item, null)
		{
			MaxHeight.Set(300, 0f);

			drawLabel = false;

			//sliderIDStart = sliderIDInPage;
			//sliderIDInPage += 10000;

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

			listType = memberInfo.Type.GetGenericArguments()[0];
			data = memberInfo.GetValue(item);

			// allow null collections to simplify modder code for OnDeserialize and allow null and empty lists to have different meanings, etc.
			SetupList();

			sortedContainer = new UISortableElement(int.MaxValue);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.Top.Set(-30f, 1f);
			sortedContainer.HAlign = 0.5f;
			text = new UIText(data == null ? "Click To Initialize" : "Click To Add");
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			text.OnClick += (a, b) => {
				Main.PlaySound(21);
				if (data == null) {
					data = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
					memberInfo.SetValue(item, data);
					text.SetText("Click To Add");
				}
				else {
					DefaultListValueAttribute defaultListValueAttribute = ConfigManager.GetCustomAttribute<DefaultListValueAttribute>(memberInfo, null, null);
					if (defaultListValueAttribute != null) {
						((IList)data).Add(defaultListValueAttribute.defaultValue);
					}
					else {
						((IList)data).Add(ConfigManager.AlternateCreateInstance(listType));
					}
				}

				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};
			sortedContainer.Append(text);
			Append(sortedContainer);

			UIImageButton upButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png")));
			upButton.Top.Set(40, 0f);
			upButton.Left.Set(0, 0f);
			upButton.OnClick += (a, b) => {
				scale = Math.Min(2f, scale + 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement) {
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(upButton);

			UIImageButton downButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png")));

			//var aasdf = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));

			//for (int i = 0; i < 100; i++) {
			//	var vb = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
			//}


			downButton.Top.Set(52, 0f);
			downButton.Left.Set(0, 0f);
			downButton.OnClick += (a, b) => {
				scale = Math.Max(1f, scale - 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement) {
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(downButton);
			//}
		}

		private void SetupList()
		{
			Type itemType = memberInfo.Type.GetGenericArguments()[0];
			//int sliderID = sliderIDStart;
			dataList.Clear();
			var deleteButtonTexture = TextureManager.Load("Images/UI/ButtonDelete");
			int top = 0;
			if (data != null) {
				for (int i = 0; i < ((IList)data).Count; i++) {
					int index = i;
					var wrapped = UIModConfig.WrapIt(dataList, ref top, memberInfo, item, 0, data, itemType, index);
					// Add delete button.

					wrapped.Item2.Left.Pixels += 24;
					wrapped.Item2.Width.Pixels -= 24;

					UIImageButton deleteButton = new UIImageButton(deleteButtonTexture);
					deleteButton.VAlign = 0.5f;
					deleteButton.OnClick += (a, b) => { ((IList)data).RemoveAt(index); SetupList(); Interface.modConfig.SetPendingChanges(); };
					wrapped.Item1.Append(deleteButton);

					/*var sortedContainer = new UISortableElement(i);
					sortedContainer.Width.Set(0f, 1f);
					sortedContainer.Height.Set(30f, 0f);
					sortedContainer.HAlign = 0.5f;

					if (itemType == typeof(int))
					{

						UIImageButton deleteButton = new UIImageButton(deleteButtonTexture);
						deleteButton.VAlign = 0.5f;
						deleteButton.OnClick += (a, b) => { dataAsList.RemoveAt(index); SetupList(); Interface.modConfig.SetPendingChanges(); };
						sortedContainer.Append(deleteButton);

						UIElement e = new UIModConfigIntItem(variable, modConfig, sliderID++, data, index);

						//Func<int> _GetValue = () => data[index];
						//Action<int> _SetValue = (int value) => { data[index] = value; Interface.modConfig.SetPendingChanges(); };
						//Func<string> Text = () => index + 1 + ": " + data[index];
						//UIElement e = new UIModConfigIntItem(_GetValue, _SetValue, Text, sliderID++);
						e.Width.Pixels = -24;
						e.Left.Pixels = 24;
						//UIElement e = new UIText("" + intlist[i]);
						sortedContainer.Append(e);
					}
					else
					{
						var e = new UIText($"{variable.Name} not handled yet ({itemType.Name})");
						e.Top.Pixels += 6;
						e.Left.Pixels += 4;
						sortedContainer.Append(e);
					}
					dataList.Add(sortedContainer);*/
				}
			}
			else {

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

		//protected override void DrawSelf(SpriteBatch spriteBatch)
		//{
		//	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Orange * 0.6f);
		//	base.DrawSelf(spriteBatch);
		//}
	}

	class NestedUIList : UIList
	{
		public NestedUIList()
		{
			//OverflowHidden = false;
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
}

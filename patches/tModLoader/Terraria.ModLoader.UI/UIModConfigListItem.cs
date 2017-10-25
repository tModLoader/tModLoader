using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;
using System.Collections.Generic;

namespace Terraria.ModLoader.UI
{
	internal class UIModConfigListItem : UIElement
	{
		private PropertyFieldWrapper variable;
		private ModConfig modConfig;
		private List<int> data;
		private int sliderIDStart;
		private NestedUIList dataList;

		public UIModConfigListItem(PropertyFieldWrapper variable, ModConfig modConfig, ref int sliderIDInPage)
		{
			sliderIDStart = sliderIDInPage;
			sliderIDInPage += 10000;

			this.variable = variable;
			this.modConfig = modConfig;
			Width.Set(0f, 1f);
			Height.Set(0f, 1f);

			string name = variable.Name;
			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(variable.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				name = att.Label;
			}
			//OverflowHidden = true;

			UISortableElement sortedContainer = new UISortableElement(-1);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.HAlign = 0.5f;
			sortedContainer.Append(new UIText(name));
			Append(sortedContainer);
			//sortedContainer.OverflowHidden = true;

			UIPanel panel = new UIPanel();
			panel.Width.Set(-25f, 1f);
			panel.Left.Set(25f, 0f);
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
			scrollbar.HAlign = 1f;
			dataList.SetScrollbar(scrollbar);
			panel.Append(scrollbar);
			
			data = (List<int>)variable.GetValue(modConfig);
			SetupList();

			sortedContainer = new UISortableElement(int.MaxValue);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.Top.Set(-30f, 1f);
			sortedContainer.HAlign = 0.5f;
			var text = new UIText("Click To Add");
			text.OnClick += (a, b) =>
			{
				Main.PlaySound(21);
				data.Add(0);
				SetupList();
				Interface.modConfig.SetPendingChanges();
			};
			sortedContainer.Append(text);
			Append(sortedContainer);
		}

		private void SetupList()
		{
			Type itemType = variable.Type.GetGenericArguments()[0];
			int sliderID = sliderIDStart;
			dataList.Clear();
			for (int i = 0; i < data.Count; i++)
			{
				var sortedContainer = new UISortableElement(i);
				sortedContainer.Width.Set(0f, 1f);
				sortedContainer.Height.Set(30f, 0f);
				sortedContainer.HAlign = 0.5f;

				if (itemType == typeof(int))
				{
					int index = i;
					Func<int> _GetValue = () => data[index];
					Action<int> _SetValue = (int value) => { data[index] = value; Interface.modConfig.SetPendingChanges(); };
					Func<string> Text = () => index+1 + ": " + data[index];
					UIElement e = new UIModConfigIntItem(_GetValue, _SetValue, Text, sliderID++);
					//UIElement e = new UIText("" + intlist[i]);
					sortedContainer.Append(e);
				}
				else
				{
					sortedContainer.Append(new UIText($"{variable.Name} not handled yet ({itemType.Name})"));
				}
				dataList.Add(sortedContainer);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Red * 0.6f);
			base.DrawSelf(spriteBatch);
		}
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
		}
	}
}

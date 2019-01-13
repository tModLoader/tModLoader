using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	class ObjectElement : ConfigElement
	{
		private Func<object> _GetValue;
		private Action<object> _SetValue;
		protected Func<string> AbridgedTextDisplayFunction;

		//SeparatePageAttribute separatePageAttribute;
		bool separatePage;
		bool ignoreSeparatePage;

		private NestedUIList dataList;
		//private object data;
		UIModConfigHoverImage initializeButton;
		UIModConfigHoverImage deleteButton;
		UIModConfigHoverImage expandButton;
		UIPanel separatePagePanel;
		UITextPanel<FuncStringWrapper> separatePageButton;
		bool expanded = true;
		int index;

		bool AllowNull => array == null; // nulls don't make sense for a collection, but a standalone might be useful. NonNull attribute might be nice.

		// Label:
		//  Members
		//  Members
		public ObjectElement(PropertyFieldWrapper memberInfo, object item, IList array = null, int index = -1, bool ignoreSeparatePage = false) : base(memberInfo, item, array)
		{
			this.index = index;
			this.ignoreSeparatePage = ignoreSeparatePage;
			_GetValue = () => memberInfo.GetValue(this.item);
			_SetValue = (object value) => {
				if (!memberInfo.CanWrite) return;
				memberInfo.SetValue(this.item, value);
			};

			if (array != null) {
				_GetValue = () => array[index];
				_SetValue = (object value) => { array[index] = value; Interface.modConfig.SetPendingChanges(); };
				// TODO: only do this if ToString is overriden. 

				var listType = memberInfo.Type.GetGenericArguments()[0];
				bool hasToString = listType.GetMethod("ToString", new Type[0]).DeclaringType != typeof(object);

				if (hasToString) {
					_TextDisplayFunction = () => index + 1 + ": " + (array[index]?.ToString() ?? "null");
					AbridgedTextDisplayFunction = () => (array[index]?.ToString() ?? "null");
				}
				else {
					_TextDisplayFunction = () => index + 1 + ": ";
				}
			}
			else {
				bool hasToString = memberInfo.Type.GetMethod("ToString", new Type[0]).DeclaringType != typeof(object);
				if (hasToString) {
					_TextDisplayFunction = () => (labelAttribute == null ? memberInfo.Name : labelAttribute.Label) + (_GetValue() == null ? "" : ": " + _GetValue().ToString());
					AbridgedTextDisplayFunction = () => _GetValue()?.ToString() ?? "";
				}
			}

			if(_GetValue() == null && !AllowNull) {
				object data = Activator.CreateInstance(memberInfo.Type);
				JsonConvert.PopulateObject("{}", data, ConfigManager.serializerSettings);
				//JsonDefaultValueAttribute jsonDefaultValueAttribute = (JsonDefaultValueAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(JsonDefaultValueAttribute));
				//if (jsonDefaultValueAttribute != null)
				//{
				//	JsonConvert.PopulateObject(jsonDefaultValueAttribute.json, subitem, ConfigManager.serializerSettings);
				//}
				_SetValue(data);
			}

			separatePage = ConfigManager.GetCustomAttribute<SeparatePageAttribute>(memberInfo, item, array) != null;
			//separatePage = separatePage && !ignoreSeparatePage;
			//separatePage = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute)) != null;
			if (separatePage && !ignoreSeparatePage) {
				// TODO: UITextPanel doesn't update...
				separatePageButton = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper() { func = _TextDisplayFunction });
				separatePageButton.HAlign = 0.5f;
				//e.Recalculate();
				//elementHeight = (int)e.GetOuterDimensions().Height;
				separatePageButton.OnClick += (a, c) => {
					UIModConfig.SwitchToSubConfig(this.separatePagePanel);
					/*	Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
						Interface.modConfig.uIElement.Append(separateListPanel);
						Interface.modConfig.configPanelStack.Push(separateListPanel);*/
					//separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

					//UIPanel panel = new UIPanel();
					//panel.Width.Set(200, 0);
					//panel.Height.Set(200, 0);
					//panel.Left.Set(200, 0);
					//panel.Top.Set(200, 0);
					//Interface.modConfig.Append(panel);

					//Interface.modConfig.subMenu.Enqueue(subitem);
					//Interface.modConfig.DoMenuModeState();
				};
				//e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
				//e.OnClick += (a, b) => { };
			}

			//data = _GetValue();// memberInfo.GetValue(this.item);
			//drawLabel = false;

			dataList = new NestedUIList();
			dataList.Width.Set(-14, 1f);
			dataList.Left.Set(14, 0f);
			dataList.Height.Set(-30, 1f);
			dataList.Top.Set(30, 0);
			dataList.ListPadding = 5f;
			Append(dataList);

			//string name = memberInfo.Name;
			//if (labelAttribute != null)
			//{
			//	name = labelAttribute.Label;
			//}
			if (array == null)
			{
				// drawLabel = false; TODO uncomment
			}

			initializeButton = new UIModConfigHoverImage(playTexture, "Initialize");
			initializeButton.Top.Pixels += 4;
			initializeButton.Left.Pixels -= 3;
			initializeButton.HAlign = 1f;
			initializeButton.OnClick += (a, b) => {
				Main.PlaySound(21);
				object data = Activator.CreateInstance(memberInfo.Type);
				// Crashes JSONItem
				JsonConvert.PopulateObject("{}", data, ConfigManager.serializerSettings); // Seems to fail on all data structures?

				//JsonDefaultValueAttribute jsonDefaultValueAttribute = (JsonDefaultValueAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(JsonDefaultValueAttribute));
				//if (jsonDefaultValueAttribute != null)
				//{
				//	JsonConvert.PopulateObject(jsonDefaultValueAttribute.json, subitem, ConfigManager.serializerSettings);
				//}

				_SetValue(data);

				//SeparatePageAttribute here?

				pendingChanges = true;
				//RemoveChild(initializeButton);
				//Append(deleteButton);
				//Append(expandButton);

				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};

			expandButton = new UIModConfigHoverImage(expandedTexture, "Expand");
			expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
			expandButton.Left.Set(-52, 1f);
			expandButton.OnClick += (a, b) => {
				expanded = !expanded;
				pendingChanges = true;
			};

			deleteButton = new UIModConfigHoverImage(deleteTexture, "Clear");
			deleteButton.Top.Set(4, 0f);
			deleteButton.Left.Set(-25, 1f);
			deleteButton.OnClick += (a, b) => { 
				_SetValue(null);
				pendingChanges = true;

				SetupList();
				//Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};

			if (_GetValue() != null) {
				//Append(expandButton);
				//Append(deleteButton);
				SetupList();
			}
			else {
				Append(initializeButton);
				//sortedContainer.Append(initializeButton);
			}

			pendingChanges = true;
			Recalculate();
		}

		bool pendingChanges = false;
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (!pendingChanges)
				return;
			pendingChanges = false;
			drawLabel = !separatePage || ignoreSeparatePage;

			RemoveChild(deleteButton);
			RemoveChild(expandButton);
			RemoveChild(initializeButton);
			RemoveChild(dataList);
			if (separatePage && !ignoreSeparatePage)
				RemoveChild(separatePageButton);
			if (_GetValue() == null) {
				Append(initializeButton);
				drawLabel = true;
			}
			else {
				if(AllowNull && !(separatePage && ignoreSeparatePage))
					Append(deleteButton);
				if (!separatePage || ignoreSeparatePage) {
					if (!ignoreSeparatePage)
						Append(expandButton);
					if (expanded) {
						Append(dataList);
						expandButton.HoverText = "Collapse";
						expandButton.SetImage(expandedTexture);
					}
					else {
						RemoveChild(dataList);
						expandButton.HoverText = "Expand";
						expandButton.SetImage(collapsedTexture);
					}
				}
				else {
					Append(separatePageButton);
				}
			}
		}

		private void SetupList() {
			dataList.Clear();

			object data = _GetValue();
			if (data != null) {
				if (separatePage && !ignoreSeparatePage) {
					separatePagePanel = UIModConfig.MakeSeparateListPanel(item, data, memberInfo, array, index, AbridgedTextDisplayFunction);
				}
				else {
					int order = 0;
					foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(data)) {
						if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
							continue;

						int top = 0;
						var wrapped = UIModConfig.WrapIt(dataList, ref top, variable, data, order++);
						if (array != null) {
							//wrapped.Item1.Left.Pixels -= 20;
							wrapped.Item1.Width.Pixels += 20;
						}
						else {
							//wrapped.Item1.Left.Pixels += 20;
							//wrapped.Item1.Width.Pixels -= 20;
						}
					}
				}
			}
		}

		public override void Recalculate()
		{
			base.Recalculate();
			float defaultHeight = separatePage ? 40 : 30;
			float h = dataList.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight;
			Height.Set(h, 0f);
			if (Parent != null && Parent is UISortableElement)
			{
				Parent.Height.Set(h, 0f);
			}
		}
	}

	internal class UIModConfigHoverImage : UIImage
	{
		internal string HoverText;

		public UIModConfigHoverImage(Texture2D texture, string hoverText) : base(texture) {
			HoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering) {
				UIModConfig.tooltip = HoverText;
			}
		}
	}

	internal class UIModConfigHoverImageSplit : UIImage
	{
		internal string HoverTextUp;
		internal string HoverTextDown;

		public UIModConfigHoverImageSplit(Texture2D texture, string hoverTextUp, string hoverTextDown) : base(texture) {
			HoverTextUp = hoverTextUp;
			HoverTextDown = hoverTextDown;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle r = GetDimensions().ToRectangle();
			if (IsMouseHovering) {
				if (Main.mouseY < r.Y + r.Height / 2) {
					UIModConfig.tooltip = HoverTextUp;
				}
				else {
					UIModConfig.tooltip = HoverTextDown;
				}
			}
		}
	}

	class FuncStringWrapper {
		public Func<string> func;
		public override string ToString() {
			return func();
		}
	}
}

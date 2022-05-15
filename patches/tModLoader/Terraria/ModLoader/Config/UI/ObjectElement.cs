using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class ObjectElement : ConfigElement<object>
	{
		protected Func<string> AbridgedTextDisplayFunction { get; set; }

		private readonly bool ignoreSeparatePage;
		//private SeparatePageAttribute separatePageAttribute;
		//private object data;
		private bool separatePage;
		private bool pendingChanges;
		private bool expanded = true;
		private NestedUIList dataList;
		private UIModConfigHoverImage initializeButton;
		private UIModConfigHoverImage deleteButton;
		private UIModConfigHoverImage expandButton;
		private UIPanel separatePagePanel;
		private UITextPanel<FuncStringWrapper> separatePageButton;

		// Label:
		//  Members
		//  Members
		public ObjectElement(bool ignoreSeparatePage = false) {
			this.ignoreSeparatePage = ignoreSeparatePage;
		}

		public override void OnBind() {
			base.OnBind();

			if (List != null) {
				// TODO: only do this if ToString is overriden.

				var listType = MemberInfo.Type.GetGenericArguments()[0];

				System.Reflection.MethodInfo methodInfo = listType.GetMethod("ToString", Array.Empty<Type>());
				bool hasToString = methodInfo != null && methodInfo.DeclaringType != typeof(object);

				if (hasToString) {
					TextDisplayFunction = () => Index + 1 + ": " + (List[Index]?.ToString() ?? "null");
					AbridgedTextDisplayFunction = () => (List[Index]?.ToString() ?? "null");
				}
				else {
					TextDisplayFunction = () => Index + 1 + ": ";
				}
			}
			else {
				bool hasToString = MemberInfo.Type.GetMethod("ToString", Array.Empty<Type>()).DeclaringType != typeof(object);

				if (hasToString) {
					TextDisplayFunction = () => (LabelAttribute == null ? MemberInfo.Name : LabelAttribute.Label) + (Value == null ? "" : ": " + Value.ToString());
					AbridgedTextDisplayFunction = () => Value?.ToString() ?? "";
				}
			}

			// Null values without AllowNullAttribute aren't allowed, but could happen with modder mistakes, so not automatically populating will hint to modder the issue.
			if (Value == null && List != null) {
				// This should never actually happen, but I guess a bad Json file could.
				object data = Activator.CreateInstance(MemberInfo.Type);
				string json = JsonDefaultValueAttribute?.Json ?? "{}";

				JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

				Value = data;
			}

			separatePage = ConfigManager.GetCustomAttribute<SeparatePageAttribute>(MemberInfo, Item, List) != null;

			//separatePage = separatePage && !ignoreSeparatePage;
			//separatePage = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute)) != null;

			if (separatePage && !ignoreSeparatePage) {
				// TODO: UITextPanel doesn't update...
				separatePageButton = new UITextPanel<FuncStringWrapper>(new FuncStringWrapper(TextDisplayFunction));
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
			//if (labelAttribute != null) {
			//	name = labelAttribute.Label;
			//}
			if (List == null) {
				// drawLabel = false; TODO uncomment
			}

			initializeButton = new UIModConfigHoverImage(PlayTexture, "Initialize");
			initializeButton.Top.Pixels += 4;
			initializeButton.Left.Pixels -= 3;
			initializeButton.HAlign = 1f;
			initializeButton.OnClick += (a, b) => {
				SoundEngine.PlaySound(21);

				object data = Activator.CreateInstance(MemberInfo.Type);
				string json = JsonDefaultValueAttribute?.Json ?? "{}";

				JsonConvert.PopulateObject(json, data, ConfigManager.serializerSettings);

				Value = data;

				//SeparatePageAttribute here?

				pendingChanges = true;
				//RemoveChild(initializeButton);
				//Append(deleteButton);
				//Append(expandButton);

				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};

			expandButton = new UIModConfigHoverImage(ExpandedTexture, "Expand");
			expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
			expandButton.Left.Set(-52, 1f);
			expandButton.OnClick += (a, b) => {
				expanded = !expanded;
				pendingChanges = true;
			};

			deleteButton = new UIModConfigHoverImage(DeleteTexture, "Clear");
			deleteButton.Top.Set(4, 0f);
			deleteButton.Left.Set(-25, 1f);
			deleteButton.OnClick += (a, b) => {
				Value = null;
				pendingChanges = true;

				SetupList();
				//Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
			};

			if (Value != null) {
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

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (!pendingChanges)
				return;
			pendingChanges = false;
			DrawLabel = !separatePage || ignoreSeparatePage;

			RemoveChild(deleteButton);
			RemoveChild(expandButton);
			RemoveChild(initializeButton);
			RemoveChild(dataList);
			if (separatePage && !ignoreSeparatePage)
				RemoveChild(separatePageButton);
			if (Value == null) {
				Append(initializeButton);
				DrawLabel = true;
			}
			else {
				if (List == null && !(separatePage && ignoreSeparatePage) && NullAllowed)
					Append(deleteButton);

				if (!separatePage || ignoreSeparatePage) {
					if (!ignoreSeparatePage)
						Append(expandButton);
					if (expanded) {
						Append(dataList);
						expandButton.HoverText = "Collapse";
						expandButton.SetImage(ExpandedTexture);
					}
					else {
						RemoveChild(dataList);
						expandButton.HoverText = "Expand";
						expandButton.SetImage(CollapsedTexture);
					}
				}
				else {
					Append(separatePageButton);
				}
			}
		}

		private void SetupList() {
			dataList.Clear();

			object data = Value;

			if (data != null) {
				if (separatePage && !ignoreSeparatePage) {
					separatePagePanel = UIModConfig.MakeSeparateListPanel(Item, data, MemberInfo, List, Index, AbridgedTextDisplayFunction);
				}
				else {
					int order = 0;
					foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(data)) {
						if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
							continue;

						int top = 0;
						var header = ConfigManager.GetCustomAttribute<HeaderAttribute>(variable, null, null);

						if (header != null) {
							var wrapper = new PropertyFieldWrapper(typeof(HeaderAttribute).GetProperty(nameof(HeaderAttribute.Header)));
							UIModConfig.WrapIt(dataList, ref top, wrapper, header, order++);
						}

						var wrapped = UIModConfig.WrapIt(dataList, ref top, variable, data, order++);

						if (List != null) {
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

		public override void Recalculate() {
			base.Recalculate();

			float defaultHeight = separatePage ? 40 : 30;
			float h = dataList.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight;

			Height.Set(h, 0f);

			if (Parent != null && Parent is UISortableElement) {
				Parent.Height.Set(h, 0f);
			}
		}
	}

	internal class UIModConfigHoverImage : UIImage
	{
		internal string HoverText;

		public UIModConfigHoverImage(Asset<Texture2D> texture, string hoverText) : base(texture) {
			HoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering) {
				UIModConfig.Tooltip = HoverText;
			}
		}
	}

	internal class UIModConfigHoverImageSplit : UIImage
	{
		internal string HoverTextUp;
		internal string HoverTextDown;

		public UIModConfigHoverImageSplit(Asset<Texture2D> texture, string hoverTextUp, string hoverTextDown) : base(texture) {
			HoverTextUp = hoverTextUp;
			HoverTextDown = hoverTextDown;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			Rectangle r = GetDimensions().ToRectangle();

			if (IsMouseHovering) {
				if (Main.mouseY < r.Y + r.Height / 2) {
					UIModConfig.Tooltip = HoverTextUp;
				}
				else {
					UIModConfig.Tooltip = HoverTextDown;
				}
			}
		}
	}

	internal class FuncStringWrapper
	{
		public Func<string> Func { get; }

		public FuncStringWrapper(Func<string> func) {
			Func = func;
		}

		public override string ToString() {
			return Func();
		}
	}
}

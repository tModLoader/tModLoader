using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	abstract class CollectionElement : ConfigElement
	{
		protected object data;
		protected UIElement dataListElement;
		protected NestedUIList dataList;
		protected float scale = 1f;
		protected DefaultListValueAttribute defaultListValueAttribute;
		protected JsonDefaultListValueAttribute jsonDefaultListValueAttribute;

		UIModConfigHoverImage initializeButton;
		UIModConfigHoverImage addButton;
		UIModConfigHoverImage deleteButton;
		UIModConfigHoverImage expandButton;
		UIModConfigHoverImageSplit upDownButton;
		bool expanded = true;

		public override void OnBind() {
			base.OnBind();
			data = memberInfo.GetValue(item);
			defaultListValueAttribute = ConfigManager.GetCustomAttribute<DefaultListValueAttribute>(memberInfo, null, null);

			MaxHeight.Set(300, 0f);
			dataListElement = new UIElement();
			dataListElement.Width.Set(-10f, 1f);
			dataListElement.Left.Set(10f, 0f);
			dataListElement.Height.Set(-30, 1f);
			dataListElement.Top.Set(30f, 0f);
			//panel.SetPadding(0);
			//panel.BackgroundColor = Microsoft.Xna.Framework.Color.Transparent;
			//panel.BorderColor =  Microsoft.Xna.Framework.Color.Transparent;
			if (data != null)
				Append(dataListElement);
			dataListElement.OverflowHidden = true;

			dataList = new NestedUIList();
			dataList.Width.Set(-20, 1f);
			dataList.Left.Set(0, 0f);
			dataList.Height.Set(0, 1f);
			dataList.ListPadding = 5f;
			dataListElement.Append(dataList);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(-16f, 1f);
			scrollbar.Top.Set(6f, 0f);
			scrollbar.Left.Pixels -= 3;
			scrollbar.HAlign = 1f;
			dataList.SetScrollbar(scrollbar);
			dataListElement.Append(scrollbar);

			PrepareTypes();
			// allow null collections to simplify modder code for OnDeserialize and allow null and empty lists to have different meanings, etc.
			SetupList();

			if (CanAdd) {
				initializeButton = new UIModConfigHoverImage(playTexture, "Initialize");
				initializeButton.Top.Pixels += 4;
				initializeButton.Left.Pixels -= 3;
				initializeButton.HAlign = 1f;
				initializeButton.OnClick += (a, b) => {
					Main.PlaySound(SoundID.Tink);
					InitializeCollection();
					SetupList();
					Interface.modConfig.RecalculateChildren(); // not needed?
					Interface.modConfig.SetPendingChanges();
					expanded = true;
					pendingChanges = true;
				};

				addButton = new UIModConfigHoverImage(plusTexture, "Add");
				addButton.Top.Set(4, 0f);
				addButton.Left.Set(-52, 1f);
				addButton.OnClick += (a, b) => {
					Main.PlaySound(SoundID.Tink);
					AddItem();
					SetupList();
					Interface.modConfig.RecalculateChildren();
					Interface.modConfig.SetPendingChanges();
					expanded = true;
					pendingChanges = true;
				};

				deleteButton = new UIModConfigHoverImage(deleteTexture, "Clear");
				deleteButton.Top.Set(4, 0f);
				deleteButton.Left.Set(-25, 1f);
				deleteButton.OnClick += (a, b) => {
					Main.PlaySound(SoundID.Tink);
					if (nullAllowed)
						NullCollection();
					else
						ClearCollection();
					SetupList();
					Interface.modConfig.RecalculateChildren();
					Interface.modConfig.SetPendingChanges();
					pendingChanges = true;
				};
			}

			expandButton = new UIModConfigHoverImage(collapsedTexture, "Expand");
			expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
			expandButton.Left.Set(-79, 1f);
			expandButton.OnClick += (a, b) => {
				expanded = !expanded;
				pendingChanges = true;
			};

			upDownButton = new UIModConfigHoverImageSplit(upDownTexture, "Scale Up", "Scale Down");
			upDownButton.Top.Set(4, 0f);
			upDownButton.Left.Set(-106, 1f);
			upDownButton.OnClick += (a, b) => {
				Rectangle r = b.GetDimensions().ToRectangle();
				if (a.MousePosition.Y < r.Y + r.Height / 2) {
					scale = Math.Min(2f, scale + 0.5f);
				}
				else {
					scale = Math.Max(1f, scale - 0.5f);
				}
				//dataListPanel.RecalculateChildren();
				////dataList.RecalculateChildren();
				//float h = dataList.GetTotalHeight();
				//MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				//Recalculate();
				//if (Parent != null && Parent is UISortableElement) {
				//	Parent.Height.Pixels = GetOuterDimensions().Height;
				//}
			};
			//Append(upButton);

			//var aasdf = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
			//for (int i = 0; i < 100; i++) {
			//	var vb = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png"));
			//}

			pendingChanges = true;
			Recalculate(); // Needed?
		}

		protected virtual bool CanAdd => true;

		protected object CreateCollectionElementInstance(Type type) {
			object toAdd;
			if (defaultListValueAttribute != null) {
				toAdd = defaultListValueAttribute.Value;
			}
			else {
				toAdd = ConfigManager.AlternateCreateInstance(type);
				if (!type.IsValueType) {
					string json = jsonDefaultListValueAttribute?.json ?? "{}";
					JsonConvert.PopulateObject(json, toAdd, ConfigManager.serializerSettings);
				}
			}
			return toAdd;
		}

		// SetupList called in base.ctor, but children need Types.
		protected abstract void PrepareTypes();

		protected abstract void AddItem();

		protected abstract void InitializeCollection();

		protected virtual void NullCollection() {
			data = null;
			SetObject(data);
		}
		protected abstract void ClearCollection();

		protected abstract void SetupList();

		bool pendingChanges = false;
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (!pendingChanges)
				return;
			pendingChanges = false;

			if (CanAdd) {
				RemoveChild(initializeButton);
				RemoveChild(addButton);
				RemoveChild(deleteButton);
			}
			RemoveChild(expandButton);
			RemoveChild(upDownButton);
			RemoveChild(dataListElement);

			if (data == null) {
				Append(initializeButton);
			}
			else {
				if (CanAdd) {
					Append(addButton);
					Append(deleteButton);
				}
				Append(expandButton);
				if (expanded) {
					Append(upDownButton);
					Append(dataListElement);
					expandButton.HoverText = "Collapse";
					expandButton.SetImage(expandedTexture);
				}
				else {
					expandButton.HoverText = "Expand";
					expandButton.SetImage(collapsedTexture);
				}
			}
		}

		public override void Recalculate() {
			base.Recalculate();
			float defaultHeight = 30;
			float h = dataListElement.Parent != null ? dataList.GetTotalHeight() + defaultHeight : defaultHeight; // 24 for UIElement
			h = Utils.Clamp(h, 30, 300 * scale);
			MaxHeight.Set(300 * scale, 0f);
			Height.Set(h, 0f);
			if (Parent != null && Parent is UISortableElement) {
				Parent.Height.Set(h, 0f);
			}
		}
	}
}

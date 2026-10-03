using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal abstract class CollectionElement : ConfigElement
{
	private UIModConfigHoverImage initializeButton;
	private UIModConfigHoverImage addButton;
	private UIModConfigHoverImage deleteButton;
	private UIModConfigHoverImage expandButton;
	private UIModConfigHoverImageSplit upDownButton;
	private bool expanded = true;
	private bool pendingChanges = false;

	protected object Data { get; set; }
	protected UIElement DataListElement { get; set; }
	protected NestedUIList DataList { get; set; }
	protected float Scale { get; set; } = 1f;
	protected DefaultListValueAttribute DefaultListValueAttribute { get; set; }
	protected JsonDefaultListValueAttribute JsonDefaultListValueAttribute { get; set; }

	protected virtual bool CanAdd => true;

	public override void OnBind()
	{
		base.OnBind();

		var expandAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ExpandAttribute>(MemberInfo, Item, List);
		if (expandAttribute != null)
			expanded = expandAttribute.Expand;

		Data = MemberInfo.GetValue(Item);
		DefaultListValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DefaultListValueAttribute>(MemberInfo, null, null);

		MaxHeight.Set(300, 0f);
		DataListElement = new UIElement();
		DataListElement.Width.Set(-10f, 1f);
		DataListElement.Left.Set(10f, 0f);
		DataListElement.Height.Set(-30, 1f);
		DataListElement.Top.Set(30f, 0f);

		//panel.SetPadding(0);
		//panel.BackgroundColor = Microsoft.Xna.Framework.Color.Transparent;
		//panel.BorderColor =  Microsoft.Xna.Framework.Color.Transparent;

		if (Data != null && expanded)
			Append(DataListElement);

		DataListElement.OverflowHidden = true;

		DataList = new NestedUIList();
		DataList.Width.Set(-20, 1f);
		DataList.Left.Set(0, 0f);
		DataList.Height.Set(0, 1f);
		DataList.ListPadding = 5f;
		DataListElement.Append(DataList);

		UIScrollbar scrollbar = new UIScrollbar();
		scrollbar.SetView(100f, 1000f);
		scrollbar.Height.Set(-16f, 1f);
		scrollbar.Top.Set(6f, 0f);
		scrollbar.Left.Pixels -= 3;
		scrollbar.HAlign = 1f;
		DataList.SetScrollbar(scrollbar);
		DataListElement.Append(scrollbar);

		PrepareTypes();
		// allow null collections to simplify modder code for OnDeserialize and allow null and empty lists to have different meanings, etc.
		SetupList();

		if (CanAdd) {
			initializeButton = new UIModConfigHoverImage(PlayTexture, Language.GetTextValue("tModLoader.ModConfigInitialize"));
			initializeButton.Top.Pixels += 4;
			initializeButton.Left.Pixels -= 3;
			initializeButton.HAlign = 1f;
			initializeButton.OnLeftClick += (a, b) => {
				SoundEngine.PlaySound(SoundID.Tink);
				InitializeCollection();
				SetupList();
				Interface.modConfig.RecalculateChildren(); // not needed?
				Interface.modConfig.SetPendingChanges();
				expanded = true;
				pendingChanges = true;
			};

			addButton = new UIModConfigHoverImage(PlusTexture, Language.GetTextValue("tModLoader.ModConfigAdd"));
			addButton.Top.Set(4, 0f);
			addButton.Left.Set(-52, 1f);
			addButton.OnLeftClick += (a, b) => {
				SoundEngine.PlaySound(SoundID.Tink);
				AddItem();
				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
				expanded = true;
				pendingChanges = true;
			};

			deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigClear"));
			deleteButton.Top.Set(4, 0f);
			deleteButton.Left.Set(-25, 1f);
			deleteButton.OnLeftClick += (a, b) => {
				SoundEngine.PlaySound(SoundID.Tink);
				if (NullAllowed)
					NullCollection();
				else
					ClearCollection();
				SetupList();
				Interface.modConfig.RecalculateChildren();
				Interface.modConfig.SetPendingChanges();
				pendingChanges = true;
			};
		}

		expandButton = new UIModConfigHoverImage(expanded ? ExpandedTexture : CollapsedTexture, expanded ? Language.GetTextValue("tModLoader.ModConfigCollapse") : Language.GetTextValue("tModLoader.ModConfigExpand"));
		expandButton.Top.Set(4, 0f); // 10, -25: 4, -52
		expandButton.Left.Set(-79, 1f);
		expandButton.OnLeftClick += (a, b) => {
			expanded = !expanded;
			pendingChanges = true;
		};

		upDownButton = new UIModConfigHoverImageSplit(UpDownTexture, Language.GetTextValue("tModLoader.ModConfigScaleUp"), Language.GetTextValue("tModLoader.ModConfigScaleDown"));
		upDownButton.Top.Set(4, 0f);
		upDownButton.Left.Set(-106, 1f);
		upDownButton.OnLeftClick += (a, b) => {
			Rectangle r = b.GetDimensions().ToRectangle();

			if (a.MousePosition.Y < r.Y + r.Height / 2) {
				Scale = Math.Min(2f, Scale + 0.5f);
			}
			else {
				Scale = Math.Max(1f, Scale - 0.5f);
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

	protected object CreateCollectionElementInstance(Type type)
	{
		object toAdd;

		if (DefaultListValueAttribute != null) {
			toAdd = DefaultListValueAttribute.Value;
		}
		else {
			toAdd = ConfigManager.AlternateCreateInstance(type);

			if (!type.IsValueType && type != typeof(string)) {
				string json = JsonDefaultListValueAttribute?.Json ?? "{}";

				JsonConvert.PopulateObject(json, toAdd, ConfigManager.serializerSettings);
			}
		}

		return toAdd;
	}

	// SetupList called in base.ctor, but children need Types.
	protected abstract void PrepareTypes();

	protected abstract void AddItem();

	protected abstract void InitializeCollection();

	protected virtual void NullCollection()
	{
		Data = null;
		SetObject(Data);
	}
	protected abstract void ClearCollection();

	protected abstract void SetupList();

	public override void Update(GameTime gameTime)
	{
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
		RemoveChild(DataListElement);

		if (Data == null) {
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
				Append(DataListElement);
				expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigCollapse");
				expandButton.SetImage(ExpandedTexture);
			}
			else {
				expandButton.HoverText = Language.GetTextValue("tModLoader.ModConfigExpand");
				expandButton.SetImage(CollapsedTexture);
			}
		}
	}

	public override void Recalculate()
	{
		base.Recalculate();

		float defaultHeight = 30;
		float h = DataListElement.Parent != null ? DataList.GetTotalHeight() + defaultHeight : defaultHeight; // 24 for UIElement

		h = Utils.Clamp(h, 30, 300 * Scale);

		MaxHeight.Set(300 * Scale, 0f);
		Height.Set(h, 0f);

		if (Parent != null && Parent is UISortableElement) {
			Parent.Height.Set(h, 0f);
		}
	}
}

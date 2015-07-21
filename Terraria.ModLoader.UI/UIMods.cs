using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.UI {
internal class UIMods : UIState
{
    private UIList modList;
    private List<UIModItem> items = new List<UIModItem>();
    public override void OnInitialize()
    {
        UIElement uIElement = new UIElement();
        uIElement.Width.Set(0f, 0.8f);
        uIElement.MaxWidth.Set(600f, 0f);
        uIElement.Top.Set(220f, 0f);
        uIElement.Height.Set(-220f, 1f);
        uIElement.HAlign = 0.5f;
        UIPanel uIPanel = new UIPanel();
        uIPanel.Width.Set(0f, 1f);
        uIPanel.Height.Set(-110f, 1f);
        uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
        uIElement.Append(uIPanel);
        modList = new UIList();
        modList.Width.Set(-25f, 1f);
        modList.Height.Set(0f, 1f);
        modList.ListPadding = 5f;
        uIPanel.Append(modList);
        UIScrollbar uIScrollbar = new UIScrollbar();
        uIScrollbar.SetView(100f, 1000f);
        uIScrollbar.Height.Set(0f, 1f);
        uIScrollbar.HAlign = 1f;
        uIPanel.Append(uIScrollbar);
        modList.SetScrollbar(uIScrollbar);
        UITextPanel uITextPanel = new UITextPanel("Mods List", 0.8f, true);
        uITextPanel.HAlign = 0.5f;
        uITextPanel.Top.Set(-35f, 0f);
        uITextPanel.SetPadding(15f);
        uITextPanel.BackgroundColor = new Color(73, 94, 171);
        uIElement.Append(uITextPanel);
        UIColorTextPanel button = new UIColorTextPanel("Enable All", Color.Green, 1f, false);
        button.Width.Set(-10f, 0.5f);
        button.Height.Set(25f, 0f);
        button.VAlign = 1f;
        button.Top.Set(-65f, 0f);
        button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
        button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
        button.OnClick += new UIElement.MouseEvent(this.EnableAll);
        uIElement.Append(button);
        UIColorTextPanel button2 = new UIColorTextPanel("Disable All", Color.Red, 1f, false);
        button2.CopyStyle(button);
        button2.HAlign = 1f;
        button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
        button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
        button2.OnClick += new UIElement.MouseEvent(this.DisableAll);
        uIElement.Append(button2);
        UITextPanel uITextPanel2 = new UITextPanel("Back", 1f, false);
        uITextPanel2.CopyStyle(button);
        uITextPanel2.Top.Set(-20f, 0f);
        uITextPanel2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
        uITextPanel2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
        uITextPanel2.OnClick += new UIElement.MouseEvent(BackClick);
        uIElement.Append(uITextPanel2);
        UITextPanel uITextPanel3 = new UITextPanel("Reload Mods", 1f, false);
        uITextPanel3.CopyStyle(uITextPanel2);
        uITextPanel3.HAlign = 1f;
        uITextPanel3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
        uITextPanel3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
        uITextPanel3.OnClick += new UIElement.MouseEvent(ReloadMods);
        uIElement.Append(uITextPanel3);
        base.Append(uIElement);
    }

    private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.PlaySound(12, -1, -1, 1);
        ((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
    }

    private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
    {
        ((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
    }

    private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.PlaySound(11, -1, -1, 1);
        Main.menuMode = 0;
    }

    private static void ReloadMods(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.PlaySound(10, -1, -1, 1);
        ModLoader.Reload();
    }

    private void EnableAll(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.PlaySound(12, -1, -1, 1);
        foreach(UIModItem modItem in items)
        {
            if(!modItem.enabled)
            {
                modItem.ToggleEnabled(evt, listeningElement);
            }
        }
    }

    private void DisableAll(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.PlaySound(12, -1, -1, 1);
        foreach (UIModItem modItem in items)
        {
            if (modItem.enabled)
            {
                modItem.ToggleEnabled(evt, listeningElement);
            }
        }
    }

    public override void OnActivate()
    {
        modList.Clear();
        items.Clear();
        string[] mods = ModLoader.FindMods();
        foreach(string mod in mods)
        {
            UIModItem modItem = new UIModItem(mod);
            modList.Add(modItem);
            items.Add(modItem);
        }
    }
}}

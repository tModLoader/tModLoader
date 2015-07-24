using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader {
public static class EquipLoader
{
    //in Terraria.Main.DrawPlayer and Terraria.Main.DrawPlayerHead get rid of checks for slot too high (not necessary for loading)
    private static readonly IDictionary<EquipType, int> nextEquip = new Dictionary<EquipType, int>();
    internal static readonly IDictionary<EquipType, IDictionary<string, int>> equips = new Dictionary<EquipType, IDictionary<string, int>>();
    internal static readonly IDictionary<int, EquipType> idToType = new Dictionary<int, EquipType>();
    internal static readonly IDictionary<int, int> idToSlot = new Dictionary<int, int>();
    internal static readonly IDictionary<int, string> femaleTextures = new Dictionary<int, string>();
    internal static readonly IDictionary<int, string> armTextures = new Dictionary<int, string>();
    static EquipLoader()
    {
        foreach(EquipType type in Enum.GetValues(typeof(EquipType)))
        {
            nextEquip[type] = GetNumVanilla(type);
            equips[type] = new Dictionary<string, int>();
        }
    }

    internal static int ReserveEquipID(EquipType type)
    {
        int reserveID = nextEquip[type];
        nextEquip[type]++;
        return reserveID;
    }

    public static int GetEquipSlot(EquipType type, string texture)
    {
        if(equips[type].ContainsKey(texture))
        {
            return equips[type][texture];
        }
        else
        {
            return 0;
        }
    }

    public static sbyte GetAccessorySlot(EquipType type, string texture)
    {
        return (sbyte)GetEquipSlot(type, texture);
    }

    internal static void ResizeAndFillArrays()
    {
        Array.Resize(ref Main.armorHeadLoaded, nextEquip[EquipType.Head]);
        Array.Resize(ref Main.armorBodyLoaded, nextEquip[EquipType.Body]);
        Array.Resize(ref Main.armorLegsLoaded, nextEquip[EquipType.Legs]);
        Array.Resize(ref Main.accHandsOnLoaded, nextEquip[EquipType.HandsOn]);
        Array.Resize(ref Main.accHandsOffLoaded, nextEquip[EquipType.HandsOff]);
        Array.Resize(ref Main.accBackLoaded, nextEquip[EquipType.Back]);
        Array.Resize(ref Main.accFrontLoaded, nextEquip[EquipType.Front]);
        Array.Resize(ref Main.accShoesLoaded, nextEquip[EquipType.Shoes]);
        Array.Resize(ref Main.accWaistLoaded, nextEquip[EquipType.Waist]);
        Array.Resize(ref Main.wingsLoaded, nextEquip[EquipType.Wings]);
        Array.Resize(ref Main.accShieldLoaded, nextEquip[EquipType.Shield]);
        Array.Resize(ref Main.accNeckLoaded, nextEquip[EquipType.Neck]);
        Array.Resize(ref Main.accFaceLoaded, nextEquip[EquipType.Face]);
        Array.Resize(ref Main.accballoonLoaded, nextEquip[EquipType.Balloon]);
        foreach(EquipType type in Enum.GetValues(typeof(EquipType)))
        {
            for(int k = GetNumVanilla(type); k < nextEquip[type]; k++)
            {
                GetLoadedArray(type)[k] = true;
            }
        }
        Array.Resize(ref Main.armorHeadTexture, nextEquip[EquipType.Head]);
        Array.Resize(ref Main.armorBodyTexture, nextEquip[EquipType.Body]);
        Array.Resize(ref Main.femaleBodyTexture, nextEquip[EquipType.Body]);
        Array.Resize(ref Main.armorArmTexture, nextEquip[EquipType.Body]);
        Array.Resize(ref Main.armorLegTexture, nextEquip[EquipType.Legs]);
        Array.Resize(ref Main.accHandsOnTexture, nextEquip[EquipType.HandsOn]);
        Array.Resize(ref Main.accHandsOffTexture, nextEquip[EquipType.HandsOff]);
        Array.Resize(ref Main.accBackTexture, nextEquip[EquipType.Back]);
        Array.Resize(ref Main.accFrontTexture, nextEquip[EquipType.Front]);
        Array.Resize(ref Main.accShoesTexture, nextEquip[EquipType.Shoes]);
        Array.Resize(ref Main.accWaistTexture, nextEquip[EquipType.Waist]);
        Array.Resize(ref Main.wingsTexture, nextEquip[EquipType.Wings]);
        Array.Resize(ref Main.accShieldTexture, nextEquip[EquipType.Shield]);
        Array.Resize(ref Main.accNeckTexture, nextEquip[EquipType.Neck]);
        Array.Resize(ref Main.accFaceTexture, nextEquip[EquipType.Face]);
        Array.Resize(ref Main.accBalloonTexture, nextEquip[EquipType.Balloon]);
        foreach(EquipType type in Enum.GetValues(typeof(EquipType)))
        {
            foreach(string texture in equips[type].Keys)
            {
                int slot = GetEquipSlot(type, texture);
                GetTextureArray(type)[slot] = ModLoader.GetTexture(texture);
                if(type == EquipType.Body)
                {
                    if(femaleTextures.ContainsKey(slot))
                    {
                        Main.femaleBodyTexture[slot] = ModLoader.GetTexture(femaleTextures[slot]);
                    }
                    else
                    {
                        Main.femaleBodyTexture[slot] = Main.armorBodyTexture[slot];
                    }
                    Main.armorArmTexture[slot] = ModLoader.GetTexture(armTextures[slot]);
                }
            }
        }
    }

    internal static void Unload()
    {
        foreach(EquipType type in Enum.GetValues(typeof(EquipType)))
        {
            nextEquip[type] = GetNumVanilla(type);
            equips[type].Clear();
        }
        idToType.Clear();
        idToSlot.Clear();
        femaleTextures.Clear();
        armTextures.Clear();
    }

    internal static int GetNumVanilla(EquipType type)
    {
        switch(type)
        {
            case EquipType.Head:
                return Main.numArmorHead;
            case EquipType.Body:
                return Main.numArmorBody;
            case EquipType.Legs:
                return Main.numArmorLegs;
            case EquipType.HandsOn:
                return Main.numAccHandsOn;
            case EquipType.HandsOff:
                return Main.numAccHandsOff;
            case EquipType.Back:
                return Main.numAccBack;
            case EquipType.Front:
                return Main.numAccFront;
            case EquipType.Shoes:
                return Main.numAccShoes;
            case EquipType.Waist:
                return Main.numAccWaist;
            case EquipType.Wings:
                return Main.maxWings;
            case EquipType.Shield:
                return Main.numAccShield;
            case EquipType.Neck:
                return Main.numAccNeck;
            case EquipType.Face:
                return Main.numAccFace;
            case EquipType.Balloon:
                return Main.numAccBalloon;
        }
        return 0;
    }

    internal static bool[] GetLoadedArray(EquipType type)
    {
        switch(type)
        {
            case EquipType.Head:
                return Main.armorHeadLoaded;
            case EquipType.Body:
                return Main.armorBodyLoaded;
            case EquipType.Legs:
                return Main.armorLegsLoaded;
            case EquipType.HandsOn:
                return Main.accHandsOnLoaded;
            case EquipType.HandsOff:
                return Main.accHandsOffLoaded;
            case EquipType.Back:
                return Main.accBackLoaded;
            case EquipType.Front:
                return Main.accFrontLoaded;
            case EquipType.Shoes:
                return Main.accShoesLoaded;
            case EquipType.Waist:
                return Main.accWaistLoaded;
            case EquipType.Wings:
                return Main.wingsLoaded;
            case EquipType.Shield:
                return Main.accShieldLoaded;
            case EquipType.Neck:
                return Main.accNeckLoaded;
            case EquipType.Face:
                return Main.accFaceLoaded;
            case EquipType.Balloon:
                return Main.accballoonLoaded;
        }
        return null;
    }

    internal static Texture2D[] GetTextureArray(EquipType type)
    {
        switch (type)
        {
            case EquipType.Head:
                return Main.armorHeadTexture;
            case EquipType.Body:
                return Main.armorBodyTexture;
            case EquipType.Legs:
                return Main.armorLegTexture;
            case EquipType.HandsOn:
                return Main.accHandsOnTexture;
            case EquipType.HandsOff:
                return Main.accHandsOffTexture;
            case EquipType.Back:
                return Main.accBackTexture;
            case EquipType.Front:
                return Main.accFrontTexture;
            case EquipType.Shoes:
                return Main.accShoesTexture;
            case EquipType.Waist:
                return Main.accWaistTexture;
            case EquipType.Wings:
                return Main.wingsTexture;
            case EquipType.Shield:
                return Main.accShieldTexture;
            case EquipType.Neck:
                return Main.accNeckTexture;
            case EquipType.Face:
                return Main.accFaceTexture;
            case EquipType.Balloon:
                return Main.accBalloonTexture;
        }
        return null;
    }

    internal static void SetSlot(Item item)
    {
        if(idToType.ContainsKey(item.type))
        {
            int slot = idToSlot[item.type];
            switch(idToType[item.type])
            {
                case EquipType.Head:
                    item.headSlot = slot;
                    break;
                case EquipType.Body:
                    item.bodySlot = slot;
                    break;
                case EquipType.Legs:
                    item.legSlot = slot;
                    break;
                case EquipType.HandsOn:
                    item.handOnSlot = (sbyte)slot;
                    break;
                case EquipType.HandsOff:
                    item.handOffSlot = (sbyte)slot;
                    break;
                case EquipType.Back:
                    item.backSlot = (sbyte)slot;
                    break;
                case EquipType.Front:
                    item.frontSlot = (sbyte)slot;
                    break;
                case EquipType.Shoes:
                    item.shoeSlot = (sbyte)slot;
                    break;
                case EquipType.Waist:
                    item.waistSlot = (sbyte)slot;
                    break;
                case EquipType.Wings:
                    item.wingSlot = (sbyte)slot;
                    break;
                case EquipType.Shield:
                    item.shieldSlot = (sbyte)slot;
                    break;
                case EquipType.Neck:
                    item.neckSlot = (sbyte)slot;
                    break;
                case EquipType.Face:
                    item.faceSlot = (sbyte)slot;
                    break;
                case EquipType.Balloon:
                    item.balloonSlot = (sbyte)slot;
                    break;
            }
        }
    }
}}

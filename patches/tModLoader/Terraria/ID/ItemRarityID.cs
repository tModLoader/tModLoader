using ReLogic.Reflection;

namespace Terraria.ID;

/// <summary>Enumerates the values used with Item.rare</summary>
public static class ItemRarityID
{
	/// <summary>Minus thirteen (-13)<br/>Master: Fiery Red<br/>Flag: item.master</summary>
	public const int Master = -13;
	/// <summary>Minus twelve (-12)<br/>Expert: Rainbow<br/>Flag: item.expert</summary>
	public const int Expert = -12;
	/// <summary>Minus eleven (-11)<br/>Quest: Amber<br/>Flag: item.quest</summary>
	public const int Quest = -11;
	/// <summary>Minus one (-1)</summary>
	public const int Gray = -1;
	/// <summary>Zero (0)</summary>
	public const int White = 0;
	/// <summary>One (1)</summary>
	public const int Blue = 1;
	/// <summary>Two (2)</summary>
	public const int Green = 2;
	/// <summary>Three (3)</summary>
	public const int Orange = 3;
	/// <summary>Four (4)</summary>
	public const int LightRed = 4;
	/// <summary>Five (5)</summary>
	public const int Pink = 5;
	/// <summary>Six (6)</summary>
	public const int LightPurple = 6;
	/// <summary>Seven (7)</summary>
	public const int Lime = 7;
	/// <summary>Eight (8)</summary>
	public const int Yellow = 8;
	/// <summary>Nine (9)</summary>
	public const int Cyan = 9;
	/// <summary>Ten (10)</summary>
	public const int Red = 10;
	/// <summary>Eleven (11)</summary>
	public const int Purple = 11;
	public const int Count = 12;
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(ItemRarityID), typeof(int));
}

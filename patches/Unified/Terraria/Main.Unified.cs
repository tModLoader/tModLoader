using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria;

partial class Main
{
	internal static string UnifiedVersion => "0.1.1";
	internal static bool UnifiedBranding => true;

	public static bool Vsync { get; set; } = true;

	internal static List<TitleLinkButton> UnifiedLinks { get; } = [];
}

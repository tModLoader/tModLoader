using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria;

partial class Main
{
	internal static string UnifiedVersion => "0.1.0";
	internal static bool UnifiedBranding => true;

	internal static List<TitleLinkButton> UnifiedLinks { get; } = [];
}

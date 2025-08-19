// Those exist just so that analyzers are compileable without
// being forced to copy many source classes or patching every import out
//
// Interestingly enough, file access modifier works just as fine as if it was internal or anything else.

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Xna.Framework
{
	file class Dummy { }
}

namespace Terraria.DataStructures
{
	file class Dummy { }
}

namespace Terraria.ModLoader
{
	file class Dummy { }
}

namespace Terraria.WorldBuilding
{
	file class Dummy { }
}

#pragma warning restore IDE0130 // Namespace does not match folder structure
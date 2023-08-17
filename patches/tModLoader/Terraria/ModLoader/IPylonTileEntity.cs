using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// Allows Tile Entities that you want to be included in vanilla's pylon list to actually be added to the list
/// when said TE is extended by this interface. What this means, in short,
/// is that whenever all of the pylons are refreshed by vanilla, Tile Entities that extend this interface will get their own
/// <seealso cref="TeleportPylonInfo"/> instance and it will be included along-side all of vanilla's pylons and any
/// other modded pylons.
/// </summary>
public interface IPylonTileEntity { }

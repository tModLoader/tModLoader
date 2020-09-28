using Terraria.ModLoader.IO;

namespace Terraria.DataStructures
{
	public partial class TileEntity
	{
		/// <summary>
		/// Allows you to save custom data for this tile entity.
		/// </summary>
		/// <returns></returns>
		public virtual TagCompound Save() {
			return null;
		}

		/// <summary>
		/// Allows you to load the custom data you have saved for this tile entity.
		/// </summary>
		public virtual void Load(TagCompound tag) {
		}
	}
}

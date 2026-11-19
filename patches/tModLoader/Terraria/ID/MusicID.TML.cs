using Terraria.ModLoader;

namespace Terraria.ID;

public partial class MusicID
{
	public partial class Sets
	{
		/// <summary>
		/// Skips Terraria's <see cref="Terraria.Audio.ASoundEffectBasedAudioTrack.ReMapVolumeToMatchXact(float)"/> function to make music play at its intended volume.
		/// <para/>This should be set in <see cref="ModType.SetStaticDefaults()"/>, preferably <see cref="ModSystem"/>
		/// </summary>
		public static bool[] SkipsVolumeRemap = Factory.CreateBoolSet(false);
	}
}

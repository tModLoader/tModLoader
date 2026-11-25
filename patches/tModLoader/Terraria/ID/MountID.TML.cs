namespace Terraria.ID;

public partial class MountID
{
	public partial class Sets
	{
		// Values taken from Mount.DoesHoverIgnoresFatigue()
		/// <summary>
		/// If set to true and the mount hovers with <see cref="Mount.MountData.usesHover"/>, it will ignore <see cref="Mount.MountData.fatigueMax"/>.
		/// <para/> Used by infinite flight mounts: <see cref="UFO"/>, <see cref="Drill"/>, <see cref="CuteFishron"/>, <see cref="WitchBroom"/>, <see cref="PirateShip"/>, <see cref="LavaShark"/>, <see cref="Bat"/>, and <see cref="Pixie"/>.
		/// </summary>
		public static bool[] HoverIgnoresFatigue = Factory.CreateBoolSet(false, UFO, Drill, CuteFishron, WitchBroom, PirateShip, LavaShark, Bat, Pixie);

		/// <summary>
		/// Allows head slot items to draw on the mount even if the mount hides the player.
		/// <para/> <see cref="ArmorIDs.Head.Sets.CanDrawOnVelociraptorMount"/> must also be true for the head item to show up.
		/// <para/> Used by <see cref="Velociraptor"/>.
		/// </summary>
		public static bool[] DrawHeadItemOnMountThatHidesThePlayer = Factory.CreateBoolSet(false, Velociraptor);
	}
}

using Terraria;
using Terraria.ID;

public class RenameAndInvertMemberTest
{
	void Method()
	{
		if (!FocusHelper.GameplayActive) { }
		if (FocusHelper.GameplayActive) { }

		bool drawHead = false;
		bool other = true;
		ArmorIDs.Head.Sets.HidesHead[0] = !(drawHead || other);
		ArmorIDs.Head.Sets.HidesHead[0] = true;
		ArmorIDs.Head.Sets.HidesHead[0] = false;
		ArmorIDs.Head.Sets.HidesHead[0] = !drawHead;
		ArmorIDs.Head.Sets.HidesHead[0] = drawHead;
		ArmorIDs.Head.Sets.HidesHead[0] = !(drawHead || other);
		ArmorIDs.Head.Sets.HidesHead[0] = (drawHead || other);
		other = !ArmorIDs.Head.Sets.HidesHead[0];
		if (!ArmorIDs.Head.Sets.HidesHead[0]) { }
		if (other == !ArmorIDs.Head.Sets.HidesHead[0]) { }
	}
}

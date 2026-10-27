using Terraria;
using Terraria.ID;

public class RenameAndInvertMemberTest
{
	void Method()
	{
		if (Main.gameInactive) { }
		if (!Main.gameInactive) { }

		bool drawHead = false;
		bool other = true;
		ArmorIDs.Head.Sets.DrawHead[0] = drawHead || other;
		ArmorIDs.Head.Sets.DrawHead[0] = false;
		ArmorIDs.Head.Sets.DrawHead[0] = true;
		ArmorIDs.Head.Sets.DrawHead[0] = drawHead;
		ArmorIDs.Head.Sets.DrawHead[0] = !drawHead;
		ArmorIDs.Head.Sets.DrawHead[0] = (drawHead || other);
		ArmorIDs.Head.Sets.DrawHead[0] = !(drawHead || other);
		other = ArmorIDs.Head.Sets.DrawHead[0];
		if (ArmorIDs.Head.Sets.DrawHead[0]) { }
		if (other == ArmorIDs.Head.Sets.DrawHead[0]) { }
	}
}

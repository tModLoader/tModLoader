using Terraria;
using Terraria.DataStructures;

public class EntitySourceTest
{
	void TestSources(IEntitySource source)
	{
		if (source is EntitySource_BossSpawn sb)
			_ = sb.Target;

		if (source is EntitySource_FishedOut sf)
			_ = sf.Fisher;

		if (source is EntitySource_OnHit hit && hit.Attacker is Player)
			_ = hit.Victim;
	}
}
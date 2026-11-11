using System;

namespace Terraria.DataStructures;

[Obsolete("Check Context == \"TorchGod\" instead.", error: true)]
public class EntitySource_TorchGod { }

[Obsolete("Renamed to EntitySource_Caught, note that Entity has become Catcher, and CaughtEntity has become Entity", error: true)]
public class EntitySource_CatchEntity { }

[Obsolete("Provides no utility over EntitySource_Parent, use that instead.")]
public class EntitySource_HitEffect : EntitySource_Parent
{
	public EntitySource_HitEffect(Entity entity, string context = null) : base(entity, context) { }
}

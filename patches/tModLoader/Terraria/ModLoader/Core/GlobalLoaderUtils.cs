using System;
using System.Buffers;
using System.Linq;

namespace Terraria.ModLoader.Core;

public static class GlobalLoaderUtils<TGlobal, TEntity> where TGlobal : GlobalType<TEntity, TGlobal> where TEntity : IEntityWithGlobals<TGlobal>
{
	private static TGlobal[] Globals => GlobalList<TGlobal>.Globals;

	private static TGlobal[][] SlotPerEntityGlobals = null;
	private static TGlobal[][] HookSetDefaultsEarly = null;
	private static TGlobal[][] HookSetDefaultsLate = null;

	static GlobalLoaderUtils()
	{
		TypeCaching.ResetStaticMembersOnClear(typeof(GlobalTypeLookups<TGlobal>));
	}

	public static void SetDefaults(TEntity entity, ref TGlobal[] entityGlobals, Action<TEntity> setModEntityDefaults)
	{
		if (entity.Type == 0)
			return;

		int initialType = entity.Type;

		entityGlobals = new TGlobal[GlobalList<TGlobal>.SlotsPerEntity];
		if (!GlobalTypeLookups<TGlobal>.Initialized) {
			SetDefaultsBeforeLookupsAreBuilt(entity, entityGlobals, setModEntityDefaults);
			return;
		}

		foreach (var g in SlotPerEntityGlobals[entity.Type]) {
			var slot = g.PerEntityIndex;
			entityGlobals[slot] = g.InstancePerEntity ? g.NewInstance(entity) : g;
		}

		setModEntityDefaults(entity);

		foreach (var g in new EntityGlobalsEnumerator<TGlobal>(HookSetDefaultsEarly[entity.Type], entity)) {
			g.SetDefaults(entity);
		}

		foreach (var g in new EntityGlobalsEnumerator<TGlobal>(HookSetDefaultsLate[entity.Type], entity)) {
			g.SetDefaults(entity);
		}

		if (entity.Type != initialType)
			throw new Exception($"A mod attempted to {typeof(TEntity).Name}.type from {initialType} to {entity.Type} during SetDefaults. {entity}");
	}

	private enum InstantiationTime
	{
		NotApplied,
		Pass1,
		Pass2
	}

	private static void SetDefaultsBeforeLookupsAreBuilt(TEntity entity, TGlobal[] entityGlobals, Action<TEntity> setModEntityDefaults)
	{
		var instTimes = ArrayPool<InstantiationTime>.Shared.Rent(Globals.Length);
		try {
			SetDefaultsBeforeLookupsAreBuilt(entity, entityGlobals, setModEntityDefaults, ref instTimes);
			UpdateGlobalTypeData?.Invoke(entity.Type, instTimes.AsSpan()[..Globals.Length]);
		}
		finally {
			ArrayPool<InstantiationTime>.Shared.Return(instTimes, clearArray: true);
		}
	}

	private static void SetDefaultsBeforeLookupsAreBuilt(TEntity entity, TGlobal[] entityGlobals, Action<TEntity> setModEntityDefaults, ref InstantiationTime[] instTimes)
	{
		// pass 1
		foreach (var g in Globals) {
			if (g.ConditionallyAppliesToEntities && !g.AppliesToEntity(entity, lateInstantiation: false))
				continue;

			if (g.PerEntityIndex >= 0)
				entityGlobals[g.PerEntityIndex] = g.InstancePerEntity ? g.NewInstance(entity) : g;

			instTimes[g.StaticIndex] = InstantiationTime.Pass1;
		}

		setModEntityDefaults(entity);

		// set defaults
		foreach (var g in Globals) {
			if (instTimes[g.StaticIndex] == InstantiationTime.Pass1)
				(g.PerEntityIndex >= 0 ? entityGlobals[g.PerEntityIndex] : g)?.SetDefaults(entity);
		}

		// pass 2
		foreach (var g in Globals) {
			if (instTimes[g.StaticIndex] == InstantiationTime.Pass1 || g.ConditionallyAppliesToEntities && !g.AppliesToEntity(entity, lateInstantiation: true))
				continue;

			if (g.PerEntityIndex >= 0)
				entityGlobals[g.PerEntityIndex] = g.InstancePerEntity ? g.NewInstance(entity) : g;

			instTimes[g.StaticIndex] = InstantiationTime.Pass2;
		}

		// set defaults
		foreach (var g in Globals) {
			if (instTimes[g.StaticIndex] == InstantiationTime.Pass2)
				(g.PerEntityIndex >= 0 ? entityGlobals[g.PerEntityIndex] : g)?.SetDefaults(entity);
		}
	}

	private delegate void TUpdateGlobalTypeData(int type, ReadOnlySpan<InstantiationTime> data);
	[ThreadStatic]
	private static TUpdateGlobalTypeData UpdateGlobalTypeData;
	public static void BuildTypeLookups(Action<int> setDefaults)
	{
		try {
			var hookSetDefaults = Globals.WhereMethodIsOverridden(g => g.SetDefaults).ToArray();

			int typeCount = GlobalList<TGlobal>.EntityTypeCount;
			Array.Fill(HookSetDefaultsEarly = new TGlobal[typeCount][], Array.Empty<TGlobal>());
			Array.Fill(HookSetDefaultsLate = new TGlobal[typeCount][], Array.Empty<TGlobal>());

			var globalsForType = new TGlobal[typeCount][];
			Array.Fill(globalsForType, Array.Empty<TGlobal>());

			var appliesToTypeCache = new GlobalTypeLookups<TGlobal>.AppliesToTypeSet[Globals.Length];

			InstantiationTime[] instTimes = new InstantiationTime[Globals.Length];
			for (int setDefaultsType = 0; setDefaultsType < typeCount; setDefaultsType++) {
				// UpdateGlobalTypeData may be called multiple times as a vanilla item calls SetDefaults to change type
				int finalType = 0;
				UpdateGlobalTypeData = (type, data) => {
					if (type == 0)
						return; // if an item gets turned to air after being instantiated, we may as well keep earlier default lookups if we can

					finalType = type;
					data.CopyTo(instTimes);
				};

				setDefaults(setDefaultsType);

				if (finalType == 0)
					continue;

				globalsForType[finalType] = GlobalTypeLookups<TGlobal>.CachedFilter(Globals, g => instTimes[g.StaticIndex] > InstantiationTime.NotApplied);
				HookSetDefaultsEarly[finalType] = GlobalTypeLookups<TGlobal>.CachedFilter(hookSetDefaults, g => instTimes[g.StaticIndex] == InstantiationTime.Pass1);
				HookSetDefaultsLate[finalType] = GlobalTypeLookups<TGlobal>.CachedFilter(hookSetDefaults, g => instTimes[g.StaticIndex] == InstantiationTime.Pass2);

				foreach (var g in Globals)
					if (g.ConditionallyAppliesToEntities && instTimes[g.StaticIndex] > InstantiationTime.NotApplied)
						appliesToTypeCache[g.StaticIndex].Add(finalType);
			}

			GlobalTypeLookups<TGlobal>.Init(globalsForType, appliesToTypeCache);
			SlotPerEntityGlobals = GlobalTypeLookups<TGlobal>.BuildPerTypeGlobalLists(Globals.Where(g => g.SlotPerEntity).ToArray());
		}
		finally {
			UpdateGlobalTypeData = null;
		}
	}
}

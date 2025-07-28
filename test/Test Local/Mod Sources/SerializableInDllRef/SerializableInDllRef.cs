using Terraria.ModLoader;
using SerializableDep;
using Terraria.ModLoader.IO;
using System;

namespace SerializableInDllRef
{
	public class ChildInDepMod : Parent
	{
		public static readonly Func<TagCompound, ChildInDepMod> DESERIALIZER = _ => new();
	}

	class SerializableInDllRef : Mod
	{
		public override void Load()
		{
			var tag = new TagCompound() {
				["test"] = new Child(),
				["test2"] = new ChildInDepMod()
			};

			Logger.Info("Deserialized " + tag.Get<Parent>("test"));
			Logger.Info("Deserialized " + tag.Get<Parent>("test2"));
		}
	}
}

using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	public abstract class ModConfig
	{
		public Mod mod { get; internal set; }
		public string Name { get; internal set; }
		public virtual bool Autoload(ref string name) => mod.Properties.Autoload;
	}

	public enum MultiplayerSyncMode
	{
		ServerDictates,
		UniquePerPlayer
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MyCustomAttribute : Attribute
	{
		public string SomeProperty { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class OptionStringsAttribute : Attribute
	{
		public string[] optionLabels { get; set; }
		public OptionStringsAttribute(string[] optionLabels)
		{
			this.optionLabels = optionLabels;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MultiplayerMode : Attribute
	{
		public MultiplayerSyncMode Mode { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class LabelAttribute : Attribute
	{
		readonly string label;

		// This is a positional argument
		public LabelAttribute(string label)
		{
			this.label = label;
		}

		public string Label
		{
			get { return label; }
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ValueRangeAttribute : Attribute
	{
		private int min;
		private int max;

		public ValueRangeAttribute(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

		public int Max
		{
			get { return max; }
			set { max = value; }
		}

		public int Min
		{
			get { return min; }
			set { min = value; }
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class FloatValueRangeAttribute : Attribute
	{
		private float min;
		private float max;

		public FloatValueRangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Max
		{
			get { return max; }
			set { max = value; }
		}

		public float Min
		{
			get { return min; }
			set { min = value; }
		}
	}
}

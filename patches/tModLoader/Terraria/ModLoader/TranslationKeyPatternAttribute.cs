using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class TranslationKeyPatternAttribute : Attribute
	{
		public string Pattern { get; }
		public TranslationKeyPatternAttribute(string pattern) {
			Pattern = pattern;
		}
	}
}

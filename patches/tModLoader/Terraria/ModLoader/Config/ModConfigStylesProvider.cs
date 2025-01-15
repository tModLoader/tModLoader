using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config;

/// <summary>
/// This class allows you to customize some styles of the config element.
/// Used as the parameter of <see cref="CustomModConfigStylesAttribute"/>.
/// </summary>
public class ModConfigStylesProvider
{
	/// <summary>
	/// Allows you to dynamically modify label color.
	/// </summary>
	/// <param name="getObject">The func to get the config element value.</param>
	/// <param name="isMouseHovering">See <see cref="UIElement.IsMouseHovering"/>.</param>
	/// <param name="canWrite">See <see cref="PropertyFieldWrapper.CanWrite"/>.</param>
	/// <param name="labelColor">The final color of the label.</param>
	public virtual void ModifyLabelColor(Func<object> getObject, bool isMouseHovering, bool canWrite, ref Color labelColor)
	{
		
	}

	/// <summary>
	/// Allows you to dynamically modify label text.
	/// </summary>
	/// <param name="getObject"></param>
	/// <param name="configElementLabel">See <see cref="ConfigElement.Label"/>.</param>
	/// <param name="label">The label to display.</param>
	public virtual void ModifyLabel(Func<object> getObject, string configElementLabel, ref string label)
	{
	}
}

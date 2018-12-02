using Microsoft.Xna.Framework;
using ReLogic.Graphics;

namespace Terraria.ModLoader
{
	// contains additional info for modders to use when using tooltip related draw hooks
	// most stuff is readonly here or only has a public getter, because the related draw hooks are not meant for modifying most info (hence also new keywords to hide from TooltipLine)
	// modders should use ModifyTooltips for modifying tooltips
	// what is modifiable is certain draw related info such as the X and Y position
	/// <summary>
	/// This class serves as a way to store information about a line that will be drawn of tooltip for an item. You will create and manipulate objects of this class if you use the draw hooks for tooltips in ModItem and GlobalItem. For examples, see ExampleSword
	/// </summary>
	public sealed class DrawableTooltipLine : TooltipLine
	{
		/// <summary>
		/// The text of this tooltip. 
		/// </summary>
		public new readonly string text;
		/// <summary>
		/// The index of the tooltip in the array
		/// </summary>
		public readonly int index;
		/// <summary>
		/// Whether or not this tooltip gives prefix information. This will make it so that the tooltip is colored either green or red.
		/// </summary>
		public new readonly bool isModifier;
		/// <summary>
		/// If isModifier is true, this determines whether the tooltip is colored green or red.
		/// </summary>
		public new readonly bool isModifierBad;

		private int _originalX;
		/// <summary>
		/// The X position where the tooltip would be drawn that is not adjusted by mods.
		/// </summary>
		public int OriginalX {
			get { return _originalX; }
			internal set { X = _originalX = value; }
		}

		private int _originalY;
		/// <summary>
		/// The Y position where the tooltip would be drawn that is not adjusted by mods.
		/// </summary>
		public int OriginalY {
			get { return _originalY; }
			internal set { Y = _originalY = value; }
		}
		/// <summary>
		/// The X position where the tooltip would be drawn.
		/// </summary>
		public int X;
		/// <summary>
		/// The Y position where the tooltip would be drawn.
		/// </summary>
		public int Y;
		/// <summary>
		/// The color the tooltip would be drawn in
		/// </summary>
		public Color color { get; internal set; }
		/// <summary>
		/// If the tooltip line's color was overridden this will hold that color, it will be null otherwise
		/// </summary>
		public new Color? overrideColor { get; internal set; }
		/// <summary>
		/// Whether the tooltip is a One Drop logo or not. If it is, the tooltip text will be empty.
		/// </summary>
		public new readonly bool oneDropLogo;
		/// <summary>
		/// The font this tooltip would be drawn with
		/// </summary>
		public DynamicSpriteFont font = Main.fontMouseText;
		/// <summary>
		/// The rotation this tooltip would be drawn in
		/// </summary>
		public float rotation = 0f;
		/// <summary>
		/// The origin of this tooltip
		/// </summary>
		public Vector2 origin = Vector2.Zero;
		/// <summary>
		/// The baseScale of this tooltip. When drawing the One Drop logo the scale is calculated by (baseScale.X + baseScale.Y) / 2
		/// </summary>
		public Vector2 baseScale = Vector2.One;

		public float maxWidth = -1;
		public float spread = 2;

		/// <summary>
		/// Creates a new DrawableTooltipLine object
		/// </summary>
		/// <param name="parent">The TooltipLine to make this DrawableTooltipLine from</param>
		/// <param name="index">The index of the line in the array</param>
		/// <param name="x">The X position where the tooltip would be drawn.</param>
		/// <param name="y">The Y position where the tooltip would be drawn.</param>
		/// <param name="color">The color the tooltip would be drawn in</param>
		public DrawableTooltipLine(TooltipLine parent, int index, int x, int y, Color color) : base(parent.mod, parent.Name, parent.text) {
			isModifier = parent.isModifier;
			isModifierBad = parent.isModifierBad;
			overrideColor = parent.overrideColor;
			oneDropLogo = parent.oneDropLogo;
			text = parent.text;

			this.index = index;
			OriginalX = x;
			OriginalY = y;
			this.color = color;
		}
	}
}

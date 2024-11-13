using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Terraria.ModLoader.Setup.CLI;

// Workaround for https://github.com/spectreconsole/spectre.console/issues/1107
// Live display expects at least 2 line breaks
internal class LiveTableRenderer : IRenderable
{
	private readonly Table table;

	public LiveTableRenderer(Table table)
	{
		this.table = table;
	}

	public Measurement Measure(RenderOptions options, int maxWidth)
	{
		return ((IRenderable)table).Measure(options, maxWidth);
	}

	public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
	{
		var segments = (List<Segment>)((IRenderable)table).Render(options, maxWidth);
		int lineBreaks = segments.Count(s => s.IsLineBreak);
		while (lineBreaks++ < 2)
			segments.Add(Segment.LineBreak);

		return segments;
	}
}
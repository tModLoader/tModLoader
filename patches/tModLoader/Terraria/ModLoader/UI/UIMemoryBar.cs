using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

internal class UIMemoryBar : UIElement
{
	private class MemoryBarItem
	{
		internal readonly string Tooltip;
		internal readonly long Memory;
		internal readonly Color DrawColor;

		public MemoryBarItem(string tooltip, long memory, Color drawColor)
		{
			Tooltip = tooltip;
			Memory = memory;
			DrawColor = drawColor;
		}
	}

	internal static bool RecalculateMemoryNeeded = true;

	private readonly List<MemoryBarItem> _memoryBarItems = new List<MemoryBarItem>();
	private long allocatedMemory; // Total process memory usage, serves as total width value of memory bar.

	public UIMemoryBar()
	{
		Width.Set(0f, 1f);
		Height.Set(20f, 0f);
	}

	public override void OnActivate()
	{
		base.OnActivate();
		// moved from constructor to avoid texture loading on JIT thread
		Show();
	}

	internal void Show()
	{
		RecalculateMemoryNeeded = true;
		Task.Run(RecalculateMemory);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (RecalculateMemoryNeeded) return;

		var rectangle = GetInnerDimensions().ToRectangle();

		var mouse = new Point(Main.mouseX, Main.mouseY);
		int xOffset = 0;
		bool drawHover = false;
		MemoryBarItem hoverData = null;

		for (int i = 0; i < _memoryBarItems.Count; i++) {
			var memoryBarData = _memoryBarItems[i];
			int width = (int)(rectangle.Width * (memoryBarData.Memory / (float)allocatedMemory));
			if (i == _memoryBarItems.Count - 1) { // Fix rounding errors on last entry for consistent right edge
				width = rectangle.Right - xOffset - rectangle.X;
			}
			var drawArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, width, rectangle.Height);
			xOffset += width;
			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawArea, memoryBarData.DrawColor);

			if (!drawHover && drawArea.Contains(mouse)) {
				drawHover = true;
				hoverData = memoryBarData;
			}
		}

		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Y, 2, rectangle.Height), Color.Black);
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, 2), Color.Black);
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X + rectangle.Width - 2, rectangle.Y, 2, rectangle.Height), Color.Black);
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - 2, rectangle.Width, 2), Color.Black);

		if (drawHover && hoverData != null) {
			UICommon.TooltipMouseText(hoverData.Tooltip);
		}
	}

	private readonly Color[] _colors = {
		new Color(232, 76, 61),//red
		new Color(155, 88, 181),//purple
		new Color(27, 188, 155),//aqua
		new Color(243, 156, 17),//orange
		new Color(45, 204, 112),//green
		new Color(241, 196, 15),//yellow
	};

	private void RecalculateMemory()
	{
		_memoryBarItems.Clear();

		long totalModMemory = 0;
		int i = 0;
		foreach (var entry in MemoryTracking.modMemoryUsageEstimates.OrderBy(v => -v.Value.total)) {
			var modName = entry.Key;
			var usage = entry.Value;
			if (usage.total <= 0 || modName == "ModLoader")
				continue;

			totalModMemory += usage.total;
			var sb = new StringBuilder();
			sb.Append(ModLoader.GetMod(modName).DisplayName);
			sb.Append($"\n {Language.GetTextValue("tModLoader.LastLoadRamUsage", SizeSuffix(usage.total))}");
			if (usage.managed > 0)
				sb.Append($"\n  {Language.GetTextValue("tModLoader.ManagedMemory", SizeSuffix(usage.managed))}");
			if (usage.code > 0)
				sb.Append($"\n  {Language.GetTextValue("tModLoader.CodeMemory", SizeSuffix(usage.code))}");
			if (usage.sounds > 0)
				sb.Append($"\n  {Language.GetTextValue("tModLoader.SoundMemory", SizeSuffix(usage.sounds))}");
			if (usage.textures > 0)
				sb.Append($"\n  {Language.GetTextValue("tModLoader.TextureMemory", SizeSuffix(usage.textures))}");
			_memoryBarItems.Add(new MemoryBarItem(sb.ToString(), usage.total, _colors[i++ % _colors.Length]));
		}

		Process process = Process.GetCurrentProcess();
		process.Refresh();
		allocatedMemory = process.PrivateMemorySize64; // Use this rather than cache a value in MemoryTracking.Finish due to OS taking time to free memory
		long nonModMemory = allocatedMemory - totalModMemory; // What we think tmod itself is using.
		_memoryBarItems.Add(new MemoryBarItem(
			$"{Language.GetTextValue("tModLoader.TerrariaMemory", SizeSuffix(nonModMemory))}\n {Language.GetTextValue("tModLoader.TotalMemory", SizeSuffix(allocatedMemory))}\n\n{Language.GetTextValue("tModLoader.InstalledMemory", SizeSuffix(GetTotalMemory()))}",
			nonModMemory, Color.DeepSkyBlue));

		/*
		var remainingMemory = availableMemory - allocatedMemory;
		_memoryBarItems.Add(new MemoryBarItem(
			$"{Language.GetTextValue("tModLoader.AvailableMemory", SizeSuffix(remainingMemory))}\n {Language.GetTextValue("tModLoader.TotalMemory", SizeSuffixavailableMemory))}",
			remainingMemory, Color.Gray));
		*/

		//portion = (maxMemory - availableMemory - meminuse) / (float)maxMemory;
		//memoryBarItems.Add(new MemoryBarData($"Other programs: {SizeSuffix(maxMemory - availableMemory - meminuse)}", portion, Color.Black));

		RecalculateMemoryNeeded = false;
	}

	private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

	// TODO: These are binary, not decimal. Add parameter to support both? Which should be which?
	internal static string SizeSuffix(long value, int decimalPlaces = 1)
	{
		if (value < 0) { return "-" + SizeSuffix(-value); }
		if (value == 0) { return "0.0 bytes"; }

		// mag is 0 for bytes, 1 for KB, 2, for MB, etc.
		int mag = (int)Math.Log(value, 1024);

		// 1L << (mag * 10) == 2 ^ (10 * mag)
		// [i.e. the number of bytes in the unit corresponding to mag]
		decimal adjustedSize = (decimal)value / (1L << (mag * 10));

		// make adjustment when the value is large enough that
		// it would round up to 1000 or more
		if (Math.Round(adjustedSize, decimalPlaces) >= 1000) {
			mag += 1;
			adjustedSize /= 1024;
		}

		return string.Format("{0:n" + decimalPlaces + "} {1}",
			adjustedSize,
			SizeSuffixes[mag]);
	}

	/*
	public static long GetAvailableMemory() {
		//TODO: Implement for all platforms
		if(Platform.IsWindows) {
			var pc = new PerformanceCounter("Mono Memory", "Available Physical Memory");
			return pc.RawValue;
		}
	}
	*/

	/// <summary> Returns total installed RAM </summary>
	public static long GetTotalMemory()
	{
		var gcMemInfo = GC.GetGCMemoryInfo();
		return gcMemInfo.TotalAvailableMemoryBytes;
	}

	/*
	[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

	private static bool IsWin64Emulator(Process process) {
		if ((Environment.OSVersion.Version.Major > 5)
			|| ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1))) {
			bool retVal;
			return IsWow64Process(process.Handle, out retVal) && retVal;
		}
		return false;
	}

	private long CalculateAvailableMemory(long availableMemory) {
		Process currentProcess = Process.GetCurrentProcess();
		foreach (var p in Process.GetProcesses()) {
			try {
				if (IsWin64Emulator(p)) {
					availableMemory -= (p.WorkingSet64);
				}
			}
			catch (Win32Exception ex) {
				if (ex.NativeErrorCode != 0x00000005) {
					//throw;
				}
			}
		}
		return Math.Max(0, availableMemory);
	}
	*/
}

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
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIMemoryBar : UIElement
	{
		private class MemoryBarItem
		{
			internal readonly string Tooltip;
			internal readonly long Memory;
			internal readonly Color DrawColor;

			public MemoryBarItem(string tooltip, long memory, Color drawColor) {
				Tooltip = tooltip;
				Memory = memory;
				DrawColor = drawColor;
			}
		}

		internal static bool RecalculateMemoryNeeded = true;

		private readonly List<MemoryBarItem> _memoryBarItems = new List<MemoryBarItem>();
		private UIPanel _hoverPanel;
		private long _maxMemory; //maximum memory Terraria could allocate before crashing if it was the only process on the system

		public override void OnInitialize() {
			Width.Set(0f, 1f);
			Height.Set(20f, 0f);
		}

		public override void OnActivate() {
			base.OnActivate();
			// moved from constructor to avoid texture loading on JIT thread
			RecalculateMemoryNeeded = true;
			Task.Run(RecalculateMemory);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (RecalculateMemoryNeeded) return;

			var rectangle = GetInnerDimensions().ToRectangle();

			var mouse = new Point(Main.mouseX, Main.mouseY);
			int xOffset = 0;
			bool drawHover = false;
			Rectangle hoverRect = Rectangle.Empty;
			MemoryBarItem hoverData = null;

			for (int i = 0; i < _memoryBarItems.Count; i++) {
				var memoryBarData = _memoryBarItems[i];
				int width = (int)(rectangle.Width * (memoryBarData.Memory / (float)_maxMemory));
				if (i == _memoryBarItems.Count - 1) { // Fix rounding errors on last entry for consistent right edge
					width = rectangle.Right - xOffset - rectangle.X;
				}
				var drawArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, width, rectangle.Height);
				xOffset += width;
				Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawArea, memoryBarData.DrawColor);

				if (!drawHover && drawArea.Contains(mouse)) {
					Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(memoryBarData.Tooltip);
					float x = stringSize.X;
					Vector2 vector = Main.MouseScreen + new Vector2(16f);
					vector.Y = Math.Min(vector.Y, Main.screenHeight - 30);
					vector.X = Math.Min(vector.X, Parent.GetDimensions().Width + Parent.GetDimensions().X - x - 40);
					var r = new Rectangle((int)vector.X, (int)vector.Y, (int)x, (int)stringSize.Y);
					r.Inflate(5, 5);
					drawHover = true;
					hoverRect = r;
					hoverData = memoryBarData;
				}
			}

			if (drawHover && hoverData != null) {
				_hoverPanel.Width.Set(hoverRect.Width + 10, 0);
				_hoverPanel.Height.Set(hoverRect.Height + 5, 0);
				_hoverPanel.Top.Set(hoverRect.Y - 10, 0);
				_hoverPanel.Left.Set(hoverRect.X - 8, 0);
				_hoverPanel.Recalculate();
				_hoverPanel.Draw(spriteBatch);

				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, hoverData.Tooltip, hoverRect.X + 5, hoverRect.Y + 2, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
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

		private void RecalculateMemory() {
			_memoryBarItems.Clear();
			_hoverPanel = new UIPanel();
			_hoverPanel.Activate();

#if WINDOWS //TODO: 64bit?
			_maxMemory = Environment.Is64BitOperatingSystem ? 4294967296 : 3221225472;
			long availableMemory = _maxMemory; // CalculateAvailableMemory(maxMemory); This is wrong, 4GB is not shared.
#else
			_maxMemory = GetTotalMemory();
			long availableMemory = _maxMemory; //This is wrong; this is assuming tML is the only thing running. Can't find an alternative, but is less likely to confuse users under current design
#endif

			long totalModMemory = 0;
			int i = 0;
			foreach (var entry in MemoryTracking.modMemoryUsageEstimates.OrderBy(v => -v.Value.total)) {
				var modName = entry.Key;
				var usage = entry.Value;
				if (usage.total <= 0 || modName == "tModLoader")
					continue;

				totalModMemory += usage.total;
				var sb = new StringBuilder();
				sb.Append(ModLoader.GetMod(modName).DisplayName);
				sb.Append($"\nEstimate last load RAM usage: {SizeSuffix(usage.total)}");
				if (usage.managed > 0)
					sb.Append($"\n Managed: {SizeSuffix(usage.managed)}");
				if (usage.managed > 0)
					sb.Append($"\n Code: {SizeSuffix(usage.code)}");
				if (usage.sounds > 0)
					sb.Append($"\n Sounds: {SizeSuffix(usage.sounds)}");
				if (usage.textures > 0)
					sb.Append($"\n Textures: {SizeSuffix(usage.textures)}");
				_memoryBarItems.Add(new MemoryBarItem(sb.ToString(), usage.total, _colors[i++ % _colors.Length]));
			}

			long allocatedMemory = Process.GetCurrentProcess().WorkingSet64;
			var nonModMemory = allocatedMemory - totalModMemory;
			_memoryBarItems.Add(new MemoryBarItem(
				$"Terraria + misc: {SizeSuffix(nonModMemory)}\n Total: {SizeSuffix(allocatedMemory)}",
				nonModMemory, Color.DeepSkyBlue));

			var remainingMemory = availableMemory - allocatedMemory;
			_memoryBarItems.Add(new MemoryBarItem(
				$"Available Memory: {SizeSuffix(remainingMemory)}\n Total: {SizeSuffix(availableMemory)}",
				remainingMemory, Color.Gray));

			//portion = (maxMemory - availableMemory - meminuse) / (float)maxMemory;
			//memoryBarItems.Add(new MemoryBarData($"Other programs: {SizeSuffix(maxMemory - availableMemory - meminuse)}", portion, Color.Black));

			RecalculateMemoryNeeded = false;
		}

		private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		internal static string SizeSuffix(long value, int decimalPlaces = 1) {
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

		public static long GetTotalMemory() {
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
}

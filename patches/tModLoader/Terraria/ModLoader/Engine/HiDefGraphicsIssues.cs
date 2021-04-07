using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Engine
{
	/// <summary>
	/// XNA makes a distinction between graphics device resets and creations.
	/// Calls to <code>GraphicsDevice.Reset</code> via <code>GraphicsDeviceManager.ApplyChanges</code> trigger a device reset on screen resolution and other changes.
	/// When a device is reset, all textures are re-uploaded from native copies stored in RAM, and the game continues to run with valid handles.
	/// When changing graphics profile, the device must be re-created, and all graphics resources must be re-initialized manually.
	/// 
	/// Sometimes (we haven't been able to track the actual cause) seemingly innocuous resets with HiDef graphics profile, fail in XNA native code, and GraphicsDevice.Reset falls back to a full recreation.
	/// XNA includes this fallback try-catch path, implying that this is a known or expected issue, however tML (and Terraria) are 
	/// not equipped to handle re-acquiring all graphics resources at any point in the load cycle.
	/// 
	/// This failed reset manifests as a silently caught InvalidOperationException, and then later causes an engine crash, normally via an ObjectDisposedException.
	/// The graphics device does not fail the initial switch to HiDef, and for most users, only fails some portion of the time (a heisenbug).
	/// 
	/// The HiDef graphics profile is normally only enabled with the "Support4K" configuration flag, and on monitors with display resolutions above 1080p
	/// However, due to some advanced shader features available on HiDef, there has been request for tML to enable HiDef graphics by default where possible.
	/// </summary>
	internal static class HiDefGraphicsIssues
	{
		private abstract class DeviceParam
		{
			public readonly string name;

			public DeviceParam(string name) {
				this.name = name;
			}

			public abstract void LogChange(GraphicsDevice g, StringBuilder sb, bool creating);
		}

		private class DeviceParam<T> : DeviceParam
		{
			private readonly Func<GraphicsDevice, T> getter;
			private readonly Func<T, string> getDescription;
			private T value;

			private string Desc => getDescription?.Invoke(value) ?? value.ToString();

			public DeviceParam(string name, Func<GraphicsDevice, T> getter, Func<T, string> getDescription = null) : base(name) {
				this.getter = getter;
				this.getDescription = getDescription;
			}

			public override void LogChange(GraphicsDevice g, StringBuilder changes, bool creating) {
				if (creating)
					value = getter(g);

				changes.Append(", ").Append(name).Append(": ").Append(Desc);

				var newValue = getter(g);
				if (!EqualityComparer<T>.Default.Equals(value, newValue)) {
					value = newValue;
					changes.Append(" -> ").Append(Desc);
				}
			}
		}

		private static List<DeviceParam> Params = new List<DeviceParam>() {
			new DeviceParam<GraphicsAdapter>("Adapter", g => g.Adapter, a => a.Description),
			new DeviceParam<DisplayMode>("DisplayMode", g => g.Adapter.CurrentDisplayMode),
			new DeviceParam<GraphicsProfile>("Profile", g => g.GraphicsProfile),
			new DeviceParam<int>("Width", g => g.PresentationParameters.BackBufferWidth),
			new DeviceParam<int>("Height", g => g.PresentationParameters.BackBufferHeight),
			new DeviceParam<bool>("Fullscreen", g => g.PresentationParameters.IsFullScreen),
			new DeviceParam<string>("Display", g => g.Adapter.DeviceName)
		};

		private static int mainThreadId;

		public static void Init() {
			Main.graphics.DeviceCreated += (s, e) => GLCallLocker.Init();

			Main.graphics.DeviceReset += LogDeviceReset;
			//Main.graphics.DeviceReset += UpdateBackbufferSizes;
			Main.graphics.DeviceCreated += (s, e) => creating = true;

			//var clientSizeChangedEventInfo = typeof(GameWindow).GetField("ClientSizeChanged", BindingFlags.NonPublic | BindingFlags.Instance);
			//clientSizeChangedEventInfo.SetValue(Main.instance.Window, null);
#if XNA && DEBUG
	        mainThreadId = Thread.CurrentThread.ManagedThreadId;
	        new Hook(typeof(Texture2D).FindMethod("ValidateCreationParameters"), new hook_ValidateCreationParameters(HookValidateCreationParameters));
#endif
		}

		// Main.SetDisplayMode runs every frame and if it detects a display size which doesn't match the PreferredBackBufferWidth/Height then it triggers a device reset via ApplyChanges
		// If the window is resized, the backbuffer sizes are updated, but the PreferredBackBufferWidth/Height fields on GraphicsDeviceManager are not, so Terraria thinks it needs to update them and reset the device.

		private static FieldInfo INTERNAL_preferredBackBufferWidth = typeof(GraphicsDeviceManager).GetField("INTERNAL_preferredBackBufferWidth", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo INTERNAL_preferredBackBufferHeight = typeof(GraphicsDeviceManager).GetField("INTERNAL_preferredBackBufferHeight", BindingFlags.Instance | BindingFlags.NonPublic);
		private static void UpdateBackbufferSizes(object sender, EventArgs e) {
			//INTERNAL_preferredBackBufferWidth.SetValue(Main.graphics, ((GraphicsDevice)sender).PresentationParameters.BackBufferWidth);
			//INTERNAL_preferredBackBufferHeight.SetValue(Main.graphics, ((GraphicsDevice)sender).PresentationParameters.BackBufferHeight);
		} 

		private static bool creating;

		private static void LogDeviceReset(object sender, EventArgs e) {
			var g = (GraphicsDevice)sender;

			var sb = new StringBuilder($"Device {(creating ? "Created" : "Reset")}");
			foreach (var param in Params)
				param.LogChange(g, sb, creating);

			Logging.Terraria.Debug(sb);
			creating = false;
		}
			    
		private delegate void orig_ValidateCreationParameters(object profile, int width, int height, SurfaceFormat format, [MarshalAs(UnmanagedType.U1)] bool mipMap);
		private delegate void hook_ValidateCreationParameters(orig_ValidateCreationParameters orig, object profile, int width, int height, SurfaceFormat format, [MarshalAs(UnmanagedType.U1)] bool mipMap);
		private static void HookValidateCreationParameters(orig_ValidateCreationParameters orig, object profile, int width, int height, SurfaceFormat format, [MarshalAs(UnmanagedType.U1)] bool mipMap) {
			if (!Program.LoadedEverything && Thread.CurrentThread.ManagedThreadId != mainThreadId)
				throw new Exception("Texture created on worker thread before graphics device is finalised");
			
			orig(profile, width, height, format, mipMap);
		}
    }
}
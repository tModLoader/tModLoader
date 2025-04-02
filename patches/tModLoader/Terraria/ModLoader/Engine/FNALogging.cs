using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terraria.ModLoader.Engine;

/// <summary>
/// Attempt to track spurious device resets, backbuffer flickers and resizes
/// Also setup some FNA logging
/// </summary>
internal static class FNALogging
{
	private abstract class DeviceParam
	{
		public readonly string name;

		public DeviceParam(string name)
		{
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

		public DeviceParam(string name, Func<GraphicsDevice, T> getter, Func<T, string> getDescription = null) : base(name)
		{
			this.getter = getter;
			this.getDescription = getDescription;
		}

		public override void LogChange(GraphicsDevice g, StringBuilder changes, bool creating)
		{
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
	public static string DriverIdentifier { get; internal set; } // note, Metal will be "Metal\nDevice Name: %s"

	internal static void RedirectLogs()
	{
		FNALoggerEXT.LogInfo = (s) => {
			if (DriverIdentifier == null && s.StartsWith("FNA3D Driver: ")) {
				DriverIdentifier = s.Substring("FNA3D Driver: ".Length);

				Logging.FNA.Info($"SDL Video Diver: {SDL2.SDL.SDL_GetCurrentVideoDriver()}");
			}

			Logging.FNA.Info(s);
		};

		// in 2 lines so you can breakpoint it
		FNALoggerEXT.LogWarn = (string s) =>
			Logging.FNA.Warn(s);
		FNALoggerEXT.LogError = (string s) =>
			Logging.FNA.Error(s);

		Logging.FNA.Debug("Querying linked library versions...");

		SDL2.SDL.SDL_GetVersion(out var sdl_version);
		Logging.FNA.Debug($"SDL v{sdl_version.major}.{sdl_version.minor}.{sdl_version.patch}");

		uint fna3d_version = FNA3D.FNA3D_LinkedVersion();
		Logging.FNA.Debug($"FNA3D v{fna3d_version / 10000}.{fna3d_version / 100 % 100}.{fna3d_version % 100}");

		uint faudio_version = FAudio.FAudioLinkedVersion();
		Logging.FNA.Debug($"FAudio v{faudio_version / 10000}.{faudio_version / 100 % 100}.{faudio_version % 100}");
	}

	public static void GraphicsInit(GraphicsDeviceManager graphics)
	{
		graphics.DeviceReset += LogDeviceReset;
		//Main.graphics.DeviceReset += UpdateBackbufferSizes;
		graphics.DeviceCreated += (s, e) => creating = true;

		//var clientSizeChangedEventInfo = typeof(GameWindow).GetField("ClientSizeChanged", BindingFlags.NonPublic | BindingFlags.Instance);
		//clientSizeChangedEventInfo.SetValue(Main.instance.Window, null);
	}

	private static bool creating;

	private static void LogDeviceReset(object sender, EventArgs e)
	{
		var graphicsDeviceManager = (GraphicsDeviceManager)sender;
		var graphicsDevice = graphicsDeviceManager.GraphicsDevice;

		var sb = new StringBuilder($"Device {(creating ? "Created" : "Reset")}");
		foreach (var param in Params)
			param.LogChange(graphicsDevice, sb, creating);

		Logging.Terraria.Debug(sb);
		creating = false;
	}

	internal static void PostAudioInit()
	{
		Logging.FNA.Info($"SDL Audio Driver: {SDL2.SDL.SDL_GetCurrentAudioDriver()}");
	}
}
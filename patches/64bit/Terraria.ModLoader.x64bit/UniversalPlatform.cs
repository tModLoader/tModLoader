using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Localization.IME;
using ReLogic.OS;
using static Terraria.Utilities.PlatformUtilities;

namespace Terraria.ModLoader.Core
{
	class UniversalPlatform : Platform
	{

		protected override string GetClipboard() {
			switch (RunningPlatform()) {
				case PlatformID.MacOSX:
					try {
						string result = default;
						using (Process process = new Process()) {
							process.StartInfo = new ProcessStartInfo("pbpaste", "-pboard general") {
								UseShellExecute = false,
								RedirectStandardOutput = true
							};

							process.Start();
							result = process.StandardOutput.ReadToEnd();
							process.WaitForExit();
						}

						return result;
					}
					catch (Exception) {
						return "";
					}
				case PlatformID.Unix:
					try {
						string result = default;
						using (Process process = new Process()) {
							process.StartInfo = new ProcessStartInfo("xsel", "-o") {
								UseShellExecute = false,
								RedirectStandardOutput = true
							};

							process.Start();
							result = process.StandardOutput.ReadToEnd();
							process.WaitForExit();
						}

						return result;
					}
					catch (Exception) {
						return "";
					}

				default:
					return InvokeInStaThread(() => System.Windows.Forms.Clipboard.GetText());
			}

		}

		protected override void SetClipboard(string text) {
			switch (RunningPlatform()) {
				case PlatformID.MacOSX:
					try {
						using (Process process = new Process()) {
							process.StartInfo = new ProcessStartInfo("pbcopy", "-pboard general -Prefer txt") {
								UseShellExecute = false,
								RedirectStandardOutput = false,
								RedirectStandardInput = true
							};

							process.Start();
							process.StandardInput.Write(text);
							process.StandardInput.Close();
							process.WaitForExit();
						}
					}
					catch (Exception) {
					}
					break;
				case PlatformID.Unix:
					if (text == "") {
						ClearClipboard();
					}
					else {
						try {
							using (Process process = new Process()) {
								process.StartInfo = new ProcessStartInfo("xsel", "-i") {
									UseShellExecute = false,
									RedirectStandardOutput = false,
									RedirectStandardInput = true
								};

								process.Start();
								process.StandardInput.Write(text);
								process.StandardInput.Close();
								process.WaitForExit();
							}
						}
						catch (Exception) {
						}
					}

					break;
				default:
					if (text != null && text != "") {
						InvokeInStaThread(delegate {
							System.Windows.Forms.Clipboard.SetText(text);
						});
					}
					break;
			}

		}

		private void ClearClipboard() {
			
			try {
				using (Process process = new Process()) {
					process.StartInfo = new ProcessStartInfo("xsel", "-c") {
						UseShellExecute = false,
						RedirectStandardOutput = false,
						RedirectStandardInput = true
					};

					process.Start();
					process.WaitForExit();
				}
			}
			catch (Exception) {
			}
		}

		private T InvokeInStaThread<T>(Func<T> callback) {
			if (GetApartmentStateSafely() == ApartmentState.STA) {
				return callback();
			}

			T result = default;
			Thread thread = new Thread((ThreadStart)delegate {
				result = callback();
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			return result;
		}

		private void InvokeInStaThread(Action callback) {
			if (GetApartmentStateSafely() == ApartmentState.STA) {
				callback();
			}
			else {
				Thread thread = new Thread((ThreadStart)delegate {
					callback();
				});

				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
			}
		}

		private ApartmentState GetApartmentStateSafely() {
			try {
				return Thread.CurrentThread.GetApartmentState();
			}
			catch {
				return ApartmentState.MTA;
			}
		}

		// TODO: Move all switch case body into it's own method
		public override string GetStoragePath() {
			switch (RunningPlatform()) {
				case PlatformID.MacOSX:
					return MacStoragePath();
				case PlatformID.Unix:
					return LinuxStoragePath();
				default:
					return WindowsStoragePath();
			}
		}

		private static string WindowsStoragePath() {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games");
		}

		private static string LinuxStoragePath() {
			string text = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
			if (string.IsNullOrEmpty(text)) {
				text = Environment.GetEnvironmentVariable("HOME");
				if (string.IsNullOrEmpty(text)) {
					return ".";
				}

				text += "/.local/share";
			}

			return text;
		}

		private static string MacStoragePath() {
			string environmentVariable = Environment.GetEnvironmentVariable("HOME");
			if (string.IsNullOrEmpty(environmentVariable)) {
				return ".";
			}

			return environmentVariable + "/Library/Application Support";
		}


		protected override PlatformIme CreateIme(IntPtr windowHandle) {
			return (PlatformIme)Activator.CreateInstance(typeof(ReLogic.Localization.IME.PlatformIme).Assembly.GetType("ReLogic.Localization.IME.FnaIme"));
		}

		public UniversalPlatform(PlatformType type) : base(type) {
		}
	}
}

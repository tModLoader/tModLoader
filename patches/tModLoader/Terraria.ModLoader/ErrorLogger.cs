using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Terraria.ModLoader
{
	/// <summary>
	/// NOTE: This class is deprecated. Use <see cref="Terraria.ModLoader.Logging"/> instead (see ExampleMod for example)
	/// This class consists of functions that write error messages to text files for you to read. It also lets you write logs to text files.
	/// </summary>
	[Obsolete("This class is deprecated. Use Terraria.ModLoader.Logging instead (see ExampleMod for example)", false)]
	public static class ErrorLogger
	{
		/// <summary>
		/// NOTE: Deprecated. Use <see cref="Terraria.ModLoader.Logging.LogDir"/> instead
		/// The file path to which logs are written and stored.
		/// </summary>
		[Obsolete("Please use Terraria.ModLoader.Logging.LogDir instead", false)]
		public static readonly string LogPath = Logging.LogDir;

		private static Object logExceptionLock = new Object();

		private static Object logLock = new Object();
		/// <summary>
		/// NOTE: Deprecated. Please use your own ILog instead, see ExampleMod for an example
		/// You can use this method for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder.
		/// </summary>
		[Obsolete("Please use your own ILog instead, see ExampleMod for an example", false)]
		public static void Log(string message) {
			string callerName = GetCallerName();
			Mod callerMod = ModLoader.GetMod(callerName);
			if (callerMod == null) {
				Logging.tML.WarnFormat("Tried to forward ErrorLogger.Log for mod {0} but failed (mod not found!)", callerName);
			}
			else {
				callerMod.Logger.Info(message);
			}
		}

		/// <summary>
		/// NOTE: Deprecated. Please use your own ILog instead, see ExampleMod for an example
		/// Allows you to log an object for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder. 
		/// </summary>
		/// <param name="param">The object to be logged.</param>
		/// <param name="alternateOutput">If true, the object's data will be manually retrieved and logged. If false, the object's ToString method is logged.</param>
		[Obsolete("Please use your own ILog instead, see ExampleMod for an example", false)]
		public static void Log(object param, bool alternateOutput = false) {
			string getParamString() {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Object type: " + param.GetType());
				foreach (PropertyInfo property in param.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					sb.AppendLine("PROPERTY " + property.Name + " = " + property.GetValue(param, null) + "\n");
				}

				foreach (FieldInfo field in param.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					sb.AppendLine("FIELD " + field.Name + " = " + (field.GetValue(param).ToString() != "" ? field.GetValue(param) : "(Field value not found)") + "\n");
				}

				foreach (MethodInfo method in param.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					sb.AppendLine("METHOD " + method.Name + "\n");
				}

				int temp = 0;

				foreach (ConstructorInfo constructor in param.GetType().GetConstructors(BindingFlags.Public | BindingFlags.NonPublic)) {
					temp++;
					sb.AppendLine("CONSTRUCTOR " + temp + " : " + constructor.Name + "\n");
				}

				return sb.ToString();
			}

			string callerName = GetCallerName();
			Mod callerMod = ModLoader.GetMod(callerName);
			if (callerMod == null) {
				Logging.tML.WarnFormat("Tried to forward ErrorLogger.Log for mod {0} but failed (mod not found!)", callerName);
			}
			else {
				callerMod.Logger.Info(!alternateOutput ? param.ToString() : getParamString());
			}
		}

		private static string GetCallerName() {
			StackTrace stackTrace = new StackTrace();
			Assembly asm = stackTrace.GetFrame(2).GetMethod().DeclaringType.Assembly;
			string name = asm.GetName().Name;
			return name.Contains("_") ? name.Split('_')[0] : name;
		}

		/// <summary>
		/// NOTE: Deprecated.
		/// Deletes all log files.
		/// </summary>
		[Obsolete("Please ue Terraria.ModLoader.Logging instead", false)]
		public static void ClearLogs() {
			lock (logLock) {
				if (!Directory.Exists(Logging.LogDir))
					Directory.CreateDirectory(Logging.LogDir);

				string[] files = new string[] {
					LogPath + Path.DirectorySeparatorChar + "Logs.txt",
					LogPath + Path.DirectorySeparatorChar + "Network Error.txt",
					LogPath + Path.DirectorySeparatorChar + "Runtime Error.txt",
				};
				foreach (var file in files) {
					try {
						File.Delete(file);
					}
					catch (Exception) {
						// Don't worry about it, modder or player might have the file open in tail or notepad.
					}
				}
			}
		}
	}
}

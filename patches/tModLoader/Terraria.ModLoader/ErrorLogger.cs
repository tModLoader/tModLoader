using System;
using System.IO;
using System.Reflection;

namespace Terraria.ModLoader
{
	/// <summary>
	/// NOTE: This class is deprecated. Use <see cref="Terraria.ModLoader.Logging"/> instead (see ExampleMod for example)
	/// This class consists of functions that write error messages to text files for you to read. It also lets you write logs to text files.
	/// </summary>
	[Obsolete("This class is deprecated. Use Terraria.ModLoader.Logging instead (see ExampleMod for example)", true)]
	public static class ErrorLogger
	{
		/// <summary>
		/// NOTE: Deprecated. Use <see cref="Terraria.ModLoader.Logging.LogDir"/> instead
		/// The file path to which logs are written and stored.
		/// </summary>
		[Obsolete("Please use Terraria.ModLoader.Logging.LogDir instead")]
		public static readonly string LogPath = Logging.LogDir;

		private static Object logExceptionLock = new Object();

		private static Object logLock = new Object();
		/// <summary>
		/// NOTE: Deprecated. Please use your own ILog instead, see ExampleMod for an example
		/// You can use this method for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder.
		/// </summary>
		[Obsolete("Please use your own ILog instead, see ExampleMod for an example")]
		public static void Log(string message)
		{
			lock (logLock)
			{
				Directory.CreateDirectory(LogPath);
				using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
				{
					writer.WriteLine(message);
				}
			}
		}

		/// <summary>
		/// NOTE: Deprecated. Please use your own ILog instead, see ExampleMod for an example
		/// Allows you to log an object for your own testing purposes. The message will be added to the Logs.txt file in the Logs folder. 
		/// </summary>
		/// <param name="param">The object to be logged.</param>
		/// <param name="alternateOutput">If true, the object's data will be manually retrieved and logged. If false, the object's ToString method is logged.</param>
		[Obsolete("Please use your own ILog instead, see ExampleMod for an example")]
		public static void Log(object param, bool alternateOutput = false)
		{
			lock (logLock)
			{
				Directory.CreateDirectory(LogPath);
				using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt"))
				{
					if (!alternateOutput)
					{
						writer.WriteLine(param.ToString());
					}
					else
					{
						writer.WriteLine("Object type: " + param.GetType());
						foreach (PropertyInfo property in param.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
						{
							writer.Write("PROPERTY " + property.Name + " = " + property.GetValue(param, null) + "\n");
						}

						foreach (FieldInfo field in param.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
						{
							writer.Write("FIELD " + field.Name + " = " + (field.GetValue(param).ToString() != "" ? field.GetValue(param) : "(Field value not found)") + "\n");
						}

						foreach (MethodInfo method in param.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
						{
							writer.Write("METHOD " + method.Name + "\n");
						}

						int temp = 0;

						foreach (ConstructorInfo constructor in param.GetType().GetConstructors(BindingFlags.Public | BindingFlags.NonPublic))
						{
							temp++;
							writer.Write("CONSTRUCTOR " + temp + " : " + constructor.Name + "\n");
						}
					}
				}
			}
		}

		/// <summary>
		/// NOTE: Deprecated.
		/// Deletes all log files.
		/// </summary>
		[Obsolete("Please ue Terraria.ModLoader.Logging instead")]
		public static void ClearLogs()
		{
			lock (logLock)
			{
				if (!Directory.Exists(Logging.LogDir))
					Directory.CreateDirectory(Logging.LogDir);

				string[] files = new string[] {
					LogPath + Path.DirectorySeparatorChar + "Logs.txt",
					LogPath + Path.DirectorySeparatorChar + "Network Error.txt",
					LogPath + Path.DirectorySeparatorChar + "Runtime Error.txt",
				};
				foreach (var file in files)
				{
					try
					{
						File.Delete(file);
					}
					catch (Exception)
					{
						// Don't worry about it, modder or player might have the file open in tail or notepad.
					}
				}
			}
		}
	}
}

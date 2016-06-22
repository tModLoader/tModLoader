import java.io.*;

public class LinuxInfo
{
    public static void main(String[] args)
    {
        String path = System.getenv("XDG_DATA_HOME");
        File directory = null;
        if (path != null)
        {
            directory = getInstallDir(path, false);
        }
        if (directory == null || !directory.exists())
        {
            path = System.getenv("HOME");
            if (path != null)
            {
                directory = getInstallDir(path, true);
            }
        }
        String[] files = new String[]
        {
            "Terraria.exe",
            "ModCompile/tModLoaderWindows.exe",
            "tModLoaderServer.exe",
            "MP3Sharp.dll",
            "Ionic.Zip.Reduced.dll",
            "Mono.Cecil.dll",
            "Terraria.exe.config",
            "ModCompile/Microsoft.Xna.Framework.dll",
            "ModCompile/Microsoft.Xna.Framework.Game.dll",
            "ModCompile/Microsoft.Xna.Framework.Graphics.dll",
            "ModCompile/Microsoft.Xna.Framework.Xact.dll"
        };
        Installer.tryInstall(files, directory);
    }

    private static File getInstallDir(String homeDir, boolean normalHome)
    {
        File file = new File(homeDir);
        if (normalHome)
        {
            file = new File(file, ".local");
            file = new File(file, "share");
        }
        file = new File(file, "Steam");
        file = new File(file, "steamapps");
        file = new File(file, "common");
        file = new File(file, "Terraria");
        return file;
    }
}

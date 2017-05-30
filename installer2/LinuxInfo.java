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
            "tModLoaderServer",
            "tModLoaderServer.bin.x86",
            "tModLoaderServer.bin.x86_64",
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
        File installDir = new File(homeDir);
        File newSteamDir;
        if (normalHome)
        {
            newSteamDir = new File(installDir, ".steam");
            if (newSteamDir.isDirectory())
            {
                installDir = newSteamDir;
                installDir = new File(installDir, "steam");
            }
            else
            {
                installDir = new File(installDir, ".local");
                installDir = new File(installDir, "share");
                installDir = new File(installDir, "Steam");
            }
        }
        else
        {
            installDir = new File(installDir, "Steam");
        }
        installDir = new File(installDir, "steamapps");
        installDir = new File(installDir, "common");
        installDir = new File(installDir, "Terraria");
        return installDir;
    }
}

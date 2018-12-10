import java.io.*;

public class MacInfo
{
    public static void main(String[] args)
    {
        String path = System.getenv("HOME");
        File directory = null;
        if (path != null)
        {
            directory = getInstallDir(path);
        }
        String[] files = new String[]
        {
            "Terraria.exe",
            "ModCompile/tModLoaderWindows.exe",
            "tModLoaderServer.exe",
            "tModLoaderServer",
            "tModLoaderServer.bin.osx",
            "MP3Sharp.dll",
            "Ionic.Zip.Reduced.dll",
            "Mono.Cecil.dll",
            "Terraria.exe.config",
            "ModCompile/Microsoft.Xna.Framework.dll",
            "ModCompile/Microsoft.Xna.Framework.Game.dll",
            "ModCompile/Microsoft.Xna.Framework.Graphics.dll",
            "ModCompile/Microsoft.Xna.Framework.Xact.dll",
            "mono/config"
        };
        Installer.tryInstall(files, directory);
        
        new File(directory, "tModLoaderServer").setExecutable(true, false);
        new File(directory, "tModLoaderServer.bin.osx").setExecutable(true, false);
    }

    private static File getInstallDir(String homeDir)
    {
        File file = new File(homeDir, "Library");
        file = new File(file, "Application Support");
        file = new File(file, "Steam");
        file = new File(file, "steamapps");
        file = new File(file, "common");
        file = new File(file, "Terraria");
        file = new File(file, "Terraria.app");
        file = new File(file, "Contents");
        file = new File(file, "MacOS");
        return file;
    }
}

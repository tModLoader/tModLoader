import java.io.*;

public class LinuxInfo
{
    public static void main(String[] args)
    {
        File directory = getInstallDir();
        
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
        
        new File(directory, "tModLoaderServer").setExecutable(true, false);
        new File(directory, "tModLoaderServer.bin.x86").setExecutable(true, false);
        new File(directory, "tModLoaderServer.bin.x86_64").setExecutable(true, false);
    }

    private static File getInstallDir()
    {
        File installDir;

        String xdgHome = System.getenv("XDG_DATA_HOME");
        if (xdgHome != null)
        {
            installDir = new File(xdgHome + "/Steam/steamapps/common/Terraria");
            if (installDir.isDirectory())
            {
                return installDir;
            }
        }

        String home = System.getenv("HOME");
        if (home != null)
        {
            installDir = new File(home + "/.local/share/Steam/steamapps/common/Terraria");
            if (installDir.isDirectory())
            {
                return installDir;
            }

            installDir = new File(home + "/.steam/steam/steamapps/common/Terraria");
            if (installDir.isDirectory())
            {
                return installDir;
            }
        }

        return null;
    }
}

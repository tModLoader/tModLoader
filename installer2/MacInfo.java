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
            "tModLoaderServer.exe",
            "tModLoaderServer",
            "Terraria.exe.config",
            "mono/config"
        };
        Installer.tryInstall(files, directory);
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

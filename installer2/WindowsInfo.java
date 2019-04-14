import java.io.*;

public class WindowsInfo
{
    public static void main(String[] args)
    {
        String path = System.getenv("ProgramFiles(x86)");
        File directory = null;
        if (path != null)
        {
            directory = getInstallDir(path);
        }
        if (directory == null || !directory.exists())
        {
            path = System.getenv("ProgramFiles");
            if (path != null)
            {
                directory = getInstallDir(path);
            }
        }
        String[] files = new String[]
        {
            "Terraria.exe",
            "tModLoaderServer.exe",
            "start-tModLoaderServer.bat",
            "start-tModLoaderServer-steam-friends.bat",
            "start-tModLoaderServer-steam-private.bat"
        };
        String[] filesToDelete = new String[]
        {
        };
        Installer.tryInstall(files, filesToDelete, directory, true);
    }

    private static File getInstallDir(String programFileDir)
    {
        File file = new File(programFileDir, "Steam");
        file = new File(file, "steamapps");
        file = new File(file, "common");
        file = new File(file, "Terraria");
        return file;
    }
}

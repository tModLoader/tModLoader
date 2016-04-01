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
            "ModCompile/tModLoaderMac.exe",
            "tModLoaderServer.exe",
            "ModCompile/FNA.dll",
            "ModCompile/Microsoft.CodeAnalysis.dll",
            "ModCompile/Microsoft.CodeAnalysis.CSharp.dll",
            "ModCompile/Mono.Cecil.Pdb.dll",
            "ModCompile/System.Reflection.Metadata.dll",
            "ModCompile/RoslynWrapper.dll",
            "start-tModLoaderServer.bat",
            "start-tModLoaderServer-steam-friends.bat",
            "start-tModLoaderServer-steam-private.bat"
        };
        Installer.tryInstall(files, directory);
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

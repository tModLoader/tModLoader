import java.io.File;

public class MacInfo {
    public static void main(String[] args) {
        String path = System.getenv("HOME");
        File directory = null;
        if (path != null) {
            directory = getInstallDir(path);
        }
        String[] files = new String[]
                {
                        "tModLoader.exe",
                        "tModLoaderServer.exe",
                        "tModLoaderServer",
                        "Terraria",
                        "tModLoader",
                        "tModLoader-kick",
                        "tModLoader-mono",
                        "I18N.dll",
                        "I18N.West.dll",
                        "libMonoPosixHelper.dylib"
                };
        String[] filesToDelete = new String[]
                {
                        "Terraria.exe.config",
                        "mono/config",
                        "MP3Sharp.dll",
                        "Ionic.Zip.Reduced.dll",
                        "Mono.Cecil.dll"
                };
        Installer.tryInstall(files, filesToDelete, directory, false);
    }

    private static File getInstallDir(String homeDir) {
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

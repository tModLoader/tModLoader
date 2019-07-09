import java.io.File;
import java.lang.reflect.InvocationTargetException;

public class WindowsInfo {
    public static void main(String[] args) {
        final File directory = getTerrariaDir();
        String[] files = new String[]
                {
                        "Terraria.exe",
                        "tModLoaderServer.exe",
                        "start-tModLoaderServer.bat",
                        "start-tModLoaderServer-steam-friends.bat",
                        "start-tModLoaderServer-steam-private.bat"
                };
        String[] filesToDelete = new String[]{};
        Installer.tryInstall(files, filesToDelete, directory, true);
    }

    private static File getTerrariaDir() {
        try {
            final String prefs = getInstallDirFromRegistry();
            if (prefs != null) {
                return new File(prefs);
            }
        } catch (Exception ignored) {
        }

        String path = System.getenv("ProgramFiles(x86)");
        File directory = null;
        if (path != null) {
            directory = getInstallDir(path);
        }
        if (directory == null || !directory.exists()) {
            path = System.getenv("ProgramFiles");
            if (path != null) {
                directory = getInstallDir(path);
            }
        }
        return directory;
    }

    final static String REG_KEY = "SOFTWARE\\WOW6432Node\\re-logic\\terraria";

    private static String getInstallDirFromRegistry() throws InvocationTargetException, IllegalAccessException {
        String path = WinRegistry.readString(
                WinRegistry.HKEY_CURRENT_USER,
                REG_KEY,
                "install_path"
        );
        if (path != null) return path;
        else return WinRegistry.readString(
                WinRegistry.HKEY_LOCAL_MACHINE,
                REG_KEY,
                "install_path"
        );
    }

    private static File getInstallDir(String programFileDir) {
        File file = new File(programFileDir, "Steam");
        file = new File(file, "steamapps");
        file = new File(file, "common");
        file = new File(file, "Terraria");
        return file;
    }
}

import javax.swing.*;
import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;

public class Installer {
    private static final String TERRARIA_VERSION = "v1.3.5.3";
    private static final int TERRARIA_SIZE = 10786816; // Windows only: We only want to make a backup of the official release
    private static final int TERRARIA_SIZE_GOG = 10786816; // Could potentially differ in a later release. Coincidence?

    public static void tryInstall(String[] files, String[] filesToDelete, File directory, boolean WindowsInstall) {
        try {
            install(files, filesToDelete, directory, WindowsInstall);
        } catch (Exception e) {
            messageBox("A problem was encountered while installing!\n" + e, JOptionPane.ERROR_MESSAGE);
        }
    }

    private static void install(final String[] files, final String[] filesToDelete, File directory, boolean isWindowsInstall) throws IOException {

        directory = getTerrariaFolder(directory);
        if (directory == null) return;

        final File terraria = findTerrariaExe(directory);
        if (terraria == null) return;

        if (isWindowsInstall) backupWindowsFiles(directory, terraria);
        deleteFiles(filesToDelete, directory);

        final StringBuilder badFiles = new StringBuilder("\n");
        installFiles(files, directory, badFiles);

        if (badFiles.length() > 1) {
            if (badFiles.length() > 8)
                messageBox("The following files were missing and could not be installed:" + badFiles + "All the other files have been installed properly. \n\n DID YOU FORGET TO UNZIP THE ZIP ARCHIVE BEFORE ATTEMPTING TO INSTALL?", JOptionPane.ERROR_MESSAGE);
            else
                messageBox("The following files were missing and could not be installed:" + badFiles + "All the other files have been installed properly.", JOptionPane.ERROR_MESSAGE);
            return;
        }
        messageBox("Installation successful!", JOptionPane.INFORMATION_MESSAGE);
    }

    private static File getTerrariaFolder(final File directory) {
        if (directory != null && directory.exists())
            return directory;

        messageBox("Please select the Terraria install directory.\n\nPlease do not select the Terraria saves folder (It contains your players and worlds), that is incorrect.\n\nThe install folder should have a file called Terraria.exe in it.", JOptionPane.PLAIN_MESSAGE);

        final JFileChooser fileChooser = new JFileChooser();
        fileChooser.setFileSelectionMode(JFileChooser.DIRECTORIES_ONLY);
        fileChooser.showDialog(null, "Select Terraria install directory");

        if (fileChooser.getCurrentDirectory() == null || !fileChooser.getCurrentDirectory().isDirectory()) {
            messageBox("Could not find place to install to!", JOptionPane.ERROR_MESSAGE);
            return null;
        }

        if (!fileChooser.getCurrentDirectory().canRead() || !fileChooser.getCurrentDirectory().canWrite()) {
            messageBox("Can not read or write in selected directory!", JOptionPane.ERROR_MESSAGE);
            return null;
        }

        return fileChooser.getSelectedFile();
    }

    private static File findTerrariaExe(final File directory) {
        final File terraria = new File(directory, "Terraria.exe");
        if (!terraria.exists()) {
            messageBox(String.format("Could not find your Terraria.exe file in %s!", directory), JOptionPane.ERROR_MESSAGE);
            return null;
        }
        return terraria;
    }

    private static void backupWindowsFiles(final File directory, final File terraria) throws IOException {
        File terrariaBackup = new File(directory, "Terraria_" + TERRARIA_VERSION + ".exe");
        File terrariaUnknown = new File(directory, "Terraria_Unknown.exe");
        if (!terrariaBackup.exists() && (terraria.length() == TERRARIA_SIZE || terraria.length() == TERRARIA_SIZE_GOG)) {
            copy(terraria, terrariaBackup);
        } else if (!terrariaUnknown.exists()) {
            File tModLoader = new File("Terraria.exe");
            if (terraria.length() == tModLoader.length()) {
                // Double install. Might be a mistake or an attempt to fix an install.
            } else {
                copy(terraria, terrariaUnknown);
            }
        }
    }

    private static void deleteFiles(final String[] filesToDelete, final File directory) {
        for (String file : filesToDelete) {
            File source = new File(directory, file);
            if (source.exists()) {
                source.delete();
            }
        }
    }

    private static void installFiles(final String[] files, final File directory, final StringBuilder badFiles) throws IOException {
        for (String file : files) {
            File source = new File(file);
            if (source.exists()) {
                File destination = new File(directory, file);
                File parent = destination.getParentFile();
                if (parent != null) {
                    parent.mkdirs();
                }
                copy(source, destination);
                if (file.equals("tModLoaderServer") || file.equals("Terraria")) {
                    final boolean exeutableSuccess = destination.setExecutable(true, false); // Result should be rw-r--r-- becoming rwxr-xr-x
                    if (!exeutableSuccess) {
                        messageBox("Failed to set binary executable permissions!", JOptionPane.WARNING_MESSAGE);
                    }
                }
            } else {
                badFiles.append(file).append("\n");
            }
        }
    }

    private static void copy(File source, File destination) throws IOException {
        Files.copy(source.toPath(), destination.toPath(), StandardCopyOption.REPLACE_EXISTING);
    }

    private static void messageBox(String message, int messageType) {
        JOptionPane.showMessageDialog(null, message, "tModLoader Installer", messageType);
    }
}

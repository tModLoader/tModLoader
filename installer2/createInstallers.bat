javac -source 1.8 -target 1.8 Installer.java WindowsInfo.java MacInfo.java LinuxInfo.java
jar cfe WindowsInstaller.jar WindowsInfo Installer.class WindowsInfo.class
jar cfe MacInstaller.jar MacInfo Installer.class MacInfo.class
jar cfe LinuxInstaller.jar LinuxInfo Installer.class LinuxInfo.class

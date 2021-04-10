# TODO: Can't test install script on windows
# Set all the parameters for the install script
set CHANNELSEL=5.0
set VERSIONSEL=5.0.5
set RUNTIMESELECT=dotnet

# Set the old version so we know what to delete when updating versions of NET
set OLDVERSION=0.0.0

#install directories
set INSTALLDIR="NET_$VERSIONSEL"
set NEWINSTALL=`find -type d -name "$INSTALLDIR"`
set OLDDIR="NET_$OLDVERSION"
set CLEANUP=`find -type d -name "$OLDDIR"`

# Check if the install for our target NET already exists, and install if not
if [-n $NEWINSTALL]
else
	curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin <--channel $CHANNELSEL --install-dir $INSTALLDIR --runtime $RUNTIMESELECT --version $VERSIONSEL>
fi

# Check if old install exists and delete if so
if [-n $CLEANUP]
then
	rmdir oldDir
fi

# Run the game
dotnet tModLoader.dll


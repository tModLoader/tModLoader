#!/usr/bin/env bash
# This file should only be called on WINDOWS!

cd "$(dirname "$0")"
. ./BashUtils.sh

echo "Verifying installation directory..."

TXTCOLOR_RED='\033[31m'
TXTCOLOR_NC='\033[0m'
ShownFileDeletionPrompt=false

CheckAndRemoveFile()
{
	# We are in LaunchUtils, so check the folder above!
	CurrentFilePath="../$1"

	if [ -f "$CurrentFilePath" ]; then
		if [ $ShownFileDeletionPrompt = false ]; then
			echo
			echo -e "${TXTCOLOR_RED}It has been detected that your installation directory contains third-party library files${TXTCOLOR_NC}"
			echo -e "${TXTCOLOR_RED}that could potentially break tModLoader. If you have previously installed other tModLoader editions${TXTCOLOR_NC}"
			echo -e "${TXTCOLOR_RED}- we recommend you to redownload them and install them into a folder separate from tModLoader\'s.${TXTCOLOR_NC}"
			echo -e "${TXTCOLOR_RED}For example: 'steamapps/common/tModLoader64bit' (placed next to the 'Terraria' directory.)${TXTCOLOR_NC}"
			echo
			read -p "Press 'ENTER' to proceed and automatically remove those files..."
			
			ShownFileDeletionPrompt=true
		fi

		rm "$CurrentFilePath"
	fi
}

# Arrays in shell scripts are completely useless.
CheckAndRemoveFile "Content/Wave Bank.xwb"
CheckAndRemoveFile "avcodec-58.dll"
CheckAndRemoveFile "avdevice-58.dll"
CheckAndRemoveFile "avfilter-7.dll"
CheckAndRemoveFile "avformat-58.dll"
CheckAndRemoveFile "avutil-56.dll"
CheckAndRemoveFile "FAudio.dll"
CheckAndRemoveFile "ffmpeg.exe"
CheckAndRemoveFile "ffplay.exe"
CheckAndRemoveFile "ffprobe.exe"
CheckAndRemoveFile "FNA.dll"
CheckAndRemoveFile "Ionic.Zip.CF.dll"
CheckAndRemoveFile "Ionic.Zip.Reduced.dll"
CheckAndRemoveFile "libjpeg-9.dll"
CheckAndRemoveFile "libpng16-16.dll"
CheckAndRemoveFile "libtheorafile.dll"
CheckAndRemoveFile "libtiff-5.dll"
CheckAndRemoveFile "libwebp-7.dll"
CheckAndRemoveFile "log4net.dll"
CheckAndRemoveFile "MojoShader.dll"
CheckAndRemoveFile "Mono.Cecil.dll"
CheckAndRemoveFile "Mono.Cecil.Mdb.dll"
CheckAndRemoveFile "Mono.Cecil.Pdb.dll"
CheckAndRemoveFile "MonoMod.RuntimeDetour.dll"
CheckAndRemoveFile "MonoMod.RuntimeDetour.xml"
CheckAndRemoveFile "MonoMod.Utils.dll"
CheckAndRemoveFile "MonoMod.Utils.xml"
CheckAndRemoveFile "MP3Sharp.dll"
CheckAndRemoveFile "Newtonsoft.Json.dll"
CheckAndRemoveFile "NVorbis.dll"
CheckAndRemoveFile "postproc-55.dll"
CheckAndRemoveFile "ReLogicFNA.dll"
CheckAndRemoveFile "SDL2.dll"
CheckAndRemoveFile "SDL2_image.dll"
CheckAndRemoveFile "soft_oal.dll"
CheckAndRemoveFile "Steamworks.NET.64Bit.dll"
CheckAndRemoveFile "steam_api64.dll"
CheckAndRemoveFile "swresample-3.dll"
CheckAndRemoveFile "swscale-5.dll"
CheckAndRemoveFile "System.Windows.Forms.Mono.dll"
CheckAndRemoveFile "zlib1.dll"

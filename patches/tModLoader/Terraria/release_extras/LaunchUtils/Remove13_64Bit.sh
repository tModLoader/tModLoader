echo "Verifying installation directory..."

IllegalFiles='avcodec-58.dll avdevice-58.dll avfilter-7.dll avformat-58.dll avutil-56.dll FAudio.dll ffmpeg.exe ffplay.exe ffprobe.exe FNA.dll Ionic.Zip.CF.dll Ionic.Zip.Reduced.dll libjpeg-9.dll libpng16-16.dll libtheorafile.dll libtiff-5.dll libwebp-7.dll log4net.dll MojoShader.dll Mono.Cecil.dll Mono.Cecil.Mdb.dll Mono.Cecil.Pdb.dll MonoMod.RuntimeDetour.dll MonoMod.RuntimeDetour.xml MonoMod.Utils.dll MonoMod.Utils.xml MP3Sharp.dll Newtonsoft.Json.dll NVorbis.dll postproc-55.dll ReLogicFNA.dll SDL2.dll SDL2_image.dll soft_oal.dll Steamworks.NET.64Bit.dll steam_api64.dll swresample-3.dll swscale-5.dll System.Windows.Forms.Mono.dll zlib1.dll'
TXTCOLOR_RED='\033[31m'
TXTCOLOR_NC='\033[0m'
ShownFileDeletionPrompt=false

for file in $IllegalFiles
do
    echo "[DEBUG] Checking file: \"$file\"..."
    # We are in LaunchUtils so check the folder above!
	if [ -f "../$file" ]; then
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
		
		rm "../$file"
	fi
done

steamcmd +force_install_dir $(pwd)/../tmod +login anonymous +app_update 1281930 +quit

input="install.txt"
if [ -f "$input" ] ; then
	str="+force_install_dir $(pwd)/../tmod +login anonymous"
	while read -r line
	do
		str="$str +workshop_download_item 1281930 $line"
	done < "$input"
	str="$str +quit"
	
	steamcmd $str
fi

echo "\n\n"
echo "The server has been installed in ../tmod directory (ie one folder up and across)."

read -p "You can run this script again to update mods/server files. Press ENTER to close." bob

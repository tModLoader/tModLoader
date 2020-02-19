#!/bin/bash

if [ $# -ne 2 ]
then
	echo "Please do not run this file manually"
	exit 1
fi

exec > >(tee "./tml_update.log") 2>&1
tpid=$1

ppid=`ps -o ppid= $tpid`
echo "Parent process id $ppid"

cmd=`ps -p $ppid -o args=`
echo "Launch script command: $cmd"

wd=`lsof -p $ppid | awk '/cwd/{ print $9 }'`
echo "Launch directory: $wd"

counter=0
while kill -0 "$tpid"; do
	if [ $counter -lt 2 ]; then
		let counter++
		echo "$tpid is still running, waiting for tModLoader to close..."
		sleep 5
	else
		# tML on mono often takes 40 seconds to close. No point waiting, just kill it
		echo "$tpid is still running, killing..."
		kill -9 "$tpid"
		sleep 2
	fi
done

echo "Updating..."
rm -rf tModLoader_update
mkdir tModLoader_update

if [[ $2 == *.tar.gz ]]; then
	tar xvzf "$2" -C tModLoader_update
else #.zip
	unzip "$2" -d tModLoader_update
fi
rm "$2"

chmod a+x tModLoader_update/Terraria
chmod a+x tModLoader_update/tModLoader*

rm tModLoader_update/README.txt tModLoader_update/*Installer*
mv tModLoader_update/* .
rmdir tModLoader_update

echo "Deleting update script"
rm -- "$0"

cd $wd
echo "Successfully updated, tModLoader will restart now"
$cmd >/dev/null 2>&1
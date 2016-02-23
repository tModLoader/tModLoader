@if (@X)==(@Y) @end /* JScript comment
	@echo off
	
	rem :: the first argument is the script name as it will be used for proper help message
	cscript //E:JScript //nologo "%~f0" "%~nx0" %*

	exit /b %errorlevel%
	
@if (@X)==(@Y) @end JScript comment */


/*
Compression/uncompression command-line tool that uses Shell.Application and WSH/Jscript -
http://msdn.microsoft.com/en-us/library/windows/desktop/bb774085(v=vs.85).aspx

Some resources That I've used:
http://www.robvanderwoude.com/vbstech_files_zip.php
https://code.google.com/p/jsxt/source/browse/trunk/js/win32/ZipFile.js?r=161




UPDATE *17-03-15*

Devnullius Plussed noticed a bug in ZipDirItems  and ZipItem functions (now fixed)
And also following issues (at the moment not handled by the script):
- if there's not enough space on the system drive (usually C:\) the script could produce various errors , most often the script halts.
- Folders and files that contain unicode symbols cannot be handled by Shell.Application object.

UPDATE *24-03-15*

Error messages are caught in waitforcount method and if shuch pops-up the script is stopped.
As I don't know hoe to check the content of the pop-up the exact reason for the failure is not given
but only the possible reasons.

UPDATE *22-02-16*

Javid Pack(https://github.com/JavidPack) has found two bugs in zipItem command and in ZipItem function.Now fixed. 

------
It's possible to be ported for C#,Powershell and JScript.net so I'm planning to do it at some time.

For sure there's a lot of room for improvements and optimization and I'm absolutely sure there are some bugs
as the script is big enough to not have.



!!!
For suggestions contact me at - npocmaka@gmail.com
!!!

*/


//////////////////////////////////////
//   CONSTANTS

// TODO - Shell.Application and Scripting.FileSystemObject objects could be set as global variables to avoid theit creation
// in every method.

//empty zip character sequense
var ZIP_DATA= "PK" + String.fromCharCode(5) + String.fromCharCode(6) + "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

var SLEEP_INTERVAL=200;

//copy option(s) used by Shell.Application.CopyHere/MoveHere
var NO_PROGRESS_BAR=4;


//oprions used for zip/unzip
var force=true;
var move=false;

//option used for listing content of archive
var flat=false;

var source="";
var destination="";

var ARGS = WScript.Arguments;
var scriptName=ARGS.Item(0);

//
//////////////////////////////////////

//////////////////////////////////////
//   ADODB.Stream extensions

if ( ! this.ADODB ) {
	var ADODB = {};
}

if ( ! ADODB.Stream ) {
	ADODB.Stream = {};
}

// writes a binary data to a file
if ( ! ADODB.Stream.writeFile ) {
	ADODB.Stream.writeFile = function(filename, bindata)
	{
        var stream = new ActiveXObject("ADODB.Stream");
        stream.Type = 2;
        stream.Mode = 3;
        stream.Charset ="ASCII";
        stream.Open();
        stream.Position = 0;
        stream.WriteText(bindata);
        stream.SaveToFile(filename, 2);
        stream.Close();
		return true;
	};
}

//
//////////////////////////////////////

//////////////////////////////////////
//   common

if ( ! this.Common ) {
	var Common = {};
}

if ( ! Common.WaitForCount ) {
	Common.WaitForCount = function(folderObject,targetCount,countFunction){
		var shell = new ActiveXObject("Wscript.Shell");
		while (countFunction(folderObject) < targetCount ){
			WScript.Sleep(SLEEP_INTERVAL);
			//checks if a pop-up with error message appears while zipping
			//at the moment I have no idea how to read the pop-up content
			// to give the exact reason for failing
			if (shell.AppActivate("Compressed (zipped) Folders Error")) {
				WScript.Echo("Error While zipping");
				WScript.Echo("");
				WScript.Echo("Possible reasons:");
				WScript.Echo(" -source contains filename(s) with unicode characters");
				WScript.Echo(" -produces zip exceeds 8gb size (or 2,5 gb for XP and 2003)");
				WScript.Echo(" -not enough space on system drive (usually C:\\)");
				WScript.Quit(432);
			}
			
		}
	}
}

if ( ! Common.getParent ) {
	Common.getParent = function(path){
		var splitted=path.split("\\");
		var result="";
		for (var s=0;s<splitted.length-1;s++){
			if (s==0) {
				result=splitted[s];
			} else {
				result=result+"\\"+splitted[s];
			}
		}
		return result;
	}
}

if ( ! Common.getName ) {
	Common.getName = function(path){
		var splitted=path.split("\\");
		return splitted[splitted.length-1];
	}
}

//file system object has a problem to create a folder with slashes at the end
if ( ! Common.stripTrailingSlash ) {
	Common.stripTrailingSlash = function(path){
		while (path.substr(path.length - 1,path.length) == '\\') {
			path=path.substr(0, path.length - 1);
		}
		return path;
	}
}
//
//////////////////////////////////////

//////////////////////////////////////
//   Scripting.FileSystemObject extensions

if (! this.Scripting) {
	var Scripting={};
}

if (! Scripting.FileSystemObject) {
	Scripting.FileSystemObject={};
}

if ( ! Scripting.FileSystemObject.DeleteItem ) {
	Scripting.FileSystemObject.DeleteItem = function (item) 
	{
		var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		if (FSOObj.FileExists(item)){
			FSOObj.DeleteFile(item);
			return true;
		} else if (FSOObj.FolderExists(item) ) {
			FSOObj.DeleteFolder(Common.stripTrailingSlash(item));
			return true;
		} else {
			return false;
		}
	}
}


if ( ! Scripting.FileSystemObject.ExistsFile ) {
	Scripting.FileSystemObject.ExistsFile = function (path)
	{
		var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		return FSOObj.FileExists(path);
	}
}
if ( !Scripting.FileSystemObject.ExistsFolder ) {
	Scripting.FileSystemObject.ExistsFolder = function (path){
	var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		return FSOObj.FolderExists(path);
	}
}

if ( ! Scripting.FileSystemObject.isFolder ) {
	Scripting.FileSystemObject.isFolder = function (path){
	var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		return FSOObj.FolderExists(path);
	}
}



if ( ! Scripting.FileSystemObject.isEmptyFolder ) {
	Scripting.FileSystemObject.isEmptyFolder = function (path){
	var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		if(FSOObj.FileExists(path)){
			return false;
		}else if (FSOObj.FolderExists(path)){	
			var folderObj=FSOObj.GetFolder(path);
			if ((folderObj.Files.Count+folderObj.SubFolders.Count)==0){
				return true;
			}
		}
		return false;	
	}

}

if ( ! Scripting.FileSystemObject.CreateFolder) {
	Scripting.FileSystemObject.CreateFolder = function (path){
		var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		FSOObj.CreateFolder(path);
		return FSOObj.FolderExists(path);
	}
}

if ( ! Scripting.FileSystemObject.ExistsItem) {
	Scripting.FileSystemObject.ExistsItem = function (path){
		var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
		return FSOObj.FolderExists(path)||FSOObj.FileExists(path);
	}
}

if ( ! Scripting.FileSystemObject.getFullPath) {
	Scripting.FileSystemObject.getFullPath = function (path){
		var FSOObj= new ActiveXObject("Scripting.FileSystemObject");
        return FSOObj.GetAbsolutePathName(path);
	}
}
//
//////////////////////////////////////

//////////////////////////////////////
//   Shell.Application extensions
if ( ! this.Shell ) {
	var Shell = {};
}

if (! Shell.Application ) {
	Shell.Application={};
}

if ( ! Shell.Application.ExistsFolder ) {
	Shell.Application.ExistsFolder = function(path){
		var ShellObj=new ActiveXObject("Shell.Application");
		var targetObject = new Object;
		var targetObject=ShellObj.NameSpace(path);
		if (typeof targetObject === 'undefined' || targetObject == null ){
			return false;
		}
		return true;
	
	}
}

if ( ! Shell.Application.ExistsSubItem ) {
	Shell.Application.ExistsSubItem = function(path){
		var ShellObj=new ActiveXObject("Shell.Application");
		var targetObject = new Object;
		var targetObject=ShellObj.NameSpace(Common.getParent(path));
		if (typeof targetObject === 'undefined' || targetObject == null ){
			return false;
		}
		
		var subItem=targetObject.ParseName(Common.getName(path));
		if(subItem === 'undefined' || subItem == null ){
			return false;
		}
		return true;
	
	}
}


if ( ! Shell.Application.ItemCounterL1 ) {
	Shell.Application.ItemCounterL1 = function(path){
		var ShellObj=new ActiveXObject("Shell.Application");
		var targetObject = new Object;
		var targetObject=ShellObj.NameSpace(path);
		if (targetObject != null ){
			return targetObject.Items().Count;	
		} else {
			return 0;
		}
	}
}
// shell application item.size returns the size of uncompressed state of the file.
if ( ! Shell.Application.getSize ) {
	Shell.Application.getSize = function(path){
		var ShellObj=new ActiveXObject("Shell.Application");
		var targetObject = new Object;
		var targetObject=ShellObj.NameSpace(path);
		if (! Shell.Application.ExistsFolder (path)){
			WScript.Echo(path + "does not exists or the file is incorrect type.Be sure you are using full path to the file");
			return 0;
		}
		if (typeof size === 'undefined'){
			var size=0;
		}
		if (targetObject != null ){
			
			for (var i=0; i<targetObject.Items().Count;i++){
				if(!targetObject.Items().Item(i).IsFolder){
					size=size+targetObject.Items().Item(i).Size;
				} else if (targetObject.Items().Item(i).Count!=0){
					size=size+Shell.Application.getSize(targetObject.Items().Item(i).Path);
				}
			}
		} else {
			return 0;
		}
		return size;
	}
}
if ( ! Shell.Application.TakeAction ) {
	Shell.Application.TakeAction = function(destination,item, move ,option){
		if(typeof destination != 'undefined' && move){
			destination.MoveHere(item,option);
		} else if(typeof destination != 'undefined') {
			destination.CopyHere(item,option);
		} 
	}
}

//ProcessItem and  ProcessSubItems can be used both for zipping and unzipping
// When an item is zipped another process is ran and the control is released
// but when the script stops also the copying to the zipped file stops.
// Though the zipping is transactional so a zipped files will be visible only after the zipping is done
// and we can rely on items count when zip operation is performed. 
// Also is impossible to compress an empty folders.
// So when it comes to zipping two additional checks are added - for empty folders and for count of items at the 
// destination.

if ( ! Shell.Application.ProcessItem ) {
	Shell.Application.ProcessItem = function(toProcess, destination  , move ,isZipping,option){
		var ShellObj=new ActiveXObject("Shell.Application");
		destinationObj=ShellObj.NameSpace(destination);
			
		if (destinationObj!= null ){
			if (isZipping && Scripting.FileSystemObject.isEmptyFolder(toProcess)) {
				WScript.Echo(toProcess +" is an empty folder and will be not processed");
				return;
			}
			Shell.Application.TakeAction(destinationObj,toProcess, move ,option);
			var destinationCount=Shell.Application.ItemCounterL1(destination);
			var final_destination=destination + "\\" + Common.getName(toProcess);
			
			if (isZipping && !Shell.Application.ExistsSubItem(final_destination)) {
				Common.WaitForCount(destination
					,destinationCount+1,Shell.Application.ItemCounterL1);
			} else if (isZipping && Shell.Application.ExistsSubItem(final_destination)){
				WScript.Echo(final_destination + " already exists and task cannot be completed");
				return;
			}
		}	
	}
}



if ( ! Shell.Application.ProcessSubItems ) {
	Shell.Application.ProcessSubItems = function(toProcess, destination  , move ,isZipping ,option){
		var ShellObj=new ActiveXObject("Shell.Application");
		var destinationObj=ShellObj.NameSpace(destination);
		var toItemsToProcess=new Object;
		toItemsToProcess=ShellObj.NameSpace(toProcess).Items();
			
		if (destinationObj!= null ){
						
			for (var i=0;i<toItemsToProcess.Count;i++) {
				
				if (isZipping && Scripting.FileSystemObject.isEmptyFolder(toItemsToProcess.Item(i).Path)){

					WScript.Echo("");
					WScript.Echo(toItemsToProcess.Item(i).Path + " is empty and will be not processed");
					WScript.Echo("");

				} else {
					Shell.Application.TakeAction(destinationObj,toItemsToProcess.Item(i),move,option);
					var destinationCount=Shell.Application.ItemCounterL1(destination);
					if (isZipping) {
						Common.WaitForCount(destination,destinationCount+1,Shell.Application.ItemCounterL1);
					}
				}
			}	
		}	
	}
}

if ( ! Shell.Application.ListItems ) {
	Shell.Application.ListItems = function(parrentObject){
		var ShellObj=new ActiveXObject("Shell.Application");
		var targetObject = new Object;
		var targetObject=ShellObj.NameSpace(parrentObject);

		if (! Shell.Application.ExistsFolder (parrentObject)){
			WScript.Echo(parrentObject + "does not exists or the file is incorrect type.Be sure the full path the path is used");
			return;
		}
		if (typeof initialSCount == 'undefined') {
			initialSCount=(parrentObject.split("\\").length-1);
			WScript.Echo(parrentObject);
		}
		
		var spaces=function(path){
			var SCount=(path.split("\\").length-1)-initialSCount;
			var s="";
			for (var i=0;i<=SCount;i++) {
				s=" "+s;
			}
			return s;
		}
		
		var printP = function (item,end){
			if (flat) {
				WScript.Echo(targetObject.Items().Item(i).Path+end);
			}else{
				WScript.Echo( spaces(targetObject.Items().Item(i).Path)+targetObject.Items().Item(i).Name+end);
			}
		}

		if (targetObject != null ){
			var folderPath="";

				for (var i=0; i<targetObject.Items().Count;i++) {
					if(targetObject.Items().Item(i).IsFolder && targetObject.Items().Item(i).Count==0 ){
						printP(targetObject.Items().Item(i),"\\");
					} else if (targetObject.Items().Item(i).IsFolder){
						folderPath=parrentObject+"\\"+targetObject.Items().Item(i).Name;
						printP(targetObject.Items().Item(i),"\\")
						Shell.Application.ListItems(folderPath);						

					} else {
						printP(targetObject.Items().Item(i),"")
						
					}
				}

			}
	}
}

//
//////////////////////////////////////


//////////////////////////////////////
//     ZIP Utils

if ( ! this.ZIPUtils ) {
	var ZIPUtils = {};
}


if ( ! this.ZIPUtils.ZipItem) {	
	ZIPUtils.ZipItem = function(source, destination ) {
		if (!Scripting.FileSystemObject.ExistsItem(source)) {
			WScript.Echo("");
			WScript.Echo("file " + source +" does not exist");
			WScript.Quit(2);	
		}
		
		if (Scripting.FileSystemObject.ExistsFile(destination) && force ) {
			Scripting.FileSystemObject.DeleteItem(destination);
			ADODB.Stream.writeFile(destination,ZIP_DATA);
		} else if (!Scripting.FileSystemObject.ExistsFile(destination)) {
			ADODB.Stream.writeFile(destination,ZIP_DATA);
		} else {
			WScript.Echo("Destination "+destination+" already exists.Operation will be aborted");
			WScript.Quit(15);
		}
		source=Scripting.FileSystemObject.getFullPath(source);
		destination=Scripting.FileSystemObject.getFullPath(destination);
		
		Shell.Application.ProcessItem(source,destination,move,true ,NO_PROGRESS_BAR);

	}
}


if ( ! this.ZIPUtils.ZipDirItems) {	
	ZIPUtils.ZipDirItems = function(source, destination ) {
		if (!Scripting.FileSystemObject.ExistsFolder(source)) {
			WScript.Echo();
			WScript.Echo("file " + source +" does not exist");
			WScript.Quit(2);	
		}
		if (Scripting.FileSystemObject.ExistsFile(destination) && force ) {
			Scripting.FileSystemObject.DeleteItem(destination);
			ADODB.Stream.writeFile(destination,ZIP_DATA);
		} else if (!Scripting.FileSystemObject.ExistsFile(destination)) {
			ADODB.Stream.writeFile(destination,ZIP_DATA);
		} else {
			WScript.Echo("Destination "+destination+" already exists.Operation will be aborted");
			WScript.Quit(15);
		}
		
		source=Scripting.FileSystemObject.getFullPath(source);
		destination=Scripting.FileSystemObject.getFullPath(destination);
		
		Shell.Application.ProcessSubItems(source, destination, move ,true,NO_PROGRESS_BAR);
		
		if (move){
			Scripting.FileSystemObject.DeleteItem(source);
		}
	}
}

if ( ! this.ZIPUtils.Unzip) {	
	ZIPUtils.Unzip = function(source, destination ) {
		if(!Shell.Application.ExistsFolder(source) ){
			WScript.Echo("Either the target does not exist or is not a correct type");
			return;
		}
		
		if (Scripting.FileSystemObject.ExistsItem(destination) && force ) {
			Scripting.FileSystemObject.DeleteItem(destination);
		} else if (Scripting.FileSystemObject.ExistsItem(destination)){
			WScript.Echo("Destination " + destination + " already exists");
			return;
		}
		Scripting.FileSystemObject.CreateFolder(destination);
		source=Scripting.FileSystemObject.getFullPath(source);
		destination=Scripting.FileSystemObject.getFullPath(destination);
		Shell.Application.ProcessSubItems(source, destination, move ,false,NO_PROGRESS_BAR);	
		
		if (move){
			Scripting.FileSystemObject.DeleteItem(source);
		}
    }		
}

if ( ! this.ZIPUtils.AddToZip) {
	ZIPUtils.AddToZip = function(source, destination ) {
		if(!Shell.Application.ExistsFolder(destination)) {
			WScript.Echo(destination +" is not valid path to/within zip.Be sure you are not using relative paths");
			Wscript.Exit("101");
		}
		if(!Scripting.FileSystemObject.ExistsItem(source)){
			WScript.Echo(source +" does not exist");
			Wscript.Exit("102");
		}
		source=Scripting.FileSystemObject.getFullPath(source);
		Shell.Application.ProcessItem(source,destination,move,true ,NO_PROGRESS_BAR); 
	}
}

if ( ! this.ZIPUtils.UnzipItem) {	
	ZIPUtils.UnzipItem = function(source, destination ) {

		if(!Shell.Application.ExistsSubItem(source)){
			WScript.Echo(source + ":Either the target does not exist or is not a correct type");
			return;
		}
		
		if (Scripting.FileSystemObject.ExistsItem(destination) && force ) {
			Scripting.FileSystemObject.DeleteItem(destination);
		} else if (Scripting.FileSystemObject.ExistsItem(destination)){
			WScript.Echo(destination+" - Destination already exists");
			return;
		} 
		
		Scripting.FileSystemObject.CreateFolder(destination);
		destination=Scripting.FileSystemObject.getFullPath(destination);
		Shell.Application.ProcessItem(source, destination, move ,false,NO_PROGRESS_BAR);
		                            
    }		
}
if ( ! this.ZIPUtils.getSize) {	
	ZIPUtils.getSize = function(path) {
		// first getting a full path to the file is attempted
		// as it's required by shell.application
		// otherwise is assumed that a file within a zip 
		// is aimed
		
		//TODO - find full path even if the path points to internal for the 
		// zip directory
		
		if (Scripting.FileSystemObject.ExistsFile(path)){
			path=Scripting.FileSystemObject.getFullPath(path);
		}
		WScript.Echo(Shell.Application.getSize(path));
	}
}

if ( ! this.ZIPUtils.list) {	
	ZIPUtils.list = function(path) {
		// first getting a full path to the file is attempted
		// as it's required by shell.application
		// otherwise is assumed that a file within a zip 
		// is aimed
		
		//TODO - find full path even if the path points to internal for the 
		// zip directory
		
		// TODO - optional printing of each file uncompressed size
		
		if (Scripting.FileSystemObject.ExistsFile(path)){
			path=Scripting.FileSystemObject.getFullPath(path);
		}
		Shell.Application.ListItems(path);
	}
}

//
//////////////////////////////////////

/////////////////////////////////////
//   parsing'n'running
function printHelp(){

	WScript.Echo( scriptName + " list -source zipFile [-flat yes|no]");
	WScript.Echo( "		list the content of a zip file");
	WScript.Echo( "	zipFile - absolute path to the zip file");
	WScript.Echo( "			could be also a directory or a directory inside a zip file or");
	WScript.Echo( "			or a .cab file or an .iso file");
	WScript.Echo( "	-flat - indicates if the structure of the zip will be printed as tree");
	WScript.Echo( "			or with absolute paths (-flat yes).Default is yes.");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " list -source C:\\myZip.zip -flat no" );
	WScript.Echo( "	" + scriptName + " list -source C:\\myZip.zip\\inZipDir -flat yes" );
	
	WScript.Echo( scriptName + " getSize -source zipFile");
	WScript.Echo( "		prints uncompressed size of the zipped file in bytes");
	WScript.Echo( "	zipFile - absolute path to the zip file");
	WScript.Echo( "			could be also a directory or a directory inside a zip file or");
	WScript.Echo( "			or a .cab file or an .iso file");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " getSize -source C:\\myZip.zip" );
	
	WScript.Echo( scriptName + " zipDirItems -source source_dir -destination destination.zip [-force yes|no] [-keep yes|no]");
	WScript.Echo( "		zips the content of given folder without the folder itself ");
	WScript.Echo( "	source_dir - path to directory which content will be compressed");
	WScript.Echo( "		Empty folders in the source directory will be ignored");
	WScript.Echo( "	destination.zip - path/name  of the zip file that will be created");
	WScript.Echo( "	-force - indicates if the destination will be overwritten if already exists.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "	-keep - indicates if the source content will be moved or just copied/kept.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " zipDirItems -source C:\\myDir\\ -destination C:\\MyZip.zip -keep yes -force no" );
	
	WScript.Echo( scriptName + " zipItem -source item -destination destination.zip [-force yes|no] [-keep yes|no]");
	WScript.Echo( "		zips file or directory to a destination.zip file ");
	WScript.Echo( "	item - path to file or directory which content will be compressed");
	WScript.Echo( "		If points to an empty folder it will be ignored");
	WScript.Echo( "		If points to a folder it also will be included in the zip file alike zipdiritems command");
	WScript.Echo( "		Eventually zipping a folder in this way will be faster as it does not process every element one by one");
	WScript.Echo( "	destination.zip - path/name  of the zip file that will be created");
	WScript.Echo( "	-force - indicates if the destination will be overwritten if already exists.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "	-keep - indicates if the source content will be moved or just copied/kept.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " zipItem -source C:\\myDir\\myFile.txt -destination C:\\MyZip.zip -keep yes -force no" );
	
	WScript.Echo( scriptName + " unzip -source source.zip -destination destination_dir [-force yes|no] [-keep yes|no]");
	WScript.Echo( "		unzips the content of a zip file to a given directory ");
	WScript.Echo( "	source - path to the zip file that will be expanded");
	WScript.Echo( "		Eventually .iso , .cab or even an ordinary directory can be used as a source");
	WScript.Echo( "	destination_dir - path to directory where unzipped items will be stored");
	WScript.Echo( "	-force - indicates if the destination will be overwritten if already exists.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "	-keep - indicates if the source content will be moved or just copied/kept.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " unzip -source C:\\myDir\\myZip.zip -destination C:\\MyDir -keep no -force no" );
	
	WScript.Echo( scriptName + " unZipItem -source source.zip -destination destination_dir [-force yes|no] [-keep yes|no]");
	WScript.Echo( "		unzips  a single within a given zip file to a destination directory");
	WScript.Echo( "	source - path to the file/folcer within a zip  that will be expanded");
	WScript.Echo( "		Eventually .iso , .cab or even an ordinary directory can be used as a source");
	WScript.Echo( "	destination_dir - path to directory where unzipped item will be stored");
	WScript.Echo( "	-force - indicates if the destination directory will be overwritten if already exists.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "	-keep - indicates if the source content will be moved or just copied/kept.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " unZipItem -source C:\\myDir\\myZip.zip\\InzipDir\\InzipFile -destination C:\\OtherDir -keep no -force yes" );
	WScript.Echo( "	" + scriptName + " unZipItem -source C:\\myDir\\myZip.zip\\InzipDir -destination C:\\OtherDir " );
	
	WScript.Echo( scriptName + " addToZip -source sourceItem -destination destination.zip  [-keep yes|no]");
	WScript.Echo( "		adds file or folder to already exist zip file");
	WScript.Echo( "	source - path to the item that will be processed");
	WScript.Echo( "	destination_zip - path to the zip where the item will be added");
	WScript.Echo( "	-keep - indicates if the source content will be moved or just copied/kept.");
	WScript.Echo( "		default is yes");
	WScript.Echo( "Example:");
	WScript.Echo( "	" + scriptName + " addToZip -source C:\\some_file -destination C:\\myDir\\myZip.zip\\InzipDir -keep no " );
	WScript.Echo( "	" + scriptName + " addToZip -source  C:\\some_file -destination C:\\myDir\\myZip.zip " );
	
	WScript.Echo( "	by Vasil \"npocmaka\" Arnaudov - npocmaka@gmail.com" );
	WScript.Echo( "	ver 0.1.2 " );
	WScript.Echo( "	latest version could be found here https://github.com/npocmaka/batch.scripts/blob/master/hybrids/jscript/zipjs.bat" );
	
	
}

function parseArguments(){
	if (WScript.Arguments.Length==1 || WScript.Arguments.Length==2 || ARGS.Item(1).toLowerCase() == "-help" ||  ARGS.Item(1).toLowerCase() == "-h" ) {
		printHelp();
		WScript.Quit(0);
   }
   
   //all arguments are key-value pairs plus one for script name and action taken - need to be even number
	if (WScript.Arguments.Length % 2 == 1 ) {
		WScript.Echo("Illegal arguments ");
		printHelp();
		WScript.Quit(1);
	}
	
	//ARGS
	for(var arg = 2 ; arg<ARGS.Length-1;arg=arg+2) {
		if (ARGS.Item(arg) == "-source") {
			source = ARGS.Item(arg +1);
		}
		if (ARGS.Item(arg) == "-destination") {
			destination = ARGS.Item(arg +1);
		}
		if (ARGS.Item(arg).toLowerCase() == "-keep" && ARGS.Item(arg +1).toLowerCase() == "no") {
			move=true;
		}
		if (ARGS.Item(arg).toLowerCase() == "-force" && ARGS.Item(arg +1).toLowerCase() == "no") {
			force=false;
		}
		if (ARGS.Item(arg).toLowerCase() == "-flat" && ARGS.Item(arg +1).toLowerCase() == "yes") {
			flat=true;
		}
	}
	
	if (source == ""){
		WScript.Echo("Source not given");
		printHelp();
		WScript.Quit(59);
	}

}

var checkDestination=function(){
	if (destination == ""){
		WScript.Echo("Destination not given");
		printHelp();
		WScript.Quit(65);
	}
}

var main=function(){
	parseArguments();
	switch (ARGS.Item(1).toLowerCase()) {
	case "list":
		ZIPUtils.list(source);
		break;
	case "getsize":
		ZIPUtils.getSize(source);
		break;
	case "zipdiritems":
		checkDestination();
		ZIPUtils.ZipDirItems(source,destination);
		break;
	case "zipitem":
		checkDestination();
		ZIPUtils.ZipItem(source,destination);
		break;
	case "unzip":
		checkDestination();
		ZIPUtils.Unzip(source,destination);
		break;
	case "unzipitem":
		checkDestination();
		ZIPUtils.UnzipItem(source,destination);
		break;
	case "addtozip":
		checkDestination();
		ZIPUtils.AddToZip(source,destination);
		break;
	default:
		WScript.Echo("No valid switch has been passed");
		printHelp();
		
	}
	
}
main();
//
//////////////////////////////////////
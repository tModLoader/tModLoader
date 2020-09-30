import os
import zipfile
import sys
import time
import tarfile

executables = ['tModLoaderServer', 'tModLoader', 'open-folder', 'tModLoaderServer.bin.x86', 'tModLoaderServer.bin.x86_64', 'tModLoader.bin.x86', 'tModLoader.bin.x86_64', 'tModLoaderServer.bin.osx', 'tModLoaderServer.bin.osx']
extra = None

def set_permissions(tarinfo):
    filename = os.path.basename(tarinfo.name)
    #print("Deciding for " + tarinfo.name)
    if filename in executables:
        print("Execute permissions set for " + tarinfo.name)
        #tarinfo.mode = 0o100777 << 16 # 0777 # for example
        tarinfo.mode = 0o777
    return tarinfo

def zipdir(path, ziph):
    # ziph is zipfile handle
    for root, dirs, files in os.walk(path):
        for filename in files:
            file = os.path.join(root, filename)
            destination = os.path.relpath(file, path)
            if extra:
                destination = os.path.join(extra, destination)
            print("Zipping " + file)
            if (filename in executables):
                f = open(file, 'br')
                bytes = f.read()
                f.close()
                
                info = zipfile.ZipInfo(destination)
                info.date_time = time.localtime()
                #info.external_attr |= 0o755 << 
                info.external_attr = 0o100777 << 16
                print("Execute permissions set for " + file)
                
                ziph.writestr(info, bytes, zipfile.ZIP_DEFLATED)
            else:
                ziph.write(file, destination)
            
if __name__ == '__main__':
    if(len(sys.argv) != 3 and len(sys.argv) != 4):
        print("[FolderName] [ZipFileName] [Relative] arguments needed")
        sys.exit()
        
    foldername = sys.argv[1]
    zipfilename = sys.argv[2]
    if len(sys.argv) == 4:
        extra = sys.argv[3] 

    if ".zip" in zipfilename:
        zipf = zipfile.ZipFile(zipfilename, 'w', zipfile.ZIP_DEFLATED)
        zipdir(foldername, zipf)
        zipf.close()
    elif ".tar.gz" in zipfilename:
        tar = tarfile.open(zipfilename, 'w:gz')
        tar.add(foldername, arcname="", filter=set_permissions) #tar the entire folder 
        tar.close()
    else:
        print("Something went wrong")
    
#!/bin/bash

shopt -s nullglob
getBuildpackDir(){
  buildpackName=$1
  dirName=$(echo -n $buildpackName | md5sum | awk '{print $1}')
  echo $dirName
}
#echo $@
tmp=/tmp
dropletDir=/tmp/droplet

echo $@
mv $dropletDir/app /home/vcap
chown -R vcap:vcap /home/vcap/app
buildpackList=()

skipDetect=false
for arg in $@; do
  if [ $arg = "-skipDetect" ] ; then
    skipDetect=true
  else
    #echo $arg
    buildpackList+=($arg)
  fi
done
if [ ! -d /tmp/buildpacks ]; then
  mkdir $tmp/buildpacks
fi

for buildpackName in ${buildpackList[@]}; do
  buildpackDirName=$(getBuildpackDir $buildpackName)
  buildpackZip="$tmp/buildpackdownloads/$buildpackName.zip"
  unzip -qo -d $tmp/buildpacks/$buildpackDirName $buildpackZip

done
buildpackOrder=$(IFS=,;printf  "%s" "${buildpackList[*]}")

#ls -l /home/vcap/app
# allow any apps with binaries to be executable. improve logic in future to not do this for all files
for file in /home/vcap/app/*; do chmod +x $file; done

builder="$tmp/lifecycle/builder"
if [ "$skipDetect" = true ]; then
  builder="$builder -skipDetect "
fi
mkdir $tmp/droplet/cache
builder="$builder -buildArtifactsCacheDir $tmp/droplet/cache -buildDir /home/vcap/app -buildpacksDir $tmp/buildpacks -outputDroplet $dropletDir/droplet.tar -buildpackOrder $buildpackOrder"
echo $builder
$builder

# unpack droplet tarball into <VOLUME>/droplet subdir
#mkdir $dropletDir/droplet
#tar -xf $dropletDir/droplet.tar -C $dropletDir/droplet
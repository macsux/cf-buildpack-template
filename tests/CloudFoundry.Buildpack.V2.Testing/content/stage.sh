#!/bin/bash
getBuildpackDir(){
  buildpackName=$1
  dirName=$(echo -n $buildpackName | md5sum | awk '{print $1}')
  echo $dirName
}
#echo $@
#tmp=~/projects/cf-buildpack-template/artifacts/test
tmp=/tmp
echo $@
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
  #buildpackOrder="$buildpackOrder$buildpackName"
  #echo "Name $buildpackName"
  #echo $buildpackOrder
done
buildpackOrder=$(IFS=,;printf  "%s" "${buildpackList[*]}")


builder="$tmp/lifecycle/builder"
if [ "$skipDetect" = true ]; then
  builder="$builder -skipDetect "
fi
builder="$builder -buildArtifactsCacheDir $tmp/cache -buildDir /home/vcap/app -buildpacksDir $tmp/buildpacks -outputDroplet $tmp/droplet/droplet.tar -buildpackOrder $buildpackOrder"
echo $builder
$builder
#tar -xf $tmp/droplet/droplet.tar -C $tmp/droplet

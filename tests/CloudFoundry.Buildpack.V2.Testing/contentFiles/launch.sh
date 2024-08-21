#!/bin/bash
shopt -s nullglob
cd /home/vcap
tmp=/tmp
tar -xf /tmp/droplet/droplet.tar -C /home/vcap

#tmp=/home/andrew/projects/cf-buildpack-template
for profileScript in  ./.profile.d/*; do
#    echo $profileScript
    . $profileScript
done
#if [ -d ./app/.profile.d ]; then
#    for profileScript in  ./app/.profile.d/*; do
##        echo $profileScript
#        . $profileScript
#    done
#fi

startCommand=$(cat /home/vcap/staging_info.yml | jq .start_command)

launcher="$tmp/lifecycle/launcher ./app $startCommand ''"
#echo $ASPNETCORE_URLS
#echo $DOTNET_ROOT
eval $launcher
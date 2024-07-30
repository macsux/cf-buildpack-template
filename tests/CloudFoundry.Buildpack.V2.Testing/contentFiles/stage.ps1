param(

    [switch]$skipDetect
)
$buildpackList = $args
$homeDir = "c:\users\vcap"
$tmp = "$homeDir\appdata\local\temp" 
function Get-BuildpackDirectory {
    param(
        $buildpackName
    )
    $bytes = [System.Text.Encoding]::ASCII.GetBytes($buildpackName);
    $md5 = [System.Security.Cryptography.MD5]::Create.Invoke()
    $hash = $md5.ComputeHash($bytes)
    $hexHash = [System.BitConverter]::ToString($hash)
    $hexHash = $hexHash.ToLower()
    $hexHash = $hexHash.Replace("-","")
    $hexHash
}


$buildacksDir = "$tmp\buildpacks"
$dropletDir = "$tmp\droplet"

if (-Not (Test-Path $buildacksDir)){
    mkdir $buildacksDir > $null
}

foreach ($buildpackName in $buildpackList){
    
#     $buildpackZip = [System.IO.Path]::Combine($tmp, "$buildpackName.zip")
    $buildpackZip = [System.IO.Path]::Combine([System.IO.Path]::Combine($tmp, "buildpackdownloads"), "$buildpackName.zip")
    $buildpackDirName = Get-BuildpackDirectory $buildpackName
    Expand-Archive $buildpackZip -DestinationPath "$buildacksDir\$buildpackDirName"
}
$buildpackOrder=$buildpackList -join ","

$builder = "$tmp\lifecycle\builder.exe -buildArtifactsCacheDir $tmp\cache -buildDir $homeDir\app -buildpacksDir $tmp\buildpacks -outputDroplet $tmp\droplet\droplet.tar -buildpackOrder $buildpackOrder"
if($skipDetect.IsPresent){
    $builder = "$builder -skipDetect"
}
Write-Output $builder
Invoke-Expression $builder

# tar -xf $tmp/droplet/droplet.tar -C $tmp/droplet
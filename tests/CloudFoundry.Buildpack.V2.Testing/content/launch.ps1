$homeDir = "c:\users\vcap"
$tmp = "$homeDir\appdata\local\temp" 
#$tmp = "C:\projects\cf-buildpack-template\tests\MyBuildpack.Tests\bin\Debug\net8.0\RunBuilderLinux-8dc2d796adf074c\droplet" 
#$homeDir = "C:\projects\cf-buildpack-template\tests\MyBuildpack.Tests\bin\Debug\net8.0\RunBuilderLinux-8dc2d796adf074c\droplet"

function SourceEnvVarsFromBat {
    param(
        $batFile
    )
    # PARSE OUT BAT FILE FOR ANY 'SET VAR=VALUE' PATTERNS AND SET THEM AT CURRENT POWERSHELL LEVEL SO THEY PROPOGATE TO THE APP (DOT SOURCING DOESN'T WORK LIKE IN BASH)
    Get-Content $batFile | .{process{
        if ($_ -match '^SET\s(.+?)=(.+)') {
            $varName = $matches[1]
            $varValue = $matches[2]
            $setVar = '$env:' + "$varName = ""$varValue"""
            Invoke-Expression $setVar
        }
    }}
}

if (Test-Path "$homeDir/.profile.d"){
    Get-ChildItem "$homeDir/.profile.d" -Filter *.bat | where Length -gt 0Kb | Foreach-Object {
        $profileScript = $_.FullName
        SourceEnvVarsFromBat $profileScript
        . $profileScript
    }
}
if (Test-Path "$homeDir/app/.profile.d"){
    Get-ChildItem "$homeDir/app/.profile.d" -Filter *.bat | where Length -gt 0Kb | Foreach-Object {
        $profileScript = $_.FullName
        SourceEnvVarsFromBat $profileScript
        . $profileScript
    }
}
$stagingInfo = $(Get-Content $tmp/staging_info.yml -Raw | ConvertFrom-Json)
$startCommand="$homedir\app\" + $stagingInfo.start_command
#Write-Output $startCommand
cd c:\users\vcap\app
. $startCommand
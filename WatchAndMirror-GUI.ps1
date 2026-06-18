# Watch-GUI.ps1

$Source = Join-Path $PSScriptRoot "GUI"
$Destination = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\ArmyCommander\GUI"

function Sync-GUI {
    Write-Host ""
    Write-Host "Sync GUI..."
    
    robocopy $Source $Destination /MIR /FFT /R:2 /W:1 /NP

    if ($LASTEXITCODE -gt 7) {
        Write-Warning "Robocopy retornou erro: $LASTEXITCODE"
    }
    else {
        Write-Host "GUI sincronizado."
    }
}

# Make an initial copy when the watcher starts
Sync-GUI

$global:pendingSync = $false
$global:lastChange = Get-Date

$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $Source
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true
$watcher.NotifyFilter = [IO.NotifyFilters]'FileName, DirectoryName, LastWrite, Size'

$action = {
    $global:pendingSync = $true
    $global:lastChange = Get-Date
}

Register-ObjectEvent $watcher Changed -Action $action | Out-Null
Register-ObjectEvent $watcher Created -Action $action | Out-Null
Register-ObjectEvent $watcher Deleted -Action $action | Out-Null
Register-ObjectEvent $watcher Renamed -Action $action | Out-Null

Write-Host ""
Write-Host "Observando mudanças em:"
Write-Host $Source
Write-Host ""
Write-Host "Destino:"
Write-Host $Destination
Write-Host ""
Write-Host "Pressione Ctrl+C para parar."

while ($true) {
    Start-Sleep -Milliseconds 300

    # Debounce: wait for the editor to finish saving before running robocopy
    if ($global:pendingSync -and ((Get-Date) - $global:lastChange).TotalMilliseconds -ge 700) {
        $global:pendingSync = $false
        Sync-GUI
    }
}

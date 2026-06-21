$bannerlordWorkshopExe = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.SteamWorkshop.exe"
$workshopUpdateXml = Join-Path $PSScriptRoot "WorkshopUpdate.xml"

Push-Location $PSScriptRoot
try {
    & $bannerlordWorkshopExe $workshopUpdateXml
}
finally {
    Pop-Location
}
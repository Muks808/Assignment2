$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Wait-RaceDayUrl($Url, $Process, $Name) {
    $deadline = (Get-Date).AddMinutes(3)
    while ((Get-Date) -lt $deadline) {
        $Process.Refresh()
        if ($Process.HasExited) {
            throw "$Name stopped. Check its terminal output for the startup error."
        }
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) { return }
        } catch {
            Start-Sleep -Seconds 1
        }
    }
    throw "$Name did not become ready at $Url. Check its terminal output."
}

try {
    $dotnet = (Get-Command dotnet -CommandType Application).Source
    foreach ($port in 5080, 5090) {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
            throw "Port $port is already in use. Stop the old RaceDay run or Docker stack and retry."
        }
    }

    Write-Host 'Starting the RaceDay API...'
    $api = Start-Process -FilePath $dotnet -ArgumentList 'run --project Assignment2 --launch-profile Assignment2' -PassThru
    Wait-RaceDayUrl 'http://localhost:5080/api/events' $api 'RaceDay API'

    Write-Host 'Starting the RaceDay website...'
    $mvc = Start-Process -FilePath $dotnet -ArgumentList 'run --project RaceDay.Mvc --launch-profile RaceDay.Mvc' -PassThru
    Wait-RaceDayUrl 'http://localhost:5090/' $mvc 'RaceDay MVC'
    Start-Process 'http://localhost:5090/'
} catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

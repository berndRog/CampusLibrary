<#
.SYNOPSIS
    Starts each service in its own PowerShell window, in dependency order:
      1. IdentityAccessServer  (https://localhost:7010)
      2. CampusLibraryApi      (https://localhost:8010)  – waits for IAS
      3. CampusLibraryClient   (https://localhost:6040)  – waits for API

.EXAMPLE
    .\tools\start-dev.ps1
    .\tools\start-dev.ps1 -Profile http

    NOTE: If script execution is blocked by Windows, run once in PowerShell:
        Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
#>

param(
    [ValidateSet('http','https')]
    [string] $Profile = 'https'
)

$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path "$PSScriptRoot\..").Path

# Ports per profile
if ($Profile -eq 'https') {
    $portIas = 7010; $portApi = 8010; $portClient = 6040
} else {
    $portIas = 7011; $portApi = 8012; $portClient = 5040
}

function Open-ServiceWindow {
    param([string] $Label, [string] $Project)
    $cmd = @"
`$host.UI.RawUI.WindowTitle = '$Label'
Write-Host '=== $Label ==='
dotnet run --project "$Project" --launch-profile $Profile
Write-Host ''
Write-Host '$Label stopped. Press any key to close...'
`$null = `$host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
"@
    Start-Process powershell `
        -ArgumentList "-NoExit", "-Command", $cmd `
        -WindowStyle Normal
}

function Wait-ForPort {
    param([int] $Port, [string] $Label)
    Write-Host "Waiting for $Label on port $Port ..."
    while ($true) {
        try {
            $tcp = New-Object System.Net.Sockets.TcpClient
            $tcp.Connect('localhost', $Port)
            $tcp.Dispose()
            break
        } catch { }
        Start-Sleep -Seconds 2
    }
    Write-Host "$Label is ready."
}

Write-Host "==> Opening window: IdentityAccessServer ..."
Open-ServiceWindow "IdentityAccessServer" "$Root\IdentityAccessServer\IdentityAccessServer.csproj"

Wait-ForPort $portIas "IdentityAccessServer"

Write-Host "==> Opening window: CampusLibraryApi ..."
Open-ServiceWindow "CampusLibraryApi" "$Root\CampusLibraryApi\CampusLibraryApi.csproj"

Wait-ForPort $portApi "CampusLibraryApi"

Write-Host "==> Opening window: CampusLibraryClient ..."
Open-ServiceWindow "CampusLibraryClient" "$Root\CampusLibraryClient\CampusLibraryClient.csproj"

Write-Host ""
Write-Host "All three services started in separate PowerShell windows."
<#
.SYNOPSIS
    Stops all CampusLibrary dev services and closes their PowerShell windows.

.EXAMPLE
    .\tools\stop-dev.ps1
    .\tools\stop-dev.ps1 -Profile http

    NOTE: If script execution is blocked by Windows, run once:
        Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
#>

param(
    [ValidateSet('http','https')]
    [string] $Profile = 'https'
)

$ErrorActionPreference = 'Stop'

if ($Profile -eq 'https') {
    $services = @(
        @{ Label = 'IdentityAccessServer'; Port = 7010 }
        @{ Label = 'CampusLibraryApi';     Port = 8010 }
        @{ Label = 'CampusLibraryClient';  Port = 6040 }
    )
} else {
    $services = @(
        @{ Label = 'IdentityAccessServer'; Port = 7011 }
        @{ Label = 'CampusLibraryApi';     Port = 8012 }
        @{ Label = 'CampusLibraryClient';  Port = 5040 }
    )
}

$anyStopped = $false

# 1. Kill dotnet processes by port
foreach ($svc in $services) {
    $conn = Get-NetTCPConnection -LocalPort $svc.Port `
                                 -State Listen `
                                 -ErrorAction SilentlyContinue
    if ($conn) {
        $pid = $conn.OwningProcess | Select-Object -First 1
        Write-Host "Stopping $($svc.Label) (port $($svc.Port), PID $pid) ..."
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        $anyStopped = $true
    } else {
        Write-Host "$($svc.Label) (port $($svc.Port)) – not running."
    }
}

# 2. Close PowerShell windows by window title (set in start-dev.ps1)
Write-Host ""
$labels = $services | ForEach-Object { $_.Label }
Get-Process powershell -ErrorAction SilentlyContinue |
    Where-Object { $labels -contains $_.MainWindowTitle } |
    ForEach-Object {
        Write-Host "Closing window: $($_.MainWindowTitle) ..."
        $_.CloseMainWindow() | Out-Null
        Start-Sleep -Milliseconds 300
        if (-not $_.HasExited) { $_.Kill() }
    }

Write-Host ""
if ($anyStopped) { Write-Host "Done. All services stopped and windows closed." }
else             { Write-Host "No services were running."                       }
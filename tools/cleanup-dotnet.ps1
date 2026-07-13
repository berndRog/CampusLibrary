<#
.SYNOPSIS
    Deletes generated build artifacts from a .NET solution/project tree
    so that ZIP archives stay small.
    
.DESCRIPTION
    Default:
    - removes bin/, obj/, TestResults/, coverage output, .vs/, .idea/, .vscode/
    - removes macOS ZIP artefacts: .DS_Store and __MACOSX
    - removes log files: *.log

    Optional:
    - -IncludeLocalDb also removes local SQLite files (*.db, *.db-shm, *.db-wal)

    The script does not delete .git.

    NOTE: If script execution is blocked, run once:
    Set-ExecutionPolicy -Scope CurrentUser RemoteSigned

.EXAMPLE
    .\cleanup-dotnet.ps1
    .\cleanup-dotnet.ps1 -Root C:\Projects\MyApp
    .\cleanup-dotnet.ps1 -DryRun
    .\cleanup-dotnet.ps1 -IncludeLocalDb
    
    # Aktuelles Verzeichnis
    .\tools\cleanup-dotnet.ps1
    
    # Bestimmter Pfad
    .\tools\cleanup-dotnet.ps1 -Root C:\Projects\MyApp
    
    # Dry-Run
    .\tools\cleanup-dotnet.ps1 -DryRun
    
    # Mit SQLite-Dateien
    .\tools\cleanup-dotnet.ps1 -IncludeLocalDb
#>

param(
    [string] $Root        = (Get-Location).Path,
    [switch] $DryRun,
    [switch] $IncludeLocalDb
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Root -PathType Container)) {
    Write-Error "Path does not exist or is not a directory: $Root"
    exit 1
}

$Root = (Resolve-Path -LiteralPath $Root).Path

Write-Host "Cleaning generated artifacts below: $Root"
if ($DryRun) {
    Write-Host "Mode: dry-run"
}

function Remove-Item-Safe {
    param([string]$Target)
    if ($DryRun) {
        Write-Host "Would delete: $Target"
    } else {
        Write-Host "Deleting: $Target"
        Remove-Item -LiteralPath $Target -Recurse -Force
    }
}

# Directory names to remove
$dirNames = @('bin','obj','TestResults','coverage','.vs','.idea','.vscode','__MACOSX')

Get-ChildItem -LiteralPath $Root -Recurse -Directory -Force |
    Where-Object {
        $dirNames -contains $_.Name -and
        $_.FullName -notmatch [regex]::Escape("$Root\.git")
    } |
    # Sort descending by depth so children are removed before parents
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object { Remove-Item-Safe $_.FullName }

# File patterns to remove
$filePatterns = @('*.DS_Store','*.trx','*.coverage','*.coveragexml','*.log')

Get-ChildItem -LiteralPath $Root -Recurse -File -Force |
    Where-Object {
        $_.FullName -notmatch [regex]::Escape("$Root\.git") -and
        ($filePatterns | Where-Object { $_.Name -like $_ })
    } |
    ForEach-Object { Remove-Item-Safe $_.FullName }

if ($IncludeLocalDb) {
    $dbPatterns = @('*.db','*.db-shm','*.db-wal')

    Get-ChildItem -LiteralPath $Root -Recurse -File -Force |
        Where-Object {
            $_.FullName -notmatch [regex]::Escape("$Root\.git") -and
            ($dbPatterns | Where-Object { $_.Name -like $_ })
        } |
        ForEach-Object { Remove-Item-Safe $_.FullName }
}

Write-Host "Done."
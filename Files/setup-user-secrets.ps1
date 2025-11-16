Param(
    [string]$ProjectPath = "ProductsDemoApplication/ProductsDemoApplication.csproj",
    [string]$Username = "test_user",
    [string]$Password = "test_password"
)

Write-Host "Setting BasicAuth user-secrets for project: $ProjectPath" -ForegroundColor Cyan

# Resolve project path relative to script location
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$FullProjectPath = Resolve-Path -Path (Join-Path $RepoRoot $ProjectPath)

if (-not (Test-Path $FullProjectPath)) {
    Write-Error "Project not found at: $FullProjectPath"
    exit 1
}

# Initialize user secrets (idempotent)
dotnet user-secrets init --project "$FullProjectPath"

# Set credentials
dotnet user-secrets set "BasicAuth:Username" "$Username" --project "$FullProjectPath"
dotnet user-secrets set "BasicAuth:Password" "$Password" --project "$FullProjectPath"

Write-Host "User secrets configured. Run the API with 'dotnet run --launch-profile https --project $ProjectPath'" -ForegroundColor Green
Write-Host "Note: Do NOT commit secrets to appsettings.json. This script uses the local user-secrets store." -ForegroundColor Yellow


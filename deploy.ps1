param(
    [Parameter(Mandatory=$true)]
    [string]$Destination  # e.g. jason@ssh.tendimensions.com
)

$RemoteDir = "~/discordbggcollection"
$TempDir = Join-Path $env:TEMP "discordbggcollection-deploy"

Write-Host "Staging files to $TempDir..."

# Clean and recreate temp staging dir
if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }
New-Item -ItemType Directory -Path $TempDir | Out-Null

# Copy source, excluding build artifacts and secrets
$ExcludeDirs = @("bin", "obj", ".vs", ".git")
robocopy "." $TempDir /E /XD $ExcludeDirs /XF "appsettings.json" "NewToken.txt" "*.user" | Out-Null

Write-Host "Copying to ${Destination}:${RemoteDir}..."
ssh $Destination "mkdir -p $RemoteDir"
scp -r "$TempDir\*" "${Destination}:${RemoteDir}/"

if ($LASTEXITCODE -ne 0) {
    Write-Error "scp failed. Aborting."
    exit 1
}

Write-Host "Cleaning up staging dir..."
Remove-Item -Recurse -Force $TempDir

Write-Host "Running deploy script on remote..."
ssh $Destination "sed -i 's/\r//' $RemoteDir/deploy.sh && bash $RemoteDir/deploy.sh"

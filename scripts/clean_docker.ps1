$REPO_ROOT = Split-Path -Parent $PSScriptRoot
Write-Host "REPO_ROOT: $REPO_ROOT"

$removeImages = $args -contains "--images"

Set-Location $REPO_ROOT

Write-Host "Stopping and removing containers + volumes + networks..."
docker compose down --volumes --remove-orphans

if ($removeImages) {
    Write-Host "Removing built images..."
    docker compose down --rmi local
}

Write-Host "Done."

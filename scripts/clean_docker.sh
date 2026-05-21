#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
echo "REPO_ROOT: $REPO_ROOT"

REMOVE_IMAGES=false
for arg in "$@"; do
  [[ "$arg" == "--images" ]] && REMOVE_IMAGES=true
done

cd "$REPO_ROOT"

echo "Stopping and removing containers + volumes + networks..."
docker compose down --volumes --remove-orphans

if $REMOVE_IMAGES; then
  echo "Removing built images..."
  docker compose down --rmi local
fi

echo "Done."

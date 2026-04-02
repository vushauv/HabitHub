#!/usr/bin/env bash
set -euo pipefail

SERVICES=(frontend-unit-test frontend-integration-test backend-unit-test backend-integration-test)

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
echo "REPO_ROOT: $REPO_ROOT"
cd "$REPO_ROOT"

COMPOSE="docker compose -f docker-compose.test.yml"
EXIT_CODE=0

run() {
  local service=$1
  echo ""
  echo "==> Running $service..."
  $COMPOSE run --rm "$service" || EXIT_CODE=1
  echo "==> $service done (exit: $?)"
}

if [ $# -eq 1 ]; then
  SERVICE=$1
  if [[ ! " ${SERVICES[*]} " =~ " ${SERVICE} " ]]; then
    echo "Unknown service: $SERVICE"
    echo "Available: ${SERVICES[*]}"
    exit 1
  fi
  run "$SERVICE"
else
  for SERVICE in "${SERVICES[@]}"; do
    run "$SERVICE"
  done
fi

$COMPOSE down --volumes 2>/dev/null || true

echo ""
if [ $EXIT_CODE -eq 0 ]; then
  echo "All tests passed."
else
  echo "One or more test suites failed."
fi

exit $EXIT_CODE

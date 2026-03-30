#!/usr/bin/env bash
set -euo pipefail

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

run frontend-test
run backend-unit-test
run backend-integration-test

$COMPOSE down --volumes 2>/dev/null || true

echo ""
if [ $EXIT_CODE -eq 0 ]; then
  echo "All tests passed."
else
  echo "One or more test suites failed."
fi

exit $EXIT_CODE

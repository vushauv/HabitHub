#!/bin/sh
set -e

HOST_UID=$(stat -c '%u' /src)
HOST_GID=$(stat -c '%g' /src)

if [ "$HOST_UID" != "0" ]; then
    groupadd -g "$HOST_GID" -o devgroup 2>/dev/null || true
    useradd -u "$HOST_UID" -g "$HOST_GID" -o -m -d /home/devuser devuser 2>/dev/null || true

    mkdir -p /tmp/nuget /tmp/dotnet_cli
    chown "$HOST_UID:$HOST_GID" /tmp/nuget /tmp/dotnet_cli

    export NUGET_PACKAGES=/tmp/nuget
    export DOTNET_CLI_HOME=/tmp/dotnet_cli

    exec gosu devuser "$@"
fi

exec "$@"

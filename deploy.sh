#!/bin/bash
set -e

CONTAINER_NAME="discordbggcollection"
IMAGE_NAME="discordbggcollection"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APPSETTINGS_PATH="$SCRIPT_DIR/appsettings.json"

if [ ! -f "$APPSETTINGS_PATH" ]; then
    echo "ERROR: $APPSETTINGS_PATH not found. Place your appsettings.json in your home directory before deploying."
    exit 1
fi

echo "Building Docker image..."
docker build -t "$IMAGE_NAME" "$SCRIPT_DIR"

echo "Stopping existing container (if running)..."
docker stop "$CONTAINER_NAME" 2>/dev/null || true
docker rm "$CONTAINER_NAME" 2>/dev/null || true

echo "Starting container..."
docker run -d \
    --name "$CONTAINER_NAME" \
    --restart unless-stopped \
    -v "$APPSETTINGS_PATH:/app/appsettings.json:ro" \
    -e DOTNET_ENVIRONMENT=Production \
    "$IMAGE_NAME"

echo "Done. Container status:"
docker ps --filter "name=$CONTAINER_NAME"

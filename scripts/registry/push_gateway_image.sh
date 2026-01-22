#!/usr/bin/env bash
set -euo pipefail

REGISTRY="${REGISTRY:-registry.tasso.dev.br}"
IMAGE="${IMAGE:-gestauto/traefik-gateway}"
TAG="${1:-latest}"

FULL_IMAGE="${REGISTRY}/${IMAGE}:${TAG}"

echo "Push da imagem ${FULL_IMAGE}"

docker push "${FULL_IMAGE}"

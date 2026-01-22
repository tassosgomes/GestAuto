#!/usr/bin/env bash
set -euo pipefail

REGISTRY="${REGISTRY:-registry.tasso.dev.br}"
IMAGE="${IMAGE:-gestauto/traefik-gateway}"
TAG="${1:-latest}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

DOCKERFILE_PATH="${REPO_ROOT}/traefik/Dockerfile"
CONTEXT_DIR="${REPO_ROOT}/traefik"

if [[ ! -f "${DOCKERFILE_PATH}" ]]; then
  echo "Dockerfile não encontrado em ${DOCKERFILE_PATH}" >&2
  exit 1
fi

echo "Build da imagem ${REGISTRY}/${IMAGE}:${TAG}"

docker build \
  -f "${DOCKERFILE_PATH}" \
  -t "${REGISTRY}/${IMAGE}:${TAG}" \
  "${CONTEXT_DIR}"

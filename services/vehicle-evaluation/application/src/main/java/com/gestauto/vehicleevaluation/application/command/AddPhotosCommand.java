package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import java.util.Map;
import java.util.UUID;

public record AddPhotosCommand(UUID evaluationId, Map<String, ImageUploadRequest> photos) {
}

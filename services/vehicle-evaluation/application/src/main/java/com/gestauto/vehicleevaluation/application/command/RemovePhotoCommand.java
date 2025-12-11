package com.gestauto.vehicleevaluation.application.command;

import java.util.UUID;

public record RemovePhotoCommand(UUID evaluationId, String photoType) {
}

package com.gestauto.vehicleevaluation.domain.value;

import java.util.Map;

public record ImageUploadResult(Map<String, UploadedPhoto> uploadedPhotos, Map<String, String> errors) {
}

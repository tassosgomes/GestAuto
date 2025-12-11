package com.gestauto.vehicleevaluation.domain.value;

import java.util.Map;

public record ImageUploadResult(Map<String, String> uploadedUrls, Map<String, String> errors) {
}

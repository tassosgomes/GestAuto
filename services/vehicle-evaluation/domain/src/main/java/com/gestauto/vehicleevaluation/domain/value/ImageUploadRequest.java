package com.gestauto.vehicleevaluation.domain.value;

import java.io.InputStream;

public record ImageUploadRequest(InputStream content, String fileName, String contentType, long size) {
}

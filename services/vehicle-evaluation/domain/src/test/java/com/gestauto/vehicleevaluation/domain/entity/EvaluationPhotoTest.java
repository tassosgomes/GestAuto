package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("EvaluationPhoto Entity Tests")
class EvaluationPhotoTest {

    @Test
    void shouldCreateAndDetectTypeAndQuality() {
        EvaluationId evaluationId = EvaluationId.generate();

        EvaluationPhoto photo = EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            3L * 1024 * 1024,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        );

        assertNotNull(photo.getPhotoId());
        assertEquals(evaluationId, photo.getEvaluationId());
        assertTrue(photo.isHighQuality());
        assertTrue(photo.isJpeg());
        assertFalse(photo.isPng());
        assertTrue(photo.getFormattedFileSize().contains("MB"));

        photo.updateThumbnailUrl(" ");
        assertNull(photo.getThumbnailUrl());

        photo.updateThumbnailUrl("http://example.test/thumb.jpg");
        assertEquals("http://example.test/thumb.jpg", photo.getThumbnailUrl());
    }

    @Test
    void shouldValidateInputs() {
        EvaluationId evaluationId = EvaluationId.generate();

        assertThrows(IllegalArgumentException.class, () -> EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            " ",
            "/photos/front.jpg",
            100,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        assertThrows(IllegalArgumentException.class, () -> EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            " ",
            100,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        assertThrows(IllegalArgumentException.class, () -> EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            -1,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        assertThrows(IllegalArgumentException.class, () -> EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            1,
            "text/plain",
            "http://example.test/front.jpg",
            null
        ));

        assertThrows(IllegalArgumentException.class, () -> EvaluationPhoto.create(
            evaluationId,
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            1,
            "image/png",
            "   ",
            null
        ));
    }

    @Test
    void shouldSupportCreateWithoutThumbnailRestoreAndPng() {
        EvaluationId evaluationId = EvaluationId.generate();

        EvaluationPhoto withoutThumb = EvaluationPhoto.createWithoutThumbnail(
            evaluationId,
            PhotoType.INTERIOR_FRONT,
            "inside.png",
            "/photos/inside.png",
            512,
            "image/png",
            "http://example.test/inside.png"
        );
        assertNull(withoutThumb.getThumbnailUrl());
        assertTrue(withoutThumb.isPng());
        assertFalse(withoutThumb.isJpeg());
        assertTrue(withoutThumb.getFormattedFileSize().contains("B") || withoutThumb.getFormattedFileSize().contains("KB"));

        EvaluationPhoto restored = EvaluationPhoto.restore(
            "photo-1",
            evaluationId,
            PhotoType.INTERIOR_FRONT,
            "inside.png",
            "/photos/inside.png",
            512,
            "image/png",
            "http://example.test/inside.png",
            "http://example.test/thumb.png",
            withoutThumb.getUploadedAt()
        );

        assertEquals("photo-1", restored.getPhotoId());
        assertEquals("http://example.test/thumb.png", restored.getThumbnailUrl());
        assertNotNull(restored.toString());
    }
}

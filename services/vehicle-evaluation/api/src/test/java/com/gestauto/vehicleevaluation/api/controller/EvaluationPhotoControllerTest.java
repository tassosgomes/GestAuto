package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.AddPhotosCommand;
import com.gestauto.vehicleevaluation.application.command.CommandHandler;
import com.gestauto.vehicleevaluation.application.command.RemovePhotoCommand;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import org.springframework.mock.web.MockMultipartFile;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.delete;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.multipart;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

class EvaluationPhotoControllerTest {

    @Test
    void uploadPhotosReturns200() throws Exception {
        @SuppressWarnings("unchecked")
        CommandHandler<AddPhotosCommand, Void> addPhotosHandler = mock(CommandHandler.class);
        @SuppressWarnings("unchecked")
        CommandHandler<RemovePhotoCommand, Void> removePhotoHandler = mock(CommandHandler.class);

        EvaluationPhotoController controller = new EvaluationPhotoController(addPhotosHandler, removePhotoHandler);
        MockMvc mockMvc = MockMvcBuilders.standaloneSetup(controller).build();

        UUID evaluationId = UUID.randomUUID();
        MockMultipartFile photo = new MockMultipartFile(
            "front",
            "front.jpg",
            "image/jpeg",
            "fake-image".getBytes()
        );

        mockMvc.perform(multipart("/api/v1/evaluations/{id}/photos", evaluationId)
                .file(photo))
            .andExpect(status().isOk());

        verify(addPhotosHandler).handle(any());
    }

    @Test
    void removePhotoReturns204() throws Exception {
        @SuppressWarnings("unchecked")
        CommandHandler<AddPhotosCommand, Void> addPhotosHandler = mock(CommandHandler.class);
        @SuppressWarnings("unchecked")
        CommandHandler<RemovePhotoCommand, Void> removePhotoHandler = mock(CommandHandler.class);

        EvaluationPhotoController controller = new EvaluationPhotoController(addPhotosHandler, removePhotoHandler);
        MockMvc mockMvc = MockMvcBuilders.standaloneSetup(controller).build();

        UUID evaluationId = UUID.randomUUID();

        mockMvc.perform(delete("/api/v1/evaluations/{id}/photos/{type}", evaluationId, "front"))
            .andExpect(status().isNoContent());

        verify(removePhotoHandler).handle(any());
    }
}

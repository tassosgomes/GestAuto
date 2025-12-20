package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.dto.EvaluationValidationDto;
import com.gestauto.vehicleevaluation.application.query.ValidateEvaluationPublicHandler;
import com.gestauto.vehicleevaluation.application.query.ValidateEvaluationPublicQuery;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/evaluations/public")
@RequiredArgsConstructor
@Slf4j
@Tag(name = "Validação Pública", description = "Validação pública de laudos via token")
public class PublicValidationController {

    private final ValidateEvaluationPublicHandler validateHandler;

    @GetMapping("/validate/{token}")
    public ResponseEntity<EvaluationValidationDto> validate(@PathVariable String token) {
        EvaluationValidationDto result = validateHandler.handle(new ValidateEvaluationPublicQuery(token));
        if (result == null) {
            return ResponseEntity.notFound().build();
        }
        return ResponseEntity.ok(result);
    }
}

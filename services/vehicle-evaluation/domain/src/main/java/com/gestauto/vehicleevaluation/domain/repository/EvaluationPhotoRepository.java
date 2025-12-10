package com.gestauto.vehicleevaluation.domain.repository;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;

import java.util.List;
import java.util.Optional;

/**
 * Repositório para persistência de fotos de avaliação.
 *
 * Interface de domínio que define operações de persistência
 * para fotos associadas a avaliações de veículos.
 */
public interface EvaluationPhotoRepository {

    /**
     * Salva uma foto de avaliação.
     *
     * @param photo foto a ser salva
     * @return foto salva com ID gerado
     */
    EvaluationPhoto save(EvaluationPhoto photo);

    /**
     * Remove uma foto por ID.
     *
     * @param photoId ID da foto a ser removida
     */
    void deleteById(String photoId);

    /**
     * Busca foto por ID.
     *
     * @param photoId ID da foto
     * @return foto encontrada ou Optional.empty()
     */
    Optional<EvaluationPhoto> findById(String photoId);

    /**
     * Lista fotos por avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return lista de fotos da avaliação
     */
    List<EvaluationPhoto> findByEvaluationId(EvaluationId evaluationId);

    /**
     * Busca foto por avaliação e tipo.
     *
     * @param evaluationId ID da avaliação
     * @param photoType tipo da foto
     * @return foto encontrada ou Optional.empty()
     */
    Optional<EvaluationPhoto> findByEvaluationIdAndPhotoType(EvaluationId evaluationId, PhotoType photoType);

    /**
     * Lista fotos por tipo.
     *
     * @param photoType tipo da foto
     * @return lista de fotos do tipo especificado
     */
    List<EvaluationPhoto> findByPhotoType(PhotoType photoType);

    /**
     * Verifica se existe foto para a avaliação e tipo.
     *
     * @param evaluationId ID da avaliação
     * @param photoType tipo da foto
     * @return true se existir foto do tipo para a avaliação
     */
    boolean existsByEvaluationIdAndPhotoType(EvaluationId evaluationId, PhotoType photoType);

    /**
     * Conta fotos por avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return quantidade de fotos da avaliação
     */
    long countByEvaluationId(EvaluationId evaluationId);

    /**
     * Remove todas as fotos de uma avaliação.
     *
     * @param evaluationId ID da avaliação
     */
    void deleteAllByEvaluationId(EvaluationId evaluationId);

    /**
     * Lista fotos grandes (acima de 2MB) para otimização.
     *
     * @return lista de fotos que precisam de otimização
     */
    List<EvaluationPhoto> findLargePhotos();

    /**
     * Atualiza URL do thumbnail da foto.
     *
     * @param photoId ID da foto
     * @param thumbnailUrl novo URL do thumbnail
     */
    void updateThumbnailUrl(String photoId, String thumbnailUrl);
}
package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationChecklistRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.infra.mapper.EvaluationChecklistMapper;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional
public class EvaluationChecklistRepositoryImpl implements EvaluationChecklistRepository {

    private final EvaluationChecklistJpaRepository jpaRepository;
    private final VehicleEvaluationJpaRepository evaluationJpaRepository;

    public EvaluationChecklistRepositoryImpl(EvaluationChecklistJpaRepository jpaRepository,
                                             VehicleEvaluationJpaRepository evaluationJpaRepository) {
        this.jpaRepository = jpaRepository;
        this.evaluationJpaRepository = evaluationJpaRepository;
    }

    @Override
    public EvaluationChecklist save(EvaluationChecklist checklist) {
        var evaluation = evaluationJpaRepository.findById(
            UUID.fromString(checklist.getEvaluationId().getValueAsString()))
            .orElseThrow(() -> new IllegalArgumentException("Evaluation not found for checklist"));
        var entity = EvaluationChecklistMapper.toEntity(checklist, evaluation);
        var saved = jpaRepository.save(entity);
        return EvaluationChecklistMapper.toDomain(saved);
    }

    @Override
    public EvaluationChecklist update(EvaluationChecklist checklist) {
        return save(checklist);
    }

    @Override
    public Optional<EvaluationChecklist> findByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.findByEvaluationId(UUID.fromString(evaluationId.getValueAsString()))
            .map(EvaluationChecklistMapper::toDomain);
    }

    @Override
    public void deleteByEvaluationId(EvaluationId evaluationId) {
        jpaRepository.deleteByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public List<EvaluationChecklist> findIncomplete() {
        return jpaRepository.findByConservationScoreIsNull().stream()
            .map(EvaluationChecklistMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<EvaluationChecklist> findByOverallScoreLessThan(double minScore) {
        return jpaRepository.findByConservationScoreLessThan((int) minScore).stream()
            .map(EvaluationChecklistMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<EvaluationChecklist> findWithCriticalIssues() {
        return jpaRepository.findWithCriticalIssues().stream()
            .map(EvaluationChecklistMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public boolean existsByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.existsByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public long countCompletedByEvaluator(String evaluatorId) {
        return jpaRepository.countCompletedByEvaluator(evaluatorId);
    }

    @Override
    public List<EvaluationChecklist> findByCreatedAtBetween(java.time.LocalDateTime startDate,
                                                            java.time.LocalDateTime endDate) {
        return jpaRepository.findByCreatedAtBetween(startDate, endDate).stream()
            .map(EvaluationChecklistMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<EvaluationChecklist> findByEvaluatorId(String evaluatorId) {
        return jpaRepository.findByEvaluatorId(evaluatorId).stream()
            .map(EvaluationChecklistMapper::toDomain)
            .collect(Collectors.toList());
    }
}

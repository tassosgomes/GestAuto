package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.repository.DepreciationItemRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.infra.mapper.DepreciationItemMapper;
import com.gestauto.vehicleevaluation.infra.mapper.MoneyMapper;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional
public class DepreciationItemRepositoryImpl implements DepreciationItemRepository {

    private final DepreciationItemJpaRepository jpaRepository;
    private final VehicleEvaluationJpaRepository evaluationJpaRepository;

    public DepreciationItemRepositoryImpl(DepreciationItemJpaRepository jpaRepository,
                                          VehicleEvaluationJpaRepository evaluationJpaRepository) {
        this.jpaRepository = jpaRepository;
        this.evaluationJpaRepository = evaluationJpaRepository;
    }

    @Override
    public DepreciationItem save(DepreciationItem depreciationItem) {
        var evaluation = evaluationJpaRepository.findById(
            UUID.fromString(depreciationItem.getEvaluationId().getValueAsString()))
            .orElseThrow(() -> new IllegalArgumentException("Evaluation not found for depreciation item"));
        var entity = DepreciationItemMapper.toEntity(depreciationItem, evaluation);
        var saved = jpaRepository.save(entity);
        return DepreciationItemMapper.toDomain(saved);
    }

    @Override
    public void deleteById(String depreciationId) {
        jpaRepository.deleteById(UUID.fromString(depreciationId));
    }

    @Override
    public Optional<DepreciationItem> findById(String depreciationId) {
        return jpaRepository.findById(UUID.fromString(depreciationId)).map(DepreciationItemMapper::toDomain);
    }

    @Override
    public List<DepreciationItem> findByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.findByEvaluationId(UUID.fromString(evaluationId.getValueAsString())).stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<DepreciationItem> findByCategory(String category) {
        return jpaRepository.findByCategory(category).stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<DepreciationItem> findByEvaluationIdAndCategory(EvaluationId evaluationId, String category) {
        return jpaRepository.findByEvaluationIdAndCategory(UUID.fromString(evaluationId.getValueAsString()), category).stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<DepreciationItem> findByCreatedBy(String createdBy) {
        return jpaRepository.findByCreatedBy(createdBy).stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public Money sumDepreciationByEvaluationId(EvaluationId evaluationId) {
        var total = jpaRepository.sumByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
        return MoneyMapper.toDomain(total);
    }

    @Override
    public long countByEvaluationId(EvaluationId evaluationId) {
        return jpaRepository.countByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public void deleteAllByEvaluationId(EvaluationId evaluationId) {
        jpaRepository.deleteAllByEvaluationId(UUID.fromString(evaluationId.getValueAsString()));
    }

    @Override
    public List<DepreciationItem> findByDepreciationValueGreaterThan(Money minValue) {
        return jpaRepository.findByDepreciationValueAmountGreaterThan(MoneyMapper.toAmount(minValue)).stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<DepreciationItem> findByEvaluationIdAndCreatedAtBetween(EvaluationId evaluationId,
                                                                       java.time.LocalDateTime startDate,
                                                                       java.time.LocalDateTime endDate) {
        return jpaRepository.findByEvaluationIdAndCreatedAtBetween(
                UUID.fromString(evaluationId.getValueAsString()), startDate, endDate)
            .stream()
            .map(DepreciationItemMapper::toDomain)
            .collect(Collectors.toList());
    }
}

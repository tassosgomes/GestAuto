package com.gestauto.vehicleevaluation.infra.repository;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import com.gestauto.vehicleevaluation.infra.mapper.MoneyMapper;
import com.gestauto.vehicleevaluation.infra.mapper.VehicleEvaluationMapper;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Collectors;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Repository;
import org.springframework.transaction.annotation.Transactional;

@Repository
@Transactional
public class VehicleEvaluationRepositoryImpl implements VehicleEvaluationRepository {

    private final VehicleEvaluationJpaRepository jpaRepository;

    public VehicleEvaluationRepositoryImpl(VehicleEvaluationJpaRepository jpaRepository) {
        this.jpaRepository = jpaRepository;
    }

    @Override
    public VehicleEvaluation save(VehicleEvaluation evaluation) {
        VehicleEvaluationJpaEntity entity = VehicleEvaluationMapper.toEntity(evaluation);
        VehicleEvaluationJpaEntity saved = jpaRepository.save(entity);
        return VehicleEvaluationMapper.toDomain(saved);
    }

    @Override
    public VehicleEvaluation update(VehicleEvaluation evaluation) {
        return save(evaluation);
    }

    @Override
    public Optional<VehicleEvaluation> findById(EvaluationId id) {
        return jpaRepository.findById(UUID.fromString(id.getValueAsString()))
            .map(VehicleEvaluationMapper::toDomain);
    }

    @Override
    public Optional<VehicleEvaluation> findByValidationToken(String validationToken) {
        return jpaRepository.findByValidationToken(validationToken)
            .map(VehicleEvaluationMapper::toDomain);
    }

    @Override
    public List<VehicleEvaluation> findByPlate(Plate plate) {
        return jpaRepository.findByPlate(plate.getValue()).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByStatus(EvaluationStatus status) {
        return jpaRepository.findByStatus(EvaluationStatusJpa.valueOf(status.name())).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByEvaluator(String evaluatorId) {
        return jpaRepository.findByEvaluatorId(evaluatorId).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByCreatedAtBetween(LocalDateTime startDate, LocalDateTime endDate) {
        return jpaRepository.findByCreatedAtBetween(startDate, endDate).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByStatusAndEvaluator(EvaluationStatus status, String evaluatorId) {
        return jpaRepository.findByStatusAndEvaluatorId(EvaluationStatusJpa.valueOf(status.name()), evaluatorId).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findPendingApproval(int limit) {
        return jpaRepository.findByStatusOrderByCreatedAtAsc(
                EvaluationStatusJpa.PENDING_APPROVAL, PageRequest.of(0, limit))
            .stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findPendingApprovals(EvaluationStatus status, int page, int size) {
        Pageable pageable = PageRequest.of(page, size);
        return jpaRepository.findByStatus(EvaluationStatusJpa.valueOf(status.name()), pageable)
            .stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findExpiringSoon(int hoursUntilExpiration) {
        LocalDateTime limit = LocalDateTime.now().plusHours(hoursUntilExpiration);
        return jpaRepository.findExpiring(EvaluationStatusJpa.APPROVED, limit).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public List<VehicleEvaluation> findByFinalValueBetween(Money minValue, Money maxValue) {
        BigDecimal min = MoneyMapper.toAmount(minValue);
        BigDecimal max = MoneyMapper.toAmount(maxValue);
        return jpaRepository.findByFinalValueAmountBetween(min, max).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public boolean existsByPlateAndStatus(Plate plate, EvaluationStatus status) {
        return jpaRepository.existsByPlateAndStatus(plate.getValue(), EvaluationStatusJpa.valueOf(status.name()));
    }

    @Override
    public long countByStatus(EvaluationStatus status) {
        return jpaRepository.countByStatus(EvaluationStatusJpa.valueOf(status.name()));
    }

    @Override
    public List<VehicleEvaluation> findAll(int page, int size) {
        return jpaRepository.findAll(PageRequest.of(page, size)).stream()
            .map(VehicleEvaluationMapper::toDomain)
            .collect(Collectors.toList());
    }

    @Override
    public void deleteById(EvaluationId id) {
        jpaRepository.deleteById(UUID.fromString(id.getValueAsString()));
    }

    @Override
    public boolean existsById(EvaluationId id) {
        return jpaRepository.existsById(UUID.fromString(id.getValueAsString()));
    }
}

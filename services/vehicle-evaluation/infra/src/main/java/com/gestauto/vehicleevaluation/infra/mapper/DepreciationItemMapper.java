package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.infra.entity.DepreciationItemJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import java.util.UUID;

/**
 * Mapper para itens de depreciação.
 */
public final class DepreciationItemMapper {

    private DepreciationItemMapper() {
    }

    public static DepreciationItemJpaEntity toEntity(DepreciationItem item, VehicleEvaluationJpaEntity evaluationJpa) {
        if (item == null) {
            return null;
        }
        DepreciationItemJpaEntity entity = new DepreciationItemJpaEntity();
        entity.setDepreciationId(UUID.fromString(item.getDepreciationId()));
        entity.setEvaluation(evaluationJpa);
        entity.setCategory(item.getCategory());
        entity.setDescription(item.getDescription());
        entity.setDepreciationValueAmount(MoneyMapper.toAmount(item.getDepreciationValue()));
        entity.setDepreciationValueCurrency(MoneyMapper.toCurrency(item.getDepreciationValue()));
        entity.setJustification(item.getJustification());
        entity.setCreatedAt(item.getCreatedAt());
        entity.setCreatedBy(item.getCreatedBy());
        return entity;
    }

    public static DepreciationItem toDomain(DepreciationItemJpaEntity entity) {
        if (entity == null) {
            return null;
        }
        Money money = MoneyMapper.fromAmountAndCurrency(
            entity.getDepreciationValueAmount(),
            entity.getDepreciationValueCurrency()
        );
        return DepreciationItem.restore(
            entity.getDepreciationId().toString(),
            EvaluationId.from(entity.getEvaluation().getId()),
            entity.getCategory(),
            entity.getDescription(),
            money,
            entity.getJustification(),
            entity.getCreatedBy(),
            entity.getCreatedAt()
        );
    }
}

package com.gestauto.vehicleevaluation.application.mapper;

import com.gestauto.vehicleevaluation.application.dto.*;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import org.mapstruct.Mapper;
import org.mapstruct.Mapping;
import org.mapstruct.MappingTarget;
import org.mapstruct.Named;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

/**
 * Mapper MapStruct para conversão entre entidade de domínio e DTOs.
 *
 * Este mapper utiliza o padrão MapStruct para gerar automaticamente
 * as conversões entre VehicleEvaluation e seus respectivos DTOs.
 */
@Mapper(componentModel = "spring")
public interface VehicleEvaluationMapper {

    /**
     * Converte entidade VehicleEvaluation para VehicleEvaluationDto completo.
     *
     * @param evaluation entidade de domínio
     * @return DTO completo da avaliação
     */
    @Mapping(source = "id.value", target = "id")
    @Mapping(source = "plate.formatted", target = "plate")
    @Mapping(source = "vehicleInfo.brand", target = "brand")
    @Mapping(source = "vehicleInfo.model", target = "model")
    @Mapping(source = "vehicleInfo.yearModel", target = "year")
    @Mapping(source = "mileage.amount", target = "mileage")
    @Mapping(source = "vehicleInfo.color", target = "color")
    @Mapping(source = "vehicleInfo.version", target = "version")
    @Mapping(source = "vehicleInfo.fuelType.description", target = "fuelType")
    @Mapping(source = "fipePrice.amount", target = "fipePrice")
    @Mapping(source = "baseValue.amount", target = "baseValue")
    @Mapping(source = "finalValue.amount", target = "finalValue")
    @Mapping(source = "approvedValue.amount", target = "approvedValue")
    @Mapping(source = "status", target = "status", qualifiedByName = "statusToString")
    @Mapping(source = "photos", target = "photos", qualifiedByName = "photosToDtos")
    @Mapping(source = "depreciationItems", target = "depreciationItems", qualifiedByName = "depreciationItemsToDtos")
    @Mapping(source = "checklist", target = "checklist", qualifiedByName = "checklistToDto")
    VehicleEvaluationDto toDto(VehicleEvaluation evaluation);

    /**
     * Converte entidade VehicleEvaluation para VehicleEvaluationSummaryDto.
     *
     * @param evaluation entidade de domínio
     * @return DTO resumido da avaliação
     */
    @Mapping(source = "id.value", target = "id")
    @Mapping(source = "plate.formatted", target = "plate")
    @Mapping(source = "vehicleInfo.brand", target = "brand")
    @Mapping(source = "vehicleInfo.model", target = "model")
    @Mapping(source = "vehicleInfo.yearModel", target = "year")
    @Mapping(source = "mileage.amount", target = "mileage")
    @Mapping(source = "status", target = "status", qualifiedByName = "statusToString")
    @Mapping(source = "finalValue.amount", target = "finalValue")
    VehicleEvaluationSummaryDto toSummaryDto(VehicleEvaluation evaluation);

    /**
     * Converte lista de entidades VehicleEvaluation para lista de DTOs resumidos.
     *
     * @param evaluations lista de entidades
     * @return lista de DTOs resumidos
     */
    List<VehicleEvaluationSummaryDto> toSummaryDtoList(List<VehicleEvaluation> evaluations);

    /**
     * Converte EvaluationPhoto para EvaluationPhotoDto.
     *
     * @param photo entidade de foto
     * @return DTO de foto
     */
    @Mapping(source = "photoId", target = "photoId")
    @Mapping(source = "photoType.description", target = "photoType")
    @Mapping(source = "uploadedAt", target = "uploadedAt")
    EvaluationPhotoDto toPhotoDto(EvaluationPhoto photo);

    /**
     * Converte lista de fotos para lista de DTOs.
     *
     * @param photos lista de entidades de fotos
     * @return lista de DTOs de fotos
     */
    @Named("photosToDtos")
    List<EvaluationPhotoDto> photosToDtos(List<EvaluationPhoto> photos);

    /**
     * Converte DepreciationItem para DepreciationItemDto.
     *
     * @param item entidade de depreciação
     * @return DTO de depreciação
     */
    @Mapping(source = "category", target = "category")
    @Mapping(source = "depreciationValue.amount", target = "depreciationValue")
    @Mapping(target = "depreciationPercentage", ignore = true)
    DepreciationItemDto toDepreciationItemDto(DepreciationItem item);

    /**
     * Converte lista de itens de depreciação para lista de DTOs.
     *
     * @param items lista de entidades de depreciação
     * @return lista de DTOs de depreciação
     */
    @Named("depreciationItemsToDtos")
    List<DepreciationItemDto> depreciationItemsToDtos(List<DepreciationItem> items);

    /**
     * Converte EvaluationChecklist para EvaluationChecklistDto.
     *
     * @param checklist entidade de checklist
     * @return DTO de checklist
     */
    @Named("checklistToDto")
    EvaluationChecklistDto toChecklistDto(EvaluationChecklist checklist);

    /**
     * Converte enum EvaluationStatus para String.
     *
     * @param status enum de status
     * @return string do status
     */
    @Named("statusToString")
    default String statusToString(com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus status) {
        return status != null ? status.name() : null;
    }

    /**
     * Converte BigDecimal para String (evitando problemas com formatação).
     *
     * @param value valor BigDecimal
     * @return string do valor
     */
    default String bigDecimalToString(BigDecimal value) {
        return value != null ? value.toString() : null;
    }

    /**
     * Converte LocalDateTime para LocalDateTime (mapper necessário para MapStruct).
     *
     * @param dateTime data/hora original
     * @return data/hora convertida
     */
    default LocalDateTime mapLocalDateTime(LocalDateTime dateTime) {
        return dateTime;
    }

    /**
     * Verifica se a avaliação está expirada.
     *
     * @param evaluation entidade de avaliação
     * @return true se estiver expirada
     */
    default boolean isExpired(VehicleEvaluation evaluation) {
        return evaluation.isExpired();
    }
}
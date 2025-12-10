package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.infra.entity.VehicleInfoEmbeddable;

/**
 * Mapper para conversão entre VehicleInfo (domínio) e VehicleInfoEmbeddable (JPA).
 */
public final class VehicleInfoMapper {

    private VehicleInfoMapper() {
    }

    public static VehicleInfoEmbeddable toEmbeddable(VehicleInfo info) {
        if (info == null) {
            return null;
        }
        return new VehicleInfoEmbeddable(
            info.getBrand(),
            info.getModel(),
            info.getYearManufacture(),
            info.getYearModel(),
            info.getColor(),
            info.getFuelType().name(),
            null, // engine not modeled in domain
            null, // transmission not modeled in domain
            null, // doors not modeled in domain
            info.getVersion(),
            null  // chassis not modeled in domain
        );
    }

    public static VehicleInfo toDomain(VehicleInfoEmbeddable embeddable) {
        if (embeddable == null) {
            return null;
        }
        String version = embeddable.getVersion() != null ? embeddable.getVersion() : "UNKNOWN";
        String color = embeddable.getColor() != null ? embeddable.getColor() : "UNKNOWN";
        FuelType fuelType = embeddable.getFuelType() != null
            ? FuelType.valueOf(embeddable.getFuelType())
            : FuelType.GASOLINE;
        return VehicleInfo.of(
            embeddable.getBrand(),
            embeddable.getModel(),
            version,
            embeddable.getYearManufacture(),
            embeddable.getYearModel(),
            color,
            fuelType
        );
    }
}

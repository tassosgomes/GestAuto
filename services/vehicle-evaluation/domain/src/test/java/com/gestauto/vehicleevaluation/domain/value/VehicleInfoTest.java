package com.gestauto.vehicleevaluation.domain.value;

import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import java.time.Year;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("VehicleInfo Value Object Tests")
class VehicleInfoTest {

    @Test
    void shouldCreateAndProvideDerivedProperties() {
        int currentYear = Year.now().getValue();

        VehicleInfo info = VehicleInfo.of(
            "Volkswagen",
            "Gol",
            "1.0",
            currentYear,
            currentYear,
            "Preto",
            FuelType.FLEX
        );

        assertEquals("Volkswagen Gol", info.getFullName());
        assertTrue(info.getFullDescription().contains("1.0"));
        assertTrue(info.isZeroKm());
        assertFalse(info.isNewerModel());
        assertEquals(0, info.getAge());
        assertFalse(info.isClassic());
        assertEquals(FuelType.FLEX, info.getFuelType());
    }

    @Test
    void shouldRejectInvalidYearsAndEmptyFields() {
        int currentYear = Year.now().getValue();

        assertThrows(IllegalArgumentException.class, () -> VehicleInfo.of(
            "Volkswagen", "Gol", "1.0", 1800, currentYear, "Preto", FuelType.FLEX
        ));

        assertThrows(IllegalArgumentException.class, () -> VehicleInfo.of(
            "Volkswagen", "Gol", "1.0", currentYear, currentYear - 1, "Preto", FuelType.FLEX
        ));

        assertThrows(IllegalArgumentException.class, () -> VehicleInfo.of(
            " ", "Gol", "1.0", currentYear, currentYear, "Preto", FuelType.FLEX
        ));
    }

    @Test
    void shouldDetectClassicAndNewerModelAndSupportEquality() {
        int currentYear = Year.now().getValue();

        VehicleInfo newerModel = VehicleInfo.of(
            "Ford",
            "Ka",
            "SE",
            currentYear - 1,
            currentYear,
            "Branco",
            FuelType.GASOLINE
        );

        assertTrue(newerModel.isNewerModel());
        assertFalse(newerModel.isZeroKm());

        VehicleInfo classic = VehicleInfo.of(
            "VW",
            "Fusca",
            "1300",
            currentYear - 40,
            currentYear - 40,
            "Azul",
            FuelType.GASOLINE
        );

        assertTrue(classic.isClassic());

        VehicleInfo viaCreate = VehicleInfo.create("Ford", "Ka", currentYear, "Branco", FuelType.GASOLINE, "SE");
        assertEquals("Ford Ka", viaCreate.getFullName());

        VehicleInfo sameAsViaCreate = VehicleInfo.of("Ford", "Ka", "SE", currentYear, currentYear, "Branco", FuelType.GASOLINE);
        assertEquals(viaCreate, sameAsViaCreate);
        assertEquals(viaCreate.hashCode(), sameAsViaCreate.hashCode());
        assertTrue(viaCreate.toString().contains("VehicleInfo{"));
    }
}

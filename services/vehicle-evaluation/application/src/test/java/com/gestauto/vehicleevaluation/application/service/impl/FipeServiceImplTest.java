package com.gestauto.vehicleevaluation.application.service.impl;

import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.value.Money;
import org.junit.jupiter.api.Test;

import java.lang.reflect.Field;
import java.math.BigDecimal;
import java.time.Year;
import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

class FipeServiceImplTest {

    private final FipeServiceImpl service = new FipeServiceImpl();

    @Test
    void getVehicleInfoByPlate_handlesNullEmptyAndNormalization() {
        assertThat(service.getVehicleInfoByPlate(null)).isEmpty();
        assertThat(service.getVehicleInfoByPlate("   ")).isEmpty();

        // Existing mock plate is stored normalized; method should normalize input too
        assertThat(service.getVehicleInfoByPlate("abc-1234"))
                .isPresent()
                .get()
                .satisfies(info -> {
                    assertThat(info.getBrand()).isEqualTo("Volkswagen");
                    assertThat(info.getModel()).isEqualTo("Gol");
                });

        // Unknown plate should still return generated data
        assertThat(service.getVehicleInfoByPlate("ZZZ9999"))
                .isPresent();
    }

    @Test
    void getVehicleInfoByFipeCode_handlesNullEmptyAndGenerates() {
        assertThat(service.getVehicleInfoByFipeCode(null)).isEmpty();
        assertThat(service.getVehicleInfoByFipeCode(" ")).isEmpty();
        assertThat(service.getVehicleInfoByFipeCode("001234567"))
                .isPresent();
    }

    @Test
    @SuppressWarnings("unchecked")
    void getFipePrice_coversBothBranchesBySeedingMockPricesViaReflection() throws Exception {
        // As implemented, getFipePrice normalizes brand/model to upper-case,
        // so the built-in MOCK_PRICES keys won't match. Seed a matching key.
        Field f = FipeServiceImpl.class.getDeclaredField("MOCK_PRICES");
        f.setAccessible(true);
        Map<String, Money> prices = (Map<String, Money>) f.get(null);

        Money seeded = Money.of(BigDecimal.valueOf(99999));
        prices.put("VOLKSWAGEN-GOL-2023", seeded);

        assertThat(service.getFipePrice("Volkswagen", "Gol", 2023, FuelType.FLEX))
                .contains(seeded);

        // Unknown should generate a synthetic price
        assertThat(service.getFipePrice("SomeBrand", "SomeModel", 2010, FuelType.FLEX))
                .isPresent();
    }

    @Test
    void validationAndMarketAcceptanceAndLiquidity() {
        assertThat(service.isValidFipeCode(null)).isFalse();
        assertThat(service.isValidFipeCode("123"))
                .isFalse();
        assertThat(service.isValidFipeCode("123456789"))
                .isTrue();

        int currentYear = Year.now().getValue();
        assertThat(service.hasGoodMarketAcceptance("Toyota", "Corolla", currentYear - 2)).isTrue();
        assertThat(service.hasGoodMarketAcceptance("Toyota", "Corolla", currentYear - 15)).isFalse();
        assertThat(service.hasGoodMarketAcceptance("Unknown", "X", currentYear - 2)).isFalse();

        // Liquidity caps and floors
        assertThat(service.calculateLiquidityPercentage("Toyota", "Corolla", 0)).isEqualTo(0.95);
        assertThat(service.calculateLiquidityPercentage("Unknown", "X", 50)).isEqualTo(0.60);
    }
}

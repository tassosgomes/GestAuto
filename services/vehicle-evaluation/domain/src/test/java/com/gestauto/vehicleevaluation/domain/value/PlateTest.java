package com.gestauto.vehicleevaluation.domain.value;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("Plate Value Object Tests")
class PlateTest {

    @Test
    void shouldNormalizeAndFormatPlate() {
        Plate plate = Plate.of(" abc-1234 ");

        assertEquals("ABC1234", plate.getValue());
        assertEquals("ABC-1234", plate.getFormatted());
        assertTrue(plate.isOldPattern());
        assertTrue(plate.isValidMercosul() || plate.isOldPattern());
        assertTrue(plate.toString().contains("formatted='ABC-1234'"));
    }

    @Test
    void shouldAcceptMercosulPattern() {
        Plate plate = Plate.of("ABC1D23");
        assertTrue(plate.isValidMercosul());
        assertFalse(plate.isOldPattern());
    }

    @Test
    void shouldRejectInvalidPlate() {
        assertThrows(IllegalArgumentException.class, () -> Plate.of(""));
        assertThrows(IllegalArgumentException.class, () -> Plate.of("AB1"));
        assertThrows(IllegalArgumentException.class, () -> Plate.of("AAAAAAAA"));
        assertThrows(IllegalArgumentException.class, () -> Plate.of("ABC12D3"));
    }

    @Test
    void shouldImplementEqualityByNormalizedValue() {
        Plate p1 = Plate.of("abc-1234");
        Plate p2 = Plate.of("ABC1234");

        assertEquals(p1, p2);
        assertEquals(p1.hashCode(), p2.hashCode());
    }
}

package com.gestauto.vehicleevaluation.application.dto;

import com.gestauto.vehicleevaluation.domain.value.Money;
import org.junit.jupiter.api.Test;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class DepreciationDetailDtoTest {

    @Test
    void of_buildsDtoAndToStringIsStable() {
        var dto = DepreciationDetailDto.of(
                "dep-1",
                "COSMETIC",
                "Scratch",
                Money.of(250.00),
                "Needs repaint"
        );

        assertThat(dto.getDepreciationId()).isEqualTo("dep-1");
        assertThat(dto.getCategory()).isEqualTo("COSMETIC");
        assertThat(dto.getDescription()).isEqualTo("Scratch");
        assertThat(dto.getDepreciationValue()).isEqualTo(Money.of(250.00));
        assertThat(dto.getJustification()).isEqualTo("Needs repaint");
        assertThat(dto.toString()).contains("dep-1").contains("COSMETIC").contains("Scratch");
    }

    @Test
    void of_rejectsNullRequiredFields() {
        assertThatThrownBy(() -> DepreciationDetailDto.of(null, "C", "D", Money.of(1), null))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("depreciationId");

        assertThatThrownBy(() -> DepreciationDetailDto.of("id", null, "D", Money.of(1), null))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("category");

        assertThatThrownBy(() -> DepreciationDetailDto.of("id", "C", null, Money.of(1), null))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("description");

        assertThatThrownBy(() -> DepreciationDetailDto.of("id", "C", "D", null, null))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("depreciationValue");
    }
}

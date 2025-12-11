package com.gestauto.vehicleevaluation.application.command;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

@Schema(description = "Comando para atualizar checklist de uma avaliação")
public record UpdateChecklistCommand(
        @NotNull
        @Schema(description = "ID da avaliação")
        UUID evaluationId,

        @Schema(description = "Condição da lataria: EXCELLENT, GOOD, FAIR, POOR")
        String bodyCondition,

        @Schema(description = "Condição da pintura: EXCELLENT, GOOD, FAIR, POOR")
        String paintCondition,

        @Schema(description = "Presença de ferrugem")
        Boolean rustPresence,

        @Schema(description = "Arranhões leves")
        Boolean lightScratches,

        @Schema(description = "Arranhões profundos")
        Boolean deepScratches,

        @Schema(description = "Pequenas amassaduras")
        Boolean smallDents,

        @Schema(description = "Grandes amassaduras")
        Boolean largeDents,

        @Schema(description = "Quantidade de reparos em portas")
        Integer doorRepairs,

        @Schema(description = "Quantidade de reparos em alas")
        Integer fenderRepairs,

        @Schema(description = "Quantidade de reparos em capô")
        Integer hoodRepairs,

        @Schema(description = "Quantidade de reparos em porta-malas")
        Integer trunkRepairs,

        @Schema(description = "Trabalho de lataria pesada")
        Boolean heavyBodywork,

        @Schema(description = "Condição do motor: EXCELLENT, GOOD, FAIR, POOR")
        String engineCondition,

        @Schema(description = "Condição da transmissão: EXCELLENT, GOOD, FAIR, POOR")
        String transmissionCondition,

        @Schema(description = "Condição da suspensão: EXCELLENT, GOOD, FAIR, POOR")
        String suspensionCondition,

        @Schema(description = "Condição dos freios: EXCELLENT, GOOD, FAIR, POOR")
        String brakeCondition,

        @Schema(description = "Vazamento de óleo")
        Boolean oilLeaks,

        @Schema(description = "Vazamento de água")
        Boolean waterLeaks,

        @Schema(description = "Corrente de distribuição OK")
        Boolean timingBelt,

        @Schema(description = "Condição da bateria: EXCELLENT, GOOD, FAIR, POOR")
        String batteryCondition,

        @Schema(description = "Condição dos pneus: EXCELLENT, GOOD, FAIR, POOR")
        String tiresCondition,

        @Schema(description = "Desgaste irregular dos pneus")
        Boolean unevenWear,

        @Schema(description = "Pneus com profundidade baixa")
        Boolean lowTread,

        @Schema(description = "Condição dos bancos: EXCELLENT, GOOD, FAIR, POOR")
        String seatsCondition,

        @Schema(description = "Condição do painel: EXCELLENT, GOOD, FAIR, POOR")
        String dashboardCondition,

        @Schema(description = "Condição dos eletrônicos: EXCELLENT, GOOD, FAIR, POOR")
        String electronicsCondition,

        @Schema(description = "Dano nos bancos")
        Boolean seatDamage,

        @Schema(description = "Dano no painel das portas")
        Boolean doorPanelDamage,

        @Schema(description = "Desgaste no volante")
        Boolean steeringWheelWear,

        @Schema(description = "CRVL presente")
        Boolean crvlPresent,

        @Schema(description = "Manual presente")
        Boolean manualPresent,

        @Schema(description = "Chave sobressalente presente")
        Boolean spareKeyPresent,

        @Schema(description = "Registros de manutenção")
        Boolean maintenanceRecords,

        @Schema(description = "Observações mecânicas")
        String mechanicalNotes,

        @Schema(description = "Observações estéticas")
        String aestheticNotes,

        @Schema(description = "Observações sobre documentação")
        String documentationNotes
) {
}

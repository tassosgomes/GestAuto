package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.value.Money;
import org.junit.jupiter.api.Test;

import java.lang.reflect.InvocationTargetException;
import java.time.LocalDateTime;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatCode;

class CommandSmokeTest {

    @Test
    void updateChecklistCommand_accessorsToStringEqualsHashCode_areStable() {
        var evaluationId = UUID.randomUUID();

        var cmd = new UpdateChecklistCommand(
                evaluationId,
                "GOOD",
                "FAIR",
                true,
                false,
                true,
            true,
            false,
            1,
            0,
            0,
            0,
            false,
                "GOOD",
                "GOOD",
                "GOOD",
                "GOOD",
                false,
                false,
                true,
                "GOOD",
                "GOOD",
                false,
                false,
                "GOOD",
                "GOOD",
                "GOOD",
                false,
                false,
                false,
                true,
                true,
                true,
                true,
                "ok",
                "ok",
                "ok"
        );

        for (var component : UpdateChecklistCommand.class.getRecordComponents()) {
            var accessor = component.getAccessor();
            assertThatCode(() -> invoke(accessor, cmd)).doesNotThrowAnyException();
        }

        var cmd2 = new UpdateChecklistCommand(
                evaluationId,
                "GOOD",
                "FAIR",
                true,
                false,
                true,
            true,
            false,
            1,
            0,
            0,
            0,
            false,
            "GOOD",
            "GOOD",
            "GOOD",
            "GOOD",
            false,
            false,
            true,
            "GOOD",
            "GOOD",
            false,
            false,
            "GOOD",
            "GOOD",
            "GOOD",
            false,
            false,
            false,
            true,
            true,
            true,
            true,
            "ok",
            "ok",
            "ok"
        );

        assertThat(cmd).isEqualTo(cmd2);
        assertThat(cmd.hashCode()).isEqualTo(cmd2.hashCode());
        assertThat(cmd.toString()).contains(evaluationId.toString());
    }

    @Test
    void evaluationApprovedEvent_smoke() {
        var before = LocalDateTime.now().minusSeconds(1);
        var approvedAt = LocalDateTime.now();

        var event = new EvaluationApprovedEvent(
                "eval-1",
                "approver-1",
                Money.of(123.45),
                approvedAt
        );

        var after = LocalDateTime.now().plusSeconds(1);

        assertThat(event.getEvaluationId()).isEqualTo("eval-1");
        assertThat(event.getApproverId()).isEqualTo("approver-1");
        assertThat(event.getApprovedValue()).isEqualTo(Money.of(123.45));
        assertThat(event.getApprovedAt()).isEqualTo(approvedAt);
        assertThat(event.getOccurredAt()).isBetween(before, after);
        assertThat(event.toString()).contains("eval-1").contains("approver-1");
    }

    @Test
    void evaluationRejectedEvent_smoke() {
        var before = LocalDateTime.now().minusSeconds(1);
        var rejectedAt = LocalDateTime.now();

        var event = new EvaluationRejectedEvent(
                "eval-2",
                "approver-2",
                "bad photos",
                rejectedAt
        );

        var after = LocalDateTime.now().plusSeconds(1);

        assertThat(event.getEvaluationId()).isEqualTo("eval-2");
        assertThat(event.getApproverId()).isEqualTo("approver-2");
        assertThat(event.getReason()).isEqualTo("bad photos");
        assertThat(event.getRejectedAt()).isEqualTo(rejectedAt);
        assertThat(event.getOccurredAt()).isBetween(before, after);
        assertThat(event.toString()).contains("eval-2").contains("approver-2").contains("bad photos");
    }

    private static Object invoke(java.lang.reflect.Method method, Object target) {
        try {
            return method.invoke(target);
        } catch (IllegalAccessException e) {
            throw new RuntimeException(e);
        } catch (InvocationTargetException e) {
            if (e.getTargetException() instanceof RuntimeException runtimeException) {
                throw runtimeException;
            }
            throw new RuntimeException(e.getTargetException());
        }
    }
}

package com.gestauto.vehicleevaluation.api.support;

import org.junit.jupiter.api.extension.ConditionEvaluationResult;
import org.junit.jupiter.api.extension.ExecutionCondition;
import org.junit.jupiter.api.extension.ExtensionContext;
import org.testcontainers.DockerClientFactory;

/**
 * Skips integration tests that depend on Docker/Testcontainers when Docker is not available.
 *
 * This is intentionally implemented without calling Testcontainers' Docker detection logic,
 * because in some environments (e.g., misconfigured Docker Desktop sockets) that detection can
 * throw instead of cleanly reporting "not available".
 */
public class DockerAvailableCondition implements ExecutionCondition {

    private static final ConditionEvaluationResult ENABLED = ConditionEvaluationResult.enabled("Docker available");

    private static volatile Boolean cachedDockerAvailable;

    @Override
    public ConditionEvaluationResult evaluateExecutionCondition(ExtensionContext context) {
        if (isDockerAvailable()) {
            return ENABLED;
        }
        return ConditionEvaluationResult.disabled("Docker not available; skipping Testcontainers integration tests");
    }

    private static boolean isDockerAvailable() {
        Boolean cached = cachedDockerAvailable;
        if (cached != null) {
            return cached;
        }

        boolean available = false;
        try {
            DockerClientFactory.instance().client();
            available = true;
        } catch (Throwable ignored) {
            available = false;
        }

        cachedDockerAvailable = available;
        return available;
    }
}

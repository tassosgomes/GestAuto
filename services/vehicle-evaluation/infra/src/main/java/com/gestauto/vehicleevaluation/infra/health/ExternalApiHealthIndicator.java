package com.gestauto.vehicleevaluation.infra.health;

import com.gestauto.vehicleevaluation.infra.client.fipe.FipeApiClient;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.actuate.health.Health;
import org.springframework.boot.actuate.health.HealthIndicator;
import org.springframework.stereotype.Component;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.HeadBucketRequest;

/**
 * Health indicator para APIs externas (FIPE e Cloudflare R2)
 * Verifica se os serviços externos estão disponíveis
 */
@Component
@Slf4j
public class ExternalApiHealthIndicator implements HealthIndicator {

    private final FipeApiClient fipeApiClient;
    private final S3Client s3Client;
    
    @Value("${app.external-apis.cloudflare-r2.bucket-name}")
    private String bucketName;

    public ExternalApiHealthIndicator(FipeApiClient fipeApiClient, S3Client s3Client) {
        this.fipeApiClient = fipeApiClient;
        this.s3Client = s3Client;
    }

    @Override
    public Health health() {
        try {
            boolean fipeHealthy = testFipeConnection();
            boolean r2Healthy = testR2Connection();

            if (fipeHealthy && r2Healthy) {
                return Health.up()
                        .withDetail("fipe-api", "UP")
                        .withDetail("cloudflare-r2", "UP")
                        .build();
            } else {
                return Health.degraded()
                        .withDetail("fipe-api", fipeHealthy ? "UP" : "DOWN")
                        .withDetail("cloudflare-r2", r2Healthy ? "UP" : "DOWN")
                        .build();
            }
        } catch (Exception e) {
            log.error("Error checking external API health", e);
            return Health.down()
                    .withDetail("error", e.getMessage())
                    .build();
        }
    }

    /**
     * Testar conexão com API FIPE
     */
    private boolean testFipeConnection() {
        try {
            // Tenta obter marcas como teste de conectividade
            var brands = fipeApiClient.getBrands();
            boolean result = !brands.isEmpty();
            log.debug("FIPE API connection test: {}", result ? "OK" : "EMPTY");
            return result;
        } catch (Exception e) {
            log.warn("FIPE API connection test failed: {}", e.getMessage());
            return false;
        }
    }

    /**
     * Testar conexão com Cloudflare R2
     */
    private boolean testR2Connection() {
        try {
            // Tenta acessar o bucket como teste de conectividade
            if (bucketName == null || bucketName.isEmpty()) {
                log.warn("Cloudflare R2 bucket name not configured");
                return false;
            }

            s3Client.headBucket(HeadBucketRequest.builder()
                    .bucket(bucketName)
                    .build());

            log.debug("Cloudflare R2 connection test: OK");
            return true;
        } catch (Exception e) {
            log.warn("Cloudflare R2 connection test failed: {}", e.getMessage());
            return false;
        }
    }
}

package com.gestauto.vehicleevaluation.infra.config;

import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import software.amazon.awssdk.auth.credentials.AwsBasicCredentials;
import software.amazon.awssdk.auth.credentials.StaticCredentialsProvider;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.S3Configuration;
import software.amazon.awssdk.services.s3.presigner.S3Presigner;

import java.net.URI;
import java.time.Duration;

/**
 * Configuração do S3 Client para Cloudflare R2
 * Inclui suporte para operações síncronas e geração de URLs pré-assinadas
 */
@Configuration
@Slf4j
public class S3Config {

    @Value("${app.external-apis.cloudflare-r2.access-key}")
    private String accessKey;

    @Value("${app.external-apis.cloudflare-r2.secret-key}")
    private String secretKey;

    @Value("${app.external-apis.cloudflare-r2.endpoint}")
    private String endpoint;

    @Value("${app.external-apis.cloudflare-r2.timeout:10}")
    private int timeout;

    /**
     * S3Client para operações com Cloudflare R2
     */
    @Bean
    public S3Client s3Client() {
        log.info("Inicializando S3Client para Cloudflare R2: {}", endpoint);

        return S3Client.builder()
                .endpointOverride(URI.create(endpoint))
                .credentialsProvider(StaticCredentialsProvider.create(
                        AwsBasicCredentials.create(accessKey, secretKey)))
                .region(Region.US_EAST_1) // R2 usa região automática, mas SDK requer uma região
                .serviceConfiguration(S3Configuration.builder()
                        .pathStyleAccessEnabled(true)
                        .build())
                .httpClientBuilder(builder ->
                        builder.connectionTimeout(Duration.ofSeconds(timeout))
                )
                .build();
    }

    /**
     * S3Presigner para gerar URLs pré-assinadas (útil para downloads diretos)
     */
    @Bean
    public S3Presigner s3Presigner() {
        log.info("Inicializando S3Presigner para Cloudflare R2");

        return S3Presigner.builder()
                .endpointOverride(URI.create(endpoint))
                .region(Region.US_EAST_1)
                .credentialsProvider(StaticCredentialsProvider.create(
                        AwsBasicCredentials.create(accessKey, secretKey)))
                .build();
    }
}

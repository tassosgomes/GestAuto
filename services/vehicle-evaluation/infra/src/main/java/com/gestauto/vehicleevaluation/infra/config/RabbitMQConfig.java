package com.gestauto.vehicleevaluation.infra.config;

import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.config.RetryInterceptorBuilder;
import org.springframework.amqp.rabbit.config.SimpleRabbitListenerContainerFactory;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.rabbit.retry.RejectAndDontRequeueRecoverer;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.retry.backoff.ExponentialBackOffPolicy;
import org.springframework.retry.interceptor.RetryOperationsInterceptor;
import org.springframework.retry.policy.SimpleRetryPolicy;
import org.springframework.retry.support.RetryTemplate;

/**
 * Configuração do RabbitMQ para publicação e consumo de eventos de domínio.
 *
 * Esta configuração define:
 * - Exchange principal para eventos (gestauto.events)
 * - Queues para o microserviço de avaliação de veículos
 * - Dead Letter Queue (DLQ) para tratamento de falhas
 * - Bindings entre exchanges e queues
 * - Políticas de retry com exponential backoff
 */
@Configuration
public class RabbitMQConfig {

    @Value("${rabbitmq.exchange.name:gestauto.events}")
    private String exchangeName;

    @Value("${rabbitmq.queue.vehicle-evaluation:vehicle-evaluation.events}")
    private String vehicleEvaluationQueue;

    @Value("${rabbitmq.queue.vehicle-evaluation.dlq:vehicle-evaluation.events.dlq}")
    private String vehicleEvaluationDlq;

    @Value("${rabbitmq.routing-key.vehicle-evaluation:vehicle.evaluation.*}")
    private String vehicleEvaluationRoutingKey;

    /**
     * Exchange principal do tipo Topic para roteamento de eventos.
     * Permite roteamento baseado em padrões de routing keys.
     */
    @Bean
    public TopicExchange gestautoEventsExchange() {
        return ExchangeBuilder
                .topicExchange(exchangeName)
                .durable(true)
                .build();
    }

    /**
     * Queue principal para eventos de avaliação de veículos.
     * Configurada com DLX para enviar mensagens falhadas à DLQ.
     */
    @Bean
    public Queue vehicleEvaluationQueue() {
        return QueueBuilder
                .durable(vehicleEvaluationQueue)
                .withArgument("x-dead-letter-exchange", exchangeName + ".dlx")
                .withArgument("x-dead-letter-routing-key", "vehicle.evaluation.failed")
                .build();
    }

    /**
     * Dead Letter Queue para mensagens que falham após todas as tentativas.
     */
    @Bean
    public Queue vehicleEvaluationDlq() {
        return QueueBuilder
                .durable(vehicleEvaluationDlq)
                .build();
    }

    /**
     * Dead Letter Exchange para rotear mensagens falhadas.
     */
    @Bean
    public DirectExchange deadLetterExchange() {
        return ExchangeBuilder
                .directExchange(exchangeName + ".dlx")
                .durable(true)
                .build();
    }

    /**
     * Binding da queue principal ao exchange de eventos.
     * Recebe todos os eventos relacionados a avaliação de veículos.
     */
    @Bean
    public Binding vehicleEvaluationBinding() {
        return BindingBuilder
                .bind(vehicleEvaluationQueue())
                .to(gestautoEventsExchange())
                .with(vehicleEvaluationRoutingKey);
    }

    /**
     * Binding da DLQ ao Dead Letter Exchange.
     */
    @Bean
    public Binding deadLetterBinding() {
        return BindingBuilder
                .bind(vehicleEvaluationDlq())
                .to(deadLetterExchange())
                .with("vehicle.evaluation.failed");
    }

    /**
     * Conversor de mensagens para JSON usando Jackson.
     * Permite serialização e desserialização automática de objetos Java.
     */
    @Bean
    public MessageConverter jackson2JsonMessageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    /**
     * Template do RabbitMQ configurado com retry automático.
     */
    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory connectionFactory) {
        RabbitTemplate template = new RabbitTemplate(connectionFactory);
        template.setExchange(exchangeName);
        template.setMessageConverter(jackson2JsonMessageConverter());
        template.setMandatory(true);
        
        // Configurar callbacks para confirmação de publicação
        template.setConfirmCallback((correlationData, ack, cause) -> {
            if (!ack) {
                // Log de falha na confirmação
                System.err.println("Message not confirmed: " + cause);
            }
        });
        
        template.setReturnsCallback(returned -> {
            // Log de mensagem retornada (não roteada)
            System.err.println("Message returned: " + returned.getMessage());
        });
        
        return template;
    }

    /**
     * Template de retry com exponential backoff.
     * - Primeira tentativa: 1 segundo
     * - Multiplicador: 2x
     * - Máximo: 10 segundos
     * - Tentativas: 3
     */
    @Bean
    public RetryTemplate retryTemplate() {
        RetryTemplate retryTemplate = new RetryTemplate();
        
        // Política de retry
        SimpleRetryPolicy retryPolicy = new SimpleRetryPolicy();
        retryPolicy.setMaxAttempts(3);
        retryTemplate.setRetryPolicy(retryPolicy);
        
        // Política de backoff exponencial
        ExponentialBackOffPolicy backOffPolicy = new ExponentialBackOffPolicy();
        backOffPolicy.setInitialInterval(1000L); // 1 segundo
        backOffPolicy.setMultiplier(2.0);
        backOffPolicy.setMaxInterval(10000L); // 10 segundos
        retryTemplate.setBackOffPolicy(backOffPolicy);
        
        return retryTemplate;
    }

    /**
     * Interceptor de retry para listeners.
     */
    @Bean
    public RetryOperationsInterceptor retryInterceptor() {
        return RetryInterceptorBuilder.stateless()
                .maxAttempts(3)
                .backOffOptions(1000, 2.0, 10000)
                .recoverer(new RejectAndDontRequeueRecoverer())
                .build();
    }

    /**
     * Factory para containers de listeners com configuração customizada.
     */
    @Bean
    public SimpleRabbitListenerContainerFactory rabbitListenerContainerFactory(
            ConnectionFactory connectionFactory) {
        SimpleRabbitListenerContainerFactory factory = new SimpleRabbitListenerContainerFactory();
        factory.setConnectionFactory(connectionFactory);
        factory.setMessageConverter(jackson2JsonMessageConverter());
        factory.setConcurrentConsumers(3);
        factory.setMaxConcurrentConsumers(10);
        factory.setPrefetchCount(10);
        factory.setAdviceChain(retryInterceptor());
        return factory;
    }
}

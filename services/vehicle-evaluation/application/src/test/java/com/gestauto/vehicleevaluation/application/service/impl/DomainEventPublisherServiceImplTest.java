package com.gestauto.vehicleevaluation.application.service.impl;

import com.gestauto.vehicleevaluation.domain.event.DomainEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationCreatedEvent;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentCaptor;
import org.springframework.context.ApplicationEventPublisher;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.*;

class DomainEventPublisherServiceImplTest {

    static class DummyRabbitPublisher {
        int calls;
        DomainEvent last;

        public void publishEvent(DomainEvent event) {
            calls++;
            last = event;
        }
    }

    @Test
    void publish_validatesAvailabilityAndNulls() {
        ApplicationEventPublisher springPublisher = mock(ApplicationEventPublisher.class);
        DomainEventPublisherServiceImpl svc = new DomainEventPublisherServiceImpl(springPublisher);

        assertThatThrownBy(() -> svc.publish(null))
                .isInstanceOf(IllegalArgumentException.class);

        svc.simulateFailure();
        assertThatThrownBy(() -> svc.publish(new EvaluationCreatedEvent("e", "u", "ABC1234", "B", "M")))
                .isInstanceOf(IllegalStateException.class);
    }

    @Test
    void publish_and_publishBatch_recordsAndDelegates() {
        ApplicationEventPublisher springPublisher = mock(ApplicationEventPublisher.class);
        DomainEventPublisherServiceImpl svc = new DomainEventPublisherServiceImpl(springPublisher);

        DummyRabbitPublisher rabbit = new DummyRabbitPublisher();
        svc.setRabbitMQEventPublisher(rabbit);

        DomainEvent event1 = new EvaluationCreatedEvent("eval-1", "user-1", "ABC1234", "Toyota", "Corolla");
        svc.publish(event1);

        ArgumentCaptor<Object> captor = ArgumentCaptor.forClass(Object.class);
        verify(springPublisher).publishEvent(captor.capture());
        assertThat(captor.getValue()).isSameAs(event1);

        assertThat(rabbit.calls).isEqualTo(1);
        assertThat(rabbit.last).isSameAs(event1);
        assertThat(svc.getPublishedEventCount()).isEqualTo(1);
        assertThat(svc.getPublishedEvents()).hasSize(1);

        // batch
        DomainEvent event2 = new EvaluationCreatedEvent("eval-2", "user-2", "DEF5678", "Honda", "Civic");
        svc.publishBatch(List.of(event2));
        assertThat(svc.getPublishedEventCount()).isEqualTo(2);
        assertThat(svc.getPublishedEvents()).hasSize(2);

        // empty batch should be no-op
        svc.publishBatch(List.of());
        assertThat(svc.getPublishedEventCount()).isEqualTo(2);

        svc.clearEvents();
        assertThat(svc.getPublishedEventCount()).isEqualTo(0);
        assertThat(svc.getPublishedEvents()).isEmpty();

        svc.restoreService();
        assertThat(svc.isAvailable()).isTrue();
    }

    @Test
    void publish_whenRabbitPublisherMisconfigured_doesNotThrow() {
        ApplicationEventPublisher springPublisher = mock(ApplicationEventPublisher.class);
        DomainEventPublisherServiceImpl svc = new DomainEventPublisherServiceImpl(springPublisher);

        // Object without publishEvent(DomainEvent)
        svc.setRabbitMQEventPublisher(new Object());

        DomainEvent event = new EvaluationCreatedEvent("eval-1", "user-1", "ABC1234", "Toyota", "Corolla");
        svc.publish(event);

        verify(springPublisher).publishEvent(event);
        assertThat(svc.getPublishedEventCount()).isEqualTo(1);
    }
}

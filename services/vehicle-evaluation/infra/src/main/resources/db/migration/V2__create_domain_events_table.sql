-- Tabela de eventos de domínio para persistência e processamento assíncrono
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE vehicle_evaluation.domain_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type VARCHAR(50) NOT NULL,
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(50) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE NOT NULL,
    processed_at TIMESTAMP WITH TIME ZONE,
    processing_attempts INTEGER DEFAULT 0,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Índices para performance
CREATE INDEX idx_domain_events_type ON vehicle_evaluation.domain_events(event_type);
CREATE INDEX idx_domain_events_aggregate ON vehicle_evaluation.domain_events(aggregate_id, aggregate_type);
CREATE INDEX idx_domain_events_occurred_at ON vehicle_evaluation.domain_events(occurred_at);
CREATE INDEX idx_domain_events_processed ON vehicle_evaluation.domain_events(processed_at) WHERE processed_at IS NULL;
CREATE INDEX idx_domain_events_pending_processing ON vehicle_evaluation.domain_events(processing_attempts, occurred_at) WHERE processed_at IS NULL;

-- Trigger para atualizar updated_at
CREATE OR REPLACE FUNCTION vehicle_evaluation.update_domain_events_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER domain_events_updated_at_trigger
    BEFORE UPDATE ON vehicle_evaluation.domain_events
    FOR EACH ROW
    EXECUTE FUNCTION vehicle_evaluation.update_domain_events_updated_at();

-- Comentários da tabela
COMMENT ON TABLE vehicle_evaluation.domain_events IS 'Tabela para persistência de eventos de domínio emitidos pelas agregações';
COMMENT ON COLUMN vehicle_evaluation.domain_events.event_id IS 'Identificador único do evento';
COMMENT ON COLUMN vehicle_evaluation.domain_events.event_type IS 'Tipo do evento (ex: EvaluationCreated, EvaluationSubmitted)';
COMMENT ON COLUMN vehicle_evaluation.domain_events.aggregate_id IS 'ID da agregação que emitiu o evento';
COMMENT ON COLUMN vehicle_evaluation.domain_events.aggregate_type IS 'Tipo da agregação (ex: VehicleEvaluation)';
COMMENT ON COLUMN vehicle_evaluation.domain_events.event_data IS 'Dados do evento em formato JSON';
COMMENT ON COLUMN vehicle_evaluation.domain_events.occurred_at IS 'Data/hora em que o evento ocorreu';
COMMENT ON COLUMN vehicle_evaluation.domain_events.processed_at IS 'Data/hora em que o evento foi processado';
COMMENT ON COLUMN vehicle_evaluation.domain_events.processing_attempts IS 'Número de tentativas de processamento';
COMMENT ON COLUMN vehicle_evaluation.domain_events.error_message IS 'Mensagem de erro em caso de falha no processamento';
COMMENT ON COLUMN vehicle_evaluation.domain_events.created_at IS 'Data/hora de criação do registro';
COMMENT ON COLUMN vehicle_evaluation.domain_events.updated_at IS 'Data/hora da última atualização do registro';
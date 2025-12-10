-- GestAuto - Vehicle Evaluation Service
-- Migration V001: Create vehicle evaluation schema aligned to domain + JPA

CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE SCHEMA IF NOT EXISTS vehicle_evaluation;
SET search_path TO vehicle_evaluation;

-- Tabela principal de avaliações
CREATE TABLE vehicle_evaluations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plate VARCHAR(7) NOT NULL UNIQUE,
    renavam VARCHAR(11) NOT NULL,
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    version VARCHAR(100) NOT NULL,
    year_manufacture INTEGER NOT NULL,
    year_model INTEGER NOT NULL,
    color VARCHAR(50) NOT NULL,
    fuel_type VARCHAR(20) NOT NULL,
    mileage_amount NUMERIC(19,2) NOT NULL,
    mileage_currency CHAR(3) NOT NULL DEFAULT 'BRL',
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT',
    fipe_price_amount NUMERIC(19,2),
    fipe_price_currency CHAR(3),
    base_value_amount NUMERIC(19,2),
    base_value_currency CHAR(3),
    final_value_amount NUMERIC(19,2),
    final_value_currency CHAR(3),
    approved_value_amount NUMERIC(19,2),
    approved_value_currency CHAR(3),
    observations TEXT,
    justification TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    submitted_at TIMESTAMPTZ,
    approved_at TIMESTAMPTZ,
    evaluator_id VARCHAR(100) NOT NULL,
    approver_id VARCHAR(100),
    valid_until TIMESTAMPTZ,
    validation_token VARCHAR(150),
    CONSTRAINT chk_status CHECK (status IN ('DRAFT','IN_PROGRESS','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED','EXPIRED')),
    CONSTRAINT chk_mileage CHECK (mileage_amount >= 0),
    CONSTRAINT chk_year_manufacture CHECK (year_manufacture >= 1900 AND year_manufacture <= EXTRACT(YEAR FROM CURRENT_DATE) + 1),
    CONSTRAINT chk_year_model CHECK (year_model >= 1900 AND year_model <= EXTRACT(YEAR FROM CURRENT_DATE) + 2)
);

-- Fotos da avaliação (15 obrigatórias por PRD)
CREATE TABLE evaluation_photos (
    photo_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL REFERENCES vehicle_evaluations(id) ON DELETE CASCADE,
    photo_type VARCHAR(30) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT NOT NULL CHECK (file_size > 0),
    content_type VARCHAR(100) NOT NULL,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    thumbnail_url VARCHAR(500),
    upload_url VARCHAR(500),
    CONSTRAINT chk_photo_type CHECK (photo_type IN (
        'FRONT','REAR','LEFT_SIDE','RIGHT_SIDE',
        'INTERIOR_FRONT','INTERIOR_REAR','DRIVER_SIDE','PASSENGER_SIDE',
        'DASHBOARD_OVERVIEW','DASHBOARD_CLUSTER','ODOMETER',
        'ENGINE_OVERVIEW','ENGINE_DETAIL','TRUNK_OPEN','TRUNK_CLOSED'
    ))
);

-- Checklist técnico
CREATE TABLE evaluation_checklists (
    checklist_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL UNIQUE REFERENCES vehicle_evaluations(id) ON DELETE CASCADE,
    body_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    paint_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    rust_presence BOOLEAN DEFAULT FALSE,
    light_scratches BOOLEAN DEFAULT FALSE,
    deep_scratches BOOLEAN DEFAULT FALSE,
    small_dents BOOLEAN DEFAULT FALSE,
    large_dents BOOLEAN DEFAULT FALSE,
    door_repairs INTEGER DEFAULT 0,
    fender_repairs INTEGER DEFAULT 0,
    hood_repairs INTEGER DEFAULT 0,
    trunk_repairs INTEGER DEFAULT 0,
    heavy_bodywork BOOLEAN DEFAULT FALSE,
    engine_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    transmission_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    suspension_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    brake_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    oil_leaks BOOLEAN DEFAULT FALSE,
    water_leaks BOOLEAN DEFAULT FALSE,
    timing_belt BOOLEAN DEFAULT FALSE,
    battery_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    tires_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    uneven_wear BOOLEAN DEFAULT FALSE,
    low_tread BOOLEAN DEFAULT FALSE,
    seats_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    dashboard_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    electronics_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    seat_damage BOOLEAN DEFAULT FALSE,
    door_panel_damage BOOLEAN DEFAULT FALSE,
    steering_wheel_wear BOOLEAN DEFAULT FALSE,
    crvl_present BOOLEAN DEFAULT FALSE,
    manual_present BOOLEAN DEFAULT FALSE,
    spare_key_present BOOLEAN DEFAULT FALSE,
    maintenance_records BOOLEAN DEFAULT FALSE,
    mechanical_notes TEXT,
    aesthetic_notes TEXT,
    documentation_notes TEXT,
    critical_issues TEXT[] DEFAULT '{}',
    conservation_score INTEGER,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_condition_generic CHECK (
        body_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        paint_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        engine_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        transmission_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        suspension_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        brake_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        tires_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        battery_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        seats_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        dashboard_condition IN ('EXCELLENT','GOOD','FAIR','POOR') AND
        electronics_condition IN ('EXCELLENT','GOOD','FAIR','POOR')
    ),
    CONSTRAINT chk_repairs CHECK (
        door_repairs BETWEEN 0 AND 10 AND
        fender_repairs BETWEEN 0 AND 10 AND
        hood_repairs BETWEEN 0 AND 10 AND
        trunk_repairs BETWEEN 0 AND 10
    )
);

-- Itens de depreciação
CREATE TABLE depreciation_items (
    depreciation_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL REFERENCES vehicle_evaluations(id) ON DELETE CASCADE,
    category VARCHAR(30) NOT NULL,
    description TEXT NOT NULL,
    depreciation_value_amount NUMERIC(19,2) NOT NULL,
    depreciation_value_currency CHAR(3) NOT NULL DEFAULT 'BRL',
    justification TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50) NOT NULL,
    CONSTRAINT chk_depreciation_category CHECK (category IN ('BODY','PAINT','MECHANICAL','TIRES','INTERIOR','DOCUMENTATION','OTHER')),
    CONSTRAINT chk_depreciation_value CHECK (depreciation_value_amount > 0)
);

-- Acessórios avaliados
CREATE TABLE evaluation_accessories (
    accessory_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL REFERENCES vehicle_evaluations(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    included BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Questões críticas registradas do checklist
CREATE TABLE checklist_critical_issues (
    issue_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    evaluation_id UUID NOT NULL REFERENCES vehicle_evaluations(id) ON DELETE CASCADE,
    issue TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índices de performance
CREATE INDEX idx_vehicle_evaluations_status ON vehicle_evaluations(status);
CREATE INDEX idx_vehicle_evaluations_plate ON vehicle_evaluations(plate);
CREATE INDEX idx_vehicle_evaluations_created_at ON vehicle_evaluations(created_at);
CREATE INDEX idx_vehicle_evaluations_submitted_at ON vehicle_evaluations(submitted_at);
CREATE INDEX idx_vehicle_evaluations_valid_until ON vehicle_evaluations(valid_until);

CREATE INDEX idx_evaluation_photos_eval ON evaluation_photos(evaluation_id);
CREATE INDEX idx_evaluation_photos_type ON evaluation_photos(photo_type);

CREATE INDEX idx_evaluation_checklists_eval ON evaluation_checklists(evaluation_id);

CREATE INDEX idx_depreciation_items_eval ON depreciation_items(evaluation_id);
CREATE INDEX idx_depreciation_items_category ON depreciation_items(category);

CREATE INDEX idx_accessories_eval ON evaluation_accessories(evaluation_id);
CREATE INDEX idx_critical_issues_eval ON checklist_critical_issues(evaluation_id);

-- Função e triggers de atualização de updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE 'plpgsql';

CREATE TRIGGER vehicle_evaluations_updated_at
    BEFORE UPDATE ON vehicle_evaluations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER evaluation_checklists_updated_at
    BEFORE UPDATE ON evaluation_checklists
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

COMMIT;
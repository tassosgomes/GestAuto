-- GestAuto - Vehicle Evaluation Service
-- Migration V001: Create vehicle evaluation schema

-- Create schema if not exists
CREATE SCHEMA IF NOT EXISTS vehicle_evaluation;

-- Set search path to vehicle_evaluation
SET search_path TO vehicle_evaluation;

-- Vehicle Evaluations Table
CREATE TABLE vehicle_evaluations (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id VARCHAR(36) UNIQUE NOT NULL,
    customer_id VARCHAR(100) NOT NULL,
    evaluator_id VARCHAR(100) NOT NULL,

    -- Vehicle Information
    plate VARCHAR(7) NOT NULL,
    renavam VARCHAR(11) NOT NULL,
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    version VARCHAR(100),
    year_manufacture INTEGER NOT NULL,
    year_model INTEGER NOT NULL,
    color VARCHAR(50),
    mileage BIGINT NOT NULL,
    fuel_type VARCHAR(20) NOT NULL,

    -- Evaluation Details
    status VARCHAR(20) NOT NULL DEFAULT 'DRAFT',
    submission_date TIMESTAMP,
    approval_date TIMESTAMP,
    approver_id VARCHAR(100),

    -- Financial Information
    fipe_value DECIMAL(12,2),
    liquidity_percentage DECIMAL(5,2),
    base_value DECIMAL(12,2),
    depreciation_total DECIMAL(12,2),
    final_value DECIMAL(12,2),

    -- Metadata
    observations TEXT,
    justification TEXT,
    valid_until TIMESTAMP,

    -- Audit
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_by VARCHAR(100),

    -- Constraints
    CONSTRAINT chk_evaluation_status
        CHECK (status IN ('DRAFT', 'PENDING_APPROVAL', 'APPROVED', 'REJECTED', 'EXPIRED')),
    CONSTRAINT chk_mileage
        CHECK (mileage >= 0),
    CONSTRAINT chk_year_manufacture
        CHECK (year_manufacture >= 1900 AND year_manufacture <= EXTRACT(YEAR FROM CURRENT_DATE) + 1),
    CONSTRAINT chk_year_model
        CHECK (year_model >= 1900 AND year_model <= EXTRACT(YEAR FROM CURRENT_DATE) + 2),
    CONSTRAINT chk_liquidity_percentage
        CHECK (liquidity_percentage >= 0 AND liquidity_percentage <= 100)
);

-- Evaluation Photos Table
CREATE TABLE evaluation_photos (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id VARCHAR(36) NOT NULL REFERENCES vehicle_evaluations(evaluation_id) ON DELETE CASCADE,
    photo_id VARCHAR(36) UNIQUE NOT NULL,
    photo_type VARCHAR(30) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    upload_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_photo_type
        CHECK (photo_type IN ('FRONT', 'REAR', 'LEFT_SIDE', 'RIGHT_SIDE', 'INTERIOR_FRONT',
                            'INTERIOR_REAR', 'DASHBOARD', 'ENGINE', 'TIRES', 'TRUNK',
                            'DAMAGE_DETAIL', 'DOCUMENT_CRLV', 'DOCUMENT_EXTRA', 'OTHER'))
);

-- Evaluation Checklist Table
CREATE TABLE evaluation_checklists (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id VARCHAR(36) NOT NULL REFERENCES vehicle_evaluations(evaluation_id) ON DELETE CASCADE,
    checklist_id VARCHAR(36) UNIQUE NOT NULL,

    -- Body and Paint
    body_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    paint_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    rust_presence BOOLEAN DEFAULT FALSE,

    -- Body Damage Details
    light_scratches BOOLEAN DEFAULT FALSE,
    deep_scratches BOOLEAN DEFAULT FALSE,
    small_dents BOOLEAN DEFAULT FALSE,
    large_dents BOOLEAN DEFAULT FALSE,
    door_repairs INTEGER DEFAULT 0,
    fender_repairs INTEGER DEFAULT 0,
    hood_repairs INTEGER DEFAULT 0,
    trunk_repairs INTEGER DEFAULT 0,
    heavy_bodywork BOOLEAN DEFAULT FALSE,

    -- Mechanical Condition
    engine_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    transmission_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    suspension_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    brake_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',

    -- Mechanical Issues
    tire_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    oil_leaks BOOLEAN DEFAULT FALSE,
    water_leaks BOOLEAN DEFAULT FALSE,
    timing_belt BOOLEAN DEFAULT FALSE,
    battery_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',

    -- Interior Condition
    seats_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    dashboard_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',
    electronics_condition VARCHAR(20) NOT NULL DEFAULT 'GOOD',

    -- Interior Issues
    seat_damage BOOLEAN DEFAULT FALSE,
    door_panel_damage BOOLEAN DEFAULT FALSE,
    steering_wheel_wear BOOLEAN DEFAULT FALSE,

    -- Documentation
    crvl_present BOOLEAN DEFAULT FALSE,
    manual_present BOOLEAN DEFAULT FALSE,
    spare_key_present BOOLEAN DEFAULT FALSE,
    maintenance_records BOOLEAN DEFAULT FALSE,

    -- Additional Notes
    mechanical_notes TEXT,
    aesthetic_notes TEXT,
    documentation_notes TEXT,

    -- Audit
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_body_condition
        CHECK (body_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_paint_condition
        CHECK (paint_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_engine_condition
        CHECK (engine_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_transmission_condition
        CHECK (transmission_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_suspension_condition
        CHECK (suspension_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_brake_condition
        CHECK (brake_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_tire_condition
        CHECK (tire_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_battery_condition
        CHECK (battery_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_seats_condition
        CHECK (seats_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_dashboard_condition
        CHECK (dashboard_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_electronics_condition
        CHECK (electronics_condition IN ('EXCELLENT', 'GOOD', 'FAIR', 'POOR')),
    CONSTRAINT chk_door_repairs
        CHECK (door_repairs >= 0 AND door_repairs <= 4),
    CONSTRAINT chk_fender_repairs
        CHECK (fender_repairs >= 0 AND fender_repairs <= 4),
    CONSTRAINT chk_hood_repairs
        CHECK (hood_repairs >= 0 AND hood_repairs <= 1),
    CONSTRAINT chk_trunk_repairs
        CHECK (trunk_repairs >= 0 AND trunk_repairs <= 1)
);

-- Depreciation Items Table
CREATE TABLE depreciation_items (
    id BIGSERIAL PRIMARY KEY,
    evaluation_id VARCHAR(36) NOT NULL REFERENCES vehicle_evaluations(evaluation_id) ON DELETE CASCADE,
    depreciation_id VARCHAR(36) UNIQUE NOT NULL,
    category VARCHAR(30) NOT NULL,
    description VARCHAR(200) NOT NULL,
    depreciation_value DECIMAL(10,2) NOT NULL,

    -- Audit
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,

    -- Constraints
    CONSTRAINT chk_depreciation_category
        CHECK (category IN ('BODY', 'PAINT', 'MECHANICAL', 'TIRES', 'INTERIOR', 'DOCUMENTATION', 'OTHER')),
    CONSTRAINT chk_depreciation_value
        CHECK (depreciation_value > 0)
);

-- Indexes for performance
CREATE INDEX idx_vehicle_evaluations_customer_id ON vehicle_evaluations(customer_id);
CREATE INDEX idx_vehicle_evaluations_evaluator_id ON vehicle_evaluations(evaluator_id);
CREATE INDEX idx_vehicle_evaluations_status ON vehicle_evaluations(status);
CREATE INDEX idx_vehicle_evaluations_plate ON vehicle_evaluations(plate);
CREATE INDEX idx_vehicle_evaluations_created_at ON vehicle_evaluations(created_at);
CREATE INDEX idx_vehicle_evaluations_submission_date ON vehicle_evaluations(submission_date);
CREATE INDEX idx_vehicle_evaluations_valid_until ON vehicle_evaluations(valid_until);

CREATE INDEX idx_evaluation_photos_evaluation_id ON evaluation_photos(evaluation_id);
CREATE INDEX idx_evaluation_photos_photo_type ON evaluation_photos(photo_type);

CREATE INDEX idx_evaluation_checklists_evaluation_id ON evaluation_checklists(evaluation_id);

CREATE INDEX idx_depreciation_items_evaluation_id ON depreciation_items(evaluation_id);
CREATE INDEX idx_depreciation_items_category ON depreciation_items(category);

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Create triggers for updated_at
CREATE TRIGGER update_vehicle_evaluations_updated_at
    BEFORE UPDATE ON vehicle_evaluations
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_evaluation_checklists_updated_at
    BEFORE UPDATE ON evaluation_checklists
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Insert sample data (optional, for development)
-- INSERT INTO vehicle_evaluations (evaluation_id, customer_id, evaluator_id, plate, renavam, brand, model, year_manufacture, year_model, color, mileage, fuel_type, created_by)
-- VALUES
-- ('550e8400-e29b-41d4-a716-446655440000', 'CUSTOMER_001', 'EVALUATOR_001', 'ABC1234', '12345678901', 'VOLKSWAGEN', 'GOL', 2020, 2021, 'WHITE', 35000, 'FLEX', 'SYSTEM');

COMMIT;
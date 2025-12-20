-- Migration V4: Add indexes to support dashboard/report queries

SET search_path TO vehicle_evaluation;

CREATE INDEX IF NOT EXISTS idx_vehicle_evaluations_evaluator_created_at
    ON vehicle_evaluations(evaluator_id, created_at);

CREATE INDEX IF NOT EXISTS idx_vehicle_evaluations_status_created_at
    ON vehicle_evaluations(status, created_at);

CREATE INDEX IF NOT EXISTS idx_vehicle_evaluations_brand_created_at
    ON vehicle_evaluations(brand, created_at);

CREATE INDEX IF NOT EXISTS idx_vehicle_evaluations_validation_token
    ON vehicle_evaluations(validation_token);

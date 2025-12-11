-- Migration V3: Add missing vehicle info columns

ALTER TABLE vehicle_evaluations ADD COLUMN chassis VARCHAR(17);
ALTER TABLE vehicle_evaluations ADD COLUMN engine VARCHAR(50);
ALTER TABLE vehicle_evaluations ADD COLUMN transmission VARCHAR(50);
ALTER TABLE vehicle_evaluations ADD COLUMN doors VARCHAR(20);

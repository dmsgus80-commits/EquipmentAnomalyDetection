-- Run once only for a database created with the old schema.sql.
-- Check the anomaly_event columns first; do not rerun if these columns already exist.
ALTER TABLE anomaly_event
    ADD COLUMN status VARCHAR(20) NOT NULL DEFAULT 'UNACKNOWLEDGED',
    ADD COLUMN acknowledged_at DATETIME NULL,
    ADD COLUMN resolved_at DATETIME NULL;

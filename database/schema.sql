CREATE DATABASE IF NOT EXISTS equipment_anomaly;

USE equipment_anomaly;

CREATE TABLE IF NOT EXISTS sensor_reading (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    air_temperature FLOAT NOT NULL,
    process_temperature FLOAT NOT NULL,
    rotational_speed INT NOT NULL,
    torque FLOAT NOT NULL,
    tool_wear INT NOT NULL,
    machine_failure BOOLEAN NOT NULL,
    failure_probability FLOAT NOT NULL,
    failure_type VARCHAR(50),
    failure_type_probability FLOAT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS anomaly_event (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    sensor_reading_id BIGINT NOT NULL,
    failure_type VARCHAR(50),
    failure_probability FLOAT NOT NULL,
    failure_type_probability FLOAT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (sensor_reading_id)
        REFERENCES sensor_reading(id)
);
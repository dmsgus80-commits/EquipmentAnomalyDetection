from fastapi import FastAPI
import joblib
import pandas as pd
import pymysql
import os
from dotenv import load_dotenv
from models.schemas import SensorData

app = FastAPI()

load_dotenv()

failure_model = joblib.load(
    "../ml/saved_models/random_forest.pkl"
)

failure_type_model = joblib.load(
    "../ml/saved_models/failure_type_random_forest.pkl"
)


def get_db_connection():
    return pymysql.connect(
        host=os.getenv("DB_HOST"),
        user=os.getenv("DB_USER"),
        password=os.getenv("DB_PASSWORD"),
        database=os.getenv("DB_NAME"),
        charset="utf8mb4",
        cursorclass=pymysql.cursors.DictCursor
    )


@app.get("/")
def root():
    return {"message": "Equipment Anomaly Detection API"}


@app.post("/predict")
def predict(data: SensorData):

    input_data = pd.DataFrame([{
        "Air temperature [K]": data.air_temperature,
        "Process temperature [K]": data.process_temperature,
        "Rotational speed [rpm]": data.rotational_speed,
        "Torque [Nm]": data.torque,
        "Tool wear [min]": data.tool_wear
    }])

    failure_prediction = failure_model.predict(input_data)[0]

    failure_probabilities = failure_model.predict_proba(input_data)[0]

    failure_index = list(failure_model.classes_).index(1)

    failure_probability = failure_probabilities[failure_index]

    failure_type = None
    failure_type_probability = None

    if failure_prediction == 1:
        failure_type = failure_type_model.predict(input_data)[0]

        type_probabilities = failure_type_model.predict_proba(input_data)[0]

        failure_type_probability = max(type_probabilities)

    conn = get_db_connection()

    try:
        with conn.cursor() as cursor:

            sql = """
                INSERT INTO sensor_reading (
                    air_temperature,
                    process_temperature,
                    rotational_speed,
                    torque,
                    tool_wear,
                    machine_failure,
                    failure_probability,
                    failure_type,
                    failure_type_probability
                )
                VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s)
            """

            cursor.execute(
                sql,
                (
                    data.air_temperature,
                    data.process_temperature,
                    data.rotational_speed,
                    data.torque,
                    data.tool_wear,
                    bool(failure_prediction),
                    float(failure_probability),
                    failure_type,
                    (
                        float(failure_type_probability)
                        if failure_type_probability is not None
                        else None
                    )
                )
            )

            sensor_reading_id = cursor.lastrowid

            if failure_prediction == 1:
                anomaly_sql = """
                    INSERT INTO anomaly_event (
                        sensor_reading_id,
                        failure_type,
                        failure_probability,
                        failure_type_probability
                    )
                    VALUES (%s, %s, %s, %s)
                """

                cursor.execute(
                    anomaly_sql,
                    (
                        sensor_reading_id,
                        failure_type,
                        float(failure_probability),
                        float(failure_type_probability)
                    )
                )

        conn.commit()

    finally:
        conn.close()

    return {
        "machine_failure": bool(failure_prediction),
        "failure_probability": float(failure_probability),
        "failure_type": failure_type,
        "failure_type_probability": (
            float(failure_type_probability)
            if failure_type_probability is not None
            else None
        )
    }


@app.get("/history")
def get_history():

    conn = get_db_connection()

    try:
        with conn.cursor() as cursor:

            sql = """
                SELECT
                    id,
                    sensor_reading_id,
                    failure_type,
                    failure_probability,
                    failure_type_probability,
                    created_at
                FROM anomaly_event
                ORDER BY created_at DESC
                LIMIT 100
            """

            cursor.execute(sql)

            rows = cursor.fetchall()


    finally:
        conn.close()

    return rows


@app.get("/sensor-readings")
def get_sensor_readings():
    conn = get_db_connection()

    try:
        with conn.cursor() as cursor:

            sql = """
                SELECT
                    id,
                    air_temperature,
                    process_temperature,
                    rotational_speed,
                    torque,
                    tool_wear,
                    machine_failure,
                    failure_probability,
                    failure_type,
                    failure_type_probability,
                    created_at
                FROM sensor_reading
                ORDER BY created_at DESC
                LIMIT 100
            """

            cursor.execute(sql)
            rows = cursor.fetchall()

    finally:
        conn.close()

    return rows
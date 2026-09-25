from fastapi import FastAPI, HTTPException
from models.schemas import SensorData
from core.database import get_db_connection
from services.prediction_service import predict_sensor_data
from services.database_service import (
    save_prediction,
    get_history,
    get_history_summary,
    get_sensor_readings,
    acknowledge_alarm,
    resolve_alarm
)

app = FastAPI()


@app.get("/")
def root():
    return {"message": "Equipment Anomaly Detection API"}


@app.post("/predict")
def predict(data: SensorData):
    result = predict_sensor_data(data)

    save_prediction(data, result)

    return result


@app.get("/history")
def history():
    return get_history()


@app.get("/history/summary")
def history_summary():
    return get_history_summary()


@app.get("/sensor-readings")
def sensor_readings():
    return get_sensor_readings()


@app.patch("/alarms/{alarm_id}/acknowledge")
def acknowledge(alarm_id: int):
    outcome = acknowledge_alarm(alarm_id)
    if outcome == "not_found":
        raise HTTPException(status_code=404, detail="Alarm not found")
    if outcome == "invalid_state":
        raise HTTPException(status_code=409, detail="Alarm is not unacknowledged")

    return {
        "message": "Alarm acknowledged",
        "alarm_id": alarm_id
    }


@app.patch("/alarms/{alarm_id}/resolve")
def resolve(alarm_id: int):
    outcome = resolve_alarm(alarm_id)
    if outcome == "not_found":
        raise HTTPException(status_code=404, detail="Alarm not found")
    if outcome == "invalid_state":
        raise HTTPException(status_code=409, detail="Alarm is not acknowledged")

    return {
        "message": "Alarm resolved",
        "alarm_id": alarm_id
    }

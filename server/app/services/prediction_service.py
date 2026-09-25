import pandas as pd

from core.model_loader import failure_model, failure_type_model
from models.schemas import SensorData


def predict_sensor_data(sensor_data: SensorData):
    input_data = pd.DataFrame([
        {
            "Air temperature [K]": sensor_data.air_temperature,
            "Process temperature [K]": sensor_data.process_temperature,
            "Rotational speed [rpm]": sensor_data.rotational_speed,
            "Torque [Nm]": sensor_data.torque,
            "Tool wear [min]": sensor_data.tool_wear
        }
    ])

    failure_prediction = failure_model.predict(input_data)[0]

    failure_probabilities = failure_model.predict_proba(input_data)

    failure_class_index = list(failure_model.classes_).index(1)

    failure_probability = failure_probabilities[0][failure_class_index]

    failure_type = None
    failure_type_probability = None

    if failure_prediction == 1:
        failure_type_prediction = failure_type_model.predict(input_data)[0]

        failure_type_probabilities = failure_type_model.predict_proba(input_data)[0]

        failure_type = failure_type_prediction
        failure_type_probability = max(failure_type_probabilities)

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

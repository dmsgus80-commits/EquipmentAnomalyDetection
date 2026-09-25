import random
import time
import requests

API_URL = "http://127.0.0.1:8000/predict"


def generate_sensor_data():
    mode = random.choice([
        "normal",
        "overheat",
        "high_torque",
        "high_tool_wear"
    ])

    if mode == "normal":
        sensor_data = {
            "air_temperature": round(random.uniform(297.0, 300.0), 1),
            "process_temperature": round(random.uniform(307.0, 310.0), 1),
            "rotational_speed": random.randint(1400, 1600),
            "torque": round(random.uniform(35.0, 50.0), 1),
            "tool_wear": random.randint(0, 150)
        }

    elif mode == "overheat":
        sensor_data = {
            "air_temperature": round(random.uniform(301.0, 304.0), 1),
            "process_temperature": round(random.uniform(312.0, 316.0), 1),
            "rotational_speed": random.randint(1350, 1550),
            "torque": round(random.uniform(40.0, 55.0), 1),
            "tool_wear": random.randint(100, 220)
        }

    elif mode == "high_torque":
        sensor_data = {
            "air_temperature": round(random.uniform(298.0, 302.0), 1),
            "process_temperature": round(random.uniform(309.0, 313.0), 1),
            "rotational_speed": random.randint(1100, 1400),
            "torque": round(random.uniform(60.0, 75.0), 1),
            "tool_wear": random.randint(120, 230)
        }

    else:
        sensor_data = {
            "air_temperature": round(random.uniform(298.0, 302.0), 1),
            "process_temperature": round(random.uniform(309.0, 313.0), 1),
            "rotational_speed": random.randint(1300, 1550),
            "torque": round(random.uniform(40.0, 55.0), 1),
            "tool_wear": random.randint(200, 250)
        }

    return mode, sensor_data


while True:
    mode, sensor_data = generate_sensor_data()

    try:
        response = requests.post(
            API_URL,
            json=sensor_data,
            timeout=3
        )

        print("시나리오:", mode)
        print("센서값:", sensor_data)
        print("AI 결과:", response.json())

    except requests.exceptions.RequestException as ex:
        print("서버 연결 실패:", ex)

    print("-" * 50)
    time.sleep(1)

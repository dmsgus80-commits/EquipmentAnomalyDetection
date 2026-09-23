import pandas as pd

from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import classification_report

import joblib
import os


df = pd.read_csv("../../data/ai4i2020.csv")

features = [
    "Air temperature [K]",
    "Process temperature [K]",
    "Rotational speed [rpm]",
    "Torque [Nm]",
    "Tool wear [min]"
]

failure_columns = ["TWF", "HDF", "PWF", "OSF", "RNF"]

failure_df = df[df["Machine failure"] == 1].copy()


def get_failure_type(row):
    for failure in failure_columns:
        if row[failure] == 1:
            return failure

    return "Unknown"


failure_df["Failure type"] = failure_df.apply(
    get_failure_type,
    axis=1
)

X = failure_df[features]
y = failure_df["Failure type"]

X_train, X_test, y_train, y_test = train_test_split(
    X,
    y,
    test_size=0.2,
    random_state=42,
    stratify=y
)

model = RandomForestClassifier(
    n_estimators=200,
    random_state=42,
    class_weight="balanced"
)

model.fit(X_train, y_train)

pred = model.predict(X_test)

print("=== Failure Type 분포 ===")
print(y.value_counts())

print("\n=== Classification Report ===")
print(classification_report(y_test, pred))

os.makedirs("saved_models", exist_ok=True)

joblib.dump(
    model,
    "saved_models/failure_type_random_forest.pkl"
)

print("\n고장 유형 분류 모델 저장 완료")

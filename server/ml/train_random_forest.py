import pandas as pd

from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import confusion_matrix, classification_report

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

X = df[features]
y = df["Machine failure"]

X_train, X_test, y_train, y_test = train_test_split(
    X,
    y,
    test_size=0.2,
    random_state=42,
    stratify=y
)

model = RandomForestClassifier(
    n_estimators=100,
    random_state=42,
    class_weight="balanced"
)

model.fit(X_train, y_train)

pred = model.predict(X_test)

print("=== Confusion Matrix ===")
print(confusion_matrix(y_test, pred))

print("\n=== Classification Report ===")
print(classification_report(y_test, pred))

os.makedirs("saved_models", exist_ok=True)

joblib.dump(
    model,
    "saved_models/random_forest.pkl"
)

print("\nRandom Forest 모델 저장 완료")

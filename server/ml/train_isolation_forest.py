import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import classification_report, confusion_matrix
import joblib
import os

#데이터 불러오기
df = pd.read_csv("../../data/ai4i2020.csv")

# 학습에 사용할 센서 컬럼
features = [
    "Air temperature [K]",
    "Process temperature [K]",
    "Rotational speed [rpm]",
    "Torque [Nm]",
    "Tool wear [min]"
]

X = df[features]

#정답값
y = df["Machine failure"]

#스케일링
scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

#Isolation Forest 학습
model = IsolationForest(
    contamination=0.034,
    random_state=42
)

model.fit(X_scaled)

#예측
pred = model.predict(X_scaled)

#Isolation Forest 결과:
# 1 = 정상
# -1 = 이상
pred_binary = (pred == -1).astype(int)

print("=== Confusion Matirix ===")
print(confusion_matrix(y, pred_binary))

# 모델 저장 폴더
os.makedirs("saved_models", exist_ok=True)

joblib.dump(model, "saved_models/isolation_forest.pkl")
joblib.dump(scaler, "saved_models/scaler.pkl")

print("\n모델 저장 완료")



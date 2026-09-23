# Equipment Anomaly Detection

산업 설비의 센서 데이터를 기반으로 설비 고장 여부와 고장 유형을 예측하고, FastAPI 서버와 WPF 클라이언트를 통해 결과를 실시간으로 모니터링하는 프로젝트입니다.

단순한 머신러닝 모델 학습에 그치지 않고 센서 데이터 생성, AI 추론, REST API, 데이터베이스 저장, 데스크톱 모니터링까지 하나의 시스템으로 구현했습니다.

## 주요 기능

- Random Forest 기반 설비 고장 여부 예측
- 고장으로 판정된 설비의 고장 유형 분류
- FastAPI 기반 REST API 제공
- MySQL 센서 데이터 및 이상 이력 저장
- 정상 및 이상 상황을 생성하는 센서 시뮬레이터
- WPF 기반 실시간 설비 상태 모니터링
- 최근 센서값 시계열 그래프
- 이상 발생 이력 DataGrid
- 고장 유형별 발생 통계 및 막대그래프
- 최근 평균 센서값과 이상 비율 표시

## 기술 스택

### AI / Backend

- Python
- FastAPI
- scikit-learn
- pandas
- joblib
- PyMySQL
- python-dotenv
- MySQL

### Client

- C#
- WPF
- LiveCharts2

## 프로젝트 구조

```text
EquipmentAnomalyDetection
├─ README.md
├─ .gitignore
├─ client
│  └─ EquipmentMonitor.Wpf
├─ data
│  └─ ai4i2020.csv
├─ database
└─ server
   ├─ .env.example
   ├─ requirements.txt
   ├─ app
   │  └─ main.py
   ├─ ml
   │  ├─ saved_models
   │  ├─ train_random_forest.py
   │  ├─ train_failure_type.py
   │  └─ train_isolation_forest.py
   └─ simulator
      └─ sensor_simulator.py
```

## 시스템 흐름

```text
Sensor Simulator
        ↓
     FastAPI
        ↓
Failure Detection Model
        ↓
Failure Type Model
        ↓
      MySQL
        ↓
    WPF Client
```

1. 센서 시뮬레이터가 설비 센서값을 생성하여 FastAPI의 `/predict` 엔드포인트로 전달합니다.
2. 고장 감지 모델이 설비의 정상 또는 고장 여부를 판단합니다.
3. 고장으로 판단되면 고장 유형 분류 모델을 추가로 실행합니다.
4. 센서값과 예측 결과를 MySQL에 저장합니다.
5. WPF 클라이언트가 REST API를 통해 최신 센서값, 이상 이력, 통계를 조회하여 표시합니다.

## 데이터셋

[AI4I 2020 Predictive Maintenance Dataset](https://archive.ics.uci.edu/dataset/601/ai4i+2020+predictive+maintenance+dataset)을 사용했습니다.

### 입력 특성

- Air Temperature
- Process Temperature
- Rotational Speed
- Torque
- Tool Wear

### 예측 대상

- Machine Failure
- Failure Type

### 주요 고장 유형

| 코드 | 의미 |
| --- | --- |
| TWF | Tool Wear Failure |
| HDF | Heat Dissipation Failure |
| PWF | Power Failure |
| OSF | Overstrain Failure |

## 모델 구조

### Failure Detection Model

5개의 센서값을 입력받아 설비의 정상/고장 여부를 분류합니다.

```python
RandomForestClassifier(
    n_estimators=100,
    class_weight="balanced",
    random_state=42
)
```

정상 데이터에 비해 고장 데이터가 적은 클래스 불균형을 완화하기 위해 `class_weight="balanced"` 옵션을 사용했습니다.

### Failure Type Classification Model

Failure Detection Model에서 고장으로 판정된 데이터를 대상으로 두 번째 Random Forest 모델이 `TWF`, `HDF`, `PWF`, `OSF` 중 고장 유형을 분류합니다. 명확한 고장 유형이 없는 일부 데이터는 `Unknown`으로 처리했습니다.

## 모델 성능

### Failure Detection Model

테스트 데이터 2,000건 기준 결과입니다.

| Metric | Score |
| --- | ---: |
| Accuracy | 0.98 |
| Failure Precision | 0.71 |
| Failure Recall | 0.69 |
| Failure F1-score | 0.70 |

Confusion Matrix:

```text
[[1913, 19],
 [  21, 47]]
```

고장 클래스의 데이터 수가 정상 클래스보다 적기 때문에 Accuracy만으로 평가하지 않고, 고장 클래스의 Precision, Recall, F1-score를 함께 확인했습니다.

### Failure Type Classification Model

고장 데이터를 대상으로 유형을 분류한 결과입니다.

| Failure Type | Precision | Recall | F1-score |
| --- | ---: | ---: | ---: |
| HDF | 0.79 | 1.00 | 0.88 |
| OSF | 0.75 | 0.75 | 0.75 |
| PWF | 0.93 | 0.78 | 0.85 |
| TWF | 0.88 | 0.78 | 0.82 |

Overall Accuracy: **0.82**

## Isolation Forest 비교 실험

초기에는 라벨 없이 이상 패턴을 탐지하는 비지도 학습 방식인 Isolation Forest도 실험했습니다.

```text
[[9378, 283],
 [ 282,  57]]
```

실험 결과 실제 고장 데이터에 대한 Recall이 낮았습니다. 고장 여부 라벨을 사용할 수 있는 데이터셋의 특성과 탐지 성능을 고려하여 최종 시스템에서는 지도학습 기반 Random Forest를 주 모델로 선택했습니다. Isolation Forest 학습 코드는 비교 실험용으로 유지했습니다.

## FastAPI 엔드포인트

### `GET /`

서버 실행 상태를 확인합니다.

### `POST /predict`

센서값을 입력받아 고장 여부와 고장 유형을 예측합니다.

요청 예시:

```json
{
  "air_temperature": 300.7,
  "process_temperature": 311.9,
  "rotational_speed": 1200,
  "torque": 68.0,
  "tool_wear": 220
}
```

응답 예시:

```json
{
  "machine_failure": true,
  "failure_probability": 0.81,
  "failure_type": "OSF",
  "failure_type_probability": 0.69
}
```

### `GET /sensor-readings`

최근 센서 데이터를 조회합니다.

### `GET /history`

최근 이상 발생 이력을 조회합니다.

## MySQL 데이터 저장

MySQL을 사용하여 센서값과 이상 이력을 저장합니다.

주요 테이블:

```text
sensor_reading
anomaly_event
```

모든 예측 요청의 센서값과 결과를 센서 기록으로 저장하고, `Machine Failure = 1`인 경우에는 이상 이벤트를 별도로 저장합니다.

## 센서 시뮬레이터

실제 센서 장비 없이 전체 시스템 흐름을 테스트할 수 있도록 Python 시뮬레이터가 센서값을 생성하고 `/predict` API로 전송합니다.

지원 시나리오:

```text
normal
overheat
high_torque
high_tool_wear
```

출력 예시:

```text
시나리오: high_torque
센서값: {...}
AI 결과: {...}
```

시나리오 이름은 AI 모델에 제공되는 정답값이 아니라 테스트를 위해 생성한 센서 패턴을 의미합니다.

## WPF 실시간 모니터링

WPF 클라이언트는 1초마다 FastAPI 서버에서 최신 데이터를 조회합니다.

표시 항목:

- Air Temperature
- Process Temperature
- RPM
- Torque
- Tool Wear
- Failure Probability
- Failure Type Probability
- Current Status
- Recent Anomaly History
- Average Air Temperature
- Average Torque
- Anomaly Count
- Latest Failure Type
- Anomaly Rate
- Failure Type Counts

정상 상태는 `NORMAL`, 고장 상태는 `DANGER - OSF`와 같이 표시되며 상태에 따라 화면 색상도 변경됩니다.

### 실시간 그래프

LiveCharts2를 사용하여 최근 센서 데이터를 시각화합니다.

- Air Temperature와 Process Temperature 시계열 표시
- X축에 센서 데이터 생성 시간 표시
- 고장 유형별 발생 횟수를 막대그래프로 표시

## 실행 방법

### 1. 저장소 준비

저장소를 받은 뒤 프로젝트 루트에서 `server` 폴더로 이동합니다.

```powershell
cd server
```

### 2. Python 가상환경 생성

```powershell
python -m venv .venv
```

### 3. Python 패키지 설치

```powershell
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

### 4. 환경변수 설정

`server/.env.example`을 복사하여 `server/.env` 파일을 생성한 뒤 실제 MySQL 접속 정보를 입력합니다.

```env
DB_HOST=127.0.0.1
DB_USER=root
DB_PASSWORD=your_password
DB_NAME=equipment_anomaly
```

`DB_PASSWORD`에는 예시 문구가 아니라 실제 MySQL 비밀번호를 입력해야 합니다.

### 5. MySQL 준비

MySQL에서 `DB_NAME`에 지정한 데이터베이스와 프로젝트에서 사용하는 테이블을 준비합니다. 접속 계정에는 해당 데이터베이스에 대한 읽기 및 쓰기 권한이 필요합니다.

### 6. FastAPI 서버 실행

프로젝트 루트 기준:

```powershell
cd server\app
..\.venv\Scripts\python.exe -m uvicorn main:app --reload
```

서버가 실행되면 기본 주소와 FastAPI 문서를 확인할 수 있습니다.

```text
http://127.0.0.1:8000
http://127.0.0.1:8000/docs
```

### 7. 센서 시뮬레이터 실행

새 터미널에서 프로젝트 루트를 기준으로 실행합니다.

```powershell
cd server\simulator
..\.venv\Scripts\python.exe sensor_simulator.py
```

### 8. WPF 클라이언트 실행

다음 솔루션 파일을 Visual Studio에서 열어 실행합니다.

```text
client/EquipmentMonitor.Wpf/EquipmentMonitor.Wpf.sln
```

FastAPI 서버가 먼저 실행되어 있어야 최신 센서값과 이상 이력을 정상적으로 조회할 수 있습니다.

## 환경설정 및 보안

데이터베이스 접속 정보는 소스 코드에 직접 작성하지 않고 `server/.env` 파일로 분리합니다. 실제 비밀번호가 포함된 `.env`는 저장소에 올리지 않고, 필요한 환경변수 이름만 담은 `.env.example`을 공유합니다.

프로젝트 루트의 `.gitignore`에는 다음과 같은 항목을 포함합니다.

```gitignore
# Python
server/.venv/
**/__pycache__/
*.pyc

# Environment variables
server/.env

# C# / Visual Studio
**/bin/
**/obj/
.vs/
*.user
*.suo

# OS
Thumbs.db
.DS_Store
```

특히 실제 인증 정보가 포함된 `server/.env`와 다시 생성할 수 있는 가상환경 `server/.venv/`는 Git에서 반드시 제외합니다. `.env.example`은 실제 비밀번호가 없으므로 저장소에 포함합니다.

## 프로젝트 목표

이 프로젝트의 목표는 머신러닝 모델 하나를 학습하는 데서 끝나지 않고 다음과 같은 전체 예측 및 모니터링 파이프라인을 직접 구현하는 것입니다.

```text
Sensor Data
→ AI Prediction
→ REST API
→ Database
→ Desktop Monitoring
```

이를 통해 모델 학습과 평가뿐 아니라 백엔드 API, 데이터 영속화, 실시간 데이터 생성, 데스크톱 UI 시각화가 연결되는 엔드 투 엔드 시스템을 구성했습니다.

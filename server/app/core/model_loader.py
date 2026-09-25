from pathlib import Path

import joblib


MODEL_DIR = Path(__file__).resolve().parent.parent.parent / "ml" / "saved_models"

failure_model = joblib.load(
    MODEL_DIR / "random_forest.pkl"
)

failure_type_model = joblib.load(
    MODEL_DIR / "failure_type_random_forest.pkl"
)

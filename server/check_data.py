import pandas as pd

df = pd.read_csv("../data/ai4i2020.csv")

print("=== 앞 5개 ===")
print(df.head())

print("\n=== 컬럼 목록 ===")
print(df.columns.tolist())

print("\n=== 데이터 정보 ===")
df.info()

print("\n=== 결측치 정보 ===")
print(df.isnull().sum())

print("\n=== Machine failure 분포 ===")
print(df["Machine failure"].value_counts())

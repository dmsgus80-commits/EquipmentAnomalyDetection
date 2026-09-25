from core.database import get_db_connection


def save_prediction(data, result):
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
                    result["machine_failure"],
                    result["failure_probability"],
                    result["failure_type"],
                    result["failure_type_probability"]
                )
            )

            sensor_reading_id = cursor.lastrowid

            if result["machine_failure"]:
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
                        result["failure_type"],
                        result["failure_probability"],
                        result["failure_type_probability"]
                    )
                )

        conn.commit()

    finally:
        conn.close()


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
                    status,
                    acknowledged_at,
                    resolved_at,
                    created_at
                FROM anomaly_event
                ORDER BY created_at DESC
                LIMIT 100
            """

            cursor.execute(sql)

            return cursor.fetchall()

    finally:
        conn.close()


def get_history_summary():
    conn = get_db_connection()

    try:
        with conn.cursor() as cursor:
            cursor.execute("""
                SELECT status, COUNT(*) AS count
                FROM anomaly_event
                GROUP BY status
            """)
            counts = {row["status"]: row["count"] for row in cursor.fetchall()}
            return {
                "total": sum(counts.values()),
                "unacknowledged": counts.get("UNACKNOWLEDGED", 0),
                "acknowledged": counts.get("ACKNOWLEDGED", 0),
                "resolved": counts.get("RESOLVED", 0)
            }
    finally:
        conn.close()


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

            return cursor.fetchall()

    finally:
        conn.close()


def update_alarm_status(alarm_id: int, expected_status: str, new_status: str, timestamp_column: str):
    conn = get_db_connection()

    try:
        with conn.cursor() as cursor:
            cursor.execute("SELECT status FROM anomaly_event WHERE id = %s FOR UPDATE", (alarm_id,))
            alarm = cursor.fetchone()
            if alarm is None:
                conn.rollback()
                return "not_found"
            if alarm["status"] != expected_status:
                conn.rollback()
                return "invalid_state"

            # Column names come only from the fixed wrappers below.
            cursor.execute(
                f"UPDATE anomaly_event SET status = %s, {timestamp_column} = NOW() WHERE id = %s",
                (new_status, alarm_id)
            )
        conn.commit()
        return "updated"

    except Exception:
        conn.rollback()
        raise
    finally:
        conn.close()


def acknowledge_alarm(alarm_id: int):
    return update_alarm_status(alarm_id, "UNACKNOWLEDGED", "ACKNOWLEDGED", "acknowledged_at")


def resolve_alarm(alarm_id: int):
    return update_alarm_status(alarm_id, "ACKNOWLEDGED", "RESOLVED", "resolved_at")

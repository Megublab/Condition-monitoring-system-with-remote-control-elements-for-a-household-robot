import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./Device.css";

interface UserData {
  user_id: number;
  user_name: string;
  device_id: string;
  device_name?: string;
  ip_address: string;
  port: number;
}

export function Device() {
  const navigate = useNavigate();
  const [user, setUser] = useState<UserData | null>(null);
  const [message, setMessage] = useState("");
  const [deviceResponse, setDeviceResponse] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const storedUser = localStorage.getItem("userData");
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem("userData");
      }
    }
  }, []);

  const sendDeviceCommand = async (commandText: string) => {
    if (!user) return;
    setLoading(true);
    setError(null);
    setMessage("");
    setDeviceResponse(null);

    try {
      const response = await axios.post(
        "https://localhost:7042/api/DeviceCommand/send-command",
        {
          deviceId: user.device_id,
          ipAddress: user.ip_address,
          port: user.port,
          commandText,
        }
      );

      const data = response.data;
      setMessage(data.message || "");
      setDeviceResponse(data.device_response || "Немає відповіді від пристрою.");
    } catch (err: any) {
      setError(err.response?.data?.message || "Помилка при відправці команди.");
    } finally {
      setLoading(false);
    }
  };

  const deleteDevice = async () => {
  if (!user) return;
  setLoading(true);
  setError(null);
  setMessage("");
  setDeviceResponse(null);

  try {
    await axios.post(`https://localhost:7042/api/Device/delete/${user.user_id}`);

    const updatedUser: UserData = {
      ...user,
      device_id: "00000000-0000-0000-0000-000000000000",
      ip_address: "",
      port: 0,
    };

    localStorage.setItem("userData", JSON.stringify(updatedUser));
    setUser(updatedUser);
    setMessage("Пристрій успішно видалено.");
  } catch (err: any) {
    setError(err.response?.data?.message || "Помилка при видаленні пристрою.");
  } finally {
    setLoading(false);
  }
 };

 const formatDeviceResponse = (response: string | null) => {
  if (!response) return null;

  try {
    const parsed = JSON.parse(response);

    if (typeof parsed === "object" && parsed !== null) {
      const translatedItems = Object.entries(parsed).map(([key, rawValue]) => {
        const value = String(rawValue); 

        switch (key) {
          case "battery":
            return (
              <li key={key}>
                🔋 <strong>Заряд пристрою:</strong> {value}%
              </li>
            );

          case "state":
            let stateLabel = "";
            switch (value) {
              case "idle":
                stateLabel = "Стоїть на станції";
                break;
              case "cleaning":
                stateLabel = "Виконує прибирання";
                break;
              case "returning":
                stateLabel = "Повертається на базу";
                break;
              case "docking":
                stateLabel = "Здійснює стикування з базою";
                break;
              default:
                stateLabel = value;
            }
            return (
              <li key={key}>
                📍 <strong>Статус пристрою:</strong> {stateLabel}
              </li>
            );

          case "available_commands":
            const commands = value.replaceAll("/", "").split(",").join(", ");
            return (
              <li key={key}>
                🛠️ <strong>Доступні команди:</strong> {commands}
              </li>
            );

          case "message":
            let messageText = "";

            switch (value.toLowerCase()) {
              case "already docking":
                messageText = "Пристрій вже стикується з базою.";
                break;
              case "returning to dock":
                messageText = "Пристрій повертається на базу.";
                break;
              case "cleaning stopped":
                messageText = "Прибирання зупинено.";
                break;
              case "already stopped":
                messageText = "Пристрій вже зупинено.";
                break;
              case "cleaning started":
                messageText = "Прибирання розпочато.";
                break;
              case "already cleaning":
                messageText = "Пристрій вже прибирає.";
                break;
              default:
                messageText = value;
            }

            return (
              <li key={key}>
                💬 <strong>Повідомлення:</strong> {messageText}
              </li>
            );

          case "device_name":
            return (
              <li key={key}>
                🧾 <strong>Назва пристрою:</strong> {value}
              </li>
            );

          case "model":
          case "model_name":
            return (
              <li key={key}>
                🆔 <strong>Модель пристрою:</strong> {value}
              </li>
            );

          default:
            return (
              <li key={key}>
                <strong>{key}:</strong> {value}
              </li>
            );
        }
      });

      return <ul className="formatted-response">{translatedItems}</ul>;
    }
  } catch {
    
  }

  return <pre>{response}</pre>;
};




  if (!user) {
    return (
      <div className="device-container">
        <h2>Увійдіть у профіль</h2>
        <p>Щоб отримати доступ до пристроїв, будь ласка, увійдіть або зареєструйтесь.</p>
        <div className="device-buttons">
          <button onClick={() => navigate("/login")} className="btn-login">
            Увійти
          </button>
          <button onClick={() => navigate("/register")} className="btn-register">
            Зареєструватися
          </button>
        </div>
      </div>
    );
  }

  const isDeviceMissing = user.device_id === "00000000-0000-0000-0000-000000000000";

  if (isDeviceMissing) {
    return (
      <div className="device-container">
        <h2>Вітаємо, {user.user_name}!</h2>
        <p>Ви ще не додали пристрій. Додайте його зараз, щоб почати керування.</p>
        <button onClick={() => navigate("/add-device")} className="btn-add-device">
          Додати пристрій
        </button>
      </div>
    );
  }

  const commandButtons = ["status", "name", "commands", "dock", "stop", "start"];

  return (
  <div className="device-container">
    <h2>Ваш пристрій</h2>
    {user.device_name && (
      <p className="device-name-display">
        Ім'я пристрою: <strong>{user.device_name}</strong>
      </p>
    )}
    <p>Керування та взаємодія з пристроєм.</p>

    <div className="device-buttons">
      {commandButtons.map((cmd) => (
        <button
          key={cmd}
          onClick={() => sendDeviceCommand(cmd)}
          className="command-button"
          disabled={loading}
        >
          {cmd.toUpperCase()}
        </button>
      ))}
      <button
        onClick={deleteDevice}
        className="btn-delete-device"
        disabled={loading}
      >
        Видалити пристрій
      </button>
    </div>

    {loading && <p>Виконується запит...</p>}
    {error && <p style={{ color: "red" }}>Помилка: {error}</p>}
    {message && <p>{message}</p>}
    {deviceResponse && (
      <div className="commands-list">
        <h3>Відповідь пристрою:</h3>
        {formatDeviceResponse(deviceResponse)}
      </div>
    )}
  </div>
  );
}

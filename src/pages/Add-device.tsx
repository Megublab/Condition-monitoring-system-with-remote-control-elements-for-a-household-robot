import React, { useEffect, useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./AddDevice.css";

interface DeviceItem {
  ipAddress: string;
  port: number;
  deviceName: string; 
}

interface ParsedDeviceName {
  device_name: string;
  model: string;
}

interface UserData {
  user_id: number;
  user_name: string;
  device_id: string;
  has_device: boolean;
}

export function AddDevice() {
  const navigate = useNavigate();
  const [user, setUser] = useState<UserData | null>(null);
  const [devices, setDevices] = useState<DeviceItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [addingDeviceIp, setAddingDeviceIp] = useState<string | null>(null);
  const [searched, setSearched] = useState(false);

  useEffect(() => {
    const storedUser = localStorage.getItem("userData");
    if (storedUser) {
      try {
        setUser(JSON.parse(storedUser));
      } catch {
        localStorage.removeItem("userData");
        navigate("/login");
      }
    } else {
      navigate("/login");
    }
  }, [navigate]);

  const searchDevices = async () => {
    setLoading(true);
    setError(null);
    setDevices([]);
    setSearched(true);

    try {
      const response = await axios.get<DeviceItem[]>(
        "https://localhost:7042/api/Device/search"
      );
      setDevices(response.data);
    } catch {
      setError("Не вдалося отримати список пристроїв.");
    } finally {
      setLoading(false);
    }
  };

 const handleAddDevice = async (device: DeviceItem) => {
  if (!user) return;
  setAddingDeviceIp(device.ipAddress);
  setError(null);

  try {
    const parsedName = JSON.parse(device.deviceName);

    const deviceData = {
      deviceName: parsedName.device_name,
      ipAddress: device.ipAddress,
      port: device.port,
    };

    const response = await axios.put(
      `https://localhost:7042/api/Auth/add-device/${user.user_id}`,
      deviceData,
      {
        headers: { "Content-Type": "application/json" },
      }
    );

    const { device_id } = response.data;

    const updatedUser = {
      ...user,
      device_id,
      has_device: true,
      ip_address: device.ipAddress,
      port: device.port,
      device_name: parsedName.device_name,
      model_name: parsedName.model,
    };

    setUser(updatedUser);
    localStorage.setItem("userData", JSON.stringify(updatedUser));

    alert("Пристрій додано та пов’язано з користувачем.");
  } catch {
    setError("Помилка при додаванні пристрою.");
  } finally {
    setAddingDeviceIp(null);
  }
};



  if (!user) return null;

  return (
    <div className="add-device-container">
      <h2>Додати пристрій</h2>

      {!searched && (
        <button onClick={searchDevices} className="btn-search-devices">
          Шукати пристрої
        </button>
      )}

      {loading && <p>Завантаження пристроїв...</p>}

      {error && <p className="error-message">{error}</p>}

      {searched && !loading && (
        <>
          {devices.length === 0 ? (
            <p>Пристрої не знайдені.</p>
          ) : (
            <ul className="device-list">
              {devices.map((device) => {
                let parsedName: ParsedDeviceName = { device_name: "Unknown", model: "Unknown" };
                try {
                  parsedName = JSON.parse(device.deviceName);
                } catch {}

                return (
                  <li key={`${device.ipAddress}:${device.port}`} className="device-list-item">
                    <div className="device-info">
                      <div><strong>IP:</strong> {device.ipAddress}</div>
                      <div><strong>Port:</strong> {device.port}</div>
                      <div><strong>Назва:</strong> {parsedName.device_name}</div>
                      <div><strong>Модель:</strong> {parsedName.model}</div>
                    </div>
                    <button
                      disabled={addingDeviceIp === device.ipAddress}
                      onClick={() => handleAddDevice(device)}
                    >
                      {addingDeviceIp === device.ipAddress ? "Додаємо..." : "Додати пристрій"}
                    </button>
                  </li>
                );
              })}
            </ul>
          )}

          <button onClick={searchDevices} className="btn-search-again">
            Шукати знову
          </button>
        </>
      )}
    </div>
  );
}

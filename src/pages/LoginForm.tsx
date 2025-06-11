import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./Login.css";

export function Login() {
  const navigate = useNavigate();

  const [userData, setUserData] = useState<{
    user_id: number;
    user_name: string;
    device_id?: string;
    has_device?: boolean;
    device_name?: string;
    model_name?: string;
    ip_address?: string;
    port?: number;
  } | null>(null);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const saved = localStorage.getItem("userData");
    if (saved) {
      setUserData(JSON.parse(saved));
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {

      const loginRes = await axios.post(
        "https://localhost:7042/api/Auth/login",
        { email, password },
        { headers: { "Content-Type": "application/json" } }
      );

      const { user_id, user_name } = loginRes.data;

      const minimalUserData = { user_id, user_name };
      setUserData(minimalUserData);
      localStorage.setItem("userData", JSON.stringify(minimalUserData));


      const userInfoRes = await axios.post(
        "https://localhost:7042/api/Auth/userinfo",
        { userId: user_id },
        { headers: { "Content-Type": "application/json" } }
      );


      const fullUserData = {
        ...minimalUserData,
        ...userInfoRes.data,
      };

      setUserData(fullUserData);
      localStorage.setItem("userData", JSON.stringify(fullUserData));
    } catch (err: any) {
      if (err.response && err.response.status === 401) {
        setError("Невірний email або пароль.");
      } else {
        setError("Сталася помилка. Спробуйте пізніше.");
      }
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("userData");
    setUserData(null);
    setEmail("");
    setPassword("");
    setError(null);
  };

  if (userData) {

    const noDeviceId = "00000000-0000-0000-0000-000000000000";
    const hasRealDevice =
      userData.device_id && userData.device_id !== noDeviceId;

    return (
      <div className="login-success">
        <h2>Вітаємо, {userData.user_name}!</h2>
        {hasRealDevice ? (
          <p>
            Ваш пристрій очікує команд, <strong>{userData.device_name}</strong>.
          </p>
        ) : (
          <>
            <p>У вас немає пристрою.</p>
            <button
              className="btn-add-device"
              onClick={() => navigate("/add-device")}
            >
              Додати пристрій
            </button>
          </>
        )}
        <button
          onClick={handleLogout}
          className="logout-button"
          style={{ marginTop: "20px" }}
        >
          Вийти
        </button>
      </div>
    );
  }

  return (
    <div className="login-container">
      <h1>Вхід до системи</h1>
      <form onSubmit={handleSubmit} className="login-form">
        <label>
          Email:
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="username"
          />
        </label>
        <label>
          Пароль:
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
        </label>
        {error && <div className="error-message">{error}</div>}
        <button type="submit" className="login-button">
          Увійти
        </button>
      </form>
    </div>
  );
}

import { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./Register.css";

export function Register() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    const userData = localStorage.getItem("userData");
    if (userData) {
      localStorage.removeItem("userData");
    }
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    const { username, email, password } = formData;

    if (!username || !email || !password) {
      setError("Усі поля обов'язкові.");
      return;
    }

    try {

      await axios.post(
        "https://localhost:7042/api/Register",
        formData,
        {
          headers: { "Content-Type": "application/json" },
        }
      );

      setSuccessMessage("Реєстрація успішна. Зараз виконується вхід...");

      setTimeout(async () => {
        try {
          const loginResponse = await axios.post(
            "https://localhost:7042/api/Auth/login",
            { email, password },
            {
              headers: { "Content-Type": "application/json" },
            }
          );

          const { user_id, user_name } = loginResponse.data;


          const minimalUserData = { user_id, user_name };
          localStorage.setItem("userData", JSON.stringify(minimalUserData));


          const userInfoRes = await axios.post(
            "https://localhost:7042/api/Auth/userinfo",
            { userId: user_id },
            {
              headers: { "Content-Type": "application/json" },
            }
          );


          const fullUserData = {
            ...minimalUserData,
            ...userInfoRes.data,
          };


          localStorage.setItem("userData", JSON.stringify(fullUserData));


          navigate("/");

        } catch (loginErr: any) {
          setError(
            loginErr.response?.data?.message || "Помилка входу після реєстрації."
          );
          setSuccessMessage("");
        }
      }, 1500);
    } catch (err: any) {
      const message = err.response?.data?.message || "Помилка при реєстрації.";
      setError(message);
      setSuccessMessage("");
    }
  };

  return (
    <div className="register-container">
      <h2 className="register-title">Реєстрація</h2>

      {error && <div className="register-error">{error}</div>}
      {successMessage && <div className="register-success">{successMessage}</div>}

      <form onSubmit={handleSubmit} className="register-form">
        <div className="register-field">
          <label className="register-label">Ім'я користувача</label>
          <input
            type="text"
            name="username"
            value={formData.username}
            onChange={handleChange}
            className="register-input"
            required
          />
        </div>

        <div className="register-field">
          <label className="register-label">Email</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            className="register-input"
            required
          />
        </div>

        <div className="register-field">
          <label className="register-label">Пароль</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            className="register-input"
            required
          />
        </div>

        <button type="submit" className="register-button">
          Зареєструватися
        </button>
      </form>
    </div>
  );
}

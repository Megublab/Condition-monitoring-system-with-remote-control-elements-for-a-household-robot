import React from "react";
import { useNavigate } from "react-router-dom";
import "./Home.css";

export function Home() {
  const navigate = useNavigate();

  return (
    <div className="home-container">
      <h1>Ласкаво просимо до системи керування</h1>
      <p className="home-description">
        Сервіс дозволяє легко керувати вашим пристроєм через
        простий та зручний інтерфейс. 
      </p>
      <p className="home-description"> 
        Увійдіть щоб побачити свій пристрій, або зареєструйтеся щоб додати новий.
      </p>
      <div className="home-buttons">
        <button onClick={() => navigate("/register")} className="btn-register">
          Реєстрація
        </button>
        <button onClick={() => navigate("/login")} className="btn-login">
          Логін
        </button>
      </div>
    </div>
  );
}

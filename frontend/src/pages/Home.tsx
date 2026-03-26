import { Link } from "react-router-dom";
import './Home.css'

export default function Home() {
  return (
    <main className="page home-container">
      <div className="background-glow background-glow-left"></div>
      <div className="background-glow background-glow-right"></div>

      <section className="home-card">
        <div className="home-content">

          <h1 className="home-title">HabitHub</h1>

          <p className="home-subtitle">Build better habits together.</p>

          <p className="home-text">
            Stay accountable, track shared progress, and keep your team motivated
            in one place.
          </p>

          <div className="home-buttons">
            <Link to="/register" className="button button-primary">
              Get started
            </Link>

            <Link to="/login" className="button button-secondary">
              Log in
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}
import { Link } from "react-router-dom";
import "./MainDashboard.css";
import "../App.css";

const auth = JSON.parse(localStorage.getItem("habithubAuth") ?? "{}");
const name = auth.name ?? "John" ;

export default function MainCreator() {
  return (
  <main className="page container dashboard-page">
    <div className="background-glow background-glow-left"></div>
    <div className="background-glow background-glow-right"></div>

    <section className="card dashboard-card">
      <div className="content dashboard-content">
        <div className="dashboard-top">
          <Link to="/" className="button button-secondary dashboard-pill">
            Home
          </Link>

          <div className="dashboard-top-right">
            <button type="button" className="button button-secondary dashboard-pill">
              Notifications (1)
            </button>
          </div>
        </div>

        <div className="dashboard-hero">
          <h1 className="title dashboard-title">Hello {name}!</h1>
        </div>

        <div className="buttons dashboard-actions">
          <button type="button" className="button button-primary dashboard-action">
            View your teams
          </button>

          <Link
            to="/settings"
            className="button button-secondary dashboard-action"
          >
            Settings
          </Link>
        </div>
      </div>
    </section>
  </main>
  );
}
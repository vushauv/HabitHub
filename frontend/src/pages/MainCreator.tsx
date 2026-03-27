import { Link } from "react-router-dom";
import "./MainDashboard.css";

const auth = JSON.parse(localStorage.getItem("habithubAuth") ?? "{}");
const name = auth.name ?? "John";

export default function MainCreator() {
  return (
    <main className="page dashboard-container">
      <div className="background-glow background-glow-left"></div>
      <div className="background-glow background-glow-right"></div>

      <section className="dashboard-card">
        <div className="dashboard-content">
          <div className="dashboard-top">
            <Link to="/" className="dashboard-pill">
              Home
            </Link>

            <div className="dashboard-top-right">
              <button type="button" className="dashboard-pill">
                Notifications (1)
              </button>
            </div>
          </div>

          <div className="dashboard-hero">
            <h1 className="dashboard-title">Hello {name}!</h1>
          </div>

          <div className="dashboard-actions">
            <button type="button" className="dashboard-action dashboard-action-primary">
              View active sessions
            </button>

            <button type="button" className="dashboard-action">
              Change password
            </button>

            <button type="button" className="dashboard-action">
              Change email
            </button>

            <button type="button" className="dashboard-action">
              Manage your teams
            </button>
          </div>
        </div>
      </section>
    </main>
  );
}
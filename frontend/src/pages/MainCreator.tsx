import { Link, useOutletContext } from "react-router-dom";
import "./MainDashboard.css";
import "../App.css";
import type { UserDto } from "../services/dtos";

export default function MainCreator() {
  const currentUser = useOutletContext<UserDto>();

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
            <h1 className="title dashboard-title">Hello {currentUser.name}!</h1>
          </div>

          <div className="buttons dashboard-actions">
            <Link
              to="/teams-creator"
              className="button button-primary dashboard-action"
            >
              Manage your teams
            </Link>

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

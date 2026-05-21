import { Link, useOutletContext } from "react-router-dom";
import "./MainDashboard.css";
import "../App.css";
import type { UserDto } from "../services/dtos";
import NotificationsDashboardLink from "../components/NotificationsDashboardLink";

export default function MainMember() {
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
            <NotificationsDashboardLink />
          </div>
        </div>

        <div className="dashboard-hero">
          <h1 className="title dashboard-title">Hello {currentUser.name}!</h1>
        </div>

        <div className="buttons dashboard-actions">
          <Link
            to="/member/teams"
            className="button button-primary dashboard-action"
          >
            View your teams
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

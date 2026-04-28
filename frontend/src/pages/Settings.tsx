import { Link } from "react-router-dom";
import "./Settings.css";
import "../App.css"
import {securityActions} from "../services/Settings.ts";

export default function Settings() {
  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content settings-content">
            <div className="page-topbar">
              <Link to="/" className="button button-secondary page-nav-button">
                Home
              </Link>
            </div>

            <div className="content-centered settings-header">
              <h1 className="title settings-title">Security settings</h1>
              <p className="text settings-text">
                Manage your account security in one place. You can change your
                password, update your email, and review active sessions.
              </p>
            </div>

            <section className="settings-grid" aria-label="Security actions">
              {securityActions.map((action) => (
                <Link key={action.to} to={action.to} className="settings-option">
                  <div className="settings-option-body">
                    <h2 className="settings-option-title">{action.title}</h2>
                    <p className="settings-option-description">
                      {action.description}
                    </p>
                  </div>

                  <span className="settings-option-arrow" aria-hidden="true">
                    →
                  </span>
                </Link>
              ))}
            </section>
          </div>
        </div>
      </section>
    </main>
  );
}
import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div>
      <h1>HabitHub</h1>
      <p>Track your habits, build your future.</p>
      <Link to="/register">Get Started</Link>
    </div>
  );
}

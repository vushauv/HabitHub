import { useLocation, useParams } from "react-router-dom";

export default function PathDisplay() {
  const params = useParams();
  const location = useLocation();
  return <p>This is /{params["*"] ?? location.pathname}!</p>
}
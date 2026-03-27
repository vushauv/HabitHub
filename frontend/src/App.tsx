import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import MainCreator  from "./pages/MainCreator";
import MainMember from "./pages/MainMember";
import ProtectedRoute from "./ProtectedRoute";


export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

         <Route element={<ProtectedRoute allowedUserType="Creator" />}>
          <Route path="/main-creator" element={<MainCreator />} />
        </Route>

        <Route element={<ProtectedRoute allowedUserType="Member" />}>
          <Route path="/main-member" element={<MainMember />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
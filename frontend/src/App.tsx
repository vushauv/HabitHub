import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import MainCreator  from "./pages/MainCreator";
import MainMember from "./pages/MainMember";
import ProtectedRoute from "./ProtectedRoute";
import Settings from "./pages/Settings";
import Sessions from "./pages/Sessions";
import ChangePassword from "./pages/ChangePassword";
import ChangeEmail from "./pages/ChangeEmail";

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

        <Route element={<ProtectedRoute />}>
          <Route path="/settings" element={<Settings />} />
        </Route>

        <Route element={<ProtectedRoute/>}> 
          <Route path="/settings/sessions" element={<Sessions />}/>
        </Route>

        <Route element={<ProtectedRoute/>}> 
          <Route path="/settings/change-password" element={<ChangePassword />}/>
        </Route>

        <Route element={<ProtectedRoute/>}> 
          <Route path="/settings/change-email" element={<ChangeEmail />}/>
        </Route>


      </Routes> 
    </BrowserRouter>
  );
}
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
import TeamsCreator from "./pages/TeamsCreator";
import TeamsMember from "./pages/TeamsMember";
import CreateTeam from "./pages/CreateTeam";
import JoinTeam from "./pages/JoinTeam";
import MemberList from "./pages/MemberList";
import CreatorHabits from "./pages/CreatorHabits";
import CreateHabit from "./pages/CreateHabit";
import HabitDetails from "./pages/HabitDetails";
import EditHabit from "./pages/EditHabit";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

         <Route element={<ProtectedRoute allowedUserType="Creator" />}>
          <Route path="/main-creator" element={<MainCreator />} />
          <Route path="/teams-creator" element={<TeamsCreator />} />
          <Route path="/create-team" element={<CreateTeam />} />
          <Route path="/member-list" element={<MemberList />} />
          <Route path="/habits-creator" element={<CreatorHabits />} />
          <Route path="/create-habit" element={<CreateHabit />} />
          <Route path="/habit-details" element={<HabitDetails />} />
          <Route path="/edit-habit" element={<EditHabit />} />
        </Route>

        <Route element={<ProtectedRoute allowedUserType="Member" />}>
          <Route path="/main-member" element={<MainMember />} />
          <Route path="/teams-member" element={<TeamsMember />} />
          <Route path="/join-team" element={<JoinTeam />} />
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

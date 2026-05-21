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
import MemberHabits from "./pages/MemberHabits";
import MemberHabitDetails from "./pages/MemberHabitDetails";
import HabitLeaderboard from "./pages/HabitLeaderboard";
import MemberLogHabit from "./pages/MemberLogHabit";
import Chat from "./pages/Chat";
import Reminders from "./pages/Reminders";
import Notifications from "./pages/Notifications";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

         <Route element={<ProtectedRoute allowedUserType="Creator" />}>
          <Route path="/creator" element={<MainCreator />} />
          <Route path="/creator/teams" element={<TeamsCreator />} />
          <Route path="/creator/teams/create" element={<CreateTeam />} />
          <Route path="/creator/teams/:teamId/members" element={<MemberList />} />
          <Route path="/creator/teams/:teamId/habits" element={<CreatorHabits />} />
          <Route path="/creator/teams/:teamId/habits/new" element={<CreateHabit />} />
          <Route path="/creator/teams/:teamId/habits/:habitId/details" element={<HabitDetails />} />
          <Route path="/creator/teams/:teamId/habits/:habitId/edit" element={<EditHabit />} />
          <Route path="/creator/teams/:teamId/chat" element={<Chat />} />
        </Route>

        <Route element={<ProtectedRoute allowedUserType="Member" />}>
          <Route path="/member" element={<MainMember />} />
          <Route path="/member/teams" element={<TeamsMember />} />
          <Route path="/member/teams/join" element={<JoinTeam />} />
          <Route path="/member/teams/:teamId/habits" element={<MemberHabits />} />
          <Route path="/member/teams/:teamId/habits/:habitId/details" element={<MemberHabitDetails />} />
          <Route path="/member/teams/:teamId/habits/:habitId/log" element={<MemberLogHabit />} />
          <Route path="/member/teams/:teamId/chat" element={<Chat />} />
        </Route>

        <Route element={<ProtectedRoute />}>
          <Route path="/notifications" element={<Notifications />} />
          <Route path="/reminders" element={<Reminders />} />
          <Route path="/settings" element={<Settings />} />
          <Route path="/teams/:teamId/habits/:habitId/leaderboard" element={<HabitLeaderboard />} />
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

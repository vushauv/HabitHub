import type { StoredAuth } from "./Auth";
import {
  getMyTodayEntryStatus,
  getTeamHabits,
} from "./Habit";
import {
  extractReminderContentDetails,
  getReminderAlerts,
  type ReminderAlertDto,
} from "./Reminder";
import { getTeams } from "./Team";

export type ReminderSource = {
  habitId: string;
  habitName: string;
  teamName: string;
  goal: string | null;
  isDoneToday: boolean;
};

export type ReminderSourcesByHabitName = Record<string, ReminderSource[]>;

export type VisibleUnreadReminderData = {
  reminders: ReminderAlertDto[];
  sourcesByHabitName: ReminderSourcesByHabitName;
};

export async function getVisibleUnreadReminderData(
  auth: StoredAuth | null,
): Promise<VisibleUnreadReminderData> {
  const [reminders, sourcesByHabitName] = await Promise.all([
    getReminderAlerts(auth),
    loadReminderSourcesByHabitName(auth),
  ]);

  return {
    reminders: reminders.filter((reminder) =>
      shouldShowReminder(reminder, sourcesByHabitName),
    ),
    sourcesByHabitName,
  };
}

export async function getVisibleUnreadReminderCount(
  auth: StoredAuth | null,
): Promise<number> {
  const { reminders } = await getVisibleUnreadReminderData(auth);

  return reminders.length;
}

export function formatReminderHabitName(reminder: ReminderAlertDto): string {
  return (
    extractReminderContentDetails(reminder.content).habitName ??
    "Reminder"
  );
}

export function formatReminderTeamName(
  reminder: ReminderAlertDto,
  sourcesByHabitName: ReminderSourcesByHabitName,
): string {
  const { habitName, teamName } = extractReminderContentDetails(
    reminder.content,
  );

  if (!habitName) {
    return "General";
  }

  if (teamName) {
    return teamName;
  }

  const teamNames = getReminderSources(
    habitName,
    sourcesByHabitName,
    teamName,
  ).map((source) => source.teamName);

  if (teamNames.length === 0) {
    return "Unknown team";
  }

  return [...new Set(teamNames)].join(", ");
}

export function formatReminderContext(
  reminder: ReminderAlertDto,
  sourcesByHabitName: ReminderSourcesByHabitName,
): string {
  const { habitName, teamName } = extractReminderContentDetails(
    reminder.content,
  );

  if (!habitName) {
    return reminder.content;
  }

  const goals = getReminderSources(habitName, sourcesByHabitName, teamName)
    .map((source) => source.goal?.trim())
    .filter((goal): goal is string => Boolean(goal));

  if (goals.length === 0) {
    return "No goal";
  }

  return [...new Set(goals)].join(", ");
}

function shouldShowReminder(
  reminder: ReminderAlertDto,
  sourcesByHabitName: ReminderSourcesByHabitName,
): boolean {
  return (
    reminder.status === "Unread" &&
    !isReminderDoneToday(reminder, sourcesByHabitName)
  );
}

function isReminderDoneToday(
  reminder: ReminderAlertDto,
  sourcesByHabitName: ReminderSourcesByHabitName,
): boolean {
  const { habitName, teamName } = extractReminderContentDetails(
    reminder.content,
  );

  if (!habitName) {
    return false;
  }

  const sources = getReminderSources(habitName, sourcesByHabitName, teamName);

  return sources.length > 0 && sources.every((source) => source.isDoneToday);
}

function getReminderSources(
  habitName: string | null,
  sourcesByHabitName: ReminderSourcesByHabitName,
  teamName: string | null,
): ReminderSource[] {
  if (!habitName) {
    return [];
  }

  const sources = sourcesByHabitName[normalizeHabitName(habitName)] ?? [];
  const normalizedTeamName = teamName?.trim().toLowerCase();

  if (!normalizedTeamName) {
    return sources;
  }

  const matchingSources = sources.filter(
    (source) => source.teamName.trim().toLowerCase() === normalizedTeamName,
  );

  return matchingSources.length > 0 ? matchingSources : sources;
}

function normalizeHabitName(habitName: string): string {
  return habitName.trim().toLowerCase();
}

async function loadReminderSourcesByHabitName(
  auth: StoredAuth | null,
): Promise<ReminderSourcesByHabitName> {
  const teams = await getTeams(auth);
  const teamHabits = await Promise.all(
    teams.map(async (team) => {
      const [activeHabits, archivedHabits] = await Promise.all([
        getTeamHabits(auth, team.teamId, "Active"),
        getTeamHabits(auth, team.teamId, "Archived"),
      ]);
      const habits = [...activeHabits, ...archivedHabits];

      return Promise.all(
        habits.map(async (habit): Promise<ReminderSource> => {
          const todayStatus = await getMyTodayEntryStatus(
            auth,
            habit.habitId,
          ).catch(() => null);

          return {
            habitId: habit.habitId,
            habitName: habit.name,
            teamName: team.name,
            goal: habit.goal,
            isDoneToday:
              todayStatus !== null &&
              (todayStatus.status === "Logged" ||
                todayStatus.status === "Skipped" ||
                todayStatus.entry !== null),
          };
        }),
      );
    }),
  );

  return teamHabits.flat().reduce<ReminderSourcesByHabitName>(
    (sourcesByHabit, source) => {
      const habitNameKey = normalizeHabitName(source.habitName);
      const currentSources = sourcesByHabit[habitNameKey] ?? [];

      if (
        !currentSources.some(
          (currentSource) =>
            currentSource.teamName === source.teamName &&
            currentSource.habitId === source.habitId,
        )
      ) {
        sourcesByHabit[habitNameKey] = [
          ...currentSources,
          source,
        ];
      }

      return sourcesByHabit;
    },
    {},
  );
}

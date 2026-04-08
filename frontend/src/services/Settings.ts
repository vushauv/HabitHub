export type SecurityAction = {
  title: string;
  description: string;
  to: string;
};

export const securityActions: SecurityAction[] = [
  {
    title: "Change password",
    description:
      "Update your password to protect your account and keep access secure.",
    to: "/settings/change-password",
  },
  {
    title: "Change email",
    description:
      "Change the email address connected to your account and keep it up to date.",
    to: "/settings/change-email",
  },
  {
    title: "Active sessions",
    description:
      "Review logged in devices and invalidate sessions you do not recognize.",
    to: "/settings/sessions",
  },
];
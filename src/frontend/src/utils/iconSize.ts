export const ICON_SIZE = {
  sm: 16,
  base: 20,
  avatar: 48,
} as const;

export type IconSize = (typeof ICON_SIZE)[keyof typeof ICON_SIZE];

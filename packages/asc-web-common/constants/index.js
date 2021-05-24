export const LANGUAGE = "language";
export const ARTICLE_PINNED_KEY = "asc_article_pinned_key";

/**
 * Enum for employee activation status.
 * @readonly
 */
export const EmployeeActivationStatus = Object.freeze({
  NotActivated: 0,
  Activated: 1,
  Pending: 2,
  AutoGenerated: 4,
});

/**
 * Enum for employee status.
 * @readonly
 */
export const EmployeeStatus = Object.freeze({
  Active: 1,
  Disabled: 2,
});

/**
 * Enum for employee type.
 * @readonly
 */
export const EmployeeType = Object.freeze({
  User: 1,
  Guest: 2,
});

/**
 * Enum for filter type.
 * @readonly
 */
export const FilterType = Object.freeze({
  None: 0,
  FilesOnly: 1,
  FoldersOnly: 2,
  DocumentsOnly: 3,
  PresentationsOnly: 4,
  SpreadsheetsOnly: 5,
  ImagesOnly: 7,
  ByUser: 8,
  ByDepartment: 9,
  ArchiveOnly: 10,
  ByExtension: 11,
  MediaOnly: 12,
});

/**
 * Enum for file type.
 * @readonly
 */
export const FileType = Object.freeze({
  Unknown: 0,
  Archive: 1,
  Video: 2,
  Audio: 3,
  Image: 4,
  Spreadsheet: 5,
  Presentation: 6,
  Document: 7,
});

/**
 * Enum for file action.
 * @readonly
 */
export const FileAction = Object.freeze({
  Create: 0,
  Rename: 1,
});

/**
 * Enum for root folders type.
 * @readonly
 */
export const FolderType = Object.freeze({
  DEFAULT: 0,
  COMMON: 1,
  BUNCH: 2,
  TRASH: 3,
  USER: 5,
  SHARE: 6,
  Projects: 8,
  Favorites: 10,
  Recent: 11,
  Templates: 12,
  Privacy: 13,
});

export const ShareAccessRights = Object.freeze({
  None: 0,
  FullAccess: 1,
  ReadOnly: 2,
  DenyAccess: 3,
  Varies: 4,
  Review: 5,
  Comment: 6,
  FormFilling: 7,
  CustomFilter: 8,
});

export const ConflictResolveType = Object.freeze({
  Skip: 0,
  Overwrite: 1,
  Duplicate: 2,
});
export const providersData = Object.freeze({
  Google: {
    label: "Google",
    icon: "/static/images/share.google.react.svg",
  },
  Facebook: {
    label: "Facebook",
    icon: "/static/images/share.facebook.react.svg",
  },
  Twitter: {
    label: "Twitter",
    icon: "/static/images/share.twitter.react.svg",
    iconOptions: { color: "#2AA3EF" },
  },
  LinkedIn: {
    label: "LinkedIn",
    icon: "/static/images/share.linkedin.react.svg",
  },
});
export const i18nBaseSettings = {
  lng: localStorage.getItem(LANGUAGE) || "en",
  supportedLngs: ["en", "ru"],
  fallbackLng: "en",
  load: "languageOnly",

  interpolation: {
    escapeValue: false, // not needed for react as it escapes by default
    format: function (value, format) {
      if (format === "lowercase") return value.toLowerCase();
      return value;
    },
  },

  react: {
    useSuspense: true,
  },
};

export const LoaderStyle = {
  title: "",
  width: "100%",
  height: "32",
  backgroundColor: "#000000",
  foregroundColor: "#000000",
  backgroundOpacity: 0.1,
  foregroundOpacity: 0.15,
  borderRadius: "3",
  radius: "3",
  speed: 2,
  animate: true,
};

import config from "./AppServerConfig";

export const AppServerConfig = config;

/**
 * Enum for Tenant trusted domains on registration.
 * @readonly
 */
export const TenantTrustedDomainsType = Object.freeze({
  None: 0,
  Custom: 1,
  All: 2,
});

export const FilesFormats = Object.freeze({
  OriginalFormat: 0,
  TxtFormat: 1,
  DocxFormat: 2,
  OdtFormat: 3,
  OdsFormat: 4,
  OdpFormat: 5,
  PdfFormat: 6,
  RtfFormat: 7,
  XlsxFormat: 8,
  PptxFormat: 9,
  CustomFormat: 10,
});

export const PasswordLimitSpecialCharacters = "!@#$%^&*";

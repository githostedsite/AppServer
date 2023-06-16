export type Security = {
  Copy: boolean;
  CopyTo: boolean;
  Create: boolean;
  Delete: boolean;
  Duplicate: boolean;
  EditAccess: boolean;
  EditRoom: boolean;
  Move: boolean;
  MoveTo: boolean;
  Mute: boolean;
  Pin: boolean;
  Read: boolean;
  Rename: boolean;
};

export type Item = {
  id: number | string;
  parentId: number | string;
  rootFolderType: number | string;
  title: string;
  label: string;
  filesCount?: number;
  foldersCount?: number;
  avatar?: string;
  icon?: string;
  isFolder: boolean;
  isDisabled?: boolean;
  security: Security;
};

export type BreadCrumb = {
  label: string;
  id: number | string;
  isRoom: boolean;
};

type setItems = (value: Item[] | null) => Item[];

export type useLoadersHelperProps = {
  items: Item[] | null;
};

export type useRootHelperProps = {
  setBreadCrumbs: (items: BreadCrumb[]) => void;
  setIsBreadCrumbsLoading: (value: boolean) => void;
  setTotal: (value: number) => void;
  setItems: (items: Item[] | setItems) => void;
  treeFolders?: Item[];
};

export type useRoomsHelperProps = {
  setBreadCrumbs: (items: BreadCrumb[]) => void;
  setIsBreadCrumbsLoading: (value: boolean) => void;
  setIsNextPageLoading: (value: boolean) => void;
  setHasNextPage: (value: boolean) => void;
  setTotal: (value: number) => void;
  setItems: (items: Item[] | setItems) => void;
  isFirstLoad: boolean;
  setIsRoot: (value: boolean) => void;
  searchValue?: string;
};

export type useFilesHelpersProps = {
  setBreadCrumbs: (items: BreadCrumb[]) => void;
  setIsBreadCrumbsLoading: (value: boolean) => void;
  setIsNextPageLoading: (value: boolean) => void;
  setHasNextPage: (value: boolean) => void;
  setTotal: (value: number) => void;
  setItems: (items: Item[] | setItems) => void;
  isFirstLoad: boolean;
  selectedItemId: string | number | undefined;
  setIsRoot: (value: boolean) => void;
  searchValue?: string;
  disabledItems: string[] | number[];
  setSelectedItemSecurity: (value: Security) => void;
};

export type FilesSelectorProps = {
  currentFolderId?: number;
  parentId?: number;
  rootFolderType?: number;
  treeFolders?: Item[];
  isVisible: boolean;
  setMoveToPanelVisible: (value: boolean) => void;
  theme: any;
  selection: any[];
  disabledItems: string[] | number[];
  isFolderActions?: boolean;
  checkFileConflicts: (
    selectedItemId: string | number | undefined,
    folderIds: string[] | number[],
    fileIds: string[] | number[]
  ) => Promise<any>;
  setConflictDialogData: (conflicts: any, operationData: any) => void;
  itemOperationToFolder: (operationData: any) => Promise<void>;
  clearActiveOperations: (
    folderIds: string[] | number[],
    fileIds: string[] | number[]
  ) => void;
  setMovingInProgress: (value: boolean) => void;
  isCopy?: boolean;
  isRestoreAll?: boolean;
  setIsFolderActions: (value: boolean) => void;
  setCopyPanelVisible: (value: boolean) => void;
  setRestoreAllPanelVisible: (value: boolean) => void;
};

namespace ASC.Files.Core.Utils
{
    public static class RootFoldersHelper
    {
        public static string GetBunchKey(FolderType type, string data, string module = "files") =>
            type switch
            {
                FolderType.Custom => $"{module}/custom/{data}",
                FolderType.Privacy => $"{module}/privacy/{data}",
                FolderType.CustomPrivacy => $"{module}/customprivacy/{data}",
                FolderType.COMMON => $"{module}/common/{data}",
                FolderType.BUNCH => $"{module}/bunch/{data}",
                FolderType.TRASH => $"{module}/trash/{data}",
                FolderType.SHARE => $"{module}/share/{data}",
                FolderType.Projects => $"{module}/projects/{data}",
                FolderType.Favorites => $"{module}/favorites/{data}",
                FolderType.Recent => $"{module}/recent/{data}",
                FolderType.Templates => $"{module}/templates/{data}"
            };
    }
}

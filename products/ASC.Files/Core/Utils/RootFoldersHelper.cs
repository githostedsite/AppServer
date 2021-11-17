namespace ASC.Files.Core.Utils
{
    public static class RootFoldersHelper
    {
        public static string GetBunchKey(FolderType type, string data, string module = "files") =>
            type switch
            {
                FolderType.Custom => $"{module}/custom/{data}",
                FolderType.CustomPrivacy => $"{module}/customprivacy/{data}"
            };
    }
}

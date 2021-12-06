using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ASC.Files.Core.Utils
{
    public static class BunchFoldersHelper
    {
        public static string MakeBunchKey(FolderType type, string data, string module = "files") =>
            $"{module}/{Enum.GetName(type).ToLower()}/{data}";
    }
}

using System.Linq;
using System.Text.RegularExpressions;

namespace ASC.Files.Core.Helpers
{
    public class ThirdPartyHelper
    {
        public static string GetProvider(string rootFolderId)
        {
            return rootFolderId.Split('-').FirstOrDefault();
        }

        public static string GetFullProviderId(string rootFolderId)
        {
            return rootFolderId.Split("-|").FirstOrDefault();
        }

        public static string GetEntryId(string folderId)
        {
            return Regex.Replace(folderId, @"^\S+-\d+-\|", "", RegexOptions.Compiled);
        }

        public static string MakeFolderId(string entryId, string provider, int providerId)
        {
            return $"{provider}-{providerId}-|{entryId}";
        }
    }
}

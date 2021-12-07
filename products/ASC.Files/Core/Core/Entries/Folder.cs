/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Diagnostics;

using ASC.Common;

namespace ASC.Files.Core
{
    public enum FolderType
    {
        DEFAULT = 0,
        COMMON = 1,
        BUNCH = 2,
        TRASH = 3,
        USER = 5,
        SHARE = 6,
        Projects = 8,
        Favorites = 10,
        Recent = 11,
        Templates = 12,
        Privacy = 13,
        Custom = 14,
        CustomPrivacy = 15,
        Archive = 16,
    }

    public interface IFolder
    {
        public FolderType FolderType { get; set; }

        public int TotalFiles { get; set; }

        public int TotalSubFolders { get; set; }

        public bool Shareable { get; set; }

        public int NewForMe { get; set; }

        public string FolderUrl { get; set; }
    }

    [DebuggerDisplay("{Title} ({ID})")]
    [Transient]
    public class Folder<T> : FileEntry<T>, IFolder
    {
        public FolderType FolderType { get; set; }

        public int TotalFiles { get; set; }

        public int TotalSubFolders { get; set; }

        public bool Shareable { get; set; }

        public int NewForMe { get; set; }

        public string FolderUrl { get; set; }
        public string BunchKey { get; set; }

        public override bool IsNew
        {
            get { return Convert.ToBoolean(NewForMe); }
            set { NewForMe = Convert.ToInt32(value); }
        }

        public Folder()
        {
            Title = string.Empty;
            FileEntryType = FileEntryType.Folder;
        }

        public Folder(FileHelper fileHelper) : this()
        {
            FileHelper = fileHelper;
        }

        public override string UniqID
        {
            get { return $"folder_{ID}"; }
        }
    }
}
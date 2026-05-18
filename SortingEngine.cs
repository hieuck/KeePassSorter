using System;
using System.Collections.Generic;
using System.Globalization;
using KeePassLib;

namespace KeePassSorter
{
    public enum SortCriteria
    {
        Title = 0,
        UserName = 1,
        Url = 2,
        CreatedTime = 3,
        ModifiedTime = 4,
        Notes = 5
    }

    public class SortingOptions
    {
        public SortCriteria Criteria;
        public bool Ascending;
        public bool Recursive;
        public bool CaseSensitive;
        public bool UseVietnamese;

        public SortingOptions()
        {
            Criteria = SortCriteria.Title;
            Ascending = true;
            Recursive = true;
            CaseSensitive = false;
            UseVietnamese = false;
        }
    }

    public class SortingEngine
    {
        /// <summary>
        /// Sắp xếp entries trong một group. Trả về tổng số entries đã sắp xếp.
        /// </summary>
        public int SortGroup(PwGroup group, SortingOptions opts)
        {
            if (group == null) return 0;

            int count = SortEntriesInGroup(group, opts);

            if (opts.Recursive)
            {
                foreach (PwGroup sub in group.Groups)
                {
                    count += SortGroup(sub, opts);
                }
            }

            return count;
        }

        private int SortEntriesInGroup(PwGroup group, SortingOptions opts)
        {
            uint entryCount = group.Entries.UCount;
            if (entryCount < 2) return (int)entryCount;

            // Lấy tất cả entries ra list
            List<PwEntry> list = new List<PwEntry>();
            foreach (PwEntry pe in group.Entries)
            {
                list.Add(pe);
            }

            // Sắp xếp
            list.Sort(delegate(PwEntry a, PwEntry b)
            {
                return CompareEntries(a, b, opts);
            });

            // Xóa entries cũ và thêm lại theo thứ tự mới
            while (group.Entries.UCount > 0)
            {
                group.Entries.Remove(group.Entries.GetAt(0));
            }

            foreach (PwEntry pe in list)
            {
                group.AddEntry(pe, false);
            }

            return list.Count;
        }

        private int CompareEntries(PwEntry a, PwEntry b, SortingOptions opts)
        {
            string sa = GetValue(a, opts.Criteria);
            string sb = GetValue(b, opts.Criteria);

            int result;
            if (opts.Criteria == SortCriteria.CreatedTime || opts.Criteria == SortCriteria.ModifiedTime)
            {
                // So sánh thời gian
                result = string.Compare(sa, sb, StringComparison.Ordinal);
            }
            else if (opts.CaseSensitive)
            {
                result = string.Compare(sa, sb, StringComparison.Ordinal);
            }
            else if (opts.UseVietnamese)
            {
                try
                {
                    CultureInfo ci = new CultureInfo("vi-VN");
                    result = string.Compare(sa, sb, true, ci);
                }
                catch
                {
                    result = string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
                }
            }
            else
            {
                result = string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
            }

            if (!opts.Ascending) result = -result;
            return result;
        }

        private string GetValue(PwEntry entry, SortCriteria criteria)
        {
            if (entry == null) return string.Empty;

            switch (criteria)
            {
                case SortCriteria.Title:
                    return entry.Strings.ReadSafe(PwDefs.TitleField);
                case SortCriteria.UserName:
                    return entry.Strings.ReadSafe(PwDefs.UserNameField);
                case SortCriteria.Url:
                    return entry.Strings.ReadSafe(PwDefs.UrlField);
                case SortCriteria.CreatedTime:
                    return entry.CreationTime.ToString("yyyyMMddHHmmss");
                case SortCriteria.ModifiedTime:
                    return entry.LastModificationTime.ToString("yyyyMMddHHmmss");
                case SortCriteria.Notes:
                    return entry.Strings.ReadSafe(PwDefs.NotesField);
                default:
                    return entry.Strings.ReadSafe(PwDefs.TitleField);
            }
        }
    }
}

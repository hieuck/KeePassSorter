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
        /// Sắp xếp entries trong một group. Trả về số entries bị đổi vị trí.
        /// </summary>
        public int SortGroup(PwGroup group, SortingOptions opts)
        {
            if (group == null || opts == null) return 0;

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
            if (entryCount < 2) return 0;

            List<EntrySortItem> items = new List<EntrySortItem>();
            uint index = 0;
            foreach (PwEntry entry in group.Entries)
            {
                items.Add(new EntrySortItem(entry, index));
                ++index;
            }

            items.Sort(delegate(EntrySortItem a, EntrySortItem b)
            {
                int result = CompareEntries(a.Entry, b.Entry, opts);
                if (result != 0) return result;
                return a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            int changed = 0;
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i].OriginalIndex != i) ++changed;
            }

            if (changed == 0) return 0;

            while (group.Entries.UCount > 0)
            {
                group.Entries.Remove(group.Entries.GetAt(0));
            }

            foreach (EntrySortItem item in items)
            {
                group.AddEntry(item.Entry, false);
            }

            return changed;
        }

        private int CompareEntries(PwEntry a, PwEntry b, SortingOptions opts)
        {
            string sa = GetValue(a, opts.Criteria);
            string sb = GetValue(b, opts.Criteria);

            int result;
            if (opts.Criteria == SortCriteria.CreatedTime || opts.Criteria == SortCriteria.ModifiedTime)
            {
                // So sánh thời gian (luôn so sánh chuẩn theo chuỗi thời gian)
                result = string.Compare(sa, sb, StringComparison.Ordinal);
            }
            else
            {
                // So sánh tự nhiên (Natural Sort Order) cho các trường văn bản
                result = CompareNatural(sa, sb, opts);
            }

            if (!opts.Ascending) result = -result;
            return result;
        }

        /// <summary>
        /// Thuật toán so sánh tự nhiên (Natural Sort Order) giống Windows Explorer.
        /// Giúp so sánh chuỗi chứa số chính xác (ví dụ: a1b < a2b < a10b).
        /// </summary>
        private int CompareNatural(string x, string y, SortingOptions opts)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int ix = 0;
            int iy = 0;

            while (ix < x.Length && iy < y.Length)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    // Đọc chuỗi số liên tục từ x
                    int sx = ix;
                    while (ix < x.Length && char.IsDigit(x[ix])) { ix++; }
                    string numX = x.Substring(sx, ix - sx);

                    // Đọc chuỗi số liên tục từ y
                    int sy = iy;
                    while (iy < y.Length && char.IsDigit(y[iy])) { iy++; }
                    string numY = y.Substring(sy, iy - sy);

                    // Loại bỏ các chữ số 0 ở đầu để so sánh giá trị số học
                    string cleanX = numX.TrimStart('0');
                    string cleanY = numY.TrimStart('0');

                    // So sánh độ dài trước (chuỗi số dài hơn sẽ lớn hơn)
                    if (cleanX.Length != cleanY.Length)
                    {
                        return cleanX.Length.CompareTo(cleanY.Length);
                    }

                    // So sánh chuỗi số có cùng độ dài theo thứ tự ký tự
                    int cmpNum = string.Compare(cleanX, cleanY, StringComparison.Ordinal);
                    if (cmpNum != 0) return cmpNum;

                    // Nếu giá trị số bằng nhau, số nào có độ dài thực tế lớn hơn (do nhiều số 0 ở đầu) sẽ đứng trước
                    int cmpLen = numX.Length.CompareTo(numY.Length);
                    if (cmpLen != 0) return cmpLen;
                }
                else if (char.IsDigit(x[ix]) && IsNaturalSuffixBoundary(y[iy]))
                {
                    return 1;
                }
                else if (IsNaturalSuffixBoundary(x[ix]) && char.IsDigit(y[iy]))
                {
                    return -1;
                }
                else if (x[ix] == '@' && IsEmailAliasBoundary(y[iy]))
                {
                    return -1;
                }
                else if (IsEmailAliasBoundary(x[ix]) && y[iy] == '@')
                {
                    return 1;
                }
                else
                {
                    // So sánh các ký tự không phải số theo cấu hình
                    string cx = x[ix].ToString();
                    string cy = y[iy].ToString();
                    int cmpChar;

                    if (opts.UseVietnamese)
                    {
                        try
                        {
                            CultureInfo ci = new CultureInfo("vi-VN");
                            cmpChar = string.Compare(cx, cy, !opts.CaseSensitive, ci);
                        }
                        catch
                        {
                            cmpChar = string.Compare(cx, cy, opts.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    else if (opts.CaseSensitive)
                    {
                        cmpChar = string.Compare(cx, cy, StringComparison.Ordinal);
                    }
                    else
                    {
                        cmpChar = string.Compare(cx, cy, StringComparison.OrdinalIgnoreCase);
                    }

                    if (cmpChar != 0) return cmpChar;
                    ix++;
                    iy++;
                }
            }

            return x.Length.CompareTo(y.Length);
        }

        private static bool IsNaturalSuffixBoundary(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private static bool IsEmailAliasBoundary(char c)
        {
            return c == '.' || c == '_' || c == '-' || c == '+';
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

        private sealed class EntrySortItem
        {
            public readonly PwEntry Entry;
            public readonly uint OriginalIndex;

            public EntrySortItem(PwEntry entry, uint originalIndex)
            {
                Entry = entry;
                OriginalIndex = originalIndex;
            }
        }
    }
}

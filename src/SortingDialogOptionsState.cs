namespace KeePassSorter
{
    /// <summary>
    /// Lưu trữ tùy chọn sắp xếp cuối cùng được dùng trong SortingDialog.
    /// Dùng static state để dialog mở lại sẽ khôi phục đúng lựa chọn trước đó.
    /// </summary>
    public static class SortingDialogOptionsState
    {
        private static SortingOptions s_lastOptions;

        public static SortingOptions GetInitialOptions()
        {
            if (s_lastOptions != null)
                return CloneOptions(s_lastOptions);

            return new SortingOptions();
        }

        public static void SaveOptions(SortingOptions options)
        {
            if (options != null)
                s_lastOptions = CloneOptions(options);
        }

        public static void Reset()
        {
            s_lastOptions = null;
        }

        private static SortingOptions CloneOptions(SortingOptions options)
        {
            return new SortingOptions
            {
                Criteria = options.Criteria,
                Ascending = options.Ascending,
                Recursive = options.Recursive,
                CaseSensitive = options.CaseSensitive,
                UseVietnamese = options.UseVietnamese
            };
        }
    }
}

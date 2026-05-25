using System;
using KeePassLib;
using KeePassLib.Security;

namespace KeePassSorter.Tests
{
    internal static class SortingEngineBehaviorTests
    {
        private static int Main()
        {
            AlreadySortedGroupReportsNoChanges();
            UnsortedGroupReportsChangesAndReordersEntries();
            return 0;
        }

        private static void AlreadySortedGroupReportsNoChanges()
        {
            PwGroup group = CreateGroup("A", "B");
            SortingEngine engine = new SortingEngine();

            int changed = engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false });

            AssertEqual(0, changed, "already sorted group should report no changed entries");
            AssertTitles(group, "A", "B");
        }

        private static void UnsortedGroupReportsChangesAndReordersEntries()
        {
            PwGroup group = CreateGroup("B", "A");
            SortingEngine engine = new SortingEngine();

            int changed = engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false });

            AssertEqual(2, changed, "unsorted two-entry group should report two changed entries");
            AssertTitles(group, "A", "B");
        }

        private static PwGroup CreateGroup(params string[] titles)
        {
            PwGroup group = new PwGroup(true, true);
            foreach (string title in titles)
            {
                PwEntry entry = new PwEntry(true, true);
                entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, title));
                group.AddEntry(entry, false);
            }
            return group;
        }

        private static void AssertTitles(PwGroup group, params string[] expected)
        {
            AssertEqual(expected.Length, (int)group.Entries.UCount, "entry count mismatch");
            for (uint i = 0; i < expected.Length; ++i)
            {
                string title = group.Entries.GetAt(i).Strings.ReadSafe(PwDefs.TitleField);
                AssertEqual(expected[i], title, "entry title mismatch at index " + i);
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!object.Equals(expected, actual))
                throw new Exception(message + ". Expected: " + expected + ", actual: " + actual);
        }
    }
}

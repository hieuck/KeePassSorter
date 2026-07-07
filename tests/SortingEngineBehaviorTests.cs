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
            NaturalSortPlacesPlainEmailBeforeNumberedVariants();
            NaturalSortPlacesBaseEmailBeforeDottedAliases();
            VietnameseSortOrdersDiacriticsCorrectly();
            DescendingOrderReversesSort();
            RecursiveSortSortsChildGroups();
            UpdateCheckerDetectsNewerSemanticVersions();
            UpdateCheckerIgnoresSameOrInvalidVersions();
            UpdateCheckerSelectsNewestSemanticTag();
            UpdateCheckerTreatsRevisionZeroAsSameVersion();
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

        private static void NaturalSortPlacesPlainEmailBeforeNumberedVariants()
        {
            PwGroup group = CreateGroup(
                "user.photos1@example.com",
                "user.photos2@example.com",
                "user.photos@example.com");
            SortingEngine engine = new SortingEngine();

            engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false });

            AssertTitles(
                group,
                "user.photos@example.com",
                "user.photos1@example.com",
                "user.photos2@example.com");
        }

        private static void NaturalSortPlacesBaseEmailBeforeDottedAliases()
        {
            PwGroup group = CreateGroup(
                "account.photos@example.com",
                "account.photos2@example.com",
                "account.photos3@example.com",
                "account.photos4@example.com",
                "account@example.com");
            SortingEngine engine = new SortingEngine();

            engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false });

            AssertTitles(
                group,
                "account@example.com",
                "account.photos@example.com",
                "account.photos2@example.com",
                "account.photos3@example.com",
                "account.photos4@example.com");
        }

        private static void VietnameseSortOrdersDiacriticsCorrectly()
        {
            PwGroup group = CreateGroup("đ", "a", "ă", "â", "b");
            SortingEngine engine = new SortingEngine();

            engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false, UseVietnamese = true });

            AssertTitles(group, "a", "ă", "â", "b", "đ");
        }

        private static void DescendingOrderReversesSort()
        {
            PwGroup group = CreateGroup("A", "C", "B");
            SortingEngine engine = new SortingEngine();

            engine.SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = false, Recursive = false });

            AssertTitles(group, "C", "B", "A");
        }

        private static void RecursiveSortSortsChildGroups()
        {
            PwGroup parent = new PwGroup(true, true);
            PwGroup child = new PwGroup(true, true);
            parent.AddGroup(child, true);

            PwEntry entryB = new PwEntry(true, true);
            entryB.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "B"));
            child.AddEntry(entryB, false);

            PwEntry entryA = new PwEntry(true, true);
            entryA.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "A"));
            child.AddEntry(entryA, false);

            SortingEngine engine = new SortingEngine();
            int changed = engine.SortGroup(parent, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = true });

            AssertEqual(2, changed, "recursive sort should report changes in child group");
            AssertTitles(child, "A", "B");
        }

        private static void UpdateCheckerDetectsNewerSemanticVersions()
        {
            AssertEqual(true, UpdateChecker.IsNewerVersion("1.0.1", "v1.0.2"), "patch update should be detected");
            AssertEqual(true, UpdateChecker.IsNewerVersion("1.0.1", "v1.1.0"), "minor update should be detected");
        }

        private static void UpdateCheckerIgnoresSameOrInvalidVersions()
        {
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1", "1.0.1"), "same version should not be detected as update");
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1", "latest"), "non-version tag should not be detected as update");
        }

        private static void UpdateCheckerSelectsNewestSemanticTag()
        {
            string tag = UpdateChecker.GetNewestVersionTag(new string[] { "latest", "v1.0.1", "v1.1.0", "draft" });
            AssertEqual("v1.1.0", tag, "newest semantic release tag should be selected");
        }

        private static void UpdateCheckerTreatsRevisionZeroAsSameVersion()
        {
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1", "1.0.1.0"), "revision zero should not be treated as newer");
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1.0", "1.0.1"), "missing revision should not be treated as older");
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

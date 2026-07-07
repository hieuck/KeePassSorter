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
            UpdateCheckerSelectsNewestTagWithoutRevisionZeroBias();
            UpdateCheckerTreatsRevisionZeroAsSameVersion();
            SortByUsernameOrdersByUserNameField();
            SortByUrlOrdersByUrlField();
            SortByNotesOrdersByNotesField();
            SortByCreationTimeOrdersOldestFirst();
            SortByModificationTimeOrdersOldestFirst();
            CaseSensitiveSortOrdersUppercaseBeforeLowercase();
            VietnameseCaseSensitiveSortPreservesVietnameseOrder();
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

        private static void UpdateCheckerSelectsNewestTagWithoutRevisionZeroBias()
        {
            string tag = UpdateChecker.GetNewestVersionTag(new string[] { "v1.0.1", "v1.0.1.0" });
            AssertEqual("v1.0.1", tag, "revision zero should not bias newest tag selection");
        }

        private static void UpdateCheckerTreatsRevisionZeroAsSameVersion()
        {
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1", "1.0.1.0"), "revision zero should not be treated as newer");
            AssertEqual(false, UpdateChecker.IsNewerVersion("1.0.1.0", "1.0.1"), "missing revision should not be treated as older");
        }

        private static void SortByUsernameOrdersByUserNameField()
        {
            PwGroup group = new PwGroup(true, true);
            AddEntry(group, "T", "B", "url", "note");
            AddEntry(group, "T", "A", "url", "note");

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.UserName, Ascending = true, Recursive = false });

            AssertUsernames(group, "A", "B");
        }

        private static void SortByUrlOrdersByUrlField()
        {
            PwGroup group = new PwGroup(true, true);
            AddEntry(group, "T", "user", "b.com", "note");
            AddEntry(group, "T", "user", "a.com", "note");

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.Url, Ascending = true, Recursive = false });

            AssertUrls(group, "a.com", "b.com");
        }

        private static void SortByNotesOrdersByNotesField()
        {
            PwGroup group = new PwGroup(true, true);
            AddEntry(group, "T", "user", "url", "B");
            AddEntry(group, "T", "user", "url", "A");

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.Notes, Ascending = true, Recursive = false });

            AssertNotes(group, "A", "B");
        }

        private static void SortByCreationTimeOrdersOldestFirst()
        {
            PwGroup group = new PwGroup(true, true);
            PwEntry older = new PwEntry(true, true);
            older.CreationTime = new DateTime(2024, 1, 1);
            older.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "Second"));
            group.AddEntry(older, false);

            PwEntry newer = new PwEntry(true, true);
            newer.CreationTime = new DateTime(2024, 1, 2);
            newer.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "First"));
            group.AddEntry(newer, false);

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.CreatedTime, Ascending = true, Recursive = false });

            AssertTitles(group, "Second", "First");
        }

        private static void SortByModificationTimeOrdersOldestFirst()
        {
            PwGroup group = new PwGroup(true, true);
            PwEntry older = new PwEntry(true, true);
            older.LastModificationTime = new DateTime(2024, 1, 1);
            older.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "Second"));
            group.AddEntry(older, false);

            PwEntry newer = new PwEntry(true, true);
            newer.LastModificationTime = new DateTime(2024, 1, 2);
            newer.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "First"));
            group.AddEntry(newer, false);

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.ModifiedTime, Ascending = true, Recursive = false });

            AssertTitles(group, "Second", "First");
        }

        private static void CaseSensitiveSortOrdersUppercaseBeforeLowercase()
        {
            PwGroup group = CreateGroup("a", "B");

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false, CaseSensitive = true });

            AssertTitles(group, "B", "a");
        }

        private static void VietnameseCaseSensitiveSortPreservesVietnameseOrder()
        {
            PwGroup group = CreateGroup("B", "Â", "A");

            new SortingEngine().SortGroup(group, new SortingOptions { Criteria = SortCriteria.Title, Ascending = true, Recursive = false, UseVietnamese = true, CaseSensitive = true });

            AssertTitles(group, "A", "Â", "B");
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

        private static void AddEntry(PwGroup group, string title, string username, string url, string notes)
        {
            PwEntry entry = new PwEntry(true, true);
            entry.Strings.Set(PwDefs.TitleField, new ProtectedString(false, title));
            entry.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, username));
            entry.Strings.Set(PwDefs.UrlField, new ProtectedString(false, url));
            entry.Strings.Set(PwDefs.NotesField, new ProtectedString(false, notes));
            group.AddEntry(entry, false);
        }

        private static void AssertTitles(PwGroup group, params string[] expected)
        {
            AssertField(group, expected, PwDefs.TitleField, "entry title mismatch at index ");
        }

        private static void AssertUsernames(PwGroup group, params string[] expected)
        {
            AssertField(group, expected, PwDefs.UserNameField, "entry username mismatch at index ");
        }

        private static void AssertUrls(PwGroup group, params string[] expected)
        {
            AssertField(group, expected, PwDefs.UrlField, "entry url mismatch at index ");
        }

        private static void AssertNotes(PwGroup group, params string[] expected)
        {
            AssertField(group, expected, PwDefs.NotesField, "entry notes mismatch at index ");
        }

        private static void AssertField(PwGroup group, string[] expected, string fieldId, string messagePrefix)
        {
            AssertEqual(expected.Length, (int)group.Entries.UCount, "entry count mismatch");
            for (uint i = 0; i < expected.Length; ++i)
            {
                string value = group.Entries.GetAt(i).Strings.ReadSafe(fieldId);
                AssertEqual(expected[i], value, messagePrefix + i);
            }
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!object.Equals(expected, actual))
                throw new Exception(message + ". Expected: " + expected + ", actual: " + actual);
        }
    }
}

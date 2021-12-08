namespace Jeskei.AccessPoint.Modules.Ingest
{
    using System;
    using System.Linq;
    using Jeskei.AccessPoint.Core;
    using System.Collections.Generic;

    public class FileEntryManager
    {
        #region private fields

        private List<FileEntry> currentState;

        #endregion

        #region constructors

        public FileEntryManager()
        {
            this.currentState = new List<FileEntry>();
        }

        public FileEntryManager(List<FileEntry> initialState)
        {
            Guard.NotNull(initialState, nameof(initialState));

            ComputeIdentifiers(initialState);

            this.currentState = initialState;
        }

        #endregion

        #region properties

        public List<FileEntry> CurrentState { get { return this.currentState; } }

        #endregion

        #region public methods

        public List<FileEntryChange> ProcessChangeset(List<FileEntry> fileState)
        {
            Guard.NotNull(fileState, nameof(fileState));

            ComputeIdentifiers(fileState);

            var additions = fileState
                .Except(this.currentState, new FileEntryByHashIdEqualityComparer())
                .Select(a => FileEntryChange.FromFileEntry(FileEntryChangeType.Add, a))
                .ToList();

            var removals = this.currentState
                .Except(fileState, new FileEntryByHashIdEqualityComparer())
                .Select(a => FileEntryChange.FromFileEntry(FileEntryChangeType.Remove, a))
                .ToList();

            var updates = fileState
                .Except(this.CurrentState, new FileEntryByPropertiesEqualityComparer())
                .Select(a => FileEntryChange.FromFileEntry(FileEntryChangeType.Update, a))
                .ToList();

            var changes = additions
                .Union(removals)
                .ToList();

            // only add updates if not already a change(addition)
            updates.ForEach(a => { if (!(changes.Any(c => c.HashId == a.HashId))) { changes.Add(a); } });

            this.currentState = fileState;

            return changes;
        }

        #endregion
        
        #region private methods

        private static void ComputeIdentifiers(List<FileEntry> fileEntries)
        {
            fileEntries
                .ForEach(a => a.HashId = HashHelper.MD5FromString(a.FullPath.ToLower()));
        }

        #endregion
    }
}

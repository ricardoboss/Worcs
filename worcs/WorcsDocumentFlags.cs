using System;

namespace worcs
{
    [Flags]
    public enum WorcsDocumentFlags : uint
    {
        RepositoryExists = 1,
        WorkingDirExists = 2,
        HasUncommitedChanges = 4
    }
}
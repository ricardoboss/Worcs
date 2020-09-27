using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace worcs
{
    public static class RepoManager
    {
        public enum InitResult
        {
            Unspecified = 0,
            Success,
            TempDirNotFound,
            RepoAlreadyExists,
        }

        public static async Task<InitResult> Initialize(WorcsDocument document)
        {
            if (!document.HasFlag(WorcsDocumentFlags.WorkingDirExists))
                return InitResult.TempDirNotFound;

            if (document.HasFlag(WorcsDocumentFlags.RepositoryExists))
                return InitResult.RepoAlreadyExists;

            Repository.Init(document.RepoDir);
            document.SetFlag(WorcsDocumentFlags.RepositoryExists);
            document.SetFlag(WorcsDocumentFlags.HasUncommitedChanges);

            return InitResult.Success;
        }

        public enum CommitResult
        {
            Unspecified = 0,
            Success,
            RepositoryNotFound,
            NoChanges,
        }

        public static async Task<CommitResult> CommitAllChanged(WorcsDocument document, Signature author, string summary)
        {
            if (!document.HasFlag(WorcsDocumentFlags.RepositoryExists))
                return CommitResult.RepositoryNotFound;

            if (!document.HasFlag(WorcsDocumentFlags.HasUncommitedChanges))
                return CommitResult.NoChanges;

            var timestampFile = Path.Combine(document.RepoDir, "changed.tmp");
            await File.WriteAllTextAsync(timestampFile, DateTimeOffset.UtcNow.ToString());

            using var repo = new Repository(document.RepoDir);
            Commands.Stage(repo, "*");

            _ = repo.Commit(summary, author, author);

            document.RemoveFlag(WorcsDocumentFlags.HasUncommitedChanges);

            return CommitResult.Success;
        }
    }
}
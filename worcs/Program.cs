using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace worcs
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                await Console.Error.WriteLineAsync("At least one argument is required. Try \"help\" to begin.");

                return;
            }

            switch (args[0])
            {
                case "p":
                case "prep":
                case "prepare":
                    await Prepare(args[1..]);

                    break;

                case "help":
                    await Help(args[1..]);

                    break;

                default:
                    await Console.Error.WriteLineAsync($"Unknown command: {args[0]}");

                    break;
            }
        }

        private static async Task Prepare(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
            {
                await Console.Error.WriteLineAsync("A word filename is required.");

                return;
            }

            var document = new WorcsDocument
            {
                WordFilepath = args[0],
            };

            await Console.Out.WriteLineAsync("Unpacking word file...");

            var unpackResult = await ZipManager.Unpack(document);
            switch (unpackResult)
            {
                case ZipManager.UnpackResult.Success:
                    break;
                case ZipManager.UnpackResult.FileNotFound:
                    await Console.Error.WriteLineAsync($"Could not unpack word file. File not found: {document.WordFilepath}");
                    return;
                case ZipManager.UnpackResult.Unspecified:
                default:
                    await Console.Error.WriteLineAsync("Could not unpack word file.");
                    return;
            }

            await Console.Out.WriteLineAsync("Creating repository...");

            var repoInitResult = await RepoManager.Initialize(document);
            switch (repoInitResult)
            {
                case RepoManager.InitResult.Success:
                    break;
                case RepoManager.InitResult.TempDirNotFound:
                    await Console.Error.WriteLineAsync("Working directory could not be found.");
                    return;
                case RepoManager.InitResult.RepoAlreadyExists:
                    await Console.Error.WriteLineAsync("Repository already exists.");
                    return;
                case RepoManager.InitResult.Unspecified:
                default:
                    await Console.Error.WriteLineAsync("Could not create repository.");
                    return;
            }

            await Console.Out.WriteLineAsync("Creating initial commit...");

            await Console.Out.WriteAsync("Your name: ");
            var name = await Console.In.ReadLineAsync();

            await Console.Out.WriteAsync("Your email: ");
            var email = await Console.In.ReadLineAsync();

            var signature = new Signature(name, email, DateTimeOffset.Now);
            var repoCommitResult = await RepoManager.CommitAllChanged(document, signature, "Initial commit");
            switch (repoCommitResult)
            {
                case RepoManager.CommitResult.Success:
                    break;
                case RepoManager.CommitResult.RepositoryNotFound:
                    await Console.Error.WriteLineAsync("Repository not found.");
                    return;
                case RepoManager.CommitResult.NoChanges:
                    await Console.Error.WriteLineAsync("No files were committed.");
                    return;
                case RepoManager.CommitResult.Unspecified:
                default:
                    await Console.Error.WriteLineAsync("Could not create initial commit.");
                    return;
            }

            await Console.Out.WriteLineAsync("Repackaging everything into original word file...");

            var packResult = await ZipManager.Pack(document);
            switch (packResult)
            {
                case ZipManager.PackResult.Success:
                    break;
                case ZipManager.PackResult.WorkingDirNotFound:
                    await Console.Error.WriteLineAsync("Working directory not found.");
                    return;
                case ZipManager.PackResult.Unspecified:
                default:
                    await Console.Error.WriteLineAsync("Could not pack document.");
                    return;
            }

            await Console.Out.WriteLineAsync($"File at {document.WordFilepath} successfully prepared!");
        }

        private static async Task Help(string[] args)
        {
            await Console.Out.WriteLineAsync($"Worcs - Word (Version) Control System v{Assembly.GetExecutingAssembly().GetName().Version}");
        }
    }
}
/* =============================================
 * Copyright 2017 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only.
 * FileName: ConcurrencyUtilities.cs
 * Purpose:  use a common locking mechanism (filestream) for all platforms to do single instance check
 *           Copy from https://github.com/NuGet/NuGet.Client/blob/6bd6cecdfedb8efcfa6b05c3694f05d6898006a1/src/NuGet.Core/NuGet.Common/ConcurrencyUtilities.cs . 
 *           See more: https://github.com/NuGet/NuGet.Client/pull/725/files#r69220965
 *                     http://aakinshin.net/blog/post/namedmutex-on-mono/
 * Author:   ElizabethHe copied on Jun. 26th, 2017
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TVU.SharedLib.GenericUtility
{
    public static class ConcurrencyUtilities
    {
        /// <summary>
        /// {0}: file path.
        /// {1}: error message;
        /// {2}: file path.
        /// </summary>
        private const string UnauthorizedAccess = "Failed to read '{0}' due to: '{1}'. Path: '{2}'.";
        /// <summary>
        /// {0} and {1} are both file paths.
        /// </summary>
        private const string UnauthorizedLockFail = "Unable to obtain lock file access on '{0}' for operations on '{1}'. This may mean that a different user or administator is holding this lock and that this process does not have permission to access it. If no other process is currently performing an operation on this file it may mean that an earlier NuGet process crashed and left an inaccessible lock file, in this case removing the file '{0}' will allow Receiver to continue.";

        private const int NumberOfRetries = 100;
        private static readonly TimeSpan SleepDuration = TimeSpan.FromMilliseconds(100);

        public async static Task<T> ExecuteWithFileLockedAsync<T>(string filePath, Func<CancellationToken, Task<T>> action, CancellationToken token)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            // limit the number of unauthorized, this should be around 30 seconds.
            var unauthorizedAttemptsLeft = NumberOfRetries;

            var lockPath = filePath;

            while (true)
            {
                FileStream fs = null;

                try
                {
                    try
                    {
                        fs = AcquireFileStream(lockPath);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        throw;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        token.ThrowIfCancellationRequested();

                        if (unauthorizedAttemptsLeft < 1)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, UnauthorizedLockFail, lockPath, filePath);

                            throw new InvalidOperationException(message);
                        }

                        unauthorizedAttemptsLeft--;

                        // This can occur when the file is being deleted
                        // Or when an admin user has locked the file
                        await Task.Delay(SleepDuration);
                        continue;
                    }
                    catch (IOException)
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(SleepDuration);
                        continue;
                    }

                    // Run the action within the lock
                    return await action(token);
                }
                finally
                {
                    if (fs != null)
                    {
                        // Dispose of the stream, this will cause a delete
                        fs.Dispose();
                    }
                }
            }
        }

        public static void ExecuteWithFileLocked(string filePath, Action action)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            // limit the number of unauthorized, this should be around 30 seconds.
            var unauthorizedAttemptsLeft = NumberOfRetries;

            var lockPath = filePath;

            while (true)
            {
                FileStream fs = null;
                try
                {
                    try
                    {
                        fs = AcquireFileStream(lockPath);
                        fs?.Lock(0, 0); // 0,0 has special meaning to lock entire file regardless of length, see more: https://stackoverflow.com/a/35463999/6091869
                    }
                    catch (DirectoryNotFoundException)
                    {
                        throw;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine(string.Format(CultureInfo.CurrentCulture, UnauthorizedAccess, lockPath, ex.Message, lockPath));
                        if (unauthorizedAttemptsLeft < 1)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, UnauthorizedLockFail, lockPath, filePath);

                            throw new InvalidOperationException(message);
                        }

                        unauthorizedAttemptsLeft--;

                        // This can occur when the file is being deleted
                        // Or when an admin user has locked the file
                        Thread.Sleep(SleepDuration);
                        continue;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(string.Format(CultureInfo.CurrentCulture, UnauthorizedAccess, lockPath, ex.Message, lockPath));
                        if (unauthorizedAttemptsLeft < 1)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, UnauthorizedLockFail, lockPath, filePath);

                            if (!SystemConfigurationInfo.IsWindows)
                                Console.WriteLine(message);
                            throw new InvalidOperationException(message);
                        }

                        unauthorizedAttemptsLeft--;
                        Thread.Sleep(SleepDuration);
                        continue;
                    }

                    // Run the action within the lock
                    action();
                    return;
                }
                finally
                {
                    // Dispose of the stream, this will cause a delete
                    fs?.Dispose();
                }
            }
        }

        private static FileStream AcquireFileStream(string lockPath)
        {
            FileOptions options;
            if (SystemConfigurationInfo.IsWindows)
            {
                // This file is deleted when the stream is closed.
                options = FileOptions.DeleteOnClose;
            }
            else
            {
                // FileOptions.DeleteOnClose causes concurrency issues on Mac OS X and Linux.
                options = FileOptions.None;
            }

            // Sync operations have shown much better performance than FileOptions.Asynchronous
            return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 32, options: options);
        }

        private static string FilePathToLockName(string filePath)
        {
            // If we use a file path directly as the name of a semaphore,
            // the ctor of semaphore looks for the file and throws an IOException
            // when the file doesn't exist. So we need a conversion from a file path
            // to a unique lock name.
            using (var sha = SHA1.Create())
            {
                // To avoid conflicts on package id casing a case-insensitive lock is used.
                var fullPath = Path.IsPathRooted(filePath) ? Path.GetFullPath(filePath) : filePath;
                var normalizedPath = fullPath.ToUpperInvariant();

                var hash = sha.ComputeHash(Encoding.UTF32.GetBytes(normalizedPath));

                return ToHex(hash);
            }
        }

        private static string ToHex(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            for (int index = 0, outIndex = 0; index < bytes.Length; index++)
            {
                c[outIndex++] = ToHexChar(bytes[index] >> 4);
                c[outIndex++] = ToHexChar(bytes[index] & 0x0f);
            }

            return new string(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ToHexChar(int input)
        {
            if (input > 9)
            {
                return (char)(input + 0x57);
            }
            else
            {
                return (char)(input + 0x30);
            }
        }
    }
}

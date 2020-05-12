using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace QuickBrake {
    public static class Extensions {

        public static bool IsNullOrEmpty(this string String) => string.IsNullOrEmpty(String);

        public static string GetStringOrString(this string StringA, string StringB) => StringA.IsNullOrEmpty() ? StringB : StringA;

        public static bool TryGetFile(this string FileName, out FileInfo File) {
#pragma warning disable CA1031 // Do not catch general exception types
            try {
                File = new FileInfo(FileName);
                return true;
            } catch (ArgumentNullException) { //FileName is null.
                Debug.WriteLine("FileName is null.", "ArgumentNullException");

            } catch (SecurityException) { //The caller does not have the required permission.
                Debug.WriteLine("The caller does not have the required permission.", "SecurityException");

            } catch (ArgumentException) { //The file name is empty, contains only white spaces, or contains invalid characters.
                Debug.WriteLine("The file name is empty, contains only white spaces, or contains invalid characters.", "ArgumentException");

            } catch (UnauthorizedAccessException) { //Access to FileName is denied.
                Debug.WriteLine("Access to FileName is denied.", "UnauthorizedAccessException");

            } catch (PathTooLongException) { //The specified path, file name, or both exceed the system-defined maximum length.
                Debug.WriteLine("The specified path, file name, or both exceed the system-defined maximum length.", "PathTooLongException");
            } catch (NotSupportedException) { //FileName contains a colon (:) in the middle of the string.
                Debug.WriteLine("FileName contains a colon (:) in the middle of the string.", "NotSupportedException");
            }

            File = null;
            return false;
#pragma warning restore CA1031 // Do not catch general exception types
        }

        public static Task WaitForExitAsync(this Process Process,
            CancellationToken CancellationToken = default) {
            TaskCompletionSource<object> Tcs = new TaskCompletionSource<object>();
            Process.EnableRaisingEvents = true;
            Process.Exited += (Sender, Args) => Tcs.TrySetResult(null);
            if (CancellationToken != default) {
                CancellationToken.Register(Tcs.SetCanceled);
            }

            return Tcs.Task;
        }
    }
}

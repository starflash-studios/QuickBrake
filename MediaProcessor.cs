using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QuickBrake {
    public struct MediaProcessor : IEquatable<MediaProcessor> {
        public static FileInfo HandBrakeCLI;
        public FileInfo CurrentFile;
        public FileInfo DestinationFile;
        public Encoder PreferredEncoder;

        public MediaProcessor(FileInfo CurrentFile, FileInfo DestinationFile = default, Encoder PreferredEncoder = default) {
            this.CurrentFile = CurrentFile;
            string CurrentFileName = CurrentFile.FullName;

            this.DestinationFile = DestinationFile ?? new FileInfo(CurrentFileName.Substring(0, CurrentFileName.Length - CurrentFile.Extension.Length) + "-2" + CurrentFile.Extension);

            this.PreferredEncoder = PreferredEncoder;
        }

        public Process GetProcess(ProcessStartInfo StartInfo = null) {
            if (DestinationFile.Exists) {
                Debug.WriteLine("Please pass a file that doesn't already exist!", "Warning");
                DestinationFile.Delete();
            }

            return new Process {
                StartInfo = StartInfo ?? new ProcessStartInfo {
                    FileName = HandBrakeCLI.FullName, Arguments = GetArgString()
                }
            };
        }

        public string GetArgString() => GetArgs().Aggregate("", (Current, Arg) => Current + " " + Arg).Trim(' ');

        public IEnumerable<string> GetArgs() {
            switch(PreferredEncoder) {
                case Encoder.x264:
                case Encoder.x264_10bit:
                case Encoder.x265:
                case Encoder.x265_10bit:
                case Encoder.MPEG2:
                case Encoder.MPEG4:
                case Encoder.VP8:
                case Encoder.VP9:
                case Encoder.Theora:
                    yield return "-e " + PreferredEncoder;
                    break;
                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    break;
            }

            yield return "--crop 0:0:0:0"; //TODO Add custom cropping

            yield return "-i \"" + CurrentFile.FullName + "\"";

            yield return "-o \"" + DestinationFile.FullName + "\"";
        }

        #region Generic Overrides
        public override string ToString() => CurrentFile.FullName;

        public string ToLogString() => $"[{CurrentFile.FullName} => {DestinationFile.FullName} (Using: {PreferredEncoder})]";

        public override bool Equals(object Obj) => Obj is MediaProcessor Processor && Equals(Processor);

        public override int GetHashCode() {
            unchecked {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                int HashCode = (CurrentFile != null ? CurrentFile.GetHashCode() : 0);
                HashCode = (HashCode * 397) ^ (DestinationFile != null ? DestinationFile.GetHashCode() : 0);
                HashCode = (HashCode * 397) ^ (int)PreferredEncoder;
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return HashCode;
            }
        }

        public bool Equals(MediaProcessor Other) =>
            EqualityComparer<FileInfo>.Default.Equals(CurrentFile, Other.CurrentFile) &&
            EqualityComparer<FileInfo>.Default.Equals(DestinationFile, Other.DestinationFile) &&
            PreferredEncoder == Other.PreferredEncoder;

        public static bool operator ==(MediaProcessor Left, MediaProcessor Right) => Left.Equals(Right);

        public static bool operator !=(MediaProcessor Left, MediaProcessor Right) => !(Left == Right);
        #endregion
    }

    // ReSharper disable InconsistentNaming
    public enum Encoder {
        Auto = 0,
        x264,
        x264_10bit,
        x265,
        x265_10bit,
        MPEG2,
        MPEG4,
        VP8,
        VP9,
        Theora
    }
    // ReSharper restore InconsistentNaming
}

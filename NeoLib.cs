//Created by Starflash Studios, 2017-19
//This work is licensed under a Creative Commons Attribution-NonCommercial 4.0 International License.
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Windows.Forms;

public static class NeoLib {

#if UNITY_WEBPLAYER
     public static string webplayerQuitURL = "https://google.com";
#endif

    /// <summary>
    /// Shorthand for string.IsNullOrEmpty(value);
    /// </summary>
    /// <param name="value">The string in question</param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string value) {
        return string.IsNullOrEmpty(value);
    }

	public static bool IsElevated {
		get { return WindowsIdentity.GetCurrent().Owner .IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid); }
	}

	public static void Wait(int time) {
		System.Threading.Thread.Sleep(time);
	}


	/// <summary>
	/// Shorthand for string.IsNullOrEmpty(value);
	/// </summary>
	/// <param name="value">The string in question</param>
	/// <returns></returns>
	public static bool IsEmptyOrNull(this string value) {
		return string.IsNullOrEmpty(value);
	}

	#region General Extensions
	public static bool Prompt() {
		string read = Console.ReadLine();
		if (read == null || read.Length < 1) { return false; } else {
			return read.ToLower()[0] == "y"[0];
		}
	}

	public static bool Prompt(out string c) { c = Console.ReadLine(); return c.ToLower()[0] == "y"[0]; }

	#endregion

	#region List/Array Management
	/// <summary>
	/// Returns a generated string (with newlines) from the entered array of strings
	/// </summary>
	/// <param name="array">The entered array of strings</param>
	/// <returns></returns>
	public static string ToString(string[] array) {
        string gen = array[0];
        for (int i = 0; i < array.Length - 1; i++) { gen += "\n" + array[i + 1]; }
        return gen;
    }

    ///<summary>
    ///Returns a random object from the given list of objects
    ///<paramref name="list">The given list</paramref>
    ///</summary>
    public static T Random<T>(this List<T> list) {
		Random random = new Random();
		try { return list[random.Next(0, list.Count)]; } catch { return list[0]; }
    }

    /// <summary>
    /// Converts the given array and returns it as a list
    /// </summary>
    /// <typeparam name="T">The returned list</typeparam>
    /// <param name="array">The given array</param>
    /// <returns></returns>
    public static List<T> ToList<T>(this T[] array) {
        List<T> list = new List<T>();
        foreach (T t in array) { list.Add(t); }
        return list;
    }

    /// <summary>
    /// Combines two arrays
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="a">The array that will be combined with "b"</param>
    /// <param name="b">The array to be combined with "a"</param>
    /// <returns></returns>
    public static T[] Combine<T>(this T[] a, T[] b) {
        List<T> nA = a.ToList();
        nA.AddRange(b.ToList());
        a = nA.ToArray();
        return nA.ToArray();
    }

    /// <summary>
    /// Returns the combination of two arrays
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="a">The first array</param>
    /// <param name="b">The second array</param>
    /// <returns></returns>
    public static T[] SCombine<T>(this T[] a, T[] b) {
        List<T> nA = a.ToList();
        nA.AddRange(b.ToList());
        return nA.ToArray();
    }

    /// <summary>
    /// Returns a generated string (with newlines) from the entered list of strings
    /// </summary>
    /// <param name="list">The entered list of strings</param>
    /// <returns></returns>
    public static string GenerateString(List<string> list) {
        string gen = list[0];
        for (int i = 0; i < list.Count - 1; i++) { gen += "\n" + list[i + 1]; }
        return gen;
    }
    #endregion

    #region Value Truncating
    /// <summary>
    /// Returns the truncated version of "value" to "dec" places
    /// </summary>
    /// <param name="value">The input value to truncate</param>
    /// <param name="dec">The amount of decimal places</param>
    /// <returns></returns>
    public static float STruncate(this float value, int decimals) {
        return ((int)(value * (float)Math.Pow(10, decimals))) / (float)Math.Pow(10, decimals);
    }

    /// <summary>
    /// Truncates the given value to the entered amount of decimal places
    /// </summary>
    /// <param name="value">The given value to be Truncated</param>
    /// <param name="decimals">The amount of decimal places</param>
    /// <returns></returns>
    public static float Truncate(this float value, int decimals) {
        float final = ((int)(value * (float)Math.Pow(10, decimals))) / (float)Math.Pow(10, decimals);
        value = final;
        return final;
    }
    #endregion

    #region Value Forcing
    /// <summary>
    /// Forces the given variable (val) positive
    /// </summary>
    /// <param name="val">The given variable</param>
    /// <returns>The positive version of val</returns>
    public static float Positive(this float val) {
        return val < 0 ? -val : val;
    }

    /// <summary>
    /// Forces the given variable (val) positive
    /// </summary>
    /// <param name="val">The given variable</param>
    /// <returns>The positive version of val</returns>
    public static int Positive(this int val) {
        return val < 0 ? -val : val;
    }

    /// <summary>
    /// Forces the given variable (val) negative
    /// </summary>
    /// <param name="val">The given variable</param>
    /// <returns>The positive version of val</returns>
    public static float Negative(this float val) {
        return val > 0 ? -val : val;
    }

    /// <summary>
    /// Forces the given variable (val) negative
    /// </summary>
    /// <param name="val">The given variable</param>
    /// <returns>The positive version of val</returns>
    public static int Negative(this int val) {
        return val > 0 ? -val : val;
    }
	#endregion

	public static DialogResult BrowseForFile(out string path, string initialDirectory = "C:\\", string title = "Browse for Files", bool checkFile = true, bool checkPath = true, string extension = "txt", string filter = "Text files (*.txt)|*.txt") {
		OpenFileDialog dialog = new OpenFileDialog {
			InitialDirectory = initialDirectory,
			Title = title,

			CheckFileExists = checkFile,
			CheckPathExists = checkPath,

			DefaultExt = extension,
			Filter = filter
		};

		path = dialog.FileName;
		return dialog.ShowDialog();
	}

	public static OpenFileDialog BrowseForFile(out DialogResult result, string initialDirectory = "C:\\", string title = "Browse for Files", bool checkFile = true, bool checkPath = true, string extension = "txt", string filter = "Text files (*.txt)|*.txt") {
		OpenFileDialog dialog = new OpenFileDialog {
			InitialDirectory = initialDirectory,
			Title = title,

			CheckFileExists = checkFile,
			CheckPathExists = checkPath,

			DefaultExt = extension,
			Filter = filter
		};

		result = dialog.ShowDialog();
		return dialog;
	}
}
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using OpenLibrary.Extension;

namespace OpenLibrary.Utility
{
	/// <summary>
	/// Generator
	/// </summary>
	public static class Generator
	{
		private static readonly System.Random random = new System.Random();
		private const string RANDOM_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		/// <summary>
		/// Generate random string for certain length with certain characters
		/// </summary>
		/// <param name="availableCharacters">list of available characters</param>
		/// <param name="size">length of string</param>
		/// <returns></returns>
		[System.Obsolete("Use RandomString(int,string) instead")]
		public static string RandomString(string availableCharacters, int size = 5)
		{
			return RandomString(size, availableCharacters);
		}

		/// <summary>
		/// Generate random string for certain length with certain characters
		/// </summary>
		/// <param name="availableCharacters">list of available characters</param>
		/// <param name="size">length of string</param>
		/// <returns></returns>
		public static string RandomString(int size = 5, string availableCharacters = RANDOM_CHARS)
		{
			availableCharacters = string.IsNullOrEmpty(availableCharacters) ? RANDOM_CHARS : new System.String(availableCharacters.Distinct().ToArray());
			char[] buffer = new char[size];
			for (int i = 0; i < size; i++)
				buffer[i] = availableCharacters[random.Next(availableCharacters.Length)];
			return new System.String(buffer);
		}

		/// <summary>
		/// Generate unique string path for temporary file.
		/// </summary>
		/// <param name="path">locate file in certail location instead of temporary folder</param>
		/// <param name="extension">file extension</param>
		/// <returns>string</returns>
		public static string UniquePath(string path = "", string extension = "")
		{
			string directory = string.IsNullOrEmpty(path) ?
				Path.GetTempPath() : Path.GetDirectoryName(path);
			string file = System.Guid.NewGuid().ToString();
			file += !string.IsNullOrEmpty(extension) ? extension :
				!string.IsNullOrEmpty(path) ? Path.GetExtension(path) : "";
			return directory + Path.DirectorySeparatorChar + file;
		}

		/// <summary>
		/// Generate slug from string
		/// </summary>
		/// <param name="word">source string</param>
		/// <returns>string</returns>
		public static string Slug(string word)
		{
			if (string.IsNullOrEmpty(word))
				throw new System.ArgumentNullException("word", "Cannot generate slug from empty word.");
			//untuk karakter spesial
			word = word.Trim().Replace("#", "-sharp").Replace("++", "plusplus");
			//ganti selain alphabet dan angka menjadi strip/dashed (-)
			string slug = Regex.Replace(word, @"[^\w\-]", "-");
			//hapus tanda strip yang berlebihan
			slug = Regex.Replace(slug, @"(\-+)", "-");
			//hapus tanda strip di awal dan diakhir
			slug = Regex.Replace(slug, @"(^\-|\-$)", "");
			return slug.ToLower();
		}

		/// <summary>
		/// Generate slug from string
		/// </summary>
		/// <param name="word">source string</param>
		/// <returns>string</returns>
		public static string Sluggify(this string word)
		{
			return Slug(word);
		}

		/// <summary>
		/// Get file extension
		/// </summary>
		/// <param name="filename">filename</param>
		/// <returns>string</returns>
		public static string FileExtension(this string filename)
		{
			if (string.IsNullOrEmpty(filename))
				return filename;
			int lastDot = filename.LastIndexOf('.');
			return lastDot < 0
				? filename.ToLower()
				: filename.Substring(lastDot + 1).ToLower();
		}

		public static string RemoveExtension(this string filename)
		{
			if (string.IsNullOrEmpty(filename))
				return filename;
			int lastDot = filename.LastIndexOf('.');
			return lastDot < 0
				       ? filename
				       : filename.Substring(0, filename.Length - (filename.Length - lastDot));
		}

		/// <summary>
		/// Generate humanize file size
		/// </summary>
		/// <param name="size">file size in byte</param>
		/// <returns>string</returns>
		public static string HumanizeFileSize(long size)
		{
			if (size < 0)
				return size.ToString();
			string[] humanSize = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
			int indexSize = 0;
			while (size >= 1024 && indexSize + 1 < humanSize.Length)
			{
				indexSize++;
				size /= 1024;
			}
			return string.Format("{0:0,#.##} {1}", size, humanSize[indexSize]);
		}

		/// <summary>
		/// Generate humanize file size
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static string HumanizeFileSize(this FileStream file)
		{
			return file == null ? "0" : HumanizeFileSize(file.Length);
		}

		/// <summary>
		/// Generate alphabet string of number (1 -> A, 27 -> AA, 62 -> BJ, etc)
		/// </summary>
		/// <param name="number">number</param>
		/// <returns>alphabet index</returns>
		public static string Alphabet(this int number)
		{
			string digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			int columnBase = digits.Length;
			if (number <= columnBase)
				return digits[number - 1].ToString();
			int digitMax = System.Math.Ceiling(System.Math.Log(int.MaxValue, columnBase)).To<int>();
			var sb = new System.Text.StringBuilder().Append(' ', digitMax);
			int column = number;
			int offset = digitMax;
			while (column > 0)
			{
				sb[--offset] = digits[--column % columnBase];
				column /= columnBase;
			}
			return sb.ToString(offset, digitMax - offset);
		}
	}
}

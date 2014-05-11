using System.Security.Cryptography;
using System.Text;

namespace OpenLibrary.Utility
{
	/// <summary>
	/// Provide encryption library
	/// </summary>
	public class Encryption
	{
		/// <summary>
		/// Encrypt using HMAC MD5 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="data">data to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacMd5(string secret, byte[] data)
		{
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			using (var hmac = new HMACMD5(secretBytes))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Encrypt using HMAC MD5 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacMd5(string secret, string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			return HmacMd5(secret, messageBytes);
		}

		/// <summary>
		/// Encrypt using HMAC SHA1 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="data">data to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha1(string secret, byte[] data)
		{
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			using (var hmac = new HMACSHA1(secretBytes))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Encrypt using HMAC SHA1 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha1(string secret, string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			return HmacSha1(secret, messageBytes);
		}

		/// <summary>
		/// Encrypt using HMAC SHA256 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="data">data to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha256(string secret, byte[] data)
		{
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			using (var hmac = new HMACSHA256(secretBytes))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Encrypt using HMAC SHA256 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha256(string secret, string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			return HmacSha256(secret, messageBytes);
		}

		/// <summary>
		/// Encrypt using HMAC SHA384 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="data">data to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha384(string secret, byte[] data)
		{
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			using (var hmac = new HMACSHA384(secretBytes))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Encrypt using HMAC SHA384 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha384(string secret, string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			return HmacSha384(secret, messageBytes);
		}

		/// <summary>
		/// Encrypt using HMAC SHA512 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="data">data to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha512(string secret, byte[] data)
		{
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			using (var hmac = new HMACSHA512(secretBytes))
			{
				return hmac.ComputeHash(data);
			}
		}

		/// <summary>
		/// Encrypt using HMAC SHA512 algorithm
		/// </summary>
		/// <param name="secret">salt/secret key</param>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] HmacSha512(string secret, string message)
		{
			var messageBytes = Encoding.UTF8.GetBytes(message);
			return HmacSha512(secret, messageBytes);
		}

		/// <summary>
		/// Encrypt using SHA1 algorithm
		/// </summary>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] Sha1(string message)
		{
			using (var encryptor = new SHA1CryptoServiceProvider())
			{
				return encryptor.ComputeHash(Encoding.UTF8.GetBytes(message));
			}
		}

		/// <summary>
		/// Encrypt using MD5 algorithm
		/// </summary>
		/// <param name="message">message to be encrypt</param>
		/// <returns></returns>
		public static byte[] Md5(string message)
		{
			using (var encryptor = new MD5CryptoServiceProvider())
			{
				return encryptor.ComputeHash(Encoding.UTF8.GetBytes(message));
			}
		}
	}
}

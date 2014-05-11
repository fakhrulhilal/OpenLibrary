using System.Configuration;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Environment type
	/// </summary>
	public enum EnvironmentType
	{
		/// <summary>
		/// Default
		/// </summary>
		Default,

		/// <summary>
		/// Development/debugging
		/// </summary>
		Development,

		/// <summary>
		/// Production/live
		/// </summary>
		Production,

		/// <summary>
		/// Stagging from development to production
		/// </summary>
		Stagging
	}

	/// <summary>
	/// Helper for configuration file
	/// </summary>
	public static class ConfigurationHelper
	{
		/// <summary>
		/// Get app configuration (App.Config).
		/// First fallback [Application]:<paramref name="key"/>.
		/// Second fallback [Environment].<paramref name="key"/>.
		/// Last fallback [Environment].[Application]:<paramref name="key"/>.
		/// </summary>
		/// <param name="key">Get configuration value based on this key</param>
		/// <returns>string</returns>
		public static string AppConfig(string key)
		{
			if (string.IsNullOrEmpty(key))
				return string.Empty;
			string output = GetConfig(key);
			if (!string.IsNullOrEmpty(output))
				return output;
			//try searching for [Application]:key if empty or not found
			string applicationName = GetConfig("Application");
			if (!string.IsNullOrEmpty(applicationName))
				output = GetConfig(applicationName + ":" + key);
			if (!string.IsNullOrEmpty(output))
				return output;
			//try searching [Environment].key if when not found
			string environment = GetConfig("Environment");
			if (!string.IsNullOrEmpty(environment))
				output = GetConfig(environment + "." + key);
			if (!string.IsNullOrEmpty(output))
				return output;
			//try searching [Environment].[Application]:key
			if (!string.IsNullOrEmpty(environment) && !string.IsNullOrEmpty(applicationName))
				output = GetConfig(string.Format("{0}.{1}:{2}", environment, applicationName, key));
			return !string.IsNullOrEmpty(output) ? output : string.Empty;
		}

		private static string GetConfig(string key)
		{
			try
			{
				return ConfigurationManager.AppSettings[key];
			}
			catch (System.IndexOutOfRangeException) { }
			catch (ConfigurationErrorsException) { }
			catch (System.NullReferenceException) { }
			return null;
		}

		/// <summary>
		/// Get app configuration (App.Config)
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="key">Get configuration value based on this key</param>
		/// <returns>string</returns>
		public static string AppConfig(this HtmlHelper helper, string key)
		{
			return AppConfig(key);
		}

		/// <summary>
		/// Get current application environment
		/// </summary>
		/// <returns>string</returns>
		public static EnvironmentType CurrentEnvironment()
		{
			string environment = AppConfig("Environment").Trim();
			environment = environment.Substring(0, 1).ToUpper() + environment.Substring(1).ToLower();
			EnvironmentType environmentType;
			//development environment as default
			return System.Enum.TryParse(environment, out environmentType) ? environmentType : EnvironmentType.Development;
		}

		/// <summary>
		/// Get current database connection name based on current environment
		/// </summary>
		/// <returns>string</returns>
		public static string CurrentDatabaseConnection()
		{
			return CurrentEnvironment().ToString() + "Connection";
		}

		/// <summary>
		/// Get current database connection string based on current environment
		/// </summary>
		/// <returns>string</returns>
		public static string CurrentConnectionString()
		{
			try
			{
				return ConfigurationManager.ConnectionStrings[CurrentDatabaseConnection()].ConnectionString;
			}
			catch (ConfigurationErrorsException) { }
			catch (System.NullReferenceException) { }
			return string.Empty;
		}
	}
}

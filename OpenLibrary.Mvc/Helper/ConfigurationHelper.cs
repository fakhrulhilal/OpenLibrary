using System.Collections.Generic;
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
		private static Dictionary<string, string> _cachedConfig;

		/// <summary>
		/// Get app configuration (App.Config)
		/// </summary>
		/// <param name="key">Get configuration value based on this key</param>
		/// <returns>string</returns>
		public static string AppConfig(string key)
		{
			if (string.IsNullOrEmpty(key))
				return string.Empty;
			_cachedConfig = _cachedConfig ?? new Dictionary<string, string>();
			try
			{
				//search in cache first
				return _cachedConfig[key];
			}
			catch (KeyNotFoundException)
			{
				//search in web.config section app
				string appConfig = ConfigurationManager.AppSettings[key];
				if (string.IsNullOrEmpty(appConfig) && key.Trim().ToLower() != "application")
				{
					//try searching for [Application]:key if empty or not found
					string applicationName = AppConfig("Application");
					string _appConfig = ConfigurationManager.AppSettings[applicationName + ":" + key];
					if (!string.IsNullOrEmpty(_appConfig))
						appConfig = _appConfig;
				}
				_cachedConfig[key] = appConfig;
				return appConfig;
			}
			catch (System.IndexOutOfRangeException) { }
			catch (ConfigurationErrorsException) { }
			catch (System.NullReferenceException) { }
			return string.Empty;
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
			System.Func<string> onDefault = () => string.Empty;
			try
			{
				return ConfigurationManager.ConnectionStrings[CurrentDatabaseConnection()].ConnectionString;
			}
			catch (ConfigurationErrorsException) { return onDefault(); }
			catch (System.NullReferenceException) { return onDefault(); }
		}
	}
}

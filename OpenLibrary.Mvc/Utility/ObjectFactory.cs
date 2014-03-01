using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenLibrary.Mvc.Utility
{
	/// <summary>
	/// Get injected object from DependencyResolver
	/// </summary>
	public class ObjectFactory
	{
		private static System.Web.Http.Dependencies.IDependencyResolver WebApiResolver { get { return System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver; } }
		private static System.Web.Mvc.IDependencyResolver MvcResolver { get { return System.Web.Mvc.DependencyResolver.Current; } }
		
		#region Decprecated Methods

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		[System.Obsolete("Use GetApiInstance instead")]
		public static object GetInstance(System.Type type)
		{
			return GetApiInstance(type);
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		[System.Obsolete("Use GetApiInstance instead")]
		public static T GetInstance<T>()
		{
			return GetApiInstance<T>();
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		[System.Obsolete("Use GetApiInstances instead")]
		public static IEnumerable GetInstances(System.Type type)
		{
			return GetApiInstances(type);
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		[System.Obsolete("Use GetApiInstances instead")]
		public static IEnumerable<T> GetInstances<T>()
		{
			return GetApiInstances<T>();
		}
		
		#endregion

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		public static object GetApiInstance(System.Type type)
		{
			return WebApiResolver.GetService(type);
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		public static T GetApiInstance<T>()
		{
			return (T)GetApiInstance(typeof(T));
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		public static IEnumerable GetApiInstances(System.Type type)
		{
			return WebApiResolver.GetServices(type);
		}

		/// <summary>
		/// Get instance for Web API
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetApiInstances<T>()
		{
			return GetApiInstances(typeof(T)).Cast<T>();
		}

		/// <summary>
		/// Get instance for MVC
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		public static object GetMvcInstance(System.Type type)
		{
			return MvcResolver.GetService(type);
		}

		/// <summary>
		/// Get instance for MVC
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		public static T GetMvcInstance<T>()
		{
			return (T)MvcResolver.GetService(typeof(T));
		}

		/// <summary>
		/// Get instances for MVC
		/// </summary>
		/// <param name="type">type of service</param>
		/// <returns></returns>
		public static IEnumerable GetMvcInstances(System.Type type)
		{
			return MvcResolver.GetServices(type);
		}

		/// <summary>
		/// Get instances for MVC
		/// </summary>
		/// <typeparam name="T">type of service</typeparam>
		/// <returns></returns>
		public static IEnumerable<T> GetMvcInstances<T>()
		{
			return GetMvcInstances(typeof(T)).Cast<T>();
		}
	}
}

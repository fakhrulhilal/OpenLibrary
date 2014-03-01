using System.Web.Mvc;
using OpenLibrary.Mvc.Helper;
using OpenLibrary.Mvc.Utility;

namespace OpenLibrary.Mvc.Attribute
{
	/// <summary>
	/// Use custom authorization based on username, access object and access right.
	/// Unauthorized user will be brought to ViewResult with the name "UnauthorizedAccess".
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
	public class AccessRightAttribute : AuthorizeAttribute
	{
		/// <summary>
		/// Implementation of access right provider.
		/// </summary>
		public IAccessRightProvider Provider { get; set; }

		/// <summary>
		/// Requested access right (using constant from <see cref="AccessRightType"/> or using convention based on action name).
		/// </summary>
		public int? AccessRight { get; set; }

		/// <summary>
		/// Object being accessed
		/// </summary>
		public string AccessObject { get; private set; }

		/// <summary>
		/// Set access right with default access object
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(System.Type provider, int accessRight)
			: this(provider, null, accessRight)
		{ }

		/// <summary>
		/// Set access object with default access right
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessObject">object being accessed</param>
		public AccessRightAttribute(System.Type provider, string accessObject)
			: this(provider, accessObject, null)
		{ }

		/// <summary>
		/// Set security with default access right and access object
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		public AccessRightAttribute(System.Type provider)
			: this(provider, null, null)
		{ }

		/// <summary>
		/// Set access right for access object
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessObject">object being accessed</param>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(System.Type provider, string accessObject, int? accessRight)
			: this(accessObject, accessRight, 0)
		{
			if (provider == null)
				throw new System.ArgumentNullException("provider", "Provider must be defined.");
			if (!typeof(IAccessRightProvider).IsAssignableFrom(provider))
				throw new System.ArgumentException("Provider must be implementation of IAccessRightProvider.");
			try
			{
				//try create instance normally
				Provider = (IAccessRightProvider)System.Activator.CreateInstance(provider);
			}
			catch (System.MissingMethodException)
			{
				//search using Dependency Injection
				Provider = (IAccessRightProvider)ObjectFactory.GetMvcInstance(provider);
				if (Provider == null)
					throw new System.ArgumentNullException(string.Format("Cannot create implementation of {0} using {1}, even using injected version.", typeof(IAccessRightProvider).Name, provider.Name));
			}
		}

		/// <summary>
		/// Set access right with default access object
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(IAccessRightProvider provider, int accessRight)
			: this(provider, null, accessRight)
		{ }

		/// <summary>
		/// Set access object with default access right
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessObject">object being accessed</param>
		public AccessRightAttribute(IAccessRightProvider provider, string accessObject)
			: this(provider, accessObject, null)
		{ }

		/// <summary>
		/// Set security with default access right and access object
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		public AccessRightAttribute(IAccessRightProvider provider)
			: this(provider, null, null)
		{ }

		/// <summary>
		/// Set access right for access object 
		/// </summary>
		/// <param name="provider">Access right provider implementation</param>
		/// <param name="accessObject">object being accessed</param>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(IAccessRightProvider provider, string accessObject, int? accessRight)
			: this(accessObject, accessRight, 0)
		{
			if (provider == null)
				throw new System.ArgumentNullException("provider", "Provider must be defined.");
			Provider = provider;
		}

		private AccessRightAttribute(string accessObject, int? accessRight, int dummy)
		{
			if (!string.IsNullOrEmpty(accessObject))
				AccessObject = accessObject;
			if (accessRight.HasValue)
				AccessRight = accessRight;
		}

		/// <summary>
		/// Set access object with default access right using injected provider.
		/// </summary>
		/// <param name="accessObject">object being accessed</param>
		public AccessRightAttribute(string accessObject)
			: this(accessObject, null)
		{ }

		/// <summary>
		/// Set access right with default access object using injected provider.
		/// </summary>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(int accessRight)
			: this((string)null, accessRight)
		{ }

		/// <summary>
		/// Set security with default access right and access object using injected provider
		/// </summary>
		public AccessRightAttribute()
			: this((string)null, null)
		{ }

		/// <summary>
		/// Set access right for access object using injected provider.
		/// </summary>
		/// <param name="accessObject">object being accessed</param>
		/// <param name="accessRight">requested access right</param>
		public AccessRightAttribute(string accessObject, int? accessRight)
			: this(accessObject, accessRight, 0)
		{
			Provider = ObjectFactory.GetMvcInstance<IAccessRightProvider>();
			if (Provider == null)
				throw new System.ArgumentException("Provider (IAccessRightProvider imlementation) must be defined. Be sure to register injection first before register filter.");
		}

		/// <summary>
		/// Main authorization process
		/// </summary>
		/// <param name="filterContext"></param>
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			var controller = filterContext.Controller;
			var actionName = filterContext.ActionDescriptor.ActionName;
			var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
			var action = filterContext.ActionDescriptor;
			//skip if controller allow anonymous (search in action/method first than class)
			if (action.IsDefined(typeof(AllowAnonymousAttribute), true))
				return;
			if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) &&
				!action.IsDefined(this.GetType(), true))
				return;
			var user = filterContext.HttpContext.User.Identity;
			//redirect to login page when not authenticated
			if (!user.IsAuthenticated)
			{
				base.OnAuthorization(filterContext);
				return;
			}
			//skip if AllowAuthenticated
			if (action.IsDefined(typeof(AllowAuthenticatedAttribute), true))
				return;
			if (!action.IsDefined(this.GetType(), true) &&
				filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAuthenticatedAttribute), true))
				return;
			string accessObject =
				AccessObject ?? "/" +
				System.String.Join("/",
								   controller.Routing(RoutingType.Area),
								   controllerName)
							 .TrimStart(new[] { '/' });
			//defined access right based on action name when not specified
			var accessRight = AccessRight;
			if (!accessRight.HasValue)
			{
				AccessRightType accessRightType;
				if (System.Enum.TryParse(actionName, true, out accessRightType))
					accessRight = (int)accessRightType;
			}
			if (Provider.IsAuthorized(user.Name, accessObject, accessRight.GetValueOrDefault()))
				return;
			//Unauthorized request
			var response = filterContext.HttpContext.Response;
			response.Clear();
			response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
			response.TrySkipIisCustomErrors = true;
			filterContext.Result = new ViewResult
			{
				ViewName = "UnauthorizedAccess",
				TempData = filterContext.Controller.TempData,
				ViewData = new ViewDataDictionary<UnauthorizedAccessRight>(new UnauthorizedAccessRight
				{
					Message = string.Format("User {0} doesn't have sufficient priviledge to {1} for {2} access right.", user.Name, accessObject, actionName),
					AccessObject = accessObject,
					AccessRight = accessRight.GetValueOrDefault(),
					ActionName = actionName
				})
			};
		}
	}

	#region Type helper

	/// <summary>
	/// Access right type
	/// </summary>
	[System.Flags]
	public enum AccessRightType : short
	{
		/// <summary>
		/// Forbid any access right
		/// </summary>
		[System.ComponentModel.Description("Forbid All")]
		None = 0,

		/// <summary>
		/// Access right to create data
		/// </summary>
		Create = 1,

		/// <summary>
		/// Alias to <see cref="Create"/>
		/// </summary>
		Add = Create,

		/// <summary>
		/// Access right to read data
		/// </summary>
		Read = 2,

		/// <summary>
		/// Alias to <see cref="Read"/>
		/// </summary>
		Index = Read,

		/// <summary>
		/// Alias to <see cref="Read"/>
		/// </summary>
		Detail = Read,

		/// <summary>
		/// Access right to update data
		/// </summary>
		Update = 4,

		/// <summary>
		/// Alias to <see cref="Update"/>
		/// </summary>
		Edit = Update,

		/// <summary>
		/// Alias to <see cref="Update"/>
		/// </summary>
		Modify = Update,

		/// <summary>
		/// Access right to delete data
		/// </summary>
		Delete = 8,

		/// <summary>
		/// Alias to <see cref="Delete"/>
		/// </summary>
		Remove = Delete,

		/// <summary>
		/// Access right to processing data
		/// </summary>
		Process = 16,

		/// <summary>
		/// Access right to approve data
		/// </summary>
		Approve = 32,

		/// <summary>
		/// Hak akses mendasar untuk data: Create-Read-Update-Delete
		/// </summary>
		[System.ComponentModel.Description("Create-Read-Update-Delete")]
		Crud = Create | Read | Update | Delete,

		/// <summary>
		/// Alias untuk semuanya
		/// </summary>
		All = Create | Read | Update | Delete | Process | Approve
	}

	/// <summary>
	/// Information sent by AccessRightAttribute for unauthorized request.
	/// </summary>
	public class UnauthorizedAccessRight
	{
		/// <summary>
		/// Object being accessed
		/// </summary>
		public string AccessObject { get; set; }

		/// <summary>
		/// Requested access right (using custom or based on <see cref="AccessRightType"/>
		/// </summary>
		public int AccessRight { get; set; }

		/// <summary>
		/// Requested action name
		/// </summary>
		public string ActionName { get; set; }

		/// <summary>
		/// Prepared friendly message
		/// </summary>
		public string Message { get; set; }
	}

	#endregion
}

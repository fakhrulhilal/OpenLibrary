using System.Text;
using System.Web.Mvc;

namespace OpenLibrary.Mvc.Helper
{
	/// <summary>
	/// Flash message type
	/// </summary>
	public enum FlashMessageType
	{
		/// <summary>
		/// Info (default: blue alert)
		/// </summary>
		Info,
		
		/// <summary>
		/// Warning (default: yellow alert)
		/// </summary>
		Warning,

		/// <summary>
		/// Block
		/// </summary>
		Block,

		/// <summary>
		/// Success (default: green alert)
		/// </summary>
		Success,

		/// <summary>
		/// Error (default: red alert)
		/// </summary>
		Error,

		/// <summary>
		/// Alias for Error (red alert)
		/// </summary>
		Danger = Error
	}

	/// <summary>
	/// Create flash message that available for 1 time event for redirected page. Set it on controller, place it on view.
	/// </summary>
	public static class FlashMessageHelper
	{
		private const string FLASH_MESSAGE = "FlashMessage_";

		/// <summary>
		/// Generate one time show only message (even redirected). 
		/// This flash message compatible with Twitter Bootstrap alert.
		/// This will clear all message.
		/// Call it in view using <code>@Html.Flash()</code>
		/// </summary>
		/// <param name="controller">Extension hook ke controller</param>
		/// <param name="message">message content</param>
		/// <param name="messageType">flash message type</param>
		public static void SetFlash(this Controller controller, string message, FlashMessageType messageType = FlashMessageType.Info)
		{
			//jika tidak sesuai dengan yang ada, maka defaultnya adalah tipe pertama
			messageType = !System.Enum.IsDefined(typeof(FlashMessageType), messageType)
												? FlashMessageType.Info
												: messageType;
			string type = messageType.ToString().ToLower();
			controller.TempData[FLASH_MESSAGE + type] = message;
		}

		/// <summary>
		/// Add additional flash message. Call this function after <see cref="SetFlash(Controller,string,FlashMessageType)"/>
		/// </summary>
		/// <param name="controller">Extension hook ke controller</param>
		/// <param name="message">message content</param>
		/// <param name="messageType">flash message type</param>
		public static void AppendFlash(this Controller controller, string message, FlashMessageType messageType)
		{
			if (!System.Enum.IsDefined(typeof(FlashMessageType), messageType))
			{
				string[] types = System.Enum.GetNames(typeof(FlashMessageType));
				throw new OpenLibraryException("Message cannot be empty when using this method: " + string.Join(", ", types), OpenLibraryErrorType.ArgumentNotValidError);
			}
			string type = messageType.ToString().ToLower();
			string lastMessage = !string.IsNullOrEmpty(controller.TempData[FLASH_MESSAGE + type].ToString()) ?
				controller.TempData[FLASH_MESSAGE + type].ToString() : string.Empty;
			lastMessage += message;
			controller.TempData[FLASH_MESSAGE + type] = lastMessage;
		}

		private static string RenderFlashMessage(string message, string type)
		{
			var htmlFlash = new TagBuilder("div");
			string className = "alert-" + type;
			if (type == "error")
				className += " alert-danger";
			htmlFlash.AddCssClass("alert " + className + " fade in");
			var htmlClose = new TagBuilder("button");
			htmlClose.AddCssClass("close");
			htmlClose.Attributes.Add("data-dismiss", "alert");
			htmlClose.Attributes.Add("type", "button");
			htmlClose.InnerHtml = "×";
			htmlFlash.InnerHtml = htmlClose.ToString();
			htmlFlash.InnerHtml += message;
			return htmlFlash.ToString();
		}

		/// <summary>
		/// Show HTML flash message that already set using <see cref="SetFlash(Controller,string,FlashMessageType)"/> or <see cref="AppendFlash(Controller,string,FlashMessageType)"/>.
		/// This flash message compatible with Twitter Bootstrap alert
		/// </summary>
		/// <param name="helper">Extension hook</param>
		/// <returns>string</returns>
		public static MvcHtmlString Flash(this HtmlHelper helper)
		{
			var html = new StringBuilder();
			string[] types = System.Enum.GetNames(typeof(FlashMessageType));
			foreach (string t in types)
			{
				string type = t.ToLower();
				if (helper.ViewContext.TempData[FLASH_MESSAGE + type] != null)
				{
					html.Append(RenderFlashMessage(helper.ViewContext.TempData[FLASH_MESSAGE + type].ToString(), type));
					//hapus dari data
					helper.ViewContext.TempData.Remove(FLASH_MESSAGE + type);
				}
			}
			return new MvcHtmlString(html.Length > 0 ? html.ToString() : string.Empty);
		}

		/// <summary>
		/// Generate one time show only message (even redirected). 
		/// This flash message compatible with Twitter Bootstrap alert.
		/// Get html generated directly.
		/// </summary>
		/// <param name="controller">Extension ke controller</param>
		/// <param name="message">message content</param>
		/// <param name="messageType">flash message type</param>
		public static string Flash(this Controller controller, string message, FlashMessageType messageType = FlashMessageType.Info)
		{
			return RenderFlashMessage(message, messageType.ToString().ToLower());
		}
	}
}

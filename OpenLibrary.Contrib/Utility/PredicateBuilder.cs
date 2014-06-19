using System.Linq;
using System.Linq.Expressions;

namespace OpenLibrary.Utility
{
	/// <summary>
	/// Expression predicate builder.
	/// Original reference: http://www.albahari.com/nutshell/predicatebuilder.aspx
	/// </summary>
	public static class PredicateBuilder
	{
		public static Expression<System.Func<T, bool>> True<T>() { return f => true; }
		public static Expression<System.Func<T, bool>> False<T>() { return f => false; }

		/// <summary>
		/// Operate bitwise OR
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="expr1">existing expression</param>
		/// <param name="expr2">new expression to be merged</param>
		/// <returns></returns>
		public static Expression<System.Func<T, bool>> Or<T>(this Expression<System.Func<T, bool>> expr1, Expression<System.Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<System.Func<T, bool>>
				  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		/// <summary>
		/// Operate bitwise AND
		/// </summary>
		/// <typeparam name="T">typeof entity</typeparam>
		/// <param name="expr1">existing expression</param>
		/// <param name="expr2">new expression to be merged</param>
		/// <returns></returns>
		public static Expression<System.Func<T, bool>> And<T>(this Expression<System.Func<T, bool>> expr1, Expression<System.Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<System.Func<T, bool>>
				  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
		}
	}
}

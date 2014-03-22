using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OpenLibrary.Extension;
using System.ComponentModel.DataAnnotations.Schema;
using OpenLibrary.Annotation;
using System.ComponentModel.DataAnnotations;

namespace OpenLibrary.Utility
{
	/// <summary>
	/// Execute SQL or stored procedure, transactional or without it, based on connection string in web.config or app.config
	/// </summary>
	public class Sql
	{
		private static IsolationLevel defaultIsolationLevel = IsolationLevel.ReadUncommitted;
		private static Dictionary<string, SqlConnection> connectionManager;
		private static Dictionary<string, SqlTransaction> transactionManager;
		private static string defaultConnectionString;
		private static System.Action<string, System.Exception> onError = (connectionString, exception) =>
		{
			if (IsTransactionStarted(connectionString))
				RollbackTransaction(connectionString);
			CloseConnection(connectionString);
			throw new OpenLibraryException("Failed execute query", exception, OpenLibraryErrorType.OperationFailedError);
		};

		/// <summary>
		/// Default connection string name in configuration file.
		/// Using appConfig section OpenLibrary.Utility:Sql(DefaultConnectionString) when not specified
		/// </summary>
		public static string DefaultConnectionString
		{
			get
			{
				if (!string.IsNullOrEmpty(defaultConnectionString))
					return defaultConnectionString;
				try
				{
					defaultConnectionString = System.Configuration.ConfigurationManager.AppSettings["OpenLibrary.Utility:Sql(DefaultConnectionString)"];
					return defaultConnectionString;
				}
				catch (System.Configuration.ConfigurationErrorsException) { }
				return string.Empty;
			}
			set { defaultConnectionString = value; }
		}

		#region Helper Method

		private static string GenerateKey(string connectionString)
		{
			return string.Format("{0}_{1}", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), connectionString);
		}

		private static SqlCommand Prepare(string connectionString, string sql, bool isStoredProcedure, params SqlParameter[] parameters)
		{
			try
			{
				var connection = OpenConnection(connectionString);
				var command = new SqlCommand(sql, connection);
				//pakai transaction jika ada
				if (IsTransactionStarted(connectionString))
					command.Transaction = GetTransaction(connectionString);
				if (parameters.Length > 0)
					command.Parameters.AddRange(parameters);
				command.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
				return command;
			}
			catch (System.Configuration.ConfigurationErrorsException exception)
			{
				throw new OpenLibraryException("Connection string not found", exception, OpenLibraryErrorType.ArgumentNotValidError);
			}
		}

		private static SqlConnection OpenConnection(string connectionString)
		{
			connectionManager = connectionManager ?? new Dictionary<string, SqlConnection>();
			//buka koneksi jika belum ada
			try
			{
				string key = GenerateKey(connectionString);
				if (!connectionManager.ContainsKey(key))
				{
					var sqlConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
					connectionManager[key] = new SqlConnection(sqlConnectionString);
				}
				var connection = connectionManager[key];
				if (connection.State == ConnectionState.Closed)
					connection.Open();
				return connection;
			}
			catch (System.NullReferenceException exception)
			{
				throw new OpenLibraryException(string.Format("There's no connection string with name {0}", connectionString), exception, OpenLibraryErrorType.ArgumentNullError);
			}
			catch (System.ArgumentNullException exception)
			{
				throw new OpenLibraryException(string.Format("There's no connection string with name {0}", connectionString), exception, OpenLibraryErrorType.ArgumentNullError);
			}
			catch (KeyNotFoundException exception)
			{
				throw new OpenLibraryException(string.Format("There's no connection string with name {0}", connectionString), exception, OpenLibraryErrorType.ArgumentNullError);
			}
			catch (System.IndexOutOfRangeException exception)
			{
				throw new OpenLibraryException(string.Format("There's no connection string with name {0}", connectionString), exception, OpenLibraryErrorType.ArgumentNullError);
			}
			catch (System.InvalidOperationException exception)
			{
				throw new OpenLibraryException(string.Format("Couldn't connect to database for connectiono string name {0}", connectionString), exception, OpenLibraryErrorType.OperationFailedError);
			}
			catch (SqlException exception)
			{
				throw new OpenLibraryException(string.Format("Couldn't connect to database for connectiono string name {0}", connectionString), exception, OpenLibraryErrorType.OperationFailedError);
			}
			catch (System.Configuration.ConfigurationErrorsException exception)
			{
				throw new OpenLibraryException(string.Format("There's error on connection configuration with name {0}", connectionString), exception, OpenLibraryErrorType.OperationFailedError);
			}
		}

		private static void CloseConnection(string connectionString)
		{
			try
			{
				string key = GenerateKey(connectionString);
				var connection = connectionManager[key];
				if (connection == null)
					return;
				if (connection.State != ConnectionState.Closed)
					connection.Close();
				connection.Dispose();
				connectionManager.Remove(key);
			}
			catch (KeyNotFoundException) { }
			DeleteTransaction(connectionString);
		}

		private static SqlTransaction GetTransaction(string connectionString)
		{
			transactionManager = transactionManager ?? new Dictionary<string, SqlTransaction>();
			string key = GenerateKey(connectionString);
			if (!transactionManager.ContainsKey(key))
				transactionManager[key] = null;
			return transactionManager[key];
		}

		private static void DeleteTransaction(string connectionString)
		{
			transactionManager = transactionManager ?? new Dictionary<string, SqlTransaction>();
			string key = GenerateKey(connectionString);
			if (transactionManager.ContainsKey(key))
			{
				var transaction = transactionManager[key];
				if (transaction != null)
					transaction.Dispose();
				transactionManager.Remove(key);
			}
		}

		/// <summary>
		/// Build SQL Query for certain DML
		/// </summary>
		/// <typeparam name="T">typeof entity class</typeparam>
		/// <param name="data">data to generated for SQL query</param>
		/// <param name="query">query type</param>
		/// <returns>SQL string</returns>
		public static string BuildSql<T>(T data, QueryType query)
			where T : class
		{
			if (data == null || data is System.String)
				return null;
			var type = typeof(T);
			List<MappingConfiguration> mappings;
			string sql = null;
			string tableName = System.Attribute.IsDefined(type, typeof(TableAttribute), true)
				? ((TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true)[0]).Name
				: type.Name;
			List<string> columns;
			switch (query)
			{
				case QueryType.Select:
					break;
				case QueryType.Insert:
					mappings = type.ExtractColumn(new[] { typeof(ReadOnlyAttribute), typeof(DatabaseGeneratedAttribute) });
					columns = mappings.Select(m => m.ColumnName).ToList();
					var parameters = mappings.Select(m => "@" + m.ColumnName).ToList();
					sql = string.Format("insert into {0}({1}) values({2});", tableName, string.Join(", ", columns), string.Join(", ", parameters));
					sql += "select SCOPE_IDENTITY();";
					break;
				case QueryType.Update:
					mappings = type.ExtractColumn(new[] { typeof(ReadOnlyAttribute), typeof(DatabaseGeneratedAttribute), typeof(KeyAttribute) });
					columns = mappings.Select(m => m.ColumnName).ToList();
					sql = string.Format("update {0} set ", tableName);
					var updateColumn = columns.Select(m => string.Format("{0} = {1}", m, "@" + m));
					sql += string.Join(", ", updateColumn);
					sql += " where ";
					//find primary key
					var keys = data.ExtractColumnWithAttributes(new[] { typeof(KeyAttribute) });
					//column primary key must be defined for update query
					if (keys.Count >= 1)
						sql += string.Join(" and ", keys.Select(m => string.Format("{0} = {1}", m.ColumnName, "@" + m.ColumnName)));
					else
						throw new System.ArgumentException("There's no primary key (KeyAttribute) defined for entity " + type.Name);
					sql += ";";
					sql += "select @@ROWCOUNT;";
					break;
				case QueryType.Delete:
					sql = string.Format("delete from {0} where ", tableName);
					var primaryKeys = data.ExtractColumnWithAttributes(new[] { typeof(KeyAttribute) });
					//column primary key must be defined for update query
					if (primaryKeys.Count >= 1)
						sql += string.Join(" and ", primaryKeys.Select(m => string.Format("{0} = {1}", m.ColumnName, "@" + m.ColumnName)));
					else
						throw new System.ArgumentException("There's no primary key (KeyAttribute) defined for entity " + type.Name);
					sql += ";";
					sql += "select @@ROWCOUNT;";
					break;
			}
			return sql;
		}

		/// <summary>
		/// Convert object instance to collection of SqlParameter
		/// </summary>
		/// <param name="data">object parameter</param>
		/// <param name="query">SQL query type</param>
		/// <returns>collection of SqlParameter</returns>
		public static SqlParameter[] ToSqlParameter<T>(T data, QueryType query) where T : class 
		{
			if (data == null || data.IsPrimitive())
				return new SqlParameter[0];
			var excludeAttributes = new List<System.Type>();
			switch (query)
			{
				case QueryType.Select:
					break;
				case QueryType.Insert:
					excludeAttributes.Add(typeof(ReadOnlyAttribute));
					excludeAttributes.Add(typeof(DatabaseGeneratedAttribute));
					break;
				case QueryType.Update:
					excludeAttributes.Add(typeof(ReadOnlyAttribute));
					//excludeAttributes.Add(typeof(DatabaseGeneratedAttribute));
					break;
				case QueryType.Delete:
					excludeAttributes.Add(typeof(ReadOnlyAttribute));
					break;
				case QueryType.Any:
					break;
			}
			var mappings = data.ExtractColumn(excludeAttributes.ToArray());
			return
				(from mapping in mappings
				 let value = data.GetFieldValue(mapping.PropertyName)
				 select new SqlParameter("@" + mapping.ColumnName, value ?? (object)System.DBNull.Value)).ToArray();
		}

		#endregion

		#region Transaction Method

		/// <summary>
		/// Start transaction based on connection string name in configuration file
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="isolationLevel">isolation level</param>
		public static void BeginTransaction(string connectionString, IsolationLevel isolationLevel)
		{
			SqlTransaction transaction = null;
			var connection = OpenConnection(connectionString);
			if (!IsTransactionStarted(connectionString))
				transaction = connection.BeginTransaction(isolationLevel);
			string key = GenerateKey(connectionString);
			transactionManager[key] = transaction;
		}

		/// <summary>
		/// Start transaction based on connection string name in configuration file with ReadUncommitted isolation level
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		public static void BeginTransaction(string connectionString)
		{
			BeginTransaction(connectionString, defaultIsolationLevel);
		}

		/// <summary>
		/// Start transaction using <see cref="DefaultConnectionString"/> as connection string name
		/// </summary>
		/// <param name="isolationLevel">isolation level</param>
		public static void BeginTransaction(IsolationLevel isolationLevel)
		{
			BeginTransaction(DefaultConnectionString, isolationLevel);
		}

		/// <summary>
		/// Start transaction using <see cref="DefaultConnectionString"/> as connection string name with ReadUncommitted isolation level
		/// </summary>
		public static void BeginTransaction()
		{
			BeginTransaction(DefaultConnectionString, defaultIsolationLevel);
		}

		/// <summary>
		/// Commit transaction based on connection string name in configuration file (it didn't rollback automatically).
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <returns></returns>
		public static bool CommitTransaction(string connectionString)
		{
			if (!IsTransactionStarted(connectionString))
				return false;
			var transaction = GetTransaction(connectionString);
			if (transaction == null)
				return false;
			bool status = false;
			try
			{
				transaction.Commit();
				DeleteTransaction(connectionString);
				status = true;
			}
			catch (SqlException) { }
			catch (System.InvalidOperationException) { }
			finally { CloseConnection(connectionString); }
			return status;
		}

		/// <summary>
		/// Commit transaction using <see cref="DefaultConnectionString"/> as connection string name (it didn't rollback automatically).
		/// </summary>
		/// <returns></returns>
		public static bool CommitTransaction()
		{
			return CommitTransaction(DefaultConnectionString);
		}

		/// <summary>
		/// Rollback transaction based on connection string name in configuration file.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <returns></returns>
		public static bool RollbackTransaction(string connectionString)
		{
			if (!IsTransactionStarted(connectionString))
				return false;
			var transaction = GetTransaction(connectionString);
			if (transaction == null)
				return false;
			bool status = false;
			try
			{
				transaction.Rollback();
				DeleteTransaction(connectionString);
				status = true;
			}
			catch (SqlException) { }
			catch (System.InvalidOperationException) { }
			finally { CloseConnection(connectionString); }
			return status;
		}

		/// <summary>
		/// Rollback transaction using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <returns></returns>
		public static bool RollbackTransaction()
		{
			return RollbackTransaction(DefaultConnectionString);
		}

		/// <summary>
		/// Commit transaction based on connection string name in configuration file, auto rollback when failed.
		/// Automatically close connection when finished.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <returns>true when successfully save changes/commit or false when failed (auto rollback)</returns>
		/// <exception cref="OpenLibrary.OpenLibraryException">See inner exception for detail error</exception>
		public static bool EndTransaction(string connectionString)
		{
			if (!IsTransactionStarted(connectionString))
				return false;
			var transaction = GetTransaction(connectionString);
			try
			{
				transaction.Commit();
			}
			catch (SqlException exception)
			{
				transaction.Rollback();
				throw new OpenLibraryException("Failed saving changes", exception, OpenLibraryErrorType.OperationFailedError);
			}
			catch (System.InvalidOperationException exception)
			{
				transaction.Rollback();
				throw new OpenLibraryException("Failed saving changes", exception, OpenLibraryErrorType.OperationFailedError);
			}
			catch (KeyNotFoundException) { }
			finally
			{
				//langsung close koneksi database
				CloseConnection(connectionString);
			}
			return true;
		}

		/// <summary>
		/// Close transaction using <see cref="DefaultConnectionString"/> as connection string name.
		/// Automatically close connection when finished.
		/// </summary>
		/// <returns>true when successfully save changes/commit or false when failed (auto rollback)</returns>
		/// <exception cref="OpenLibrary.OpenLibraryException">See inner exception for detail error</exception>
		public static bool EndTransaction()
		{
			return EndTransaction(DefaultConnectionString);
		}

		/// <summary>
		/// Check wether transaction is running/started or not based on connection string name in configuration file
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <returns></returns>
		public static bool IsTransactionStarted(string connectionString)
		{
			transactionManager = transactionManager ?? new Dictionary<string, SqlTransaction>();
			string key = GenerateKey(connectionString);
			return
				transactionManager.ContainsKey(key) &&
				transactionManager[key] != null &&
				transactionManager[key].Connection != null;
		}

		/// <summary>
		/// Check wether transaction is running/started or not using <see cref="DefaultConnectionString"/> as connection string name
		/// </summary>
		/// <returns></returns>
		public static bool IsTransactionStarted()
		{
			return IsTransactionStarted(DefaultConnectionString);
		}

		#endregion

		/// <summary>
		/// Execute non resultset query.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns></returns>
		/// <exception cref="OpenLibrary.OpenLibraryException">See inner exception for detail error</exception>
		public static object ExecuteNonQuery(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
		{
			var command = Prepare(connectionString, sql, isStoredProcedure, parameters);
			command.CommandText = sql;
			try
			{
				var output = command.ExecuteScalar();
				//hanya close koneksi database jika tidak menggunakan transaction
				if (!IsTransactionStarted(connectionString))
					CloseConnection(connectionString);
				return output;
			}
			catch (SqlException exception) { onError(connectionString, exception); }
			catch (System.InvalidCastException exception) { onError(connectionString, exception); }
			catch (System.InvalidOperationException exception) { onError(connectionString, exception); }
			catch (System.IO.IOException exception) { onError(connectionString, exception); }
			return null;
		}

		/// <summary>
		/// Execute non resultset query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns></returns>
		/// <exception cref="OpenLibrary.OpenLibraryException">See inner exception for detail error</exception>
		public static object ExecuteNonQuery(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
		{
			return ExecuteNonQuery(DefaultConnectionString, sql, isStoredProcedure, parameters);
		}

		/// <summary>
		/// Execute non resultset query.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation), use <see cref="ReadOnlyAttribute"/> for skip including as SqlParameter.</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static object ExecuteNonQuery(string connectionString, string sql, object parameters, bool isStoredProcedure = false, QueryType query = QueryType.Any)
		{
			if (parameters == null)
				throw new OpenLibraryException("Parameters cannot be null", OpenLibraryErrorType.ArgumentNullError);
			if (parameters.IsPrimitive())
				throw new OpenLibraryException("Parameters cannot be primitive data type", OpenLibraryErrorType.ArgumentNotValidError);
			var sqlParameters = ToSqlParameter(parameters, query);
			return ExecuteNonQuery(connectionString, sql, isStoredProcedure, sqlParameters);
		}

		/// <summary>
		/// Execute non resultset query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation), use <see cref="ReadOnlyAttribute"/> for skip including as SqlParameter.</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <returns></returns>
		public static object ExecuteNonQuery(string sql, object parameters, bool isStoredProcedure = false)
		{
			return ExecuteNonQuery(DefaultConnectionString, sql, parameters, isStoredProcedure);
		}

		/// <summary>
		/// Execute non resultset query.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation), use <see cref="ReadOnlyAttribute"/> for skip including as SqlParameter.</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="query">SQL query type</param>
		/// <returns></returns>
		public static List<object> ExecuteNonQuery(string connectionString, string sql, IEnumerable<object> parameters, bool isStoredProcedure = false, QueryType query = QueryType.Any)
		{
			if (parameters == null)
				throw new OpenLibraryException("Parameters cannot be null", OpenLibraryErrorType.ArgumentNullError);
			if (parameters.IsPrimitive())
				throw new OpenLibraryException("Parameters cannot be primitive data type", OpenLibraryErrorType.ArgumentNotValidError);
			var output = new List<object>();
			foreach (var sqlParameter in parameters)
				output.Add(ExecuteNonQuery(connectionString, sql, sqlParameter, isStoredProcedure, query));
			return output;
		}

		/// <summary>
		/// Execute non resultset query.
		/// </summary>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation), use <see cref="ReadOnlyAttribute"/> for skip including as SqlParameter.</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="query">SQL query type</param>
		/// <returns></returns>
		public static List<object> ExecuteNonQuery(string sql, IEnumerable<object> parameters, bool isStoredProcedure = false, QueryType query = QueryType.Any)
		{
			return ExecuteNonQuery(DefaultConnectionString, sql, parameters, isStoredProcedure, query);
		}

		/// <summary>
		/// Execute resultset query.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>Key is column name, value is data</returns>
		public static List<Dictionary<string, object>> Query(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
		{
			var command = Prepare(connectionString, sql, isStoredProcedure, parameters);
			var output = new List<Dictionary<string, object>>();
			try
			{
				using (var reader = command.ExecuteReader())
				{
					var columns = new List<string>();
					while (reader.Read())
					{
						var row = new Dictionary<string, object>();
						if (columns.Count < 1)
						{
							int totalColumn = reader.FieldCount;
							for (int i = 0; i < totalColumn; i++)
								columns.Add(reader.GetName(i));
						}
						columns.ForEach(column => row.Add(column, reader[column]));
						output.Add(row);
					}
				}
				if (!IsTransactionStarted(connectionString))
					CloseConnection(connectionString);
			}
			catch (SqlException exception) { onError(connectionString, exception); }
			catch (System.InvalidCastException exception) { onError(connectionString, exception); }
			catch (System.InvalidOperationException exception) { onError(connectionString, exception); }
			catch (System.IO.IOException exception) { onError(connectionString, exception); }
			return output;
		}

		/// <summary>
		/// Execute resultset query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>Key is column name, value is data</returns>
		public static List<Dictionary<string, object>> Query(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
		{
			return Query(DefaultConnectionString, sql, isStoredProcedure, parameters);
		}

		/// <summary>
		/// Execute resultset query and map to strong type object.
		/// </summary>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="mapper">mapping function</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string connectionString, string sql, System.Func<SqlDataReader, T> mapper, bool isStoredProcedure = false, params SqlParameter[] parameters)
		{
			var command = Prepare(connectionString, sql, isStoredProcedure, parameters);
			var output = new List<T>();
			try
			{
				if (command.Connection.State == ConnectionState.Closed)
					command.Connection.Open();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
						try
						{
							output.Add(mapper(reader));
						}
						catch (System.Exception exception)
						{
							throw new OpenLibraryException("Failed mapping row to entity", exception, OpenLibraryErrorType.OperationFailedError);
						}
				}
				if (!IsTransactionStarted(connectionString))
					CloseConnection(connectionString);
			}
			catch (SqlException exception) { onError(connectionString, exception); }
			catch (System.InvalidCastException exception) { onError(connectionString, exception); }
			catch (System.InvalidOperationException exception) { onError(connectionString, exception); }
			catch (System.IO.IOException exception) { onError(connectionString, exception); }
			return output;
		}

		/// <summary>
		/// Execute resultset query and map to strong type object using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="mapper">mapping function</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string sql, System.Func<SqlDataReader, T> mapper, bool isStoredProcedure = false,
			params SqlParameter[] parameters)
		{
			return Query(DefaultConnectionString, sql, mapper, isStoredProcedure, parameters);
		}

		/// <summary>
		/// Execute resultset query and map to strong type object using automapping (support default DataAnnotation).
		/// </summary>
		/// <typeparam name="T">typeof output entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
			where T : class, new()
		{
			//generate column -> field mapping 
			var mapping = typeof(T).ExtractColumn();
			System.Func<SqlDataReader, T> mapper = reader =>
			{
				T entity = new T();
				foreach (var map in mapping)
				{
					try
					{
						entity.SetFieldValue(map.PropertyName, reader[map.ColumnName]);
					}
					catch (System.IndexOutOfRangeException)
					{
						System.Diagnostics.Debug.WriteLine("Cannot find column {0} for property name {1}", map.ColumnName, map.PropertyName);
						entity.SetFieldValue(map.PropertyName, null);
					}
				}
				return entity;
			};
			return Query(connectionString, sql, mapper, isStoredProcedure, parameters);
		}

		/// <summary>
		/// Execute resultset query and map to strong type object using automapping (support default DataAnnotation) using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof output entity</typeparam>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <param name="parameters">parameter when available</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters)
			where T : class, new()
		{
			return Query<T>(DefaultConnectionString, sql, isStoredProcedure, parameters);
		}

		/// <summary>
		/// Execute resultset query and map to strong type object using automapping (support default DataAnnotation).
		/// </summary>
		/// <typeparam name="T">typeof output entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation).</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string connectionString, string sql, object parameters, bool isStoredProcedure = false)
			where T : class, new()
		{
			if (parameters == null)
				throw new OpenLibraryException("Parameters cannot be null", OpenLibraryErrorType.ArgumentNullError);
			if (parameters.IsPrimitive())
				throw new OpenLibraryException("Parameters cannot be primitive data type", OpenLibraryErrorType.ArgumentNotValidError);
			//create SqlParameter based on anonymous object
			var sqlParameters = ToSqlParameter(parameters, QueryType.Any);
			return Query<T>(connectionString, sql, isStoredProcedure, sqlParameters);
		}

		/// <summary>
		/// Execute resultset query and map to strong type object using automapping (support default DataAnnotation) using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof output entity</typeparam>
		/// <param name="sql">sql query or stored procedure name</param>
		/// <param name="parameters">anonymous object or instance of class (support default DataAnnotation).</param>
		/// <param name="isStoredProcedure">determine wether <paramref name="sql"/> is raw sql or stored procedure name</param>
		/// <returns>collection of strong type object</returns>
		public static List<T> Query<T>(string sql, object parameters, bool isStoredProcedure = false)
			where T : class, new()
		{
			return Query<T>(DefaultConnectionString, sql, parameters, isStoredProcedure);
		}

		/// <summary>
		/// Execute insert query.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="data">entity to be inserted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Insert<T>(string connectionString, T data)
			where T : class
		{
			if (data == null)
				return null;
			string sql = BuildSql(data, QueryType.Insert);
			var output = ExecuteNonQuery(connectionString, sql, false, ToSqlParameter(data, QueryType.Insert));
			//test wether T has DatabaseGenerated
			var generatedField = typeof(T).ExtractColumnWithAttributes(new[] { typeof(DatabaseGeneratedAttribute) });
			if (generatedField.Count > 0)
				data.SetFieldValue(generatedField[0].PropertyName, output);
			return output;
		}

		/// <summary>
		/// Execute insert query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="data">entity to be inserted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Insert<T>(T data)
			where T : class
		{
			return Insert(DefaultConnectionString, data);
		}

		/// <summary>
		/// Execute insert query for a collection of data.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="data">entities to be inserted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static List<object> Insert<T>(string connectionString, T[] data)
			where T : class
		{
			if (data == null || data.Length < 1)
				throw new OpenLibraryException("Data to be inserted cannot be null", OpenLibraryErrorType.ArgumentNullError);
			var output = new List<object>();
			T exampleData = data.First();
			string sql = BuildSql(exampleData, QueryType.Insert);
			//test wether T has DatabaseGenerated
			var generatedField = typeof(T).ExtractColumnWithAttributes(new[] { typeof(DatabaseGeneratedAttribute) });
			string primaryKey = generatedField.Count > 0 ? generatedField[0].PropertyName : null;
			foreach (var item in data)
			{
				var rowOutput = ExecuteNonQuery(connectionString, sql, false, ToSqlParameter(item, QueryType.Insert));
				if (!string.IsNullOrEmpty(primaryKey))
					rowOutput.SetFieldValue(primaryKey, rowOutput);
				output.Add(rowOutput);
			}
			return output;
		}

		/// <summary>
		/// Execute insert query for a collection of data using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="data">entities to be inserted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static List<object> Insert<T>(T[] data)
			where T : class
		{
			return Insert(DefaultConnectionString, data);
		}

		/// <summary>
		/// Execute update query.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="data">entity to be updated</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Update<T>(string connectionString, T data)
			where T : class
		{
			try
			{
				string sql = BuildSql(data, QueryType.Update);
				var sqlParameter = ToSqlParameter(data, QueryType.Update);
				return ExecuteNonQuery(connectionString, sql, false, sqlParameter);
			}
			catch (System.ArgumentException exception)
			{
				throw new OpenLibraryException("Failed exeuting update query. See inner exception for detail.", exception, OpenLibraryErrorType.ArgumentNotValidError);
			}
		}

		/// <summary>
		/// Execute update query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="data">entity to be updated</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Update<T>(T data)
			where T : class
		{
			return Update(DefaultConnectionString, data);
		}

		/// <summary>
		/// Execute delete query.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="connectionString">configuration name in configuration file</param>
		/// <param name="data">entity to be deleted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Delete<T>(string connectionString, T data)
			where T : class
		{
			string sql = BuildSql(data, QueryType.Delete);
			var sqlParameter = ToSqlParameter(data, QueryType.Delete);
			return ExecuteNonQuery(connectionString, sql, false, sqlParameter);
		}

		/// <summary>
		/// Execute delete query using <see cref="DefaultConnectionString"/> as connection string name.
		/// </summary>
		/// <typeparam name="T">typeof input entity</typeparam>
		/// <param name="data">entity to be deleted</param>
		/// <returns>Return of <see cref="ExecuteNonQuery(string,string,bool,System.Data.SqlClient.SqlParameter[])"/></returns>
		public static object Delete<T>(T data)
			where T : class
		{
			return Delete(DefaultConnectionString, data);
		}
	}

	/// <summary>
	/// SQL query type
	/// </summary>
	public enum QueryType
	{
		/// <summary>
		/// Select query
		/// </summary>
		Select,

		/// <summary>
		/// Insert query
		/// </summary>
		Insert,

		/// <summary>
		/// Update query
		/// </summary>
		Update,

		/// <summary>
		/// Delete query
		/// </summary>
		Delete,

		/// <summary>
		/// General query
		/// </summary>
		Any
	}
}

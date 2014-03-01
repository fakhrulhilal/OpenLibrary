OpenLibrary
===========

# Table of Contents
- [Description](#description)
- [Install](#install)
- [Documentation](#documentation)
    - [Document](#document)
    	- [CSV](#csv)
    		- [Importing from CSV](#importing-from-csv)
    		- [Exporting to CSV](#exporting-to-csv)
    	- [Old Excel](#old-excel-before-excel-1997)
    		- [Importing from old excel](#importing-from-old-excel)
    		- [Exporting to old excel](#exporting-to-old-excel)
    	- [New Excel (excel 1997 an above)](#new-Excel-excel-1997-an-above)
			- [Importing from new excel](#importing-from-new-excel)
			- [Exporting to new excel](#exporting-to-new-excel)
    - [MVC](#mvc)
    - [Extension](#extension)
    - [Utility](#utility)
        - [SQL](#sql)
            - [Select Entity](#select-entity)
                - [Using dictionary as return output](#using-dictionary-as-return-output)
                - [Using strong type with custom entity mapper](#using-strong-type-with-custom-entity-mapper)
                - [Using strong type with default entity mapper](#using-strong-type-with-default-entity-mapper)
            - [Insert Single Entity](#insert-single-entity)
            - [Insert Multiple Entity](#insert-multiple-entity)
            - [Delete Entity](#delete-entity)
            - [Update Entity](#update-entity)
            - [Execute Non Query](#execute-non-query)
            - [Transaction](#transaction)

# Description
This library was built based on my work experience. I was gathering little by little. So, you can contribute to this repo to share something usefull for others. It consits of 4 usefull functions: Document (for export/import from/to excel & csv), MVC (extend ASP.net MVC framework), Extension (various object extension), utility (general utility function).

# Install
Just go to nuget package manager and type these command below:
```
PM> Install-Package OpenLibrary
```

# Documentation
## Document

> __namespace :__ `OpenLibrary.Document`

From version 1.7.3, it's moved to difference assembly and nuget package. 
```
PM> Install-Package OpenLibrary-Document
```

This library has dependency to EPPlus (for export/import excel XML format), NPOI (for export/import old excel format). We can export/import of CSV, excel (old & new format) by using this library, and of course, in our format. Basically, first row are preserved for caption header (later, we can define header row position). This libray will scan header row for caption to be used as property name in our POCO, and then MEMBERLAKUKAN all rows in that column as data for property in our POCO.
We can also define custom mapping using `MappingOptionAttribute` (located in `OpenLibrary.Annotation` namespace). 
- Caption: string on header row in file (csv or excel)
- Field: property name (will be processed automatically)
- Width: column width (applied only for excel when exporting)
- Sequence: column order, smaller will be shown first (applied only when exporting, will be processed automatically when importing)
- Type: property type (applied only when importing, will be processed automatically)
This library will search Caption defined in `MappingOptionAttribute` at the first, and then `Name` property in `DisplayAttribute` as first fallback, and then property name as last fallback. We can also use `NotMappedAttribute` to be skipped when exporting/importing. This library will globally ignore these property names: CreatedBy, CreatedTime, ModifiedBy, ModifiedTime. This library will only export/import primitive data type. We can also override property mapping later when exporting/importing. All export methods are preceded with "To" word and "From" word for import methods.
Suppose we have a POCO like this:
```csharp
public class Employee
{
	[Key, NotMapped]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Display(Name = "Fullname")]
	public string Name { get; set; }

	[Display(Name = "Active"), MappingOption("Status")]
	public bool IsActive { get; set; }
	
	//we'll use this for helper in our code, ignore when exporting/importing
	[NotMapped]
	public string Status { get { return IsActive ? "Active" : "Non Active"; } }
		
	public int Age { get; set; }

	[Display(Name = "Birth")]
	public System.DateTime? BirthDate { get; set; }

	public System.DateTime CreatedTime { get; set; }

	public string CreatedBy { get; set; }

	public System.DateTime? ModifiedTime { get; set; }

	public string ModifiedBy { get; set; }
}
```
So our headr row file will be like this:
| Fullname |  Status | Age | Birth |

### CSV

For CSV file, we're using comma as default delimiter, but you can override it incase your file using different delimiter. For CSV, it's better to define date time format for DateTime data type or using yyyy-MM-dd as default format.

#### Importing from CSV

__Syntax__
```csharp
//import and execute function for each row
void FromCsv<T>(Stream file, System.Action<T> action, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false) where T : class, new();
void FromCsv(Stream file, System.Action<Dictionary<string, object>> action, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false);

//get all imported rows in collection
List<T> FromCsv<T>(Stream file, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false) where T : class, new();
List<T> FromCsv<T>(string filename, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false) where T : class, new();
List<Dictionary<string, object>> FromCsv(Stream file, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false);
List<Dictionary<string, object>> FromCsv(string filename, List<MappingOption> importOption = null, string dateFormat = "yyyy-MM-dd", string delimiter = ",", bool isCaseSensitive = false);
```
Example:
```csharp
//import from CSV and directly save to table for each row
Csv.FromCsv<Employee>("employee.csv", row => Sql.Insert(row));
//get all imported rows, header row are case sensitive, convert all caption to uppercase
var importOption = typeof(Employee).ExtractField(); //use this extension to build custom mapping, located in OpenLibrary.Extension namespace
//remove age
importOption = importOption.RemoveAll(m => m.Caption == "AGE");
var data = Csv.FromCsv<Employee>("employee.csv", importOption, delimiter: ";", isCaseSensitive: true);
```

#### Exporting to CSV

__Syntax__
```csharp
Stream ToCsv<T>(IEnumerable<T> data, List<MappingOption> exportOption = null, string delimiter = ",", string dateFormat = "") where T : class;
void ToCsv<T>(IEnumerable<T> data, string filename, List<MappingOption> exportOption = null, string delimiter = ",", string dateFormat = "") where T : class;
```
Example
```csharp
//data from above code
var stream = Csv.ToCsv(data);
//use id-ID as default culture
Csv.ToCsv(data, "employee.csv", dateFormat: "dd-MM-yyyy", delimiter: ";");
```

### Old Excel (before excel 1997)

#### Importing from old excel

By default, this library will use first worksheet as source if we don't define worksheet name when importing.
__Syntax__
```csharp
//import and execute function for each row
void FromExcel<T>(Stream file, System.Action<T> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "") where T : class, new();
void FromExcel<T>(string filename, System.Action<T> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "") where T : class, new();
void FromExcel(Stream file, System.Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "");
void FromExcel(string filename, System.Action<Dictionary<string, object>> action, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "");

//get all imported rows
List<T> FromExcel<T>(Stream file, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "") where T : class, new();
List<T> FromExcel<T>(string filename, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "") where T : class, new();
List<Dictionary<string, object>> FromExcel(string filename, string worksheetName = "", List<MappingOption> importOption = null, int headerRow = 1, bool isBreakOnEmptyRow = false, bool isCaseSensitive = false, string dateFormat = "");
```
Example
```csharp
//import from first worksheet and directly save to database
Excel.FromExcel<Employee>("employee.xls", row => Sql.Insert(row));
//import from certain worksheet, header row in certain row, BirthDate is in string (not __pure__ excel date)
Excel.FromExcel<Employee>("data.xls", row => Sql.Insert(row), "employee", headerRow: 5, dateFormat: "yyyy-MM-dd");

//get all imported rows, using dictionary as output instead of entity class (dictionary value is converted based on mapping option)
var importOption = typeof(Employee).ExtractField();
/our Status is written in string, so we change it from boolean to string
var statusOption = importOption.First(m => m.Field == "IsActive"); //field is property name in our POCO
statusOption.Type = typeof(string);
List<Dictionary<string, object>> employees = Excel.FromExcel("data.xls", "employee", importOption);
//dictionary key is Field (not Caption) in importOption
//change back status to boolean
data.ForEach(row => row["IsActive"] = ((string)row["IsActive"]).ToUpper() == "ACTIVE");
```

#### Exporting to old excel

__Syntax__
```csharp
Stream ToExcel<T>(IEnumerable<T> data, string worksheetName = "", List<MappingOption> exportOption = null, DocumentType formatType = DocumentType.Xls) where T : class;
void ToExcel<T>(IEnumerable<T> data, string filename, string worksheetName = "", List<MappingOption> exportOption = null, DocumentType formatType = DocumentType.Xls);
```
Example
```csharp
var data new List<Employee>
{
	new Employee { Age = 20, Name = "Robert" },
	new Employee { Age = 30, Name = "John" }	
};
var fileStream = Excel.ToExcel<Employee>(data, "employee");
```

### New Excel (excel 1997 an above)

#### Importing from new excel

#### Exporting to new excel

## MVC

--to be done--

## Extension

--to be done--

## Utility

This section covers functions outside above.

### SQL
> __namespace :__ `OpenLibrary.Utility`

It's an alternative to Entity Framework, dapper dot net, etc. If you prefer to use SqlDataReader and POCO classes than model first approach, than you're in the right place. You can reuse your POCO classes which you use in Entity Framework (compatible with [microsoft standar](http://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations%29v=vs.95%29.aspx "DataAnnotation"). Basically, it's based on connection string defined in web.config/app.config. All methods defined in this utility are preceded with connection string name. But you can override it using default connection string configured in your web.config/app.config, just add it in AppConfig section like example below:
```xml
<configuration>
	<connectionStrings>
		<add name="OpenLibraryConnection" connectionString="Data Source=.\SQLExpress;User Id=sa;Password=;Initial Catalog=OpenLibraryDB"/>
	</connectionStrings>
	<appSettings>
		<add key="OpenLibrary.Utility:Sql(DefaultConnectionString)" value="OpenLibraryConnection"/>
	</appSettings>
</configuration>
```
With this utility function, you don't need to open and close connection, it will be maintained automatically for you. You can either use transaction. It will close connection automatically when you don't use transaction or error on executing query. It will remain opened when you use transaction, so you can execute more than one command transactionally.

Suppose you have table as follows:
```sql
create table Mst_Employee (
    Id int identity(1,1) not null primary key,
    Name varchar(100) not null,
    Active bit not null,
	Age int not null,
	BirthDate date null,
    CreatedTime datetime not null default(getdate()),
    CreatedBy varchar(50) not null default('SYSTEM'),
    ModifiedTime datetime null,
    ModifiedBy varchar(50)
)
```
and here's our POCO class (you can generate it using [T-SQL POCO generator](http://blog.fakhrulhilal.com/post/70766076969/t-sql-c-poco-generator), I made some customization)
```csharp
[Table("Mst_Employee")]
public class Employee
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[MaxLength(100)]
	[Required]
	public string Name { get; set; }

    //different field name in table
	[Column("Active")]
	public bool IsActive { get; set; }
	
	//this is doesn't exist in table
	[NotMapped]
	public string Status { get { return IsActive ? "Active" : "Non Active"; } }
	
	//applied only for select, ignore on insert, update, delete
	//located in OpenLibrary.Annotation
	[ReadOnly]
	[Column("birthYear")]
	public System.DateTime? YearOfBirthDate { get; set; }
	
	public int Age { get; set; }

	[DataType(DataType.Date)]
	public System.DateTime? BirthDate { get; set; }

	public System.DateTime CreatedTime { get; set; }

	[MaxLength(50)]
	[Required]
	public string CreatedBy { get; set; }

	public System.DateTime? ModifiedTime { get; set; }

	[MaxLength(50)]
	public string ModifiedBy { get; set; }
}
```

#### Select Entity

There are various methods that we can use for select query.

##### Using dictionary as return output
__Syntax__
```csharp
//using certain connection string name
List<Dictionary<string, object>> Query(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters);

//using default connection string name
List<Dictionary<string, object>> Query(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters);
```
Output will return dictionary where column name (from table, not from entity class) as key and field value as dictionary value.
Example:
```csharp
//select active employee
var allData = Sql.Query("select * from Mst_Employee where Active = 1 and Name like @Name and CreatedBy = @creator", false, new SqlParameter("@Name", "%robert%"), new SqlParameter("@creator", "admin"));
//as same as
var allData = Sql.Query("OpenLibraryConnection", "select * from Mst_Employee where Active = 1 and Name like @Name and CreatedBy = @creator", false, new SqlParameter("@Name", "%robert%"), new SqlParameter("@creator", "admin"));
```
> __IMPORTANT__ There's no `IsActive` key in output, you should use `Active` key when getting result from database. It's difference with other method describe below.

Or you can exec stored procedure:
`var allData = Sql.Query("SP_Employee_select", true, new SqlParameter("@Name", "%robert%"), new SqlParameter("@creator", "admin"));`
where `SP_Employee_select` is as below:
```sql
create procedure SP_Employee_select (
    @Name varchar(max) = null, 
    @creator varchar(max) = null,
	@modifiedBy varchar(max) = null,
	@modifiedTime date = null
) as
begin
    select 
        *, 
        case when BirthDate is null then null else year(BirthDate) end birthYear 
    from Mst_Employee 
    where 
        Active = 1 
        and (@Name is null or Name like @Name)
        and CreatedBy = case when @creator is null then CreatedBy else @creator end
		and (@modifiedBy is null or ModifiedBy = @modifiedBy)
		and (@modifiedTime is null or cast(ModifiedTime as date) = @modifiedTime);
end
```
##### Using strong type with custom entity mapper
__Syntax__
```csharp
//using certain connection string name
List<T> Query<T>(string connectionString, string sql, System.Func<SqlDataReader, T> mapper, bool isStoredProcedure = false, params SqlParameter[] parameters);

//using default connection string name
List<T> Query<T>(string sql, System.Func<SqlDataReader, T> mapper, bool isStoredProcedure = false, params SqlParameter[] parameters);
```
Example:
```csharp
//I use ObjectExtension for convertion
//be sure to use OpenLibrary.Extension namespace
List<Employee> activeEmployee = Sql.Query<Employee>("SP_Employee_select", reader => new Employee 
{ 
	Id = reader["Id"].To<int>(),
	Name = reader["Name"].To<string>(),
	Age = reader["Age"].To<int>(),
	BirthDate = reader["BirthDate"].To<System.DateTime?>(),
	CreatedTime = reader["CreatedTime"].To<System.DateTime>(),
	CreatedBy = reader["CreatedBy"].To<string>(),
	IsActive = reader["Active"].To<bool>(),
	ModifiedTime = reader["ModifiedTime"].To<System.DateTime?>(),
	ModifiedBy = reader["ModifiedBy"].To<string>()
}, true, new SqlParameter("@Name", "%robert%"), new SqlParameter("@creator", "admin"));
```
##### Using strong type with default entity mapper
__Syntax__
```csharp
//using certain connection string name with SqlParameter for passing sql parameter
List<T> Query<T>(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters) where T : class, new();

//using default connection string name with SqlParameter for passing sql parameter
List<T> Query<T>(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters) where T : class, new();

//using certain connection string name with anonymous object for passing sql parameter
List<T> Query<T>(string connectionString, string sql, object parameters, bool isStoredProcedure = false) where T : class, new();

//using default connection string name with anonymous object for passing sql parameter
List<T> Query<T>(string sql, object parameters, bool isStoredProcedure = false) where T : class, new();
```
Example:
```csharp
//continuation of the above code
var activeEmployee = Sql.Query<Employee>("SP_Employee_select", new { Name = "%robert", creator = "admin" }, true);
//select unmodified employee
var unmodifiedEmployee = Sql.Query<Employee>("SP_Employee_select", new { 
	Name = "%robert", 
	creator = "admin",
	modifiedBy = (string)null,
	modifiedTime = (System.DateTime?)null
}, true);
```
#### Insert Single Entity
__Syntax__
```csharp
//insert entity to table with certain connection string name
object Insert<T>(string connectionString, T data) where T : class;

//insert entity to table with default connection string name
object Insert<T>(T data) where T : class;
```
Example:
```csharp
var employee = new Employee
{
	Name = "Robert",
	Age = 25,
	BirthDate = new System.DateTime(2000, 1, 1),
	IsActive = true,
	CreatedBy = "admin",
	CreatedTime = System.DateTime.Now
};
var id = Sql.Insert(employee);
//id = employee.Id --> your new identity key
```

#### Insert Multiple Entity
__Syntax__
```csharp
//insert multiple entity with certain connection string name
List<object> Insert<T>(string connectionString, T[] data) where T : class;

//insert multiple entity with default connection string name
List<object> Insert<T>(T[] data) where T : class;
```
Example
```csharp
var employees = new[] {
	new Employee
	{	
		Name = "Robert",
		Age = 25,
		BirthDate = new System.DateTime(2000, 1, 1),
		IsActive = true,
		CreatedBy = "admin",
		CreatedTime = System.DateTime.Now
	},
	new Employee
	{	
		Name = "Junior",
		Age = 15,
		IsActive = false,
		CreatedBy = "admin",
		CreatedTime = System.DateTime.Now
	}
};
Sql.Insert(employees);
```
#### Delete Entity
> __IMPORANT__ You've to define entity key (decorate with `KeyAttribute`)

Basically, it will delete entities based on primary key. Output is number of affected rows.
__Syntax__
```csharp
//delete using certain connection string name
object Delete<T>(string connectionString, T data) where T : class;

//delete using default connetion string name
object Delete<T>(T data) where T : class;
```
Example:
```csharp
//delete employee with Id = 1
Sql.Delete(new Employee { Id = 1 });
//delete first employee
var firstEmployee = Sql.Query<Employee>("select top 1 * from Mst_Employee").FirstOrDefault();
Sql.Delete(firstEmployee);
```
#### Update Entity
> __IMPORANT__ You've to define entity key (decorate with `KeyAttribute`)

Basically, it will update all property except decorated with `NotMappedAttribute`, `KeyAttribute`, `ReadOnlyAttribute` (located in `OpenLibrary.Annotation` namespace), `DatabaseGeneratedAttribute`. So, you've to be carefull when using this lazy syntax. Output is number of affected rows.
__Syntax__
```csharp
//update entity using certain connection string name
object Update<T>(string connectionString, T data) where T : class;

//update entity using default connection string name
object Update<T>(T data) where T : class;
```
Example:
```csharp
var firstEmployee = Sql.Query<Employee>("select top 1 * from Mst_Employee").FirstOrDefault();
//change his age
if (firstEmployee != null)
{
	firstEmployee.Age += 5;
	Sql.Update(firstEmployee);
}
```
#### Execute Non Query
Basically, it just wrapper for `SqlCommand.ExecuteScalar()` method. So, all outputs are from that `SqlCommand.ExecuteScalar()`.
__Syntax__
```csharp
//execute query using SqlParameter for passing sql parameter
object ExecuteNonQuery(string connectionString, string sql, bool isStoredProcedure = false, params SqlParameter[] parameters);
object ExecuteNonQuery(string sql, bool isStoredProcedure = false, params SqlParameter[] parameters);

//execute query using anonymous object for passing sql parameter
object ExecuteNonQuery(string connectionString, string sql, object parameters, bool isStoredProcedure = false, QueryType query = QueryType.Any);
object ExecuteNonQuery(string sql, object parameters, bool isStoredProcedure = false, QueryType query = QueryType.Any);
```
Example:
```csharp
//increment age by 2 for employee created by admin
int totalAffected = Sql.ExecuteNonQuery("update Mst_Employee set Age = Age + 2 where CreatedBy = @creator; select @@ROWCOUNT;", new { creator = "admin" }).To<int>();
```
#### Transaction
At first, you've to start transaction with these syntaxes:
```csharp
//start transaction with certain connection string and isolation level
void BeginTransaction(string connectionString, IsolationLevel isolationLevel);
//start transaction with certain connection string and default isolation level (IsolationLevel.ReadUncommitted)
void BeginTransaction(string connectionString);
////start transaction with default connection string and certain isolation level
void BeginTransaction(IsolationLevel isolationLevel);
//start transaction with default connection string and default isolation level (IsolationLevel.ReadUncommitted)
void BeginTransaction();
```
and then you can execute multiple sql command as describe above. Once you've finished with you're doing, you can save it using `Sql.EndTransaction()` or `Sql.EndTransaction(string connectionString)` (commit or rollback when error occured). Or you can manual commit using `Sql.CommitTransaction()` or `Sql.CommitTransaction(string connectionString)`, or manual rollback using `RollbackTransaction()` or `RollbackTransaction(string connectionString)`.

Basically, when error occured in the middle command (before you execute `Sql.EndTransaction()` or `Sql.EndTransaction(string connectionString)` or `Sql.CommitTransaction()` or `Sql.CommitTransaction(string connectionString)`), it will rollback automatically.

using OpenLibrary.Utility;
using OpenLibrary.Extension;
using OpenLibrary.Mvc.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using OpenLibrary.Annotation;
using OpenLibrary.Document;

namespace OpenLibrary.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			//string sql = "insert into reffpcc(xpcc_id, xname, xtype, xphone, xaddress, xcountry, xcommision, xdeposit) values(@xpcc_id, @xname, @xtype, @xphone, @xaddress, @xcountry, @xcommision, @xdeposit)";
			//var test = new AgentPcc
			//{
			//	Pcc = Generator.RandomString(8),
			//	Name = "iroel object",
			//	Type = "beda",
			//	Phone = "+87965465678",
			//	Address = "Lamongan",
			//	Country = "Indonesia"
			//};
			//var test2 = new AgentPcc
			//{
			//	Pcc = Generator.RandomString(8),
			//	Name = "iroel object2",
			//	Type = "beda2",
			//	Phone = "+87965465678",
			//	Address = "Lamongan2",
			//	Country = "Indonesia2"
			//};
			//var agent = Sql.Query<Agent>("select top 1 * from Mst_Agent where Id = 2").FirstOrDefault();
			//if (agent != null)
			//{
			//	agent.BrandName = "iroel";
			//	agent.CorporateName = "PT iroel";
			//	Sql.Update(agent);
			//}
			//agent = Sql.Query<Agent>("select top 1 * from Mst_Agent where Id = @Id", agent).FirstOrDefault();
			//Sql.Delete(agent);
			//Sql.ExecuteNonQuery(sql, new[] { test, test2 });
			//Sql.Insert(new[] { test, test2 });
			//Sql.ExecuteNonQuery(sql, false,
			//	new SqlParameter("@xpcc_id", Generator.RandomString(6)),
			//	new SqlParameter("@xname", "iroel SqlParameter"),
			//	new SqlParameter("@xphone", "+987678"),
			//	new SqlParameter("@xaddress", "Lamongan"),
			//	new SqlParameter("@xcountry", "Indo"),
			//	new SqlParameter("@xtype", "cowok"),
			//	new SqlParameter("@xcommision", DBNull.Value),
			//	new SqlParameter("@xdeposit", null));
			//var data = Sql.Query<AgentPcc>("select top 10 * from reffpcc where xpcc_id like @pcc", new { pcc = "%0%" });
			//if (data.Count < 1)
			//	Console.WriteLine("data kosong");
			//else
			//	data.ForEach(item => Console.WriteLine(item));
			typeof(Agent).ExtractField().ForEach(option => Console.WriteLine("Field = {0}, Caption = {1}, Width = {2}, Priority = {3}", option.Field, option.Caption, option.Width, option.Sequence.GetValueOrDefault()));
			//List<Employee> activeEmployee = Sql.Query<Employee>("SP_Employee_select", reader => new Employee 
			//{ 
			//	Id = reader["Id"].To<int>(),
			//	Name = reader["Name"].To<string>(),
			//	Age = reader["Age"].To<int>(),
			//	BirthDate = reader["BirthDate"].To<System.DateTime?>(),
			//	CreatedTime = reader["CreatedTime"].To<System.DateTime>(),
			//	CreatedBy = reader["CreatedBy"].To<string>(),
			//	IsActive = reader["Active"].To<bool>(),
			//	ModifiedTime = reader["ModifiedTime"].To<System.DateTime?>(),
			//	ModifiedBy = reader["ModifiedBy"].To<string>()
			//}, true, new SqlParameter("@Name", "%robert%"), new SqlParameter("@creator", "admin"));
			var activeEmployee = Sql.Query<Employee>("SP_Employee_select", new { Name = "%robert", creator = "admin" }, true);
			var unmodifiedEmployee = Sql.Query<Employee>("SP_Employee_select", new
			{
				Name = "%robert",
				creator = "admin",
				modifiedBy = (string)null,
				modifiedTime = (System.DateTime?)null
			}, true);
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
var firstEmployee = Sql.Query<Employee>("select top 1 * from Mst_Employee").FirstOrDefault();
//change his age
if (firstEmployee != null)
{
	firstEmployee.Age += 5;
	Sql.Update(firstEmployee);
}
			Console.ReadLine();
		}
	}

	[Table("Mst_Employee")]
	public class Employee
	{
		[Key]
		[Column("Id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column("Name")]
		[MaxLength(100)]
		[Required]
		public string Name { get; set; }

		[Column("Active")]
		public bool IsActive { get; set; }

		[Column("Age")]
		public int Age { get; set; }

		[Column("BirthDate")]
		[DataType(DataType.Date)]
		public System.DateTime? BirthDate { get; set; }

		[Column("CreatedTime")]
		public System.DateTime CreatedTime { get; set; }

		[Column("CreatedBy")]
		[MaxLength(50)]
		[Required]
		public string CreatedBy { get; set; }

		[Column("ModifiedTime")]
		public System.DateTime? ModifiedTime { get; set; }

		[Column("ModifiedBy")]
		[MaxLength(50)]
		public string ModifiedBy { get; set; }
	}

	[Table("reffpcc")]
	class AgentPcc
	{
		[Column("xpcc_id"), Required, MaxLength(10)]
		public string Pcc { get; set; }

		[Column("xname"), Required, MaxLength(50)]
		public string Name { get; set; }

		[Column("xtype"), Required, MaxLength(50)]
		public string Type { get; set; }

		[Column("xphone"), MaxLength(50)]
		public string Phone { get; set; }

		[Column("xaddress"), MaxLength(100)]
		public string Address { get; set; }

		[Column("xcity"), MaxLength(50)]
		public string City { get; set; }

		[Column("xcountry"), Required, MaxLength(10)]
		public string Country { get; set; }

		[Column("created_datetime"), ReadOnly]
		public System.DateTime? RegistrationTime { get; set; }

		[Column("xdeposit")]
		public decimal? Deposit { get; set; }

		[Column("xcommision")]
		public decimal? Commision { get; set; }

		[Column("xva"), MaxLength(20)]
		public string Va { get; set; }

		[Column("xbilltype"), MaxLength(3)]
		public string BillingType { get; set; }

		public override string ToString()
		{
			return string.Format("PCC = {0}, Deposit = {1}, Name = {2}, Type = {3}, Phone = {4}, Registration = {5}, VA = {6}", Pcc, Deposit.Format(decimalPoint: 2), Name, Type, Phone, RegistrationTime.Format(), Va);
		}
	}

	class TestAttribute : System.Attribute
	{
		public TestAttribute(int data)
		{

		}

		public string Test<T>(T data) where T : class { return null; }
	}

	[Table("Mst_Agent"), System.Obsolete]
	public class Agent
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Column("AgentLevelId")]
		[MappingOption("Agent Level", 4)]
		public int? AgentLevelId { get; set; }

		[Column("ParentAgentId")]
		[MappingOption("Parent Agent", 1)]
		public int? ParentAgentId { get; set; }

		[Column("CityId")]
		public int? CityId { get; set; }

		[Column("Pcc"), Required, MaxLength(50), Display(Name = "PCC")]
		public string Pcc { get; set; }

		[Column("BrandName"), MaxLength(100)]
		public string BrandName { get; set; }

		[Column("IsActive")]
		public bool IsActive { get; set; }

		[Column("CorporateName"), MaxLength(200)]
		public string CorporateName { get; set; }

		[Column("Iata"), MaxLength(50)]
		public string Iata { get; set; }

		[Column("Email"), MaxLength(50)]
		public string Email { get; set; }

		[Column("Telephone"), MaxLength(50)]
		public string Telephone { get; set; }

		[Column("CreatedBy"), ReadOnly, MaxLength(50)]
		public string CreatedBy { get; set; }

		[Column("CreatedTime"), ReadOnly, MaxLength]
		public System.DateTime CreatedTime { get; set; }

		[Column("ModifiedBy"), ReadOnly, MaxLength(50)]
		public string ModifiedBy { get; set; }

		[Column("ModifiedTime"), ReadOnly]
		public System.DateTime? ModifiedTime { get; set; }
	}
}

using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using OpenLibrary.Extension;
using OpenLibrary.Mvc.Helper;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenLibrary.Annotation;
using OpenLibrary.Utility;

namespace OpenLibrary.Test
{
	class Program
	{
// ReSharper disable UnusedParameter.Local
		static void Main(string[] args)
// ReSharper restore UnusedParameter.Local
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
			/*var activeEmployee = Sql.Query<Employee>("SP_Employee_select", new { Name = "%robert", creator = "admin" }, true);
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
			var employees = new[]
			{
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
			*/
			/*
			string sql = @"
select 
	cs.String ConnectionStringName,
	job.*
from {0} job
left join {1} cs on job.ConnectionStringId = cs.Id
where 
	job.IsActive = 1 
order by job.Priority asc";
			//cari job schedule yg masih aktif
			var jobs = Sql.Query<JobSchedule>(string.Format(sql, Sql.Table<JobSchedule>(), Sql.Table<ConnectionString>()));
			foreach (var job in jobs)
			{
				var now = System.DateTime.Now;
				System.DateTime lastJobTime;
				switch (job.PeriodType)
				{
					case PeriodType.Second:
						lastJobTime = now.AddSeconds(-job.PeriodIncrement);
						break;
					case PeriodType.Minute:
						lastJobTime = now.AddMinutes(-job.PeriodIncrement);
						break;
					case PeriodType.Hour:
						lastJobTime = now.AddHours(-job.PeriodIncrement);
						break;
					case PeriodType.Day:
						lastJobTime = now.AddDays(-job.PeriodIncrement);
						break;
					case PeriodType.Month:
						lastJobTime = now.AddMonths(-job.PeriodIncrement);
						break;
					case PeriodType.Year:
						lastJobTime = now.AddYears(-job.PeriodIncrement);
						break;
					default:
						lastJobTime = now;
						break;
				}
				//eventLog.WriteEntry(string.Format("Looking for a job....found {0} active jobs", jobs.Count));
				var lastJob =
					Sql.Query<JobLog>(
						string.Format("select top 1 * from {0} where JobScheduleId = @Id and ProcessStart >= @lastJobTime",
									  Sql.Table<JobLog>()),
						new { job.Id, lastJobTime }).FirstOrDefault();
				if (lastJob != null)
					continue;
				var log = new JobLog
				{
					FunctionName = job.FunctionName,
					JobScheduleId = job.Id,
					Name = job.Name,
					ProcessStart = now,
					StatusType = StatusType.Running
				};
				Sql.Insert(log);
			}
			*/
			string cnn = "metadata=res://*/Database.csdl|res://*/Database.ssdl|res://*/Database.msl;provider=System.Data.SqlClient;provider connection string='Data Source=.;initial catalog=CRM;user id=crm;password=mrc;multipleactiveresultsets=True;App=EntityFramework'satu dua";
			var cocok = Regex.Match(cnn, @"(?:provider connection string=)(?<quot>\&quot\;|')(?<connection>.+)\k<quot>", RegexOptions.IgnoreCase);
			if (cocok.Success)
			{
				Console.WriteLine(cocok.Groups["connection"].Value);
			}
			Console.WriteLine("nilai monthly ke enum => {0}", 5.To<PeriodType>());
			Console.WriteLine("nilai monthly dari enum => {0}", PeriodType.Month.To<int>());
			Console.WriteLine("nilai monthly dari string => {0}", "Month".To<PeriodType>());
			string connectionString = "LDAP://adfs.abacus-ind.co.id/CN=Users,DC=abacus-ind,DC=co,DC=id";
			var matchs = Regex.Matches(connectionString, @"(?:DC=)(?<domain>[\w\-]+)", RegexOptions.IgnoreCase);
			var domainList = (from Match match in matchs
							  select match.Groups["domain"].Value).ToList();
			Console.WriteLine("domain -> {0}", string.Join(".", domainList));
			var test = new DokuRedirect
			{
				Amount = 23802803.00m,
				OrderId = "78273972983",
				Password = "kajskldjlf",
				ResponseCode = DokuResponseCode.NotifyFailed,
				PaymentChannel = DokuPaymentChannelType.VisaMasterCard,
				MemberId = "28390-23802-233",
				RequestUuid = System.Guid.NewGuid().ToString()
			};
			var test2 = test.Map<PaymentConfirmation>();
			var test3 = new PaymentGateway
			{
				Respond = 0,
				Status = "Success",
				Description = "Insert Successfull"
			};
			string xml3 = test3.ToXml();
			string xml2 = @"<pg>
	<respond>03</respond>
	<status>Success</status>
	<description>Insert Successfull</description>
</pg>";
			var serializer = new XmlSerializer(typeof(PaymentGateway));
			var reader = new System.IO.StringReader(xml2);
			var test4 = serializer.Deserialize(reader) as PaymentGateway;
			Console.WriteLine(test.Describe());
			Console.WriteLine(test.Describe(new[] { typeof(NotMappedAttribute), typeof(Annotation.ReadOnlyAttribute) }));
			Console.WriteLine(test.Describe(m => new { m.Amount, m.RequestUuid }));
			//var test4 = xml2.FromXml<PaymentGateway>();
			Console.ReadLine();
		}
	}

	#region Type Helper

	[XmlRoot("pg")]
	public class PaymentGateway
	{
		[XmlElement("respond")]
		public int Respond { get; set; }

		[XmlElement("status")]
		public string Status { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }
	}

	public class DokuRedirect
	{
		/// <summary>
		/// Total Amount
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Transaction ID from merchant
		/// </summary>
		public string OrderId { get; set; }

		/// <summary>
		/// Hashed key combination encryption (use SHA1 method). 
		/// The hashed key generated from combining these parameters value in this order:
		/// AMOUNT+MALLID+&lt;shared key&gt;+TRANSIDMERCHANT+RESULTMSG+VERIFYSTATUS
		/// </summary>
		[Annotation.ReadOnly]
		public string Password { get; set; }

		/// <summary>
		/// 0000: success, others failed
		/// </summary>
		public DokuResponseCode ResponseCode { get; set; }

		/// <summary>
		/// See payment channel code list
		/// </summary>
		[NotMapped]
		public DokuPaymentChannelType PaymentChannel { get; set; }

		/// <summary>
		/// SESSIONID
		/// </summary>
		public string RequestUuid { get; set; }

		/// <summary>
		/// Virtual Account identifier for VA transaction
		/// </summary>
		public string MemberId { get; set; }
	}

	/// <summary>
	/// Doku Payment Channel
	/// </summary>
	public enum DokuPaymentChannelType
	{
		/// <summary>
		/// Visa or Master Card
		/// </summary>
		[Description("Visa/Master Card")]
		VisaMasterCard = 1
	}

	/// <summary>
	/// Response Code from Doku
	/// </summary>
	public enum DokuResponseCode
	{
		/// <summary>
		/// Successful approval
		/// </summary>
		[Description("Successful approval")]
		Success = 0,

		/// <summary>
		/// Undefined error
		/// </summary>
		[Description("Undefined error")]
		UndefinedError = 5555,

		/// <summary>
		/// Payment channel not registered
		/// </summary>
		[Description("Payment channel not registered")]
		PaymentChannelNotRegistered = 5501,

		/// <summary>
		/// Merchant is disabled
		/// </summary>
		[Description("Merchant is disabled")]
		MerchantIsDisabled = 5502,

		/// <summary>
		/// Maximum attempt 3 times
		/// </summary>
		[Description("Maximum attempt 3 times")]
		MaximumAttempt = 5503,

		/// <summary>
		/// Words not match
		/// </summary>
		[Description("Words not match")]
		WordsNotMatch = 5504,

		/// <summary>
		/// Invalid parameter
		/// </summary>
		[Description("Invalid parameter")]
		InvalidParameter = 5505,

		/// <summary>
		/// Notify failed
		/// </summary>
		[Description("Notify failed")]
		NotifyFailed = 5506,

		/// <summary>
		/// Invalid parameter detected / Customer click cancel process
		/// </summary>
		[Description("Invalid parameter detected / Customer click cancel process")]
		InvalidParameterDetectedOrCustomerCancel = 5507,

		/// <summary>
		/// Re-enter transaction
		/// </summary>
		[Description("Re-enter transaction")]
		ReEnterTransaction = 5508,

		/// <summary>
		/// Payment code already expired
		/// </summary>
		[Description("Payment code already expired")]
		PaymentCodeExpired = 5509,

		/// <summary>
		/// Cancel by Customer
		/// </summary>
		[Description("Cancel by Customer")]
		CancelledByCustomer = 5510,

		/// <summary>
		/// Not an error, payment code has not been paid by Customer
		/// </summary>
		[Description("Not an error, payment code has not been paid by Customer")]
		PaymentCodeNotPaidByCustomer = 5511
	}

	/// <summary>
	/// Konfirmasi tambahan setelah notifikasi
	/// </summary>
	public class PaymentConfirmation
	{
		/// <summary>
		/// ID dari request yg bersangkutan
		/// </summary>
		public int? PaymentRequestId { get; set; }

		/// <summary>
		/// nominal transaksi
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Nomor transaksi
		/// </summary>
		public string OrderId { get; set; }

		/// <summary>
		/// Word secret antara merchant dengan payment gateway
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Kode response dari doku -&gt; 0000: success, other failed
		/// </summary>
		public DokuResponseCode? ResponseCode { get; set; }

		/// <summary>
		/// See payment channel code list
		/// </summary>
		public DokuPaymentChannelType? PaymentChannel { get; set; }

		/// <summary>
		/// Request identifier. Id unik untuk mengidentifikasi message tersebut dengan message lainnya.
		/// Pada Doku diberi nama SessionID
		/// </summary>
		public string RequestUuid { get; set; }

		/// <summary>
		/// Virtual Account identifier for VA transaction.
		/// Pada doku diberi nama PaymentCode
		/// </summary>
		public string MemberId { get; set; }

		/// <summary>
		/// Tanggal diinput ke database
		/// </summary>
		public System.DateTime CreatedTime { get; set; }

		/// <summary>
		/// IP yg mengirim request
		/// </summary>
		public string CreatorIp { get; set; }

		/// <summary>
		/// Raw request
		/// </summary>
		public string RawRequest { get; set; }

		/// <summary>
		/// Raw response
		/// </summary>
		public string RawResponse { get; set; }
	}

	[Table("ConnectionString")]
	public class ConnectionString : BaseMaster<int>
	{
		[Required, Column("Name")]
		public string Name { get; set; }

		[Required, Column("String")]
		public string String { get; set; }

		[Column("Description")]
		public string Description { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	[Table("JobSchedule")]
	public class JobSchedule : BaseMaster<int>
	{
		[Column("ConnectionStringId")]
		public int? ConnectionStringId { get; set; }

		[Annotation.ReadOnly, Column("ConnectionStringName")]
		public string ConnectionString { get; set; }

		[Required, Column("Name")]
		public string Name { get; set; }

		[Column("IsActive")]
		public bool IsActive { get; set; }

		[Column("Description")]
		public string Description { get; set; }

		[Required, Column("FunctionName")]
		public string FunctionName { get; set; }

		[Column("Priority")]
		public int Priority { get; set; }

		[Column("PeriodType")]
		public int IntervalType { get; set; }

		[NotMapped]
		public PeriodType PeriodType
		{
			get { return (PeriodType)IntervalType; }
			set { IntervalType = (int)value; }
		}

		[Column("PeriodIncrement")]
		public int PeriodIncrement { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	[Table("JobLog")]
	public class JobLog : BaseEntity<int>
	{
		[Column("JobScheduleId")]
		public int? JobScheduleId { get; set; }

		[Column("Name")]
		[Required]
		public string Name { get; set; }

		[Column("ProcessStart")]
		public System.DateTime ProcessStart { get; set; }

		[Column("ProcessEnd")]
		public System.DateTime? ProcessEnd { get; set; }

		[Column("Status")]
		public int? Status { get; set; }

		[NotMapped]
		public StatusType? StatusType
		{
			get { return (StatusType?)Status; }
			set { Status = (int?)value; }
		}

		[Required, Column("FunctionName")]
		public string FunctionName { get; set; }

		[Column("Message")]
		public string Message { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, FunctionName);
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
		public DateTime? BirthDate { get; set; }

		[Column("CreatedTime")]
		public DateTime CreatedTime { get; set; }

		[Column("CreatedBy")]
		[MaxLength(50)]
		[Required]
		public string CreatedBy { get; set; }

		[Column("ModifiedTime")]
		public DateTime? ModifiedTime { get; set; }

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

		[Column("created_datetime"), Annotation.ReadOnly]
		public DateTime? RegistrationTime { get; set; }

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

	class TestAttribute : Attribute
	{
		public TestAttribute(int data)
		{

		}

		public string Test<T>(T data) where T : class { return null; }
	}

	[Table("Mst_Agent")]
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

		[Column("CreatedBy"), Annotation.ReadOnly, MaxLength(50)]
		public string CreatedBy { get; set; }

		[Column("CreatedTime"), Annotation.ReadOnly, MaxLength]
		public DateTime CreatedTime { get; set; }

		[Column("ModifiedBy"), Annotation.ReadOnly, MaxLength(50)]
		public string ModifiedBy { get; set; }

		[Column("ModifiedTime"), Annotation.ReadOnly]
		public DateTime? ModifiedTime { get; set; }
	}

	public abstract class BaseEntity<T>
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public T Id { get; set; }
	}

	public abstract class BaseMaster<T> : BaseEntity<T>
	{
		[Column("CreatedTime")]
		public System.DateTime CreatedTime { get; set; }

		[Required, Column("CreatedBy"), MaxLength(50)]
		public string CreatedBy { get; set; }

		[Column("ModifiedTime")]
		public System.DateTime? ModifiedTime { get; set; }

		[Column("ModifiedBy"), MaxLength(50)]
		public string ModifiedBy { get; set; }
	}
	
	public enum StatusType
	{
		Failed = 0,
		Running = 1,
		Success = 2
	}

	public enum PeriodType
	{
		Second = 1,

		Minute = 2,

		Hour = 3,

		Day = 4,

		Month = 5,

		Year = 6
	}

	#endregion
}

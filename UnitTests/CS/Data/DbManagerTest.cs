using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using BLToolkit.Data.DataProvider;
using NUnit.Framework;

using BLToolkit.Data;
using BLToolkit.EditableObjects;
using BLToolkit.Mapping;
using BLToolkit.Reflection;

namespace Data
{
	[TestFixture]
	public class DbManagerTest
	{
		public enum Gender
		{
			[MapValue("F")] Female,
			[MapValue("M")] Male,
			[MapValue("U")] Unknown,
			[MapValue("O")] Other
		}

		public class Person
		{
			[MapField("PersonID")]
			public int    ID;
			public string FirstName;
			public string MiddleName;
			public string LastName;
			public Gender Gender;
		}

		public class DataTypeTest
		{
			[MapField("DataTypeID")]
			public int       ID;
			[MapIgnore(false)]
			public Byte[]    Binary_;
			public Boolean   Boolean_;
			public Byte      Byte_;
			[MapIgnore(false)]
			public Byte[]    Bytes_;
			public DateTime  DateTime_;
			public Decimal   Decimal_;
			public Double    Double_;
			public Guid      Guid_;
			public Int16     Int16_;
			public Int32     Int32_;
			public Int64     Int64_;
			public Decimal   Money_;
			public Single    Single_;
			public String    String_;

			public Char      Char_;
			public SByte     SByte_;
			[MapIgnore(false)]
			public Stream    Stream_;
			public UInt16    UInt16_;
			public UInt32    UInt32_;
			public UInt64    UInt64_;
			[MapIgnore]
			public XmlReader Xml_;
			[MapField("Xml_")]
			public XmlDocument XmlDoc_;
		}

		public class DataTypeSqlTest
		{
			[MapField("DataTypeID")]
			public int ID;
			public SqlBinary   Binary_;
			public SqlBoolean  Boolean_;
			public SqlByte     Byte_;
			public SqlDateTime DateTime_;
			public SqlDecimal  Decimal_;
			public SqlDouble   Double_;
			public SqlGuid     Guid_;
			public SqlInt16    Int16_;
			public SqlInt32    Int32_;
			public SqlInt64    Int64_;
			public SqlMoney    Money_;
			public SqlSingle   Single_;
			public SqlString   String_;
			[MapIgnore(false)]
			public SqlBytes    Bytes_;
			[MapIgnore(false)]
			public SqlChars    Chars_;
			[MapIgnore(false)]
			public SqlXml      Xml_;
		}

		[Test]
		public void ExecuteList1()
		{
			using (DbManager db = new DbManager())
			{
				ArrayList list = db
					.SetCommand("SELECT * FROM Person")
					.ExecuteList(typeof(Person));
				
				Assert.IsNotEmpty(list);
			}
		}

		[Test]
		public void ExecuteList2()
		{
			using (DbManager db = new DbManager())
			{
				IList list = db
					.SetCommand("SELECT * FROM Person")
					.ExecuteList(new EditableArrayList(typeof(Person)), typeof(Person));
				
				Assert.IsNotEmpty(list);
			}
		}

		[Test]
		public void ExecuteObject()
		{
			using (DbManager db = new DbManager())
			{
				Person p = (Person)db
					.SetCommand("SELECT * FROM Person WHERE PersonID = " + db.DataProvider.Convert("id", ConvertType.NameToQueryParameter),
					db.Parameter("id", 1))
					.ExecuteObject(typeof(Person));

				TypeAccessor.WriteConsole(p);
			}
		}

		[Test]
		public void ExecuteObject2()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeTest dt = (DataTypeTest)db
					.SetCommand("SELECT * FROM DataTypeTest WHERE DataTypeID = " + db.DataProvider.Convert("id", ConvertType.NameToQueryParameter),
					db.Parameter("id", 2))
					.ExecuteObject(typeof(DataTypeTest));

				TypeAccessor.WriteConsole(dt);
			}
		}

#if !ORACLE
		[Test]
#endif
		public void ExecuteObject2Sql()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeSqlTest dt = (DataTypeSqlTest)db
					.SetCommand("SELECT * FROM DataTypeTest WHERE DataTypeID = " + db.DataProvider.Convert("id", ConvertType.NameToQueryParameter),
					db.Parameter("id", 2))
					.ExecuteObject(typeof(DataTypeSqlTest));

				TypeAccessor.WriteConsole(dt);
			}
		}

#if MSSQL
		[Test]
#endif
		public void NewConnection()
		{
			string connectionString = "Server=.;Database=BLToolkitData;Integrated Security=SSPI";

			using (DbManager db = new DbManager(new SqlConnection(connectionString)))
			{
				db
					.SetSpCommand ("Person_SelectByName",
						db.Parameter("@firstName", "John"),
						db.Parameter("@lastName",  "Pupkin"))
					.ExecuteScalar();
			}
		}

		public class OutRefTest
		{
			public int    ID             = 5;
			public int    outputID;
			public int    inputOutputID  = 10;
			public string str            = "5";
			public string outputStr;
			public string inputOutputStr = "10";
		}

#if !ACCESS
		[Test]
		public void MapOutput()
		{
			OutRefTest o = new OutRefTest();

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("OutRefTest", db.CreateParameters(o,
						new string[] {      "outputID",      "outputStr" },
						new string[] { "inputOutputID", "inputOutputStr" },
						null))
					.ExecuteNonQuery(o);
			}

			Assert.AreEqual(5,     o.outputID);
			Assert.AreEqual(15,    o.inputOutputID);
			Assert.AreEqual("5",   o.outputStr);
			Assert.AreEqual("510", o.inputOutputStr);
		}

		public class PeturnParameter
		{
			public int Value;
		}

		[Test]
		public void MapReturnValue()
		{
			PeturnParameter e = new PeturnParameter();

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("Scalar_ReturnParameter")
					.ExecuteNonQuery("Value", e);
			}

			Assert.AreEqual(12345, e.Value);
		}

		[Test]
		public void InsertAndMapBack()
		{
			Person e = new Person();
			e.FirstName = "Crazy";
			e.LastName  = "Frog";
			e.Gender    =  Gender.Other;

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("Person_Insert", db.CreateParameters(e, new string[] { "PersonID" }, null, null))
					.ExecuteObject(e);

				Assert.IsTrue(e.ID > 0);

				db
					.SetSpCommand("Person_Delete", db.CreateParameters(e))
					.ExecuteNonQuery();
			}
		}

		[Test]
		public void MapDataRow()
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("ID",             typeof(int));
			dataTable.Columns.Add("outputID",       typeof(int));
			dataTable.Columns.Add("inputOutputID",  typeof(int));
			dataTable.Columns.Add("str",            typeof(string));
			dataTable.Columns.Add("outputStr",      typeof(string));
			dataTable.Columns.Add("inputOutputStr", typeof(string));

			DataRow dataRow = dataTable.Rows.Add(new object[]{5, 0, 10, "5", null, "10"});

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("OutRefTest", db.CreateParameters(dataRow,
						new string[] {      "outputID",      "outputStr" },
						new string[] { "inputOutputID", "inputOutputStr" },
						null))
					.ExecuteNonQuery(dataRow);
			}

			Assert.AreEqual(5,     dataRow["outputID"]);
			Assert.AreEqual(15,    dataRow["inputOutputID"]);
			Assert.AreEqual("5",   dataRow["outputStr"]);
			Assert.AreEqual("510", dataRow["inputOutputStr"]);
		}
#endif
		
		[Test]
		public void CreateParametersTest()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeTest dt = new DataTypeTest();
				
				dt.ID        = 12345;
				dt.Binary_   = new byte[2] {1, 2};
				dt.Boolean_  = true;
				dt.Byte_     = 250;
				dt.Bytes_    = new byte[2] {2, 1};
				dt.DateTime_ = DateTime.Now;
				dt.Decimal_  = 9876543210.0m;
				dt.Double_   = 12345.67890;
				dt.Guid_     = Guid.Empty;
				dt.Int16_    = 12345;
				dt.Int32_    = 1234567890;
				dt.Int64_    = 1234567890123456789;
				dt.Money_    = 99876543210.0m;
				dt.Single_   = 1234.0f;
				dt.String_   = "Crazy Frog";

				dt.Char_     = 'F';
				dt.SByte_    = 123;
				dt.Stream_   = new MemoryStream(5);
				dt.UInt16_   = 65432;
				dt.UInt32_   = 4000000000;
				dt.UInt64_   = 12345678901234567890;
				dt.Xml_      = new XmlTextReader(new StringReader("<xml/>"));
				dt.XmlDoc_   = new XmlDocument(); dt.XmlDoc_.LoadXml("<xmldoc/>");

				IDbDataParameter[] parameters = db.CreateParameters(dt);

				foreach (IDbDataParameter parameter in parameters)
				{
					Console.WriteLine("{0}: '{1}'", parameter.ParameterName, parameter.Value);
				}
			}
		}

#if!ORACLE
		[Test]
#endif
		public void CreateParametersSqlTest()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeSqlTest dt = new DataTypeSqlTest();
				
				dt.ID        = 12345;
				dt.Binary_   = new SqlBinary(new byte[2] {1, 2});
				dt.Boolean_  = new SqlBoolean(1);
				dt.Byte_     = new SqlByte(250);
				dt.DateTime_ = new SqlDateTime(DateTime.Now);
				dt.Decimal_  = new SqlDecimal(9876543210.0m);
				dt.Double_   = new SqlDouble(12345.67890);
				dt.Guid_     = new SqlGuid(Guid.Empty);
				dt.Int16_    = new SqlInt16(12345);
				dt.Int32_    = new SqlInt32(1234567890);
				dt.Int64_    = new SqlInt64(1234567890123456789);
				dt.Money_    = new SqlMoney(99876543210.0m);
				dt.Single_   = new SqlSingle(1234.0f);
				dt.String_   = new SqlString("Crazy Frog");

				dt.Bytes_    = new SqlBytes(new byte[2] {2, 1});
				dt.Chars_    = new SqlChars(new char[2] {'B', 'L'});
				dt.Xml_      = new SqlXml(new XmlTextReader(new StringReader("<xml/>")));

				IDbDataParameter[] parameters = db.CreateParameters(dt);

				foreach (IDbDataParameter parameter in parameters)
				{
					Console.WriteLine("{0}: '{1}'", parameter.ParameterName, parameter.Value);
				}
			}
		}

		public struct DBInfo
		{
			public DateTime TimeValue;
		}
		
		[Test]
		public void CreateParametersStructTest()
		{
			DBInfo dbInfo = new DBInfo();
			dbInfo.TimeValue = DateTime.Now;
			
			using (DbManager db = new DbManager())
			{
				IDbDataParameter[] parameters = db.CreateParameters(dbInfo);
				
				Assert.IsNotNull(parameters);
				Assert.AreEqual(1, parameters.Length);
				Assert.AreEqual(dbInfo.TimeValue, parameters[0].Value);
				
			}
		}

		public class FirstPart
		{
			public string FirstName;
		}

		public class SecondPart
		{
			public string LastName;
		}

		[Test]
		public void CreateManyParametersTest()
		{
			FirstPart  f = new FirstPart();
			SecondPart s = new SecondPart();

			f.FirstName = "John";
			s.LastName = "Pupkin";

			using (DbManager db = new DbManager())
			{
				Person p = (Person)db
					.SetSpCommand ("Person_SelectByName", db.CreateParameters(f), db.CreateParameters(s))
					.ExecuteObject(typeof(Person));
				
				Assert.IsNotNull(p);
			}
		}
	}
}

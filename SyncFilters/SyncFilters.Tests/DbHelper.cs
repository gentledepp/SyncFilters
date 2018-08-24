using System;
using System.Data.SqlClient;

namespace SyncFilters.Tests
{
    public class DbHelper
    {
        /// <summary>
        /// Returns the database server to be used in the untittests - note that this is the connection to appveyor SQL Server 2016 instance!
        /// see: https://www.appveyor.com/docs/services-databases/#mysql
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public static String GetDatabaseConnectionString(string dbName)
        {

            // check if we are running on appveyor or not
            string isOnAppVeyor = Environment.GetEnvironmentVariable("APPVEYOR");

            if (!String.IsNullOrEmpty(isOnAppVeyor) && isOnAppVeyor.ToLowerInvariant() == "true")
                return $@"Server=(local)\SQL2016;Database={dbName};UID=sa;PWD=Password12!";
            else
                return $@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=true;";

        }

        /// <summary>
        /// Generate a database
        /// </summary>
        public void CreateDatabase(string dbName, bool recreateDb = true)
        {
            using (var masterConnection = new SqlConnection(GetDatabaseConnectionString("master")))
            {
                masterConnection.Open();
                var cmdDb = new SqlCommand(GetCreationDBScript(dbName, recreateDb), masterConnection);
                cmdDb.ExecuteNonQuery();
                masterConnection.Close();
            }
        }


        /// <summary>
        /// Delete a database
        /// </summary>
        public void DeleteDatabase(string dbName)
        {
            using (var masterConnection = new SqlConnection(GetDatabaseConnectionString("master")))
            {
                masterConnection.Open();
                var cmdDb = new SqlCommand(GetDeleteDatabaseScript(dbName), masterConnection);
                cmdDb.ExecuteNonQuery();
                masterConnection.Close();
            }
        }

        public void ExecuteScript(string dbName, string script)
        {
            using (var connection = new SqlConnection(GetDatabaseConnectionString(dbName)))
            {
                connection.Open();
                var cmdDb = new SqlCommand(script, connection);
                cmdDb.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void ExecuteScripts(string dbName, string scripts)
        {
            var scriptArray = scripts.Split("\r\nGO\r\n");

            using (var connection = new SqlConnection(GetDatabaseConnectionString(dbName)))
            {
                connection.Open();
                foreach (var script in scriptArray)
                {
                    var cmdDb = new SqlCommand(script, connection);
                    cmdDb.ExecuteNonQuery();
                }
                connection.Close();
            }
        }


        /// <summary>
        /// Gets the Create or Re-create a database script text
        /// </summary>
        private string GetCreationDBScript(string dbName, Boolean recreateDb = true)
        {
            if (recreateDb)
                return $@"if (exists (Select * from sys.databases where name = '{dbName}'))
                    begin
	                    alter database [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	                    drop database {dbName}
                    end
                    Create database {dbName}";
            else
                return $@"if not (exists (Select * from sys.databases where name = '{dbName}')) 
                          Create database {dbName}";

        }

        private string GetDeleteDatabaseScript(string dbName)
        {
            return $@"if (exists (Select * from sys.databases where name = '{dbName}'))
                    begin
	                    alter database [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	                    drop database {dbName}
                    end";
        }


    }
}

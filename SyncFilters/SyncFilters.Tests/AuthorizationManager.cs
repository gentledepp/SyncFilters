using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;

namespace SyncFilters.Tests
{
    public class AuthorizationManager
    {
        private string _databaseName;
        private DbHelper _dbHelper;


        public AuthorizationManager(string databaseName)
        {
            _databaseName = databaseName;
            _dbHelper = new DbHelper();
        }

        public void AddGroupMember(int groupId, int userId, int tenantId)
        {
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                var cmd = new SqlCommand("[dbo].[AddEdgeWithSpaceSavingsPrincipal] @StartVertexId, @EndVertexId, @StartPrincipalType, @EndPrincipalType, @TenantId", connection);
                cmd.Parameters.Add("@StartVertexId", SqlDbType.BigInt).Value = userId;
                cmd.Parameters.Add("@EndVertexId", SqlDbType.BigInt).Value = groupId;
                cmd.Parameters.Add("@StartPrincipalType", SqlDbType.TinyInt).Value = 1;
                cmd.Parameters.Add("@EndPrincipalType", SqlDbType.TinyInt).Value = 2;
                cmd.Parameters.Add("@TenantId", SqlDbType.Int).Value = tenantId;

                cmd.ExecuteNonQuery();
                //connection.Execute(@"[dbo].[AddEdgeWithSpaceSavingsPrincipal] @StartVertexId, @EndVertexId, @StartPrincipalType, @EndPrincipalType, @TenantId",
                //    new
                //    {
                //        StartVertexId = userId,
                //        EndVertexId = groupId,
                //        StartPrincipalType = 1,
                //        EndPrincipalType = 2,
                //        TenantId = tenantId
                //    });

                connection.Close();
            }
        }

        public void RemoveGroupMember(int groupId, int userId, int tenantId)
        {
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                var cmd = new SqlCommand("[dbo].[RemoveEdgeWithSpaceSavingsPrincipal] @StartVertexId, @EndVertexId, @PrincipalType, @TenantId", connection);
                cmd.Parameters.Add("@StartVertexId", SqlDbType.BigInt).Value = userId;
                cmd.Parameters.Add("@EndVertexId", SqlDbType.BigInt).Value = groupId;
                cmd.Parameters.Add("@PrincipalType", SqlDbType.TinyInt).Value = 1;
                cmd.Parameters.Add("@TenantId", SqlDbType.Int).Value = tenantId;

                cmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}

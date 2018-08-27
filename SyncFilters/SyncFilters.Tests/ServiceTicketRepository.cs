using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace SyncFilters.Tests
{
    public class ServiceTicket
    {
        public Guid ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int StatusValue { get; set; }
        public int EscalationLevel { get; set; }
        public DateTime? Opened { get; set; }
        public DateTime? Closed { get; set; }
        public int TenantId { get; set; }
    }

    public class ServiceTicketRepository
    {
        private string _databaseName;
        private DbHelper _dbHelper;

        public ServiceTicketRepository(string databaseName)
        {
            _databaseName = databaseName;
            _dbHelper = new DbHelper();
        }

        public IEnumerable<ServiceTicket> GetServiceTickets()
        {
            IEnumerable<ServiceTicket> principals;

            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                principals = connection.Query<ServiceTicket>("select * from ServiceTickets").ToList();

                connection.Close();
            }

            return principals;
        }

        public void Update(ServiceTicket ticket)
        {
            var props = typeof(ServiceTicket).GetProperties()
                .Where(p => !string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase)).ToList();

            var setter = string.Join(", ", props.Select(p => $"{p.Name}=@{p.Name}"));
            var query = $"Update ServiceTickets set {setter} where ID = @ID";
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();
                connection.Execute(query, ticket);
                connection.Close();
            }
        }

        public void Delete(ServiceTicket ticket)
        {
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();
                connection.Execute("delete from ServiceTickets where ID = @ID", ticket);
                connection.Close();
            }
        }
    }
}

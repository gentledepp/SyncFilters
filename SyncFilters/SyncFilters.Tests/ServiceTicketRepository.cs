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
    }
}

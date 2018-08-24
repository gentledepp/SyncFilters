using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace SyncFilters.Tests
{
    [Flags]
    public enum SyncReasons : int
    {
        None = 0,
        BelongsToTenant = 1,
        IsAssignedUser = 2,
    }

    public class SyncFilter
    {
        public long Id { get; set; }
        public Guid ItemId { get; set; }
        public byte[] Modified { get; set; }
        public int AppliedRules { get; set; }
        public long UserId { get; set; }
        public int TenantId { get; set; }
        public DateTime DeletionTime { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class ServiceTicketsSyncFilterManager
    {
        private string _databaseName;
        private DbHelper _dbHelper;
        
        public ServiceTicketsSyncFilterManager(string databaseName)
        {
            _databaseName = databaseName;
            _dbHelper = new DbHelper();
        }

        public void AddFilter(Guid itemId, int userId, SyncReasons reason, int tenantId)
        {
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                connection.Execute(@"merge into ServiceTickets_syncfilter as t
using (values(@Itemid, @Reason, @UserId, @TenantId)) as s(ItemId,AppliedRules,UserId,TenantId)
on t.itemId = s.ItemId and t.UserId = s.UserId and t.tenantId = s.TenantId
when matched then 
	update set t.appliedrules |= s.appliedrules,
			-- whenever the appliedrules value changes from >0 to 0 or vice versa, update the date
			t.modified = (case when (t.appliedrules = 0 and t.appliedrules|s.appliedrules > 0) 
						then @@DBTS+1 else t.modified end),
			-- whenever the appliedrules value changes to 0, setup the deleted datetime (so we can delete all ""deleted"" stuff after e.g. 3 months)
			t.deletiontime = '1753-01-01'
when not matched then
	insert (ItemId,Modified,AppliedRules,UserId,Tenantid,DeletionTime)
	values (s.ItemId,@@DBTS+1,s.AppliedRules,s.UserId,s.TenantId, '1753-01-01');",
                    new {ItemId = itemId, Reason = (int) reason, UserId = userId, TenantId = tenantId});

                connection.Close();
            }
        }

        public void RemoveFilter(Guid itemId, int userId, SyncReasons reason, int tenantId)
        {
            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                connection.Execute(@"merge into ServiceTickets_syncfilter as t
using (values(@Itemid, @Reason, @UserId, @TenantId)) as s(ItemId,AppliedRules,UserId,TenantId)
on t.itemId = s.ItemId and t.UserId = s.UserId and t.tenantId = s.TenantId
when matched then 
	update set t.appliedrules = (case when t.appliedrules & s.appliedrules = s.appliedrules then t.appliedrules^s.appliedrules else t.appliedrules end),
			-- whenever the appliedrules value changes from >0 to 0 or vice versa, update the date
			t.modified = (case when ((t.appliedrules > 0 and t.appliedrules^s.appliedrules = 0)
									or (t.appliedrules = 0 and t.appliedrules^s.appliedrules > 0)) 
						then @@DBTS+1 else t.modified end),
			-- whenever the appliedrules value changes to 0, setup the deleted datetime (so we can delete all ""deleted"" stuff after e.g. 3 months)
			t.deletiontime = (case when (t.appliedrules > 0 and t.appliedrules^s.appliedrules = 0) 
						then getutcdate() else '1753-01-01' end)
when not matched then
	insert (ItemId,Modified,AppliedRules,UserId,Tenantid,DeletionTime)
	values (s.ItemId,@@DBTS+1,s.AppliedRules,s.UserId,s.TenantId, getUtcDate());",
                    new { ItemId = itemId, Reason = (int)reason, UserId = userId, TenantId = tenantId });

                connection.Close();
            }
        }


        public IEnumerable<SyncFilter> GetSyncFilters()
        {
            IEnumerable<SyncFilter> principals;

            using (var connection = new SqlConnection(DbHelper.GetDatabaseConnectionString(_databaseName)))
            {
                connection.Open();

                principals = connection.Query<SyncFilter>("select * from ServiceTickets_syncfilter").ToList();

                connection.Close();
            }

            return principals;
        }
    }
}

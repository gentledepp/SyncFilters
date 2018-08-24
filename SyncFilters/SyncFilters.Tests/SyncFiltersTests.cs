using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Filter;
using Dotmim.Sync.SqlServer;
using Xunit;

namespace SyncFilters.Tests
{
    public class SyncFilterClassFixture
    {
        private string createTableScript =
        $@"if (not exists (select * from sys.tables where name = 'ServiceTickets'))
            begin
                CREATE TABLE [ServiceTickets](
	            [ID] [uniqueidentifier] NOT NULL,
	            [Title] [nvarchar](max) NOT NULL,
	            [Description] [nvarchar](max) NULL,
	            [StatusValue] [int] NOT NULL,
	            [EscalationLevel] [int] NOT NULL,
	            [Opened] [datetime] NULL,
	            [Closed] [datetime] NULL,
	            [CustomerID] [int] NULL,
                CONSTRAINT [PK_ServiceTickets] PRIMARY KEY CLUSTERED ( [ID] ASC ));
            end";

        private string datas =
        $@"
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [CustomerID]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
          ";

        private DbHelper helperDb = new DbHelper();
        private string serverDbName = "Test_SyncFilter_ServerDB";
        private string client1DbName = "Test_SyncFilter_Client1";
        private string client2DbName = "Test_SyncFilter_Client2";
        private string client3DbName = "Test_SyncFilter_Client3";

        public String ServerConnectionString => DbHelper.GetDatabaseConnectionString(serverDbName);
        public String Client1ConnectionString => DbHelper.GetDatabaseConnectionString(client1DbName);
        public String Client2ConnectionString => DbHelper.GetDatabaseConnectionString(client2DbName);
        public String Client3ConnectionString => DbHelper.GetDatabaseConnectionString(client3DbName);

        public void Provision()
        {

            // create databases
            helperDb.CreateDatabase(serverDbName);
            helperDb.CreateDatabase(client1DbName);
            helperDb.CreateDatabase(client2DbName);
            helperDb.CreateDatabase(client3DbName);

            // create table
            helperDb.ExecuteScript(serverDbName, createTableScript);

            // insert table
            helperDb.ExecuteScript(serverDbName, datas);

        }

        public void Deprovision()
        {
            helperDb.DeleteDatabase(serverDbName);
            helperDb.DeleteDatabase(client1DbName);
            helperDb.DeleteDatabase(client2DbName);
            helperDb.DeleteDatabase(client3DbName);
        }
    }

    [Collection("Sync")]
    public class SyncFilterFixture : IClassFixture<SyncFilterClassFixture>, IDisposable
    {
        SyncFilterClassFixture _fixture;

        public SyncFilterFixture(SyncFilterClassFixture fixture)
        {
            this._fixture = fixture;
            this._fixture.Provision();
        }

        #region RandomlyFilteredStoredProcedure

        const string randomlyFilteredStoredProcedure =
            @"ALTER PROCEDURE [dbo].[ServiceTickets_CustomerIDRandom__selectchanges]
	@sync_min_timestamp bigint,
	@sync_scope_id uniqueidentifier,
	@sync_scope_is_new bit,
	@sync_scope_is_reinit bit,
	@CustomerID int,
	@Random int
AS
BEGIN
SELECT	[side].[ID], 
	[base].[Title], 
	[base].[Description], 
	[base].[StatusValue], 
	[base].[EscalationLevel], 
	[base].[Opened], 
	[base].[Closed], 
	[base].[CustomerID], 
	[side].[sync_row_is_tombstone], 
	[side].[create_scope_id], 
	[side].[create_timestamp], 
	[side].[update_scope_id], 
	[side].[update_timestamp] 
FROM [ServiceTickets] [base]
RIGHT JOIN [ServiceTickets_tracking] [side]
ON [base].[ID] = [side].[ID]
WHERE (
	([side].[CustomerID] = @CustomerID)
	OR (([side].[update_scope_id] = @sync_scope_id or [side].[update_scope_id] IS NULL)
		AND ([side].[CustomerID] IS NULL))
	)
-- use random filter to only sync if the passed value is even
AND @Random % 2 = 0
AND (
	-- Update made by the local instance
	[side].[update_scope_id] IS NULL
	-- Or Update different from remote
	OR [side].[update_scope_id] <> @sync_scope_id
	-- Or we are in reinit mode so we take rows even thoses updated by the scope
	OR @sync_scope_is_reinit = 1
    )
AND (
	-- And Timestamp is > from remote timestamp
	[side].[timestamp] > @sync_min_timestamp
	OR
	-- remote instance is new, so we don't take the last timestamp
	@sync_scope_is_new = 1
	)
AND (
	[side].[sync_row_is_tombstone] = 1 
	OR
	([side].[sync_row_is_tombstone] = 0 AND [base].[ID] is not null	)
)

END";

        #endregion

        [Fact]
        public async Task RandomFilterIncludingAllChanges()
        {

            SqlSyncProvider serverProvider = new SqlSyncProvider(_fixture.ServerConnectionString);
            SqlSyncProvider clientProvider = new SqlSyncProvider(_fixture.Client1ConnectionString);

            SyncAgent agent = new SyncAgent(clientProvider, serverProvider, new[] { "ServiceTickets" });
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "CustomerID"));
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "Random", DbType.Int64));
            agent.Parameters.Add("ServiceTickets", "CustomerID", 1);
            agent.Parameters.Add("ServiceTickets", "Random", 22);

            await serverProvider.ProvisionAsync(agent.Configuration, SyncProvision.All);
            // alter selectchanges procedure to randomly filter data
            using (var sqlConnection = new SqlConnection(_fixture.ServerConnectionString))
            {
                using (var sqlCmd = new SqlCommand(randomlyFilteredStoredProcedure, sqlConnection))
                {
                    sqlConnection.Open();
                    sqlCmd.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }

            var session = await agent.SynchronizeAsync();

            // Only 4 lines should be downloaded
            Assert.Equal(40, session.TotalChangesDownloaded);
            Assert.Equal(0, session.TotalChangesUploaded);

        }

        //[Fact]
        //public async Task RandomFilterExcludingAllChanges()
        //{

        //    SqlSyncProvider serverProvider = new SqlSyncProvider(fixture.ServerConnectionString);
        //    SqlSyncProvider clientProvider = new SqlSyncProvider(fixture.Client1ConnectionString);

        //    SyncAgent agent = new SyncAgent(clientProvider, serverProvider, new[] { "ServiceTickets" });
        //    agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "CustomerID"));
        //    agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "Random", DbType.Int64));
        //    agent.Parameters.Add("ServiceTickets", "CustomerID", 1);
        //    agent.Parameters.Add("ServiceTickets", "Random", 3); // od nuber excludes all changes

        //    await serverProvider.ProvisionAsync(agent.Configuration, SyncProvision.All);
        //    // alter selectchanges procedure to randomly filter data
        //    using (var sqlConnection = new SqlConnection(fixture.ServerConnectionString))
        //    {
        //        using (var sqlCmd = new SqlCommand(randomlyFilteredStoredProcedure, sqlConnection))
        //        {
        //            sqlConnection.Open();
        //            sqlCmd.ExecuteNonQuery();
        //            sqlConnection.Close();
        //        }
        //    }

        //    var session = await agent.SynchronizeAsync();

        //    // Only 4 lines should be downloaded
        //    Assert.Equal(0, session.TotalChangesDownloaded);
        //    Assert.Equal(0, session.TotalChangesUploaded);

        //}

        public void Dispose()
        {
            this._fixture.Deprovision();
        }
    }
}

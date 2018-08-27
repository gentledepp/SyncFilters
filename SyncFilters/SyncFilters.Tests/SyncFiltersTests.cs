using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Filter;
using Dotmim.Sync.SqlServer;
using Shouldly;
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
	            [TenantId] [bigint] NOT NULL,
                CONSTRAINT [PK_ServiceTickets] PRIMARY KEY CLUSTERED ( [ID] ASC ));
            end";

        private string datas =
        $@"
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES ('07fba173-c861-4c12-9fcf-cca5787c4b72', N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES ('48df6e95-827e-42a2-850f-761fa4e66dda', N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 3', N'Description 3', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 4', N'Description 4', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre Client 1', N'Description Client 1', 1, 0, CAST(N'2016-07-29T17:26:20.720' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 6', N'Description 6', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 1)
            INSERT [ServiceTickets] ([ID], [Title], [Description], [StatusValue], [EscalationLevel], [Opened], [Closed], [TenantId]) VALUES (newid(), N'Titre 7', N'Description 7', 1, 0, CAST(N'2016-07-29T16:36:41.733' AS DateTime), NULL, 10)
          ";

        public String ServerConnectionString => DbHelper.GetDatabaseConnectionString(ServerDbName);
        public String Client1ConnectionString => DbHelper.GetDatabaseConnectionString(Client1DbName);
        public String Client2ConnectionString => DbHelper.GetDatabaseConnectionString(Client2DbName);
        public String Client3ConnectionString => DbHelper.GetDatabaseConnectionString(Client3DbName);

        public DbHelper DbHelper { get; set; } = new DbHelper();

        public string ServerDbName { get; } = "Test_SyncFilter_ServerDB";

        public string Client1DbName { get; } = "Test_SyncFilter_Client1";

        public string Client2DbName { get; } = "Test_SyncFilter_Client2";

        public string Client3DbName { get; } = "Test_SyncFilter_Client3";

        public void Provision()
        {

            // create databases
            DbHelper.CreateDatabase(ServerDbName);
            DbHelper.CreateDatabase(Client1DbName);
            DbHelper.CreateDatabase(Client2DbName);
            DbHelper.CreateDatabase(Client3DbName);

            // create table
            DbHelper.ExecuteScript(ServerDbName, createTableScript);

            // create sync principal table
            DbHelper.ExecuteScripts(ServerDbName, System.IO.File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "SQL", "setup.sql")));
            
            // insert table
            DbHelper.ExecuteScript(ServerDbName, datas);

        }

        public void Deprovision()
        {
            //DbHelper.DeleteDatabase(ServerDbName);
            //DbHelper.DeleteDatabase(Client1DbName);
            DbHelper.DeleteDatabase(Client2DbName);
            DbHelper.DeleteDatabase(Client3DbName);
        }
    }

    [Collection("Sync")]
    public class SyncFilterFixture : IClassFixture<SyncFilterClassFixture>, IDisposable
    {
        SyncFilterClassFixture _fixture;
        private readonly SqlSyncProvider _serverProvider;
        private SyncConfiguration _configuration;

        public SyncFilterFixture(SyncFilterClassFixture fixture)
        {
            this._fixture = fixture;
            this._fixture.Provision();
            _configuration = new SyncConfiguration(new []{"ServiceTickets"});
            _configuration.Filters.Add(new FilterClause("ServiceTickets", "TenantId"));
            _configuration.Filters.Add(new FilterClause("ServiceTickets", "UserId", DbType.Int64));


            _serverProvider = new SqlSyncProvider(_fixture.ServerConnectionString);
        }

        [Fact]
        public void CanAddMembersToGroup()
        {
            // Arrange
            var auth = new AuthorizationManager(_fixture.ServerDbName);
            var userId = 1;
            var groupId = 2;
            var tenantId = 1;

            // Act
            auth.AddGroupMember(groupId, userId, tenantId);

            // Assert
            var edges1 = auth.GetPrincpialEdges().ToList();
            edges1.ShouldNotBeEmpty();
            edges1.Single().IsDeleted.ShouldBeFalse();

        }

        [Fact]
        public void CanRemoveMembersFromGroup()
        {
            // Arrange
            var auth = new AuthorizationManager(_fixture.ServerDbName);
            var userId = 1;
            var groupId = 2;
            var tenantId = 1;
            auth.AddGroupMember(groupId, userId, tenantId);

            // Act
            auth.RemoveGroupMember(groupId, userId, tenantId);


            // Assert
            var edges1 = auth.GetPrincpialEdges().ToList();
            edges1.ShouldNotBeEmpty();
            edges1.Single().IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task CanAddSyncPermission()
        {
            // Arrange
            var auth = new AuthorizationManager(_fixture.ServerDbName);
            var userId = 1;
            var groupId = 2;
            var tenantId = 1;
            auth.AddGroupMember(groupId, userId, tenantId);

            await ProvisionSyncFilters();

            var filterManager = new ServiceTicketsSyncFilterRepository(_fixture.ServerDbName);

            // Act
            filterManager.AddFilter(new Guid("48df6e95-827e-42a2-850f-761fa4e66dda"), groupId,
                SyncReasons.BelongsToTenant, tenantId);

            // Assert
            var filters = filterManager.GetSyncFilters().ToList();
            filters.ShouldNotBeEmpty();
            var filter = filters.Single();
            filter.AppliedRules.ShouldBe((int)SyncReasons.BelongsToTenant);
            filter.DeletionTime.ShouldBe(new DateTime(1753, 1, 1));
            filter.TenantId.ShouldBe(tenantId);
            filter.UserId.ShouldBe(groupId, "we authorize the group to save space");

        }

        [Fact]
        public async Task CanRemoveSyncPermission()
        {
            // Arrange
            var auth = new AuthorizationManager(_fixture.ServerDbName);
            var userId = 1;
            var groupId = 2;
            var tenantId = 1;
            auth.AddGroupMember(groupId, userId, tenantId);

            await ProvisionSyncFilters();

            var filterManager = new ServiceTicketsSyncFilterRepository(_fixture.ServerDbName);
            filterManager.AddFilter(new Guid("48df6e95-827e-42a2-850f-761fa4e66dda"), groupId,
                SyncReasons.BelongsToTenant, tenantId);
            
            // Act
            filterManager.RemoveFilter(new Guid("48df6e95-827e-42a2-850f-761fa4e66dda"), groupId,
                SyncReasons.BelongsToTenant | SyncReasons.IsAssignedUser, tenantId);

            // Assert
            var filters = filterManager.GetSyncFilters().ToList();
            filters.ShouldNotBeEmpty();
            var filter = filters.Single();
            filter.AppliedRules.ShouldBe(0);
            filter.DeletionTime.ShouldNotBe(new DateTime(1753, 1, 1));
            filter.TenantId.ShouldBe(tenantId);
            filter.UserId.ShouldBe(groupId, "we authorize the group to save space");

        }

        [Fact]
        public async Task WhenNoSyncFilterRowsExist()
        {
            SqlSyncProvider clientProvider = new SqlSyncProvider(_fixture.Client1ConnectionString);

            SyncAgent agent = new SyncAgent(clientProvider, _serverProvider, new[] { "ServiceTickets" });
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "TenantId"));
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "UserId", DbType.Int64));
            agent.Parameters.Add("ServiceTickets", "TenantId", 1);
            agent.Parameters.Add("ServiceTickets", "UserId", 1);

            await ProvisionSyncFilters();


            var session = await agent.SynchronizeAsync();

            // nothing should be downloaded, as no syncfilters are present
            Assert.Equal(0, session.TotalChangesDownloaded);
            Assert.Equal(0, session.TotalChangesUploaded);

            var ticketRepo = new ServiceTicketRepository(_fixture.ServerDbName);
            var tickets = ticketRepo.GetServiceTickets().ToList();
            tickets.ShouldNotBeEmpty();
            tickets.Count.ShouldBe(50);
        }


        [Fact]
        public async Task WhenSyncFilterIsAdded_SyncsRows()
        {
            var inspectors = 1;
            var inspector_gadget = 2;
            var ticketRepo = new ServiceTicketRepository(_fixture.ServerDbName);
            var filterRepo = new ServiceTicketsSyncFilterRepository(_fixture.ServerDbName);
            var auth = new AuthorizationManager(_fixture.ServerDbName);


            SqlSyncProvider clientProvider = new SqlSyncProvider(_fixture.Client1ConnectionString);

            SyncAgent agent = new SyncAgent(clientProvider, _serverProvider, new[] { "ServiceTickets" });
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "TenantId"));
            agent.Configuration.Filters.Add(new FilterClause("ServiceTickets", "UserId", DbType.Int64));
            agent.Parameters.Add("ServiceTickets", "TenantId", 1);
            agent.Parameters.Add("ServiceTickets", "UserId", inspector_gadget);

            await ProvisionSyncFilters();

            await Task.Delay(200);

            await agent.SynchronizeAsync();


            auth.AddGroupMember(inspectors, inspector_gadget, 1);

            // add sync filter row for each ticket
            var tickets = ticketRepo.GetServiceTickets().Where(t => t.TenantId == 1).ToList();
            foreach (var ticket in tickets)
            {
                filterRepo.AddFilter(ticket.ID, inspectors, SyncReasons.BelongsToTenant, 1);
            }


            // Act
            var session = await agent.SynchronizeAsync();

            // nothing should be downloaded, as no syncfilters are present
            Assert.Equal(tickets.Count, session.TotalChangesDownloaded);
            Assert.Equal(0, session.TotalChangesUploaded);
        }


        private async Task ProvisionSyncFilters()
        {
            await _serverProvider.ProvisionAsync(_configuration, SyncProvision.All);
            // now provision sync filters
            var syncFilterSql = System.IO.File
                .ReadAllText(Path.Combine(Environment.CurrentDirectory, "SQL", "syncfilter.sql"))
                .Replace("__TARGETTABLE__", "ServiceTickets");
            _fixture.DbHelper.ExecuteScripts(_fixture.ServerDbName, syncFilterSql);
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

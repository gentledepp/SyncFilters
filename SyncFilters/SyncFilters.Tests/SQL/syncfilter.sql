
CREATE TABLE [dbo].[__TARGETTABLE___syncfilter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [uniqueidentifier] NOT NULL,
	[Modified] [binary](8) NOT NULL,
	[AppliedRules] [int] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[TenantId] [bigint] NOT NULL,
	[DeletionTime] [datetime2](7) NOT NULL DEFAULT '1753-01-01',
	[RowVersion] [timestamp] NOT NULL,
 CONSTRAINT [PK___TARGETTABLE___syncfilter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO



ALTER PROCEDURE [dbo].[__TARGETTABLE___TenantIdUserId__selectchanges]
	@sync_min_timestamp bigint,
	@sync_scope_id uniqueidentifier,
	@sync_scope_is_new bit,
	@sync_scope_is_reinit bit,
	@TenantId bigint,
	@UserId bigint
AS
BEGIN

-- clause 1: get all modified items where there exists at least one non-deleted sync row
SELECT 	[side].[ID], 
	[base].[Title], 
	[base].[Description], 
	[base].[StatusValue], 
	[base].[EscalationLevel], 
	[base].[Opened], 
	[base].[Closed], 
	[base].[TenantId], 
	[side].[sync_row_is_tombstone], 
	[side].[create_scope_id], 
	[side].[create_timestamp], -- keep the timestamp, as in the 1st clause, we focus on the changed items (not added sync rows)
	[side].[update_scope_id], 
	[side].[update_timestamp]  
FROM [__TARGETTABLE__] [base]
RIGHT JOIN [__TARGETTABLE___tracking] [side]
ON [base].[Id] = [side].[Id]
WHERE (
	([side].[TenantId] = @TenantId)
	OR (([side].[update_scope_id] = @sync_scope_id or [side].[update_scope_id] IS NULL)
		AND ([side].[TenantId] IS NULL))
	)
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
	-- And the row is toombstoned
	-- NOTE: we do *not* filter fir any syncfilters here => this means that a client may get a tombstoned item, even though (s)he never had a valid sync row assigned
	--		 this *could* lead to a massive set of rows being unnecessarily transmitted, in case your system has a lot of deletes. However, it speeds up the query for those systems that dont have many deletions - e.g. ones that use soft deletes)
	[side].[sync_row_is_tombstone] = 1 
	OR
	(
		-- Or there exists a row in the base table (RIGHT JOIN!)s
		[side].[sync_row_is_tombstone] = 0 AND [base].[Id] is not null	
		-- and there exists at least one non-deleted syncfilter
		and exists (select s.id from __TARGETTABLE___syncfilter s
						inner join principaledge e on e.endvertex = s.userid and e.startprincipaltype = 1 and e.startvertex = @userid --user
						where s.itemid = [base].id and s.appliedrules > 0 and e.isdeleted = 0)
	)
)
UNION ALL
-- clause 2: get all items, which were not modified, but their sync rows are newer (toombstoned or not)
SELECT 	[side].[ID], 
	[base].[Title], 
	[base].[Description], 
	[base].[StatusValue], 
	[base].[EscalationLevel], 
	[base].[Opened], 
	[base].[Closed], 
	[base].[TenantId], 
	[side].[sync_row_is_tombstone], 
	[side].[create_scope_id], 
	--[side].[create_timestamp], 
	@sync_min_timestamp as [create_timestamp], -- use the clients timestamp as "create", so dotmim.sync registers any new rows that are added due to added sync filters as "DmRowState.Added"!
	[side].[update_scope_id], 
	[side].[update_timestamp]  
FROM [__TARGETTABLE__] [base]
RIGHT JOIN [__TARGETTABLE___tracking] [side]
ON [base].[Id] = [side].[Id]
WHERE (
	([side].[TenantId] = @TenantId)
	OR (([side].[update_scope_id] = @sync_scope_id or [side].[update_scope_id] IS NULL)
		AND ([side].[TenantId] IS NULL))
	)
-- we do NOT care who actually made the changes, as the sync is triggered by the syncfilter table only!
--AND (
--	-- Update made by the local instance
--	[side].[update_scope_id] IS NULL
--	-- Or Update different from remote
--	OR [side].[update_scope_id] <> @sync_scope_id
--	-- Or we are in reinit mode so we take rows even thoses updated by the scope
--	OR @sync_scope_is_reinit = 1
--    )
AND (
	-- And Timestamp is <= from remote timestamp, as the items that DO have changes are already included in clause 1!
	[side].[timestamp] <= @sync_min_timestamp
	)
-- and there is at least one younger, non-deleted syncfilter (permission was added to user)
and exists (select s.id from __TARGETTABLE___syncfilter s
					inner join principaledge e on e.endvertex = s.userid and e.startprincipaltype = 1 and e.startvertex = @userid --user
						where s.itemid = [base].id and s.appliedrules > 0 and e.IsDeleted = 0 and (s.modified >  @sync_min_timestamp or e.modified > @sync_min_timestamp))
-- make sure this item was not already synced by an older valid and non-deleted filter
and not exists (select s.id from __TARGETTABLE___syncfilter s
					inner join principaledge e on e.endvertex = s.userid and e.startprincipaltype = 1 and e.startvertex = @userid --user
						where s.itemid = [base].id and s.appliedrules > 0 and e.IsDeleted = 0 and (s.modified <=  @sync_min_timestamp and e.modified <= @sync_min_timestamp))
UNION ALL
-- clause 3: get all toombstoned items
--					- where no non-deleted syncfilter exists
--					- and there is a deleted sync filter younger than "date"
SELECT 	[side].[ID], 
	[base].[Title], 
	[base].[Description], 
	[base].[StatusValue], 
	[base].[EscalationLevel], 
	[base].[Opened], 
	[base].[Closed], 
	[base].[TenantId], 
	1 as [sync_row_is_tombstone], -- mark all rows that lost their sync filter as "tombstoned" so Dotmim.Sync also detects these as tombstoned/deleted
	[side].[create_scope_id], 
	[side].[create_timestamp], 
	[side].[update_scope_id], 
	[side].[update_timestamp] 
FROM [__TARGETTABLE__] [base]
RIGHT JOIN [__TARGETTABLE___tracking] [side]
ON [base].[Id] = [side].[Id]
WHERE (
	([side].[TenantId] = @TenantId)
	OR (([side].[update_scope_id] = @sync_scope_id or [side].[update_scope_id] IS NULL)
		AND ([side].[TenantId] IS NULL))
	)
-- we do NOT care who actually made the changes, as the sync is triggered by the syncfilter table only!
--AND (
--	-- Update made by the local instance
--	[side].[update_scope_id] IS NULL
--	-- Or Update different from remote
--	OR [side].[update_scope_id] <> @sync_scope_id
--	-- Or we are in reinit mode so we take rows even thoses updated by the scope
--	OR @sync_scope_is_reinit = 1
--    )
-- we also do NOT care if the row in the base table has changed, or it is a new scope (as if it IS a new scope, the tombstoned items should not be retreived anyways) (correct?)
--AND (
--	-- And Timestamp is > from remote timestamp
--	[side].[timestamp] > @sync_min_timestamp
--	OR
--	-- remote instance is new, so we don't take the last timestamp
--	@sync_scope_is_new = 1
--	)
AND (
	-- we do NOT care about items here, that are deleted in the base table! (see clause 1 for these)
	---- And the row is toombstoned
	--[side].[sync_row_is_tombstone] = 1 
	--OR
	-- Or there exists a row in the base table (RIGHT JOIN!)s
	(
		[side].[sync_row_is_tombstone] = 0 AND [base].[Id] is not null
		AND
		
		-- where no non-deleted syncfilter exists
		not exists (select s.id from __TARGETTABLE___syncfilter s
							inner join principaledge e on e.endvertex = s.userid and e.startprincipaltype = 1 and e.startvertex = @userid --user
						where s.itemid = [base].id and s.appliedrules > 0 and e.isdeleted = 0)
		-- and there is a deleted sync filter younger than "date"
		and exists (select s.id from __TARGETTABLE___syncfilter s 
							inner join principaledge e on e.endvertex = s.userid and e.startprincipaltype = 1 and e.startvertex = @userid --user
						where s.itemid = [base].id and ((s.appliedrules = 0 and s.modified > @sync_min_timestamp) or (e.IsDeleted = 1 and e.modified > @sync_min_timestamp)))
	)
)

END


GO



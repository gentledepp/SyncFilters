CREATE TABLE [dbo].[PrincipalEdge](
	[StartVertex] [bigint] NOT NULL,
	[EndVertex] [bigint] NOT NULL,
	[Hops] [int] NOT NULL,
	[StartPrincipalType] [tinyint] NOT NULL,
	[EndPrincipalType] [tinyint] NOT NULL,
	[DelMark] [bit] NOT NULL DEFAULT 0,
	[TenantId] [int] NOT NULL,
	[Modified] [timestamp] NOT NULL,
	[IsDeleted] [bit] NOT NULL DEFAULT 0,
 CONSTRAINT [PK_PrincipalEdge] PRIMARY KEY CLUSTERED 
(
	[StartVertex] ASC,
	[EndVertex] ASC,
	[Hops] ASC,
	[StartPrincipalType] ASC,
	[EndPrincipalType] ASC,
	[TenantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


GO


CREATE PROCEDURE [dbo].[AddEdgeWithSpaceSavingsPrincipal]
    @StartVertexId [bigint],
    @EndVertexId [bigint],
    @StartPrincipalType [tinyint] = 0,
    @EndPrincipalType [tinyint] = 0,
    @TenantId [int]
AS
BEGIN
    SET NOCOUNT ON
    IF EXISTS(SELECT Hops 
    FROM PrincipalEdge 
    WHERE StartVertex = @StartVertexId 
    AND EndVertex = @EndVertexId 
    AND StartPrincipalType = @StartPrincipalType
    AND EndPrincipalType = @EndPrincipalType
    AND TenantId = @TenantId
    AND Hops = 0)
    BEGIN
    RETURN 0 -- DO NOTHING!!!
    END
    
    IF @StartVertexId = @EndVertexId AND @StartPrincipalType = @EndPrincipalType
    OR EXISTS (SELECT Hops 
    FROM PrincipalEdge
    WHERE StartVertex = @EndVertexId 
    AND EndVertex = @StartVertexId
    AND StartPrincipalType = @EndPrincipalType
    AND EndPrincipalType = @StartPrincipalType
    AND TenantId = @TenantId)
    BEGIN
    RAISERROR ('Attempt to create a circular relation detected!', 16, 1)
    RETURN 0
    END
    
    CREATE TABLE #CandidatesPrincipalEdge ( 
    StartVertex bigint,
    EndVertex bigint,
    StartPrincipalType tinyint,
    EndPrincipalType tinyint,
    TenantId bigint)
    
    -- step 1: A's incoming edges to B
    INSERT INTO #CandidatesPrincipalEdge
    SELECT StartVertex 
    , @EndVertexId
    , StartPrincipalType
    , @EndPrincipalType
    , TenantId
    FROM PrincipalEdge
    WHERE EndVertex = @StartVertexId
    AND EndPrincipalType = @StartPrincipalType
    AND TenantId = @TenantId
    
    -- step 2: A to B's outgoing edges
    UNION
    SELECT @StartVertexId
    , EndVertex
    , @StartPrincipalType
    , EndPrincipalType
    , TenantId
    FROM PrincipalEdge
    WHERE StartVertex = @EndVertexId
    AND StartPrincipalType = @EndPrincipalType
    AND TenantId = @TenantId
    
    -- step 3: A’s incoming edges to end vertex of B's outgoing edges
    UNION
    SELECT A.StartVertex 
    , B.EndVertex
    , A.StartPrincipalType
    , B.EndPrincipalType
    , A.TenantId
    FROM PrincipalEdge A
    CROSS JOIN PrincipalEdge B
    WHERE A.EndVertex = @StartVertexId
    AND B.StartVertex = @EndVertexId
    AND A.EndPrincipalType = @StartPrincipalType
    AND B.StartPrincipalType = @EndPrincipalType
    AND A.TenantId = B.TenantId
    
    INSERT INTO PrincipalEdge (
    StartVertex,
    EndVertex,
    Hops,
    StartPrincipalType,
    EndPrincipalType,
    TenantId)
    VALUES ( 
    @StartVertexId
    , @EndVertexId
    , 0
    , @StartPrincipalType
    , @EndPrincipalType
    , @TenantId)

   update  E
   set --E.Modified = DEFAULT, not necessary as timestamp/rowversion rows are updated anyway
       E.IsDeleted = 0 -- remove deletion flag 
   from PRINCIPALEDGE E inner join #CandidatesPrincipalEdge C on E.StartVertex = C.StartVertex
                     AND E.EndVertex = C.EndVertex 
                     AND E.Hops = 1
    
    INSERT INTO PrincipalEdge (
    StartVertex,
    EndVertex,
    Hops,
    StartPrincipalType,
    EndPrincipalType,
    TenantId)
    SELECT StartVertex,
    EndVertex,
    1,
    StartPrincipalType,
    EndPrincipalType,
    TenantId
    FROM #CandidatesPrincipalEdge C
    WHERE NOT EXISTS (
    SELECT Hops
    FROM PrincipalEdge E
    WHERE E.StartVertex = C.StartVertex
    AND E.EndVertex = C.EndVertex 
    AND E.Hops = 1
    AND E.StartPrincipalType = C.StartPrincipalType
    AND E.EndPrincipalType = C.EndPrincipalType
    And E.TenantId = C.TenantId)
    
END


GO



CREATE PROCEDURE [dbo].[RemoveEdgeWithSpaceSavingsPrincipal]
    @StartVertexId [bigint],
    @EndVertexId [bigint],
    @PrincipalType [tinyint] = 1,
    @TenantId [int]
AS
BEGIN
    
    SET NOCOUNT ON
    Update PrincipalEdge
    set IsDeleted = 1
	WHERE Hops = 0
    AND StartVertex = @StartVertexId
    AND EndVertex = @EndVertexId
    AND StartPrincipalType = @PrincipalType
    AND TenantId = @TenantId
    
    IF @@ROWCOUNT = 0
    BEGIN
    RETURN -- NOTHING TO DELETE
    END
    
    --UPDATE PrincipalEdge SET DelMark = 0
    
    UPDATE PrincipalEdge
    SET DelMark = 1
    FROM PrincipalEdge 
    INNER JOIN (
    SELECT StartVertex
    , @EndVertexId AS EndVertex
    , StartPrincipalType as StartPrincipalType
    FROM PrincipalEdge
    WHERE EndVertex = @StartVertexId
    AND StartPrincipalType = @PrincipalType
    AND TenantId = @TenantId
    UNION
    SELECT @StartVertexId
    , EndVertex AS EndVertex
    , StartPrincipalType as StartPrincipalType
    FROM PrincipalEdge
    WHERE StartVertex = @EndVertexId
    AND StartPrincipalType = @PrincipalType
    AND TenantId = @TenantId
    UNION
    SELECT A.StartVertex 
    , B.EndVertex
    , A.StartPrincipalType as StartPrincipalType
    FROM PrincipalEdge A
    CROSS JOIN PrincipalEdge B
    WHERE A.EndVertex = @StartVertexId
    AND B.StartVertex = @EndVertexId
    AND A.StartPrincipalType = @PrincipalType
    AND A.TenantId = @TenantId
    AND B.TenantId = @TenantId
    ) AS C
    ON C.StartVertex = PrincipalEdge.StartVertex
    AND C.EndVertex = PrincipalEdge.EndVertex 
    AND c.StartPrincipalType = PrincipalEdge.StartPrincipalType
    WHERE Hops > 0
	and IsDeleted = 0;
    
    WITH SafeRows AS
    ( SELECT StartVertex, EndVertex, StartPrincipalType FROM PrincipalEdge WHERE DelMark = 0 and IsDeleted = 0 AND TenantId = @TenantId)
    UPDATE PrincipalEdge
    SET DelMark = 0
    FROM PrincipalEdge
    INNER JOIN SafeRows S1
    ON S1.StartVertex = PrincipalEdge.StartVertex
    INNER JOIN SafeRows S2 
    ON S1.EndVertex = S2.StartVertex
    AND S2.EndVertex = PrincipalEdge.EndVertex
    WHERE DelMark = 1
	   and IsDeleted = 0;
    
    WITH SafeRows AS
    ( SELECT StartVertex, EndVertex FROM PrincipalEdge WHERE DelMark = 0 and IsDeleted = 0  AND TenantId = @TenantId)
    UPDATE PrincipalEdge
    SET DelMark = 0
    FROM PrincipalEdge 
    INNER JOIN SafeRows	S1
    ON S1.StartVertex = PrincipalEdge.StartVertex
    INNER JOIN SafeRows S2 
    ON S1.EndVertex = S2.StartVertex
    INNER JOIN SafeRows S3    
    ON S2.EndVertex = S3.StartVertex
    AND S3.EndVertex = PrincipalEdge.EndVertex
    WHERE DelMark = 1
	   and IsDeleted   = 0;
    
   Update PrincipalEdge 
		set delmark = 0, IsDeleted = 1--, Modified = getutcdate() not necessary as timestamp/rowversion columns are updated anyways
        WHERE DelMark = 1 
END
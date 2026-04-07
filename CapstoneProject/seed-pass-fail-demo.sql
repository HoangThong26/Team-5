USE CapstoneManagement;
GO

/*
  Seeds demo final-grade data so the Admin Dashboard pass/fail donut can render.
  Logic expected by the frontend:
  - Pass: AverageScore >= 5
  - Fail: AverageScore < 5
*/

IF NOT EXISTS (SELECT 1 FROM Groups)
BEGIN
  RAISERROR('No groups found. Create at least 2 groups before running this seed.', 16, 1);
  RETURN;
END
GO

;WITH RankedGroups AS (
  SELECT
    GroupId,
    ROW_NUMBER() OVER (ORDER BY GroupId) AS rn
  FROM Groups
),
DemoScores AS (
  SELECT GroupId, CAST(8.20 AS DECIMAL(5,2)) AS AverageScore, N'B+' AS GradeLetter
  FROM RankedGroups
  WHERE rn = 1

  UNION ALL

  SELECT GroupId, CAST(4.30 AS DECIMAL(5,2)) AS AverageScore, N'F' AS GradeLetter
  FROM RankedGroups
  WHERE rn = 2
)
MERGE FinalGrades AS target
USING DemoScores AS source
ON target.GroupId = source.GroupId
WHEN MATCHED THEN
  UPDATE SET
    target.AverageScore = source.AverageScore,
    target.GradeLetter = source.GradeLetter,
    target.IsPublished = 1,
    target.PublishedAt = SYSDATETIME()
WHEN NOT MATCHED THEN
  INSERT (GroupId, AverageScore, GradeLetter, IsPublished, PublishedAt)
  VALUES (source.GroupId, source.AverageScore, source.GradeLetter, 1, SYSDATETIME());
GO

SELECT
  g.GroupId,
  g.GroupName,
  fg.AverageScore,
  fg.GradeLetter,
  fg.IsPublished,
  fg.PublishedAt
FROM Groups g
LEFT JOIN FinalGrades fg ON fg.GroupId = g.GroupId
ORDER BY g.GroupId;
GO

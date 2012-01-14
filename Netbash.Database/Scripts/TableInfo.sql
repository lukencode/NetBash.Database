DECLARE @cmdstr varchar(100)
--Create Temporary Table
CREATE TABLE #TempTable
(
    [Table_Name] varchar(50),
    Row_Count int,
    Table_Size varchar(50),
    Data_Space_Used varchar(50),
    Index_Space_Used varchar(50),
    Unused_Space varchar(50)
)
--Create Stored Procedure String
SELECT @cmdstr = 'sp_msforeachtable ''sp_spaceused "?"'''
--Populate Tempoary Table
INSERT INTO #TempTable EXEC(@cmdstr)
--Determine sorting method
SELECT * FROM #TempTable ORDER BY Table_Name
--Delete Temporay Table
DROP TABLE #TempTable
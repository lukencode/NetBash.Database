--Disable Constraints & Triggers
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL'
--Perform delete operation on all table for cleanup
EXEC sp_MSforeachtable 'DELETE ?'
--Enable Constraints & Triggers again
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'
--Reset Identity on tables with identity column
EXEC sp_MSforeachtable 'IF OBJECTPROPERTY(OBJECT_ID(''?''), ''TableHasIdentity'') = 1
BEGIN DBCC CHECKIDENT (''?'',RESEED,0) END'

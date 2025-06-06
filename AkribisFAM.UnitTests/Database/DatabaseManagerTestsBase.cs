using AkribisFAM.Interfaces;
using AkribisFAM.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

[TestClass]
public abstract class DatabaseManagerTestsBase
{
    protected IDatabaseManager _dbManager;
    private const string TestDbFileName = @"C:\Temp\TestDatabase.sqlite";

    [TestInitialize]
    public void TestInitialize()
    {
        // Delete test DB if exists to start fresh
        if (File.Exists(TestDbFileName))
        {
            File.Delete(TestDbFileName);
        }

        _dbManager = new DatabaseManager(TestDbFileName);

        // Optionally create tables here or in DatabaseManager constructor
        CreateTables();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Cleanup resources
        if (_dbManager is IDisposable disposable)
            disposable.Dispose();

        //if (File.Exists(TestDbFileName))
        //    File.Delete(TestDbFileName);
    }

    private void CreateTables()
    {
        // Use SQLite commands to create tables, or ensure your DBManager does it.
        // For brevity, you can call a method on _dbManager that ensures tables exist.
    }
}

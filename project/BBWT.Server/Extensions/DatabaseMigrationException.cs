namespace BBWT.Server.Extensions;


[Serializable]
public class DatabaseMigrationException : Exception
{
    public DatabaseMigrationException() { }
    public DatabaseMigrationException(string message) : base(message) { }
    public DatabaseMigrationException(string message, Exception inner) : base(message, inner) { }
    protected DatabaseMigrationException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public bool IsMigrationsAppRun { get; set; }
}

namespace BBWM.Core.Data;

/// <summary>
/// Base interface for entities with generic primary key named "Id".
/// </summary>
public interface IEntity<TKey> : IBaseEntity
    where TKey : IEquatable<TKey>
{
    TKey Id { get; set; }
}

/// <summary>
/// Base interface for entities with integer primary key named "Id".
/// </summary>
public interface IEntity : IEntity<int>
{
}

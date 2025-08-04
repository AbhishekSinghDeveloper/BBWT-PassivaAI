namespace BBWM.Core.Data;

/// <summary>
/// Auditable entity definition.
/// </summary>
public interface IAuditableEntity<TKey> : IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}

/// <summary>
/// Auditable entity definition with integer key.
/// </summary>
public interface IAuditableEntity : IEntity
{
}

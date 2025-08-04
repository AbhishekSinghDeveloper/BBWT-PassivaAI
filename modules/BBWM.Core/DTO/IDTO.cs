namespace BBWM.Core.DTO;

/// <summary>
/// Base interface for dtos with generic primary key named "Id".
/// </summary>
/// <typeparam name="TKey"></typeparam>
public interface IDTO<TKey> : IBaseDTO
    where TKey : IEquatable<TKey>
{
    TKey Id { get; set; }
}

/// <summary>
/// Base interface for dtos with integer primary key named "Id".
/// </summary>
public interface IDTO : IDTO<int>
{
}

namespace CarePath.Models.Entities;

/// <summary>
/// Base class for all domain entities with an integer primary key.
/// .NET 10: No changes required — plain C# class.
/// </summary>
public abstract class Entity
{
    public int Id { get; set; }
}

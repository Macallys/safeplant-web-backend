namespace PlantMonitoring.Domain.Entities;

public sealed class IndustrialArea
{
    private IndustrialArea()
    {
    }

    public IndustrialArea(Guid id, string name, string location, DateTimeOffset updatedAt)
    {
        Id = id;
        Name = name;
        Location = location;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = "";

    public string Location { get; private set; } = "";

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(string name, string location, DateTimeOffset updatedAt)
    {
        Name = name;
        Location = location;
        UpdatedAt = updatedAt;
    }
}

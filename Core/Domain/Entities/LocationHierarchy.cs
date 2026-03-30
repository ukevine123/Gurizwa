namespace Domain.Entities;

public class Province
{
    public string Name { get; set; } = string.Empty;
    public List<District> Districts { get; set; } = [];
}

public class District
{
    public string Name { get; set; } = string.Empty;
    public List<Sector> Sectors { get; set; } = [];
}

public class Sector
{
    public string Name { get; set; } = string.Empty;
    public List<Cell> Cells { get; set; } = [];
}

public class Cell
{
    public string Name { get; set; } = string.Empty;
    public List<Village> Villages { get; set; } = [];
}

public class Village
{
    public string Name { get; set; } = string.Empty;
}

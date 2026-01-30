public class EducationTable
{
    public int EducationId { get; set; }

    public string? SchoolName { get; set; }
    public string? Department { get; set; }
    public string? ClassLevel { get; set; }

    public int StartYear { get; set; }
    public int? EndYear { get; set; }

    public bool IsCurrent { get; set; }

}

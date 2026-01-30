using Portfolyo.Data;

public class AboutViewModel
{
    // HERO / KART
    public AboutMeTable About { get; set; }

    // HAKKIMDA DETAY
    public AboutInfoTable Info { get; set; }

    // ALT DETAYLAR
    public List<AboutMe2Table> Details { get; set; }
    public List<EducationTable> Educations { get; set; }
}

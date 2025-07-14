Console.WriteLine("Hello, World!");
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<StudySession> StudySessions { get; set; } = new();
}
public class StudySession
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string Notes { get; set; }

    public int SkillId { get; set; }
    public Skill Skill { get; set; }
}
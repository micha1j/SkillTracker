using Microsoft.EntityFrameworkCore;

namespace SkillTrainer
{
    class Program
    {
        static void UtwórzSkill(AppDbContext context, string name)
        {
            bool takasamanazwa = context.Skills.Any(s => s.Name == name);
            if (takasamanazwa == true) { Console.WriteLine("Ten skill już istnieje"); return; }
            context.Skills.Add(new Skill { Name = name });
            context.SaveChanges();
        }
        static void UtwórzStudySession(AppDbContext context, string skillname, DateTime data, int durationMinutes, string notes)
        {
            bool takasamanazwa = context.Skills.Any(s => s.Name == skillname);
            if (takasamanazwa == false)
            {
                UtwórzSkill(context, skillname);
            }
            var skill = context.Skills.SingleOrDefault(s => s.Name == skillname);
            if (skill == null)
            {
                throw new InvalidOperationException("Skill nie został poprawnie utworzony lub pobrany z bazy.");
            }
            var session = new StudySession { Date = data, DurationMinutes = durationMinutes, Notes = notes, Skill = skill};
            context.StudySessions.Add(session);
            context.SaveChanges();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            using var context = new AppDbContext();
        }
    }

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

    public class AppDbContext : DbContext
    {
        public DbSet<Skill> Skills { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=skilltrack.db");
        }
    }
}
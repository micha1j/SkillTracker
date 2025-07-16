using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;

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
        static void UtwórzStudySession(AppDbContext context, string skillname, int durationMinutes, string notes, DateTime? data=null)
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
            DateTime ostdata = data ?? DateTime.Now;
            var session = new StudySession { Date = ostdata, DurationMinutes = durationMinutes, Notes = notes, Skill = skill};
            context.StudySessions.Add(session);
            context.SaveChanges();
        }
        static bool NowySkillKonsola(AppDbContext context)
        {
            Console.WriteLine("Podaj nazwę umiejętności:");
            string nameofskill = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nameofskill))
            {
                Console.WriteLine("Nie podałeś żadnej nazwy.");
                return false;
            }
            else
            {
                UtwórzSkill(context, nameofskill); return true;
            }
        }
        static bool NowaSesjaKonsola(AppDbContext context)
        {
            Console.WriteLine("Istniejące umiejętności:");
            var skille = context.Skills.ToList();
            foreach (Skill skill in skille) { Console.WriteLine($"{skill.Name}"); }
            Console.WriteLine("Wpisz nazwę umiejętności, czas trwania w minutach i notatki oddzielone przecinkami:");
            string dataofsession = Console.ReadLine();
            string[] data = dataofsession.Split(",");

            if (data.Length != 3 || !int.TryParse(data[1], out int minutes))
            {
                Console.WriteLine("Zły format. Spróbuj: nazwa, minuty, notatka");
                return false;
            }

            string name = data[0].Trim();
            string notes = data[2].Trim();

            Console.WriteLine("Opcjonalnie wpisz datę i czas sesji (np. 2025-07-14 18:00), domyślnie jest czas teraźniejszy(nic nie wpisuj):");
            string datentime = Console.ReadLine();
            if (datentime == "") { UtwórzStudySession(context, name, minutes, notes); return true; }
            else if (!DateTime.TryParse(datentime, out DateTime parsedDate))
            {
                Console.WriteLine("Zły format daty");
                return false;
            }
            else { UtwórzStudySession(context, name, minutes, notes, parsedDate); return true; }
        }
        static List<Skill> WyswietlSkille(AppDbContext context, int a)
        {
            var skillezsesjami = context.Skills.Include(s => s.StudySessions).ToList();
            if (a == 1) //wszystkie skille
            {
                int i = 0;
                foreach (var skill in skillezsesjami)
                {
                    i++;
                    Console.WriteLine($"{i})  {skill.Name}");
                }
                return skillezsesjami;
            }
            else if (a == 2) // skille z sesjami
            {
                var skilleZSesjami = skillezsesjami.Where(s => s.StudySessions.Count != 0).ToList();

                for (int i = 0; i < skilleZSesjami.Count; i++)
                {
                    Console.WriteLine($"{i + 1})  {skilleZSesjami[i].Name}");
                }

                return skilleZSesjami;
            }
            else //skille bez sesji
            {
                if (a != 3) { throw new ArgumentException("Nieprawidłowy argument: a musi być 1, 2 lub 3."); }
                var skillebez = skillezsesjami.Where(s => s.StudySessions.Count == 0).ToList();

                for (int i = 0; i < skillebez.Count; i++)
                {
                    Console.WriteLine($"{i + 1})  {skillebez[i].Name}");
                }

                return skillebez;
            }
            
        }
        
        static List<StudySession> WyswietlSesje(AppDbContext context, int a, string skillname = null)
        {
            var sesje = context.StudySessions.Include(s => s.Skill).ToList();
            if (a == 1) //wszystkie
            {
                for (int i = 0; i < sesje.Count; i++)
                {
                    var sesja = sesje[i];
                    Console.WriteLine($"{i+1}) {sesja.Skill.Name}: {sesja.Date:g}, {sesja.DurationMinutes} min, Notatka: {sesja.Notes}");
                    
                }
                return sesje;
            }
            else // po skillu
            {
                if (a != 2 || skillname == null) { throw new ArgumentException("Nieprawidłowy argument: a musi być 1 lub 2."); }
                var sesjeskilla = sesje.Where(s => s.Skill.Name == skillname).ToList();
                for (int i = 0; i < sesjeskilla.Count; i++)
                {
                    var sesja = sesjeskilla[i];
                    Console.WriteLine($"{i+1} {sesja.Skill.Name}: {sesja.Date:g}, {sesja.DurationMinutes} min, Notatka: {sesja.Notes}");
                    
                }
                return sesjeskilla;

            }
        }
        
        static void PokazDane(AppDbContext context)
        {
            Console.WriteLine("Co chcesz przejrzeć    1) skille     2)sesje?");
            string wybor1 = Console.ReadLine();
            if (wybor1 == "1")
            {
                Console.WriteLine("Jakie skille chcesz przejrzeć?  1) wszystkie   2)z sesjami   3)bez sesji");
                string wybor2 = Console.ReadLine();
                if (int.TryParse(wybor2, out int numer) && (numer == 1 || numer == 2 || numer == 3))
                {
                    Console.WriteLine("1) edytuj nazwę  2)  usuń(wraz z przypisanymi sesjami)  3) wróc do menu \n \n \n");
                    List<Skill> lista = WyswietlSkille(context, numer);
                    string wybor3 = Console.ReadLine();
                    if (wybor3 == "1")
                    {
                        Console.WriteLine("Wybierz skill którego nazwę chcesz zmienić\n \n \n");
                        int i = 1;
                        foreach (var skill in lista)
                        {
                            Console.WriteLine($"{i}) {skill.Name}");
                            i++;
                        }
                        string wybor4 = Console.ReadLine();
                        if (int.TryParse(wybor4, out int number2) && (number2 > 0) && (number2 < lista.Count + 1))
                        {
                            Console.WriteLine($"Wybrałeś/aś {lista[number2 - 1].Name}");
                            Console.WriteLine("Wpisz nową nazwę:");
                            string nowanazwa = Console.ReadLine();
                            if (nowanazwa == "") { Console.WriteLine("Nie wpisano żadnej nazwy, zmiana nie zostanie zapisana"); return; }
                            lista[number2 - 1].Name = nowanazwa.Trim();
                            context.SaveChanges();
                        }
                        else { Console.WriteLine($"{wybor4} nie odpowiada żadnej umiejętności. Spróbuj ponownie "); }
                    }
                    else if (wybor3 == "2")
                    {
                        Console.WriteLine("Wybierz skill którego chcesz usunąć\n \n \n");
                        int i = 1;
                        foreach (var skill in lista)
                        {
                            Console.WriteLine($"{i}) {skill.Name}");
                            i++;
                        }
                        string wybor4 = Console.ReadLine();
                        if (int.TryParse(wybor4, out int number2) && (number2 > 0) && (number2 < lista.Count + 1))
                        {
                            Console.WriteLine($"Wybrałeś {lista[number2 - 1].Name}, 1)  potwierdź   2) anuluj");
                            string wybor5 = Console.ReadLine();
                            if (wybor5 == "1")
                            {
                                context.Skills.Remove(lista[number2 - 1]);
                                context.SaveChanges();
                            }
                            else { Console.WriteLine("Skill nie zostanie usunięty"); }
                        }
                        else { Console.WriteLine($"{wybor4} nie odpowiada żadnej umiejętności. Spróbuj ponownie "); }
                    }
                    else
                    {
                        if (wybor3 == "3")
                        {
                            Console.WriteLine("Powrót do menu");
                        }
                        else { Console.WriteLine($"{wybor3} to nie 1, 2 bądź 3. Powrót do menu"); }
                    }
                }

                else
                {
                    Console.WriteLine($"{wybor2} to nie 1, 2 lub 3. Powrót do menu");
                }
                
            }
            else if (wybor1 == "2")
            {
                Console.WriteLine("1) Wszystkie sesje 2) Sesje danego skilla");
                string wybor2 = Console.ReadLine();
                if (int.TryParse(wybor2, out int numer) && (numer == 1 || numer == 2))
                {
                    List<StudySession> lista;
                    if (numer == 2)
                    {
                        var skille = context.Skills.Include(s=>s.StudySessions).ToList();
                        var skillezsesjami = skille.Where(s => s.StudySessions.Count != 0).ToList();
                        for (int i = 0; i < skillezsesjami.Count; i++)
                        {
                            var skill = skillezsesjami[i];
                            Console.WriteLine($"{i+1}) {skill.Name}");
                        }
                        string wybor3 = Console.ReadLine();
                        if (int.TryParse(wybor3, out int number2) && (number2 > 0) && (number2 < skillezsesjami.Count + 1))
                        {
                            Console.WriteLine($"Wybrałeś sesje skilla {skillezsesjami[number2-1]}");
                            lista = WyswietlSesje(context, numer, skillezsesjami[number2 - 1].Name);
                        }
                        else { Console.WriteLine($"{wybor3} nie odpowiada za żadną umiejętność. Powrót do menu"); return; }

                    }
                    else
                    {
                        lista = WyswietlSesje(context, numer);
                    }
                    Console.WriteLine("Wybierz co chcesz zrobić 1) usuń 2) wróc do menu");
                    string wybor4 = Console.ReadLine();
                    if (wybor4 == "1")
                    {
                        Console.WriteLine("Wybierz które chcesz usunąć");
                        for (int i = 0; i < lista.Count; i++)
                        {
                            var sesja = lista[i];
                            Console.WriteLine($"{i+1}) {sesja.Skill.Name}, {sesja.Date:g}, {sesja.DurationMinutes} min, Notatka {sesja.Notes}");
                        }
                        string wybor5 = Console.ReadLine();
                        if (int.TryParse(wybor5, out int number3) && (number3 > 0) && (number3 < lista.Count + 1))
                        {
                            Console.WriteLine($"{lista[number3-1].Skill.Name}, {lista[number3 - 1].Date:g}, {lista[number3 - 1].DurationMinutes} min, Notatka {lista[number3 - 1].Notes} zostanie usunięty 1)potwierdź 2) anuluj");
                            string wybor6 = Console.ReadLine();
                            if (wybor6 == "1")
                            {
                                context.StudySessions.Remove(lista[number3 - 1]);
                                context.SaveChanges();
                            }
                            else { Console.WriteLine("Sesja nie zostanie usunięta"); }
                        }
                        else { Console.WriteLine($"{wybor5} nie odpowiada żadnej sesji"); }
                    }

                }
            }
            else { Console.WriteLine($"{wybor1} to nie 1 bądź 2"); }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Witam w SkillTrainer");
            using var context = new AppDbContext();
            context.Database.Migrate();
            while (true)
            {
                Console.WriteLine(" 1) przegląd i edycja danych 2) dodanie nowych danych");
                string wyb = Console.ReadLine();
                if (wyb == "1")
                {
                    PokazDane(context);
                }
                else if (wyb == "2")
                    {
                    Console.WriteLine("Chcesz dodać nowy skill czy sesję nauki?\n wpisz 1 by dodać skill       wpisz 2 by dodać sesję");
                    string typakcji = Console.ReadLine();
                    if (typakcji == "1")
                    {
                        if (NowySkillKonsola(context) == false) { Console.WriteLine("Skill nie zostanie utworzony, spróbuj jeszcze raz"); }


                    }
                    else if (typakcji == "2")
                    {
                        if (NowaSesjaKonsola(context) == false) { Console.WriteLine("Sesja nie zostanie zapisana. Spróbuj jeszcze raz"); }

                    }
                    else { Console.WriteLine("Nie wpisano 1 ani 2. Spróbuj jeszcze raz"); }
                }
            }
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
            string dbPath = Path.Combine(AppContext.BaseDirectory, "skilltrack.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.Skill)
                .WithMany(s => s.StudySessions)
                .HasForeignKey(s => s.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
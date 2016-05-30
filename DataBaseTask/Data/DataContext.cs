using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using LinqToExcel;

namespace DataBaseTask.Data
{
    public class DataContext : DbContext
    {
        public DataContext(string text = "Server=tcp:anovi-data-server.database.windows.net,1433;Database=Anovi-data;User ID=L0dom@anovi-data-server;Password=1@3$qWeR;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;") 
            : base(text)
        {
            Configuration.AutoDetectChangesEnabled = 
            Configuration.ValidateOnSaveEnabled = false;
        }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Enrollee> Enrollees { get; set; }
        public DbSet<Residence> Residences { get; set; }

        public static void UpdateToDefault()
        {
            using (var dc = new DataContext())
            {
                dc.Exams.Load();
                dc.Residences.Load();
                dc.ExamResults.Load();
                dc.Enrollees.Load();
                dc.SaveChanges();
            }
            DbSetDefaultValue(_defaultExams);
            DbSetDefaultValue(_defaultResidences);
            ClearDbSet<ExamResult>();
            var defaultEnrollees = ReadEnrolleesFromXml();
            DbSetDefaultValue(defaultEnrollees);
        }

        private static IEnumerable<Enrollee> ReadEnrolleesFromXml(string path = "/Data/abit.xls")
        {
            path = HostingEnvironment.MapPath(path);
            using (var context = new DataContext())
            {
                context.Residences.Load();
                var book = new ExcelQueryFactory(path);
                var names = book.GetWorksheetNames();
                var worksheet = book.Worksheet(names.First());
                var tmp = worksheet.ToList()
                    .Select(x => Tuple.Create(x, new Enrollee
                    {
                        DateOfCompletion = DateTime.Parse(x[0]),
                        RegNumber = int.Parse(x[2]),
                        Surname = x[3],
                        Name = x[4],
                        Patronymic = x[5],
                        Gender = x[6] == "1" ? Gender.Boy : Gender.Girl,
                        Birthday = DateTime.Parse(x[7]),
                        School = x[9],
                        YearOfEnd = int.Parse(x[10]),
                        Address = x[11],
                        Residence = context.Residences.Local.FirstOrDefault(y=>y.Id == int.Parse(x[12])),
                        SeriesOfDocument = x[13],
                        NumberOfDocument = x[14],
                        DateOfIssueOfTheDocument = DateTime.Parse(x[15]),
                        TypeOfCompletion = x[16] == "1" ? TypeOfCompletion.Atcertification : TypeOfCompletion.Diploma,
                        SeriesOfDocumentCompletionEducation = x[17],
                        NumberOfDocumentCompletionEducation = x[18],
                    }))
                .ToList();
                foreach (var x in tmp)
                {
                    x.Item2.ExamResults = ReadFromXmlExamResults(context, x.Item1, x.Item2).ToList();
                }
                return tmp.Select(x => x.Item2);
            }
            
        }

        private static IEnumerable<ExamResult> ReadFromXmlExamResults(DataContext context, Row row, Enrollee enrollee)
        {
            var examI = 23;
            var resI = examI + 7;
            context.Exams.Load();
            var empty = context.Exams.Local.First(x=>x.Name=="Не сдавал");
            for (var i = 0; i < 7; i++)
            {
                int eI;
                if (!int.TryParse(row[examI + i], out eI) || eI == 0)
                {
                    yield return new ExamResult
                    {
                        Exam = empty,
                        Enrollee = enrollee,
                        Result = 0
                    };
                    continue;
                }
                //eI += context.Exams.Local.First().Id - 1;
                int eR;
                if (!int.TryParse(row[resI + i], out eR))
                    eR = 0;
                var exam = context.Exams.Local.First(x=> x.Id==eI);
                yield return new ExamResult
                {
                    Exam = exam,
                    Enrollee = enrollee,
                    Result = eR
                };
            }
        }

        private static void DbSetDefaultValue<T>(IEnumerable<T> defaultCollection)
            where T : class
        {
            ClearDbSet<T>();
            AddToDbSet(defaultCollection);
        }
        private static void ClearDbSet<T>() 
            where T : class
        {
            var context = new DataContext();
            var dbSet = context.Set<T>();
            if (dbSet == null) return;
            dbSet.RemoveRange(dbSet);
            context.SaveChanges();
        }

        private static void AddToDbSet<T>(IEnumerable<T> defaultCollection)
            where T : class
        {
            var context = new DataContext();
            var dbSet = context.Set<T>();
            if (dbSet == null) return;
            dbSet.AddRange(defaultCollection);
            context.SaveChanges();
        }

        private static readonly IEnumerable<Exam> _defaultExams =
            "Творческий экзамен (Исполнение программы)\r\nСобеседование(Коллоквиум)\r\nСольфеджио (письменно и устно)\r\nГармония (письменно и устно)\r\nРусский язык и литература (сочинение)\r\nПрофессиональный экзамен (Сольфеджио и элементарная теория музыки - письменно и устно)\r\nДирижирование и коллоквиум\r\nФортепиано\r\nДирижирование и работа с хором\r\nСочинение (композиция) и коллоквиум\r\nМузыкальная литература (устно)\r\nЗвукорежиссура и коллоквиум\r\nПрофессиональный экзамен (Теория музыки:  сольфеджио и гармония - письменно и устно)\r\nЗвукорежиссура\r\nЛитература\r\nРусский язык\r\nМузыкальная литература (устно) и коллоквиум\r\nТворческий экзамен (Дирижирование и Фортепиано)\r\nТворческий экзамен (Звукорежиссура - письменно и устно)\r\nПрофессиональный экзамен (Теория музыки:  сольфеджио и гармония - письменно и устно; Фортепиано)\r\nТворческий экзамен (Гармония - письменно и устно; Музыкальная литература - устно)\r\nПрофессиональный экзамен  (Сольфеджио - письменно и устно; Фортепиано)\r\nТворческий экзамен (Сочинение - представление письменных работ)\r\nПрофессиональный экзамен  (Сольфеджио - письменно и устно; Гармония - письменно и устно; Фортепиано)\r\nТворческий экзамен (Фортепиано)\r\nПрофессиональный экзамен (Теория музыки:  сольфеджио и гармония - письменно и устно;  Музыкальная литература - устно)\r\nТворческий экзамен (Сочинение - представление творческих работ; Музыкальная литература - устно)\r\nИсполнение программы\r\nДирижирование\r\nСочинение: представление  работ\r\nНе сдавал"
            .Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Select(x=> new Exam {Name = x});

        private static readonly IEnumerable<Residence> _defaultResidences = 
            "Екатеринбург\r\nСвердловская область\r\nРеспублика  Адыгея\r\nРеспублика Башкортостан\r\nРеспублика Бурятия\r\nРеспублика Алтай\r\nРеспублика Дагестан\r\nИнгушская Республика\r\nКабардино-Балкарская Республика\r\nРеспублика Калмыкия\r\nКарачаево-Черкесская Республика\r\nРеспублика Карелия\r\nРеспублика Коми\r\nРеспублика Марий-Эл\r\nМордовская Республика\r\nРеспублика Саха (Якутия)\r\nРеспублика Северная Осетия (Алания)\r\nРеспублика Татарстан\r\nРеспублика Тыва\r\nУдмуртская Республика\r\nРеспублика Хакасия\r\nЧеченская Республика  (Ичкерия)\r\nЧувашская Республика\r\nАлтайский край\r\nКраснодарский край\r\nКрасноярский край\r\nПриморский край\r\nСтавропольский край\r\nХабаровский край\r\nАмурская область\r\nАрхангельская область\r\nАстраханская область\r\nБелгородская область\r\nБрянская область\r\nВладимирская область\r\nВолгоградская область\r\nВологодская область\r\nВоронежская область\r\nИвановская область\r\nИркутская область\r\nКалининградская область\r\nКалужская область\r\nКамчатская область\r\nКемеровская область\r\nКировская область\r\nКостромская область\r\nКурганская область\r\nКурская область\r\nЛенинградская область\r\nЛипецкая область\r\nМагаданская область\r\nМосковская область\r\nМурманская область\r\nНижегородская область\r\nНовгородская область\r\nНовосибирская область\r\nОмская область\r\nОренбургская область\r\nОрловская область\r\nПензенская область\r\nПермский край\r\nПсковская область\r\nРостовская область\r\nРязанская область\r\nСамарская область\r\nСаратовская область\r\nСахалинская область\r\nСмоленская область\r\nТамбовская область\r\nТверская область\r\nТомская область\r\nТульская область\r\nТюменская область\r\nУльяновская область\r\nЧелябинская  область\r\nЧитинская область\r\nЯрославская область\r\nМосква\r\nСанкт-Петербург\r\nЕврейская  автономная область\r\nАгинский Бурятский АО\r\nКоми-Пермяцкий АО\r\nКорякский АО\r\nНенецкий АО\r\nТаймырский (Долгано-Ненецкий) АО\r\nУсть-Ордынский Бурятский АО\r\nХанты-Мансийский АО\r\nЧукотский АО\r\nЭвенкийский АО\r\nЯмало-Ненецкий АО\r\nБеларусь\r\nКазахстан\r\nУкраина\r\nУзбекистан\r\nГpузия\r\nAзеpбайджан\r\nМолдова\r\nKиpгизия\r\nТаджикистан\r\nАpмения\r\nТуpкменистан\r\nАбхазия\r\nЭстония\r\nЛитва\r\nЛатвия\r\nЮжная Корея\r\nМонголия\r\nРеспублика Аргентина\r\n"
            .Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new Residence {Name = x});
        
    }
}
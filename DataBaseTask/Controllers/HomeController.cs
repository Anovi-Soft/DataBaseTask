using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DataBaseTask.Data;

namespace DataBaseTask.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Default()
        {
            DataContext.UpdateToDefault();
            return View();
        }
        public ActionResult FirstTask()
        {
            //DataContext.UpdateToDefault();
            var dataContext = new DataContext();
            return View(dataContext.Exams);
        }

        public ActionResult FirstTaskDataInsert(string[] entity, string[] remove)
        {
            var result = RemoveFirstTaskEntities(remove)
                .ToList()
                .Concat(InsertFirstTaskEntities(entity));
            ViewBag.Title = "Задание №1 Результат";
            return View("InsertResult", result
                .Select(x=>Tuple.Create(x.Item1, ((InsertResult)Enum.Parse(typeof(InsertResult), x.Item2)).ToRussian())));
        }
        
        private IEnumerable<Tuple<string, string>> RemoveFirstTaskEntities(string[] remove)
        {
            var firstTaskContext = new DataContext();
            if (remove == null || !remove.Any()) yield break;
            foreach (var key in remove)
            {
                int id;
                if (int.TryParse(key, out id))
                {
                    var exam = firstTaskContext.Exams.FirstOrDefault(x => x.Id == id);
                    var name = exam.Name;
                    firstTaskContext.Exams.Remove(exam);
                    yield return Tuple.Create(name, InsertResult.Removed.ToString());
                    continue;
                }
                yield return Tuple.Create(key, InsertResult.None.ToString());
            }
            firstTaskContext.SaveChanges();
        }

        private IEnumerable<Tuple<string,string>> InsertFirstTaskEntities(string[] entity)
        {
            var firstTaskContext = new DataContext();
            if (entity == null || !entity.Any()) return new List<Tuple<string, string>>();
            var tmp = new List<string>();
            var valid = entity.Select(x =>
            {
                var splt = x.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return Tuple.Create(splt.First(), string.Join(":", splt.Skip(1)));
            })
            .OrderBy(x=> x.Item1.StartsWith("new-block") ? 0 : 1)
            .Select(x=>Tuple.Create(x,Validate(x, tmp)))
            .ToList();
            foreach (var pair in valid.Where(x=>(x.Item2 & InsertResult.Success)!= InsertResult.None))
            {
                switch (pair.Item2)
                {
                    case InsertResult.Added:
                        firstTaskContext.Exams.Add(new Exam {Name = pair.Item1.Item2});
                        break;
                    case InsertResult.Updates:
                        var exam = firstTaskContext.Exams.First(x => x.Id == int.Parse(pair.Item1.Item1));
                        exam.Name = pair.Item1.Item2;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            firstTaskContext.SaveChanges();
            return valid.Select(x=>Tuple.Create(x.Item1.Item2, x.Item2.ToString()));
        }

        private InsertResult Validate(Tuple<string, string> pair, ICollection<string> previousStrings)
        {
            var firstTaskContext = new DataContext();
            var value = pair.Item2;
            int id;
            var guidParsed = int.TryParse(pair.Item1, out id);
            if (!value.Trim().Any()) return InsertResult.Empty;
            var replacedMarks = ReplacePunctuationMarks(value);
            var exams = firstTaskContext.Exams.ToList();
            if (exams.Any(x => ReplacePunctuationMarks(x.Name) == replacedMarks) || 
                previousStrings.Any(x=> ReplacePunctuationMarks(x) == replacedMarks))
                return InsertResult.AlreadyExists;
            previousStrings.Add(pair.Item2);
            if (guidParsed && exams.Any(x=>x.Id == id))
                return InsertResult.Updates;
            return InsertResult.Added;
        }

        readonly Regex _regexMarks = new Regex(@"[^\w\s]");
        private string ReplacePunctuationMarks(string value) =>
            _regexMarks.Replace(value, "");


        public ActionResult SecondTask()
        {
            //DataContext.UpdateToDefault();
            var dataContext = new DataContext();
            return View(dataContext.Enrollees);
        }

        public ActionResult EnrolleeInfo(int id)
        {
            using (var dataContext = new DataContext())
            {
                dataContext.Residences.Load();
                dataContext.Exams.Load();
                Enrollee enrollee;
                if (id == 0)
                {
                    var empty = dataContext.Exams.First(x => x.Name == "Не сдавал");
                    enrollee = new Enrollee
                    {
                            ExamResults = Enumerable.Repeat(new ExamResult
                            {
                                Exam = empty,
                                Result = 0
                            }, 7)
                            .ToList()
                    };
                }
                else
                {
                    enrollee = dataContext.Enrollees
                    .Include(x => x.ExamResults.Select(y => y.Exam))
                    .First(x => x.Id == id);
                }
                return View(new EnrolleInfoModel
                {
                    Enrollee = enrollee,
                    Residences = dataContext.Residences.Local,
                    Exams = dataContext.Exams.Local
                });
            }
        }

        public ActionResult EnrolleeInfoEdit
            (
                int id,
                string surname,
                string name,
                string patronymic,
                DateTime dateOfCompletion,
                Gender gender,
                DateTime birthday,
                int yearOfEnd,
                TypeOfCompletion typeOfCompletion,
                string seriesOfDocumentCompletionEducation,
                string numberOfDocumentCompletionEducation,
                string seriesOfDocument,
                string numberOfDocument,
                DateTime dateOfIssueOfTheDocument,
                int residence,
                string address,
                int[] exam,
                int[] examR
            )
        {
            var old = DateTime.Now.Year - birthday.Year;
            if (old < 15 || old > 100)
            {
                return View((object) "Возраст не моложе 15 лет и не старше 100 ");
            }
            using (var dc = new DataContext())
            {
                var man = id == 0 
                    ? new Enrollee() 
                    : dc.Enrollees
                    .Include(x=>x.ExamResults)
                    .First(x => x.Id == id);
                man.Surname = surname;
                man.Name = name;
                man.Patronymic = patronymic;
                man.DateOfCompletion = dateOfCompletion;
                man.Gender = gender;
                man.Birthday = birthday;
                man.YearOfEnd = yearOfEnd;
                man.TypeOfCompletion = typeOfCompletion;
                man.SeriesOfDocumentCompletionEducation = seriesOfDocumentCompletionEducation;
                man.NumberOfDocumentCompletionEducation = numberOfDocumentCompletionEducation;
                man.SeriesOfDocument = seriesOfDocument;
                man.NumberOfDocument = numberOfDocument;
                man.DateOfIssueOfTheDocument = dateOfIssueOfTheDocument;
                man.Residence = dc.Residences.FirstOrDefault(x => x.Id == residence);
                man.Address = address;
                if (id == 0)
                    man.ExamResults = new List<ExamResult>
                    {
                        new ExamResult(),
                        new ExamResult(),
                        new ExamResult(),
                        new ExamResult(),
                        new ExamResult(),
                        new ExamResult(),
                        new ExamResult(),
                    };
                for (var i = 0; i < 7; i++)
                {
                    man.ExamResults[i].Exam = dc.Exams.First(x => x.Id == exam[i]);
                    man.ExamResults[i].Result = examR[i];
                }
                if (id == 0)
                {
                    dc.Enrollees.Add(man);
                }
                else
                {
                    dc.SaveChanges();
                }
            }
            return View();
        }
    }

    public class EnrolleInfoModel
    {
        public Enrollee Enrollee { get; set; }
        public IEnumerable<Residence> Residences { get; set; }
        public IEnumerable<Exam> Exams { get; set; }
    }

    [Flags]
    public enum InsertResult
    {
        None = 0,
        Added = 1,
        Updates = 2,
        Success = Added | Updates,
        Empty = 4,
        AlreadyExists = 8,
        Removed = 16,
    }

    public static class EnumExtender
    {
        public static string ToRussian(this InsertResult self)
        {
            switch (self)
            {
                case InsertResult.None:
                    return "";
                case InsertResult.Added:
                    return "Добавлено";
                case InsertResult.Updates:
                    return "Обновлено";
                case InsertResult.Success:
                    return "Удачно";
                case InsertResult.Empty:
                    return "Пусто";
                case InsertResult.AlreadyExists:
                    return "Уже существует";
                case InsertResult.Removed:
                    return "Удалено";
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            }
        }
    }
}
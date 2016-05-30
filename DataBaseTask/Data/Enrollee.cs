using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace DataBaseTask.Data
{
    public class Enrollee
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int RegNumber { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Patronymic { get; set; }
        
        public Gender Gender { get; set; }
        
        public DateTime DateOfCompletion { get; set; }
        
        public DateTime Birthday { get; set; }
        
        public int YearOfEnd { get; set; }

        [Required]
        public string School { get; set; }
        
        public TypeOfCompletion TypeOfCompletion { get; set; }

        [Required]
        public string SeriesOfDocumentCompletionEducation { get; set; }

        [Required]
        public string NumberOfDocumentCompletionEducation { get; set; }

        [Required]
        public string SeriesOfDocument { get; set; }

        [Required]
        public string NumberOfDocument { get; set; }
        
        public DateTime DateOfIssueOfTheDocument { get; set; }

        [Required]
        public Residence Residence { get; set; }

        [Required]
        public string Address { get; set; }

        public virtual List<ExamResult> ExamResults { get; set; }
    }

    public enum Gender
    {
        Boy = 1,
        Girl = 2
    }

    public enum TypeOfCompletion
    {
        Atcertification = 1,
        Diploma = 2
    }
}
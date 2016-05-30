using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DataBaseTask.Data
{
    public class ExamResult
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Exam Exam { get; set; }
        
        public int Result { get; set; }

        public Enrollee Enrollee { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PddApp
{
    public class PddQuestion
    {
        public int TicketNumber {get;set;}
        public int QuestionNumber {get;set;}

        public string Question { get; set; }
        public string ImagePath { get; set; }

        public string[] Choices { get; set; }

        public string CorrectAnswer { get; set; }

        public string Tip { get; set; }

        public static PddQuestion Load(int ticket, int question)
        {
            var result = new PddQuestion();



            return result;
        }
    }
}

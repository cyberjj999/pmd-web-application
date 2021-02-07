using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    public static class Globals
    {
            public static double answerscorrect = 0;
            public static double answerswrong = 0;
            public static double delayedanswerscorrect = 0;
            public static double delayedanswerswrong = 0;
            public static int count = 1;

            public static List<DB.DualDatabaseTestSchemeQuestion> GlobalQuestionList = new List<DB.DualDatabaseTestSchemeQuestion>();
            public static List<DB.DualDatabaseTestSchemeTest> GlobalTestList = new List<DB.DualDatabaseTestSchemeTest>();


    }
}
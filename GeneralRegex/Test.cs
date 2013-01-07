using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using GeneralRegex;

namespace GeneralRegexTest
{
    public class Test
    {
        static void S()
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^(?>(0[12])*(?=02e))");
            System.Text.RegularExpressions.MatchCollection matches = regex.Matches("01020102e");

            Console.WriteLine();
            int matchCount = 0;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                matchCount++;
                Console.WriteLine("Match " + matchCount + ": " + (match.Value == null ? "NULL" : match.Value));
                System.Text.RegularExpressions.GroupCollection groups = match.Groups;
                int groupCount = -1;
                foreach (System.Text.RegularExpressions.Group group in groups)
                {
                    groupCount++;
                    Console.WriteLine("\tGroup " + groupCount + ": " + (group.Value == null ? "NULL" : group.Value));
                    System.Text.RegularExpressions.CaptureCollection captures = group.Captures;
                    int capCount = 0;
                    foreach (System.Text.RegularExpressions.Capture capture in captures)
                    {
                        capCount++;
                        Console.WriteLine("\t\tCapture " + capCount + ": " + (capture.Value == null ? "NULL" : capture.Value));
                    }
                }
            }
        }

        static void U()
        {
            Regex<char> regex = new Regex<char>(@"^(?*(/0[/1/2])*(?>=/0/2/e))");
            Console.WriteLine(regex.PrintPattern());
            Console.WriteLine();
            Match<char> match = regex.Match("01020102e".ToArray(), (str, ch) =>
            {
                return str[0] == ch;
            }, RegexOption.None);

            int matchCount = 0;
            while (match.Success)
            {
                Console.WriteLine();
                matchCount++;
                Console.WriteLine("Match " + matchCount + ": " + (match.Value == null ? "NULL" : new string(match.Value.ToArray())));
                Group<char>[] groups = match.Groups.Values.ToArray();
                foreach (Group<char> group in groups)
                {
                    Console.WriteLine("\tGroup " + group.Name + ": " + (group.Value == null ? "NULL" : new string(group.Value.ToArray())));
                    Capture<char>[] captures = group.Captures;
                    int capCount = 0;
                    foreach (Capture<char> capture in captures)
                    {
                        capCount++;
                        Console.WriteLine("\t\tCapture " + capCount + ": " + (capture.Value == null ? "NULL" : new string(capture.Value.ToArray())));
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
                match = regex.MatchNext();
            }
        }

        static void Main(string[] args)
        {
            Debug.Listeners.Add(new ConsoleTraceListener());
            U();
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}

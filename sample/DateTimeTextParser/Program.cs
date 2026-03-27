using System;
using DateTimeParser;

ParseAndPrint("2017-01-01");
ParseAndPrint("2017-01-01 05:28:10");
ParseAndPrint("2017-01-01 05:28");
ParseAndPrint("2017-01-01T05:28:10");
ParseAndPrint("2017-01-01T05:28");
ParseAndPrint("2017-01-01 05:x8:10");
return;

static void ParseAndPrint(string input)
{
    try
    {
        var dt = DateTimeTextParser.Parse(input);
        Console.WriteLine("Input: \"{0}\", Parsed value: \"{1:o}\"", input, dt);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Input: \"{0}\"", input);
        Console.WriteLine(ex.ToString());
        Console.ForegroundColor = ConsoleColor.White;
    }
}

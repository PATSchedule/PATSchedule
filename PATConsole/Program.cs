using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Windows;

const int EXIT_SUCCESS = 0;
const int EXIT_FAILURE = 1;

try
{
    // russo sparche fix
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    var m = new PATShared.Schedule();
    await m.FetchSchedule(new DateTime(2021, 12, 1, 0, 0, 0, 0));
    var asdasdsa = m.GetScheduleForGroup("МХ-21-2");

    Console.WriteLine(asdasdsa);
    
    Console.ReadKey(true);

    return EXIT_SUCCESS;
}
catch (Exception exc)
{
    Console.Error.WriteLine("-- Exception:");
    Console.Error.WriteLine(exc);
    Console.Error.WriteLine("-- Exception end.");
    return EXIT_FAILURE;
}

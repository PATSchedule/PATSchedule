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

    var m = new PATShared.Moodle();

    const string mUSERNAME = "[redacted]@permaviat.ru";
    const string mPASSWORD = "[redacted]";

    // делаем логин
    var mtoken = await m.LoginNew(mUSERNAME, mPASSWORD);


    /*
    var eventsinfo = await m.Request<JObject>(
                mtoken,
                "core_calendar_get_action_events_by_timesort",
                new { timesortfrom = (int)PATShared.Utils.GetLocalUnixTime(), limittononsuspendedevents = true }
            );
    */
    var asubstatus = await m.Request<JObject>(mtoken, "mod_assign_get_submission_status",
                            "userid=2281&assignid=1032"
    );

    Console.WriteLine(asubstatus);
    
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

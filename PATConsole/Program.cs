using System.Text;

const int EXIT_SUCCESS = 0;
const int EXIT_FAILURE = 1;

try
{
    // russo sparche fix
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    
    var userstorage = new PATShared.Students();
    await userstorage.Load();
    // user ids will start with "TG_" if the student registered from Telegram
    // or "VK_" if from VK.com, then followed by their profile id (on VK it's called a Profile ID, in TG it's a User ID)
    userstorage.SetUser("TG_1234567", new PATShared.StudentInfo("МХ-21-2" /* ... */));

    var myuser = userstorage.GetUser("TG_1234567");

    var schstorage = new PATShared.Schedule();
    await schstorage.FetchSchedule(new DateTime(2021, 10, 22));

    var myschedule = schstorage.GetScheduleForGroup(myuser.Group);

    Console.WriteLine($"Schedule for {myuser.Group}:");
    foreach (var elem in myschedule)
    {
        Console.WriteLine($"Lesson {elem.Para}: {elem.Subject} [{elem.Room}].");
    }

    Console.WriteLine("End.");
    return EXIT_SUCCESS;
}
catch (Exception exc)
{
    Console.Error.WriteLine("-- Exception:");
    Console.Error.WriteLine(exc);
    Console.Error.WriteLine("-- Exception end.");
    return EXIT_FAILURE;
}

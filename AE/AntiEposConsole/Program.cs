// See https://aka.ms/new-console-template for more information
Console.OutputEncoding = System.Text.Encoding.UTF8;
await Console.Out.WriteLineAsync("Hello, World!");


var login = "[][][]"; // email
var pass = "[][][]";

var epos = new AntiEpos.AEClient();

await epos.Login(login, pass);

// законы российской федерации запрещают онлайн сервисам обрабатывать данные пользователей без их
// информированного согласия, РСААГ этим законам тоже подчиняется, по этому там и есть вызов check_agreement
var agdata = await epos.CheckAgreement();

// ...... ;-;
if (!agdata.Status || agdata.Data is null || !agdata.Data.AgreedUser)
{
    await Console.Out.WriteLineAsync("User did not agree to data processing.");
    return 1;
}

// зайти под студента
await epos.Authenticate(AntiEpos.AEAuthMode.Student);

var userinfo = epos.UserInfo;
if (userinfo is null || userinfo.AcademicYears is null || userinfo.Sessions is null || userinfo.AuthToken is null || userinfo.Sessions.Profiles is null)
{
    await Console.Out.WriteLineAsync("None or incomplete user info.");
    return 1;
}

await Console.Out.WriteLineAsync($"--------------------------------- ОБЩАЯ ИНФОРМАЦИЯ");
await Console.Out.WriteLineAsync($"звать {userinfo.Sessions.FirstName} {userinfo.Sessions.LastName} {userinfo.Sessions.MiddleName}");
await Console.Out.WriteLineAsync($"родился {userinfo.Sessions.DateOfBirth}");
await Console.Out.WriteLineAsync($"пол {userinfo.Sessions.Sex}");
await Console.Out.WriteLineAsync($"нужна смена пароля? {userinfo.Sessions.PasswordChangeRequired}");
await Console.Out.WriteLineAsync($"--------------------------------- АКАДЕМ ПРОФИЛИ");

foreach (var it in userinfo.Sessions.Profiles)
{
    await Console.Out.WriteLineAsync($"---- ПРОФИЛЬ");
    await Console.Out.WriteLineAsync($"{it.UserId} - {it.SchoolShortname}");
    await Console.Out.WriteLineAsync($"по умолчанию? {it.IsDefault}");
    await Console.Out.WriteLineAsync($"тип {it.Type}");
}

await Console.Out.WriteLineAsync($"--------------------------------- АКАДЕМ ГОДА");

foreach (var it in userinfo.AcademicYears)
{
    await Console.Out.WriteLineAsync($"---- ГОД");
    await Console.Out.WriteLineAsync($"№{it.Id} - '{it.BeginDate}' - '{it.EndDate}' - '{it.Name}'");
}

var myprogress = await epos.FetchProgress(userinfo.AcademicYears.Last().Id, true);

await Console.Out.WriteLineAsync("end!");

await Console.In.ReadLineAsync();

return 0;


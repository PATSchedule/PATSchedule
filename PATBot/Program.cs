using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PATBot
{
    public static class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();
        static PATShared.Students Students = new PATShared.Students();

        static long MyId = 0;

        static ReplyKeyboardRemove RemoveKeyboard = new ReplyKeyboardRemove();
        static InlineKeyboardButton[][] InlineDateButtons = new InlineKeyboardButton[][] {
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Позавчера", "s-2.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Вчера", "s-1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Сегодня", "s0.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Завтра", "s1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Послезавтра", "s2.0") }
        };
        static InlineKeyboardButton[][] InlineDateButtonsPoned = new InlineKeyboardButton[][] {
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Позавчера", "s-2.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Вчера", "s-1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Сегодня", "s0.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Завтра", "s1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Понедельник", "s3.0") }
        };
        static InlineKeyboardButton[][] InlineDateButtonsNegPoned = new InlineKeyboardButton[][] {
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Понедельник", "s-3.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Вчера", "s-1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Сегодня", "s0.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Завтра", "s1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Послезавтра", "s2.0") }
        };
        static InlineKeyboardMarkup InlineDateMarkup = new InlineKeyboardMarkup(InlineDateButtons);
        static InlineKeyboardMarkup InlineDateMarkupNegPoned = new InlineKeyboardMarkup(InlineDateButtonsNegPoned);
        static InlineKeyboardMarkup InlineDateMarkupPoned = new InlineKeyboardMarkup(InlineDateButtonsPoned);

        const string NAME_BANNED = "❌ ой ой, вы быть забанен из данный бот во слава великий китай компартия 🇨🇳🇨🇳🇨🇳🇨🇳🇨🇳 лидер xi.";
        const string NAME_SCHEDULE = "📅 Расписание";
        const string NAME_HOMEWORK = "💼 Задания";
        const string NAME_ABOUTBOT = "🐯 О боте";
        const string NAME_CHANGEGR = "⚙️ Сменить группу";
        const string NAME_EPOS = "🐻 ЭПОС";

        // 'Just Zoo It!' pack:
        const string NAME_PIGSTICKER = "CAACAgIAAxkBAAICfWF5bV4fOwncaPA-UIRE36ze22LnAALoAAP0exkAAcnMJRAtN9vTIQQ";
        static string[] NAME_CATSTICKERS = new string[]
        {
            "CAACAgIAAxkBAAICg2F5bz83bppeNgWEHvJwyc38Od0yAALgAAP0exkAASzrKFdeleMwIQQ",
            "CAACAgIAAxkBAAIChWF5b2NOdVV0-TmACnL-KAmb62F0AAL4AAP0exkAAQtRF3q3XTjpIQQ",
            "CAACAgIAAxkBAAICh2F5b3vxeL4zu2beyPqsTPSnGwwQAAMBAAL0exkAAfRksArkn456IQQ",
            "CAACAgIAAxkBAAICiWF5b54h0kNnRH69lL_NB0j6OHx7AAIRAQAC9HsZAAH1nz_2mMhZ0CEE"
        };

        static KeyboardButton[][] MenuButtons = new KeyboardButton[][]
        {
            new KeyboardButton[]{ NAME_SCHEDULE },
            new KeyboardButton[]{ NAME_HOMEWORK },
            new KeyboardButton[]{ NAME_EPOS     },
            new KeyboardButton[]{ NAME_ABOUTBOT },
            new KeyboardButton[]{ NAME_CHANGEGR }
        };

        static Random Rnd = new Random();

        static ReplyKeyboardMarkup MenuMarkup = new ReplyKeyboardMarkup(MenuButtons) { ResizeKeyboard = true, OneTimeKeyboard = true };

        static InlineKeyboardMarkup EposMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("📅 Д/З ЭПОС", "eHw"),
                    InlineKeyboardButton.WithCallbackData("💯 Оценки ЭПОС", "eGrades"),
                    InlineKeyboardButton.WithCallbackData("❌ Выйти", "eDel"),
                });

        static InlineKeyboardMarkup MoodleMarkupDelete = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("❌ Выйти", "m0"),
                    InlineKeyboardButton.WithCallbackData("➡️", "m1")
                });

        static InlineKeyboardMarkup MoodleMarkupBoth = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️", "m-1"),
                    InlineKeyboardButton.WithCallbackData("➡️", "m1")
                });

        static InlineKeyboardMarkup MoodleMarkupLeft = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️", "m-1"),
                    //InlineKeyboardButton.WithCallbackData("➡️", "m1")
                });

        static InlineKeyboardMarkup MoodleMarkupRight = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    //InlineKeyboardButton.WithCallbackData("⬅️", "m-1"),
                    InlineKeyboardButton.WithCallbackData("➡️", "m1")
                });

        class MoodleListTag
        {
            public class MoodlePageInfo
            {
                public string? contents { get; set; }
            }

            public int page = -1;
            public DateTime startfrom = DateTime.Now;
            public List<MoodlePageInfo> pages = new List<MoodlePageInfo>();
        }

        static async Task<Tuple<string, InlineKeyboardMarkup?>> PrintEposInfo(string rsaagLogin, string rsaagPassword, string patuserid)
        {
            var sb = new StringBuilder();
            InlineKeyboardMarkup? ikm = null;
            var patsi = Students.GetUser(patuserid);
            if (patsi is null || patsi.RsaagPassword is null)
            {
                throw new InvalidOperationException("User is null..?");
            }

            try
            {
                using var epos = new AntiEpos.AEClient();
                await PATShared.EposTagClass.EnsureEposAuth(epos, rsaagLogin, rsaagPassword);
                await epos.Authenticate(AntiEpos.AEAuthMode.Student);

                var user = epos.UserInfo;

                // ох...
                if (user is null
                    || user.Sessions is null
                    || user.AcademicYears is null
                    || user.AuthToken is null
                    || user.Sessions.Profiles is null
                    || user.Sessions.Profiles.Length <= 0
                    )
                    throw new InvalidOperationException("Недостаточно информации о пользователе");

                var acyear = user.AcademicYears.LastOrDefault();
                if (acyear is null)
                    throw new InvalidOperationException("Нет академических годов");

                var progress = await epos.FetchProgress(acyear.Id);
                if (progress is null || progress.Length <= 0)
                    throw new InvalidOperationException("Недостаточно информации об оценках");

                var myprof = user.Sessions.Profiles.Where(x => x.IsDefault).FirstOrDefault();
                if (myprof is null)
                    throw new InvalidOperationException("Не назначен профиль по умолчанию");

                sb.AppendLine($"Профиль ЭПОС по умолчанию:");
                sb.AppendLine($"{user.Sessions.FirstName} {user.Sessions.LastName}");
                sb.AppendLine($"{myprof.SchoolShortname}");
                sb.AppendLine($"Текущий академ. год: {acyear.Id}|{acyear.Name}");
                sb.AppendLine($"Сам ЭПОС, если что, здесь: https://school.permkrai.ru/desktop");

                patsi.EposTag = new PATShared.EposTagClass() { Progress = progress };

                ikm = EposMarkup;
            }
            catch (Exception exc)
            {
                sb.AppendLine("❌ Произошла ошибка получения данных из РСААГ:\n" + exc.ToString());
            }

            return Tuple.Create(sb.ToString(), ikm);
        }

        static async Task<Tuple<string, InlineKeyboardMarkup?>> PrintMoodleInfo(string moodletoken, string patuserid)
        {
            var sb = new StringBuilder();
            var m = new PATShared.Moodle(PATShared.Schedule.client);
            InlineKeyboardMarkup? ikm = null;

            try
            {
                var userinfo = await m.Request<PATShared.MoodleSiteInfoReply>(
                    moodletoken,
                    "core_webservice_get_site_info"
                );

                var assigninfo = await m.Request<PATShared.MoodleAssignmentsReply>(
                    moodletoken,
                    "mod_assign_get_assignments"
                );

                var patsi = Students.GetUser(patuserid);
                if (patsi is null)
                {
                    throw new InvalidOperationException("User is null..?");
                }

                if (assigninfo is null || assigninfo.courses is null || assigninfo.courses.Length < 1)
                {
                    throw new InvalidOperationException("Assignments block is null.");
                }

                var tag = new MoodleListTag();

                sb.AppendLine($"Вы зашли в [{userinfo.sitename}]({userinfo.siteurl})\nкак [{userinfo.firstname}]({userinfo.siteurl}/user/profile.php?id={userinfo.userid})\nИспользуйте кнопки ниже для навигации по курсам.\n\nДанные актуальны на {tag.startfrom.ToString("d", PATShared.Schedule.my_culture)}.\nДля выхода из Moodle нажмите на ❌.\nТЕСТ: версия мудл у сайта={userinfo.release}"); 

                var i = 1;

                foreach (var e in assigninfo.courses)
                {
                    if (e is null || e.assignments is null)
                    {
                        throw new InvalidOperationException("Course block is null.");
                    }

                    var j = 0;
                    var coursename = e.shortname;
                    var courseurl = $"{userinfo.siteurl}/course/view.php?id={e.id}";

                    if (e.assignments.Length < 1) continue;

                    var astr = $"Страница {i} из {{0}} (одна страница - один предмет)\n\n";
                    astr += $"[{coursename}]({courseurl}):\n";

                    foreach (var a in e.assignments)
                    {
                        if (a.allowsubmissionsfromdate == 0 || a.duedate == 0) continue;
                        var astart = PATShared.Utils.GetLocalFromUnixTime(a.allowsubmissionsfromdate);
                        if (tag.startfrom < astart) continue;

                        var asubstatus = await m.Request<PATShared.MoodleSubmissionStatus>(moodletoken, "mod_assign_get_submission_status",
                            $"userid={userinfo.userid}&assignid={a.id}"
                        );

                        if (asubstatus is null || asubstatus.lastattempt is null || asubstatus.lastattempt.submission is null)
                        {
                            //throw new InvalidOperationException("Submission status is null.");
                            continue;
                        }

                        var astat = asubstatus.lastattempt.submission.status;
                        var asubdat = Math.Max(asubstatus.lastattempt.submission.timemodified, asubstatus.lastattempt.submission.timecreated);
                        var adate = "";

                        if (asubdat is long asubdat_)
                        {
                            adate = PATShared.Utils.GetLocalFromUnixTime(asubdat_).ToString("d MMM HH:mm:ss", PATShared.Schedule.my_culture);
                        }

                        var aurl = $"{userinfo.siteurl}/mod/assign/view.php?id={a.cmid}";
                        var aname = a.name;
                        var adt = PATShared.Utils.GetLocalFromUnixTime(a.duedate);
                        var abegins = PATShared.Utils.GetLocalFromUnixTime(a.allowsubmissionsfromdate).ToString("d MMM HH:mm:ss", PATShared.Schedule.my_culture);
                        var adts = adt.ToString("d MMM HH:mm:ss", PATShared.Schedule.my_culture);

                        astr += $" - [{aname}]({aurl}), ";

                        if (astat is null || astat == "new")
                            astr += $"⏰ сдать до {adts}!";
                        else
                            astr += $"✅ уже сдано {adate}.";

                        if (tag.startfrom < astart)
                            astr += $" (откроется {abegins})";

                        astr += "\n";

                        ++j;
                    }

                    if (j > 0)
                    {
                        tag.pages.Add(new MoodleListTag.MoodlePageInfo() { contents = astr });
                        ++i;
                    }
                }

                ikm = MoodleMarkupDelete;

                patsi.Tag = tag;
                Students.SetUser(patuserid, patsi);

            }
            catch (Exception exc)
            {
                sb.AppendLine("❌ Произошла ошибка получения данных из Moodle:\n" + exc.ToString());
            }

            return Tuple.Create(sb.ToString(), ikm);
        }

        static void FixConsole()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch
            {
                // if we have no console or can't do it.
            }
        }

        static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var msg = "";

            if (exception is ApiRequestException are)
            {
                msg = $"Telegram API Error:\n[{are.ErrorCode}]\n{are.Message}\n";
                foreach (DictionaryEntry kvp in are.Data)
                {
                    msg += $"{kvp.Key} = {kvp.Value}\n";
                }
            }
            else
            {
                msg = exception.ToString() + "\n";
            }

            await Console.Error.WriteLineAsync(msg);
        }

        static async Task HandleUpdateMyChatMemberAsync(ITelegramBotClient botClient, ChatMemberUpdated upd, CancellationToken cancellationToken)
        {
            var stat = upd.NewChatMember.Status;
            var userId = upd.NewChatMember.User.Id;
            var userName = upd.NewChatMember.User.Username;

            if (userId == MyId)
            {
                await Console.Out.WriteLineAsync("Wtf?!");
                userId = upd.From.Id;
                userName = upd.From.Username;
            }

            var deluserid = "TG_" + userId.ToString();

            if (stat == ChatMemberStatus.Kicked || stat == ChatMemberStatus.Left)
            {
                if (cancellationToken.IsCancellationRequested) return;
                Students.DelUser(deluserid);
                await Console.Out.WriteLineAsync($"User deregistered: {deluserid}, @{userName}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"User started dialog: {deluserid}, @{userName}");
            }
        }

        static InlineKeyboardMarkup GetDateMarkup()
        {
            // послезавтра==воскресенье => понедельник
            // позавчера==воскресенье => понедельник
            return
                (DateTime.Now.AddDays(2.0).DayOfWeek == DayOfWeek.Sunday) ? InlineDateMarkupPoned :
                (DateTime.Now.AddDays(-2.0).DayOfWeek == DayOfWeek.Sunday) ? InlineDateMarkupNegPoned :
                InlineDateMarkup;
        }

        static async Task HandleUpdateCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery upd, CancellationToken cancellationToken)
        {
            Rnd.Next();
            var cberr = false;
            var msg = "Ошибка: ";
            var chatId = upd.Message.Chat.Id;
            var userId = upd.From.Id;
            var cbuserid = "TG_" + userId.ToString();
            var msgId = upd.Message.MessageId;
            var mydt = DateTime.Today;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var myuser = Students.GetUser(cbuserid);
            var imr = GetDateMarkup();

            var mystr = upd.Data ?? "";

            if (myuser is null)
            {
                cberr = true;
                msg += "Пользователь не найден. ";
            }
            else if (!cberr && mystr.StartsWith('e'))
            {
                imr = EposMarkup;

                if (myuser.EposTag is null)
                {
                    if (myuser.RsaagPassword != "")
                        await PrintEposInfo(myuser.RsaagLogin, myuser.RsaagPassword, cbuserid); // создаст тэг
                    else
                    {
                        imr = null;
                        cberr = true;
                        msg += "Нет данных авторизации РСААГ. Войдите снова ";
                    }
                }

                //else
                {
                    msg = "";
                    switch (mystr)
                    {
                        case "eGrades":
                            {
                                foreach (var g in myuser.EposTag.Progress)
                                {
                                    msg += $"{g.SubjectName}(ср.={g.AverageFive}): ";
                                    foreach (var periods in g.Periods)
                                    {
                                        foreach (var marks in periods.Marks)
                                        {
                                            foreach (var markvalues in marks.Values)
                                            {
                                                msg += $"{(int)markvalues.Five} ";
                                            }
                                        }
                                    }

                                    msg += "\n";
                                }

                                break;
                            }

                        case "eDel":
                            {
                                myuser.RsaagLogin = "";
                                myuser.RsaagPassword = "";
                                Students.SetUser(cbuserid, myuser);
                                msg = "🗑 Ваши данные авторизации РСААГ были удалены, нажмите на кнопку ЭПОС ещё раз для авторизации.";
                                imr = null;
                                break;
                            }
                    }
                }
            }
            else if (!cberr && mystr.StartsWith('s'))
            {
                mystr = mystr.Substring(1);

                mydt.AddDays(double.Parse(mystr, CultureInfo.InvariantCulture));

                if (mydt.DayOfWeek == DayOfWeek.Sunday)
                {
                    cberr = true;
                    msg += "Не могу показать расписание на воскресенье. ";
                }

                if (!cberr)
                {
                    var cmysch = new PATShared.Schedule();
                    await cmysch.FetchSchedule(mydt);
                    
                    if (myuser is null)
                    {
                        cberr = true;
                        msg += "Не могу найти пользователя. ";
                    }
                    else
                    {
                        msg = $"📅 {myuser.Group}, {mydt.ToString("dddd d MMMM yyyy", PATShared.Schedule.my_culture)}:\n";
                        var mysch = cmysch.GetScheduleForGroup(myuser.Group);

                        var appnd = "\n";

                        if (mysch is null)
                        {
                            msg += "- Ошибка, попробуй поменять группу или просто попробуй ещё раз.";
                        }
                        else
                        {
                            var suffixes = new List<string>();
                            var si = 0;
                            //var lastb = PATShared.Building.UNKNOWN;
                            var hassport = mysch.Any(a => a.Room.Trim().ToLower(PATShared.Schedule.my_culture) == "спортзал");

                            var prep = " ⏰ ";

                            for (si = 0; si < mysch.Count; ++si)
                            {
                                if (mysch[si].CanIgnore()) continue;

                                if (mydt.Day == 30 && mydt.Month == 12 && mydt.Year == 2021)
                                {
                                    // см. https://permaviat.ru/news/2021/12/25/950/
                                    // можно было вынести такие вот внеплановые замены в отдельный JSON, но мне лень.
                                    // новый год же!
                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.P1)[mysch[si].Para - 1]);
                                    appnd = "(пара час 30дек, см. сайт техникума)";
                                }
                                else if (mysch[si].Para == 0)
                                {
                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.UNK)[0]);
                                    appnd = "(практика)";
                                }
                                else if (mydt.DayOfWeek == DayOfWeek.Saturday)
                                {
                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.SUB)[mysch[si].Para - 1]);
                                    appnd = "(субботнее расписание звонков)";
                                }
                                else if (mysch[si].Room.ToLower(PATShared.Schedule.my_culture) == "спортзал" || mysch[si].Room == "")
                                {
                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.A1)[mysch[si].Para - 1]);
                                    appnd = "(спортзал, предполагаю А 1 этаж)";
                                    hassport = true;
                                }
                                else
                                {
                                    switch (mysch[si].Room[0])
                                    {
                                        case 'С':
                                            {
                                                suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.C)[mysch[si].Para - 1]);
                                                appnd = "(строительный корпус)";
                                                break;
                                            }

                                        case 'Т':
                                            {
                                                var parsedok = int.TryParse(mysch[si].Room.Split('-')[1], out int cabnum);
                                                if (!parsedok) cabnum = 100;

                                                if (cabnum % 2 == 0)
                                                {
                                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.T1)[mysch[si].Para - 1]);
                                                    appnd = "(корпус Т чёт)";
                                                }
                                                else
                                                {
                                                    suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.T2)[mysch[si].Para - 1]);
                                                    appnd = "(корпус Т нечёт)";
                                                }

                                                break;
                                            }

                                        case 'А':
                                            {
                                                var cabfloor = mysch[si].Room.Split('-')[1][0];

                                                // если в расписании есть спортзал, то 1 этаж...
                                                // if (hassport) cabfloor = '1';
                                                // оказывается так делать не надо??

                                                switch (cabfloor)
                                                {
                                                    case '1':
                                                        {
                                                            suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.A1)[mysch[si].Para - 1]);
                                                            appnd = "(А 1 этаж)";
                                                            break;
                                                        }

                                                    case '2':
                                                        {
                                                            suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.A2)[mysch[si].Para - 1]);
                                                            appnd = "(А 2 этаж)";
                                                            break;
                                                        }

                                                    case '3':
                                                        {
                                                            suffixes.Add(prep + PATShared.Utils.FetchClockSchedule( PATShared.Building.A3)[mysch[si].Para - 1]);
                                                            appnd = "(А 3 этаж)";
                                                            break;
                                                        }
                                                }

                                                break;
                                            }

                                        default:
                                            {
                                                suffixes.Add(prep + " 🤷");
                                                appnd = "(неизвестное расписание звонков)";
                                                break;
                                            }
                                    }
                                }
                            }

                            if (cmysch.ReplacementsUsed)
                            {
                                appnd += $"\n\nЗамены применены из: [{cmysch.ReplacementFile}]({cmysch.ReplacementUrl})";
                            }
                            else
                            {
                                appnd += "\n";
                            }

                            si = 0;
                            foreach (var par in mysch)
                            {
                                if (par.CanIgnore()) continue;
                                msg += par.ToString().Trim() + suffixes[si++] + "\n";
                            }

                            msg += appnd;
                        }
                    }
                }

                var dtnow = DateTime.Now;
                msg += "\nДанные актуальны на " + dtnow.ToString("dd.MM.yyyy HH:mm.ss", PATShared.Schedule.my_culture);
                // 31 декабря
                if (dtnow.Day == 31 && dtnow.Month == 12)
                {
                    // последний эмодзи: юникод кодпоинт U+1FAC2 "PEOPLE HUGGING EMOJI"
                    msg += "\nС новым годом вас, от PATSchedule! 🐯🍰🫂";
                    msg += "\nЛучше отдохните, а не расписание смотрите 🙃...";
                }
            }
            else if (!cberr && mystr.StartsWith('m'))
            {
                imr = null;
                mystr = mystr.Substring(1);

                var mdir = int.Parse(mystr);
                if (mdir == 0)
                {
                    myuser.MoodleToken = "";
                    Students.SetUser(cbuserid, myuser);

                    msg = "Ваши данные авторизации Moodle были удалены. Нажмите ещё раз на кнопку 'Задания' для авторизации.";
                }
                else
                {
                    var mtag = (MoodleListTag?)myuser.Tag;

                    if (mtag is MoodleListTag)
                    {
                        mtag.page += mdir;

                        if (mtag.page >= mtag.pages.Count) mtag.page = mtag.pages.Count - 1;
                        if (mtag.page < 0) mtag.page = 0;

                        msg = string.Format(mtag.pages[mtag.page].contents ?? "ОШИБКА {0} ОШИБКА", mtag.pages.Count);

                        imr = MoodleMarkupBoth;
                        if (mtag.page < 1) imr = MoodleMarkupRight;
                        else if (mtag.page >= mtag.pages.Count - 1) imr = MoodleMarkupLeft;
                    }
                    else
                    {
                        cberr = true;
                        msg += "Нет данных Moodle. o_O ";
                    }
                }
            }

            await botClient.EditMessageTextAsync(
                chatId: chatId,
                messageId: msgId,
                text: msg,
                parseMode: ParseMode.Markdown,
                replyMarkup: imr,
                cancellationToken: cancellationToken
            );

            await botClient.AnswerCallbackQueryAsync(
                upd.Id,
                cancellationToken: cancellationToken
            );
        }

        static async Task HandleOnMessageAsync(ITelegramBotClient botClient, Message upd, CancellationToken cancellationToken)
        {
            IReplyMarkup? replyKeyboardMarkup = MenuMarkup;
            var chatId = upd.Chat.Id;
            var userId = upd.From.Id;
            var patuserid = "TG_" + userId.ToString();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (upd.Type != MessageType.Text)
            {
                await botClient.SendTextMessageAsync(chatId, "Прости, пока я понимаю только текст, но если тигр будет прилежно учиться, тигр будет понимать и стикеры!", cancellationToken: cancellationToken);
                return;
            }
            
            var chatTxt = upd.Text;

            if (Students.GetUser(patuserid) is PATShared.StudentInfo patsi)
            {
                // здесь мы получаем группу...
                if (patsi.Group == "")
                {
                    var mygroup = chatTxt.Trim().Replace(' ', '-').Replace(".", "").ToUpper(PATShared.Schedule.my_culture);
                    var msg = $"Твоя группа: {mygroup}\nЧто вас интересует?";

                    // TODO: проверить название группы на правильность...
                    var parts = mygroup.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    var validgroup =
                        parts.Length == 3
                        && parts[0].Length == 2
                        && parts[1].Length == 2
                        && parts[2].Length == 1
                        && ushort.TryParse(parts[1], out ushort _p1)
                        && ushort.TryParse(parts[2], out ushort _p2)
                        && _p1 > 0
                        && _p2 > 0;

                    if (validgroup)
                    {
                        Students.SetUser(patuserid, new PATShared.StudentInfo(mygroup));
                    }
                    else
                    {
                        msg = "Группа введена некорректно. Попробуй ещё раз.";
                        replyKeyboardMarkup = RemoveKeyboard;
                    }

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: msg,
                        cancellationToken: cancellationToken,
                        replyMarkup: replyKeyboardMarkup
                    );
                }
                else
                {
                    // надо отправить сообщение.
                    string msg = "ошибка";
                    var sendout = true;
                    switch (chatTxt)
                    {
                        case NAME_ABOUTBOT:
                            {
                                msg = "Это глупый бот, который набросал @nikthecat из группы МХ-21-2.\nО всех проблемах писать тоже ему. (или бить, легонько)\nБот написан на C#, вертится на личном компе сабжа, исходный код доступен [по ссылке](https://github.com/PATSchedule/PATSchedule).\n\n(да, аватарка это мой плюшевый тигр, мне он нраица и менять НЕ БУДУ!!!)";
                                break;
                            }

                        case NAME_CHANGEGR:
                            {
                                msg = $"Твоя текущая группа: {patsi.Group}\nПришли новую группу одним сообщением, как раньше.";
                                Students.SetUser(patuserid, new PATShared.StudentInfo(""));
                                replyKeyboardMarkup = RemoveKeyboard;
                                break;
                            }

                        case NAME_EPOS:
                            {
                                if (patsi.RsaagLogin == "" || patsi.RsaagPassword == "" || patsi.RsaagLogin == "$!WAIT")
                                {
                                    msg = "🐻 РСААГ Авторизация (powered by AntiEpos!)\nПришлите ваш логин и пароль от РСААГ через пробел.\nПример:\nMoodle-Lucshe@po-faktam.ru parol123\nАвторизация через госуслуги НЕ работает, даже не просите.";
                                    patsi.RsaagLogin = "$!WAIT";
                                    Students.SetUser(patuserid, patsi);
                                }
                                else
                                {
                                    sendout = false;

                                    var _msgwait = await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "⏲ РСААГ авторизация...",
                                        cancellationToken: cancellationToken
                                    );

                                    var tt = await PrintEposInfo(patsi.RsaagLogin, patsi.RsaagPassword, patuserid);
                                    msg = tt.Item1;

                                    await botClient.EditMessageTextAsync(
                                        _msgwait.Chat.Id,
                                        _msgwait.MessageId,
                                        text: msg,
                                        parseMode: ParseMode.Markdown,
                                        replyMarkup: tt.Item2,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                break;
                            }

                        case NAME_HOMEWORK:
                            {
                                if (patsi.MoodleToken == "" || patsi.MoodleToken == "$!WAIT")
                                {
                                    msg = "Вы не авторизованы в Moodle, пришлите ваш логин и пароль от edu.permaviat.ru одним сообщением через пробел.\nПример:\nAD-21-1-10@permaviat.ru AbCdEfGh1234Ijk";
                                    patsi.MoodleToken = "$!WAIT";
                                    Students.SetUser(patuserid, patsi);
                                }
                                else
                                {
                                    sendout = false;

                                    var _msgwait = await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "⏲ Подождите пожалуйста, связываюсь с Moodle...",
                                        cancellationToken: cancellationToken
                                    );

                                    var tt = await PrintMoodleInfo(patsi.MoodleToken, patuserid);
                                    msg = tt.Item1;

                                    await botClient.EditMessageTextAsync(
                                        _msgwait.Chat.Id,
                                        _msgwait.MessageId,
                                        text: msg,
                                        parseMode: ParseMode.Markdown,
                                        replyMarkup: tt.Item2,
                                        cancellationToken: cancellationToken
                                    );
                                }

                                break;
                            }

                        case NAME_SCHEDULE:
                            {
                                msg = "На какой день показать?\nПока можно только кнопками снизу:";
                                replyKeyboardMarkup = GetDateMarkup();
                                break;
                            }

                        case "pig":
                        case "пиг":
                        case "hru":
                        case "hrue":
                        case "хрю":
                        case "hruxe":
                        case "hruxa":
                        case "хрюха":
                            {
                                sendout = false;

                                await botClient.SendStickerAsync(
                                    chatId: chatId,
                                    sticker: new Telegram.Bot.Types.InputFiles.InputOnlineFile(NAME_PIGSTICKER),
                                    cancellationToken: cancellationToken,
                                    disableNotification: true
                                );

                                break;
                            }

                        case "cat":
                        case "кот":
                        case "meow":
                        case "мяу":
                        case "кися":
                        case "китя":
                        case "котик":
                        case "кошка":
                        case "koshq":
                        case "koshque":
                            {
                                sendout = false;

                                await botClient.SendStickerAsync(
                                    chatId: chatId,
                                    sticker: new Telegram.Bot.Types.InputFiles.InputOnlineFile(NAME_CATSTICKERS[Rnd.Next(0, NAME_CATSTICKERS.Length)]),
                                    cancellationToken: cancellationToken,
                                    disableNotification: true
                                );

                                break;
                            }

                        case "/start":
                            {
                                msg = $"Твоя текущая группа: {patsi.Group}.\nЕё можно сменить в меню.";
                                break;
                            }

                        default:
                            {
                                if (patsi.MoodleToken == "$!WAIT")
                                {
                                    try
                                    {
                                        var mmessage = chatTxt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        if (mmessage.Length != 2)
                                        {
                                            throw new InvalidOperationException("Нет либо логина, либо пароля.");
                                        }

                                        var mc = new PATShared.Moodle(PATShared.Schedule.client);
                                        var mresult = await mc.LoginNew(mmessage[0], mmessage[1]);

                                        patsi.MoodleToken = mresult;
                                        Students.SetUser(patuserid, patsi);

                                        msg = "✅ Вы авторизованы успешно, удалите сообщение с логином и паролем и нажмите на кнопку ещё раз. Если вы остановите диалог с ботом то ваши данные авторизации будут удалены.";
                                    }
                                    catch (Exception exc)
                                    {
                                        msg = $"❌ Произошла ошибка авторизации Moodle, свяжитесь с автором бота:\n{exc}";
                                    }
                                }
                                else if (patsi.RsaagLogin == "$!WAIT")
                                {
                                    try
                                    {
                                        var mmessage = chatTxt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        if (mmessage.Length != 2)
                                        {
                                            throw new InvalidOperationException("Нет либо логина, либо пароля.");
                                        }

                                        using var epos = new AntiEpos.AEClient();
                                        await epos.Login(mmessage[0], mmessage[1]);
                                        var agreementdat = await epos.CheckAgreement();
                                        if (!agreementdat.Status || agreementdat.Data is null || !agreementdat.Data.AgreedUser)
                                            throw new InvalidOperationException("Вы не согласились на обработку данных в РСААГ!");

                                        patsi.RsaagLogin = mmessage[0];
                                        patsi.RsaagPassword = mmessage[1];
                                        Students.SetUser(patuserid, patsi);

                                        msg = "✅ Успешно, удалите сообщение с логином и паролем и нажмите на кнопку ещё раз. Если вы остановите диалог с ботом то ваши данные авторизации будут удалены.";
                                    }
                                    catch (Exception exc)
                                    {
                                        msg = $"❌ Произошла ошибка авторизации в РСААГ, свяжитесь с автором бота:\n{exc}";
                                    }
                                }
                                else
                                msg = "Прости, я просто милый плюшевый тигр, я тебя не понимаю... хотя очень бы хотел...";

                                break;
                            }
                    }

                    if (sendout)
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: msg,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken,
                        replyMarkup: replyKeyboardMarkup
                    );
                }
            }
            else
            {
                var msg = "Привет! 👋\nЯ не знаю твою группу. Можешь отправить её одним сообщением? Если она содержит дробь, то отправлять нужно БЕЗ неё, как в расписании.\nПример: МХ-21-2\nили ЛП-21-1";
                Students.SetUser(patuserid, new PATShared.StudentInfo(""));
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: msg,
                    cancellationToken: cancellationToken,
                    replyMarkup: RemoveKeyboard
                );

                Console.WriteLine($"Initial dialog with: {upd.From.Username}");
            }
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                switch (update.Type)
                {
                    case UpdateType.MyChatMember:
                        {
                            await HandleUpdateMyChatMemberAsync(botClient, update.MyChatMember, cancellationToken);
                            break;
                        }

                    case UpdateType.CallbackQuery:
                        {
                            await HandleUpdateCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                            break;
                        }

                    case UpdateType.Message:
                        {
                            await HandleOnMessageAsync(botClient, update.Message, cancellationToken);
                            break;
                        }
                }
            }
            catch (Exception exc)
            {
                await HandleErrorAsync(botClient, exc, cancellationToken);
            }
        }

        public static async Task Main()
        {
            FixConsole();

            var mytgtoken = "";
            var myvktoken = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                mytgtoken = Environment.GetEnvironmentVariable("PATSCHEDULE_TG_TOKEN", EnvironmentVariableTarget.Machine);
                myvktoken = Environment.GetEnvironmentVariable("PATSCHEDULE_VK_TOKEN", EnvironmentVariableTarget.Machine);
            }
            else
            {
                mytgtoken = Environment.GetEnvironmentVariable("PATSCHEDULE_TG_TOKEN", EnvironmentVariableTarget.Process);
                myvktoken = Environment.GetEnvironmentVariable("PATSCHEDULE_VK_TOKEN", EnvironmentVariableTarget.Process);
            }

            if (string.IsNullOrWhiteSpace(mytgtoken))
            {
                Console.Error.WriteLine("TG Token is not set. Please set the PATSCHEDULE_TG_TOKEN environment variable.");
                return;
            }

            if (string.IsNullOrWhiteSpace(myvktoken))
            {
                Console.WriteLine("VK API Token is not set. This is a warning, bot will run in TG mode only.");
            }

            await Students.Load();

            Console.WriteLine("Downloading clock schedule...");
            await PATShared.Utils.DownloadClockSchedule(PATShared.Schedule.client, Cts.Token);

            Console.WriteLine("Running DB save task...");
            Students.RunSaveTask(Cts.Token);

            var botClient = new TelegramBotClient(mytgtoken, PATShared.Schedule.client);

            Console.WriteLine("Telegram instantiated, getting info...");
            var me = await botClient.GetMeAsync(Cts.Token);

            MyId = me.Id;

            Console.WriteLine("PATSchedule/TG info:");
            Console.WriteLine($"Username:         @{me.Username}");
            Console.WriteLine($"Id:               {me.Id}");
            Console.WriteLine($"Name:             'first={me.FirstName},last={me.LastName}'");
            Console.WriteLine($"Can join groups?  {me.CanJoinGroups}");
            Console.WriteLine($"Can read all msg? {me.CanReadAllGroupMessages}");
            Console.WriteLine($"Inline queries?   {me.SupportsInlineQueries}");
            Console.WriteLine($"Am I a bot?       {me.IsBot}");
            Console.WriteLine();

            Console.WriteLine("Starting listen...");

            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions(), Cts.Token);

            Console.WriteLine("Entering delay...");

            await Task.Delay(-1, Cts.Token);
        }
    }
}

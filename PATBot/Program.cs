using System;
using System.Collections;
using System.Collections.Generic;
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

        static ReplyKeyboardRemove RemoveKeyboard = new ReplyKeyboardRemove();
        static InlineKeyboardButton[][] InlineDateButtons = new InlineKeyboardButton[][] {
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Позавчера", "s-2.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Вчера", "s-1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Сегодня", "s0.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Завтра", "s1.0") },
            new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Послезавтра", "s2.0") }
        };
        static InlineKeyboardMarkup InlineDateMarkup = new InlineKeyboardMarkup(InlineDateButtons);

        const string NAME_BANNED = "❌ ой ой, вы быть забанен из данный бот во слава великий китай компартия 🇨🇳🇨🇳🇨🇳🇨🇳🇨🇳 лидер xi.";
        const string NAME_SCHEDULE = "📅 Расписание";
        const string NAME_HOMEWORK = "💼 Задания";
        const string NAME_ABOUTBOT = "🐯 О боте";
        const string NAME_CHANGEGR = "⚙️ Сменить группу";

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
            new KeyboardButton[]{ NAME_ABOUTBOT },
            new KeyboardButton[]{ NAME_CHANGEGR }
        };

        static Random Rnd = new Random();

        static ReplyKeyboardMarkup MenuMarkup = new ReplyKeyboardMarkup(MenuButtons, true, true);

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

                sb.AppendLine($"Вы зашли в [{userinfo.sitename}]({userinfo.siteurl})\nкак [{userinfo.firstname}]({userinfo.siteurl}/user/profile.php?id={userinfo.userid})\nИспользуйте кнопки ниже для навигации по курсам.\n\nДанные актуальны на {tag.startfrom:d}.\nДля выхода из Moodle нажмите на ❌.\nТЕСТ: версия мудл у сайта={userinfo.release}"); 

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
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
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
            var deluserid = "TG_" + upd.From.Id.ToString();

            if (stat == ChatMemberStatus.Kicked || stat == ChatMemberStatus.Left)
            {
                if (cancellationToken.IsCancellationRequested) return;
                Students.DelUser(deluserid);
                await Console.Out.WriteLineAsync($"User deregistered: {deluserid}, @{upd.NewChatMember.User.Username}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"User started dialog: {deluserid}, @{upd.NewChatMember.User.Username}");
            }
        }

        static async Task HandleUpdateCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery upd, CancellationToken cancellationToken)
        {
            Rnd.Next();
            var cberr = false;
            var msg = "Ошибка: ";
            var cbuserid = "TG_" + upd.From.Id.ToString();
            var chatId = upd.Message.Chat.Id;
            var msgId = upd.Message.MessageId;
            var mydt = DateTime.Today;

            if (cbuserid == "TG_1094694175")
            {
                await botClient.SendTextMessageAsync(chatId, NAME_BANNED, cancellationToken: cancellationToken);
                return;
            }

            var myuser = Students.GetUser(cbuserid);
            var imr = InlineDateMarkup;

            var mystr = upd.Data;

            if (myuser is null)
            {
                cberr = true;
                msg += "Пользователь не найден. ";
            }
            else if (!cberr && mystr.StartsWith('s'))
            {
                mystr = mystr.Substring(1);

                switch (mystr)
                {
                    case "-1.0": mydt = mydt.AddDays(-1.0); break;
                    case "-2.0": mydt = mydt.AddDays(-2.0); break;
                    case "1.0": mydt = mydt.AddDays(1.0); break;
                    case "2.0": mydt = mydt.AddDays(2.0); break;
                    case "0.0": break;
                    default:
                        {
                            cberr = true;
                            msg += "Неверно указан день. ";
                            break;
                        }
                }

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
                        msg = $"📅 {myuser.Group}, {mydt:dddd d MMMM yyyy}:\n";
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

                                if (mysch[si].Para == 0)
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
                                                var cabnum = int.Parse(mysch[si].Room.Split('-')[1]);
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
                                                if (hassport) cabfloor = '1';

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
                msg += "\nДанные актуальны на " + dtnow.ToLongDateString() + " " + dtnow.ToLongTimeString();
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
            var patuserid = "TG_" + chatId.ToString();

            if (patuserid == "TG_1094694175")
            {
                await botClient.SendTextMessageAsync(chatId, NAME_BANNED, cancellationToken: cancellationToken);
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
                                replyKeyboardMarkup = InlineDateMarkup;
                                break;
                            }

                        case "pig":
                        case "пиг":
                        case "hru":
                        case "hrue":
                        case "хрю":
                        case "hruxe":
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

                Console.WriteLine($"Initial dialog with: {upd.Chat.Username}");
            }
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
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

            Students.RunSaveTask(Cts.Token);

            var botClient = new TelegramBotClient(mytgtoken, PATShared.Schedule.client);
            var me = await botClient.GetMeAsync(Cts.Token);

            Console.WriteLine("PATSchedule/TG info:");
            Console.WriteLine($"Username:         @{me.Username}");
            Console.WriteLine($"Id:               {me.Id}");
            Console.WriteLine($"Name:             '{me.FirstName} {me.LastName}'");
            Console.WriteLine($"Can join groups?  {me.CanJoinGroups}");
            Console.WriteLine($"Can read all msg? {me.CanReadAllGroupMessages}");
            Console.WriteLine($"Inline queries?   {me.SupportsInlineQueries}");
            Console.WriteLine($"Am I a bot?       {me.IsBot}");
            Console.WriteLine();

            Console.WriteLine("Listening...");

            botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), Cts.Token);

            await Task.Delay(-1, Cts.Token);
        }
    }
}

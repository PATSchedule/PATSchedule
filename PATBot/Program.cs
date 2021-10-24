using System.Collections;
using System.Text;
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
        static PATShared.Students Students = new PATShared.Students();
        static CancellationTokenSource Cts = new CancellationTokenSource();

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

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var removekbd = new ReplyKeyboardRemove();
                var replyButtons = new KeyboardButton[] { "Расписание", "Сменить группу", "О боте" };
                var dateButtons = new InlineKeyboardButton[][] {
                    new InlineKeyboardButton[] { "Позавчера" },
                    new InlineKeyboardButton[] { "Вчера" },
                    new InlineKeyboardButton[] { "Сегодня" },
                    new InlineKeyboardButton[] { "Завтра" },
                    new InlineKeyboardButton[] { "Послезавтра" }
                };

                IReplyMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(replyButtons, true);
                var dateKeyboardMarkup = new InlineKeyboardMarkup(dateButtons);

                if (update.Type == UpdateType.MyChatMember)
                {
                    var stat = update.MyChatMember.NewChatMember.Status;
                    var deluserid = "TG_" + update.MyChatMember.From.Id.ToString();

                    if (stat == ChatMemberStatus.Kicked || stat == ChatMemberStatus.Left)
                    {

                        Students.DelUser(deluserid);
                        await Students.Save();

                        Console.WriteLine($"User deregistered: {deluserid}");
                    }
                    else
                    {
                        Console.WriteLine($"User started dialog: {deluserid}");
                    }
                }

                if (update.Type == UpdateType.CallbackQuery)
                {
                    var cberr = false;
                    var msg = "Ошибка: ";
                    var cbuserid = "TG_" + update.CallbackQuery.From.Id.ToString();
                    var mydt = DateTime.Today;

                    if (cbuserid == "TG_1094694175")
                    {
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "ой ой, вы быть забанен из данный бот во слава великий китай компартия.");
                        return;
                    }

                    var mystr = update.CallbackQuery.Data.ToLower(PATShared.Schedule.my_culture).Trim().Replace(".", "");
                    switch (mystr)
                    {
                        case "вчера": mydt = mydt.AddDays(-1.0); break;
                        case "позавчера": mydt = mydt.AddDays(-2.0); break;
                        case "завтра": mydt = mydt.AddDays(1.0); break;
                        case "послезавтра": mydt = mydt.AddDays(2.0); break;
                        case "сегодня": break;
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

                        var myuser = Students.GetUser(cbuserid);
                        if (myuser is null)
                        {
                            cberr = true;
                            msg += "Не могу найти пользователя. ";
                        }
                        else
                        {
                            msg = $"Расписание для группы {myuser.Group} на {mydt:d MMMM yyyy}:\n";
                            var mysch = cmysch.GetScheduleForGroup(myuser.Group);

                            if (mysch is null)
                            {
                                msg += "- Нет расписания, попробуй поменять день или группу.";
                            }
                            else
                                foreach (var par in mysch)
                                {
                                    msg += par.ToString() + "\n";
                                }
                        }
                    }

                    msg += "\nДанные актуальны на " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

                    await botClient.EditMessageTextAsync(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        text: msg,
                        replyMarkup: dateKeyboardMarkup,
                        cancellationToken: cancellationToken
                    );

                    await botClient.AnswerCallbackQueryAsync(
                        update.CallbackQuery.Id,
                        cancellationToken: cancellationToken
                    );
                }

                if (update.Type != UpdateType.Message)
                    return;

                if (update.Message.Type != MessageType.Text)
                    return;

                var chatId = update.Message.Chat.Id;
                var chatTxt = update.Message.Text;
                var patuserid = "TG_" + chatId.ToString();

                if (patuserid == "TG_1094694175")
                {
                    await botClient.SendTextMessageAsync(chatId, "ой ой, вы быть забанен из данный бот во слава великий китай компартия.");
                    return;
                }

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
                            && uint.TryParse(parts[1], out uint _p1)
                            && uint.TryParse(parts[2], out uint _p2)
                            && _p1 > 0
                            && _p2 > 0;

                        if (validgroup)
                        {
                            Students.SetUser(patuserid, new PATShared.StudentInfo(mygroup));
                            await Students.Save();
                        }
                        else
                        {
                            msg = "Группа введена некорректно. Попробуй ещё раз.";
                            replyKeyboardMarkup = removekbd;
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
                        string? msg;
                        switch (chatTxt)
                        {
                            case "О боте":
                                {
                                    msg = "Это глупый бот который набросал @nikthecat из группы МХ-21-2.\nО всех проблемах писать тоже ему. (или бить, легонько)";
                                    break;
                                }

                            case "Сменить группу":
                                {
                                    msg = $"Твоя текущая группа: {patsi.Group}\nПришли новую группу одним сообщением, как раньше.";
                                    Students.SetUser(patuserid, new PATShared.StudentInfo(""));
                                    replyKeyboardMarkup = removekbd;
                                    break;
                                }

                            case "Расписание":
                                {
                                    msg = "На какой день показать?\nПока можно только кнопками снизу:";

                                    replyKeyboardMarkup = dateKeyboardMarkup;

                                    break;
                                }

                            default:
                                {
                                    msg = "Прости, я просто милый плюшевый тигр, я тебя не понимаю... хотя очень бы хотел...";
                                    break;
                                }
                        }

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: msg,
                            cancellationToken: cancellationToken,
                            replyMarkup: replyKeyboardMarkup
                        );
                    }
                }
                else
                {
                    var msg = "Привет! Я не знаю твою группу. Можешь отправить её одним сообщением? Если она содержит дробь, то отправлять нужно БЕЗ неё. (как в расписании)\nПример: МХ-21-2\nили ЛП-21-1";
                    Students.SetUser(patuserid, new PATShared.StudentInfo(""));
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: msg,
                        cancellationToken: cancellationToken,
                        replyMarkup: removekbd
                    );

                    Console.WriteLine($"Initial dialog with: {update.Message.Chat.Username}");
                }
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc);
            }
        }

        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var mytgtoken = Environment.GetEnvironmentVariable("PATSCHEDULE_TG_TOKEN");
            if (string.IsNullOrWhiteSpace(mytgtoken))
            {
                Console.Error.WriteLine("TG Token is not set. Please set the PATSCHEDULE_TG_TOKEN environment variable.");
                return;
            }

            await Students.Load();

            var botClient = new TelegramBotClient(mytgtoken);
            var me = await botClient.GetMeAsync(Cts.Token);

            Console.WriteLine("PATSchedule/TG info:");
            Console.WriteLine($"Username: @{me.Username}");
            Console.WriteLine($"Id:       {me.Id}");
            Console.WriteLine($"Name:     '{me.FirstName}'");
            Console.WriteLine();

            Console.WriteLine("Listening...");

            botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), Cts.Token);

            await Task.Delay(-1);
        }
    }
}

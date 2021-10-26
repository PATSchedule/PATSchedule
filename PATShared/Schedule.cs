using AngleSharp;
using AngleSharp.Html.Dom;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;

namespace PATShared
{
    public class Schedule
    {
        public static readonly HttpClient client = new HttpClient();
        static readonly string repl_uri = "https://permaviat.ru/raspisanie-zamen/";
        public static readonly CultureInfo my_culture = new CultureInfo("ru-RU"); // руссиш спарше


        IDictionary<string, IList<SingleReplacement>> MySchedule;

        public Schedule()
        {
            MySchedule = new Dictionary<string, IList<SingleReplacement>>();
        }

        public IList<SingleReplacement>? GetScheduleForGroup(string groupName)
        {
            var cpy = groupName.ToUpper(my_culture).Trim().Replace(' ', '-');
            if (MySchedule.ContainsKey(cpy)) return MySchedule[cpy];
            else return null;
        }

        static DateTime ParseName(string n)
        {
            // >:(
            // то они "ЗАмены" напишут
            // то точки в конце поставят
            // >:((((
            // короче нам нужна конкретная дата, "21 квинтября 1337"
            n = n
                .ToLower(my_culture)
                .Substring(n.IndexOf("на")+2)
                .Replace(".", "")
                .Replace(",", "")
                .Trim();

            return DateTime.Parse(n, my_culture);
        }

        public class SingleReplacement : ICloneable
        {
            public int Para; // номер пары, 1,2,3,4.... 0 если это полная замена (нужно стереть все существующие пары!)
            public string Room; // 'Т-228'
            public string Subject; // 'Информатика (Паренкова А.С.)'
            public Building MyBuilding;

            public SingleReplacement(int _p, string _room, string _subject)
            {
                Para = _p;
                Room = _room;
                Subject = _subject;
            }

            public bool CanIgnore()
            {
                return Subject.ToLower(my_culture).Trim() == "нет" && Room == "";
            }

            public object Clone()
            {
                var _new = new SingleReplacement(Para, Room, Subject);
                return _new;
            }

            public override string ToString()
            {
                var pref = Para > 0 ? (" " + Para.ToString()) : "";
                var suff = Subject;
                if (Room != "") suff += " " + Room;

                return $"Пара{pref}: {suff}";
            }
        }

        class SingleLine : ICloneable
        {
            public string Group;
            public string Para;
            public string Lesson;
            public string Room;

            public bool IsEmpty()
            {
                // если не заданы ни группа, ни номера пар, то это пустышка (нафиг не надо)
                // можно пропустить.
                return Group.Length < 1 && Para.Length < 1;
            }

            public void Sanitize()
            {
                // спасибо тебе учебная часть за долбанутое форматирование документа 20 века
                // мммМММММММММММ лишние пробелы
                // оаоаоа разное написание аудиторий (то "С 308" то "С-308")
                // про названия пар я вообще молчу (почему иногда фамилии преподавателей с МАЛЕНЬКОЙ БУКВЫ пишите????)
                Group = Group.Trim().Replace("\r", "").Replace("\n", "");
                Para = Para.Trim().Replace("\r", "").Replace("\n", "").Replace(' ', ',').Replace('.', ',');
                Lesson = Lesson.Trim().Replace("\r", "").Replace("\n", "");
                Room = Room.Trim().Replace("\r", "").Replace("\n", "").Replace(' ', '-').ToUpper(my_culture);

                if (Para.EndsWith(',')) Para = Para.TrimEnd(',');

                // если номер пары не указан, значит эта замена - единственная пара у группы.
                // укажем тут 0 а дальше по ситуации.
                if (Para.Length < 1 && Group.Length > 1)
                    Para = 0.ToString();
            }

            public bool IsMultiple()
            {
                return Para.Contains(',');
            }

            public IList<SingleLine> ParseMultiple()
            {
                IList<SingleLine> list = new List<SingleLine>();

                string[] nums = Para.Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < nums.Length; ++i)
                {
                    list.Add(new SingleLine(Group, nums[i], Lesson, Room));
                }

                return list;
            }

            public SingleLine(string _g, string _p, string _l, string _r)
            {
                Group = _g;
                Para = _p;
                Lesson = _l;
                Room = _r;
            }

            public override string ToString()
            {
                return $"[{Group};{Para};{Lesson};{Room}]";
            }

            public object Clone()
            {
                var _new = new SingleLine(Group, Para, Lesson, Room);
                return _new;
            }
        }

        IList<SingleReplacement> CloneList(IList<SingleReplacement> toclone)
        {
            IList<SingleReplacement> n = new List<SingleReplacement>();

            foreach (var ii in toclone)
            {
                n.Add((SingleReplacement)ii.Clone());
            }

            return n;
        }

        IDictionary<string, IList<SingleReplacement>> Merge(IDictionary<string, IList<SingleReplacement>> Base, IDictionary<string, IList<SingleReplacement>> Add)
        {
            IDictionary<string, IList<SingleReplacement>> r = new Dictionary<string, IList<SingleReplacement>>();

            // копируем Base в r.
            foreach (var kvp in Base)
            {
                r[kvp.Key] = CloneList(kvp.Value);
            }

            // мержим Add в r.
            foreach (var kvp in Add)
            {
                // если в списке есть пара 0, то это глобальная замена.
                if (!r.ContainsKey(kvp.Key) || kvp.Value.Any(a => a.Para == 0))
                {
                    // просто переносим как есть
                    r[kvp.Key] = CloneList(kvp.Value);
                }
                else
                {
                    // по-умному
                    var copylist = CloneList(kvp.Value);
                    for (int i = 0; i < r[kvp.Key].Count; ++i)
                    {
                        // если уже есть пара с таким-то названием, то замена.
                        var thing = copylist.FirstOrDefault(a => a.Para == r[kvp.Key][i].Para);
                        if (thing is SingleReplacement)
                        {
                            r[kvp.Key][i] = (SingleReplacement)thing.Clone();
                            copylist.Remove(thing);
                        }
                    }

                    // переносим остатки.
                    foreach (var ii in copylist)
                    {
                        r[kvp.Key].Add(ii);
                    }
                }
            }

            return r;
        }

        async Task<IDictionary<string, IList<SingleReplacement>>> FetchOne(string url)
        {
            using var _docx = await client.GetStreamAsync(url);
            var d = new Dictionary<string, IList<SingleReplacement>>();

            using var document = WordprocessingDocument.Open(_docx, false);

            //document.
            if (document.MainDocumentPart is null)
                throw new InvalidOperationException("MainDocumentPart must not be null under any circumstances.");

            var reptbl = document.MainDocumentPart.Document.Descendants<Table>().First();

            // rows: Группа, Пара, Предмет, Аудитория
            // 

            int x = 0, y = 0;

            IList<SingleLine> lines = new List<SingleLine>();

            // первый шаг, парсим таблицу во что-то более вменяемое...
            foreach (var row in reptbl.Descendants<TableRow>())
            {
                SingleLine tmp = new SingleLine("", "", "", "");
                var cells = row.Descendants<TableCell>();
                foreach (var cell in cells)
                {
                    string mytext = "";

                    foreach (var _p in cell.Descendants<Paragraph>())
                    {
                        foreach (var _t in _p.Descendants<Text>())
                        {
                            mytext += _t.InnerText;
                        }
                    }

                    switch (x)
                    {
                        case 0: tmp.Group = mytext; break;
                        case 1: tmp.Para = mytext; break;
                        case 2: tmp.Lesson = mytext; break;
                        case 3: tmp.Room = mytext; break;
                    }

                    ++x;
                }

                // приводим написание строчки к какому-то общему формату.
                tmp.Sanitize();

                if (x > 0 && y > 0 && !tmp.IsEmpty())
                {
                    lines.Add(tmp);
                }

                ++y;
                x = 0;
            }

            // заполняем пустые группы
            string lastgroup = "";
            for (int i = 0; i < lines.Count; ++i)
            {
                if (lines[i].Group != "") lastgroup = lines[i].Group;
                else
                {
                    for (int j = i; j < lines.Count; ++j)
                    {
                        if (lines[j].Group != "") break;
                        else lines[j].Group = lastgroup;
                    }
                }

                if (lines[i].IsMultiple())
                {
                    var toadd = lines[i].ParseMultiple();
                    for (int j = toadd.Count - 1; j >= 0; --j)
                    {
                        lines.Insert(i, toadd[j]);
                    }

                    lines.RemoveAt(i + toadd.Count);
                }
            }

            // заполняем словарик
            for (int i = 0; i < lines.Count; ++i)
            {
                if (!d.ContainsKey(lines[i].Group))
                    d[lines[i].Group] = new List<SingleReplacement>();

                if (!int.TryParse(lines[i].Para, NumberStyles.Integer, my_culture, out int _parnum))
                {
                    throw new InvalidOperationException("wtf?");
                }

                d[lines[i].Group].Add(new SingleReplacement(_parnum, lines[i].Room, lines[i].Lesson));
            }

            return d;
        }

        async Task<IDictionary<string, IList<SingleReplacement>>> FetchExcel(string url)
        {
            using var _xlsx = await client.GetStreamAsync(url);
            var d = new Dictionary<string, IList<SingleReplacement>>();

            using var document = SpreadsheetDocument.Open(_xlsx, false);

            if (document.WorkbookPart is null)
                throw new InvalidOperationException("WorkbookPart cannot be null!");

            var thesheet = document.WorkbookPart.Workbook.Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>().First();
            string? sheetid = thesheet.Id;
            if (sheetid is null)
                throw new InvalidOperationException("Sheet.Id cannot be null!");

            var wbpart = (WorksheetPart)document.WorkbookPart.GetPartById(sheetid);
            var wsheet = wbpart.Worksheet;
            if (wsheet is null)
                throw new InvalidOperationException("Worksheet cannot be null!");

            int x = 0, y = 0;

            var rows = wsheet.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.SheetData>()?.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>();
            foreach (var row in rows)
            {
                var cells = row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                foreach (var cell in cells)
                {
                    string? txt = cell.CellValue?.Text;
                    if (txt is null)
                    {
                        int a = 0;
                    }

                    ++x;
                }

                x = 0;
                ++y;
            }


            return d;
        }

        async Task FetchExcels(DateTime origindate)
        {
            var json = await client.GetStringAsync("https://raw.githubusercontent.com/PATSchedule/BaseSchedule/main/Schedule.json");

            var jsonsch = JsonBaseSchedule.Parse(json);

            MySchedule.Clear();

            foreach (var kvp in jsonsch.Data)
            {
                string mygroup = kvp.Key;
                var days = kvp.Value;

                var lst = new List<SingleReplacement>();

                var i = ((int)origindate.DayOfWeek) - 1;
                if (days.Length <= i) continue;

                foreach (var para in days[i])
                {
                    var parnum = para.Para;

                    // пара только для второй недели
                    if (parnum > 20)
                    {
                        // если сейчас не вторая неделя то пофиг
                        if (!Utils.IsSecondWeek(origindate)) continue;
                        parnum -= 20;
                    }
                    else if (parnum > 10) // только для первой
                    {
                        // если сейчас вторая неделя то пофиг
                        if (Utils.IsSecondWeek(origindate)) continue;
                        parnum -= 10;
                    }

                    var appendsub = para.Subject;
                    if (para.Teacher.Length > 0)
                        appendsub += " (" + para.Teacher + ")";

                    lst.Add(new SingleReplacement(parnum, para.Room, appendsub));
                }

                MySchedule[mygroup] = CloneList(lst);
            }
        }

        public async Task FetchSchedule(DateTime origindate)
        {
            await FetchExcels(origindate); // fetch base schedule

            var bcfg = Configuration.Default
                .WithDefaultLoader()
                .WithDefaultCookies()
                .WithMetaRefresh()
                .WithLocaleBasedEncoding()
                .WithCulture(my_culture);

            using var context = BrowsingContext.New(bcfg);
            using var document = await context.OpenAsync(repl_uri);

            var files = document.GetElementsByClassName("file_tree")[0].GetElementsByClassName("file_link");

            foreach (var e in files)
            {
                foreach (var ee in e.Children)
                {
                    foreach (var eee in ee.Children)
                    {
                        if (eee is IHtmlAnchorElement ihae)
                        {
                            // href относителен, и AngleSharp думает что оно на локалхост указывает...
                            var actualurl = ihae.Href.Replace("http://localhost", "https://permaviat.ru");
                            var date = ParseName(ihae.Text);

                            // пару раз там учебная часть выложила замены в .PDF
                            if (!actualurl.EndsWith(".docx")) continue;
                            if (DateTime.Compare(date, origindate) != 0) continue;

                            // скачиваем замену:
                            var replacement = await FetchOne(actualurl);
                            MySchedule = Merge(MySchedule, replacement);

                            return;
                        }
                    }
                }
            }

        }
    }
}

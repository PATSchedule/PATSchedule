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
using System.IO;

namespace PATShared
{
    public class Schedule
    {
        public static readonly HttpClient client = new HttpClient();
        static readonly string repl_uri = "https://permaviat.ru/raspisanie-zamen/";
        public static readonly CultureInfo my_culture = new CultureInfo("ru-RU"); // руссиш спарше

        class CacheEntry
        {
            public string FileUrl;
            public IDictionary<string, IList<SingleReplacement>> Data;

            public CacheEntry(string fileurl, IDictionary<string, IList<SingleReplacement>> data)
            {
                FileUrl = fileurl;
                Data = data;
            }
        }

        static IDictionary<DateTime, CacheEntry> Cache = new Dictionary<DateTime, CacheEntry>();
        IDictionary<string, IList<SingleReplacement>> MySchedule = new Dictionary<string, IList<SingleReplacement>>();

        public bool ReplacementsUsed = false;
        public string ReplacementUrl = "";
        public string ReplacementFile = "";

        public IList<SingleReplacement>? GetScheduleForGroup(string groupName)
        {
            var cpy = groupName.ToUpper(my_culture).Trim().Replace(' ', '-');
            if (MySchedule.ContainsKey(cpy)) return MySchedule[cpy];
            else return null;
        }

        static string FixupName(string n)
        {
            return n
                .Replace("декб", "декаб");
                // добавлять сюда опечатки учебной части.
                // а если учебная часть это вдруг читает, вам не стыдно? -_-
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
                //.Replace(".", "")
                .Replace(",", "")
                .Trim();

            n = FixupName(n);

            return DateTime.Parse(n, my_culture);
        }

        public class SingleReplacement : ICloneable
        {
            public int Para; // номер пары, 1,2,3,4.... 0 если это полная замена (нужно стереть все существующие пары!)
            public string Room; // 'Т-228'
            public string Subject; // 'Информатика (Паренкова А.С.)'

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
                var pref = Para > 0 ? (Para.ToString() + ":") : "";
                var suff = Subject;
                if (Room != "") suff += " " + Room;

                return $"{pref} {suff}";
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
                Group = Group.Trim().Replace("\r", "").Replace("\n", "").Replace("_", "-");
                while (Group.Contains("--")) Group = Group.Replace("--", "-");
                Group = Group.Trim(); // финальный проход...
                Para = new string(Para.Trim().Replace("\r", "").Replace("\n", "").Replace(' ', ',').Replace('.', ',')
                    .Where(x => char.IsDigit(x) || x == ',').ToArray());
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

                var nums = Para.Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < nums.Length; ++i)
                {
                    list.Add(new SingleLine(Group, nums[i], Lesson, Room));
                }

                var gr = Group;
                var grpos = gr.IndexOf(',');
                if (grpos > 0)
                {
                    nums = gr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    gr = gr.Substring(0, grpos - 1); // "МХ-21-"
                    for (var i = 0; i < nums.Length; ++i)
                    {
                        // TODO: реализовать такой странный формат групп.
                    }
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

        // нет в шарпе анонимных функций :(
        delegate NPOI.SS.UserModel.ICell DGetActiveCell(NPOI.SS.UserModel.ISheet sht, NPOI.SS.Util.CellAddress addr);
        delegate string DGetCellValue(NPOI.SS.UserModel.ICell cell);
        delegate NPOI.SS.Util.CellAddress DMoveAddress(NPOI.SS.Util.CellAddress addr, int offrow, int offcol);

        class StringChitatel
        {
            string str;
            int pos;

            public StringChitatel(string _s)
            {
                str = _s;
                pos = 0;
            }

            public int Pos()
            {
                return pos;
            }

            public void ToEnd()
            {
                pos = str.Length - 1;
            }

            public void ToStart()
            {
                pos = 0;
            }

            public void SkipWhitespaceForwards()
            {
                while (char.IsWhiteSpace(str[pos])) ++pos;
            }

            public void SkipWhitespaceBackwards()
            {
                while (char.IsWhiteSpace(str[pos])) --pos;
            }

            public string StringAtBackwards(int l)
            {
                var s = str.Substring(pos - l + 1, l);
                pos -= l;
                return s;
            }

            public string StringAtForwards(int l)
            {
                var s = str.Substring(pos, l);
                pos += l;
                return s;
            }

            public char CharAt(int p = -1)
            {
                return (p < 0) ? str[pos] : str[p];
            }
        }

        SingleReplacement ParseExcelCells(int num, string subjectcell, string teachercell)
        {
            var reader = new StringChitatel(subjectcell);
            reader.ToEnd();
            reader.SkipWhitespaceBackwards();

            var suffix = string.IsNullOrWhiteSpace(teachercell) ? "" : $" ({teachercell})";

            var hasbuilding = uint.TryParse(reader.StringAtBackwards(3), NumberStyles.AllowTrailingWhite, my_culture, out uint roomdigits);
            if (!hasbuilding)
            {
                return new SingleReplacement(num, "", subjectcell + suffix);
            }

            reader.SkipWhitespaceBackwards();
            var buildingletter = reader.StringAtBackwards(1)[0];
            var fullroomname = buildingletter.ToString() + "-" + roomdigits.ToString();
            return new SingleReplacement(num, fullroomname, subjectcell.Substring(0, reader.Pos() + 1).Trim() + suffix);
        }

        async Task<IDictionary<string, IList<SingleReplacement>>> FetchWeirdXls(string url)
        {
            // либо берём из кэша cache[ссылка] = byte[], либо скачиваем
            var _xls = await client.GetByteArrayAsync(url);
            using var _ms = new MemoryStream(_xls);
            var d = new Dictionary<string, IList<SingleReplacement>>();

            // -- кошмар начинается -- //
            var workbook = NPOI.SS.UserModel.WorkbookFactory.Create(_ms);
            if (workbook is null) throw new NullReferenceException();
            var sheet = workbook.GetSheetAt(0);
            if (sheet is null) throw new NullReferenceException();

            DGetActiveCell getcell = (NPOI.SS.UserModel.ISheet sht, NPOI.SS.Util.CellAddress addr) => {
                return sht.GetRow(addr.Row).GetCell(addr.Column);
            };
            DGetCellValue getcellv = (NPOI.SS.UserModel.ICell cell) => {
                return (cell is null) ? string.Empty : (cell.ToString() ?? string.Empty);
            };
            DMoveAddress movecell = (NPOI.SS.Util.CellAddress addr, int offrow, int offcol) => {
                return new NPOI.SS.Util.CellAddress(addr.Row + offrow, addr.Column + offcol);
            };

            var _START = NPOI.SS.Util.CellAddress.A1;
            var _NUMBERSC = int.MinValue;
            while (true)
            {
                var thevalue = getcellv(getcell(sheet, _START));
                if (!thevalue.Trim().Contains("День недели"))
                {
                    _START = movecell(_START, 1, 0);
                    continue;
                }

                while (true)
                {
                    thevalue = getcellv(getcell(sheet, _START));
                    _NUMBERSC = _START.Column;
                    _START = movecell(_START, 0, 1);
                    if (thevalue.Trim().Contains("№ пары"))
                    {
                        break;
                    }
                }

                break;
            }

            var _SHEETPOS = new NPOI.SS.Util.CellAddress(_START.Row, _START.Column);
            var _MAXLESSONS = int.MinValue;
            
            while (true)
            {
                var _groupname = getcellv(getcell(sheet, _SHEETPOS));
                if (string.IsNullOrWhiteSpace(_groupname))
                {
                    _SHEETPOS = movecell(_SHEETPOS, (_MAXLESSONS * 2) + 1, _START.Column - _SHEETPOS.Column);
                    if (string.IsNullOrWhiteSpace(getcellv(getcell(sheet, _SHEETPOS))))
                    {
                        break;
                    }

                    _MAXLESSONS = int.MinValue;
                    continue;
                }

                _groupname = _groupname.Trim().Replace("\r\n", "").Replace("\n", "").Replace(" ", "-").ToUpper(my_culture);
                var lst = new List<SingleReplacement>();
                var _LESSONADDR = movecell(_SHEETPOS, 1, 0);
                //await Console.Out.WriteLineAsync($"Processing group {_groupname}");
                while (true)
                {
                    var _lessonstring = getcellv(getcell(sheet, new NPOI.SS.Util.CellAddress(_LESSONADDR.Row, _NUMBERSC)));
                    _lessonstring = _lessonstring.Trim().Replace("\r\n", "").Replace("\n", "");
                    var _parseok = int.TryParse(_lessonstring, out int i);
                    if (!_parseok)
                    {
                        break;
                    }

                    _MAXLESSONS = Math.Max(_MAXLESSONS, i);
                    var _subjectname = getcellv(getcell(sheet, _LESSONADDR));
                    _LESSONADDR = movecell(_LESSONADDR, 1, 0);
                    var _teachername = getcellv(getcell(sheet, _LESSONADDR));
                    _LESSONADDR = movecell(_LESSONADDR, 1, 0);

                    _subjectname = _subjectname.Trim().Replace("\r\n", "").Replace("\n", "");
                    _teachername = _teachername.Trim().Replace("\r\n", "").Replace("\n", "");
                    if (string.IsNullOrWhiteSpace(_subjectname))
                    {
                        continue;
                    }

                    var sr = ParseExcelCells(i, _subjectname, _teachername);
                    lst.Add(sr);
                    //await Console.Out.WriteLineAsync($"Processed {sr}.");
                }

                // -- пытаемся исправить пары у которых не проставлен корпус.... -- //
                var knownroom = "";
                for (var i = 0; i < lst.Count; ++i)
                {
                    if (!string.IsNullOrWhiteSpace(lst[i].Room))
                    {
                        // 'А-1??'
                        knownroom = lst[i].Room[0] + "-1??";
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(knownroom))
                {
                    var istelta = knownroom[0] == 'Т';

                    for (var i = 0; i < lst.Count; ++i)
                    {
                        if (string.IsNullOrWhiteSpace(lst[i].Room))
                        {
                            var supposedname = lst[i].Subject.ToLower(my_culture);
                            if (supposedname.Contains("физкул") || supposedname.Contains("физич"))
                            {
                                lst[i].Room = istelta ? "Т-Спортзал" : "Спортзал";
                            }
                            else
                            {
                                lst[i].Room = knownroom;
                            }
                        }
                    }
                }

                if (d.ContainsKey(_groupname))
                {
                    throw new InvalidDataException("Already processed group " + _groupname);
                }

                d[_groupname] = lst;
                _SHEETPOS = movecell(_SHEETPOS, 0, 1);
            }

            return d;
        }

        async Task<IDictionary<string, IList<SingleReplacement>>> FetchOne(string url)
        {
            // учебная часть всегда выкладывает новый файл вместо того чтобы обновлять текущий
            // и слава богу
            var _docx = await client.GetByteArrayAsync(url);
            using var _ms = new MemoryStream(_docx);
            var d = new Dictionary<string, IList<SingleReplacement>>();

            using var document = WordprocessingDocument.Open(_ms, false);

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
                    throw new InvalidOperationException("Номер пары не верен, вините учебную часть...");
                }

                d[lines[i].Group].Add(new SingleReplacement(_parnum, lines[i].Room, lines[i].Lesson));
            }

            return d;
        }

        /*
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
                        
                    }

                    ++x;
                }

                x = 0;
                ++y;
            }


            return d;
        }
        */

        async Task FetchExcels(DateTime origindate)
        {
            var json = await client.GetStringAsync("https://raw.githubusercontent.com/PATSchedule/BaseSchedule/main/Schedule.json");

            var jsonsch = JsonBaseSchedule.Parse(json);
            if (jsonsch is null) return;

            MySchedule.Clear();

            foreach (var kvp in jsonsch.Data)
            {
                var mygroup = kvp.Key;
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
            try
            {
                lock (Cache)
                {
                    var hascached = false;
                    var tocleanup = new List<DateTime>();
                    // смотрим сначала есть ли наш искомый элемент в кэше, ЛИБО (!!!)
                    // если есть какие-то очень очень старые записи то помечаем их на удаление...
                    foreach (var item in Cache)
                    {
                        // сначала if date == key, чтобы случайно не удалить нужную кэшированую запись
                        if (origindate == item.Key)
                        {
                            hascached = true;
                        }
                        else if ((DateTime.Now - item.Key).Days > 10)
                        {
                            // потом уже проверяем нужно ли удалить
                            tocleanup.Add(item.Key);
                        }
                    }

                    foreach (var item in tocleanup)
                    {
                        Console.WriteLine($"Removing very old cache entry {item:dd MM yyyy}");
                        Cache.Remove(item);
                    }

                    if (hascached)
                    {
                        var cacheentry = Cache[origindate];
                        MySchedule = cacheentry.Data;

                        // проставить ссылку на файл из которого был сделан кэш, если ссылка есть конечно
                        if (!string.IsNullOrWhiteSpace(cacheentry.FileUrl))
                        {
                            ReplacementsUsed = true;
                            ReplacementUrl = cacheentry.FileUrl;
                            ReplacementFile = new Uri(ReplacementUrl).Segments.Last();
                        }

                        return;
                    }
                }

                var bcfg = Configuration.Default
                    .WithDefaultLoader()
                    .WithDefaultCookies()
                    .WithMetaRefresh()
                    .WithLocaleBasedEncoding()
                    .WithCulture(my_culture);

                using var context = BrowsingContext.New(bcfg);
                using var document = await context.OpenAsync(repl_uri);

                var files = document.GetElementsByClassName("file_tree")[0].GetElementsByClassName("file_link");

                ReplacementsUsed = false;

                if (files.Length < 0) return;
                foreach (var e in files)
                {
                    if (e.Children.Length < 1) continue;
                    foreach (var ee in e.Children.Reverse())
                    {
                        if (ee.Children.Length < 1) continue;
                        foreach (var eee in ee.Children.Reverse())
                        {
                            if (eee is IHtmlAnchorElement ihae)
                            {
                                // href относителен, и AngleSharp думает что оно на локалхост указывает...
                                var actualurl = ihae.Href.Replace("http://localhost", "https://permaviat.ru");
                                var date = ParseName(ihae.Text);

                                // пару раз там учебная часть выложила замены в .PDF
                                if (DateTime.Compare(date, origindate) != 0) continue;

                                // ставим флажок что замены найдены
                                ReplacementsUsed = true;
                                ReplacementUrl = actualurl;
                                ReplacementFile = new Uri(ReplacementUrl).Segments.Last();

                                // скачиваем замену:
                                if (ReplacementFile.EndsWith(".docx"))
                                {
                                    await FetchExcels(origindate); // fetch base schedule
                                    var docxreplacement = await FetchOne(actualurl);
                                    MySchedule = Merge(MySchedule, docxreplacement);

                                    lock (Cache)
                                    {
                                        Cache[origindate] = new CacheEntry(ReplacementUrl, MySchedule);
                                    }
                                }
                                else if (ReplacementFile.EndsWith(".xls") || ReplacementFile.EndsWith(".xlsx"))
                                {
                                    var xlsxreplacement = await FetchWeirdXls(actualurl);
                                    // здесь без Merge()...
                                    MySchedule = xlsxreplacement;

                                    lock (Cache)
                                    {
                                        Cache[origindate] = new CacheEntry(ReplacementUrl, MySchedule);
                                    }
                                }
                                else
                                {
                                    throw new InvalidDataException("Replacement file has an invalid data type, " + ReplacementFile);
                                }

                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Unable to fetch replacements: {exc}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PATShared
{
    public class StudentInfo
    {
        public string Group { get; set; }
        // TODO: sth else?
        public string MoodleToken { get; set; }

        // используется для навигации по moodle.
        public object? Tag { get; set; }

        public StudentInfo(string group = "", string moodletoken = "")
        {
            Group = group;
            MoodleToken = moodletoken;
        }

        public StudentInfo(StudentInfo _other)
        {
            Group = _other.Group;
            MoodleToken = _other.MoodleToken;
            Tag = _other.Tag;
        }
    }

    public class Students
    {
        static readonly int SAVETIME = 15 * 1000;
        static readonly string FILEPATH = "db.txt";
        IDictionary<string, StudentInfo> Users;
        object LockObject;
        bool IsDirty;


        public Students()
        {
            IsDirty = false;
            Users = new Dictionary<string, StudentInfo>();
            LockObject = new object();
        }

        private async Task SaveTask(object octs)
        {
            CancellationToken cts = (CancellationToken)octs;

            Console.WriteLine("Entering db save thread...");

            while (true)
            {
                if (cts.IsCancellationRequested) return;
                await Task.Delay(SAVETIME);
                string towrite = "# An error had occurred when saving the database!\n";

                lock (LockObject)
                {
                    if (!IsDirty) continue;

                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("{0}{1}{2}{3}", "# PATSchedule user database, DO NOT EDIT, string format:", "\n", "# {id}_{user}={group};{moodletoken};{...}", "\n");
                    foreach (var kvp in Users)
                    {
                        sb.AppendFormat("{0}={1};{2};{3}", kvp.Key, kvp.Value.Group, kvp.Value.MoodleToken, /*, ... */ "\n");
                    }

                    towrite = sb.ToString();

                    Console.WriteLine($"Saving database at {DateTime.Now:G}...");
                    IsDirty = false;
                }

                await File.WriteAllTextAsync(FILEPATH, towrite, Encoding.UTF8);
            }
        }

        public StudentInfo? GetUser(string userid)
        {
            try
            {
                lock (LockObject)
                {
                    return Users[userid];
                }
            }
            catch
            {
                return null;
            }
        }

        public void SetUser(string userid, StudentInfo info)
        {
            lock (LockObject)
            {
                Users[userid] = info;
                IsDirty = true;
            }
        }

        public bool DelUser(string userid)
        {
            lock (LockObject)
            {
                IsDirty = true;
                return Users.Remove(userid);
            }
        }

        public void RunSaveTask(CancellationToken cts)
        {
            Task.Factory.StartNew(SaveTask, cts);
        }

        public async Task Load()
        {
            var lines = new string[0];

            try
            {
                if (!File.Exists(FILEPATH))
                {
                    Console.WriteLine("Database file does not exist. Will start empty.");
                    return;
                }

                lines = await File.ReadAllLinesAsync(FILEPATH, Encoding.UTF8);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Unable to read the database file:");
                Console.WriteLine(exc.ToString());
                Console.WriteLine("-- will start without a database. o_O");
            }

            lock (LockObject)
            {
                Users.Clear();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.StartsWith('#') || !line.Contains('=')) continue;

                    var kvp = line.Trim().Split('=');
                    var key = kvp[0];
                    var values = kvp[1].Split(';');

                    Users[key] = new StudentInfo(
                        (values.Length > 0) ? values[0] : "",
                        (values.Length > 1) ? values[1] : ""
                        /*, ...*/
                    );
                }

                IsDirty = false;
            }
        }
    }
}

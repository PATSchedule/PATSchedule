using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    }

    public class Students
    {
        static readonly string FILEPATH = "db.txt";
        IDictionary<string, StudentInfo> Users;
        object LockObject;

        public Students()
        {
            Users = new Dictionary<string, StudentInfo>();
            LockObject = new object();
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
            }
        }

        public void DelUser(string userid)
        {
            lock (LockObject)
            {
                Users.Remove(userid);
            }
        }

        public async Task Load()
        {
            if (!File.Exists(FILEPATH)) return;

            var lines = await File.ReadAllLinesAsync(FILEPATH, Encoding.UTF8);

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
            }
        }

        public async Task Save()
        {
            string towrite = "";

            lock (LockObject)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}{1}{2}{3}", "# PATSchedule user database, DO NOT EDIT, string format:", "\n", "# {id}_{user}={group};{moodletoken};{...}", "\n");
                foreach (var kvp in Users)
                {
                    sb.AppendFormat("{0}={1};{2};{3}", kvp.Key, kvp.Value.Group, kvp.Value.MoodleToken, /*, ... */ "\n");
                }

                towrite = sb.ToString();
            }

            await File.WriteAllTextAsync(FILEPATH, towrite, Encoding.UTF8);
        }
    }
}

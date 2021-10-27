#pragma warning disable IDE1006 // Стили именования
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Mime;

namespace PATShared
{
    public class MoodleException : Exception
    {
        public MoodleException() : base() { }
        public MoodleException(string message) : base(message) { }
        public MoodleException(string message, Exception innerException) : base(message, innerException) { }
        protected MoodleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class MoodleWarning
    {
        public string? item { get; set; }
        public int? itemid { get; set; }
        public string? warningcode { get; set; }
        public string? message { get; set; }
    }

    public class MoodleLoginReply
    {
        public string? token { get; set; }
        public string? privatetoken { get; set; }
        public string? error { get; set; }
        public string? errorcode { get; set; }
    }

    public class MoodleAttachment
    {
        public string? filename { get; set; }
        public string? filepath { get; set; }
        public long filesize { get; set; }
        public string? fileurl { get; set; }
        public int timemodified { get; set; }
        public string? mimetype { get; set; }
        public bool isexternalfile { get; set; }
    }

    public class MoodleSubmissionStatusRequest
    {
        public int assignid { get; set; }
        public int userid { get; set; }
        public int groupid { get; set; }
    }
    public class MoodleSubmissionPrevAttempt
    {
        public int attemptnumber { get; set; }
    }

    public class MoodleSubmissionFeedback
    {
        public int gradeddate { get; set; }
    }

    public class MoodleSubmissionCore
    {
        public int id { get; set; }
        public int userid { get; set; }
        public string? status { get; set; }
        public int? submissiongroup { get; set; }
        public bool submissionsenabled { get; set; }
        public bool locked { get; set; }
        public bool graded { get; set; }
        public bool canedit { get; set; }
        public bool caneditowner { get; set; }
        public bool cansubmit { get; set; }
        public int extensionduedate { get; set; }
        public bool blindmarking { get; set; }
        public int[]? usergroups { get; set; }
        public long timecreated { get; set; }
        public long timemodified { get; set; }
        public int attemptnumber { get; set; }
    }

    public class MoodleSubmissionLastAttempt
    {
        public MoodleSubmissionCore? submission { get; set; }
    }

    public class MoodleSubmissionStatus
    {
        
        public MoodleSubmissionLastAttempt? lastattempt { get; set; }
        public MoodleSubmissionPrevAttempt[]? previousattempts { get; set; }
        public MoodleWarning[]? warnings { get; set; }
    }

    public class MoodleAttachmentConfig
    {
        public string? plugin { get; set; }
        public string? subtype { get; set; }
        public string? name { get; set; }
        public string? value { get; set; }
    }

    public class MoodleAssignment
    {
        public int id { get; set; }
        public int cmid { get; set; }
        public int course { get; set; }
        public string? name { get; set; }
        public int nosubmissions { get; set; }
        public int submissiondrafts { get; set; }
        public int sendnotifications { get; set; }
        public int sendlatenotifications { get; set; }
        public int sendstudentnotifications { get; set; }
        public long duedate { get; set; }
        public long allowsubmissionsfromdate { get; set; }
        public int grade { get; set; }
        public long timemodified { get; set; }
        public int completionsubmit { get; set; }
        public int cutoffdate { get; set; }
        public long gradingduedate { get; set; }
        public int teamsubmission { get; set; }
        public int requireallteammemberssubmit { get; set; }
        public int teamsubmissiongroupingid { get; set; }
        public int blindmarking { get; set; }
        public int hidegrader { get; set; }
        public int revealidentities { get; set; }
        public int maxattempts { get; set; }
        public int markingworkflow { get; set; }
        public int markingallocation { get; set; }
        public int requiresubmissionstatement { get; set; }
        public int preventsubmissionnotingroup { get; set; }
        public MoodleAttachmentConfig[]? configs { get; set; }
        public string? intro { get; set; }
        public int introformat { get; set; }
        public MoodleAttachment[]? introfiles { get; set; }
        public MoodleAttachment[]? introattachments { get; set; }
    }

    public class MoodleCourse
    {
        public int id { get; set; }
        public string? fullname { get; set; }
        public string? shortname { get; set; }
        public long? timemodified { get; set; }
        public string? idnumber { get; set; }
        public string? summary { get; set; }
        public int? summaryformat { get; set; }
        public long? startdate { get; set; }
        public long? enddate { get; set; }
        public bool? visible { get; set; }
        public bool? showactivitydates { get; set; }
        public bool? showcompletionconditions { get; set; }
        public string? fullnamedisplay { get; set; }
        public string? viewurl { get; set; }
        public string? courseimage { get; set; }
        public int? progress { get; set; }
        public bool? hasprogress { get; set; }
        public bool? isfavourite { get; set; }
        public bool? hidden { get; set; }
        public bool? showshortname { get; set; }
        public string? coursecategory { get; set; }
        public MoodleAssignment[]? assignments { get; set; }
    }

    public class MoodleSiteInfoReply
    {
        public class MoodleSiteInfoFunction
        {
            public string? name { get; set; }
            public string? version { get; set; }
        }

        public class MoodleAdvancedFeature
        {
            public string? name { get; set; }
            public int value { get; set; }
        }

        public string? sitename { get; set; }
        public string? username { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? fullname { get; set; }
        public string? lang { get; set; }
        public int userid { get; set; }
        public string? siteurl { get; set; }
        public string? userpictureurl { get; set; }
        public MoodleSiteInfoFunction[]? functions { get; set; }
        public int downloadfiles { get; set; }
        public int uploadfiles { get; set; }
        public string? release { get; set; }
        public string? version { get; set; }
        public string? mobilecssurl { get; set; }
        public MoodleAdvancedFeature[]? advancedfeatures { get; set; }
        public bool usercanmanageownfiles { get; set; }
        public long userquota { get; set; }
        public long usermaxuploadfilesize { get; set; }
        public int userhomepage { get; set; }
        public string? userprivateaccesskey { get; set; }
        public int siteid { get; set; }
        public string? sitecalendartype { get; set; }
        public string? usercalendartype { get; set; }
        public bool userissiteadmin { get; set; }
        public string? theme { get; set; }
    }

    public class MoodleAssignmentsReply
    {
        public MoodleCourse[]? courses { get; set; }
        public MoodleWarning[]? warnings { get; set; }

        public object? Tag { get; set; }
    }

    public class MoodleTimesortRequest
    {
        // https://github.com/moodle/moodle/blob/master/calendar/externallib.php#L401
        // наш техникум использует мудл 3.11.3+ (совсем недавно обновили, судя по датам) так что.. сойдёт?

        public int? timesortfrom { get; set; } = 0;
        public int? timesortto { get; set; } = null;
        public int? aftereventid { get; set; } = 0;
        public int? limitnum { get; set; } = 20;
        public bool? limittononsuspendedevents { get; set; } = false;
        public int? userid { get; set; } = null;
        public string? searchvalue { get; set; } = null;
    }

    public class MoodleEventsReply
    {
        public class MoodleEvent
        {
            public int id { get; set; }
            public string? name { get; set; }
            public string? description { get; set; }
            public int descriptionformat { get; set; }
            public string? location { get; set; }
            public int? userid { get; set; }
            public string? component { get; set; }
            public string? modulename { get; set; }
            public int instance { get; set; }
            public string? eventtype { get; set; }
            public long timestart { get; set; }
            public long timeduration { get; set; }
            public long timesort { get; set; }
            public long timeusermidnight { get; set; }
            public int visible { get; set; }
            public long timemodified { get; set; }
            public MoodleCourse? course { get; set; }
            public bool canedit { get; set; }
            public bool candelete { get; set; }
            public string? deleteurl { get; set; }
            public string? editurl { get; set; }
            public string? viewurl { get; set; }
            public bool isactionevent { get; set; }
            public bool iscourseevent { get; set; }
            public bool iscategoryevent { get; set; }
            public string? normalisedeventtype { get; set; }
            public string? normalisedeventtypetext { get; set; }
            public string? url { get; set; }
        }

        public MoodleEvent[]? events { get; set; }
        public int firstid { get; set; }
        public int lastid { get; set; }
    }

    public class Moodle
    {
        HttpClient client;
        string baseUrl;
        string serviceName;

        public Moodle(HttpClient? hc = null, string url = "https://edu.permaviat.ru", string servName = "moodle_mobile_app")
        {
            client = (hc is null) ? new HttpClient() : hc;
            baseUrl = url;
            serviceName = servName;

            baseUrl = baseUrl.TrimEnd('/');
            if (!baseUrl.StartsWith("https://"))
            {
                throw new MoodleException("This library only supports https instances due to security reasons.");
            }
        }

        public async Task<T> Request<T>(string wstoken, string wsfunction, string? data = null)
        {
            var append = "";
            if (data is string dstr) append += "&" + dstr;

            var repl = await client.GetAsync($"{baseUrl}/webservice/rest/server.php?wstoken={wstoken}&wsfunction={wsfunction}&moodlewsrestformat=json{append}");
            if (!repl.IsSuccessStatusCode)
            {
                throw new MoodleException($"Request exception, HTTP error, {repl.StatusCode}");
            }
            
            var jsonrepl = await repl.Content.ReadAsStringAsync();
            if (jsonrepl is null)
            {
                throw new MoodleException($"Request exception, reply is empty.");
            }

            var retval = JsonConvert.DeserializeObject<T>(jsonrepl);
            if (retval is null)
            {
                throw new MoodleException($"Request exception, failed to deserialize json into {typeof(T)}.");
            }

            return retval;
        }

        public async Task<string> LoginNew(string username, string password)
        {
            var resp = await client.GetAsync($"{baseUrl}/login/token.php?service={serviceName}&username={username}&password={password}", HttpCompletionOption.ResponseContentRead);
            if (resp is null)
            {
                throw new MoodleException("Login failed, reply is null, check your HttpClient instance.");
            }

            if (!resp.IsSuccessStatusCode)
            {
                throw new MoodleException($"Login failed, HTTP error, {resp.StatusCode}");
            }

            var json = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new MoodleException($"Login failed, reply is empty.");
            }

            var reply = JsonConvert.DeserializeObject<MoodleLoginReply>(json);
            if (reply.token is null)
            {
                throw new MoodleException($"Login failed, token error, {reply.error} {reply.errorcode}");
            }

            return reply.token;
        }
    }
}
#pragma warning restore IDE1006 // Стили именования

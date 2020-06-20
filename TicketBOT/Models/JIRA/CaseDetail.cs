using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TicketBOT.Models.JIRA
{
    public class CreatedDate
    {
        public DateTime iso8601 { get; set; }
        public DateTime jira { get; set; }
        public string friendly { get; set; }
        public long epochMillis { get; set; }
    }

    public class AvatarUrls
    {
        public string _48x48 { get; set; }
        public string _24x24 { get; set; }
        public string _16x16 { get; set; }
        public string _32x32 { get; set; }
    }

    public class Links
    {
        public string jiraRest { get; set; }
        public AvatarUrls avatarUrls { get; set; }
        public string self { get; set; }
    }

    public class Reporter
    {
        public string name { get; set; }
        public string key { get; set; }
        public string emailAddress { get; set; }
        public string displayName { get; set; }
        public bool active { get; set; }
        public string timeZone { get; set; }
        public Links _links { get; set; }
    }

    public class RequestFieldValue
    {
        public string fieldId { get; set; }
        public string label { get; set; }
        public object value { get; set; }
        public object renderedValue { get; set; }
    }

    public class StatusDate
    {
        public DateTime iso8601 { get; set; }
        public DateTime jira { get; set; }
        public string friendly { get; set; }
        public long epochMillis { get; set; }
    }

    public class CurrentStatus
    {
        public string status { get; set; }
        public StatusDate statusDate { get; set; }
    }
    public partial class TemperaturesLinks
    {
        [JsonProperty("jiraRest")]
        public Uri JiraRest { get; set; }

        [JsonProperty("web")]
        public Uri Web { get; set; }

        [JsonProperty("self")]
        public Uri Self { get; set; }
    }

    public class JIRACaseDetail
    {
        public JIRACaseDetail()
        {
            requestFieldValues = new List<RequestFieldValue>();

        }
        public IList<string> _expands { get; set; }
        public int issueId { get; set; }
        public string issueKey { get; set; }
        public string requestTypeId { get; set; }
        public string serviceDeskId { get; set; }
        public CreatedDate createdDate { get; set; }
        public Reporter reporter { get; set; }
        public List<RequestFieldValue> requestFieldValues { get; set; }
        public CurrentStatus currentStatus { get; set; }

        [JsonProperty("_links")]
        public TemperaturesLinks Links { get; set; }
    }



}

using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
    public enum ConvLogType
    {
        MuteLog,
        ChatLog,
    }

    public enum Question
    {
        None, // last action / not involve any question.
        CompanyName,
        Application,
        IssueApplicationName,
        IssueDescription,
        TicketCode,
        VerificationCode,
        Retry,
    }

    public enum ConstructType
    {
        None,
        Greeting,
        RequestOperator,
        RequestBotAssistance,
        SearchCompany,
        RequestVerificationCode,
        CreateTicket,
        TicketDescription,
        TicketCreationConfirmation,
        TicketCreated,
        CheckTicket,
        TicketFound,
        Ending,
        NotImplemented,
        Retry,
        Error,
    }

    public class Conversation
    {
        public Conversation()
        {
            Id = Guid.NewGuid().ToString();
        }
        [BsonId]
        public string Id { get; set; }
        public string SenderPageId { get; set; }
        public string ConversationData { get; set; }
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
        public int ConversationLogType { get; set; } = (int)ConvLogType.ChatLog;
    }

    public class ConversationData
    {
        // The last question asked.
        public int LastQuestionAsked { get; set; } = (int)Question.None;
        // Is it answered by sender?
        public bool Answered { get; set; } = false;
        public string AnswerFreeText { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}

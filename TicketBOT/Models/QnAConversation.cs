
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Models
{
    public class QnAConversation
    {
        // Identifies the last question asked.
        public enum Question
        {
            None, // last action / not involve any question.
            Company,
            Issue,
            TicketCode,
        }

        public enum ConstructType
        {
            None,
            Greeting,
            Ending,
        }

        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public string FbSenderId { get; set; }
        // The last question asked.
        public int LastQuestionAsked { get; set; } = (int)Question.None;
        // Is it answered by sender?
        public bool Answered { get; set; } = false;
        public string AnswerFreeText { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
    }
}

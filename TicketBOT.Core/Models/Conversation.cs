﻿using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TicketBOT.Core.Models
{
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
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SenderPageId { get; set; }
        public string ConversationData { get; set; }
        public DateTime ModifiedOn { get; set; } = DateTime.Now;
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
using TicketBOT.Services.Interfaces;

namespace TicketBOT.Services.JiraServices
{
    public class JiraUserRegistrationService : IUserRegistrationService
    {
        public string GetUserInfo(string psid)
        {
            return $"Reply: {psid}";
        }
    }
}

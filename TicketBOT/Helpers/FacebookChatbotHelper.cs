using System.Security.Cryptography;
using System.Text;
using TicketBOT.Core.Models;

namespace TicketBOT.Helpers
{
    public static class FacebookChatbotHelper
    {
        public static bool VerifySignature(ApplicationSettings appSetting, string signature, string body)
        {
            var hashString = new StringBuilder();
            string mno = appSetting.FacebookApp.AppSecret;
            var val = TicketBOT.Core.Helpers.Utility.ParseDInfo(mno, appSetting.General.SysInfo);
            using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(val)))
            {
                var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
                foreach (var item in hash)
                    hashString.Append(item.ToString("X2"));
            }

            return hashString.ToString().ToLower() == signature.ToLower();
        }

        public static bool VerifyIsOneTimeNotifPayload(Models.Facebook.Messaging message)
        {
            try
            {
                return message.optin != null ? true : false;
            }
            catch
            {
                return false;
            }
        }
    }
}

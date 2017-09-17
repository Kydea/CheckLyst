using Microsoft.Owin;

namespace CheckListManager.WebApplication.Models.IntegratedAuthentication
{
    public class IntegratedAuthenticationService
    {
        public bool IsAuthorised(IHeaderDictionary requestHeaders)
        {
            return false;

            //if (RequestHeaders.TryGetValue("x-authentication-token", out string[] Values) && Values?.Length == 1)
            //{
            //    string[] Tokens = Values[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (Service.IsAuthorised(Tokens[0], Tokens[1]))
            //        return true;
            //}

            //return false;
        }
    }
}

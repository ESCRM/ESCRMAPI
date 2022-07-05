using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.WindowManager.Integration.Models;
namespace IDS.EBSTCRM.WindowManager.Integration.Services {
    public class CredentialService {
        public static Response IsValid(Credential model, out User user) {
            var userresult = new User();

            var result = new Response();
            if (string.IsNullOrEmpty(model.Username)) {
                result.Status = false;
                result.Message.Add("Username is required.");
            }
            if (string.IsNullOrEmpty(model.Password)) {
                result.Status = false;
                result.Message.Add("Password is required.");
            }            
            if (result.Status) {
                var context = new SQLDB();
                userresult = context.Users_Login(model.Username, model.Password);
                if(userresult==null)
                {
                    result.Status = false;
                    result.Message.Add("Login unsuccessful.");
                }
                else
                {
                    if (userresult.DeletedDate != null)
                    {
                        result.Status = false;
                        result.Message.Add("User is deleted.");
                    }
                    if (userresult.PasswordExpires<System.DateTime.Now)
                    {
                        result.Status = false;
                        result.Message.Add("Password expired.");
                    }
                    if (userresult == null) {
                        result.Status = false;
                        result.Message.Add("Wrong credentials");
                    }
                }
                context.Dispose();
                context = null;
            }
            user = userresult;
            return result;
        }
    }
}
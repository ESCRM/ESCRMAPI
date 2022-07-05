using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.WindowManager.Integration.Models;
namespace IDS.EBSTCRM.WindowManager.Integration.Services {
    public class DynamicFieldService {
        public static string[] ValidUIFieldViewState = { "null", "", null };
        public static string[] InvalidUIFieldType = { "label", "vr", "hr", "button", "map", "title", "externaldatalink", "sqllabel" };
        public static Response IsValid(Credential model, out User user) {
            return CredentialService.IsValid(model, out user);
        }
    }
}
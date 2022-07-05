using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.Base.Classes;
using IDS.EBSTCRM.WindowManager.Integration.Models;
using System.Collections.Generic;

namespace IDS.EBSTCRM.WindowManager.Integration.Services
{
    public class AgreementServiceForAPI
    {
        public static List<Agreement> GetAgreementsByUser(User user)
        {
            List<Agreement> result = new List<Agreement>();

            var context = new SQLDB();
            try
            {
                result = context.GetAgreements(user);
            }
            catch (System.Exception)
            {
                result = null;
            }
            context.Dispose();
            context = null;

            return result;
        }
    }
}
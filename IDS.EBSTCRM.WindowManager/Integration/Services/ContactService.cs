using IDS.EBSTCRM.Base;
using IDS.EBSTCRM.WindowManager.Integration.Models;
using System.Linq;
using System.Collections.Generic;
namespace IDS.EBSTCRM.WindowManager.Integration.Services {
    public class ContactService {
        public static Response IsValid(List<SysField> contactRequest, List<DynamicField> contactDynamicField) {

            var result = new Response();

            if (result.Status) {

                // First Level : Validation
                // Contact information should be available
                if (contactRequest.Count == 0) {
                    result.Status = false;
                    result.Message.Add("Der kan ikke gemmes før den nyoprettede kontaktperson er udfyldt korrekt.");
                } else {

                    // Second Level : Validation
                    // Contact fields should be from "[DynamicFields]" table
                    var messageContact = "Kan ikke gemmes, fordi følgende felter for kontakten ikke er fra systemet:";
                    foreach (var contactField in contactRequest) {
                        var validField = contactDynamicField.Any(w => w.DatabaseColumn == contactField.Name);

                        // add this error when there's invalid field sent through api object
                        if (!validField) {
                            result.Status = false;
                            messageContact = messageContact + "\n" + contactField.Name;
                        }
                    }
                    if (!result.Status) { result.Message.Add(messageContact); }


                    // Third Level : Validation
                    // Checking required contact fields
                    if (result.Status) {
                        messageContact = "Der kan ikke gemmes, da følgende felter for kontaktpersonen ikke er udfyldt korrekt:";
                        foreach (var contactField in contactDynamicField.Where(w => w.RequiredState == 1).ToList()) {
                            var field = contactRequest.FirstOrDefault(w => w.Name == contactField.DatabaseColumn);
                            if (field != null) {
                                if (string.IsNullOrEmpty(field.Value)) {
                                    result.Status = false;
                                    messageContact = messageContact + "\n" + field.Name;
                                }
                            } else {

                                // when required field not found in contact request
                                result.Status = false;
                                messageContact = messageContact + "\n" + contactField.DatabaseColumn;
                            }
                        }
                        if (!result.Status) { result.Message.Add(messageContact); }
                    }
                }
            }
            return result;

        }
    }
}
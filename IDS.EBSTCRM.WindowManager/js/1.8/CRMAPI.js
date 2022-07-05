//CRM API - Main class
var CRM_API = function (userData, w) {
    //Current user logged in
    this.CurrentUser = new CRM_User(userData[0], userData[1], userData[2], userData[3], userData[4]);

    //W2L Integration
    this.W2L = new CRM_W2L();

    //AVN Fields
    this.AVN = new CRM_AVNFields();

    //Access to Window Manalger
    this.GUI = w.WindowManager;

    //SMV POT Fields
    this.SMVPOT = new CRM_SMVFields();

    //Email sender
    this.Email = new CRM_Email();
};

//Email Sending
var CRM_Email = function () {
    this.SendEmail = function (from, to, subject, body, onSuccess, onError) {
        var onErrorOccurred = function (AjaxObject, onError) {
            var result = AjaxObject.Response;
            AjaxObject.Reset();
            AjaxObject.Dispose();
            AjaxObject = null;

            var re = new RegExp('<title\\b[^>]*>(.*?)</title>');
            var m = re.exec(result);
            if (m != null && onError != null) {
                onError(m);
            }
        }

        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/SendEmail.aspx';

        AjaxObject.encVar('from', from);
        AjaxObject.encVar('to', to);
        AjaxObject.encVar('subject', subject);
        AjaxObject.encVar('body', body);

        AjaxObject.OnCompletion = function () { onSuccess(); };
        AjaxObject.OnError = function () { onErrorOccurred(AjaxObject, onError); };
        AjaxObject.OnFail = function () { onErrorOccurred(AjaxObject, onError); };
        AjaxObject.RunAJAX();
    };
}


//User details
var CRM_User = function (un, pw, fn, ln, em) {
    this.Username = un;
    this.Firstname = fn;
    this.Lastname = ln;
    this.Email = em;

};

//AVN Fields
var CRM_AVNFields = function () {

    //Get value from an AVN field
    this.GetValue = function (fieldName) {
        var o = document.getElementById(fieldName);

        if (o != null) {
            return o.value;
        }
        else
            w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
    };

    //Set a value to an AVN field
    this.SetValue = function (fieldName, value) {
        var o = document.getElementById(fieldName);

        if (o != null) {
            o.value = value;
            o.onchange();
        }
        else
            w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
    }

    //Get checked from an AVN field
    this.GetChecked = function (fieldName) {
        var o = document.getElementById(fieldName);

        if (o != null) {
            return o.checked;
        }
        else
            w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
    };

    //Set a checked to an AVN field
    this.SetChecked = function (fieldName, value) {
        var o = document.getElementById(fieldName);

        if (o != null) {
            o.checked = value;
            o.onchange();
        }
        else
            w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
    }

};

//CRM SMV/POT Fields
var CRM_SMVFields = function () {

    this.Contact = new CRM_SMVContactFields();
    this.Company = new CRM_SMVCompanyFields();
};

var CRM_SMVContactFields = function () {

    //Internal Methods
    var getValue = function (fieldName, showWarning) {
        var o = __GetContactObject(fieldName);

        if (o != null) {
            return o.value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }

        return '';
    };

    var setValue = function (fieldName, value, showWarning) {
        var o = __GetContactObject(fieldName);

        if (o != null) {
            o.value = value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }
    };

    var getChecked = function (fieldName, showWarning) {
        var o = __GetContactObject(fieldName);

        if (o != null) {
            return o.checked;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }

        return '';
    };

    var setChecked = function (fieldName, value, showWarning) {
        var o = __GetContactObject(fieldName);

        if (o != null) {
            o.checked = value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }
    };

    //Methods
    this.GetValue = function (fieldName) { return getValue(fieldName, true); };
    this.SetValue = function (fieldName, value) { return setValue(fieldName, value, true); };
    this.GetChecked = function (fieldName) { return getChecked(fieldName, true); };
    this.SetChecked = function (fieldName, value) { return setChecked(fieldName, value, true); };

    //Properties
//    this.Firstname = getValue('z_contacts_1_Fornavn_1');
//    this.Lastname = getValue('z_contacts_1_Efternavn_1');
//    this.Email = getValue('z_contacts_1_Email_1');
//    this.Phone1 = getValue('z_contacts_1_Telefon 1_1');
//    this.Phone2 = getValue('z_contacts_1_Telefon 2_1');
//    this.Fax = getValue('z_contacts_1_Fax_1');

    this.Firstname = getValue('Fornavn');
    this.Lastname = getValue('Efternavn');
    this.Email = getValue('Email');
    this.Phone1 = getValue('Telefon 1');
    this.Phone2 = getValue('Telefon 2');
    this.Fax = getValue('Fax');
};

var CRM_SMVCompanyFields = function () {
    //Internal Methods
    var getValue = function (fieldName, showWarning) {
        var o = __GetCompanyObject(fieldName);

        if (o != null) {
            return o.value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }

        return '';
    };

    var setValue = function (fieldName, value, showWarning) {
        var o = __GetCompanyObject(fieldName);

        if (o != null) {
            o.value = value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }
    };

    var getChecked = function (fieldName, showWarning) {
        var o = __GetCompanyObject(fieldName);

        if (o != null) {
            return o.checked;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }

        return '';
    };

    var setChecked = function (fieldName, value, showWarning) {
        var o = __GetCompanyObject(fieldName);

        if (o != null) {
            o.checked = value;
        }
        else {
            if (showWarning)
                w.Alert('Feltet "' + fieldName + '" findes ikke!', 'Fejl!');
        }
    };

    //Methods
    this.GetValue = function (fieldName) { return getValue(fieldName, true); };
    this.SetValue = function (fieldName, value) { return setValue(fieldName, value, true); };
    this.GetChecked = function (fieldName) { return getChecked(fieldName, true); };
    this.SetChecked = function (fieldName, value) { return setChecked(fieldName, value, true); };

    //Properties
//    this.CompanyName = getValue('z_companies_1_Firmanavn_1');
//    this.Email = getValue('z_companies_1_Email_1');
//    this.Fax = getValue('z_companies_1_Fax_1');
//    this.County = getValue('z_companies_1_Kommune_1');
//    this.CVR = getValue('z_companies_1_CVR-nummer_1');
//    this.CPR = getValue('z_contacts_1_CPR-nummer_1');
//    this.Address = getValue('z_companies_1_Gade_1');
//    this.ZipCode = getValue('z_companies_1_Post nr._1');
//    this.City = getValue('z_companies_1_By_1');
//    this.Phone = getValue('z_companies_1_Telefon_1');
//    this.Web = getValue('z_companies_1_Web_1');
//    this.Established = getValue('z_companies_1_Etableringsår_1');
//    this.Type = getValue('z_companies_1_Type_1');
//    this.Sector = getValue('z_companies_1_Branche_1');
//    this.Growth = getValue('z_companies_1_Vækst_1');
//    this.FirstContact = getValue('z_companies_1_Første Kontakt_1');
//    this.PNR = getValue('z_companies_1_P-Nummer_1');
//    this.Country = getValue('z_companies_1_Land_1');

    this.CompanyName = getValue('Firmanavn');
    this.Email = getValue('Email');
    this.Fax = getValue('Fax');
    this.County = getValue('Kommune');
    this.CVR = getValue('CVR-nummer');
    this.CPR = getValue('CPR-nummer');
    this.Address = getValue('Gade');
    this.ZipCode = getValue('Post nr.');
    this.City = getValue('By');
    this.Phone = getValue('Telefon');
    this.Web = getValue('Web');
    this.Established = getValue('Etableringsår');
    this.Type = getValue('Type');
    this.Sector = getValue('Branche');
    this.Growth = getValue('Vækst');
    this.FirstContact = getValue('Første Kontakt');
    this.PNR = getValue('P-Nummer');
    this.Country = getValue('Land');

};

//W2L
var CRM_W2L = function () {

    this.OpenVaekstHjulImage = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);


        if (isNaN(aId) || aId == 0) {
            w.Alert('Du skal gemme din AVN før væksthjulet kan hentes som et billede!', 'Fejl!', 'Images/Alert.png');
            return;
        }

        window.open('Integration/W2L.aspx?Event=DownloadImage&AvnId=' + aId);
    };

    this.OpenVaekstHjulImageVaekstUnivers = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);


        if (isNaN(aId) || aId == 0) {
            w.Alert('Du skal gemme din AVN før væksthjulet kan hentes som et billede!', 'Fejl!', 'Images/Alert.png');
            return;
        }

        window.open('Integration/W2L.aspx?Type=2&Event=DownloadImage&AvnId=' + aId);
    };

    this.OpenStartVaekst = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);
        var smvId = parseInt(document.getElementById('txtSMVContactId').value, 10);

        if (isNaN(smvId)) {
            smvId = parseInt(document.getElementById('txtSMVCompanyId').value, 10);
        }

        if (isNaN(aId) || isNaN(smvId) || aId == 0 || smvId == 0) {
            w.Alert('Du skal gemme din AVN før Startvækst kan åbnes!', 'Fejl!', 'Images/Alert.png');
            return;
        }
        window.open('Integration/W2L.aspx?Event=Login&AvnId=' + aId + "&smvId=" + smvId + "&Firstname=" + (CRM.SMVPOT.Contact.Firstname).replace(/\&/g, '%26') + "&Lastname=" + (CRM.SMVPOT.Contact.Lastname).replace(/\&/g, '%26') + "&Email=" + escape(CRM.SMVPOT.Contact.Email));
    };

    this.DownloadVaekstPlan = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);
        var smvId = parseInt(document.getElementById('txtSMVContactId').value, 10);

        if (isNaN(smvId)) {
            smvId = parseInt(document.getElementById('txtSMVCompanyId').value, 10);
        }

        if (isNaN(aId) || isNaN(smvId) || aId == 0 || smvId == 0) {
            w.Alert('Du skal gemme din AVN før væksthjulet kan hentes som PDF!', 'Fejl!', 'Images/Alert.png');
            return;
        }

        window.open('Integration/W2L.aspx?Event=DownloadPDF&AvnId=' + aId + "&smvId=" + smvId);
    };
    this.DownloadVaekstPlanVaekstUnivers = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);
        var smvId = parseInt(document.getElementById('txtSMVContactId').value, 10);

        if (isNaN(smvId)) {
            smvId = parseInt(document.getElementById('txtSMVCompanyId').value, 10);
        }

        if (isNaN(aId) || isNaN(smvId) || aId == 0 || smvId == 0) {
            w.Alert('Du skal gemme din AVN før væksthjulet kan hentes som PDF!', 'Fejl!', 'Images/Alert.png');
            return;
        }

        window.open('Integration/W2L.aspx?Type=2&Event=DownloadPDF&AvnId=' + aId + "&smvId=" + smvId);
    };

    this.SaveVaekstPlan = function () {
        var aId = parseInt(document.getElementById('txtId').value, 10);
        var smvId = parseInt(document.getElementById('txtSMVContactId').value, 10);

        if (isNaN(smvId)) {
            smvId = parseInt(document.getElementById('txtSMVCompanyId').value, 10);
        }

        if (isNaN(aId) || isNaN(smvId) || aId == 0 || smvId == 0) {
            w.Alert('Du skal gemme din AVN før du kan gemme den på StarkVækst!', 'Fejl!', 'Images/Alert.png');
            return;
        }

        window.open('Integration/W2L.aspx?Event=Save&AvnId=' + aId + "&smvId=" + smvId);
    };
};


//Default not implemented routine
function CRM_NotImplemented() {
    w.Alert('Denne funktion er ikke implementeret!', 'Ikke implementeret!');
};



// Builds entire help collection for the API
function CRM_BuildAPIGuide(tw) {
    var r = tw.AddNode('CRM', 'Images/AVN/namespace.png');

    //User object
    var u = r.AddNode('CurrentUser', 'Images/AVN/class.png');
    u.Example = 'Den bruger der er logget ind i CRM Systemet';

    u.AddNode('Username', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.CurrentUser.Username);';
    //u.AddNode('Password', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.CurrentUser.Password);';
    u.AddNode('Firstname', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.CurrentUser.Firstname);';
    u.AddNode('Lastname', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.CurrentUser.Lastname);';
    u.AddNode('Email', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.CurrentUser.Email);';

    // W2L
    var w = r.AddNode('W2L', 'Images/AVN/class.png');
    w.Example = 'Integration til W2L - Startvækst';

    w.AddNode('OpenStartVaekst', 'Images/AVN/method.png').Example = 'CRM.W2L.OpenStartVaekst();';
    w.AddNode('DownloadVaekstPlan', 'Images/AVN/method.png').Example = 'CRM.W2L.DownloadVaekstPlan();';
    w.AddNode('DownloadVaekstPlanVaekstUnivers', 'Images/AVN/method.png').Example = 'CRM.W2L.DownloadVaekstPlanVaekstUnivers();';
    w.AddNode('SaveVaekstPlan', 'Images/AVN/method.png').Example = 'CRM.W2L.SaveVaekstPlan();';
    w.AddNode('OpenVaekstHjulImage', 'Images/AVN/method.png').Example = 'CRM.W2L.OpenVaekstHjulImage();';
    w.AddNode('OpenVaekstHjulImageVaekstUnivers', 'Images/AVN/method.png').Example = 'CRM.W2L.OpenVaekstHjulImageVaekstUnivers();';

    // AVN
    var a = r.AddNode('AVN', 'Images/AVN/class.png');
    a.Example = 'Styring og interaktion med Projektnoter';

    a.AddNode('GetValue([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.AVN.GetValue(\'Feltnavn\'));';
    a.AddNode('SetValue([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.AVN.SetValue(\'Feltnavn\',\'Felt værdi\');';
    a.AddNode('GetChecked([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.AVN.GetChecked(\'Feltnavn\'));';
    a.AddNode('SetChecked([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.AVN.SetChecked(\'Feltnavn\', true);';

    // SMV POT FIELDS
    var smvpot = r.AddNode('SMVPOT', 'Images/AVN/class.png');
    smvpot.Example = 'Hent gemte værdier fra SMV/POT kontaktpersoner og virksomheder';

    //Contact
    var smvContact = smvpot.AddNode('Contact', 'Images/AVN/class.png');
    smvContact.Example = 'Hent gemte værdier fra SMV/POT kontaktpersoner';

    smvContact.AddNode('GetValue([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.GetValue(\'Feltnavn\'));';
    smvContact.AddNode('SetValue([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.SMVPOT.Contact.SetValue(\'Feltnavn\',\'Felt værdi\');';
    smvContact.AddNode('GetChecked([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.GetChecked(\'Feltnavn\'));';
    smvContact.AddNode('SetChecked([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.SMVPOT.Contact.SetChecked(\'Feltnavn\', true);';

    smvContact.AddNode('Firstname', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Firstname);';
    smvContact.AddNode('Lastname', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Lastname);';
    smvContact.AddNode('Email', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Email);';
    smvContact.AddNode('Phone1', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Phone1);';
    smvContact.AddNode('Phone2', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Phone2);';
    smvContact.AddNode('Fax', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Contact.Fax);';

    //Company
    var smvCompany = smvpot.AddNode('Company', 'Images/AVN/class.png');
    smvCompany.Example = 'Hent gemte værdier fra SMV/POT virksomheder';

    smvCompany.AddNode('GetValue([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.GetValue(\'Feltnavn\'));';
    smvCompany.AddNode('SetValue([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.SMVPOT.Company.SetValue(\'Feltnavn\',\'Felt værdi\');';
    smvContact.AddNode('GetChecked([FieldName])', 'Images/AVN/method.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.GetChecked(\'Feltnavn\'));';
    smvContact.AddNode('SetChecked([FieldName],[Value])', 'Images/AVN/method.png').Example = 'CRM.SMVPOT.Company.SetChecked(\'Feltnavn\', true);';

    smvCompany.AddNode('CompanyName', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.CompanyName);';
    smvCompany.AddNode('CVR', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.CVR);';
    smvCompany.AddNode('CPR', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.CPR);';
    smvCompany.AddNode('PNR', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.PNR);';

    smvCompany.AddNode('Address', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Address);';
    smvCompany.AddNode('ZipCode', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.ZipCode);';
    smvCompany.AddNode('City', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.City);';
    smvCompany.AddNode('County', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.County);';
    smvCompany.AddNode('Country', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Country);';

    smvCompany.AddNode('Email', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Email);';

    smvCompany.AddNode('Phone', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Phone);';
    smvCompany.AddNode('Fax', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Fax);';

    smvCompany.AddNode('Web', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Web);';
    
    smvCompany.AddNode('Established', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Established);';
    smvCompany.AddNode('Type', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Type);';
    smvCompany.AddNode('Sector', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Sector);';
    smvCompany.AddNode('Growth', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.Growth);';
    smvCompany.AddNode('FirstContact', 'Images/AVN/prop.png').Example = 'CRM.GUI.Alert(CRM.SMVPOT.Company.FirstContact);';


    // GUI
    var g = r.AddNode('GUI', 'Images/AVN/class.png');
    g.Example = 'Tilgå CRM Systemets Window Manager';

    g.AddNode('Alert([Message], <i>[Title], [Icon], [CallBack]</i>)', 'Images/AVN/method.png').Example = '//Funktion der kaldes, når brugeren trykker "OK"\r\nfunction MinFunktion(sender, retval)\r\n{\r\n//Kode her\r\n};\r\n\r\n//Dette afvikles nu\r\nCRM.GUI.Alert(\'Tryk på OK\', \'Besked\', \'Images/alertSuccess.png\', MinFunktion);';

    //Send emails
    var sendEmails = r.AddNode('Email', 'Images/AVN/class.png');
    sendEmails.Example = 'Send en email';

    sendEmails.AddNode('SendEmail([To], [From], [Subject], [Body], <i>[OnSucess], [OnError]</i>)', 'Images/AVN/method.png').Example = 'CRM.Email.SendEmail(\'mgr@vhsyd.dk\', \'Mit emne\', \'<b>Fed tekst i email</b>\', function() { w.Alert(\'Email afsendt\'); }, function(fejlbesked) { w.Alert(\'Fejl under afsendelse: \' + fejlbesked); } );';

};

/* INTERNAL HIDDEN FUNCTIONS */
function __GetCompanyObject (fieldName, noReverse) {
    var o = null; // document.getElementById(fieldName);
    //if (o == null) {
        var orgs = document.getElementById('txtOrganisations').value.split(',');

        for (var i = 0; i < orgs.length; i++) {
            var name = 'z_companies_' + orgs[i] + '_' + fieldName + '_' + orgs[i];
            o = document.getElementById('z_companies_' + orgs[i] + '_' + fieldName + '_' + orgs[i]);
            if (o != null) break;
        }
    //}

        if (o == null && !noReverse)
            o = __GetContactObject(fieldName, true);

    return o;
};

//Get object related to SMV Contact
function __GetContactObject(fieldName, noReverse) {
    var o = null; // document.getElementById(fieldName);
    //if (o == null) {
        var orgs = document.getElementById('txtOrganisations').value.split(',');

        for (var i = 0; i < orgs.length; i++) {
            var name = 'z_contacts_' + orgs[i] + '_' + fieldName + '_' + orgs[i];
            o = document.getElementById('z_contacts_' + orgs[i] + '_' + fieldName + '_' + orgs[i]);
            if (o != null) break;
        }
    //}

        if (o == null && !noReverse)
            o = __GetCompanyObject(fieldName, true);

    return o;
};
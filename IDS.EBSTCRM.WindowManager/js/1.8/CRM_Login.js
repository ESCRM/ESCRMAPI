function logoff()
{
    if(startMenu.OriginalParent != startMenu.parentNode)
    {
        if(startMenu.parentNode)
            startMenu.parentNode.removeChild(startMenu);
            
        startMenu.OriginalParent.appendChild(startMenu);
    }   
    if(noteReminderButton)
    {
        for(var i=0;i<noteReminderButton.Reminders.length;i++)
        {
            clearTimeout( noteReminderButton.Reminders[i].TimerId );
        }
        noteReminderButton.Reminders.length=0;
        noteReminderButton.Reminders=new Array();
    }   
    //windowManager.Desktop.FadeToColor('black', function() { afterfade_logoff(); windowManager.ChangeLayout('styles/windowmanager/default.css'); });
    afterfade_logoff(); windowManager.ChangeLayout('styles/windowmanager/default.css');
};

function afterfade_logoff()
{
    widgets.Clear();
    
    var doc = document;
    var ud = doc.getElementById('spCurrentUser');
    ud.setAttribute('uid' , '');
    
    windowManager.Desktop.Taskbar.Hide();
    
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/login.aspx';
    AjaxObject.encVar('dologoff', 'true');

    AjaxObject.OnCompletion = function(){ __logoff_doLogoff(AjaxObject); }; 
    AjaxObject.OnError = function(){ __logoff_doLogoff(AjaxObject);};
    AjaxObject.OnFail = function(){  __logoff_doLogoff(AjaxObject);};
    AjaxObject.RunAJAX();
    
    
};

function __logoff_doLogoff(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject=null;
    
    windowManager.CloseWindows();
    windowManager.Desktop.Taskbar.ClearButtons();
    unBuildTaskbar();
    
    document.getElementById('trTopToolbar').style.display='none';
    
    startMenuRecentItems.SetSize(258, 22);
    windowManager.CreateWindow('login.aspx');
    // windowManager.Desktop.UnFadeFromColor('black', function() {  });
};

function login()
{
    core.DOM.DisableFields(document);
    document.getElementById('imgBusy').style.display='';
    
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/login.aspx';
    AjaxObject.encVar('dologin', 'true');
    AjaxObject.encVar('username', document.frmLogin.txtUsername.value);
    AjaxObject.encVar('password', document.frmLogin.txtPassword.value);


    AjaxObject.OnCompletion = function(){ __login_doLogin(AjaxObject); }; 
    AjaxObject.OnError = function(){ __login_doLogin_Err(AjaxObject);};
    AjaxObject.OnFail = function(){  __login_doLogin_Err(AjaxObject);};
    AjaxObject.RunAJAX();
};

function __login_doLogin_Err(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject=null;
    var patt = new RegExp(/<title>(.+?)<\/title>/g);
    var arr = patt.exec(result);
    var val = arr[arr.length-1];
    
    w.Alert(val, 'Fejl i login','images/alertFailed.png');
    
    core.DOM.EnableFields(document);
    document.frmLogin.txtPassword.value = '';
    document.getElementById('imgBusy').style.display='none';
    
};

function __login_doLogin(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject=null;
    
    var rd = document.createElement('DIV');
    rd.innerHTML = result;
    
    if(rd.childNodes[0].innerHTML=='OK')
    {
        //windowManager.Desktop.FadeToColor('black', function() { 
        
        parent.document.getElementById('quickLinks').innerHTML = rd.childNodes[23].innerHTML;
        
        w.WindowManager.Desktop.Taskbar.Show();
        
        __writeUserDetails(w.WindowManager.Document, rd)
        
        parent.__renderUserDetails(); //w.WindowManager.Document);

        if (parent.document.getElementById('txtOpenWindow').value != '') {
            w.WindowManager.CreateWindow(parent.document.getElementById('txtOpenWindow').value);
        }

        parent.buildTaskbar();
        parent.loadWidgets();
        w.Close();

         //windowManager.Desktop.UnFadeFromColor('black');  //});

    }
    else if(rd.childNodes[0].innerHTML=='CHANGEPWD')
    {
        var div = document.createElement('DIV');
        div.innerHTML=result;
        var nw = w.CreateWindow('userPasswordChange.aspx?userId=' + div.childNodes[1].innerHTML, w);
        nw.Variables = null;
        nw.OnClose =__pwdChanged;
    }
    else
    {
        w.Alert('Du har indtastet et forkert brugernavn eller password!', 'Fejl i login','images/alert.png');
        core.DOM.EnableFields(document);
        document.frmLogin.txtPassword.value = '';
        document.getElementById('imgBusy').style.display='none';
    
    }
};

function __pwdChanged(sender,retval)
{
    if(retval)
    {
        document.frmLogin.txtPassword.value ='';
        core.DOM.EnableFields(document);
        document.getElementById('txtPassword').focus();
        document.getElementById('imgBusy').style.display='none';
        
    }
    else
    {
        document.frmLogin.txtUsername.value ='';
        document.frmLogin.txtPassword.value ='';
        core.DOM.EnableFields(document);
        document.getElementById('txtUsername').focus();
        document.getElementById('imgBusy').style.display='none';
    }
    
};

function __writeUserDetails(doc, rd)
{
    if(!doc) doc = document;
    var ud = doc.getElementById('spCurrentUser');

    

    ud.innerHTML = rd.childNodes[20].innerHTML;
    ud.setAttribute('uid' , rd.childNodes[6].innerHTML);
    ud.setAttribute('role' , rd.childNodes[2].innerHTML);
    ud.setAttribute('roleName' , rd.childNodes[2].innerHTML);
    ud.setAttribute('syncEmailsAtStart' , rd.childNodes[3].innerHTML =='1' ? 'true' : '');
    ud.setAttribute('earlywarning' , rd.childNodes[5].innerHTML =='1' ? 'true' : '');
    ud.setAttribute('widgets' , rd.childNodes[11].innerHTML);
    ud.setAttribute('backgroundImage' , rd.childNodes[15].innerHTML);
    ud.setAttribute('windowLayout' , rd.childNodes[17].innerHTML);
    ud.setAttribute('useTopMenu' , rd.childNodes[13].innerHTML);
    ud.setAttribute('TaskbarGrouping' , rd.childNodes[21].innerHTML);
    ud.setAttribute('organisationType' , rd.childNodes[22].innerHTML);
    ud.setAttribute('nowelcome' , rd.childNodes[18].innerHTML);
    ud.setAttribute('faultyContactsOrCompanies', rd.childNodes[24].innerHTML);

    //New properties
    ud.setAttribute('AllowImportData', rd.childNodes[25].innerHTML);
    ud.setAttribute('AllowMassRegistrations', rd.childNodes[26].innerHTML);
    ud.setAttribute('AllowMailgroupModifications', rd.childNodes[27].innerHTML);
    ud.setAttribute('AllowImpersonation', rd.childNodes[28].innerHTML);
    ud.setAttribute('FreeUser', rd.childNodes[29].innerHTML);
    ud.setAttribute('InvoiceAdvising', rd.childNodes[30].innerHTML);
    ud.setAttribute("AllowHourlyTimesheet", rd.childNodes[31].innerHTML);
};

function __renderUserDetails(doc)
{
    var hasParent = doc !=null;
    if(!doc) doc = document;
    var ud = doc.getElementById('spCurrentUser');

    if(ud.getAttribute('faultyContactsOrCompanies') == 'true')
    {
        if(hasParent)
        {
            parent.setTimeout(function() {  windowManager.CreateWindow('SMVStagingErrors.aspx'); },1000);
        }
        else
            setTimeout(function() { windowManager.CreateWindow('SMVStagingErrors.aspx'); },1000);
    }
    else if(ud.getAttribute('nowelcome')!='true')
    {
        if(hasParent)
        {
            parent.setTimeout(function() {  windowManager.CreateWindow('GettingStarted.aspx'); },1000);
        }
        else
            setTimeout(function() { windowManager.CreateWindow('GettingStarted.aspx'); },1000);
    }
    
    //4D1F4033-1CB9-4BED-B7CA-AE1ED49A6F9B    
    //alert(ud.getAttribute('uid').toUpperCase() != '4D1F4033-1CB9-4BED-B7CA-AE1ED49A6F9B');
//    if (ud.getAttribute('uid').toUpperCase() != '4D1F4033-1CB9-4BED-B7CA-AE1ED49A6F9B') {

//        doc.getElementById('divSwitchUser').style.display = 'none';
//        //doc.getElementById('divImportData').style.display = 'none';
//    }
//    else {
//        doc.getElementById('divSwitchUser').style.display = '';
//        //doc.getElementById('divImportData').style.display = '';
    //    }

//        if (ud.getAttribute('uid').toUpperCase() != '4D1F4033-1CB9-4BED-B7CA-AE1ED49A6F9B' &&
//            ud.getAttribute('uid').toUpperCase() != '8721F328-4DE7-4CD4-873D-F631EBBEEB2A') {

//            doc.getElementById('spAdmnAVNNoteCategories').style.display = 'none';
//            doc.getElementById('spAdmAVNs').style.display = 'none';
//        }
//        else {
//            doc.getElementById('spAdmnAVNNoteCategories').style.display = 'none';
//            doc.getElementById('spAdmAVNs').style.display = 'none';
//        }

    doc.getElementById('divSwitchUser').style.display = ud.getAttribute('AllowImpersonation') == 'true' ? '' : 'none';
    if(doc.getElementById('divImportData')!=null) doc.getElementById('divImportData').style.display = ud.getAttribute('AllowImportData') == 'true' ? '' : 'none';
    doc.getElementById('divBatchJob').style.display = ud.getAttribute('AllowMassRegistrations') == 'true' ? '' : 'none';

    doc.getElementById('spStartmenuUsername').innerHTML = ud.innerHTML;
    doc.getElementById('aEarlyWarning').style.display = ud.getAttribute('earlywarning')=='true' ? '' : 'none';
    doc.getElementById('aGlobalAdmin').style.display=parseInt(ud.getAttribute('role'))>30 ? '' : 'none';
    doc.getElementById('aAdmin').style.display = parseInt(ud.getAttribute('role')) > 0 ? '' : 'none';

    if (ud.getAttribute('AllowHourlyTimesheet') == "true") {
        doc.getElementById('aTimeApproval').style.display = '';
        doc.getElementById('divWeeklyTimeApproval').style.display = 'none';
    } else {
        doc.getElementById('aTimeApproval').style.display = 'none';
        doc.getElementById('divWeeklyTimeApproval').style.display = '';
    }
    
    if(parseInt(ud.getAttribute('role'),10)==25)
    {            
        //doc.getElementById('spAdmProjects').style.display='none';
        //doc.getElementById('spAdmItemNos').style.display='none';
        doc.getElementById('admSpFinancialPools').style.display='none';
        doc.getElementById('admSpUnits').style.display='none';
    }
    else 
    {            
        //doc.getElementById('spAdmProjects').style.display='';
        //doc.getElementById('spAdmItemNos').style.display='';
        doc.getElementById('admSpFinancialPools').style.display='';
        doc.getElementById('admSpUnits').style.display='';
    }
    
    doc.getElementById('spTrashCan').style.display=parseInt(ud.getAttribute('role'))!=25 ? '' : 'none';
    doc.getElementById('spAdminDesignFields').style.display=parseInt(ud.getAttribute('role'))!=25 ? '' : 'none';
    doc.getElementById('spAdminDoublesManager').style.display=parseInt(ud.getAttribute('organisationType'))<2 ? parseInt(ud.getAttribute('role'))!=25 ? '' : 'none' : 'none';

    windowManager.ChangeLayout('styles/windowmanager/' + ud.getAttribute('windowLayout'));
    windowManager.Desktop.Taskbar.SetAutogroupButtons(ud.getAttribute('TaskbarGrouping')=='1');
};
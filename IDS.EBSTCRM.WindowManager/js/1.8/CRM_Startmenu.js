var startMenu = null;
var startMenuRecentItems = null;
var topToolbar = null;
var topToolbarContextMenu = null;
var projectContextMenu = null;
var taskbarContextMenu = null;
var noteReminderButton = null;
var noteReminderWindow = null;
//** Tilføjet Timegodkendelse variable. ESCRM-134/135
var noteTimegodkendelseWindow = null;

function loadWidgets(doc) {
    if (!doc) doc = document;
    var ud = doc.getElementById('spCurrentUser');

    var ws = ud.getAttribute('widgets').split('\\');

    for (var i = 0; i < ws.length; i++) {
        if (ws[i] != '' && ws[i] != ' ' && ws[i] != null)
            widgets.AddWidget(ws[i]);
    }
};

//** Tilføjet for at vise favoritter, ESCRM-118/121
function loadFavoriteWidget() {
    //** Checvk hvor manger widgets der er først, ESCRM-13/133
    if (widgets.Widgets.length == 0)
        widgets.AddWidget('FavoriteSMV@@');
};

//** Tilføjet for at fjerne favoritter, ESCRM-118/121
function unLoadFavoriteWidget() {
    //** Checvk hvor manger widgets der er først, ESCRM-13/133
    if (widgets.Widgets.length >= 1)
        widgets.ClearFavorite('FavoriteSMV');
};

function taskbarSearch(sender) {
    var sel = document.getElementById('selStartmenuSearchIn');

    var args = new Array();
    args[0] = (sender.srcElement || sender.target).value;
    var nw = null;



    if (sel.value == 'SMV')
        nw = windowManager.CreateWindow('SMVContacts.aspx');
    else if (sel.value == 'SAM')
        nw = windowManager.CreateWindow('SAMContacts.aspx');
    else
        nw = windowManager.CreateWindow('media.aspx');

    nw.Variables = args;
};

function reloadMostViewedWindows() {
    //TODO: Check if this causes "insecure"
    startMenuRecentItems.ReadURLItems('ajax/getMostUsedWindows.aspx');
    setTimeout('reloadMostViewedWindows()', 60000);

};

function buildTaskbar() {
    //Reminders
    if (!noteReminderButton) {
        noteReminderButton = document.createElement('IMG');
        noteReminderButton.src = 'images/note.png';
        noteReminderButton.style.marginTop = 12;
        noteReminderButton.style.display = 'none';
        noteReminderButton.style.cursor = 'pointer';
        windowManager.Desktop.Taskbar.NotifyControl.appendChild(noteReminderButton);
        noteReminderButton.onclick = ShowReminderWindow;

        noteReminderButton.Reminders = new Array();
    }
    else {
        for (var i = 0; i < noteReminderButton.Reminders.length; i++) {
            clearTimeout(noteReminderButton.Reminders[i].TimerId);
        }
        noteReminderButton.Reminders.length = 0;
        noteReminderButton.Reminders = new Array();
    }

    //prepare keyboard events
    windowManager.Core.Keyboard.BindKeyUpToObject(document.getElementById('txtStartMenuSearch'), 13, taskbarSearch);

    //Prepare recent listview

    var div = document.getElementById('divStartmenuRecentItems');
    if (div.childNodes.length == 0) {
        startMenuRecentItems = new IDC_Listview(document, div, 258, 30);
        startMenuRecentItems.AddColumnHeader('Medie', 260);
        startMenuRecentItems.MultiSelect = false;
        startMenuRecentItems.OnSelect = function (l) { if (!l) return; l.Listview.ClearSelected(); windowManager.CreateWindow(l.Items[1]); };
        startMenuRecentItems.HideColumnHeaders();
        startMenuRecentItems.SetSize(258, 22);
        startMenuRecentItems.SetSize(258, parseInt(tblRecentItems.offsetHeight / 22, 10) * 22);
        startMenuRecentItems.Container.style.overflow = 'hidden';
        startMenuRecentItems.ChangeView(1);
    }
    else {
        startMenuRecentItems.SetSize(258, 30);
        startMenuRecentItems.SetSize(258, parseInt(tblRecentItems.offsetHeight / 22, 10) * 22);
    }

    reloadMostViewedWindows();


    startMenu = document.getElementById('tblStartmenu');
    startMenu.OriginalParent = startMenu.parentNode;



    //Startmenu
    cmdStart = windowManager.Desktop.Taskbar.CreateTaskbarButton('images/login32.png', 'images/login.png', 'Start',
        function () {
            while (windowManager.Desktop.Taskbar.MultipleWindowsControl.hasChildNodes()) {
                windowManager.Desktop.Taskbar.MultipleWindowsControl.removeChild(windowManager.Desktop.Taskbar.MultipleWindowsControl.lastChild);
            }



            windowManager.Desktop.Taskbar.MultipleWindowsControl.appendChild(startMenu);
            windowManager.ContextMenuManager.Show(windowManager.Desktop.Taskbar.MultipleWindowsControl, 0, null, null, 40,
                function () { return (windowManager.Desktop.Taskbar.MultipleWindowsControl.offsetWidth - startMenu.offsetWidth) / 2; }
            );
            tblRecentItemsAndSearch.style.height = tblRecentItemsAndSearch.parentNode.offsetHeight;
            startMenuRecentItems.SetSize(258, startMenuRecentItems.ParentObject.parentNode.offsetHeight);
        });

    cmdStart.AnimationEnabled = false;
    cmdStart.Control.oncontextmenu = null;



    //Quicklinks      
    var qls = document.getElementById('quickLinks');

    while (qls.hasChildNodes()) {
        var nd = qls.removeChild(qls.lastChild);
        var btn = windowManager.Desktop.Taskbar.CreateTaskbarButton(nd.getAttribute('icon32'),
            nd.getAttribute('icon16'),
            core.isIE ? nd.innerText : nd.textContent,
            function (btn) { windowManager.CreateWindow(btn.WindowUrl); },
            null,
            null,
            nd.getAttribute('uid'));

        btn.WindowUrl = nd.getAttribute('url');
        btn.AutoBindWithWindows = true;
    }

    //alert('refresh sytem');
    RefreshSystemMessages();
    buildDesktopContextMenu();
    buildTopToolbar();
};


function unBuildTaskbar() {
    var hd = document.getElementById('hiddenItems');
    //    startMenu.parentNode.removeChild(startMenu);
    //    hd.appendChild(startMenu);
    windowManager.Desktop.Taskbar.ClearButtons();
    projectContextMenu.Hide();
    windowManager.UnBindMouseContextMenuToObject(windowManager.Desktop.Background);
    projectContextMenu.Clear();
};

function searchBoxFocus(sender) {
    if (sender.value == sender.orgValue) sender.value = '';
    sender.className = 'StartmenuSearchFocus';
    sender.select();

};

function searchBoxBlur(sender) {
    if (sender.value == '') {
        sender.value = sender.orgValue;
        sender.className = 'StartmenuSearch';
    }
};

function pinToTaskbar(btn, win, index) {
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/UserQuickLink.aspx';
    AjaxObject.encVar('event', 'add');

    AjaxObject.encVar('Icon16', btn.ContextMenuIcon);
    AjaxObject.encVar('Icon32', btn.Icon);
    AjaxObject.encVar('Name', btn.Name);
    AjaxObject.encVar('SortOrderInt', index);
    AjaxObject.encVar('URL', win.Url);

    AjaxObject.OnCompletion = function () { __pinToTaskbar(AjaxObject, btn); };
    AjaxObject.OnError = function () { __pinToTaskbar(AjaxObject, btn); };
    AjaxObject.OnFail = function () { __pinToTaskbar(AjaxObject, btn); };
    AjaxObject.RunAJAX();
};

function __pinToTaskbar(AjaxObject, btn) {
    var result = AjaxObject.Response;
    if (btn) {
        btn.Uniqueidentifier = result;
    }

    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;
};

function unPinTaskbar(btn, win) {
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/UserQuickLink.aspx';
    AjaxObject.encVar('event', 'remove');

    AjaxObject.encVar('id', btn.Uniqueidentifier);

    AjaxObject.OnCompletion = function () { __pinToTaskbar(AjaxObject); };
    AjaxObject.OnError = function () { __pinToTaskbar(AjaxObject); };
    AjaxObject.OnFail = function () { __pinToTaskbar(AjaxObject); };
    AjaxObject.RunAJAX();

};

//New NNE Open Company Method
var nneLoggedInFrame = null;

function GoNNECompanyProfile(id) {

    if (nneLoggedInFrame != null) {
        _GoNNECompanyProfile(id);
    }
    else {
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/NNETracking.aspx';
        AjaxObject.OnCompletion = function () { _NNETrackingDone(AjaxObject); };
        AjaxObject.OnError = function () { _NNETrackingDone(AjaxObject); };
        AjaxObject.OnFail = function () { _NNETrackingDone(AjaxObject); };
        AjaxObject.RunAJAX();

        //NNE Login URL
        var nneUrl = 'http://nnerhverv.escrm.dk';

        //Create a new frame
        if (document.all) {
            nneLoggedInFrame = window.open(nneUrl, 'nneSearch', 'width=1010,height=600,scrollbars=yes,statusbar=yes,toolbar=yes,resizable=yes');

            //Load actual window after a short wait for login to finish
            setTimeout(function () { _GoNNECompanyProfile(id); }, 2500);
        }
        else {
            nneLoggedInFrame = document.createElement('IFRAME');
            nneLoggedInFrame.style.position = 'absolute';
            nneLoggedInFrame.style.zIndex = 100000;
            nneLoggedInFrame.style.left = '0px';
            nneLoggedInFrame.style.top = '0px';
            nneLoggedInFrame.style.width = '1px';
            nneLoggedInFrame.style.height = '1px';
            nneLoggedInFrame.frameBorder = 0;
            nneLoggedInFrame.style.display = '';
            document.body.appendChild(nneLoggedInFrame);
            nneLoggedInFrame.src = nneUrl;

            //Load actual window after a short wait for login to finish
            setTimeout(function () { _GoNNECompanyProfile(id); nneLoggedInFrame.style.display = 'none'; }, 2500);
        }


    }
};

//Opens a new window, displaying NNE Company Profile if possible
function _GoNNECompanyProfile(id) {
    var nneUrl = 'http://erhverv.nnmarkedsdata.dk/Content/View/Profile.aspx?from=search&p=0&id=' + id.toString();
    window.open(nneUrl, 'nneProfile', 'width=1010,height=600,scrollbars=yes,statusbar=yes,toolbar=yes,resizable=yes');
    return;

    var frm = document.createElement('FORM');

    frm.action = nneUrl; //core.WebsiteUrl.replace('https://','http://') + 'gotoNNE.aspx';'http://www.erhverv.nnmarkedsdata.dk/Content/View/Profile.aspx?from=search&p=0&id=101923580'; //
    frm.method = 'post';
    frm.style.position = 'absolute';
    frm.style.left = -2500;
    frm.style.top = -2500;
    frm.target = '_blank';

    document.body.appendChild(frm);

    frm.submit();

    document.body.removeChild(frm);
    frm = null;

}

function _NNETrackingDone(AjaxObject) {
    if (AjaxObject) {
        var result = AjaxObject.Response;
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;
    }
}

//Open NNE and show Search Window
function GoNNE() {
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/NNETracking.aspx';
    AjaxObject.OnCompletion = function () { _GoNNE(AjaxObject); };
    AjaxObject.OnError = function () { _GoNNE(AjaxObject); };
    AjaxObject.OnFail = function () { _GoNNE(AjaxObject); };
    AjaxObject.RunAJAX();
};

function _GoNNE(AjaxObject) {
    if (AjaxObject) {
        var result = AjaxObject.Response;
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;
    }

    var nneUrl = 'http://nnerhverv.escrm.dk';
    window.open(nneUrl, 'nneSearch', 'width=1010,height=600,scrollbars=yes,statusbar=yes,toolbar=yes,resizable=yes');
    return;

    if (!core.IsIE) {
        window.open(nneUrl); //'http://nnerhverv.escrm.dk');
        return;
    }

    var frm = document.createElement('FORM');

    frm.action = 'http://nnerhverv.escrm.dk'; //core.WebsiteUrl.replace('https://','http://') + 'gotoNNE.aspx';'http://www.erhverv.nnmarkedsdata.dk/Content/View/Profile.aspx?from=search&p=0&id=101923580'; //
    frm.method = 'post';
    frm.style.position = 'absolute';
    frm.style.left = -2500;
    frm.style.top = -2500;
    frm.target = '_blank';

    document.body.appendChild(frm);

    frm.submit();

    document.body.removeChild(frm);
    frm = null;

};

function GoNNEField(name, value, p) {
    var un = document.createElement('INPUT');
    un.type = 'hidden';
    un.id = name;
    un.value = value;
    un.name = name;
    alert('name=' + un.name);
    p.appendChild(un);
};

//Get new System Messages (on going)
function RefreshSystemMessages() {
    var ud = document.getElementById('spCurrentUser');
    var uid = ud.getAttribute('uid');

    //Abort after a logout
    if (uid == '' || uid == null) return;


    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/SystemMessages.aspx';

    AjaxObject.OnCompletion = function () { _RefreshSystemMessages(AjaxObject); };
    AjaxObject.OnError = function () { _RefreshSystemMessages(AjaxObject); };
    AjaxObject.OnFail = function () { _RefreshSystemMessages(AjaxObject); };
    AjaxObject.RunAJAX();
};


function _RefreshSystemMessages(AjaxObject) {
    //Get result and clean up
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    //parse result
    if (result != '') {
        var div = document.createElement('DIV');
        div.innerHTML = result;



        while (div.hasChildNodes()) {

            var o = div.removeChild(div.firstChild);

            if (o.getAttribute('type') == 'chat') {
                var nw = windowManager.GetWindowByName(o.innerHTML);
                if (!nw) {
                    nw = windowManager.CreateWindow('OnlineUsersChat.aspx?id=' + o.getAttribute('uid'));
                    nw.Name = o.innerHTML;
                }
            }
            else if (o.getAttribute('type') == 'note') {
                addNoteReminder(o);
            }
            else if (o.getAttribute('type') == 'avnnote') {
                addNoteReminder(o);
            }
            else if (o.getAttribute('type') == 'error') {
                if (o.innerHTML.indexOf('NNESearcher.cs') == -1 && o.innerHTML.indexOf('SMVInstantSearch.aspx') == -1)
                    windowManager.Alert(o.innerHTML, 'Der er desværre opstået en fejl!');
            }
        }
    }

    //Redo in 3 seconds
    setTimeout(function () { RefreshSystemMessages(); }, 300000);
};

var noteReminder = function (obj) {

};

function addNoteReminder(obj) {
    //make sure the note isn't already added
    for (var i = 0; i < noteReminderButton.Reminders.length; i++) {
        if (noteReminderButton.Reminders[i].getAttribute('uid') == obj.getAttribute('uid')) {
            return;
        }
    }

    var delay = parseInt(obj.getAttribute('alertInSeconds'), 10);

    noteReminderButton.Reminders.push(obj);

    if (delay <= 0) {
        ShowReminderWindow();
        noteReminderButton.style.display = '';
    }
    else {
        //Less than 1 hour away, prepare an alert else dump the alert, and try again in a while..

        if (delay < 3600) {
            noteReminderButton.Reminders[noteReminderButton.Reminders.length - 1].TimerId = setTimeout(function () { ShowReminderWindow(); }, delay * 1000);
        }
    }


};

function dismissReminder(id) {
    for (var i = noteReminderButton.Reminders.length - 1; i >= 0; i--) {
        if (noteReminderButton.Reminders[i] == id) {
            clearTimeout(noteReminderButton.Reminders[i].TimerId);
            noteReminderButton.Reminders.splice(i, 1);
        }
    }
    noteReminderButton.style.display = 'none';

};

function ShowReminderWindow() {
    if (!noteReminderWindow) {
        noteReminderWindow = windowManager.CreateWindow('NotesReminder.aspx');
        noteReminderWindow.OnClose = function () { noteReminderWindow = null; };
    }
};

//** Tilgøjet en ny funktion, ESCRM-134/135
function ShowTimeGodkendelseWindow() {
    if (!noteTimegodkendelseWindow) {
        noteTimegodkendelseWindow = windowManager.CreateWindow('timeApproval.aspx?type=controller');
        noteTimegodkendelseWindow.OnClose = function () { noteTimegodkendelseWindow = null; };
    }
};

function buildDesktopContextMenu() {
    projectContextMenu = windowManager.CreateContextMenu();
    var smv = projectContextMenu.AddItem('SMV & POT kontakter', 'images/smv.png', null);
    var sam = projectContextMenu.AddItem('SAM kontakter', 'images/sam.png', null);
    projectContextMenu.AddSeparator();
    var hour = projectContextMenu.AddItem('Timeregistreringer', 'images/hour.png', null);
    var driv = projectContextMenu.AddItem('Kørselsregistreringer', 'images/drivingRegistration.png', null);

    smv.AddItem('Opret ny SMV kontakt', 'images/smvNew.png', function () { windowManager.CreateWindow('SMVContactsEdit.aspx?type=SMV'); });

    smv.AddItem('Opret ny POT kontakt', 'images/potNew.png', function () { windowManager.CreateWindow('SMVContactsEdit.aspx?type=POT'); });
    smv.AddSeparator();
    smv.AddItem('Søg efter SMV/POT', 'images/smv.png', function () { windowManager.CreateWindow('SMVContacts.aspx'); });

    sam.AddItem('Opret ny SAM kontakt', 'images/samNew.png', function () { windowManager.CreateWindow('SAMContactsEdit.aspx'); });
    sam.AddSeparator();
    sam.AddItem('Søg efter Ny SMV/POT', 'images/sam.png', function () { windowManager.CreateWindow('SAMContacts.aspx'); });

    hour.AddItem('Åbn ugeoversigt', 'images/hour.png', function () { windowManager.CreateWindow('hourUsage.aspx'); });
    hour.AddSeparator();
    hour.AddItem('Ny tidsregistrering for en uge', 'images/hourNew.png', function () { windowManager.CreateWindow('hourUsageEdit.aspx'); });

    driv.AddItem('Åbn kørselsregistreringer', 'images/drivingRegistration.png', function () { windowManager.CreateWindow('drivingRegistrations.aspx'); });
    driv.AddSeparator();
    driv.AddItem('Ny kørselsregistrering', 'images/drivingRegistrationNew.png', function () { windowManager.CreateWindow('drivingRegistrationsEdit.aspx'); });

    projectContextMenu.AddSeparator();
    projectContextMenu.AddItem('Personlige indstillinger', 'images/user.png', function () { windowManager.CreateWindow('OrganisationsEditUser.aspx?id=' + document.getElementById('spCurrentUser').getAttribute('uid') + '&isPersonal=true'); });
    projectContextMenu.AddItem('Skift password', 'images/passwordExpires.png', function () { windowManager.CreateWindow('userPasswordChange.aspx'); });
    projectContextMenu.AddSeparator();
    projectContextMenu.AddItem('Gadgets i højre side', 'images/gadget.png', function () { windowManager.CreateWindow('widgetBrowser.aspx'); });
    projectContextMenu.AddItem('Udseende og funktioner', 'images/design.png', function () { windowManager.CreateWindow('userPersonlization.aspx'); });

    windowManager.BindMouseContextMenuToObject(windowManager.Desktop.Background, projectContextMenu);


    //tasbar context menu
    taskbarContextMenu = windowManager.CreateContextMenu();
    var buttonLayout = taskbarContextMenu.AddItem('Gruppering', 'images/taskbar.png', null);
    taskbarContextMenu.AddSeparator();
    var taskbarMenus = taskbarContextMenu.AddItem('Menuer', 'images/menuLayout.png', null);

    buttonLayout.AddItem('Grupper automatisk', '', function () { crm_setTaskbarGrouping('1'); windowManager.Desktop.Taskbar.SetAutogroupButtons(true); });
    buttonLayout.AddItem('Ingen gruppering', '', function () { crm_setTaskbarGrouping('0'); windowManager.Desktop.Taskbar.SetAutogroupButtons(false); });

    taskbarMenus.AddItem('Top- og bundmenu', '', function () { crm_setMenus('1'); document.getElementById('trTopToolbar').style.display = ''; });
    taskbarMenus.AddItem('Kun menu i bunden', '', function () { crm_setMenus('0'); document.getElementById('trTopToolbar').style.display = 'none'; });



    windowManager.BindMouseContextMenuToObject(windowManager.Desktop.Taskbar.Control, taskbarContextMenu);
};

function crm_setLayout(value) {
    var AjaxObject = new windowManager.Core.Ajax();
    AjaxObject.requestFile = 'ajax/userPersonlization.aspx';
    AjaxObject.encVar('backgroundStyle', value);
    AjaxObject.encVar('saveBackgroundStyle', 'true');

    AjaxObject.OnCompletion = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnError = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnFail = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.RunAJAX();
};

function __crm_savePersonlization(AjaxObject) {
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;
};

function crm_setMenus(value) {
    var AjaxObject = new windowManager.Core.Ajax();
    AjaxObject.requestFile = 'ajax/userPersonlization.aspx';
    AjaxObject.encVar('useTopMenu', value);
    AjaxObject.encVar('saveTopMenu', 'true');

    AjaxObject.OnCompletion = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnError = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnFail = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.RunAJAX();
};

function crm_setTaskbarGrouping(value) {
    var AjaxObject = new windowManager.Core.Ajax();
    AjaxObject.requestFile = 'ajax/setUserProperty.aspx';
    AjaxObject.encVar('property', 'TaskbarGrouping');
    AjaxObject.encVar('value', value);

    AjaxObject.OnCompletion = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnError = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.OnFail = function () { __crm_savePersonlization(AjaxObject); };
    AjaxObject.RunAJAX();
};


var admDesign = null, admProjects = null, admItemNos = null, admFinancialPools = null, admUnits = null;
var trash = null;
var state1 = null;
var state2 = null;
var state3 = null;
var ebstadm = null;
var adm = null;
var stat = null;
var ew = null;
var funx = null;
var links = null;

var adms = null;
var stats = null;
var ews = null;

function buildTopToolbar() {
    var ud = document.getElementById('spCurrentUser');
    var useTop = ud.getAttribute('useTopMenu');
    var role = parseInt(ud.getAttribute('role'));

    if (!topToolbar) {
        var obj = document.getElementById('toolbarTop');
        topToolbar = new Toolbar(document, obj);

        var cmdStart = topToolbar.AddButton('CRM Portal', 'images/Login.png', function () { topToolbarContextMenu.Show(this.Control.offsetLeft, this.Control.offsetTop + this.Control.offsetHeight); });
        topToolbar.AddSeparator();
        topToolbar.AddButton('SMV& POT kontaktpersoner', 'images/SMV.png', function () { windowManager.CreateWindow('SMVContacts.aspx'); });
        topToolbar.AddButton('Projektnoter', 'images/avnNoteList.png', function () { windowManager.CreateWindow('Statistics_AVNCategories.aspx'); });
        topToolbar.AddButton('SAM kontaktpersoner', 'images/SAM.png', function () { windowManager.CreateWindow('SAMContacts.aspx'); });
        topToolbar.AddSeparator();
        topToolbar.AddButton('Kalender', 'images/calendar.png', function () { windowManager.CreateWindow('ExchangeCalendar.aspx'); });
        topToolbar.AddSeparator();
        topToolbar.AddButton('Interessegrupper', 'images/mailgroup.png', function () { windowManager.CreateWindow('mailgroups.aspx'); });
        topToolbar.AddSeparator();
        topToolbar.AddButton('Timeregistreringer', 'images/hour.png', function () { windowManager.CreateWindow('hourUsage.aspx'); });
        topToolbar.AddButton('Kørselsregnskaber', 'images/drivingRegistration.png', function () { windowManager.CreateWindow('drivingRegistrations.aspx'); });
        topToolbar.AddBlankSeperator();
        topToolbar.AddButton('Support', 'images/question.png', function () {
            window.open('http://blog.escrm.dk/guides/');
            //support();
        });
        //topToolbar.AddButton('Feedback', 'images/feedback.png', function () { windowManager.CreateWindow('ZenDeskFeedBack.aspx'); });

        topToolbarContextMenu = windowManager.CreateContextMenu(windowManager.Core);

        topToolbarContextMenu.AddItem('SMV& POT kontaktpersoner', 'images/smv.png', function () { windowManager.CreateWindow('SMVContacts.aspx'); });
        topToolbarContextMenu.AddItem('Projektnoter', 'images/avnNoteList.png', function () { windowManager.CreateWindow('Statistics_AVNCategories.aspx'); });
        topToolbarContextMenu.AddItem('SAM kontaktpersoner', 'images/sam.png', function () { windowManager.CreateWindow('SAMContacts.aspx'); });
        topToolbarContextMenu.AddItem('Kalender', 'images/calendar.png', function () { windowManager.CreateWindow('ExchangeCalendar.aspx'); });
        topToolbarContextMenu.AddItem('Interessegrupper', 'images/mailgroup.png', function () { windowManager.CreateWindow('mailgroups.aspx'); });
        topToolbarContextMenu.AddItem('Timeregistreringer', 'images/hour.png', function () { windowManager.CreateWindow('hourUsage.aspx'); });
        topToolbarContextMenu.AddItem('Kørselsregnskaber', 'images/drivingRegistration.png', function () { windowManager.CreateWindow('drivingRegistrations.aspx'); });
        topToolbarContextMenu.AddSeparator();
        links = topToolbarContextMenu.AddItem('Links', 'images/externalLinks.png');
        topToolbarContextMenu.AddSeparator();
        funx = topToolbarContextMenu.AddItem('Mine funktioner', 'images/user.png');
        ews = topToolbarContextMenu.AddSeparator();
        ew = topToolbarContextMenu.AddItem('Early warning', 'images/earlyWarning.png');
        stats = topToolbarContextMenu.AddSeparator();
        stat = topToolbarContextMenu.AddItem('Statistikker', 'images/statistics.png');
        adms = topToolbarContextMenu.AddSeparator();
        adm = topToolbarContextMenu.AddItem('Administration', 'images/admin.png');
        ebstadm = topToolbarContextMenu.AddItem('EBST Administration', 'images/globalAdmin.png');

        links.AddItem('NNErhverv', 'images/nne.gif', function () { GoNNE(); });
        links.AddItem('Brugerevaluering', 'images/userEvaluation.png', function () { window.open('http://www.brugerevaluering.dk'); });
        links.AddItem('CRM Blog', 'images/blog.png', function () { window.open('http://blog.escrm.dk'); });
        links.AddItem('Support', 'images/question.png', function () {
            window.open('http://blog.escrm.dk/guides/');
        });
        links.AddItem('Outlook Integration', 'images/outlook16.png', function () { window.open('https://www.escrm.dk/downloads/Outlook2CRM.msi'); });

        funx.AddItem('Personlige indstillinger', 'images/user.png', function () { windowManager.CreateWindow('OrganisationsEditUser.aspx?id=' + document.getElementById('spCurrentUser').getAttribute('uid') + '&isPersonal=true'); });

        funx.AddItem('Påmindelser', 'images/note.png', function () { ShowReminderWindow(); });

        //** Tilføjet menu Timegodkendelse, ESCRM-134/135
        if (ud.getAttribute('AllowHourlyTimesheet') == "true") {
            funx.AddItem('Timegodkendelse', 'images/hour.png', function () { ShowTimeGodkendelseWindow(); });
        }

        funx.AddItem('Beskyt kontakter', 'images/checkNames.png', function () { windowManager.CreateWindow('Statistics_ImportantFields.aspx'); });

        funx.AddItem('Skift password', 'images/passwordExpires.png', function () { windowManager.CreateWindow('userPasswordChange.aspx'); });

        funx.AddItem('Exchange indstillinger', 'images/sync.png', function () { windowManager.CreateWindow('ExchangeSettings.aspx'); });
        funx.AddItem('Batch opdatering', 'images/batch.png', function () { windowManager.CreateWindow('BatchUpdate.aspx'); });
        funx.AddItem('Importér data', 'images/database.png', function () { windowManager.CreateWindow('ImportData.aspx'); });

        funx.AddItem('Udseende og funktioner', 'images/design.png', function () { windowManager.CreateWindow('userPersonlization.aspx'); });
        funx.AddItem('Gadgets i højre side af skærmen', 'images/gadget.png', function () { windowManager.CreateWindow('widgetBrowser.aspx'); });
        funx.AddItem('Velkomstbillede', 'images/login.png', function () { windowManager.CreateWindow('gettingStarted.aspx'); });
        funx.AddItem('Rettighedsmatrix', 'images/security.png', function () { windowManager.CreateWindow('userSecurityMatrix.aspx'); });
        funx.AddItem('Løse kontaktpersoner og virksomheder', 'images/smv.png', function () { windowManager.CreateWindow('SMVPOTLooseItems.aspx'); });
        funx.AddItem('Kontaktaftaler', 'images/file.png', function () { windowManager.CreateWindow('Agreements.aspx'); });

        ew.AddItem('Virksomheder', 'images/ewCompany.png', function () { windowManager.CreateWindow('earlyWarningCompanies.aspx'); });
        ew.AddItem('Frivillige', 'images/ewVolunteer.png', function () { windowManager.CreateWindow('earlyWarningVolunteers.aspx'); });

        stat.AddItem('Brugerevalueringer', 'images/userEvaluation.png', function () { windowManager.CreateWindow('Statistics_UserEvaluations.aspx'); });
        stat.AddItem('Rapportgenerator', 'images/reportGenerator.png', function () { windowManager.CreateWindow('ReportGenerator.aspx'); });

        state1 = stat.AddItem('Eksporter...', 'images/export.png');
        state2 = stat.AddItem('Aktiviteter...', 'images/project.png');
        stat.AddItem('Ressourceforbrug', 'images/resource.png', function () { windowManager.CreateWindow('StatisticsRessourcesReport.aspx'); });
        stat.AddItem('Kontaktpersonstatistik', 'images/smv.png', function () { windowManager.CreateWindow('StatisticsContactsReport.aspx'); });
        state3 = stat.AddItem('Vejledninger...', '');

        state1.AddItem('Eksporter timeregnskaber', 'images/hour.png', function () { windowManager.CreateWindow('hourUsageExport.aspx'); });
        state1.AddItem('Eksporter kørselsregnskaber', 'images/drivingRegistration.png', function () { windowManager.CreateWindow('drivingRegistrationsExport.aspx'); });

        state2.AddItem('Antal vejledningsmøder', 'images/calendar.png', function () { windowManager.CreateWindow('StatisticsProjectsReport.aspx'); });
        state2.AddItem('Vejledte kontaktpersoner', 'images/smv.png', function () { windowManager.CreateWindow('StatisticsProjectsReport.aspx?type=contacts'); });

        state3.AddItem('Mødelængder', 'images/calendar.png', function () { windowManager.CreateWindow('StatisticsMeetingsReport.aspx'); });
        state3.AddItem('Mødeaktiviteter', 'images/project.png', function () { windowManager.CreateWindow('StatisticsMeetingsReport.aspx?type=projects'); });

        adm.AddItem('Timeregnskaber', 'images/hour.png', function () { windowManager.CreateWindow('adminHourUsage.aspx'); });
        adm.AddItem('Kørselsregnskaber', 'images/drivingRegistration.png', function () { windowManager.CreateWindow('adminDrivingRegistration.aspx'); });
        adm.AddItem('Opsæt notekategorier', 'images/noteCategories.png', function () { windowManager.CreateWindow('NoteCategories.aspx'); });

        adm.AddItem('Projektnoter', 'images/anvNote.png', function () { windowManager.CreateWindow('adminAVNs.aspx'); });
        adm.AddItem('Opsæt Projektnote kategorier', 'images/anvNote.png', function () { windowManager.CreateWindow('AdminAvnNoteCategories.aspx'); });


        adm.AddItem('Opsæt dokumentkategorier', 'images/file.png', function () { windowManager.CreateWindow('FileCategories.aspx'); });

        admProjects = adm.AddItem('Opsæt aktiviteter', 'images/project.png', function () { windowManager.CreateWindow('SecondaryProjects.aspx'); });
        admItemNos = adm.AddItem('Opsæt varenumre', 'images/itemNumber.png', function () { windowManager.CreateWindow('ItemNumbers.aspx'); });
        admFinancialPools = adm.AddItem('Finansieringspuljer', 'images/money.png', function () { windowManager.CreateWindow('FinalcialPools.aspx'); });
        admUnits = adm.AddItem('Enheder', 'images/organisation.png', function () { windowManager.CreateWindow('organisations.aspx'); });
        trash = adm.AddItem('Skraldespand', 'images/trashcan.png', function () { windowManager.CreateWindow('trashcan.aspx'); });
        admDesign = adm.AddItem('Design felter', 'images/design.png', function () { windowManager.CreateWindow('FieldDesigner.aspx'); });
        admDoubles = adm.AddItem('SMV Dublethåndtering', 'images/smv.png', function () { windowManager.CreateWindow('SMVDoublesManager.aspx'); });
        admNews = adm.AddItem('Rediger nyheder', 'images/login.png', function () { windowManager.CreateWindow('OrganisationEditNews.aspx'); });
        admUserGroups = adm.AddItem('Brugergrupper', 'images/UserGroup.png', function () { windowManager.CreateWindow('UserGroups.aspx'); });


        ebstadm.AddItem('Enheder', 'images/organisation.png', function () { windowManager.CreateWindow('organisations.aspx'); });
        ebstadm.AddItem('Kommuner', 'images/county.png', function () { windowManager.CreateWindow('Counties.aspx'); });
        ebstadm.AddItem('Hovedaktiviteter', 'images/project.png', function () { windowManager.CreateWindow('PrimaryProjects.aspx'); });
        ebstadm.AddItem('Emneord & beskrivelser', 'images/keyword.png', function () { windowManager.CreateWindow('DescriptionWords.aspx'); });
        ebstadm.AddItem('Kompetencer', 'images/competence.png', function () { windowManager.CreateWindow('Competences.aspx'); });
        ebstadm.AddItem('Design felter', 'images/design.png', function () { windowManager.CreateWindow('FieldDesigner.aspx'); });
        ebstadm.AddItem('Reparér feltdesign', 'images/database.png', function () { windowManager.CreateWindow('DatabaseTools.aspx'); });
        ebstadm.AddItem('Reparér Projektnoter', 'images/anvNote.png', function () { windowManager.CreateWindow('adminAVNsRepair.aspx'); });
        ebstadm.AddItem('Online brugere', 'images/onlineUser.png', function () { windowManager.CreateWindow('OnlineUsers.aspx'); });

        ebstadm.AddItem('Rediger Evalueringer', 'images/anvNote.png', function () { window.open('/admin/del-projektnoter.aspx', '_blank'); });

        ebstadm.AddItem('Specielle brugergruppe rettigheder', 'images/UserGroup.png', function () { windowManager.CreateWindow('UsergroupCustomRights.aspx'); });
        ebstadm.AddItem('Rapport til SOAP godkendelse', 'images/reportgenerator.png', function () { windowManager.CreateWindow('ReportGeneratorIntegrationsList.aspx'); });


        topToolbarContextMenu.AddSeparator();
        topToolbarContextMenu.AddItem('Log af', '', function () { setTimeout(function () { logoff(); }, 100); });

    }

    var orgType = parseInt(ud.getAttribute('organisationType'));

    if (orgType > 1)
        admDoubles.Hide();
    else
        admDoubles.Show();

    if (role > 0) {
        adm.Show();
        adms.Show();

        if (role == 25) {
            //admProjects.Hide();
            //admItemNos.Hide();
            admFinancialPools.Hide();
            admUnits.Hide();
        }
        else {
            //admProjects.Show();
            //admItemNos.Show();
            admFinancialPools.Show();
            admUnits.Show();
        }

    }
    else {
        adm.Hide();
        adms.Hide();
    }

    if (role != 25) {
        trash.Show();
        admDesign.Show();

        if (orgType < 3)
            admDoubles.Show();
    }
    else {
        trash.Hide();
        admDesign.Hide();
        admDoubles.Hide();
    }

    if (role > 30) {
        ebstadm.Show();
    }
    else {
        ebstadm.Hide();
    }

    if (ud.getAttribute('earlywarning') == 'true' || role > 30) {
        ew.Show();
        ews.Show();
    }
    else {
        ew.Hide();
        ews.Hide();
    }

    document.getElementById('trTopToolbar').style.display = useTop == '1' || useTop == 'true' ? '' : 'none';
};

function support() {
    window.location.href = "mailto:escrm@vhsyd.dk?Subject=Spørgsmål%20om%20ESCRM%20fra%20" + document.getElementById('spCurrentUser').innerText + " (" + document.getElementById('spCurrentUser').getAttribute('UserName') + ") i " + document.getElementById('spCurrentUser').getAttribute('OrganisationName') + " (" + document.getElementById('spCurrentUser').getAttribute('OrganisationId') + ")";
}
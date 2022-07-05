var isDirty = false;
var samlw = null;
var hs = null;
var vs = null;
var tb = null;
var tbSAM = null;
var tbNotes = null;
var tbMailgroups = null;
var tbDocuments = null;
var tbMeetings = null;
var tbEmails = null;
var tbReportedMeetings = null;
var wm = null;
var io = null;
var dd = null;
var tabs = null;
var pb = null;

//** Tilføjet af Jens, ESCRM-13/97
var cmdFavorit = null;

//** ESCRM-111/112/113
var valgtContactId;

var cmdSave = null;
var cmdAddCompany = null;

var cmdEvaluate = null;
var cmdDriftevaluering = null;

var cmdConvert = null;
var cmdDelete = null;
var cmdForward = null;
var cmdPrint = null;
var toolbar = null;
var cmdKill = null;
var cmdGlue = null;
var cmdGoNNE = null;
var cmdWebSite = null;
var cmdFullscreen = null;
var cmdKontraktaftale = null;

var w = null;
var core = null;
var localCore = null;

var liHistory = null;
var tbHistory = null;
var tbAVN = null;
var liAVN = null;

var currentDateObject = null;

var cmdNewAVN = null;
var cmdEditAVN = null;
var cmdDeleteAVN = null;

var listviewMinHeight = 350;

var mapInputFields = new Array();

var listviewStyle = 0;
var windowLastHeight = 0;

var cmdBalanced = null;
var cmdCenter = null;
var cmdFullLog = null;
var cmdFullEdit = null;

var cmdCreateMeeting = null;

var CRM = null;

//CPR array
var cprStatus = [];

//Initialize
function init() {
    //Manage information line size, before initializing window and scrolling regions
    var oo = document.getElementById('spCompanyIsTransferred');
    if (oo.scrollHeight > 20) {
        var od = document.getElementById('spCompanyIsTransferredFull');
        oo.parentNode.className = 'windowInlineHeaderDown';
        od.className = 'windowInlineHeaderUp';
    }


    //Create window and set properties
    w = window.frameElement.IDCWindow;

    //Create CRM API object
    CRM = new CRM_API(document.getElementById('txtUserData').value.split('\t'), w);

    w.UseDirtyManager = true;
    w.DirtyMessage = 'Der er lavet ændringer på stamkortet.<br><br>Er du sikker på, at du vil lukke vinduet uden at gemme?';
    w.DirtyTitle = 'Luk uden at gemme?';

    core = w.WindowManager.Core;
    localCore = new IDC_Core(document, document.body);
    wm = w.WindowManager;


    datePicker = new DatePicker(document, parent.document.body, top.window);
    datePicker.OnPickDate = OnDatePicked;


    w.Icons = ['images/smv.png', 'images/smv32.png'];
    w.Name = 'Rediger SMV';
    w.Width = contactFields.scrollWidth + tdTreeview.offsetWidth + 16;

    if (w.Width > w.WindowManager.Desktop.Background.offsetWidth - 40) w.Width = w.WindowManager.Desktop.Background.offsetWidth - 40;
    if (w.Width < 640) w.Width = 640;

    w.Height = w.WindowManager.Desktop.Background.offsetHeight * 0.66;
    w.CenterScreen();
    w.OnResized = OnResize;
    w.OnMaximize = OnResize;
    w.OnRestore = OnResize;

    w.MinWidth = 640;
    w.MinHeight = 490;

    toolbar = document.getElementById('toolbar');
    tb = new Toolbar(document, toolbar);

    tw = new IDC_Treeview(document, document.getElementById('treeview'));

    //Set as Maximized
    w.WindowState = 2;

    //Show window
    w.Show();

    //If something went wrong - close the window now!
    if (document.getElementById('closeNow').value != '') {
        w.Alert(closeNow.value, 'Kan ikke åbne', null, null, null);
        w.Close();
        return;
    }

    //Toolbar AVN
    tbAVN = new Toolbar(document, document.getElementById('toolbarAvn'));
    cmdNewAVN = tbAVN.AddButton('Ny', 'images/anvNoteNew.png', function () { createNewAVN(); });
    cmdEditAVN = tbAVN.AddButton('Åbn', 'images/edit.png', function () { AVNEdit(); });
    tbAVN.AddSeperator();
    cmdDeleteAVN = tbAVN.AddButton('Slet', 'images/delete.png', function () { AVNDelete(); });

    //Listview AVN
    liAVN = new IDC_Listview(document, document.getElementById('listviewAvn'), '199', '200', localCore); //Listview(document, document.getElementById('SAMlistview'), document.getElementById('SAMcolumnheaders'));
    liAVN.ViewStyles.push(IDC_ListviewItemAVNStyle);
    liAVN.AutomaticDeselectItemOnClick = true;
    liAVN.MultiSelect = false;
    liAVN.HideColumnHeaders();
    liAVN.OnDoubleClick = AVNEdit;
    liAVN.ChangeView(5);
    liAVN.Container.style.backgroundColor = '#eaf0f6';
    liAVN.AddColumnHeader('Titel', 190);

    cmdEditAVN.SetEnabledState(liAVN.SelectedItems.length == 1);
    cmdDeleteAVN.SetEnabledState(liAVN.SelectedItems.length == 1);

    liAVN.OnSelect = function () {
        cmdEditAVN.SetEnabledState(liAVN.SelectedItems.length == 1);
        cmdDeleteAVN.SetEnabledState(liAVN.SelectedItems.length == 1);
    };

    //Toolbar
    //cmdSave = tb.AddButton('Gem stamdata', 'images/save.gif', function () { Save(); });
    //** Har lavet en ny knap, ESCRM-190(191)
    cmdSave = tb.AddButton('Gem stamdata', 'images/save.gif', function () { Save(); });

    //** Sæt favorit knap i toolbar, ESCRM-13/97
    cmdFavorit = tb.AddButton('Sæt som favorit', 'images/favoriteSMVOff.png', function () { SetFavorite(); });

    tb.AddSeperator();
    var tbNewContact = tb.AddButton('Ny kontakt', 'images/samNew.png', function () { addContact(); });
    cmdAddCompany = tb.AddButton('Ny afdeling', 'images/smvNew.png', function () { addCompany(); });

    if (document.getElementById('isAdmin').value == '1') {
        tb.AddSeperator();
        cmdDelete = tb.AddButton('Slet', 'images/delete.png', function () { deleteNode(); });
    }

    tb.AddSeperator();
    cmdKill = tb.AddTinyButton('Nedlæg', 'images/dead_smv.png', function () { abandonNode(); }, true);

    tb.AddSeperator();
    tb.AddTinyButton('Send email', 'images/newEmail.png', function () { sendEmail(); });
    cmdWebSite = tb.AddTinyButton('Åbn webside', 'images/webpage.png', function () { openWebSite(); });

    //Open NNE company reference
    cmdGoNNE = tb.AddTinyButton('Åbn NNE', 'images/nne.gif', function () { openNNE(); });

    tb.AddSeperator();
    cmdEvaluate = tb.AddButton('Vækstevaluering', 'images/userEvaluation.png', function () { evaluateUser('vaekst'); });
    cmdDriftevaluering = tb.AddButton('Driftevaluering', 'images/userEvaluation.png', function () { evaluateUser('drift'); });

    tb.AddSeperator();
    cmdConvert = tb.AddButton('Konverter til SMV', 'images/convertToSMV.png', function () { convertToSMV(); });

    tb.AddSeperator();
    cmdForward = tb.AddButton('Anvis', 'images/transfer.png', function () { forwardCompany(); });

    tb.AddSeperator();
    tb.AddButton('Udskriv', 'images/print.png', function () { printCompanyAndContact(); });

    //Set Toolbar as Context Menu
    w.ToolbarContextMenu = tb;

    if (document.getElementById('isAdmin').value == '1') {
        tb.AddSeperator();
        cmdGlue = tb.AddTinyButton('Smelt sammen', 'images/glue.png', function () { glueContacts(); });
    }

    //    tb.AddSeperator();
    //    tb.AddButton('Åbn sti', 'images/print.png', function () { window.open('file://D:\\Photos\\map_preview.jpg'); });

    tb.AddSeperator();
    tb.AddTinyButton('Vis direkte link', 'images/link16.png', function () { LinkToThis(this.Control, getSMVurl()); });


    if (document.getElementById('txtAllowContactAgreement').value == "1") {
        tb.AddSeperator();
        cmdKontraktaftale = tb.AddButton('Se kontraktaftale', 'images/competence.png', function () { ShowAgreement(); });
        cmdKontraktaftale.Hide();
    }

    //History listview & toolbar
    liHistory = new IDC_Listview(document, document.getElementById('divContactHistoryListview'), '100', '100', localCore); //Listview(document, document.getElementById('SAMlistview'), document.getElementById('SAMcolumnheaders'));
    liHistory.EnableColumnFilters = true;
    liHistory.MultiSelect = false;
    //samlw.Keyboard = parent.io.keyboard;
    liHistory.AddColumnHeader('Type', 80);
    var colDate = liHistory.AddColumnHeader('Dato', 130, null, 'date', null, 'dd-MM-yyyy HH:mm');
    liHistory.AddColumnHeader('Titel', 160);
    liHistory.AddColumnHeader('Gemt på', 120);
    liHistory.AddColumnHeader('Bruger', 120);
    liHistory.AddColumnHeader('Deling', 160);//90); //** Har sat bredden til det samme som titel, ESCRM-11/141/94/138/142
    liHistory.AddColumnHeader('Kategori', 160);//90);   //** Har sat bredden til det samme som titel, ESCRM-11/141/94/138/142
    liHistory.AddColumnHeader('Påmindelse', 110, null, 'date', null, 'dd-MM-yyyy HH:mm');
    liHistory.AddColumnHeader('Tidsforbrug', 80, 'right', 'numeric');
    liHistory.AddColumnHeader('Sted', 110);
    liHistory.AddColumnHeader('Pujle', 150);

    /*liHistory.AddColumnHeader('USER ID', 150);
    liHistory.AddColumnHeader('TYPE', 150);
    liHistory.AddColumnHeader('RELATION', 150);
    liHistory.AddColumnHeader('CONTACTID', 150);
    liHistory.AddColumnHeader('ORGID', 150);*/

    liHistory.OnDoubleClick = editHistoryItem;
    liHistory.ChangeView(0);

    liHistory.SortItemsBy(colDate);
    liHistory.SortItemsBy(colDate);

    var dp = w.CreateContextMenu(localCore);
    dp.Listview = liHistory;
    dp.AddItem('SAM relation', 'images/samNew.png', function (ct) { createSAM(); });
    dp.AddItem('Note', 'images/noteNew.png', function (ct) { createNote(); });
    dp.AddItem('Dokument', 'images/mediaAddFiles.png', function (ct) { addDocument(); });
    dp.AddItem('Tilknytning til interessegruppe', 'images/mailgroupNew.png', function (ct) { addToMailGroup(); });
    cmdCreateMeeting = dp.AddItem('Nyt møde', 'images/7calendar.png', function (ct) { createMeeting(); });


    tbHistory = new Toolbar(document, document.getElementById('divContactHistoryToolbar'));
    tbHistory.AddDropdownButton('Tilføj', 'images/newAny.png', function () { dp.ShowBelowControl(this.Control); });

    tbHistory.AddButton('Åbn/Rediger', 'images/edit.png', function () { editHistoryItem(); });
    tbHistory.AddSeperator();
    tbHistory.AddButton('Slet', 'images/Delete.png', function () { deleteHistoryItem(); });
    tbHistory.AddSeperator();
    createPrintLWDropDownMenu(tbHistory, null, liHistory);

    var cmdBalanced = null;
    var cmdCenter = null;
    var cmdFullLog = null;
    var cmdFullEdit = null;

    tbHistory.AddSeperator();

    cmdBalanced = tbHistory.AddTinyButton('Balanceret', 'images/balanced.png', function () {
        listviewStyle = 0;
        cmdBalanced.SetSelectedState(true);
        cmdCenter.SetSelectedState(false);
        cmdFullLog.SetSelectedState(false);
        cmdFullEdit.SetSelectedState(false);

        fixLWsAMethod(); // OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    }, true);

    cmdCenter = tbHistory.AddTinyButton('Centeret', 'images/centered.png', function () {
        listviewStyle = 1;
        cmdBalanced.SetSelectedState(false);
        cmdCenter.SetSelectedState(true);
        cmdFullLog.SetSelectedState(false);
        cmdFullEdit.SetSelectedState(false);

        fixLWsAMethod(); //OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    }, true);

    cmdFullLog = tbHistory.AddTinyButton('Stor log', 'images/bigLog.png', function () {
        listviewStyle = 2;
        cmdBalanced.SetSelectedState(false);
        cmdCenter.SetSelectedState(false);
        cmdFullLog.SetSelectedState(true);
        cmdFullEdit.SetSelectedState(false);

        fixLWsAMethod(); // OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    }, true);

    cmdFullEdit = tbHistory.AddTinyButton('Stor redigering', 'images/bigEdit.png', function () {
        listviewStyle = 3;
        cmdBalanced.SetSelectedState(false);
        cmdCenter.SetSelectedState(false);
        cmdFullLog.SetSelectedState(false);
        cmdFullEdit.SetSelectedState(true);

        fixLWsAMethod(); //OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    }, true);

    cmdBalanced.SetSelectedState(true);
    cmdBalanced.Control.parentNode.style.width = '100%';
    cmdBalanced.Control.parentNode.align = 'right';


    //Treeview
    tw.OnNodeSelect = function (node) {
        document.getElementById('txtAutoStore').focus();
        if (node.ParentTreeviewNode) {
            cmdCreateMeeting.Hide();
        }
        else {
            cmdCreateMeeting.Show();
        }
        selectContact(node);
        //cmdNewAVN.SetEnabledState(node.ParentTreeviewNode != null);
    };

    //Parse data to treeview and select correct node
    if (document.getElementById('baseId').value > 0) {
        tw.__ToNodes(document.getElementById('divSAMCTData'), 1, document.getElementById('baseId').value);
    }
    else if (document.getElementById('baseId').value < 0) {
        tw.__ToNodes(document.getElementById('divSAMCTData'), 1);
        tw.SelectNode(tw.Nodes[0].Nodes[0]);
    }
    else {
        tw.__ToNodes(document.getElementById('divSAMCTData'), 1);
        tw.SelectNode(tw.Nodes[0]);
    }

    tbNewContact.Image.src = tw.Nodes[0].Icon.indexOf('pot.png') > -1 || tw.Nodes[0].Icon.indexOf('potNew.png') > -1 ? 'images/potContactNew.png' : 'images/smvContactNew.png';

    //Fix title icon
    if (tw.Nodes[0].Icon.indexOf('pot.png') > -1 || tw.Nodes[0].Icon.indexOf('potNew.png') > -1) {
        w.SetWindowIcons(['images/pot.png', 'images/pot32.png']);
        w.SetTitle(tw.Nodes[0].Uniqueidentifier > 0 ? 'Rediger POT virksomhed (POT)' : 'Ny POT Virksomhed (POT)');
    }
    else {
        w.SetWindowIcons(['images/smv.png', 'images/smv32.png']);
        w.SetTitle(tw.Nodes[0].Uniqueidentifier > 0 ? 'Rediger SMV virksomhed (SMV)' : 'Ny SMV Virksomhed (SMV)');
    }


    //Saving progressbar
    pb = new Progressbar(document, document.getElementById('progressHere'), 266);
    //pb.OnAnimateEvent = function (p) { window.__WINDOW.UI.SetTaskbarProgress(p); };
    pb.OnAnimateDoneEvent = null;

    OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());

    fixInputChanges();
    DataLinkBuild(companyFields);
    DataLinkBuild(contactFields);
    RequeredFieldsBuild();

    //dd.AddDropZone(document.body, 'email', stuffDropped);
    w.OnKeyDown = keyDown;

    //** Disable vækst- og drift evaluering, ESCRM-111/112
    CanEvaluationButtonsBeShown();
};

// NOTE: This function not in use anymore. Just here for reference purpose for other future developers.
// Load company details from CVR
function GetfromCVR() {
    var pnr = $('#pid').val();
    var cvr = $('#cvr').val();
    if (pnr != '') {
        var postdata = [];
        postdata.push({ name: 'action', value: 'getfromcvr' });
        postdata.push({ name: 'pid', value: pnr });
        postdata.push({ name: 'cvr', value: cvr })
        $.post("/ajax/smvEdit.aspx", postdata, function (result) {
            var c = JSON.parse(result);
            $('#z_companies_1_CVR-nummer_1').val(c.CVR);
            $('#z_companies_1_P-Nummer_1').val(c.PNummer);
            $('#z_companies_1_Firmanavn_1').val(c.CompanyName);
            $('#z_companies_1_Gade_1').val(c.Street);
            $('#z_companies_1_By_1').val(c.City);
            $("[id='z_companies_1_Post nr._1']").val(c.Zipcode);
        });
    }
}

function getSMVurl() {
    var args = '';

    if (tw.SelectedNode && tw.SelectedNode.Uniqueidentifier) {

        var dt = tw.SelectedNode.Uniqueidentifier.toString().split('.');
        if (tw.SelectedNode.ParentTreeviewNode) {
            args = 'id=' + dt[0];
        }
        else {
            args = 'companyId=' + dt[0];
        }
    }
    return '/SMVContactsEdit.aspx?' + args;
};

function keyDown(key, target, e) {
    if (key == 27) {
        w.Close();
        return true;
    }

    if (w.Keyboard.KeyControlPressed && (key == 83 || key == 115)) {
        save();
        e.returnValue = false;
        e.cancelBubble = true;
        return false;
    }
};

//Edit/Open history item
function editHistoryItem(node) {
    if (node == null) {
        if (liHistory.SelectedItems.length > 0)
            node = liHistory.SelectedItems[0];
    }

    if (node == null) {
        w.Alert('Vælg et element at åbne/redigere.');
        return;
    }

    switch (node.Items[12]) {
        case 'COMPANYUPDATED':
        case 'COMPANYCREATED':
        case 'CONTACTUPDATED':
        case 'CONTACTCREATED':
            w.Alert('Status beskeder kan ikke åbnes!', 'Kan ikke åbne!');
            break;

        case 'CONTACTTOEVALUATION':
            w.Alert('Evalueringer kan ikke åbnes!', 'Kan ikke åbne!');
            break;

        case 'CONTACTTRANSFERRED':
        case 'COMPANYTRANSFERRED':
            w.Alert('Anvisninger kan ikke åbnes!', 'Kan ikke åbne!');
            break;

        case 'SAM':
            var nw = w.CreateWindow('SAMContactsEdit.aspx?id=' + node.Uniqueidentifier);
            nw.Variables = node;
            break;

        case 'Tilskudsinformation': /* ESCRM-9/41 */
            var nw = w.CreateWindow('TilskudsinformationShow.aspx?id=' + node.Uniqueidentifier + '&data=' + node.Items[10]);
            //nw.Variables = node;
            break;

        case 'NOTE':
            var nw = w.CreateWindow('SMVNote.aspx?id=' + node.Uniqueidentifier, w);
            nw.Variables = node;
            nw.OnClose = _createNote;
            break;

        case 'FILE':
            var nw = w.CreateWindow('MediaUploadNewFile.aspx?fileId=' + node.Uniqueidentifier, w);
            nw.Variables = node;
            nw.OnClose = _editDocument;
            //window.open('mediaGetFile.aspx?fileId=' + node.Uniqueidentifier);
            break;

        case 'MAILGROUP':
            //var nw = w.CreateWindow('mailgroupEdit.aspx?Id=' + node.Uniqueidentifier, w);
            w.Alert('Tilknytninger til interessegrupper kan ikke åbnes');
            break;

        case 'MEETING':
            openMeeting(node);
            break;

        case 'MEETINGREPORTING':
            openReportedMeeting(node);
            break;

    }
}

function deleteHistoryItem(node) {
    if (node == null) {
        if (liHistory.SelectedItems.length > 0)
            node = liHistory.SelectedItems[0];
    }

    if (node == null) {
        w.Alert('Vælg et element at slette.');
        return;
    }

    switch (node.Items[12]) {
        case 'COMPANYUPDATED':
        case 'COMPANYCREATED':
        case 'CONTACTUPDATED':
        case 'CONTACTCREATED':
            w.Alert('Status beskeder kan ikke slettes!', 'Kan ikke slette!');
            break;

        case 'CONTACTTOEVALUATION':
            w.Alert('Evalueringer kan ikke slettes!', 'Kan ikke slette!');
            break;

        case 'CONTACTTRANSFERRED':
        case 'COMPANYTRANSFERRED':
            w.Alert('Anvisninger kan ikke slettes!', 'Kan ikke slette!');
            break;


        case 'SAM':
            deleteSAM();
            break;

        case 'NOTE':
            deleteNote();
            break;

        case 'FILE':
            deleteDocuments();
            break;

        case 'MAILGROUP':
            deleteFromMailgroup();
            break;

        case 'MEETING':
            deleteMeetings(node);
            break;

        case 'MEETINGREPORTING':
            deleteMeetingReporting(node);
            break;
    }
}

//Forward company to another organisation
function getChosenCompanyNode() {
    if (tw.SelectedNode.ParentTreeviewNode)
        return tw.SelectedNode.ParentTreeviewNode;
    else
        return tw.SelectedNode;
};

function forwardCompany() {
    var node = tw.SelectedNode;
    if (node != null) {
        if (node.Uniqueidentifier != null && node.Uniqueidentifier != '') {
            if (node.ParentTreeviewNode != null) {
                var nw = w.CreateWindow('SMVContactsTransfer.aspx?ids=' + node.Uniqueidentifier, w);
                nw.Variables = null;
                nw.OnClose = __forwardCompany;
            }
            else {
                w.Alert('Virksomheder kan ikke anvises!<br><br>Vælg en kontaktperson i stedet.', 'Kan ikke anvise en virksonhed!', null, null, null);
            }
        }
        else
            w.Alert('Du skal gemme, før kontaktpersonen kan anvises til en anden enhed eller konsulent', 'Kan ikke anvise kontaktperson!', null, null, null);
    }
};

function __forwardCompany(sender, retval) {
};

function printCompanyAndContact() {
    var node = getChosenCompanyNode();
    if (node.Uniqueidentifier != null && node.Uniqueidentifier != '') {
        var nw = w.CreateWindow('smvPrint.aspx?id=' + node.Uniqueidentifier + '&type=contact', w);
        var vars = new Array();
        vars.push(node.Icon.indexOf('smv') > -1 ? 'SMV' : 'POT');
        vars.push(node.Uniqueidentifier);

        if (tw.SelectedNode.ParentTreeviewNode)
            vars.push(tw.SelectedNode.Uniqueidentifier);
        else
            vars.push(node.Nodes[0].Uniqueidentifier);

        nw.Variables = vars;
    }
    else
        w.Alert('Du skal gemme, før virksomheden og eventuelle kontaktpersoner kan udskrives', 'Kan ikke udskrive!', null, null, null);
};

//find website fields and read its content
function openWebSite() {
    var ips = document.getElementById(tw.SelectedNode.ParentTreeviewNode ? 'contactFields' : 'companyFields').getElementsByTagName('INPUT');
    for (var i = 0; i < ips.length; i++) {
        var ff = ips[i];
        if (ff.parentNode.parentNode.parentNode.parentNode) {
            if (ff.parentNode.parentNode.parentNode.parentNode.getAttribute('DataLink') == 'NNECompanyWeb') {
                if (ff.value != '') {
                    var url = ff.value.indexOf('http://') > -1 || ff.value.indexOf('https://') > -1 ? ff.value : 'http://' + ff.value;
                    window.open(url);
                    return;
                }
                else {
                    w.Alert('Det valgte stamkort har ikke en webadresse defineret.', 'Kan ikke åbne website!', null, null, null);
                    return;
                }
                break;
            }
        }
    }

    w.Alert('Det valgte stamkort har ikke en webadresse defineret.', 'Kan ikke åbne website!', null, null, null);
};

function sendEmail() {
    var ips = document.getElementById(tw.SelectedNode.ParentTreeviewNode ? 'contactFields' : 'companyFields').getElementsByTagName('INPUT');
    for (var i = 0; i < ips.length; i++) {
        var ff = ips[i];
        if (ff.parentNode.parentNode.parentNode.parentNode) {
            if (ff.parentNode.parentNode.parentNode.parentNode.getAttribute('DataLink') == 'NNECompanyEmail' || ff.parentNode.parentNode.parentNode.parentNode.getAttribute('DataLink') == 'DGSEmail') {
                if (ff.value != '') {
                    var url = 'mailto:' + ff.value;
                    window.open(url);
                    return;
                }
                else {
                    w.Alert('Det valgte stamkort har ikke en emailadresse defineret.', 'Kan ikke sende en email!', null, null, null);
                    return;
                }
                break;
            }
        }
    }
    w.Alert('Det valgte stamkort har ikke en emailadresse defineret.', 'Kan ikke sende en email!', null, null, null);
};

//Delete contact /company
function abandonNode(cmd) {
    var items = new Array();

    var goCompany = false;
    var children = getChosenCompanyNode().Nodes.length;

    if (tw.SelectedNode.ParentTreeviewNode) {
        var alreadyDead = 0;
        for (var i = 0; i < tw.SelectedNode.ParentTreeviewNode.Nodes.length; i++) {
            if (tw.SelectedNode.ParentTreeviewNode.Nodes[i].Icon.indexOf('dead_') > -1)
                alreadyDead++;
        }

        if (cmdKill.IsSelected && children - alreadyDead == 1)
            goCompany = true;
    }
    else
        goCompany = true;

    if (!goCompany) {
        items.push(tw.SelectedNode.Uniqueidentifier);

        var nw = w.CreateWindow('Delete.aspx?type=smvcontact' + (!cmdKill.IsSelected ? 'un' : '') + 'abandon', w);
        nw.Variables = items;
        nw.OnClose = _abandonContact;
    }
    else {
        var node = getChosenCompanyNode();
        items.push(node.Uniqueidentifier);

        var nw = w.CreateWindow('Delete.aspx?type=smvcompany' + (!cmdKill.IsSelected ? 'un' : '') + 'abandon', w);
        nw.Variables = items;
        nw.OnClose = _abandonCompany;
    }
};

function _abandonContact(sender, retval) {
    if (retval) {

        var isSMV = tw.SelectedNode.Icon.indexOf('smv') > -1;

        //** Kan vi nedlægge kontakt, vis nedlagt ikon, ESCRM-178/179
        if (document.getElementById('canAbandon').value == "true") {
            tw.SelectedNode.SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + 'Contact.png');
        }

        var alreadyDead = 0;
        var notDead = 0;
        for (var i = 0; i < tw.SelectedNode.ParentTreeviewNode.Nodes.length; i++) {
            if (tw.SelectedNode.ParentTreeviewNode.Nodes[i].Icon.indexOf('dead_') > -1)
                alreadyDead++;
            else
                notDead++;
        }

        if (cmdKill.IsSelected) {
            tw.SelectedNode.DataObject.childNodes[8].setAttribute('AbandonedBy', cmdKill.IsSelected ? '<b>' + document.getElementById('usernameAndInfo').value + '</b>' : '');
            tw.SelectedNode.DataObject.childNodes[8].setAttribute('AbandonedDate', cmdKill.IsSelected ? '<b>I dag</b>' : '');



            if (alreadyDead == tw.SelectedNode.ParentTreeviewNode.Nodes.length || tw.SelectedNode.ParentTreeviewNode.Nodes.length == 1)
                tw.SelectedNode.ParentTreeviewNode.SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + '.png');


            if (tw.SelectedNode.ParentTreeviewNode.DataObject != null) {
                tw.SelectedNode.ParentTreeviewNode.DataObject.childNodes[8].setAttribute('AbandonedBy', cmdKill.IsSelected ? '<b>' + document.getElementById('usernameAndInfo').value + '</b>' : '');
                tw.SelectedNode.ParentTreeviewNode.DataObject.childNodes[8].setAttribute('AbandonedDate', cmdKill.IsSelected ? '<b>I dag</b>' : '');
            }
        }
        else {

            if ((notDead != tw.SelectedNode.ParentTreeviewNode.Nodes.length && alreadyDead != tw.SelectedNode.ParentTreeviewNode.Nodes.length) || tw.SelectedNode.ParentTreeviewNode.Nodes.length == 1)
                tw.SelectedNode.ParentTreeviewNode.SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + '.png');

            tw.SelectedNode.DataObject.childNodes[8].setAttribute('AbandonedBy', '');
            tw.SelectedNode.DataObject.childNodes[8].setAttribute('AbandonedDate', '');

            tw.SelectedNode.ParentTreeviewNode.SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + '.png');
            if (tw.SelectedNode.ParentTreeviewNode.DataObject != null) {
                tw.SelectedNode.ParentTreeviewNode.DataObject.childNodes[8].setAttribute('AbandonedBy', '');
                tw.SelectedNode.ParentTreeviewNode.DataObject.childNodes[8].setAttribute('AbandonedDate', '');
            }
        }
    }
    else {
        cmdKill.SetSelectedState(!cmdKill.IsSelected);
    }

    if (tw.SelectedNode.DataObject) {
        document.getElementById('tdAbandonedBy').innerHTML = tw.SelectedNode.DataObject.childNodes[8].getAttribute('AbandonedBy') || '';
        document.getElementById('tdAbandonedDate').innerHTML = tw.SelectedNode.DataObject.childNodes[8].getAttribute('AbandonedDate') || '';
        document.getElementById('tdAbandonedBy').parentNode.style.display = document.getElementById('tdAbandonedDate').innerHTML != '' ? '' : 'none';
    }

    cmdKill.SetTextAndIcon(cmdKill.IsSelected ? 'Fjern nedlæggelse' : 'Nedlæg');
};

function _abandonCompany(sender, retval) {
    var node = getChosenCompanyNode();

    if (retval) {
        var isSMV = tw.SelectedNode.Icon.indexOf('smv') > -1;

        //** Kan vi nedlægge virksomhed, vis nedlagt ikon, ESCRM-178/179
        if (document.getElementById('canAbandon').value == "true") {
            node.SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + '.png');
        }

        if (node.DataObject != null) {
            if (cmdKill.IsSelected) {
                node.DataObject.childNodes[8].setAttribute('AbandonedBy', '<b>' + document.getElementById('usernameAndInfo').value + '</b>');
                node.DataObject.childNodes[8].setAttribute('AbandonedDate', '<b>I dag</b>');
            }
            else {
                node.DataObject.childNodes[8].setAttribute('AbandonedBy', '');
                node.DataObject.childNodes[8].setAttribute('AbandonedDate', '');
            }
        }

        for (var i = 0; i < node.Nodes.length; i++) {
            //** Kan vi nedlægge virksomhed, vis nedlagt ikon, ESCRM-178/179
            if (document.getElementById('canAbandon').value == "true") {
                node.Nodes[i].SetIcon('images/' + (cmdKill.IsSelected ? 'dead_' : '') + (isSMV ? 'smv' : 'pot') + 'Contact.png');
            }

            if (node.Nodes[i].DataObject != null) {
                if (cmdKill.IsSelected) {
                    node.Nodes[i].DataObject.childNodes[8].setAttribute('AbandonedBy', '<b>' + document.getElementById('usernameAndInfo').value + '</b>');
                    node.Nodes[i].DataObject.childNodes[8].setAttribute('AbandonedDate', '<b>I dag</b>');
                }
                else {
                    node.Nodes[i].DataObject.childNodes[8].setAttribute('AbandonedBy', '');
                    node.Nodes[i].DataObject.childNodes[8].setAttribute('AbandonedDate', '');
                }
            }
        }
    }
    else {
        cmdKill.SetSelectedState(!cmdKill.IsSelected);
    }

    if (node.DataObject) {
        document.getElementById('tdAbandonedBy').innerHTML = node.DataObject.childNodes[8].getAttribute('AbandonedBy') || '';
        document.getElementById('tdAbandonedDate').innerHTML = node.DataObject.childNodes[8].getAttribute('AbandonedDate') || '';
        document.getElementById('tdAbandonedBy').parentNode.style.display = document.getElementById('tdAbandonedDate').innerHTML != '' ? '' : 'none';
    }

    cmdKill.SetTextAndIcon(cmdKill.IsSelected ? 'Fjern nedlæggelse' : 'Nedlæg');
};

//Delete contact /company
function deleteNode() {
    var items = new Array();

    //remove / deactivate a contact, where multiple contacts are present
    if (tw.SelectedNode.ParentTreeviewNode && tw.SelectedNode.ParentTreeviewNode.Nodes.length > 0) {
        if (tw.SelectedNode.Uniqueidentifier != null && parseInt(tw.SelectedNode.Uniqueidentifier, 10) > 0) {
            items.push(tw.SelectedNode.Uniqueidentifier);
            var nw = w.CreateWindow('Delete.aspx?type=smvcontact', w);
            nw.Variables = items;
            nw.OnClose = _deleteContact;
        }
        else {
            if (!tw.SelectedNode.IsDirty) {
                tw.SelectedNode.Remove(tw.SelectedNode);
            }
            else {
                //Confirm = function(Question, Title, Icon, CallBack)
                w.Confirm('Er du sikker på, at du vil slette den valgte kontaktperson?<br><br>Denne er endnu ikke gemt, og vil derfor gå tabt.', 'Slet kontaktperson?', null, function (sender, retval) {
                    if (!retval) return;
                    tw.SelectedNode.Remove(tw.SelectedNode);
                    if (tw.Nodes.length == 0) {
                        w.IsDirty = false;
                        w.Close();
                        return;
                    }
                });
                //alert('no can do!');
            }
            if (tw.Nodes.length == 0) {
                w.IsDirty = false;
                w.Close();
                return;
            }
            return;
        }
    }
    else {
        var node = getChosenCompanyNode();
        if (node.Uniqueidentifier != null && parseInt(node.Uniqueidentifier, 10) > 0) {
            items.push(node.Uniqueidentifier);

            var nw = w.CreateWindow('Delete.aspx?type=smvcompany', w);
            nw.Variables = items;
            nw.OnClose = _deleteCompany;
        }
        else {
            if (!node.IsDirty && !tw.SelectedNode.IsDirty) {
                node.Remove(node);
            }
            else {
                w.Confirm('Er du sikker på, at du vil slette den valgte virksomhed?<br><br>Denne er endnu ikke gemt, og vil derfor gå tabt.', 'Slet virksomhed?', null, function (sender, retval) {
                    if (!retval) return;
                    node.Remove(node);
                    if (tw.Nodes.length == 0) {
                        w.IsDirty = false;
                        w.Close();
                        return;
                    }
                });
            }
            if (tw.Nodes.length == 0) {
                w.IsDirty = false;
                w.Close();
                return;
            }
        }
    }
    cmdKill.SetTextAndIcon(cmdKill.IsSelected ? 'Fjern nedlæggelse' : 'Nedlæg');
};

function _deleteContact(sender, retval) {
    if (retval) {

        var node = getChosenCompanyNode();
        if (retval[0] == tw.SelectedNode.Uniqueidentifier) {

            //w.Alert('Kan ikke slette kontaktpersonen, da denne er sendt til evaluering eller der findes andre data på kontaktpersonen som skal slettes først.');
            if (tw.SelectedNode.ParentTreeviewNode) {
                var items = new Array();
                var nw = w.CreateWindow('FlagsOnDelete.aspx?type=smvcontact&id=' + tw.SelectedNode.Uniqueidentifier, w);
                nw.Variables = items;
                nw.OnClose = null;
            }
            return;
        }

        tw.SelectedNode.Remove(tw);
        if (tw.Nodes.length <= 1 && node.Nodes.length == 0) {
            // commented by Ravi
            //w.IsDirty = false;
            //w.Close();
        }
        else {
            if (node.Nodes.length > 0)
                tw.SelectNode(node.Nodes[node.Nodes.length - 1]);
            else {
                node.Remove(node);
                tw.SelectNode(tw.Nodes[0]);
            }
        }
    }
};

function _deleteCompany(sender, retval) {
    if (retval) {

        var node = getChosenCompanyNode();
        if (node.Uniqueidentifier == retval[0]) {
            w.Alert('Kan ikke slette virksomheden, da en kontaktperson heri, er sendt til evaluering eller besidder data, som skal slettes først.');
        }

        node.Remove(tw);
        if (tw.Nodes.length == 0) {
            w.Close();
        }
        else {
            tw.SelectNode(tw.Nodes[tw.Nodes.length - 1]);
        }
    }
};

//Add a new contact
function addContact() {
    //    //User cannot create a new contact, if there is an "Unknown" Contact Present
    //    if (tw.SelectedNode.ParentTreeviewNode) {
    //        alert(tw.SelectedNode.DataObject);
    //        var dls = '';

    //        for (var i = 0; i < tw.SelectedNode.DataObject.childNodes[0].childNodes.length; i++) {
    //            dls += tw.SelectedNode.DataObject.childNodes[0].childNodes[i].type + '\n';

    ////            if (tw.SelectedNode.DataObject.childNodes[0].childNodes[i].type == 'checkbox') {
    ////                // 'Ukendt kontaktperson') {
    ////                var obj = document.getElementById(tw.SelectedNode.DataObject.childNodes[0].childNodes[i].id);

    //                if (obj != null) {
    //                    if (obj.parentNode.parentNode.parentNode.parentNode.getAttribute('DataLink') == 'Ukendt kontaktperson') {
    //                        alert('UKENDT KONTAKTPERSON');
    //                    }
    //                }

    //            //}
    //        }

    //        alert(dls);
    //    }
    //    else {

    //    }

    if (tw.SelectedNode.ParentTreeviewNode) {
        var n = tw.SelectedNode.ParentTreeviewNode.AddNode('Ny ' + (tw.SelectedNode.ParentTreeviewNode.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT') + ' kontaktperson', 'images/' + (tw.SelectedNode.ParentTreeviewNode.Icon.indexOf('smv') > 0 ? 'smv' : 'pot') + 'ContactNew.png');
        tw.SelectNode(n);
    }
    else {
        var n = tw.SelectedNode.AddNode('Ny ' + (tw.SelectedNode.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT') + ' kontaktperson', 'images/' + (tw.SelectedNode.Icon.indexOf('smv') > 0 ? 'smv' : 'pot') + 'ContactNew.png');
        tw.SelectNode(n);
    }


};

//Add a new contact
function addCompany() {
    //            if(tw.SelectedNode.ParentTreeviewNode)
    //            {
    var node = getChosenCompanyNode();
    var n = tw.AddNode('Ny ' + (node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT') + ' afdeling', 'images/' + (node.Icon.indexOf('smv') > 0 ? 'smv' : 'pot') + 'New.png');
    tw.SelectNode(n);

    addContact();
    n.Expand(n);

    tw.SelectNode(n);
    //            }
    //            else
    //            {
    //                var n =tw.SelectedNode.AddNode('Ny ' + (tw.SelectedNode.Icon.indexOf('smv')>0 ? 'SMV' : 'POT') + ' kontaktperson', 'images/' + (tw.SelectedNode.Icon.indexOf('smv')>0 ? 'smv' : 'pot') + 'ContactNew.png');
    //                tw.SelectNode(n);
    //            }


};

//Make sure any changes made are stored into memory for later save
function fixInputChanges() {
    _fixInputChanges(tblRoot.getElementsByTagName('INPUT'));
    _fixInputChanges(tblRoot.getElementsByTagName('TEXTAREA'));
    _fixInputChanges(tblRoot.getElementsByTagName('SELECT'));
    _createMapObjects(tblRoot.getElementsByTagName('DIV'));
};

function _fixInputChanges(arr) {
    for (var i = 0; i < arr.length; i++) {
        arr[i].onchange = function () {
            AutoLookUpData(this);
            updateTWNDValue(this);

            if (this.name == 'cbNoCVR') {
                setCVRRequired(this);
            }

            //Check if im a map changing item
            for (var x = 0; x < mapInputFields.length; x++) {
                if (this.id == mapInputFields[x]) {
                    updateMaps();
                    break;
                }
            }
        };

        if (arr[i].type == 'checkbox')
            arr[i].onclick = arr[i].onchange;
    }
};

function _createMapObjects(arr) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i].getAttribute('googlemaps') == 'true') {
            __createMapObject(arr[i]);
        }
    }
};

function __createMapObject(o) {
    var tbl = o.parentNode.parentNode.parentNode.parentNode;

    var latlng = new google.maps.LatLng(56.25, 10);

    var mapOptions = {
        zoom: 12,
        center: latlng,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    }


    tbl.map = new google.maps.Map(o, mapOptions);

    google.maps.event.trigger(tbl.map, 'resize');

    tbl.mapStreetSource = document.getElementById(tbl.getAttribute('MapStreetDataSource'));
    tbl.mapCitySource = document.getElementById(tbl.getAttribute('MapCityDataSource'));
    tbl.mapCountrySource = document.getElementById(tbl.getAttribute('MapCountryDataSource'));

    if (tbl.mapStreetSource != null)
        mapInputFields.push(tbl.getAttribute('MapStreetDataSource'));

    if (tbl.mapCitySource != null)
        mapInputFields.push(tbl.getAttribute('MapCityDataSource'));

    if (tbl.mapCountrySource != null)
        mapInputFields.push(tbl.getAttribute('MapCountryDataSource'));


    updateMap(tbl);
};

function updateMaps() {

    var arr = tblRoot.getElementsByTagName('DIV');
    for (var i = 0; i < arr.length; i++) {
        if (arr[i].getAttribute('googlemaps') == 'true') {
            updateMap(arr[i].parentNode.parentNode.parentNode.parentNode);
        }
    }
}

function updateMap(tbl) {
    var div = tbl.getElementsByTagName('DIV')[0].childNodes[0];

    //Try reading address values
    var street = tbl.mapStreetSource != null ? tbl.mapStreetSource.value : '';
    var zip = tbl.mapCitySource != null ? tbl.mapCitySource.value : '';
    var country = tbl.mapCountrySource != null ? tbl.mapCountrySource.value : '';


    if (street != '' && zip != '' && country != '') {
        div.parentNode.style.backgroundImage = '';
        div.style.display = '';

        //Invoke Ajax Call, executing the report
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/GeoCode.aspx';

        var addressLookup = '\t' + street + ', ' + zip + ', ' + country;

        AjaxObject.encVar('addresses', addressLookup);

        AjaxObject.OnCompletion = function () { __ajaxMapAddressLoaded(tbl, AjaxObject); };
        AjaxObject.OnError = function () { __ajaxMapAddressLoaded(tbl, AjaxObject); };
        AjaxObject.OnFail = function () { __ajaxMapAddressLoaded(tbl, AjaxObject); };
        AjaxObject.RunAJAX();
    }
    else {
        div.style.display = 'none';
        div.parentNode.style.backgroundImage = 'url(./images/noMap.png)';
        div.parentNode.style.backgroundRepeat = 'no-repeat';
        div.parentNode.style.backgroundPosition = 'center center';
    }

    google.maps.event.trigger(tbl.map, 'resize');

};

function __ajaxMapAddressLoaded(tbl, AjaxObject) {
    //Get Results and clean up Ajax object
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var o = document.createElement('DIV');
    o.innerHTML = result;

    var plotFound = false;

    while (o.hasChildNodes()) {
        var l = o.removeChild(o.firstChild);
        if (l.tagName == 'B') {
            if (l.children.length == 3) {
                var latLng = new google.maps.LatLng(parseFloat(l.children[1].innerHTML), parseFloat(l.children[2].innerHTML));

                plotFound = true;

                if (tbl.mapMarker != null) {
                    tbl.mapMarker.setPosition(latLng);
                }
                else {
                    tbl.mapMarker = new google.maps.Marker({
                        map: tbl.map,
                        position: latLng,
                        title: l.children[0].innerHTML
                    });


                    google.maps.event.addListener(tbl.mapMarker, 'click', function () {

                        var street = tbl.mapStreetSource != null ? tbl.mapStreetSource.value : '';
                        var zip = tbl.mapCitySource != null ? tbl.mapCitySource.value : '';
                        var country = tbl.mapCountrySource != null ? tbl.mapCountrySource.value : '';

                        if (street != '' && zip != '' && country != '') {
                            window.open('https://maps.google.com/maps?q=' + escape(street + ', ' + zip + ', ' + country));
                        }
                    });
                }
                tbl.map.setCenter(latLng);
            }
        }
    }

    var div = tbl.getElementsByTagName('DIV')[0].childNodes[0];

    if (!plotFound) {
        div.style.display = 'none';
        div.parentNode.style.backgroundImage = 'url(./images/noMap.png)';
        div.parentNode.style.backgroundRepeat = 'no-repeat';
        div.parentNode.style.backgroundPosition = 'center center';
    }
    else {
        div.parentNode.style.backgroundImage = '';
        div.style.display = '';
    }

    google.maps.event.trigger(tbl.map, 'resize');
};

function updateTWNDValue(input) {
    isDirty = true;
    var node = tw.SelectedNode;
    node.IsDirty = true;
    w.IsDirty = true;
    for (var i = 0; i < node.DataObject.childNodes[0].childNodes.length; i++) {
        if (node.DataObject.childNodes[0].childNodes[i].getAttribute('uId') == input.id) {
            if (input.tagName == 'SELECT') {
                var data = '';
                for (var u = 0; u < input.options.length; u++) {
                    if (input.options[u].selected) {
                        data += (data == '' ? '' : '\n') + input.options[u].value;
                    }
                }

                node.DataObject.childNodes[0].childNodes[i].value = data;
            }
            else {
                if (input.type == 'checkbox') {
                    node.DataObject.childNodes[0].childNodes[i].value = input.checked ? 'true' : '';
                }
                else {
                    node.DataObject.childNodes[0].childNodes[i].value = input.value;
                }
            }
        }

        //Test the field
        RequeredFieldTestField(input);
    }

    if (tw.SelectedNode.ParentTreeviewNode) {
        //contact
        var arr = document.getElementsByTagName('INPUT');
        var firstName = '';
        var lastName = '';
        for (var i = 0; i < arr.length; i++) {
            var o = arr[i];

            var op = null;

            try {
                op = o.parentNode.parentNode.parentNode.parentNode;
            }
            catch (err) {
                op = o;
            }
            if (!op) op = o;

            if (op.getAttribute('DataLink') == 'DGSContactFirstname') {
                firstName = o.value;
                if (lastName != '') break;
            }
            if (op.getAttribute('DataLink') == 'DGSCOntactLastname') {
                lastName = o.value;
                if (firstName != '') break;
            }
        }

        tw.SelectedNode.SetName(firstName + ' ' + lastName);
        w.SetTitle(tw.Nodes[0].Name.replace(/&amp;/g, '&') + ', ' + tw.SelectedNode.Name.replace(/&amp;/g, '&') + (tw.Nodes[0].Icon.indexOf('pot.png') == -1 && tw.Nodes[0].Icon.indexOf('potContactNew.png') == -1 ? ' (SMV)' : ' (POT)'));
    }
    else {
        //company
        var arr = document.getElementsByTagName('INPUT');
        var companyName = '';
        for (var i = 0; i < arr.length; i++) {
            var o = arr[i];

            var op = null;

            try {
                op = o.parentNode.parentNode.parentNode.parentNode;
            }
            catch (err) {
                op = o;
            }
            if (!op) op = o;

            if (op.getAttribute('DataLink') == 'NNECompanyName') {
                companyName = o.value;
                break;
            }
        }
        tw.SelectedNode.SetName(companyName);
        w.SetTitle(tw.Nodes[0].Name.replace(/&amp;/g, '&') + (tw.Nodes[0].Icon.indexOf('pot.png') == -1 && tw.Nodes[0].Icon.indexOf('potNew.png') == -1 ? ' (SMV)' : ' (POT)'));
    }
};

//Contact reader
var contentIsLoading = false;
function selectContact(node) {

    if (node.Uniqueidentifier) {
        var dt = node.Uniqueidentifier.toString().split('.');
        //** Sæt contactID, ESCRM-111/112/113
        valgtContactId = dt[0];
        if (dt.length == 2) {
            node.Uniqueidentifier = dt[0];
            node.nneId = dt[1];

            if (node.ParentTreeviewNode)
                node.ParentTreeviewNode.nneId = dt[1];
        }

        //** Kald smvEdit.aspx med parametre, ESCRM-13/97
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('Id', node.Uniqueidentifier);
        AjaxObject.encVar('Icon', node.Icon);
        AjaxObject.encVar('type', 'setFavorite');

        AjaxObject.OnCompletion = function () { GetFavorite("Completion", node, AjaxObject); };
        AjaxObject.OnError = function () { GetFavorite("OnError", node, AjaxObject); };
        AjaxObject.OnFail = function () { GetFavorite("OnFail", node, AjaxObject); };
        AjaxObject.RunAJAX();
    }

    if (node.ParentTreeviewNode) {
        if (node.ParentTreeviewNode.Uniqueidentifier) {
            dt = node.ParentTreeviewNode.Uniqueidentifier.toString().split('.');

            if (dt.length == 2) {
                node.ParentTreeviewNode.Uniqueidentifier = dt[0];
                node.ParentTreeviewNode.nneId = dt[1];

                node.nneId = dt[1];
            }
        }
    }

    cmdWebSite.SetEnabledState(node.ParentTreeviewNode == null ? true : false);
    cmdAddCompany.SetEnabledState(tw.Nodes[0].Icon.indexOf('pot.png') == -1 && tw.Nodes[0].Icon.indexOf('potNew.png') == -1);

    if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null && node.Uniqueidentifier != '0') {
        if (cmdDelete) cmdDelete.Enable();
        if (node.ParentTreeviewNode)
            cmdForward.Enable();
        else
            cmdForward.Disable();
    }
    else {
        if (cmdDelete) cmdDelete.Enable();
        cmdForward.Disable();
    }

    if (node.nneId) {
        cmdGoNNE.Enable();
    }
    else {
        var tmpNode = node;
        if (node.ParentTreeviewNode)
            tmpNode = node.ParentTreeviewNode;

        if (tmpNode.DataObject != null) {
            if (tmpNode.DataObject.childNodes != null) {
                var nneId = parseInt(tmpNode.DataObject.childNodes[0].getAttribute('nneId'), 10);
                if (nneId > 0)
                    cmdGoNNE.Enable();
                else
                    cmdGoNNE.Disable();
            }
            else
                cmdGoNNE.Disable();
        }
        else
            cmdGoNNE.Disable();
    }


    if (cmdConvert) {
        if (tw.Nodes[0].Icon.indexOf('pot.png') == -1) {
            cmdConvert.Disable();
        }
        else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null)
            cmdConvert.Enable();
        else
            cmdConvert.Disable();

    }

    if (node.ParentTreeviewNode) {
        if (cmdEvaluate) {
            if (trUserIsSentToEvaluation.style.display != 'none')
                cmdEvaluate.Disable();
            else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null) {
                if (document.getElementById('localWithNoContract').value == '1')
                    cmdEvaluate.Disable();
                else
                    cmdEvaluate.Enable();
            }
            else
                cmdEvaluate.Disable();
        }
        if (cmdDriftevaluering) {
            if (trUserIsSentToEvaluation.style.display != 'none')
                cmdDriftevaluering.Disable();
            else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null) {
                if (document.getElementById('localWithNoContract').value == '1')
                    cmdDriftevaluering.Disable();
                else
                    cmdDriftevaluering.Enable();
            }
            else
                cmdDriftevaluering.Disable();
        }

        if (cmdGlue) {
            if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null)
                cmdGlue.Enable();
            else
                cmdGlue.Disable();
        }

        if (node.DataObject)
            __loadContact(node);
        else
            _loadContact(node);

        if (companyFields.style.display == '') {
            companyFields.style.display = 'none';
            contactFields.style.display = '';
            contactFields.style.width = companyFields.style.width;
            contactFields.style.height = companyFields.style.height;
        }
    }
    else {
        $('#trContactAgreement').hide();
        if (cmdEvaluate) {
            cmdEvaluate.Disable();
        }
        if (cmdDriftevaluering) {
            cmdDriftevaluering.Disable();
        }

        if (cmdGlue) {
            cmdGlue.Disable();
        }

        if (node.DataObject) {
            __loadCompany(node);
        }
        else {
            _loadCompany(node);
        }

        if (contactFields.style.display == '') {
            contactFields.style.display = 'none';
            companyFields.style.display = '';
            companyFields.style.width = contactFields.style.width;
            companyFields.style.height = contactFields.style.height;
        }
    }

    fixLWsAMethod();
};

function _loadCompany(node) {
    if (contentIsLoading) {
        setTimeout(function () { _loadCompany(node); }, 100);
        return;
    }

    $('#companyID').val(node.Uniqueidentifier);
    contentIsLoading = true;
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/smvEdit.aspx';
    AjaxObject.encVar('Id', node.Uniqueidentifier);
    AjaxObject.encVar('type', 'company');
    AjaxObject.encVar('type2', node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT');
    AjaxObject.encVar('loadHistory', 'true');
    AjaxObject.encVar('pid', $('#pid').val());

    if (node.Uniqueidentifier == null || node.Uniqueidentifier == 'null' || node.Uniqueidentifier == '0') {
        var sid = tw.Nodes[0].Uniqueidentifier;
        AjaxObject.encVar('sid', sid);
    }

    core.DOM.DisableFields(document);

    AjaxObject.OnCompletion = function () {
        __loadCompany(node, AjaxObject);
    };
    AjaxObject.OnError = function () { __loadCompany(node, AjaxObject); };
    AjaxObject.OnFail = function () { __loadCompany(node, AjaxObject); };
    AjaxObject.RunAJAX();
};

function __loadCompany(node, AjaxObject) {
    if (AjaxObject) {
        var result = AjaxObject.Response;
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;
        var obj = document.createElement('DIV');
        obj.innerHTML = result;
        node.DataObject = obj;

        core.DOM.EnableFields(document);
        contentIsLoading = false;
    }

    //** Sæt den aktuelle smv i companyId2, ESCRM-13/97
    $('#currentCompanyID2').val(node.Uniqueidentifier);

    var ctData = node.DataObject.childNodes[0];
    for (var i = 0; i < ctData.childNodes.length; i++) {
        var oo = document.getElementById(ctData.childNodes[i].getAttribute('uId'));
        if (oo) {
            var data = ctData.childNodes[i].value;

            if (oo.tagName == 'INPUT' || oo.tagName == 'TEXTAREA') {
                if (oo.type == 'checkbox') {
                    oo.checked = data == 'true' || data == '1' || data == 'Ja';
                }
                else {
                    oo.value = data;
                    var sensitive = oo.getAttribute("sensitive");
                    if (sensitive != undefined) {
                        if (sensitive == "1" && data == '') {
                            oo.removeAttribute("disabled");
                        } else {
                            oo.setAttribute("disabled", "disabled");
                        }
                    }
                }
            }
            else if (oo.tagName == 'SELECT') {
                for (var u = 0; u < oo.options.length; u++) {
                    oo.options[u].selected = false;
                }
                var items = data.split('\n');
                var itemSelected = false;
                for (var u = 0; u < items.length; u++) {
                    var opt = getOptionFromValue(oo, items[u]);
                    if (opt) {
                        opt.selected = true;
                        itemSelected = true;
                        //break;
                    }
                }

                if (!itemSelected && oo.options.length > 0) {
                    if (data != '') {
                        var elOptNew = document.createElement('option');
                        elOptNew.text = data;
                        elOptNew.value = data;
                        try {
                            oo.add(elOptNew, null); // standards compliant; doesn't work in IE
                        }
                        catch (ex) {
                            oo.add(elOptNew); // IE only
                        }
                        oo.options[oo.options.length - 1].selected = true;
                    }
                    else {
                        if (!oo.getAttribute('multiple'))
                            oo.options[0].selected = true;
                    }
                }

                var sensitive = oo.getAttribute("sensitive");
                if (sensitive != undefined) {
                    var selValue = oo.options[oo.selectedIndex].value;
                    if (sensitive == "1" && selValue == '') {
                        oo.removeAttribute("disabled");
                    } else {
                        oo.setAttribute("disabled", "disabled");
                    }
                }
            }

            //Test the field
            RequeredFieldTestField(oo);
        }
    }

    //Set cvr state
    //setCVRRequired(document.getElementById('cbNoCVR'));
    //Set my title
    w.SetTitle(tw.SelectedNode.Name.replace(/&amp;/g, '&') + (tw.Nodes[0].Icon.indexOf('pot.png') == -1 && tw.Nodes[0].Icon.indexOf('potNew.png') == -1 ? ' (SMV)' : ' (POT)'));
    //          

    //Read SMV data
    var cache = node.DataObject.childNodes[1].innerHTML;
    liHistory.__ToItems(node.DataObject.childNodes[1]);
    node.DataObject.childNodes[1].innerHTML = cache;

    if (node.DataObject.childNodes.length > 3) {
        cache = node.DataObject.childNodes[3].innerHTML;
        liAVN.__ToItems(node.DataObject.childNodes[3]);
        node.DataObject.childNodes[3].innerHTML = cache;
    }

    //History
    document.getElementById('tdCreatedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('createdBy' || '');
    document.getElementById('tdDateCreated').innerHTML = node.DataObject.childNodes[2].getAttribute('dateCreated') || '';
    document.getElementById('tdUpdatedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('lastUpdatedBy') || '';
    document.getElementById('tdDateUpdated').innerHTML = node.DataObject.childNodes[2].getAttribute('lastUpdated') || '';

    document.getElementById('tdAbandonedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('AbandonedBy') || '';
    document.getElementById('tdAbandonedDate').innerHTML = node.DataObject.childNodes[2].getAttribute('AbandonedDate') || '';

    document.getElementById('tdUpdatedBy').parentNode.style.display = document.getElementById('tdDateUpdated').innerHTML != '' ? '' : 'none';
    document.getElementById('tdCreatedBy').parentNode.style.display = document.getElementById('tdDateCreated').innerHTML != '' ? '' : 'none';
    document.getElementById('tdAbandonedBy').parentNode.style.display = document.getElementById('tdAbandonedDate').innerHTML != '' ? '' : 'none';

    cmdKill.SetSelectedState(document.getElementById('tdAbandonedDate').innerHTML != '');
    cmdKill.SetTextAndIcon(cmdKill.IsSelected ? 'Fjern nedlæggelse' : 'Nedlæg');


    if (node.DataObject != null) {
        if (node.DataObject.childNodes != null) {
            var nneId = parseInt(node.DataObject.childNodes[0].getAttribute('nneId'), 10);
            if (nneId > 0) {
                node.nneId = nneId;
                cmdGoNNE.Enable();
            }
            else
                cmdGoNNE.Disable();
        }
        else
            cmdGoNNE.Disable();
    }
    else
        cmdGoNNE.Disable();

    // Refresh map data
    updateMaps();
    $('#z_companies_1_Firmanavn_1').trigger("onchange");
};

function _loadContact(node) {
    if (contentIsLoading) {
        setTimeout('_loadContact(tw.SelectedNode);', 100);
        return;
    }

    $('#contactID').val(node.Uniqueidentifier);
    contentIsLoading = true;
    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/smvEdit.aspx';
    AjaxObject.encVar('Id', node.Uniqueidentifier);
    AjaxObject.encVar('type', 'contact');
    AjaxObject.encVar('type2', node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT');

    if (node.ParentTreeviewNode == null)
        AjaxObject.encVar('loadHistory', 'true');
    else if (node.ParentTreeviewNode.DataObject == null)
        AjaxObject.encVar('loadHistory', 'true');

    core.DOM.DisableFields(document);

    AjaxObject.OnCompletion = function () { __loadContact(node, AjaxObject); };
    AjaxObject.OnError = function () { __loadContact(node, AjaxObject); };
    AjaxObject.OnFail = function () { __loadContact(node, AjaxObject); };
    AjaxObject.RunAJAX();
};

function __loadContact(node, AjaxObject) {
    if (AjaxObject) {
        var result = AjaxObject.Response;
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;

        var obj = document.createElement('DIV');
        obj.innerHTML = result;
        node.DataObject = obj;

        core.DOM.EnableFields(document);
        contentIsLoading = false;
    }
    var ctData = node.DataObject.childNodes[0];


    for (var i = 0; i < ctData.childNodes.length; i++) {
        var oo = document.getElementById(ctData.childNodes[i].getAttribute('uId'));

        if (ctData.childNodes[i].getAttribute('uId') == 'trContactAgreement') {
            if (document.getElementById('txtAllowContactAgreement').value == "1") {
                var data = ctData.childNodes[i].value;
                if (data == "1") {
                    $('#trContactAgreement').hide();
                    cmdKontraktaftale.Show();
                }
                else {
                    cmdKontraktaftale.Hide();
                    $('#trContactAgreement').show();
                    $('#trContactAgreement .windowInlineHeader').attr("style", "background-color: red; background-image: none;");
                    $('#trContactAgreement .windowInlineHeader').html('<span id="aContactName" runat="server" style="font-weight:bold;">Kontakt</span> har ingen aftale. <a class="windowInlineHeaderLink" onclick="ShowAgreement();">Tilføj kontraktsaftale</a>.');
                }
            }
        }
        else {
            if (oo) {
                var data = ctData.childNodes[i].value;

                if (oo.tagName == 'INPUT' || oo.tagName == 'TEXTAREA') {
                    if (oo.type == 'checkbox')
                        oo.checked = data == 'true' || data == '1' || data == 'Ja';
                    else {
                        //Get table parent of input
                        //                            var tblp = oo.parentNode.parentNode.parentNode.parentNode;
                        //                            if (tblp.getAttribute('dbDefaultValue') != null && tblp.getAttribute('dbDefaultValue') != '' && (data == '' || data == null)) {
                        //                                oo.value = tblp.getAttribute('dbDefaultValue');
                        //                                oo.style.backgroundColor = 'yellow';
                        //                            }
                        //                            else {
                        //                                oo.value = data;
                        //                                oo.style.color = null;
                        //                            }
                        oo.value = data;
                        var sensitive = oo.getAttribute("sensitive");
                        if (sensitive != undefined && sensitive == "1" && data == '') {
                            oo.removeAttribute("disabled");
                        }
                    }
                }
                else if (oo.tagName == 'SELECT') {

                    for (var u = 0; u < oo.options.length; u++) {
                        oo.options[u].selected = false;
                    }
                    var items = data.split('\n');
                    var itemSelected = false;
                    for (var u = 0; u < items.length; u++) {
                        var opt = getOptionFromValue(oo, items[u]);
                        if (opt) {
                            opt.selected = true;
                            itemSelected = true;
                        }
                    }

                    if (!itemSelected && oo.options.length > 0) {
                        if (data != '') {
                            var elOptNew = document.createElement('option');
                            elOptNew.text = data;
                            elOptNew.value = data;
                            try {
                                oo.add(elOptNew, null); // standards compliant; doesn't work in IE
                            }
                            catch (ex) {
                                oo.add(elOptNew); // IE only
                            }
                            oo.options[oo.options.length - 1].selected = true;
                        }
                        else {
                            if (!oo.getAttribute('multiple')) {
                                oo.options[0].selected = true;
                            }
                        }
                    }

                    var sensitive = oo.getAttribute("sensitive");
                    if (sensitive != undefined) {
                        //var selValue = oo.options[oo.selectedIndex].value;
                        //alert(selValue + ' <> ' + sensitive);
                        alert(sensitive);
                    }
                }

                //Test the field
                RequeredFieldTestField(oo);
            }
        }
    }

    //Set my title
    //w.SetTitle(tw.SelectedNode.ParentNode.Name.replace(/&amp;/g,'&') + ', ' + tw.SelectedNode.Name.replace(/&amp;/g,'&') + (tw.Nodes[0].Icon.indexOf('pot.png') == -1 ? ' (SMV)' : ' (POT)') );

    //Read SMV data


    if (node.ParentTreeviewNode.DataObject != null) {
        var cache = node.ParentTreeviewNode.DataObject.childNodes[1].innerHTML;
        liHistory.__ToItems(node.ParentTreeviewNode.DataObject.childNodes[1]);
        node.ParentTreeviewNode.DataObject.childNodes[1].innerHTML = cache;

        if (node.DataObject.childNodes.length > 3) {
            cache = node.ParentTreeviewNode.DataObject.childNodes[3].innerHTML;
            liAVN.__ToItems(node.ParentTreeviewNode.DataObject.childNodes[3]);
            node.ParentTreeviewNode.DataObject.childNodes[3].innerHTML = cache;
        }
    }
    else {
        var cache = node.DataObject.childNodes[1].innerHTML;
        liHistory.__ToItems(node.DataObject.childNodes[1]);
        node.DataObject.childNodes[1].innerHTML = cache;

        if (node.DataObject.childNodes.length > 3) {
            cache = node.DataObject.childNodes[3].innerHTML;
            liAVN.__ToItems(node.DataObject.childNodes[3]);
            node.DataObject.childNodes[3].innerHTML = cache;
        }
    }


    //Updated / Created
    document.getElementById('tdCreatedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('createdBy') || '';
    document.getElementById('tdDateCreated').innerHTML = node.DataObject.childNodes[2].getAttribute('dateCreated') || '';
    document.getElementById('tdUpdatedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('lastUpdatedBy') || '';
    document.getElementById('tdDateUpdated').innerHTML = node.DataObject.childNodes[2].getAttribute('lastUpdated') || '';

    document.getElementById('tdAbandonedBy').innerHTML = node.DataObject.childNodes[2].getAttribute('AbandonedBy') || '';
    document.getElementById('tdAbandonedDate').innerHTML = node.DataObject.childNodes[2].getAttribute('AbandonedDate') || '';

    document.getElementById('tdUpdatedBy').parentNode.style.display = document.getElementById('tdDateUpdated').innerHTML != '' ? '' : 'none';
    document.getElementById('tdCreatedBy').parentNode.style.display = document.getElementById('tdDateCreated').innerHTML != '' ? '' : 'none';
    document.getElementById('tdAbandonedBy').parentNode.style.display = document.getElementById('tdAbandonedDate').innerHTML != '' ? '' : 'none';

    cmdKill.SetSelectedState(document.getElementById('tdAbandonedDate').innerHTML != '');
    cmdKill.SetTextAndIcon(cmdKill.IsSelected ? 'Fjern nedlæggelse' : 'Nedlæg');

    //Refresh map data
    updateMaps();
};

function getOptionFromValue(select, value) {
    value = value.replace('\r', '').replace('\n', '');

    for (var i = 0; i < select.options.length; i++) {

        if (select.options[i].value.toLowerCase() == value.toLowerCase()) {
            return select.options[i];
        }
    }

    return null;
};

function hSplitSet(h1, h2) {

};

function fixLWsBMethod(h2) {

};

function OnResize(win, t, l, w, h) {
    //document.getElementById('spDebug').innerHTML += 'resize: ' + h + '<br>';
    windowLastHeight = h;

    var tww = tw.Container.offsetWidth;
    tdContent.style.width = w - tww;

    if (liAVN != null)
        liAVN.SetSize(199, (h / 2) - 50);

    tw.SetSize(200 - (document.all ? 0 : 5), h - toolbar.offsetHeight - (document.all ? 0 : 4) - document.getElementById('tblAVN').offsetHeight);
    fixLWsAMethod();

    if (contactFields.style.display == '') {
        contactFields.style.width = w - tww - (document.all ? 0 : 0);
        //contactFields.style.height = h - tdContentBottom.offsetHeight - toolbar.offsetHeight - trUserIsSentToEvaluation.offsetHeight - trCompanyIsTransferred.offsetHeight;
    }
    else {
        companyFields.style.width = w - tww - (document.all ? 0 : 0);
        //companyFields.style.height = h - tdContentBottom.offsetHeight - toolbar.offsetHeight - trUserIsSentToEvaluation.offsetHeight - trCompanyIsTransferred.offsetHeight;
    }
};

function fixLWsAMethod() {
    //document.getElementById('spDebug').innerHTML += 'fixlw: ' + w.GetInnerHeight() + '<br>';
    //Set fields height
    contactFields.style.height = '';
    companyFields.style.height = '';

    //Set listview size
    if (liHistory != null)
        liHistory.SetSize(parseInt(tdContent.style.width, 10), 1);

    var cfH = 0;

    if (windowLastHeight == 0) windowLastHeight = w.GetInnerHeight();

    var tHeight = windowLastHeight -
        document.getElementById('toolbar').offsetHeight -
        document.getElementById('divContactHistoryToolbar').offsetHeight -
        (document.getElementById('trUserIsSentToEvaluation') != null ? document.getElementById('trUserIsSentToEvaluation').offsetHeight : 0) -
        (document.getElementById('trCompanyIsTransferred') != null ? document.getElementById('trCompanyIsTransferred').offsetHeight : 0) -
        (core.IsIE ? 1 : 0);

    //Figure out, which way to distribute the height
    //Balanced, to fit fields data, with a "max"
    if (listviewStyle == 0) {
        if (contactFields.style.display == '') {
            cfH = contactFields.scrollHeight + 8;
            if (contactFields.scrollWidth > contactFields.offsetWidth)
                cfH += 24;
        }
        else {
            cfH = companyFields.scrollHeight + 8;
            if (companyFields.scrollWidth > companyFields.offsetWidth)
                cfH += 24;
        }

        if (tHeight - cfH < 100)
            cfH = tHeight - 100;
        //        alert(cfH + ' total h: ' + tHeight);

    }
    //Centered
    else if (listviewStyle == 1) {
        cfH = tHeight / 2;
    }
    else if (listviewStyle == 2) {
        cfH = 50;
    }
    else if (listviewStyle == 3) {
        cfH = tHeight - 50;
    }


    //Limit size
    if (cfH > tHeight - 50)
        cfH = tHeight - 50;


    //Listview size, based on the rest air in height
    var lwH = tHeight - cfH;

    //document.getElementById('spDebug').innerHTML += 'fixlw: ' + tHeight + ' / ' + cfH + ' / ' + lwH + '<br>';

    //Set fields height
    contactFields.style.height = cfH;
    companyFields.style.height = cfH;

    //Set listview size
    if (liHistory != null)
        liHistory.SetSize(parseInt(tdContent.style.width, 10), lwH);

    //** Disable vækst- og drift evaluering, ESCRM-111/112
    CanEvaluationButtonsBeShown();
};

function setCVRRequired(sender) {
    var iCVR = document.getElementById('txtCVR');
    var tCVR = iCVR.parentNode.parentNode.parentNode.parentNode;
    //            var img = document.getElementById('required_txtCVR');

    if (sender.checked) {
        iCVR.value = '';
        iCVR.style.backgroundColor = '#f1f1f1';
        iCVR.disabled = true;
        tCVR.setAttribute('RequiredState', '0');
        iCVR.RequiredObject.style.display = 'none';
    }
    else {
        iCVR.disabled = false;
        iCVR.style.backgroundColor = '';
        tCVR.setAttribute('RequiredState', '1');
        iCVR.RequiredObject.style.display = '';
        //Test the field
        RequeredFieldTestField(iCVR);
    }
};

function setPNRRequired(sender) {
    //            alert('set pnr req');
    //            return;
    var iCVR = document.getElementById('txtCVR');
    var tCVR = iCVR.parentNode.parentNode.parentNode.parentNode;
    //            var img = document.getElementById('required_txtCVR');

    if (sender.checked) {
        iCVR.value = '';
        iCVR.style.backgroundColor = '#f1f1f1';
        iCVR.disabled = true;
        tCVR.setAttribute('RequiredState', '0');
        iCVR.RequiredObject.style.display = 'none';
    }
    else {
        iCVR.disabled = false;
        iCVR.style.backgroundColor = '';
        tCVR.setAttribute('RequiredState', '1');
        iCVR.RequiredObject.style.display = '';
        //Test the field
        RequeredFieldTestField(iCVR);
    }
};
var hiddenIndex = -1;

function LoadHidden(node) {
    if (!node.DataObject) {
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('Id', node.Uniqueidentifier);
        AjaxObject.encVar('type', hiddenIndex == -1 ? 'company' : 'contact');
        AjaxObject.encVar('type2', node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT');

        AjaxObject.OnCompletion = function () { _LoadHidden(node, AjaxObject); };
        AjaxObject.OnError = function () { _LoadHidden(node, AjaxObject); };
        AjaxObject.OnFail = function () { _LoadHidden(node, AjaxObject); };
        AjaxObject.RunAJAX();
    }
    else
        _LoadHidden(node, null);

};

function _LoadHidden(node, AjaxObject) {
    if (AjaxObject) {
        var result = AjaxObject.Response;
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;

        var obj = document.createElement('DIV');
        obj.innerHTML = result;
        node.DataObject = obj;
    }

    hiddenIndex++;

    if (hiddenIndex < tw.Nodes[0].Nodes.length) {
        LoadHidden(tw.Nodes[0].Nodes[hiddenIndex]);
    }
    else {
        setTimeout(function () { Save(null, true, 0); }, 1);
    }
};

function loadEverythingAndSaveASSMV() {
    hiddenIndex = -1;
    LoadHidden(tw.Nodes[0]);
};

function Save(callBack, convertToSMV, primaryIndex) {
    document.getElementById('txtAutoStore').focus();

    var mCP = '';
    var sCP = '';

    if (primaryIndex == null) {
        if (document.getElementById('convertToSMV').value == 'true') {
            loadEverythingAndSaveASSMV();
            return;
        }
        primaryIndex = 0;
    }

    // Company first
    var ctData = null;
    if (tw.Nodes[primaryIndex].DataObject) {
        ctData = tw.Nodes[primaryIndex].DataObject.childNodes[0];

        for (var i = 0; i < ctData.childNodes.length; i++) {
            var fld = RequeredFieldTestFieldManual(ctData.childNodes[i].getAttribute('uId'), ctData.childNodes[i].value);
            if (fld) {
                if (IsFieldOnlySuggested(ctData.childNodes[i].getAttribute('uId')))
                    sCP += (sCP == '' ? '' : '<br>') + '&nbsp;&nbsp;&nbsp;&nbsp;' + fld;
                else
                    mCP += (mCP == '' ? '' : '<br>') + '&nbsp;&nbsp;&nbsp;&nbsp;' + fld;
            }
        }

        if (mCP != '') {
            w.Alert('Der kan ikke gemmes, da følgende felter for virksomheden ikke er udfyldt korrekt:<br><br>' + mCP, 'Kan ikke gemme', null, null, null);
            tw.SelectNode(tw.Nodes[primaryIndex]);
            if (primaryIndex > 0)
                resetButtonsAndStuffAfterSave();
            else
                w.SetCloseable(true);
            return;
        }

        if (sCP != '') {
            w.Confirm('Det anbefales at følgende felter udfyldes, før virksomheden gemmes:<br><br>' + sCP + '<br><br><b>Bemærk: Denne virksomhed ikke kan overføres før alle felter markeret med en stjerne er udfyldt.</b>', 'Gem inkomplet virksomhed?', null, function (sender, val) { if (val) { _ctSave(callBack, convertToSMV, primaryIndex); } else { return; } }, null, null);
            tw.SelectNode(tw.Nodes[primaryIndex]);
            if (primaryIndex > 0)
                resetButtonsAndStuffAfterSave();
            else
                w.SetCloseable(true);
            return;
        }
    }
    else {
        //company has not been loaded
        if (tw.Nodes[primaryIndex].Uniqueidentifier == '' || tw.Nodes[primaryIndex].Uniqueidentifier == null) {
            w.Alert('Der kan ikke gemmes før den nyoprettede virksomhed er udfyldt korrekt.', 'Kan ikke gemme', null, null, null);
            tw.SelectNode(tw.Nodes[primaryIndex]);
            if (primaryIndex > 0)
                resetButtonsAndStuffAfterSave();
            else
                w.SetCloseable(true);
            return;
        }
    }
    _ctSave(callBack, convertToSMV, primaryIndex);
};

function _ctSave(callBack, convertToSMV, primaryIndex) {
    var mCP = '';
    var sCP = '';
    var ctData = null;

    for (var tn = 0; tn < tw.Nodes[primaryIndex].Nodes.length; tn++) {
        if (tw.Nodes[primaryIndex].Nodes[tn].Uniqueidentifier != null) {
            var skip = false;
            for (var c = 0; c < cprStatus.length; c++) {
                if (cprStatus[c].split('#')[0] == tw.Nodes[primaryIndex].Nodes[tn].Uniqueidentifier) {
                    skip = true;
                }
            }
            if (!skip) {
                cprStatus.push(tw.Nodes[primaryIndex].Nodes[tn].Uniqueidentifier + '#0');
            }
        }
    }

    // Company is ready to be saved - verify all contacts
    var selectedContactNode = tw.SelectedNode.Uniqueidentifier;
    for (var n = 0; n < tw.Nodes[primaryIndex].Nodes.length; n++) {

        if (tw.Nodes[primaryIndex].Nodes[n].DataObject) {

            ctData = tw.Nodes[primaryIndex].Nodes[n].DataObject.childNodes[0];

            for (var i = 0; i < ctData.childNodes.length; i++) {
                if (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == selectedContactNode &&
                    ctData.childNodes[i].getAttribute('uId') == 'z_contacts_1_CPR-nummer_1' &&
                    !$('#z_contacts_1_CPR-nummer_1').prop('disabled')) {
                    ctData.childNodes[i].value = $('#z_contacts_1_CPR-nummer_1').val();
                }

                var fld = RequeredFieldTestFieldManual(ctData.childNodes[i].getAttribute('uId'), ctData.childNodes[i].value);

                var through = true;
                if (ctData.childNodes[i].getAttribute('uId') == 'z_contacts_1_CPR-nummer_1' &&
                    $('#z_contacts_1_CPR-nummer_1').prop('disabled')) {
                    through = false;
                }
                if (ctData.childNodes[i].getAttribute('uId') == 'z_contacts_1_CPR-nummer_1') {

                    // value
                    var cprValue = ctData.childNodes[i].value;
                    var isCprValue = (cprValue.length > 5 && cprValue.substring(cprValue.length - 5, cprValue.length) == '-****');
                    if (isCprValue) {
                        through = false;
                    }

                    // lookup into cpr status array
                    for (var nl = 0; nl < cprStatus.length; nl++) {
                        var lookupNodeUniqueIdentifier = cprStatus[nl].split('#')[0];
                        var lookupCPRAllowed = cprStatus[nl].split('#')[1];
                        if (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == lookupNodeUniqueIdentifier) {
                            if (lookupCPRAllowed == 1) {
                                through = false;
                            }
                        }
                    }
                }

                if (through) {
                    if (fld) {
                        if (IsFieldOnlySuggested(ctData.childNodes[i].getAttribute('uId'))) {
                            sCP += (sCP == '' ? '' : '<br>') + '&nbsp;&nbsp;&nbsp;&nbsp;' + fld;
                        }
                        else {
                            mCP += (mCP == '' ? '' : '<br>') + '&nbsp;&nbsp;&nbsp;&nbsp;' + fld;
                        }
                    }
                }
            }
        }
        else {

            //contact has not been loaded
            if (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == '' || tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == null) {
                w.Alert('Der kan ikke gemmes før den nyoprettede kontaktperson er udfyldt korrekt.', 'Kan ikke gemme', null, null, null);
                tw.SelectNode(tw.Nodes[primaryIndex].Nodes[n]);
                if (primaryIndex > 0)
                    resetButtonsAndStuffAfterSave();
                else
                    w.SetCloseable(true);
                return;
            }
        }

        if (mCP != '') {
            if (mCP == '&nbsp;&nbsp;&nbsp;&nbsp;CPR-nummer') {
                w.Confirm('CPR nummeret opfylder ikke modulus kontrol. Ønsker du at fortsætte?', 'Kan ikke gemme', null, function (sender, val) {
                    if (val) {
                        // CPR Status update
                        for (var i = 0; i < cprStatus.length; i++) {
                            if (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == cprStatus[i].split('#')[0]) {
                                cprStatus[i] = (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier + '#1');
                            }
                        }
                        Save();
                    } else {
                        return;
                    }
                }, null, null);

                tw.SelectNode(tw.Nodes[primaryIndex].Nodes[n]);
                if (primaryIndex > 0) { resetButtonsAndStuffAfterSave(); }
                else { w.SetCloseable(true); }

                return;
            } else {
                w.Alert('Der kan ikke gemmes, da følgende felter for kontaktpersonen ikke er udfyldt korrekt:<br><br>' + mCP, 'Kan ikke gemme', null, null, null);
                tw.SelectNode(tw.Nodes[primaryIndex].Nodes[n]);
                if (primaryIndex > 0) { resetButtonsAndStuffAfterSave(); }
                else { w.SetCloseable(true); }

                return;
            }

            // CPR Status update
            for (var i = 0; i < cprStatus.length; i++) {
                var cprNodeUniqueIdentifier = cprStatus[i].split('#')[0];
                var cprAllowStatus = cprStatus[i].split('#')[1];
                if (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier == cprNodeUniqueIdentifier) {
                    cprStatus[i] = (tw.Nodes[primaryIndex].Nodes[n].Uniqueidentifier + '#1');
                }
            }
        }
    }

    if (sCP != '') {
        w.Confirm('Det anbefales at følgende felter udfyldes, før kontaktpersonen gemmes:<br><br>' + sCP + '<br><br><b>Bemærk: Denne virksomhed/kontaktperson ikke kan overføres før alle felter markeret med en stjerne er udfyldt.</b>', 'Gem inkomplet kontaktperson?', null, function (sender, val) { if (val) { __ctSave(callBack, convertToSMV, primaryIndex); } else { return; } }, null, null);
        tw.SelectNode(tw.Nodes[primaryIndex].Nodes[n]);
        if (primaryIndex > 0) resetButtonsAndStuffAfterSave();
        return;
    } else {
        //Ready for save!!!
        __ctSave(callBack, convertToSMV, primaryIndex);
    }
};

function _readyforsave(sCP, callBack, convertToSMV, primaryIndex) {
    if (sCP != '') {
        w.Confirm('Det anbefales at følgende felter udfyldes, før kontaktpersonen gemmes:<br><br>' + sCP + '<br><br><b>Bemærk: Denne virksomhed/kontaktperson ikke kan overføres før alle felter markeret med en stjerne er udfyldt.</b>', 'Gem inkomplet kontaktperson?', null, function (sender, val) { if (val) { __ctSave(callBack, convertToSMV, primaryIndex); } else { return; } }, null, null);
        tw.SelectNode(tw.Nodes[primaryIndex].Nodes[n]);
        if (primaryIndex > 0) resetButtonsAndStuffAfterSave();
        return;
    }
    else {
        //Ready for save!!!
        __ctSave(callBack, convertToSMV, primaryIndex);
    }
}

function __ctSave(callBack, convertToSMV, primaryIndex) {
    //Ready for save!!!
    core.DOM.DisableFields(document);
    cmdSave.Disable();

    _saving(0, callBack, convertToSMV, primaryIndex);

};

function __CheckForSameEmail(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, failure) {
    var ajaxResult = AjaxObject.Response;
    var result = ajaxResult.split(',');

    //CLEAN AJAX
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    //See if the contact is "unknown"
    var ctIsUnknown = false;

    if (node != null) {
        var ctData = node.DataObject.childNodes[0];

        for (var i = 0; i < ctData.childNodes.length; i++) {
            var uid = ctData.childNodes[i].getAttribute('uId');
            var o = document.getElementById(uid);

            if (o != null) {
                var op = o.parentNode.parentNode.parentNode.parentNode;

                if (op.getAttribute('DataLink') == 'Ukendt kontaktperson') {

                    if (o.checked) {
                        ctIsUnknown = true;
                        break;
                    }
                }
            }
        }
    }

    if (ajaxResult == '' || ctIsUnknown) {
        _saving(index, callBack, convertToSMV, primaryIndex, true);
    }
    else {
        var nw = w.CreateWindow('SMVContactsConfirmDoubleContact.aspx', w);

        nw.Variables = [[index, callBack, convertToSMV, primaryIndex], result];
        nw.OnClose = ___CheckForSameEmail;
        return;
    }
};

function ___CheckForSameEmail(sender, retval) {
    if (retval) {
        _saving(retval[0], retval[1], retval[2], retval[3], true);
    }
    else {
        resetButtonsAndStuffAfterSave(null);
        isDirty = true;
        w.IsDirty = true;

        //Abort save - check if contact is new or already exists
        var node = null;
        var index = sender.Variables[0][0];
        var primaryIndex = sender.Variables[0][3];

        if (index == 0)
            node = tw.Nodes[primaryIndex];
        else
            node = tw.Nodes[primaryIndex].Nodes[index - 1];

        if (node != null) {
            if (isNaN(parseInt(node.Uniqueidentifier, 10))) {
                SMVDeletePOTCompanyAfterFailedSave(node);
            }
        }
    }
};

function _saving(index, callBack, convertToSMV, primaryIndex, dontDoEmailCheck) {

    tblSave.style.display = '';

    var max = tw.Nodes[primaryIndex].Nodes.length + 1;
    var percent = parseInt((index + 1) / max * 100);
    pb.AnimateProgress(percent);

    var node = null;

    if (index == 0)
        node = tw.Nodes[primaryIndex];
    else {
        node = tw.Nodes[primaryIndex].Nodes[index - 1];

        if (node.DataObject && node.IsDirty && dontDoEmailCheck != true) {
            //Harcoded email address for contact - validate and check if it is unique in the database
            var txtEmail = document.getElementById('z_contacts_1_Email_1');
            if (txtEmail != null) {
                if (txtEmail.value != null && txtEmail.value != '') {
                    //alert('validate email: ' + txtEmail.value);

                    //Run ajax
                    var AjaxObject = new core.Ajax();
                    AjaxObject.requestFile = 'ajax/SMV_GetContactsWithSameEmail.aspx';
                    AjaxObject.encVar('contactId', node.Uniqueidentifier);
                    AjaxObject.encVar('email', txtEmail.value);

                    AjaxObject.OnCompletion = function () { __CheckForSameEmail(index, AjaxObject, node, callBack, convertToSMV, primaryIndex); };
                    AjaxObject.OnError = function () { __CheckForSameEmail(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
                    AjaxObject.OnFail = function () { __CheckForSameEmail(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
                    AjaxObject.RunAJAX();
                    return;
                }
            }
        }
    }

    if (node.DataObject && (node.IsDirty || convertToSMV == true)) {

        //ajax     
        var txtOut = '';

        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('save', 'true');
        AjaxObject.encVar('type', index == 0 || index == null ? 'company' : 'contact');
        AjaxObject.encVar('txtId', node.Uniqueidentifier);
        AjaxObject.encVar('txtType', node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT');

        txtOut += (txtOut == '' ? '' : '\n') + 'save=true';
        txtOut += (txtOut == '' ? '' : '\n') + 'type=' + (index == 0 || index == null ? 'company' : 'contact');
        txtOut += (txtOut == '' ? '' : '\n') + 'txtId=' + node.Uniqueidentifier;
        txtOut += (txtOut == '' ? '' : '\n') + 'txtType=' + (node.Icon.indexOf('smv') > 0 ? 'SMV' : 'POT');

        if (convertToSMV)
            AjaxObject.encVar('convertToSMV', 'true');

        if (index > 0)
            AjaxObject.encVar('txtCompanyId', tw.Nodes[primaryIndex].Uniqueidentifier);

        var ctData = node.DataObject.childNodes[0];
        AjaxObject.encVar('txtNNEId', ctData.getAttribute('nneId'));
        txtOut += (txtOut == '' ? '' : '\n') + 'txtNNEId=' + ctData.getAttribute('nneId');

        for (var i = 0; i < ctData.childNodes.length; i++) {
            var oo = document.getElementById(ctData.childNodes[i].getAttribute('uId'));
            if (oo) {
                if (oo.type == 'checkbox') {
                    if (ctData.childNodes[i].value == 'true' || ctData.childNodes[i].value == '1' || ctData.childNodes[i].value == 'Ja')
                        AjaxObject.encVar('_' + ctData.childNodes[i].getAttribute('uId'), '1');
                    else
                        AjaxObject.encVar('_' + ctData.childNodes[i].getAttribute('uId'), '0');
                }
                else if (oo.tagName == 'SELECT') {
                    //alert(oo.name + ' multiple is ' + oo.getAttribute('multiple'));
                    if (oo.getAttribute('multiple')) {
                        AjaxObject.encVar('_' + ctData.childNodes[i].getAttribute('uId'), ctData.childNodes[i].value);
                    }
                    else {
                        var value = getCorrectSelectValue(oo, ctData.childNodes[i].value);
                        AjaxObject.encVar('_' + ctData.childNodes[i].getAttribute('uId'), value);
                    }
                }
                else {
                    AjaxObject.encVar('_' + ctData.childNodes[i].getAttribute('uId'), ctData.childNodes[i].value);
                }

                txtOut += (txtOut == '' ? '' : '\n') + ctData.childNodes[i].getAttribute('uId') + '=' + ctData.childNodes[i].value;
            }
        }

        AjaxObject.OnCompletion = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex); };
        AjaxObject.OnError = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
        AjaxObject.OnFail = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
        AjaxObject.RunAJAX();
    }
    else {

        //If this is a child node of a company, which just has been created, make sure to update CompanyOwnerId for the contact 
        if (node.ParentTreeviewNode != null) {
            if (node.ParentTreeviewNode.JustCreated) {

                //alert('set new company id to children: ' + node.Uniqueidentifier + '.CompanyOwnerId=' + node.ParentTreeviewNode.Uniqueidentifier);
                var AjaxObject = new core.Ajax();
                AjaxObject.requestFile = 'ajax/smvEdit.aspx';
                AjaxObject.encVar('setContactOwnerId', 'true');
                AjaxObject.encVar('ContactId', node.Uniqueidentifier);
                AjaxObject.encVar('CompanyId', node.ParentTreeviewNode.Uniqueidentifier);

                AjaxObject.OnCompletion = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex); };
                AjaxObject.OnError = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
                AjaxObject.OnFail = function () { __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, true); };
                AjaxObject.RunAJAX();

            }
            else
                __saving(index, null, node, callBack, convertToSMV, primaryIndex);
        }
        else
            __saving(index, null, node, callBack, convertToSMV, primaryIndex);
    }
};

function getCorrectSelectValue(obj, currentValue) {
    for (var i = 0; i < obj.options.length; i++) {
        if (obj.options[i].value.toLowerCase() == currentValue.toLowerCase()) {
            //alert('Value for ' + obj.name + ' is  in list');
            return obj.options[i].value;
        }
    }


    if (currentValue == '' && obj.options.length > 0) {
        //alert('Value for ' + obj.name + ' is not in list and value is "", returning "' + obj.options[0].value + '"');
        return obj.options[0].value
    }
    else {
        //alert('Value for ' + obj.name + ' is not in list and value is set');
        return currentValue;
    }
};

function setUpdatedState(node) {
    //new or upate           
    if (parseInt(node.Uniqueidentifier, 10) > 0) {
        //        if (node.DataObject != null) {
        //            node.DataObject.childNodes[8].setAttribute('lastUpdatedBy', '<b>' + document.getElementById('usernameAndInfo').value + '</b>');
        //            node.DataObject.childNodes[8].setAttribute('lastUpdated', '');
        //        }
        //        document.getElementById('tdUpdatedBy').innerHTML = '<b>' + document.getElementById('usernameAndInfo').value + '</b>';
        //        document.getElementById('tdDateUpdated').innerHTML = '<b>I dag</b>';

        //        document.getElementById('tdDateUpdated').parentNode.style.display = '';

        node.JustCreated = false;
    }
    else {
        //        document.getElementById('tdCreatedBy').innerHTML = '<b>' + document.getElementById('usernameAndInfo').value + '</b>';
        //        document.getElementById('tdDateCreated').innerHTML = '<b>I dag</b>';

        //        if (node.DataObject != null) {
        //            node.DataObject.childNodes[8].setAttribute('createdBy', '<b>' + document.getElementById('usernameAndInfo').value + '</b>');
        //            node.DataObject.childNodes[8].setAttribute('dateCreated', '<b>I dag</b>');
        //        }

        node.JustCreated = true;
    }
};

function __saving(index, AjaxObject, node, callBack, convertToSMV, primaryIndex, failure) {
    var max = tw.Nodes[primaryIndex].Nodes.length + 1;
    
    if (AjaxObject) {
        node.IsDirty = false;
        w.IsDirty = false;

        var ajaxResult = AjaxObject.Response;
        var result = ajaxResult.split(',');

        //CLEAN AJAX
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;

        if (result.length == 1) {

            //ID Obtained? Make sure the ID is valid.
            var IdForSavedItem = parseInt(result[0], 10);

            if (IdForSavedItem == 0 || result[0] == '' || isNaN(result[0])) {
                w.Alert('Der opstod desværre en fejl, mens der blev forsøgt at gemme.<br><br><b>Løsningsforslag:</b><br>Prøv at gemme igen<br>Ryd din browser-cache og prøv igen.', 'Kan ikke gemme', null, null, null);
                resetButtonsAndStuffAfterSave(node);
                node.IsDirty = true;
                w.IsDirty = true;
                return;
            }

            setUpdatedState(node);
            node.Uniqueidentifier = IdForSavedItem;

            node.SetIcon('images/' + (node.Icon.indexOf('smv') > 0 ? 'smv' : 'pot') + (index == 0 ? '' : 'Contact') + '.png');

            if (convertToSMV)
                w.SetTitle(tw.Nodes[primaryIndex].Name.replace(/&amp;/g, '&') + ', ' + tw.SelectedNode.Name.replace(/&amp;/g, '&') + (tw.Nodes[primaryIndex].Icon.indexOf('pot.png') == -1 && tw.Nodes[primaryIndex].Icon.indexOf('potNew.png') == -1 ? ' (SMV)' : ' (POT)'));
        }
        else {

            //Something went wrong during the save attemp - try figureing out, what it is, and act accordingly.
            //Existing PNR/CPR
            var allNumberic = true;
            for (var i = 0; i < result.length; i++) {
                if (isNaN(result[i])) {
                    allNumberic = false;
                    break;
                }
            }

            //if entire array is numeric, either PNR or CPR exists elsewhere in the database.
            if (allNumberic) {

                //Is saving object contact or company?
                if (node.ParentTreeviewNode != null) {
                    //Delete the company, if the only contact attached is the newly saved company
                    SMVDeletePOTCompanyAfterFailedSave(node);

                    var mic = '';

                    if (result.length > 2 && document.getElementById('isAdmin').value == '1') {
                        mic = '<br><br><div style="white-space:nowrap;font-weight:bold;font-size:13px;">Bemærk: Der findes dubletter med dette CPR-nummer!</div><br>' +
                            'Det anbefales, at der straks ryddes op i disse! <a href="javascript:void(0);" onclick="">Tryk her for at rydde op i dubletterne</a> (Bemærk: Denne funktion er ikke implementeret).';
                    }

                    w.Alert('<div style="white-space:nowrap;font-weight:bold;font-size:13px;">Kontaktpersonen med dette CPR-nummer findes allerede!</div><br>' +
                        'Ønsker du at oprette en ny kontaktperson, skal du vælge et andet CPR-nummer, ellers skal du afbryde oprettelsen og i stedet:<br><br>'
                        + '<a href="javascript:void(0);" ' +
                        'onclick="window.frameElement.IDCWindow.CreateWindow(\'SMVContactsEdit.aspx?id=' + result[1] + '\'); window.frameElement.IDCWindow.Close();">' +
                        'finde den eksisterende kontaktperson</a>.' + mic, 'Kan ikke gemme', null, null, null);
                }
                else //Company
                {
                    var mic = '';

                    if (result.length > 2 && document.getElementById('isAdmin').value == '1') {
                        mic = '<br><br><div style="white-space:nowrap;font-weight:bold;font-size:13px;">Bemærk: Der findes dubletter med dette P-nummer!</div><br>' +
                            'Det anbefales, at der straks ryddes op i disse! <a href="javascript:void(0);" onclick="window.frameElement.IDCWindow.CreateWindow(\'SMVDoublesManager.aspx\');">Tryk her for at rydde op i dubletterne</a>.';
                    }

                    tw.SelectNode(tw.Nodes[primaryIndex]);
                    w.Alert('<div style="white-space:nowrap;font-weight:bold;font-size:13px;">Virksomheden med dette CVR og P-nummer findes allerede!</div><br>' +
                        'Ønsker du at oprette en ny afdeling, skal du vælge et andet P-nummer, ellers skal du afbryde oprettelsen og i stedet:<br><br>'
                        + '<a href="javascript:void(0);" ' +
                        'onclick="window.frameElement.IDCWindow.CreateWindow(\'SMVContactsEdit.aspx?companyid=' + result[1] + '\'); window.frameElement.IDCWindow.Close();">' +
                        'finde den eksisterende virksomhed</a>.' + mic, 'Kan ikke gemme', null, null, null);
                }

                //Reset and exit save loop!
                resetButtonsAndStuffAfterSave(node);
                node.IsDirty = true;
                w.IsDirty = true;
                return;
            }
            else //Generic error occurred!
            {
                w.Alert('Der opstod desværre en fejl, mens der blev forsøgt at gemme.<br><br><b>Løsningsforslag:</b><br>Prøv at gemme igen<br>Ryd din browser-cache og prøv igen.<br><br><a href="javascript:void(0);" onclick="var w = window.open(\'about:blank\');w.document.open(); w.document.write(window.frameElement.IDCWindow.Variables);w.document.close();">Se intern fejlmedelelse</a>', 'Kan ikke gemme', null, null, ajaxResult);
                resetButtonsAndStuffAfterSave(node);
                node.IsDirty = true;
                w.IsDirty = true;
                return;
            }
        }
    }

    index++;

    if (index < max) {
        _saving(index, callBack, convertToSMV, primaryIndex);
    }
    else {
        if (primaryIndex < tw.Nodes.length - 1) {
            pb.ResetTo(0);
            primaryIndex++;
            core.DOM.EnableFields(document);
            Save(callBack, convertToSMV, primaryIndex);
        }
        else {
            resetButtonsAndStuffAfterSave(node, true);
            isDirty = false;
            w.IsDirty = false;

            cmdAddCompany.SetEnabledState(tw.Nodes[0].Icon.indexOf('pot.png') == -1);

            if (callBack)
                callBack();
        }
    }
};

function SMVDeletePOTCompanyAfterFailedSave(node) {
    if (!node.ParentTreeviewNode) return;

    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/SMVDeletePOTCompanyAfterFailedSave.aspx';
    AjaxObject.encVar('CompanyId', node.ParentTreeviewNode.Uniqueidentifier);

    AjaxObject.OnCompletion = function () { __SMVDeletePOTCompanyAfterFailedSave(AjaxObject, node); };
    AjaxObject.OnError = function () { __SMVDeletePOTCompanyAfterFailedSave(AjaxObject, node); };
    AjaxObject.OnFail = function () { __SMVDeletePOTCompanyAfterFailedSave(AjaxObject, node); };
    AjaxObject.RunAJAX();
};

function __SMVDeletePOTCompanyAfterFailedSave(AjaxObject, node) {
    //CLEAN AJAX
    var ajaxResult = AjaxObject.Response;

    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    if (!isNaN(ajaxResult)) {
        node.ParentTreeviewNode.Uniqueidentifier = 0;
        node.ParentTreeviewNode.SetIcon('images/potContactNew.png');
        node.ParentTreeviewNode.IsDirty = true;
    }

};

function getAllIdsToVerify() {
    var result = new Array();
    result.push(new Array());
    result.push(new Array());

    for (var i = 0; i < tw.Nodes.length; i++) {
        result[0].push(tw.Nodes[i].Uniqueidentifier);

        for (var ii = 0; ii < tw.Nodes[i].Nodes.length; ii++) {
            result[1].push(tw.Nodes[i].Nodes[ii].Uniqueidentifier);
        }
    }

    return result;
};

function verifyData(cpIdsPending, ctIdsPending, node, total) {

    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/smvVerify.aspx';

    AjaxObject.OnCompletion = function () { _verifyData(AjaxObject, cpIdsPending, ctIdsPending, node, total); };
    AjaxObject.OnError = function () { _verifyData(AjaxObject, cpIdsPending, ctIdsPending, node, total); };
    AjaxObject.OnFail = function () { _verifyData(AjaxObject, cpIdsPending, ctIdsPending, node, total); };

    //Verify companies, then contacts
    if (cpIdsPending.length > 0) {
        AjaxObject.encVar('companyId', cpIdsPending[0]);
        cpIdsPending.splice(0, 1);
        AjaxObject.RunAJAX();
    }
    else if (ctIdsPending.length > 0) {
        AjaxObject.encVar('contactId', ctIdsPending[0]);
        ctIdsPending.splice(0, 1);
        AjaxObject.RunAJAX();
    }
    else {
        _verifyData(null, cpIdsPending, ctIdsPending, node, total);
    }
};

function _verifyData(AjaxObject, cpIdsPending, ctIdsPending, node, total) {
    if (AjaxObject != null) {
        var result = AjaxObject.Response;

        if (result != '1') {
            pb.ProgressIsBad();
        }
        else {
            pb.ProgressIsGood();
        }

        //CLEAN AJAX
        AjaxObject.Reset();
        AjaxObject.Dispose();
        delete AjaxObject;
        AjaxObject = null;
    }
    var reached = total - (cpIdsPending.length + ctIdsPending.length);
    pb.AnimateProgress(reached / total * 100);

    //Resume save complete, once all nodes has been verified.
    if (cpIdsPending.length == 0 && ctIdsPending.length == 0)
        _resetButtonsAndStuffAfterSave(node);
    else
        verifyData(cpIdsPending, ctIdsPending, node, total);
};

function resetButtonsAndStuffAfterSave(node, doVerifyDataCheck) {
    document.getElementById('tblSavingHeader').innerHTML = 'Verificerer data...';
    pb.ResetTo(0);
    pb.ProgressIsGood();

    if (doVerifyDataCheck != null) {
        var res = getAllIdsToVerify();
        verifyData(res[0], res[1], node, res[0].length + res[1].length);
    }
    else
        _resetButtonsAndStuffAfterSave(node);

};

function _resetButtonsAndStuffAfterSave(node) {
    tblSave.style.display = 'none';
    pb.ResetTo(0);

    if (pb.IsBad()) {
        w.Alert('Der opstod desværre en fejl, mens der blev forsøgt at verficere de gemte data.<br><br><b>Løsningsforslag:</b><br>Prøv at gemme igen<br>Ryd din browser-cache og prøv igen.', 'Fejl under verifikation', null, null, null);
    }


    pb.ProgressIsGood();

    core.DOM.EnableFields(document);
    cmdSave.Enable();
    if (cmdDelete) cmdDelete.Enable();
    cmdForward.Enable();

    if (cmdConvert) {
        if (!node) node = tw.SelectedNode || tw.Nodes[0];

        if (tw.Nodes[0].Icon.indexOf('pot.png') == -1) {
            cmdConvert.Disable();
        }
        else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null)
            cmdConvert.Enable();
        else
            cmdConvert.Disable();

        if (node.ParentTreeviewNode) {
            if (cmdEvaluate) {
                if (trUserIsSentToEvaluation.style.display != 'none') {
                    cmdEvaluate.Disable();
                }
                else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null) {
                    if (document.getElementById('localWithNoContract').value == '1')
                        cmdEvaluate.Disable();
                    else
                        cmdEvaluate.Enable();
                }
                else {
                    cmdEvaluate.Disable();
                }
            }
            if (cmdDriftevaluering) {
                if (trUserIsSentToEvaluation.style.display != 'none') {
                    cmdDriftevaluering.Disable();
                }
                else if (node.Uniqueidentifier != '' && node.Uniqueidentifier != null) {
                    if (document.getElementById('localWithNoContract').value == '1')
                        cmdDriftevaluering.Disable();
                    else
                        cmdDriftevaluering.Enable();
                }
                else {
                    cmdDriftevaluering.Disable();
                }
            }
        }
        else {
            if (cmdEvaluate) { cmdEvaluate.Disable(); }
            if (cmdDriftevaluering) { cmdDriftevaluering.Disable(); }
        }
    }

    w.SetCloseable(true);
};

function OnClose() {
    if (isDirty) {
        w.Confirm('Vil du lukke denne SMV/POT virksomhed uden at gemme?', 'Luk uden at gemme?', null, _OnClose);
        return true;
    }
};

function _OnClose(sender, retval) {
    if (retval)
        window.Close();
};

//Notes management
function createNote() {

    var arg = '?cpId=';
    if (tw.SelectedNode.ParentTreeviewNode)
        arg = '?ctId=';

    if (!tw.SelectedNode.Uniqueidentifier || tw.SelectedNode.Uniqueidentifier == '0') {
        if (tw.SelectedNode.ParentTreeviewNode)
            w.Alert('Du skal gemme din kontaktpersons stamdata før du kan tilføje noter.', 'Kan ikke oprette ny note!', null, null, null);
        else
            w.Alert('Du skal gemme din virksomheds stamdata før du kan tilføje noter.', 'Kan ikke oprette ny note!', null, null, null);

        return;
    }

    var nw = w.CreateWindow('SMVNote.aspx' + arg + tw.SelectedNode.Uniqueidentifier, w);
    nw.Variables = null;
    nw.OnClose = _createNote;
};

function _createNote(sender, retval) {
    if (retval) {
        var node = retval[1];
        if (node) {

            node.SetName(retval[4], 1);
            node.SetColumnName(retval[2], 2);
            node.SetColumnName(retval[3], 4);
            node.SetColumnName(retval[6], 5);
            node.SetColumnName(retval[8], 6);
            node.SetColumnName(retval[7], 7);
            node.SetColumnName(userId.value, 11);

            node.Icon16 = 'images/note.png';
            node.SetCurrentIcon('images/note.png');
        }
        else {
            var values = new Array();

            values.push('Note');
            values.push(retval[4]);
            values.push(retval[2]);
            values.push(tw.SelectedNode.Name);
            values.push(retval[3]);
            values.push(retval[6]);
            values.push(retval[8]);
            values.push(retval[7]);
            values.push('');
            values.push('');
            values.push('');

            values.push(userId.value);
            values.push('NOTE');
            values.push('0');
            values.push(tw.SelectedNode.Uniqueidentifier);
            values.push(orgId.value);

            node = new IDC_ListviewItem(liHistory.Document, liHistory.Container, values);
            node.Icon16 = 'images/noteNew.png';
            node.Uniqueidentifier = retval[0];

            liHistory.AddItemObjectAt(node, 0);

            //store
            if (tw.SelectedNode != null)
                tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
        }
    }
};

function editNote(node) {
    var uid = '';

    if (!node) {
        if (liHistory.SelectedItems.length == 0) {
            w.Alert('Marker en note du ønsker at redigere.', 'Kan ikke redigere!', null, null, null);
            return;
        }

        node = liHistory.SelectedItems[0];
    }

    if (node) uid = (node.Uniqueidentifier ? node.Uniqueidentifier : '');

    var nw = w.CreateWindow('SMVNote.aspx?id=' + uid, w);
    nw.Variables = node;
    nw.OnClose = _createNote;

};

var noteDeleteItems = null;
function deleteNote() {
    var items = new Array();
    var noDelete = '';


    for (var i = 0; i < liHistory.SelectedItems.length; i++) {

        //only delete own or organisation notes if admin.
        if (liHistory.SelectedItems[i].Items[11] == userId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else
            noDelete += (noDelete == '' ? '' : '<br>') + liHistory.SelectedItems[i].Items[0];
    }

    if (items.length == 0) {
        w.Alert('Du har ikke rettigheder til at slette den eller de noter du har valgt.', 'Kan ikke slette!', null, null, null);
        return;
    }

    noteDeleteItems = items;

    if (noDelete != '') {
        w.Confirm('Følgende noter kan ikke slettes på grund af manglende rettigheder:<br><br>' + noDelete + '<br><br>Fortsæt alligevel?', 'Kan ikke slette alle noter', __deleteNote);
        return;
    }
    else
        __deleteNote(null, true);

};

function __deleteNote(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('Delete.aspx?type=note', w);
        nw.Variables = noteDeleteItems;
        nw.OnClose = _deleteNote;
    }
};

function _deleteNote(sender, retval) {
    if (retval) {
        for (var i = liHistory.SelectedItems.length; i--; i > -1) {
            //only delete own or organisation notes if admin.
            if (liHistory.SelectedItems[i].Items[11] == userId.value)
                liHistory.SelectedItems[i].Remove();
            else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
                liHistory.SelectedItems[i].Remove();
        }
    }
};

//SAM management
function editSAM(node) {
    var uid = '';

    if (!node) {
        if (samlw.SelectedItems.length == 0) {
            w.Alert('Marker en SAM kontaktperson du ønsker at åbne.', 'Kan ikke åbne SAM!', null, null, null);
            return;
        }

        node = samlw.SelectedItems[0];
    }

    if (node) uid = (node.Uniqueidentifier ? node.Uniqueidentifier : '');

    var nw = w.CreateWindow('SAMContactsEdit.aspx?id=' + uid);
    nw.Variables = node;

};

function createSAM() {

    if (!tw.SelectedNode.ParentTreeviewNode) {
        w.Alert('Der kan ikke tilføjes SAM kontaktpersoner på en virksomhed.<br><br>Vælg en kontaktperson i stedet.', 'Kan ikke tilføje SAM kontaktperson!', null, null, null);
        return;
    }

    if (!tw.SelectedNode.Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan tilføje SAM kontaktpersoner.', 'Kan ikke tilføje SAM kontaktperson!', null, null, null);
        return;
    }

    var nw = w.CreateWindow('SAMContactsBrowse.aspx', w);
    nw.Variables = null;
    nw.OnClose = _createSAM;
};

function __createSAM(sender, retval) {
    if (retval) {
        var values = new Array();

        var hhv = '';
        if (retval[2] == '1') hhv += (hhv == '' ? '' : ', ') + 'Henvisning';
        if (retval[4] == '1') hhv += (hhv == '' ? '' : ', ') + 'SAM';
        if (retval[3] == '1') hhv += (hhv == '' ? '' : ', ') + 'Mentor';

        values.push('SAM Relation');
        values.push('I dag');
        values.push(retval[1][2] + (hhv != '' ? ' (' + hhv + ')' : ''));
        values.push(tw.SelectedNode.Name);
        values.push(retval[5]);
        values.push('');
        values.push('');
        values.push('');
        values.push('');
        values.push('');
        values.push(retval[7]);

        values.push(userId.value);
        values.push('SAM');
        values.push(retval[9]);
        values.push(tw.SelectedNode.Uniqueidentifier);
        values.push(orgId.value);

        node = new IDC_ListviewItem(liHistory.Document, liHistory.Container, values);
        node.Icon16 = 'images/samNew.png';
        node.Uniqueidentifier = retval[0];

        liHistory.AddItemObjectAt(node, 0);

        //store
        if (tw.SelectedNode != null)
            tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
    }
};

function _createSAM(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('SMVChooseSAMRelation.aspx?id=' + retval[0] + '&ctId=' + tw.SelectedNode.Uniqueidentifier, w);
        nw.Variables = retval;
        nw.OnClose = __createSAM;
    }
};

var samDeleteItems = null;
function deleteSAM() {

    var items = new Array();
    var noDelete = '';

    for (var i = 0; i < liHistory.SelectedItems.length; i++) {
        if (isAdmin.value == '0' && liHistory.SelectedItems[i].Items[11] == userId.value)
            items.push(liHistory.SelectedItems[i].Items[13]);
        else if (isAdmin.value == '1')
            items.push(liHistory.SelectedItems[i].Items[13]);
        else
            noDelete += (noDelete == '' ? '' : '<br>') + liHistory.SelectedItems[i].Items[1];
    }

    if (items.length == 0) {
        w.Alert('Du har ikke rettigheder til at slette relationerne til den eller de SAM kontaktpersoner du har valgt.', 'Kan ikke slette!', null, null, null);
        return;
    }

    samDeleteItems = items;

    if (noDelete != '') {
        w.Confirm('Følgende SAM kontaktpersoner kan ikke slettes på grund af manglende rettigheder:<br><br>' + noDelete + '<br><br>Fortsæt alligevel?', 'Kan ikke slette alle SAM kontaktpersoner', __deleteSAM);
        return;
    }
    else
        __deleteSAM(null, true);

};

function __deleteSAM(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('Delete.aspx?type=samrelationbyid', w);
        nw.Variables = samDeleteItems;
        nw.OnClose = _deleteSAM;
    }
};

function _deleteSAM(sender, retval) {
    if (retval) {
        for (var i = liHistory.SelectedItems.length; i--; i > -1) {
            //only delete own or other SAM rel if admin.
            if (isAdmin.value == '0' && liHistory.SelectedItems[i].Items[11] == userId.value)
                liHistory.SelectedItems[i].Remove();
            else if (isAdmin.value == '1')
                liHistory.SelectedItems[i].Remove();
        }
    }
};

function addToMailGroup() {
    if (!tw.SelectedNode.ParentTreeviewNode) {
        w.Alert('Der kan ikke tilføjes interessegrupper til en virksomhed.<br><br>Vælg en kontaktperson i stedet.', 'Kan ikke tilføje interessegruppe!', null, null, null);
        return;
    }

    if (!tw.SelectedNode.Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan tilføje en interessegruppe.', 'Kan ikke tilføje interessegruppe!', null, null, null);
        return;
    }

    var nw = w.CreateWindow('mailgroupsBrowse.aspx?contactId=' + tw.SelectedNode.Uniqueidentifier, w);
    nw.Variables = null;
    nw.OnClose = __addToMailGroup;
};

function __addToMailGroup(sender, retval) {
    if (retval) {

        node = new IDC_ListviewItem(liHistory.Document, liHistory.Container, ['Interessegruppe', retval[3], retval[1], tw.SelectedNode.Name, retval[2], '', '', '', '', '', '', userId.value, 'MAILGROUP', '0', tw.SelectedNode.Uniqueidentifier, orgId.value]);
        node.Icon16 = 'images/mailgroupTwNew.png';
        node.Uniqueidentifier = retval[0];

        liHistory.AddItemObjectAt(node, 0);

        //store
        if (tw.SelectedNode != null)
            tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
    }
};

function deleteFromMailgroup() {
    if (liHistory.SelectedItems.length == 0) {
        w.Alert('Marker en eller flere interessegrupper du ønsker at slette relationen til.', 'Kan ikke slette!', null, null, null);
        return;
    }

    var items = new Array();
    for (var i = 0; i < liHistory.SelectedItems.length; i++) {
        //only delete own or organisation notes if admin.
        items.push(tw.SelectedNode.Uniqueidentifier + '|' + liHistory.SelectedItems[i].Uniqueidentifier);
    }

    var nw = w.CreateWindow('Delete.aspx?type=mailgrouprelation', w);
    nw.Variables = items;
    nw.OnClose = _deleteFromMailgroup;

};

function _deleteFromMailgroup(sender, retval) {
    if (retval) {
        for (var i = liHistory.SelectedItems.length; i--; i > -1) {
            liHistory.SelectedItems[i].Remove();
        }
    }
};

function addDocument() {
    if (!tw.SelectedNode.ParentTreeviewNode) {
        w.Alert('Der kan ikke tilføjes documenter til en virksomhed.<br><br>Vælg en kontaktperson i stedet.', 'Kan ikke tilføje dokument!', null, null, null);
        return;
    }

    if (!tw.SelectedNode.Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan tilføje et dokument.', 'Kan ikke tilføje dokument!', null, null, null);
        return;
    }

    var nw = w.CreateWindow('MediaUploadNewFile.aspx?contactId=' + tw.SelectedNode.Uniqueidentifier, w);
    nw.Variables = null;
    nw.OnClose = _addDocument;
};

function _editDocument(sender, retval) {
    if (retval) {
        liHistory.SelectedItems[0].SetColumnName(retval[0], 5);
        liHistory.SelectedItems[0].SetColumnName(retval[1], 6);

        //store
        if (tw.SelectedNode != null)
            tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
    }
};

function _addDocument(sender, retval) {
    if (retval) {
        var div = document.createElement('DIV');
        div.innerHTML = retval[0];

        liHistory.AddItemsFromHTMLObjectAt(liHistory, div, 0);

        //store
        if (tw.SelectedNode != null)
            tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
    }
};

function deleteDocuments() {
    if (liHistory.SelectedItems.length == 0) {
        w.Alert('Vælg en fil som du ønsker at slette.', 'Kan ikke slette!', null, null, null);
        return;
    }
    var items = new Array();
    for (var i = 0; i < liHistory.SelectedItems.length; i++) {
        items.push(liHistory.SelectedItems[i].Uniqueidentifier);
    }

    var nw = w.CreateWindow('Delete.aspx?type=mediafile', w);
    nw.Variables = items;
    nw.OnClose = _deleteDocuments;
};

function _deleteDocuments(sender, retval) {
    if (retval) {
        if (retval[0] != 'ACCESS DENIED') {
            for (var i = liHistory.SelectedItems.length; i--; i > -1) {
                liHistory.SelectedItems[i].Remove();
            }
        }
        else {
            w.Alert('Du har ikke rettigheder til at slette dette dokument', 'Kan ikke slette!', null, null, null);
        }
    }
};

function getDocument(node) {
    if (!node) {
        if (lwdocuments.SelectedItems.length == 1)
            node = lwdocuments.SelectedItems[0];
        else {
            w.Alert('Vælg en fil du ønsker at hente', 'Kan ikke åbne!', null, null, null);
            return;
        }
    }

    window.open('mediaGetFile.aspx?fileId=' + node.Uniqueidentifier);
};

function evaluateUser(type) {

    var trUserIsSentToEvaluation = document.getElementById('trUserIsSentToEvaluation');
    if (trUserIsSentToEvaluation.style.display != 'none') {
        w.Alert('Der er allerede sendt til kontaktperson til evaluering for denne virksomhed.', 'Kan ikke sende til evaluering!', null, null, null);
        return;
    }
    if (!tw.SelectedNode.ParentTreeviewNode) {
        w.Alert('Virksomheder kan ikke sendes til evaluering.<br><br>Vælg en kontaktperson i stedet.', 'Kan ikke sende til evaluering!', null, null, null);
        return;
    }
    if (!tw.SelectedNode.Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan sende vedkommende til evaluering.', 'Kan ikke sende til evaluering!', null, null, null);
        return;
    }

    var d = '';
    var companyNode = tw.SelectedNode.ParentTreeviewNode;

    //company data not loaded yet
    if (companyNode.DataObject == null) {
        var postdata = [];
        postdata.push({ name: 'action', value: 'getcompany' });
        postdata.push({ name: 'companyid', value: companyNode.Uniqueidentifier });
        $.post("/ajax/smvEdit.aspx", postdata, function (data) {
            var cvr = '';
            var pnummer = '';
            if (data != '') {
                cvr = data.split('#')[0];
                pnummer = data.split('#')[1];
            }
            _allowEvaluateUser(cvr, pnummer, type);
        });
    } else {
        var cvr = $('#z_companies_1_CVR-nummer_1');
        var pnummer = $('#z_companies_1_P-Nummer_1');
        var cvr_value = '';
        var pnummer_value = '';
        if (cvr.length) {
            cvr_value = cvr.val();
        }
        if (pnummer.length) {
            pnummer_value = pnummer.val();
        }

        _allowEvaluateUser(cvr_value, pnummer_value, type);
    }
};

var evaluateType = '';
function _allowEvaluateUser(cvr_value, pnummer_value, type) {

    evaluateType = type;

    // regex
    var numberRegex = /^\d*$/;

    // company variable
    var cvr_sensitive = false;
    var pnummer_sensitive = false;
    var cvr = $('#z_companies_1_CVR-nummer_1');
    var pnummer = $('#z_companies_1_P-Nummer_1');

    // contact variable
    var cpr_value = '';
    var cpr_sensitive = false;
    var cpr = $('#z_contacts_1_CPR-nummer_1');

    if (cvr.length) {
        if (cvr.attr('sensitive') != undefined) {
            if (cvr.attr('sensitive') == '1') {
                cvr_sensitive = true;
            }
        }
    }
    if (pnummer.length) {
        if (pnummer.attr('sensitive') != undefined) {
            if (pnummer.attr('sensitive') == '1') {
                pnummer_sensitive = true;
            }
        }
    }
    if (cpr.length) {
        cpr_value = cpr.val();
        if (cpr.attr('sensitive') != undefined) {
            if (cpr.attr('sensitive') == '1') {
                cpr_sensitive = true;
            }
        }
    }

    if (cvr_value != '' && pnummer_value != '') {

        var cvrSensitiveValue = (cpr_value.indexOf('****') > -1);
        if (cvr_sensitive && cvrSensitiveValue) {
            // allow evaluate
        }
        else if (cvr_sensitive && !cvrSensitiveValue) {
            if (cvr_value.length == 8 && numberRegex.test(cvr_value)) {
                // allow evaluate
            } else {
                w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>CVR-nummer', 'Kan ikke sende til evaluering!', null, null, null);
                return;
            }
        }
        else if (!cvr_sensitive) {
            if (cvr_value.length == 8 && numberRegex.test(cvr_value)) {
                // allow evaluate
            } else {
                w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>CVR-nummer', 'Kan ikke sende til evaluering!', null, null, null);
                return;
            }
        }


        var pnrSensitiveValue = (pnummer_value.indexOf('****') > -1);
        if (pnummer_sensitive && pnrSensitiveValue) {
            // allow evaluate
        }
        else if (pnummer_sensitive && !pnrSensitiveValue) {
            if (pnummer_value.length == 10 && numberRegex.test(pnummer_value)) {
                // allow evaluate
            } else {
                w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>P-Nummer', 'Kan ikke sende til evaluering!', null, null, null);
                return;
            }
        }
        else if (!pnummer_sensitive) {
            if (pnummer_value.length == 10 && numberRegex.test(pnummer_value)) {
                // allow evaluate
            } else {
                w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>P-Nummer', 'Kan ikke sende til evaluering!', null, null, null);
                return;
            }
        }

    } else if (cvr_value == '' && pnummer_value == '' && cpr_value != '') {
        var cprSensitiveValue = (cpr_value.length > 5 && cpr_value.substring(cpr_value.length - 5, cpr_value.length) == '-****');
        if (cpr_sensitive && cprSensitiveValue) {
            // allow evaluate
        }
        else if (cpr_sensitive && !cprSensitiveValue) {
            if (!isCPR(cpr_value)) {
                w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>CPR-nummer', 'Kan ikke sende til evaluering!', null, null, null);
                return;
            }
        }
        else if (!cpr_sensitive && isCPR(cpr_value)) {
            w.Alert('Kan ikke gemmes, fordi følgende felt for kontakten ikke er udfyldt korrekt:<br/>CPR-nummer', 'Kan ikke sende til evaluering!', null, null, null);
            return;
        }
    } else {
        w.Alert('Der skal være enten CVR og P-nummer på Virksomheden eller CPR nummer på Kontaktpersonen.', 'Kan ikke sende til evaluering!', null, null, null);
        return;
    }

    w.Confirm('Vil du sende denne kontaktperson til brugerevaluering?<br><br><b>Husk:</b> Denne handling kan ikke fortrydes!', 'Send til evaluering?', 'images/sam32.png', _evaluateUser, null, null);
}

function _evaluateUser(sender, retval) {
    if (retval) {
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('Id', tw.SelectedNode.Uniqueidentifier);
        AjaxObject.encVar('sendToEvaluation', 'true');
        AjaxObject.encVar('evaluateType', evaluateType);

        core.DOM.DisableFields(document);

        AjaxObject.OnCompletion = function () { __evaluateUser(AjaxObject); };
        AjaxObject.OnError = function () { __evaluateUser(AjaxObject); };
        AjaxObject.OnFail = function () { __evaluateUser(AjaxObject); };
        AjaxObject.RunAJAX();
    }
};

function __evaluateUser(AjaxObject) {
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var div = document.createElement('DIV');
    div.innerHTML = result;

    spEvalName.innerHTMl = div.childNodes[0].innerHTML;
    spEvalBy.innerHTMl = div.childNodes[1].innerHTML;

    trUserIsSentToEvaluation.style.display = '';
    cmdEvaluate.Disable();
    cmdDriftevaluering.Disable();

    core.DOM.EnableFields(document);
};

function convertToSMV() {
    if (!tw.SelectedNode.Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan konvertere til en SMV.', 'Kan ikke konvertere til SMV!', null, null, null);
        return;
    }
    if (!getChosenCompanyNode().Uniqueidentifier) {
        w.Alert('Du skal gemme din kontaktpersons stamdata før du kan konvertere til en SMV.', 'Kan ikke konvertere til SMV!', null, null, null);
        return;
    }
    if (w.IsDirty) {
        w.Alert('Du skal gemme dine stamdata før du kan konvertere til en SMV.', 'Kan ikke konvertere til SMV!', null, null, null);
        return;
    }

    w.Confirm('Vil du konvertere denne POT virksomhed til en SMV virksomhed?<br><br><b>Checkliste inden du konveterer:</b><ul><li>Du har et CVR-nummer til virksomheden</li><li>Du har et P-nummer til virksomheden</li></ul><br><b>Efter konverteringen:</b><ul><li>Udfyld CVR-nummer og P-nummer på virksomhedsstamkortet</li><li>Gem stamkortet</li></ul><br>Hvis en eller flere punkter ikke udføres, forbliver virksomheden en POT virksomhed.', 'Konverter til SMV?', null, _convertToSMV);
};

function _convertToSMV(sender, retval) {
    if (retval) {
        /*var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('Id',tw.Nodes[0].Uniqueidentifier);
        AjaxObject.encVar('convertToSMV','true');
                
        core.DOM.DisableFields(document);
                
        AjaxObject.OnCompletion = function(){ __convertToSMV(AjaxObject); }; 
        AjaxObject.OnError = function(){ __convertToSMV(AjaxObject);};
        AjaxObject.OnFail = function(){  __convertToSMV(AjaxObject);};
        AjaxObject.RunAJAX();*/
        var node = getChosenCompanyNode();
        var id = tw.SelectedNode.Uniqueidentifier;
        if (node == tw.SelectedNode)
            id = node.Nodes[0].Uniqueidentifier;

        w.CreateWindow('SMVContactsEdit.aspx?id=' + id + '&asSMV=true');
        w.Close();

    }
};

function __convertToSMV(AjaxObject) {
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var nw = w.CreateWindow('SMVContactsEdit.aspx?id=' + tw.Nodes[0].Nodes[0].Uniqueidentifier);
    w.Close();
};

function _instantSearch(sender, result) {
    if (result) {
        if (result.length == 2) {
            w.Confirm('Dette vil lukke denne virksomhed og åbne den valgte fra søgningen.<br><br>Ingen ændringer vil blive gemt.', 'Åbn valgte virksomhed?', null, function (sender, val) { if (val) { document.getElementById('tblSavingHeader').innerHTML = 'Henter data...'; document.getElementById('tblSave').style.display = ''; document.location.href = 'SMVContactsEdit.aspx?companyId=' + result[0]; } }, null, null);
            return;
        }

        var div = document.createElement('div');
        div.innerHTML = result[0];

        while (div.firstChild) {
            var d = div.removeChild(div.firstChild);
            if (d.getAttribute('linkId') == 'txtNNEId') {
                if (tw.SelectedNode != null) {
                    if (tw.SelectedNode.DataObject != null) {
                        if (tw.SelectedNode.DataObject.childNodes != null) {
                            tw.SelectedNode.DataObject.childNodes[0].setAttribute('nneId', d.value);
                        }
                    }
                }
            }
            else {
                var ip = document.getElementById(d.getAttribute('linkId'));

                if (ip && d.value != '' && d.value != null) {
                    ip.value = d.value;
                    ip.onchange();
                }
            }
        }


        if (tw.SelectedNode.DataObject != null) {
            if (tw.SelectedNode.DataObject.childNodes != null) {
                var nneId = parseInt(tw.SelectedNode.DataObject.childNodes[0].getAttribute('nneId'), 10);
                if (nneId > 0)
                    cmdGoNNE.Enable();
                else
                    cmdGoNNE.Disable();
            }
            else
                cmdGoNNE.Disable();
        }
        else
            cmdGoNNE.Disable();


    }
};

function openReportedMeeting(node) {
    var args = new Array();

    args.push(node.Items[13]);
    args.push(document.getElementById('userId').value);

    var cpId = tw.SelectedNode.Uniqueidentifier;
    if (tw.SelectedNode.ParentTreeviewNode != null)
        cpId = tw.SelectedNode.ParentTreeviewNode.Uniqueidentifier;

    var nw = w.CreateWindow('ExchangeCalendarAppointment.aspx?companyId=' + cpId, w);
    nw.Variables = args;
};

function openMeeting(node) {

    var companyNode = getChosenCompanyNode();
    var cpId = companyNode.Uniqueidentifier;

    var cnId = '0';
    if (tw.SelectedNode.ParentTreeviewNode != null) {
        cnId = tw.SelectedNode.Uniqueidentifier;
    }

    var args = new Array();
    var nw = w.CreateWindow('adminCalendarEventEdit.aspx?type=1&companyId=' + cpId + '&contactId=' + cnId + '&id=' + node.Uniqueidentifier, w);
    nw.Variables = args;
    nw.OnClose = _editMeeting;
};

function createMeeting() {
    /*var cpId = tw.SelectedNode.Uniqueidentifier;
    if (tw.SelectedNode.ParentTreeviewNode != null)
        cpId = tw.SelectedNode.ParentTreeviewNode.Uniqueidentifier;

    var nw = w.CreateWindow('adminCalendarEventEdit.aspx?type=1&companyId=' + cpId, w);
    nw.Variables = null;
    nw.OnClose = _editMeeting;*/

    var companyNode = getChosenCompanyNode();
    var cpId = companyNode.Uniqueidentifier;

    var cnId = '0';
    if (tw.SelectedNode.ParentTreeviewNode != null) {
        cnId = tw.SelectedNode.Uniqueidentifier;
    }

    var nw = w.CreateWindow('adminCalendarEventEdit.aspx?type=1&companyId=' + cpId + '&contactId=' + cnId, w);
    nw.Variables = null;
    nw.OnClose = _editMeeting;
};

function _editMeeting(sender, retval) {

    alert(retval);
    if (retval) {
        var mt = null;

        //find correct meeting
        for (var i = 0; i < lwmeetings.Items.length; i++) {
            if (lwmeetings.Items[i].Uniqueidentifier.toString().endsWith(retval[0])) {
                mt = lwmeetings.Items[i];
                break;
            }
        }

        if (mt) {
            mt.SetColumnName(retval[1], 1);
            mt.SetColumnName(retval[2], 2);
        }
        else {
            var values = new Array();

            values.push(retval[1]);
            values.push(retval[2]);
            values.push(tw.SelectedNode.Name);
            values.push(retval[3]);
            values.push('');

            node = new IDC_ListviewItem(liHistory.Document, liHistory.Container, values);
            node.Icon16 = 'images/calendarNew.png';
            node.Uniqueidentifier = retval[0];

            liHistory.AddItemObjectAt(node, 0);

            //store
            if (tw.SelectedNode != null)
                tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
        }
    }

    if (retval) {
        var node = retval[1];
        if (node) {

            node.SetColumnName(retval[1], 2);
            node.SetColumnName(retval[2], 4);

            node.Icon16 = 'images/note.png';
            node.SetCurrentIcon('images/note.png');

            mt.SetColumnName(retval[1], 1);
            mt.SetColumnName(retval[2], 2);

            node.Icon16 = 'images/calendarNew.png';
            node.SetCurrentIcon('images/calendarNew.png');
        }
        else {

            var values = new Array();

            values.push('Meeting');
            values.push(retval[1]);
            values.push(retval[2]);
            values.push(tw.SelectedNode.Name);
            values.push(retval[3]);
            values.push('');
            values.push('');
            values.push('');
            values.push('');
            values.push('');
            values.push('');

            values.push(userId.value);
            values.push('');
            values.push('0');
            values.push(tw.SelectedNode.Uniqueidentifier);
            values.push(orgId.value);

            node = new IDC_ListviewItem(liHistory.Document, liHistory.Container, values);
            node.Icon16 = 'images/calendarNew.png';
            node.Uniqueidentifier = retval[0];

            liHistory.AddItemObjectAt(node, 0);

            //store
            if (tw.SelectedNode != null)
                tw.SelectedNode.DataObject.childNodes[1].innerHTML = liHistory.ItemsToHTML();
        }
    }
};

var meetingDeleteItems = null;
function deleteMeetings() {

    var items = new Array();
    var noDelete = '';

    for (var i = 0; i < liHistory.SelectedItems.length; i++) {
        //only delete own or organisation meeting if admin.
        if (liHistory.SelectedItems[i].Items[11] == userId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else
            noDelete += (noDelete == '' ? '' : '<br>') + liHistory.SelectedItems[i].Items[0];
    }

    if (items.length == 0) {
        w.Alert('Vælg de møder, du vil slette.', 'Kan ikke slette!', null, null, null);
        return;
    }

    meetingDeleteItems = items;

    if (noDelete != '') {
        w.Confirm('Følgende metting kan ikke fjernes på grund af manglende rettigheder:<br><br>' + noDelete + '<br><br>Fortsæt alligevel?', 'Kan ikke slette alle noter', __deleteMeetings);
        return;
    }
    else
        __deleteMeetings(null, true);
};

function __deleteMeetings(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('Delete.aspx?type=admincalendarevent', w);
        nw.Variables = meetingDeleteItems;
        nw.OnClose = _deleteMeetings;
    }
};

function _deleteMeetings(sender, retval) {
    if (retval) {
        for (var i = liHistory.SelectedItems.length; i--; i > -1) {
            //only delete own or organisation meeting if admin.
            if (liHistory.SelectedItems[i].Items[11] == userId.value)
                liHistory.SelectedItems[i].Remove();
            else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
                liHistory.SelectedItems[i].Remove();
        }
    }
};

// delete meeting reporting
var meetingReportingDeleteItems = null;
function deleteMeetingReporting() {

    var items = new Array();
    var noDelete = '';

    for (var i = 0; i < liHistory.SelectedItems.length; i++) {

        //only delete own or organisation meeting if admin.
        if (liHistory.SelectedItems[i].Items[11] == userId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
            items.push(liHistory.SelectedItems[i].Uniqueidentifier);
        else
            noDelete += (noDelete == '' ? '' : '<br>') + liHistory.SelectedItems[i].Items[0];
    }

    if (items.length == 0) {
        w.Alert('Vælg de indrapporterede møder, du vil slette.', 'Kan ikke slette!', null, null, null);
        return;
    }

    meetingReportingDeleteItems = items;

    if (noDelete != '') {
        w.Alert('Indrapporterede møder kan ikke fjernes på grund af manglende rettigheder.');
        return;
    }
    else {
        __deleteMeetingReporting(null, true);
    }
};

function __deleteMeetingReporting(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('Delete.aspx?type=meetingreporting', w);
        nw.Variables = meetingReportingDeleteItems;
        nw.OnClose = _deleteMeetingReporting;
    }
};

function _deleteMeetingReporting(sender, retval) {
    if (retval) {
        for (var i = liHistory.SelectedItems.length; i--; i > -1) {
            //only delete own or organisation meeting if admin.
            if (liHistory.SelectedItems[i].Items[11] == userId.value)
                liHistory.SelectedItems[i].Remove();
            else if (isAdmin.value == '1' && liHistory.SelectedItems[i].Items[15] == orgId.value)
                liHistory.SelectedItems[i].Remove();
        }
    }
};

function showtransferText() {
    var od = document.getElementById('spCompanyIsTransferredFull');
    var oo = document.getElementById('spCompanyIsTransferred');

    //alert(oo.scrollHeight + " x " + oo.scrollWidth);

    od.innerHTML = oo.innerHTML;
    od.style.height = '';
    od.style.display = '';
    od.style.overflowY = 'hidden';


    if (od.offsetHeight > 250) {
        od.style.height = '250px';
        od.style.overflowY = 'scroll';
    }
    //alert(oo.parentNode.offsetWidth + ' / ' + od.offsetWidth);
    od.style.width = oo.parentNode.offsetWidth - (core.IsIE ? 0 : 21);

};

function hidetransferText() {
    document.getElementById('spCompanyIsTransferredFull').style.display = 'none';
};

function glueContacts() {
    var node = tw.SelectedNode;
    var pw = w.CreateWindow('PleaseWait.aspx');
    var nw = w.CreateWindow('SMVContactsGlueContacts.aspx?contactId=' + node.Uniqueidentifier + '&companyId=' + node.ParentTreeviewNode.Uniqueidentifier, w);
    nw.Variables = pw;
    //nw.ShowDialog();
    nw.OnClose = __glueContacts;
};

function __glueContacts(sender, retval) {
    if (retval) {
        w.WindowManager.CreateWindow('SMVContactsEdit.aspx?id=' + retval);
        w.IsDirty = false;
        w.Close();
    }
};

//Opens Company Details in NNE
function openNNE() {

    var tmpNode = tw.SelectedNode;
    if (tmpNode) {
        if (tmpNode.ParentTreeviewNode)
            tmpNode = tmpNode.ParentTreeviewNode;

        if (tmpNode.DataObject != null) {
            if (tmpNode.DataObject.childNodes != null) {
                var nneId = parseInt(tmpNode.DataObject.childNodes[0].getAttribute('nneId'), 10);
                if (nneId > 0)
                    parent.GoNNECompanyProfile(nneId);
            }
        }
        else if (tmpNode.nneId) {
            var nneId = parseInt(tmpNode.nneId, 10);
            if (nneId > 0)
                parent.GoNNECompanyProfile(nneId);
        }
    }
};

function createNewAVN() {
    var ctId = 0;
    var cpId = 0;

    if (tw.SelectedNode.ParentTreeviewNode != null)
        ctId = tw.SelectedNode.Uniqueidentifier;
    else
        cpId = tw.SelectedNode.Uniqueidentifier;

    if (parseInt(ctId, 10) == 0 && parseInt(cpId, 10) == 0) {
        w.Alert('Du skal gemme din SMV/POT kontaktperson før der kan tilknyttes Avancerede noter på den!', 'Der skal gemmes!');
        return;
    }

    var nw = w.CreateWindow('SMVCreateAVN.aspx?isCompany=' + (tw.SelectedNode.ParentTreeviewNode == null ? "true" : "") + '&ctId=' + ctId + '&cpId=' + cpId, w);
    nw.Variables = null;
    nw.OnClose = __createNewAVN;
};

function __createNewAVN(sender, retval) {
    if (retval) {

        var v = new Array();
        v.push(retval[0]);
        v.push(retval[1]);

        var i = new Array();
        i.push(retval[2][0]);
        i.push(retval[2][1]);

        v.push(i);

        var ctId = retval[3][1];
        var cpId = retval[3][0];

        //        if (tw.SelectedNode.ParentTreeviewNode != null)
        //            ctId = tw.SelectedNode.Uniqueidentifier;
        //        else
        //            cpId = tw.SelectedNode.Uniqueidentifier;

        if (parseInt(ctId, 10) == 0 && parseInt(cpId, 10) == 0) {
            w.Alert('Du skal gemme din SMV/POT kontaktperson før der kan tilknyttes Avancerede noter på den!', 'Der skal gemmes!');
            return;
        }

        var nw = w.CreateWindow('SMVEditAVN.aspx?typeId=' + retval[0] + '&cpId=' + cpId + '&ctId=' + ctId, w);

        nw.Variables = v;
        nw.OnClose = __editAVN;
    }
};

function __editAVN(sender, retval) {
    if (retval) {

        var item = liAVN.getItemByUniqueidentifier(retval[0]);

        if (item == null) {
            var subItems = new Array();

            subItems.push(retval[7]);
            subItems.push(retval[8]);
            subItems.push(retval[6]);
            subItems.push('1');

            subItems.push(retval[6]);
            subItems.push(retval[9]);
            subItems.push(retval[3]);
            subItems.push(retval[4]);
            subItems.push('');
            subItems.push('');
            subItems.push(tw.SelectedNode.Name);
            subItems.push(document.getElementById('userId').value);

            if (liAVN.Items.length > 0)
                liAVN.AddItemAt(retval[0], retval[1], ['Images/SharedIcons/16x16/' + retval[2], 'Images/SharedIcons/32x32/' + retval[2]], subItems, 0);
            else
                liAVN.AddItem(retval[0], retval[1], ['Images/SharedIcons/16x16/' + retval[2], 'Images/SharedIcons/32x32/' + retval[2]], subItems);
        }
        else {
            item.SetName(retval[1]);
            item.SetName(retval[3], 9);
            item.SetName(retval[4], 10);
        }

        //store
        if (tw.SelectedNode != null) {
            tw.SelectedNode.DataObject.childNodes[3].innerHTML = liAVN.ItemsToHTML();
        }
    }
};

function AVNDelete(node) {
    //    w.Alert('Det er i øjeblikket ikke muligt at slette Projektnoter.', 'Fejl!', 'Images/AVNNote32.png');
    //    return;

    if (node == null) {
        if (liAVN.SelectedItems.length == 0) {
            w.Alert('Marker en Projektnote du vil slette!', 'Marker en AVN!');
            return;
        }

        var items = new Array();
        var noDelete = '';
        var uid = document.getElementById('userId').value;

        for (var i = 0; i < liAVN.SelectedItems.length; i++) {

            if (uid != liAVN.SelectedItems[i].Items[12]) {
                noDelete += (noDelete == '' ? '' : '<br>') + liAVN.SelectedItems[i].Items[0];
            }
            else
                items.push(liAVN.SelectedItems[i].Uniqueidentifier);
        }
        if (items.length == 0) {
            w.Alert('Du har ikke rettigheder til at slette den eller de projektnoter du har valgt.', 'Kan ikke slette!', null, null, null);
            return;
        }

        noteDeleteItems = items;

        if (noDelete != '') {
            w.Confirm('Følgende projektnoter kan ikke slettes på grund af manglende rettigheder:<br><br>' + noDelete + '<br><br>Fortsæt alligevel?', 'Kan ikke slette alle projektnoter', __AVNDelete);
            return;
        }
        else
            __AVNDelete(null, true);

    }
};

function __AVNDelete(sender, retval) {
    if (retval) {
        var nw = w.CreateWindow('Delete.aspx?type=avnentity', w);
        nw.Variables = noteDeleteItems;
        nw.OnClose = _AVNDelete;
    }
}

function _AVNDelete(sender, retval) {
    if (retval) {
        var uid = document.getElementById('userId').value;
        for (var i = liAVN.SelectedItems.length - 1; i >= 0; i--) {
            if (uid == liAVN.SelectedItems[i].Items[12]) {
                liAVN.SelectedItems[i].Remove();
            }
        }

        //store
        if (tw.SelectedNode != null) {
            tw.SelectedNode.DataObject.childNodes[3].innerHTML = liAVN.ItemsToHTML();
        }
    }
};

function AVNEdit(node) {
    if (node == null) {
        if (liAVN.SelectedItems.length == 0) {
            w.Alert('Marker en AVN du vil åbne!', 'Marker en AVN!');
            return;
        }
        node = liAVN.SelectedItems[0];
    }

    var ids = node.Uniqueidentifier.split('_');

    var v = new Array();

    v.push(ids[0]);
    v.push(node.Items[0]);

    var i = new Array();
    i.push(node.Icon16);
    i.push(node.Icon32);

    v.push(i);
    
    var nw = w.CreateWindow('SMVEditAVN.aspx?typeId=' + ids[0] + '&Id=' + ids[1], w);

    nw.Variables = v;
    nw.OnClose = __editAVN;
};

//Detailed listview item
var IDC_ListviewItemAVNStyle = function (listview, item) {
    this.ViewName = 'AVN Listeview item';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = document.createElement('TABLE');
    this.Control.cellPadding = 0;
    this.Control.cellSpacing = 0;
    this.Control.border = 0;
    this.Control.className = 'avn_' + item.Items[1];

    this.Control.clsNormal = 'avn_' + item.Items[1];
    this.Control.clsHover = 'avn_' + item.Items[1] + '_h';
    this.Control.clsSelected = 'avn_' + item.Items[1] + '_s';

    if (item.Items[4] == '0') {
        this.Control.style.opacity = 0.5;
        this.Control.style.filter = 'alpha(opacity=50)';
    }

    this.Control.style.width = '100%';

    this.Control.tr = this.Control.insertRow(-1);

    this.Control.trDetails = this.Control.insertRow(-1);
    this.Control.trDetails.style.display = 'none';
    this.Control.trDetails.td = this.Control.trDetails.insertCell(-1);
    this.Control.trDetails.td.className = 'avnListviewDetailsFrame_' + item.Items[1];

    cr = this.Control.tr.insertCell(-1).appendChild(this.createDiv(item.Items[0], 165, 'avnListviewIcon', 'url(' + listview.CurrentURL + item.Icon16 + ')', listview.ColumnHeaders[0].TextAlign));
    var tdPM = this.Control.tr.insertCell(-1);
    tdPM.align = 'right';
    var imgPM = document.createElement('IMG');
    tdPM.appendChild(imgPM);
    imgPM.src = 'Images/navigatorDown.gif';
    imgPM.trDetails = this.Control.trDetails;
    imgPM.onclick = function () {
        if (this.trDetails.style.display == '') {
            imgPM.src = 'Images/navigatorDown.gif';
            this.trDetails.style.display = 'none';
        }
        else {
            imgPM.src = 'Images/navigatorUp.gif';
            this.trDetails.style.display = '';
        }
    };

    this.Control.trDetails.td.colSpan = 2;

    //Set tooltip
    var tp = item.Items[5] + '\r\n' +
        item.Items[2] + '\r\n\r\n' +
        'Oprettet d.:\t' + item.Items[7] + '\r\n' +
        'Oprettet af:\t' + item.Items[8] + '\r\n' +
        //(item.Items[9] != ''  ? '\r\nSidst opdateret d.:\t' + item.Items[9] + '\r\nSidst opdateret af:\t' + item.Items[10] + '\r\n' : '\r\n') +
        //** Har rettet det så den tjekker på 31-12-2999 00:00, ESCRM-196/195
        ((item.Items[9] != '' && item.Items[9] != '31-12-2999 00:00') ? '\r\nSidst opdateret d.:\t' + item.Items[9] + '\r\nSidst opdateret af:\t' + item.Items[10] + '\r\n' : '\r\nSidst opdateret d.:\r\nSidst opdateret af:\r\n') +
        '\r\nGemt på:\t' + item.Items[11];

    var tdd = '<table border="0" cellpadding="0" cellspacing="0">' +
        '<tr><td valign="top" colspan="2" class="avnListviewDetails_' + item.Items[1] + '">' + item.Items[5] + '</td></tr>' +
        '<tr><td valign="top" colspan="2" class="avnListviewDetails_' + item.Items[1] + '">' + item.Items[2] + '</td></tr>' +
        '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Oprettet d.:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[7] + '</td></tr>' +
        '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Oprettet af:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[8] + '</td></tr>' +
        //(item.Items[9] != '' ? '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret d.:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[9] + '</td></tr><tr><td nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret af:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[10] + '</td></tr>' : '') +
        //** Har rettet det så den tjekker på 31-12-2999 00:00, ESCRM-196/195
        ((item.Items[9] != '' && item.Items[9] != '31-12-2999 00:00') ? '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret d.:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[9] + '</td></tr><tr><td nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret af:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[10] + '</td></tr>' : '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret d.:</td><td nowrap class="avnListviewDetailsRight_' + item.Items[1] + '">&nbsp;</td></tr><tr><td  nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret af:</td><td nowrap class="avnListviewDetailsRight_' + item.Items[1] + '">&nbsp;</td></tr>') +
        '<tr><td valign="top" class="avnListviewDetailsLeft_' + item.Items[1] + '">Gemt på:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[11] + '</td></tr></table>';

    this.Control.trDetails.td.innerHTML = tdd;

    this.Control.title = tp;

    //EVENTS
    this.SelectionChanged = function (item) {
        item.Control.className = (item.IsSelected ? item.Control.clsSelected : item.Control.clsNormal);

        //this.Control.trDetails.style.display = (item.IsSelected ? '' : 'none');
    };

    this.OnMouseOver = function (item) {
        if (!item.IsSelected) item.Control.className = item.Control.clsHover;
    };

    this.OnMouseOut = function (item) {
        if (!item.IsSelected) item.Control.className = item.Control.clsNormal;
    };

    this.OnTextChanged = function (item) {

        //Set tooltip
        var tp = item.Items[5] + '\r\n' +
            item.Items[2] + '\r\n\r\n' +
            'Oprettet d.:\t' + item.Items[7] + '\r\n' +
            'Oprettet af:\t' + item.Items[8] + '\r\n' +
            (item.Items[9] != '' ? '\r\nSidst opdateret d.:\t' + item.Items[9] + '\r\nSidst opdateret af:\t' + item.Items[10] + '\r\n' : '\r\n') +
            '\r\nGemt på:\t' + item.Items[11];

        var tdd = '<table border="0" cellpadding="0" cellspacing="0">' +
            '<tr><td valign="top" colspan="2" class="avnListviewDetails_' + item.Items[1] + '">' + item.Items[5] + '</td></tr>' +
            '<tr><td valign="top" colspan="2" class="avnListviewDetails_' + item.Items[1] + '">' + item.Items[2] + '</td></tr>' +
            '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Oprettet d.:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[7] + '</td></tr>' +
            '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Oprettet af:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[8] + '</td></tr>' +
            (item.Items[9] != '' ? '<tr><td valign="top" nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret d.:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[9] + '</td></tr><tr><td nowrap class="avnListviewDetailsLeft_' + item.Items[1] + '">Opdateret af:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[10] + '</td></tr>' : '') +
            '<tr><td valign="top" class="avnListviewDetailsLeft_' + item.Items[1] + '">Gemt på:</td><td class="avnListviewDetailsRight_' + item.Items[1] + '">' + item.Items[11] + '</td></tr></table>';

        this.Control.trDetails.td.innerHTML = tdd;

        this.Control.title = tp;

        item.Control.tr.childNodes[0].childNodes[0].innerHTML = item.Items[0];

        //        for (var i = 0; i < item.Items.length && i < item.Control.tr.childNodes.length; i++) {
        //            item.Control.tr.childNodes[i].childNodes[0].innerHTML = item.Items[i];
        //        }
    };

    this.OnIconChanged = function (item) {
        item.Control.tr.childNodes[0].childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon16 + ')';
    };

    return this;
};

IDC_ListviewItemAVNStyle.prototype.createDiv = function (value, w, cls, bgimg, textalign) {
    var d = document.createElement('DIV');
    d.innerHTML = value;
    d.className = cls;
    d.style.width = w;
    d.style.textAlign = textalign;

    //Clean HTML Tags
    //d.title = value.replace(/<(.*?)>/g, '');

    if (bgimg)
        d.style.backgroundImage = bgimg;

    return d;
};

// Classified items
function Show(object) {
    var button = $(object);
    var orgsrc = button.attr('src');
    var targetfieldid = object.getAttribute('data-target-id');
    var targetfieldname = object.getAttribute('data-target-name');
    var targetfieldtype = object.getAttribute('data-target-type');
    var contactid = $('#contactID').val();
    var companyid = $('#companyID').val();
    w.Confirm('Dette felt er et klassificeret felt. Hvis du vælger OK vil det blive logget at du har set indholdet af feltet.', 'Information', null, function (sender, val) {
        if (val) {
            button.attr('src', '/images/busy.gif');
            var postdata = [];
            postdata.push({ name: 'action', value: 'viewclassified' });
            postdata.push({ name: 'targetfieldid', value: targetfieldid });
            postdata.push({ name: 'targetfieldname', value: targetfieldname });
            postdata.push({ name: 'targetfieldtype', value: targetfieldtype });
            postdata.push({ name: 'contactid', value: contactid });
            postdata.push({ name: 'companyid', value: companyid });
            $.post("/ajax/smvEdit.aspx", postdata, function (data) {
                button.attr('src', orgsrc);
                //button.attr('disabled', 'disabled');
                $("[id='" + targetfieldname + "']").removeAttr("disabled");
                $("[id='" + targetfieldname + "']").val(data);
                $("[id='" + targetfieldname + "']").focus();
            });
        } else {
            return;
        }
    }, null, null);
}

function Expiration(obj) {
    var node = getChosenCompanyNode();
    var positivefieldid = $(obj).data('field-id');
    var objecttype = $(obj).data('object-type');
    var screenrefkey = $(obj).data('screen-ref-key');
    var companyid = node.Uniqueidentifier;
    if (companyid == null) {
        w.Alert('Du skal gemme virksomhedens stamdata før du vælger kontaktfeltet', 'Kan ikke gemme!', null, null, null);
        return;
    }
    else {
        var arg = '?type=' + objecttype + '&cpid=' + companyid + "&objectid=" + companyid + "&screenrefkey=" + screenrefkey + "&positivefieldid=" + positivefieldid;
        var nw = w.CreateWindow('Expiration.aspx' + arg, w);
        nw.Variables = null;
        nw.OnClose = _createExpiration;
    }
};

function _createExpiration(sender, retval) {
    if (retval) { }
}

function pickDateToControl(id) {
    //datePicker
    currentDateObject = document.getElementById(id);

    var sl = document.getElementById('companyFields');
    if (sl.style.display != '')
        sl = document.getElementById('contactFields');

    var pos = localCore.DOM.GetObjectPosition(currentDateObject);

    var dateparts = currentDateObject.value.split(/[.,\/ -]/);
    var dt = new Date();
    var day = dt.getDate();
    var month = dt.getMonth();
    var year = dt.getFullYear();

    if (dateparts.length == 3) {
        day = parseInt(dateparts[0], 10);
        month = parseInt(dateparts[1], 10) - 1;
        year = parseInt(dateparts[2], 10);
    }

    datePicker.SetCurrentDate(day, month, year);

    if (w.WindowState != 2)
        datePicker.Show(pos[0] + w.Left - sl.scrollLeft, pos[1] + w.Top + 74 - sl.scrollTop);
    else
        datePicker.Show(pos[0] - sl.scrollLeft, pos[1] + 74 - sl.scrollTop);
};

function OnDatePicked(value) {
    if (currentDateObject != null) {
        var y = value.getFullYear().toString();
        var m = (value.getMonth() + 1).toString();
        var d = value.getDate().toString();
        if (m.length == 1) m = '0' + m;
        if (d.length == 1) d = '0' + d;
        currentDateObject.value = d + '-' + m + '-' + y;
        currentDateObject.onchange();
    }
};

function ShowAgreement() {
    if (tw.SelectedNode.ParentTreeviewNode) {
        if (tw.SelectedNode.Uniqueidentifier == null) {
            w.Alert('Du skal gemme din kontakts stamdata, før du kan tilføje kontraktsaftale.', 'Kan ikke oprette ny kontraktsaftale!', null, null, null);
        }
        else {
            var items = new Array();
            var nw = w.CreateWindow('AgreementContacts.aspx?id=' + tw.SelectedNode.Uniqueidentifier, w);
            nw.Variables = items;
            nw.OnClose = __showAgreement;
        }
    }
};

function __showAgreement(sender, retval) {
    if (tw.SelectedNode.ParentTreeviewNode) {
        _loadContact(tw.SelectedNode);
    }
};

//** Funktion der undersøger om evaluerings knapperne skal kunne vises, ESCRM-111/112
function CanEvaluationButtonsBeShown() {
    var elems = document.body.getElementsByTagName("*");
    try {
        //** Find UserId, CVR og P-nummer
        var userId = document.getElementById("userId").value;
        var companyId = document.getElementById("companyID").value;
        //var contactId = elems.contactID.defaultValue;

        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('evaluationButtons', 'true');
        AjaxObject.encVar('userID', userId);
        AjaxObject.encVar('companyID', companyId);
        //AjaxObject.encVar('contactID', contactId);
        //** Vi bruger en ny variable
        AjaxObject.encVar('contactID', valgtContactId);

        AjaxObject.OnCompletion = function () { SetEvaluationButtons('Completion', AjaxObject); };
        AjaxObject.OnError = function () { SetEvaluationButtons('OnError', AjaxObject); };
        AjaxObject.OnFail = function () { SetEvaluationButtons('OnFail', AjaxObject); };
        AjaxObject.RunAJAX();
    }
    catch (err) {
        alert('Der opstod en fejl: ' + err);
    }
};

//** Enable/Disable buttons, ESCRM-111/112
function SetEvaluationButtons(message, AjaxObject) {
    try {
        if (message == 'Completion') {
            //alert(AjaxObject.Response.substring(0, (AjaxObject.Response.indexOf('<'))));
            //** Har vaekst/drift i nuværende år
            if (AjaxObject.Response.substring(0, (AjaxObject.Response.indexOf('<'))) != '1') {
                cmdEvaluate.Disable();
                cmdDriftevaluering.Disable();
            }
            else if (AjaxObject.Response == 'fail') {
                cmdEvaluate.Disable();
                cmdDriftevaluering.Disable();
            }
        }
    }
    catch (err) {
        alert(err);
    }
};

//** Function til at sætte smv aom favorit, ESCRM-13/97
function SetFavorite() {
    var elems = document.body.getElementsByTagName("*");
    try {
        //** Find UserId, CVR og P-nummer
        var userId = document.getElementById("userId").value;
        var companyId = $('#currentCompanyID2').val(); //document.getElementById("companyID").value;
        var contactId = elems.contactID.defaultValue;

        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/smvEdit.aspx';
        AjaxObject.encVar('favorite', 'true');
        AjaxObject.encVar('userID', userId);
        AjaxObject.encVar('companyID', companyId);
        AjaxObject.encVar('contactID', contactId);

        AjaxObject.OnCompletion = function () { ChangeFavorite('Completion', AjaxObject); };
        AjaxObject.OnError = function () { ChangeFavorite('OnError', AjaxObject); };
        AjaxObject.OnFail = function () { ChangeFavorite('OnFail', AjaxObject); };
        AjaxObject.RunAJAX();
    }
    catch (err) {

    }
};

//** Function til at sætte smv som favorit, ESCRM-13/97
function ChangeFavorite(message, AjaxObject) {
    try {
        if (message == 'Completion') {
            //** Har sat favorit
            if (AjaxObject.Response.substring(0, 7) == 'created') {
                //** Sæt ikon til on
                cmdFavorit.SetTextAndIcon('Fjern som favorit', 'images/favoriteSMVOn.png');
                //** Vis favorite widget på root, ESCRM-118/121
                parent.loadFavoriteWidget();
            }
            //** Har fjernet favorit
            if (AjaxObject.Response.substring(0, 7) == 'deleted') {
                //** Sæt ikon til off
                cmdFavorit.SetTextAndIcon('Sæt som favorit', 'images/favoriteSMVOff.png');
                //** Fjern favorite widget på root, ESCRM-118/121
                parent.unLoadFavoriteWidget();
            }
        }
    }
    catch (err) {
        alert(err);
    }
};

//** Kaldes efter RunAjax, ESCRM-13/97
function GetFavorite(message, node, AjaxObject) {
    try {
        if (message == 'Completion') {
            //** Har sat favorit
            if (AjaxObject.Response.substring(0, 1) == '1') {
                //** Sæt ikon til on
                cmdFavorit.SetTextAndIcon('Fjern som favorit', 'images/favoriteSMVOn.png');
            }
            //** Har fjernet favorit
            if (AjaxObject.Response.substring(0, 1) == '0') {
                //** Sæt ikon til off
                cmdFavorit.SetTextAndIcon('Sæt som favorit', 'images/favoriteSMVOff.png');
            }
        }
    }
    catch (err) {
        alert(err);
    }
};
var lw = null;
var hs = null;
var vs = null;
var tb = null;
var wm = null;
var io = null;
var dd = null;

var toolbar = null;
var cmdEdit = null;
var cmdDelete = null;
var cmdSend = null;
var cmdMove = null;
var cmdPrev = null;
var cmdNext = null;
var cmdApprove = null;
var cmdDecline = null;
var cmdCounterPost = null;

var pb = null;
var w = null;
var core = null;
var localCore = null;

var timeEditor = null;
var projectEditor = null;
var timeTextEditor = null;

var floatFormat = null;
var projectContextMenu = null;
var myWeekDate = null;
var divProjects = null;
var tblProjects = null;
var divWeekView = null;

var trToolbar;
var trInfo;
var trHeaders;
var trSums;

var projectsToDelete = new Array();
var objTxtSizeMeasurer = null;

function buildContextMenu() {
    divProjects = document.getElementById('divProjects');
    projectContextMenu = w.CreateContextMenu(localCore);
    var obj = document.getElementById('divProjects');
    while (divProjects.hasChildNodes()) {
        var o = divProjects.removeChild(divProjects.firstChild);
        var sub = projectContextMenu.AddItem(o.getAttribute('projectname'), '', null);
        while (o.hasChildNodes()) {
            var o2 = o.removeChild(o.firstChild);
            var node = sub.AddItem(o2.innerHTML, '', function (ctx) { SetSelectedProject(ctx.pid, ctx.sid, ctx.pserial, ctx.cserial, ctx.projectname); });
            node.pid = o2.getAttribute('pid');
            node.sid = o2.getAttribute('cid');
            node.pserial = o2.getAttribute('pserial');
            node.cserial = o2.getAttribute('cserial');
            node.projectname = o2.innerHTML;
        }
    }
    //w.BindMouseContextMenuToObjectAndChildren(document.body, projectContextMenu, localCore);
};

function SetSelectedProject(pid, sid, pserial, cserial, name) {
    var pn = projectEditor.CurrentObject;
    pn.setAttribute('serialnoid', pid);
    pn.setAttribute('casenoid', sid);
    pn.setAttribute('pserial', pserial);
    pn.setAttribute('cserial', cserial);
    pn.setAttribute('projectname', name);

    projectEditor.Td1.innerHTML = '<DIV style="width:398px;white-space:nowrap;height:16px;overflow:hidden;">' + (pn.getAttribute('projectname') || '- Vælg en aktivitet -') + '</DIV>';
    projectEditor.Td1.title = (pn.getAttribute('projectname') || '- Vælg en aktivitet -');

    fixCaseNumberDots(projectEditor.Td1, pn);

    if (tblProjects.childNodes[0].childNodes.length - 1 == pn.Y) AddRow();

    //Set start of editing!
    if (document.getElementById('txtReadOnly').value == '') {
        var o = getNextEditObjectFromCoords(1, pn.Y);
        if (o) editAtThisPoint(o);
    }
    if (!w.IsDirty) w.IsDirty = true;

};

function fixCaseNumberDots(td, pn) {
    var t = (pn.getAttribute('projectname') || '- Vælg en aktivitet -');
    var w = GetTextWidth(t, 'Tahoma', '12px');
    if (w > 398) {
        //        td.style.backgroundImage = 'url(images/note.png)';
        //        td.style.backgroundRepeat = 'no-repeat';
        //        td.style.backgroundPosition = 'right center';
        while (w > 390) {
            t = t.substring(0, t.length - 1);
            td.getElementsByTagName('DIV')[0].innerHTML = t + '...';
            var w = GetTextWidth(t, 'Tahoma', '12px');
        }

    }
    else {
        //td.style.backgroundImage = '';
    }

};

function GetTextWidth(text, font, size) {
    if (objTxtSizeMeasurer == null) {
        objTxtSizeMeasurer = document.createElement('DIV');
        objTxtSizeMeasurer.style.left = -10000;
        objTxtSizeMeasurer.style.top = -10000;
        objTxtSizeMeasurer.style.whiteSpace = 'nowrap';
        objTxtSizeMeasurer.style.background = 'red';
        objTxtSizeMeasurer.style.position = 'absolute';
        document.body.appendChild(objTxtSizeMeasurer);
    }

    objTxtSizeMeasurer.style.fontFamily = font;
    objTxtSizeMeasurer.style.fontSize = size;
    objTxtSizeMeasurer.innerHTML = text;

    return objTxtSizeMeasurer.offsetWidth;
};

function init() {
    w = window.frameElement.IDCWindow;
    w.UseDirtyManager = true;
    w.DirtyMessage = 'Der er lavet ændringer i din ugeregistrering.<br><br>Vil du lukke vinduet uden at gemme?';
    w.DirtyTitle = 'Luk uden at gemme?';

    core = w.WindowManager.Core;
    localCore = new IDC_Core();
    floatFormat = new IDC_LocalizationSettings(',', '.', 2);

    //Saving progressbar
    pb = new Progressbar(document, document.getElementById('progressHere'), 266);

    pb.OnAnimateDoneEvent = null;


    wm = w.WindowManager;

    w.Icons = ['images/hour.png', 'images/hour32.png'];
    w.Name = (document.getElementById('txtId').value == '' ? 'Nyt' : 'Rediger') + ' timeregnskab';

    w.Width = w.WindowManager.Desktop.Background.offsetWidth * 0.66;
    w.Height = w.WindowManager.Desktop.Background.offsetHeight * 0.66;
    w.CenterScreen();
    w.OnResized = OnResize;
    w.OnMaximize = OnResize;
    w.OnRestore = OnResize;

    w.MinWidth = 832;
    w.MinHeight = 190;

    //Set as Maximized
    w.WindowState = 2;

    w.Show();
    w.TaskbarButton.SetHoveringColor('green');

    tblProjects = document.getElementById('tblProjects');
    divWeekView = document.getElementById('divWeekView');
    trToolbar = document.getElementById('trToolbar');
    trInfo = document.getElementById('trInfo');
    trHeaders = document.getElementById('trHeaders');
    trSums = document.getElementById('trSums');

    //COntextMenu
    buildContextMenu();

    //toolbar 
    tb = new Toolbar(document, document.getElementById('toolbar'));

    //admin toolbar
    if (document.getElementById('isAdmin').value == 'true') {
        cmdApprove = tb.AddButton('Godkend', 'images/okay.png', function () { locateNextEditPoint(); SetWeekStatus(true); });
        cmdDecline = tb.AddButton('Afvis', 'images/cancel.png', function () { locateNextEditPoint(); SetWeekStatus(false); });
        tb.AddSeperator();
        cmdSave = tb.AddButton('Gem', 'images/save.gif', function () { locateNextEditPoint(); SaveWeek(); });
        tb.AddSeperator();
        cmdDelete = tb.AddButton('Slet række', 'images/deleteLine.png', function () { deleteRow(); });
        cmdReset = tb.AddButton('Ryd "0" rækker', 'images/resetLine.png', function () { removeZeroRows(); });
        tb.AddSeperator();
        cmdCounterPost = tb.AddButton('Posteringsrettelse', 'images/counterPost.png', function () { counterPostHours(); });
        cmdCounterPost.Disable();
        tb.AddSeperator();
        lblCurrentWeek = tb.AddLabel('Uge ' + document.getElementById('txtWeekNumber').value + '/' + document.getElementById('txtYear').value);
    }
    else {
        cmdSave = tb.AddButton('Gem', 'images/save.gif', function () { SaveWeek(); });
        tb.AddSeperator();
        cmdDelete = tb.AddButton('Slet række', 'images/deleteLine.png', function () { deleteRow(); });
        cmdReset = tb.AddButton('Ryd "0" rækker', 'images/resetLine.png', function () { removeZeroRows(); });
        tb.AddSeperator();
        cmdSend = tb.AddButton('Send til godkendelse', 'images/hourSendToApproval.png', function () { locateNextEditPoint(); sendToApproval(); });
        tb.AddSeperator();
        cmdMove = tb.AddButton('Skift ugenummer', 'images/moveWeek.png', function () { locateNextEditPoint(); moveWeek(); });

        tb.AddSeperator();
        cmdPrev = tb.AddTinyButton('En uge tilbage', 'images/buttonLeft.png', function () { locateNextEditPoint(); prevWeek(); });
        lblCurrentWeek = tb.AddLabel('Uge ' + document.getElementById('txtWeekNumber').value + '/' + document.getElementById('txtYear').value);
        cmdNext = tb.AddTinyButton('En uge frem', 'images/buttonRight.png', function () { locateNextEditPoint(); nextWeek(); });
    }
    tb.AddSeperator();
    cmdPrint = tb.AddButton('Udskriv', 'images/print.png', function () { locateNextEditPoint(); printHourUsage(); });

    //Set Toolbar as Context Menu
    w.ToolbarContextMenu = tb;


    OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());

    timeEditor = document.createElement('INPUT');
    timeEditor.className = 'hourUsageDayEditBox';

    timeTextEditor = document.createElement('TR');
    timeTextEditor.td = document.createElement('TD');
    timeTextEditor.appendChild(timeTextEditor.td);
    timeTextEditor.td.colSpan = 9;
    timeTextEditor.TextArea = document.createElement('TEXTAREA');
    //timeTextEditor.TextArea.maxlength=30;
    timeTextEditor.td.appendChild(timeTextEditor.TextArea);
    timeTextEditor.TextArea.style.height = 30;
    timeTextEditor.TextArea.style.width = '100%';
    timeTextEditor.td.style.paddingLeft = 408;

    //            timeTextEditor.onkeyup = function() { if(timeTextEditor.TextArea.value.length>30) timeTextEditor.TextArea.value = timeTextEditor.TextArea.value.substring(0,30); if(timeEditor.CurrentObject) { if(timeEditor.CurrentObject.Text != timeTextEditor.TextArea.value && !w.IsDirty) w.IsDirty=true; timeEditor.CurrentObject.Text = timeTextEditor.TextArea.value; } };
    //            timeTextEditor.TextArea.onkeypress = function() { if(this.value.length>30) this.value = this.value.substring(0,30); return (this.value.length <= 30); };

    timeTextEditor.onkeyup = function () {
        if (timeEditor.CurrentObject) {
            if (timeEditor.CurrentObject.Text != timeTextEditor.TextArea.value && !w.IsDirty)
                w.IsDirty = true;

            timeEditor.CurrentObject.style.backgroundImage = timeTextEditor.TextArea.value != '' && timeTextEditor.TextArea.value != null ? 'url(images/note.png)' : '';
            timeEditor.CurrentObject.style.backgroundRepeat = 'no-repeat';
            timeEditor.CurrentObject.style.backgroundPosition = 'left center';

            timeEditor.CurrentObject.Text = timeTextEditor.TextArea.value;
        }
    };



    projectEditor = document.createElement('TABLE');
    projectEditor.cellPadding = 0;
    projectEditor.cellSpacing = 0;
    projectEditor.border = 0;
    projectEditor.TableRow = projectEditor.insertRow(-1);
    projectEditor.TdOuter1 = projectEditor.TableRow.insertCell(-1);
    projectEditor.Td1 = document.createElement('div');
    projectEditor.TdOuter1.appendChild(projectEditor.Td1);
    projectEditor.Td1.style.overflow = 'hidden';
    //projectEditor.Td1.style.backgroundColor='red';
    projectEditor.Td1.style.width = '384px';
    projectEditor.Td1.style.display = 'block';
    projectEditor.Td1.style.whiteSpace = 'nowrap';

    projectEditor.Td2 = projectEditor.TableRow.insertCell(-1);
    projectEditor.TdOuter1.style.width = '100%';
    var img = document.createElement('IMG');
    img.src = 'images/navigatorDown.gif';
    img.border = 0;
    img.alt = '';
    projectEditor.Td2.appendChild(img);

    projectEditor.onclick = function () {
        projectContextMenu.Show(projectEditor.Td2.offsetLeft + projectEditor.Td2.offsetWidth, projectEditor.parentNode.offsetTop + divWeekView.offsetTop - divWeekView.scrollTop);
    };

    localCore.Keyboard.BindKeyPressToObject(timeEditor, 9, function () { return false; });
    localCore.Keyboard.BindKeyDownToObject(timeEditor, 9, function () { locateNextEditPoint(); return false; });

    localCore.Keyboard.BindKeyPressToObject(timeEditor, 13, function () { return false; });
    localCore.Keyboard.BindKeyDownToObject(timeEditor, 13, function () { locateNextEditPoint(); return false; });

    localCore.Keyboard.BindKeyPressToObject(projectEditor, 9, function () { return false; });
    localCore.Keyboard.BindKeyDownToObject(projectEditor, 9, function () { locateNextEditPoint(); return false; });

    localCore.Keyboard.BindKeyPressToObject(projectEditor, 13, function () { return false; });
    localCore.Keyboard.BindKeyDownToObject(projectEditor, 13, function () { locateNextEditPoint(); return false; });

    localCore.Keyboard.BindKeyPressToObject(timeTextEditor, 9, function () { return false; });
    localCore.Keyboard.BindKeyDownToObject(timeTextEditor, 9, function () { locateNextEditPoint(); return false; });

    //localCore.Keyboard.BindKeyPressToObject( timeTextEditor, 13, function() { return false; });
    //localCore.Keyboard.BindKeyDownToObject( timeTextEditor, 13, function() { locateNextEditPoint(); return false; });

    localCore.Keyboard.BindKeyPressToObject(timeEditor, 40, function () { timeTextEditor.TextArea.focus(); return false; });
    localCore.Keyboard.BindKeyDownToObject(timeEditor, 40, function () { timeTextEditor.TextArea.focus(); return false; });

    localCore.Keyboard.BindKeyPressToObject(timeTextEditor.TextArea, 38, function () { editAtThisPoint(getNextEditObjectFromCoords(timeEditor.CurrentObject.X, timeEditor.CurrentObject.Y)); return false; });
    localCore.Keyboard.BindKeyDownToObject(timeTextEditor.TextArea, 38, function () { editAtThisPoint(getNextEditObjectFromCoords(timeEditor.CurrentObject.X, timeEditor.CurrentObject.Y)); return false; });

    loadProjectsInWeek(document.getElementById('txtId').value);

    //AddRow();
};

function counterPostHours() {
    var nw = w.CreateWindow('adminHourUsageCounterPost.aspx', w);
    nw.Variables = document.getElementById('txtId').value;
    nw.OnClose = __counterPostHours;
};

function __counterPostHours(sender, retval) {
    if (retval) {
        loadProjectsInWeek(document.getElementById('txtId').value);

    }
};

function printHourUsage() {
    if (w.IsDirty) {
        w.Alert('Der er foretaget ændringer i denne uge.<br><br>Du skal gemme dine ændringer, før ugen kan udskrives.', 'Kan ikke udskrive!', null, null, null);
        return;
    }
    var ids = document.getElementById('txtId').value;
    var nw = w.CreateWindow('hourUsagePrint.aspx?ids=' + ids);
    //nw.Variables = items;

    //lw.PrintFilteredItems(w.WindowManager);
};

var tmpNw = null;

function SetWeekStatus(state) {
    if (w.IsDirty) {
        w.Alert('Der er foretaget ændringer i denne uge.<br><br>Du skal gemme dine ændringer, før ugen kan ' + (state ? 'godkendes' : 'afvises') + '.', 'Kan ikke ' + (state ? 'godkende' : 'afvise') + '!', null, null, null);
        return;
    }

    var items = new Array();
    items.push([document.getElementById('txtId').value, document.getElementById('txtYear').value + ' / ' + document.getElementById('txtWeekNumber').value, document.getElementById('txtUserData').value, trSumLine.childNodes[trSumLine.childNodes.length - 1].innerHTML]);

    var nw = w.CreateWindow('adminHourUsageConfirm.aspx?type=' + (state ? 'confirm' : 'decline'), w);
    nw.Variables = items;
    nw.OnClose = _SetWeekStatus;
};

function _SetWeekStatus(sender, retval) {
    if (retval) {

        w.Close(true);
    }

};

function sendToApproval() {
    if (w.IsDirty) {
        w.Alert('Der er foretaget ændringer i denne uge.<br><br>Du skal gemme dine ændringer, før ugen kan sendes til godkendelse.', 'Kan ikke sende til godkendelse!', null, null, null);
        return;
    }

    var items = new Array();
    items.push(document.getElementById('txtId').value);

    var nw = w.CreateWindow('hourUsageSendToApproval.aspx', w);
    nw.Variables = items;
    nw.OnClose = _sendToApproval;

};

function _sendToApproval(sender, retval) {
    if (retval) {
        document.getElementById('txtReadOnly').value = 'readonly';
        if (document.getElementById('allowHourTimeApproval').value == "0") {
            cmdSave.Disable();
        }
        cmdReset.Disable();
        cmdDelete.Disable();
        if (cmdMove) cmdMove.Disable();
        if (cmdApprove) cmdApprove.Disable();
        if (cmdDecline) cmdDecline.Disable();
        cmdSend.Disable();

        var info = document.getElementById('tdInfo');
        info.innerHTML = '<img src="images/hourPending.png" style="vertical-align:text-bottom;margin-right:2px;">Ugen er sendt til godkendelse';
        document.getElementById('trInfo').style.display = '';
        OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    }
};

function moveWeek() {
    var id = parseInt(document.getElementById('txtId').value);
    if (id > 0) {
        var nw = w.CreateWindow('hourUsageMoveToWeek.aspx?id=' + document.getElementById('txtId').value, w);
        nw.Variables = null;
        nw.OnClose = _moveWeek;
    }
    else
        w.Alert('Du skal gemme ugen', 'Ugen skal gemmes, før ugenummeret kan skiftes.');
};

function _moveWeek(sender, retval) {
    if (retval) {
        loadProjectsInWeek(document.getElementById('txtId').value);
    }
};

function nextWeek() {
    if (!w.IsDirty)
        __nextWeek(null, true);
    else
        w.Confirm('Du er ved at navigere væk fra en uge hvor du har lavet ændringer der ikke er gemt.<br><br>Alle ændringer vil blive tabt!<br><br>Fortsæt?', 'Der er lavet ændringer', null, __nextWeek);

};
function __nextWeek(sender, retval) {
    if (retval) {
        myWeekDate.setDate(myWeekDate.getDate() + 7);
        loadProjectsFromDate(myWeekDate);
    }
};

function prevWeek() {
    if (!w.IsDirty)
        __prevWeek(null, true);
    else
        w.Confirm('Du er ved at navigere væk fra en uge hvor du har lavet ændringer der ikke er gemt.<br><br>Alle ændringer vil blive tabt!<br><br>Fortsæt?', 'Der er lavet ændringer', null, __prevWeek);
};

function __prevWeek(sender, retval) {
    if (retval) {
        myWeekDate.setDate(myWeekDate.getDate() - 7);
        loadProjectsFromDate(myWeekDate);
    }
};

function OnResize(win, t, l, w, h) {
    if (divWeekView) {
        divWeekView.style.width = w;
        divWeekView.style.height = h - trToolbar.offsetHeight - trInfo.offsetHeight - trHeaders.offsetHeight - trSums.offsetHeight;
    }
};


function AddRow(CounterPostedRow) {
    var tr = tblProjects.insertRow(-1);
    tr.Lines = new Array();

    if (CounterPostedRow) {
        tr.style.backgroundImage = 'url(images/counterPosted.gif)';
    }

    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));
    tr.Lines.push(tr.insertCell(-1));

    var rowmax = -1;
    for (var yy = 0; yy < tblProjects.childNodes[0].childNodes.length; yy++) {
        if (tblProjects.childNodes[0].childNodes[yy].Lines) rowmax++;
    }

    for (var i = 0; i < tr.Lines.length; i++) {
        tr.Lines[i].Status = 0;
        tr.Lines[i].Value = 0;
        tr.Lines[i].innerHTML = i == 0 ? '<DIV style="width:398px;white-space:nowrap;height:16px;overflow:hidden;">' + (tr.Lines[i].getAttribute('projectname') || '- Vælg en aktivitet -') + '</DIV>' : '0,00'; //<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="">&nbsp;</div>';
        tr.Lines[i].title = (tr.Lines[i].getAttribute('projectname') || '- Vælg en aktivitet -');
        tr.Lines[i].className = i == 0 ? 'hourUsageProject' : i < tr.Lines.length - 1 ? 'hourUsageDay' + (!isValueANumberNotZero(tr.Lines[i].Value) ? 'Null' : isValueANumberLessThanZero(tr.Lines[i].Value) ? 'Negative' : '') : 'hourUsageSum';

        tr.Lines[i].X = i;
        tr.Lines[i].Y = rowmax;

        if (i < tr.Lines.length - 1) {
            tr.Lines[i].Value = 0;
            tr.Lines[i].Text = '';
            tr.Lines[i].onclick = function () {
                if (document.getElementById('allowHourTimeApproval').value == '0') {
                    if (document.getElementById('txtReadOnly').value != '')
                        return;
                    editAtThisPoint(this);
                } else {
                    if (document.getElementById('txtReadOnly').value == '' || this.Status == 0) {
                        editAtThisPoint(this);
                    }
                }
            };
        }
    }

    return tr;
};

function _refreshRejectList(sender, retval) {
}

function isValueANumberNotZero(val) {
    val = localCore.Localization.ParseLcNum(val, floatFormat);
    if (isNaN(val)) return false;
    return parseFloat(val) != 0;
}

function isValueANumberLessThanZero(val) {
    val = localCore.Localization.ParseLcNum(val, floatFormat);
    if (isNaN(val)) return false;
    return parseFloat(val) < 0;
}

function isValueANumber(val) {
    val = localCore.Localization.ParseLcNum(val, floatFormat);
    if (isNaN(val)) return false;
    return true;
}

function editAtThisPoint(obj) {
    console.log(obj);
    if (!obj) {
        if (projectEditor.CurrentObject) {
            var pn = projectEditor.CurrentObject;
            if (projectEditor.parentNode == pn)
                pn.removeChild(projectEditor);

            projectEditor.CurrentObject = null;
            pn.className = 'hourUsageProject';
            pn.innerHTML = '[' + pn.getAttribute('pserial') + '-' + pn.getAttribute('cserial') + '] ' + pn.getAttribute('serialName') + ' - ' + pn.getAttribute('caseName') || '- Vælg en aktivitet -';

            //w.IsDirty = true;
        }

        if (timeEditor.CurrentObject) {
            var pn = timeEditor.CurrentObject;
            if (timeEditor.parentNode == pn)
                pn.removeChild(timeEditor);

            if (!isValueANumber(timeEditor.value)) timeEditor.value = localCore.Localization.FormatNum(pn.Value, floatFormat);

            var val = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
            if (!w.IsDirty) w.IsDirty = val != pn.Value;
            pn.Value = val;
            pn.innerHTML = localCore.Localization.FormatNum(pn.Value, floatFormat); // + '<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="' + (pn.Text || '').replace(/"/g,"&qout;") + '">' + (pn.Text || '&nbsp;').replace(/"/g,"&qout;") + '</div>';
            pn.title = pn.Text;
            pn.className = 'hourUsageDay' + (!isValueANumberNotZero(timeEditor.value) ? 'Null' : isValueANumberLessThanZero(timeEditor.value) ? 'Negative' : '');
            SumEditedField(pn);


        }

        if (timeTextEditor.parentNode)
            timeTextEditor.parentNode.removeChild(timeTextEditor);

        return;
    }
    if (obj.X == 0) {
        if (timeEditor.CurrentObject) {

            var pn = timeEditor.CurrentObject;
            if (timeEditor.parentNode == pn)
                pn.removeChild(timeEditor);

            if (!isValueANumber(timeEditor.value)) timeEditor.value = localCore.Localization.FormatNum(pn.Value, floatFormat);

            var val = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
            if (!w.IsDirty) w.IsDirty = val != pn.Value;
            pn.Value = val;

            pn.innerHTML = localCore.Localization.FormatNum(pn.Value, floatFormat); // + '<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="' + (timeTextEditor.TextArea.value || '').replace(/"/g,"&qout;") + '">' + (timeTextEditor.TextArea.value || '&nbsp;').replace(/"/g,"&qout;") + '</div>';
            pn.title = timeTextEditor.TextArea.value || '';
            pn.className = 'hourUsageDay' + (!isValueANumberNotZero(timeEditor.value) ? 'Null' : isValueANumberLessThanZero(timeEditor.value) ? 'Negative' : '');
            SumEditedField(pn);

            if (!w.IsDirty) w.IsDirty = pn.Text != (timeTextEditor.TextArea.value || '');
            pn.Text = timeTextEditor.TextArea.value || '';

            timeEditor.CurrentObject = null;

            if (timeTextEditor) {
                if (timeTextEditor.parentNode)
                    timeTextEditor.parentNode.removeChild(timeTextEditor);
            }

            //w.IsDirty = true;
        }

        if (projectEditor.CurrentObject) {
            var pn = projectEditor.CurrentObject;
            if (projectEditor.parentNode == pn)
                pn.removeChild(projectEditor);

            projectEditor.CurrentObject = null;
            pn.className = 'hourUsageProject';
            pn.innerHTML = '<DIV style="width:398px;white-space:nowrap;height:16px;overflow:hidden;">' + (pn.getAttribute('projectname') || '- Vælg en aktivitet -') + '</DIV>';
            pn.title = (pn.getAttribute('projectname') || '- Vælg en aktivitet -');
            fixCaseNumberDots(pn, pn);

            //w.IsDirty = true;
        }

        if (obj) {
            obj.innerHTML = '';
            obj.className = 'hourUsageProjectEdit';
            obj.appendChild(projectEditor);

            divWeekView.scrollTop = obj.offsetTop;

            projectEditor.CurrentObject = obj;
            projectEditor.Td1.innerHTML = '<DIV style="width:398px;white-space:nowrap;height:16px;overflow:hidden;">' + (obj.getAttribute('projectname') || '- Vælg en aktivitet -') + '</DIV>';
            projectEditor.Td1.title = (obj.getAttribute('projectname') || '- Vælg en aktivitet -');
            //w.IsDirty = true;
        }
        else {
            projectEditor.CurrentObject = null;
        }

    }
    else {
        //no edit when no project selected
        var tdproj = obj.parentNode.getElementsByTagName('TD')[0];
        if (!tdproj.getAttribute('serialnoid') || !tdproj.getAttribute('casenoid')) {
            w.Alert('Du skal vælge en aktivitet, før der kan angives tidsforbrug');
            return;
        }

        if (projectEditor.CurrentObject) {
            var pn = projectEditor.CurrentObject;
            if (projectEditor.parentNode == pn)
                pn.removeChild(projectEditor);

            projectEditor.CurrentObject = null;
            pn.className = 'hourUsageProject';
            pn.innerHTML = '<DIV style="width:398px;white-space:nowrap;height:16px;overflow:hidden;">' + (pn.getAttribute('projectname') || '- Vælg en aktivitet -') + '</DIV>';
            pn.title = (pn.getAttribute('projectname') || '- Vælg en aktivitet -');
            fixCaseNumberDots(pn, pn);
            //w.IsDirty = true;
        }

        if (timeEditor.CurrentObject) {
            //alert(timeEditor.CurrentObject.Text + ' ' + );
            if (isValueANumber(timeEditor.value) || timeTextEditor.TextArea.value != '') {
                var valt = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
                if (valt != 0 || timeTextEditor.TextArea.value != '') {
                    var tdproj = timeEditor.CurrentObject.parentNode.getElementsByTagName('TD')[0];
                    if (!tdproj.getAttribute('serialnoid') || !tdproj.getAttribute('casenoid')) {
                        timeEditor.value = '';
                        timeTextEditor.TextArea.value = '';
                        timeEditor.CurrentObject.Value = 0;
                        timeEditor.CurrentObject.Text = '';
                        w.Alert('Du skal vælge en aktivitet, før der kan angives tidsforbrug');
                        return;
                    }
                }
            }

            var pn = timeEditor.CurrentObject;
            if (timeEditor.parentNode == pn)
                pn.removeChild(timeEditor);
            if (!isValueANumber(timeEditor.value)) timeEditor.value = localCore.Localization.FormatNum(pn.Value, floatFormat);

            //pn.Value = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
            var val = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
            if (!w.IsDirty) w.IsDirty = val != pn.Value;
            pn.Value = val;

            pn.innerHTML = localCore.Localization.FormatNum(pn.Value, floatFormat); // + '<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="' + (timeTextEditor.TextArea.value || '').replace(/"/g,"&qout;") + '">' + (timeTextEditor.TextArea.value || '&nbsp;').replace(/"/g,"&qout;") + '</div>';
            pn.title = timeTextEditor.TextArea.value || '';

            pn.className = 'hourUsageDay' + (!isValueANumberNotZero(timeEditor.value) ? 'Null' : isValueANumberLessThanZero(timeEditor.value) ? 'Negative' : '');
            if (!w.IsDirty) w.IsDirty = pn.Text != (timeTextEditor.TextArea.value || '');
            pn.Text = timeTextEditor.TextArea.value || '';

            SumEditedField(pn);


            //w.IsDirty = true;
        }

        if (obj) {
            obj.innerHTML = '';
            obj.className = 'hourUsageDayEditMode';
            obj.appendChild(timeEditor);

            var po = obj.parentNode.nextSibling;

            if (po)
                obj.parentNode.parentNode.insertBefore(timeTextEditor, po);
            else
                obj.parentNode.parentNode.appendChild(timeTextEditor);

            timeTextEditor.TextArea.value = obj.Text || '';

            divWeekView.scrollTop = obj.offsetTop;

            timeEditor.CurrentObject = obj;
            timeEditor.value = localCore.Localization.FormatNum(obj.Value, floatFormat);
            setTimeout(function () { timeEditor.select(); }, 10);
        }
        else {
            timeEditor.CurrentObject = null;
        }
    }
};

function StoreValue() {

    if (!timeEditor.CurrentObject) return false;
    if (isValueANumberNotZero(timeEditor.value)) return false;

    var val = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
    if (!w.IsDirty) w.IsDirty = val != pn.Value;
    timeEditor.CurrentObject.Value = val;

    //timeEditor.CurrentObject.Value = localCore.Localization.ParseLcNum(timeEditor.value, floatFormat);
    SumEditedField(timeEditor.CurrentObject);
    //w.IsDirty = true;
};

function locateNextEditPoint() {

    if (!timeEditor.CurrentObject && !projectEditor.CurrentObject) return false;
    if (timeEditor.CurrentObject) {
        var rowmax = -1;
        for (var yy = 0; yy < tblProjects.childNodes[0].childNodes.length; yy++) {
            if (tblProjects.childNodes[0].childNodes[yy].Lines) rowmax++;
        }
        if (timeEditor.CurrentObject.Y == rowmax && isValueANumberNotZero(timeEditor.value)) AddRow();
    }

    var y = projectEditor.CurrentObject ? projectEditor.CurrentObject.Y : timeEditor.CurrentObject.Y;
    var x = projectEditor.CurrentObject ? projectEditor.CurrentObject.X : timeEditor.CurrentObject.X;
    var max = tblProjects.childNodes[0].childNodes.length;

    if (localCore.Keyboard.KeyShiftPressed) {
        y--;
        if (y < 0) {
            y = max - 2;
            if (!tblProjects.childNodes[0].childNodes[y].Lines) y--;
            x--;
            if (x < 1) x = 7;
        }
        else {
            if (!tblProjects.childNodes[0].childNodes[y].Lines) y--;
            if (y < 0) {
                y = max - 2;
                if (!tblProjects.childNodes[0].childNodes[y].Lines) y--;
                x--;
                if (x < 1) x = 7;
            }
        }
    }
    else {
        y++;
        if (y >= max - 1) {
            y = 0;
            if (!tblProjects.childNodes[0].childNodes[y].Lines) y++;
            x++;
            if (x > 7) x = 1;
        }
        else {
            if (!tblProjects.childNodes[0].childNodes[y].Lines) y++;
            if (y >= max - 1) {
                y = 0;
                if (!tblProjects.childNodes[0].childNodes[y].Lines) y++;
                x++;
                if (x > 7) x = 1;
            }
        }
    }

    var row = getNextEditObjectFromCoords(x, y);
    if (document.getElementById('allowHourTimeApproval').value == '0') {
        editAtThisPoint(row);
        return false;
    } else {
        if (document.getElementById('txtReadOnly').value == '' || row.Status == 0) {
            editAtThisPoint(row);
            return false;
        }
    }
};

function getNextEditObjectFromCoords(x, y) {
    return tblProjects.childNodes[0].childNodes[y].Lines[x];
};


function SumEditedField(obj) {
    sumRow(obj.parentNode);
    sumVertical(obj.X);
    sumVertical(8);
};

function sumRow(row) {
    if (!row.Lines) return;
    var lines = row.Lines;
    var val = 0;
    for (var i = 1; i < lines.length - 1; i++) {
        val += lines[i].Value;
    }
    lines[lines.length - 1].Value = val;
    lines[lines.length - 1].innerHTML = localCore.Localization.FormatNum(val, floatFormat);
    lines[lines.length - 1].className = val < 0 ? 'hourUsageSumNegative' : 'hourUsageSum';
};

function sumVertical(x) {
    var val = 0;
    var rows = tblProjects.childNodes[0].childNodes;

    for (var y = 0; y < rows.length; y++) {
        if (rows[y].Lines)
            val += rows[y].Lines[x].Value;
    }

    trSumLine.childNodes[x].innerHTML = localCore.Localization.FormatNum(val, floatFormat);
    trSumLine.childNodes[x].className = val < 0 ? x == 8 ? 'hourUsageBottomSumNegative' : 'hourUsageBottomDayNegative' : x == 8 ? 'hourUsageBottomSum' : 'hourUsageBottomDay';

    return val;
};




function removeZeroRows() {
    editAtThisPoint(null);

    for (var y = tblProjects.childNodes[0].childNodes.length - 1; y >= 0; y--) {
        if (tblProjects.childNodes[0].childNodes[y].Lines[8].Value == 0) {
            var o = tblProjects.childNodes[0].removeChild(tblProjects.childNodes[0].childNodes[y]);
            addToDeleted(o);
        }
    }

    reIndexRowItems();
    AddRow();

};

function deleteAllRows() {

    editAtThisPoint(null);
    if (tblProjects.childNodes.length == 0) return;
    for (var y = tblProjects.childNodes[0].childNodes.length - 1; y >= 0; y--) {
        var o = tblProjects.childNodes[0].removeChild(tblProjects.childNodes[0].childNodes[y]);
        addToDeleted(o);
    }
    reIndexRowItems();
};

function deleteRow() {
    if (projectEditor.CurrentObject) {
        var tr = projectEditor.CurrentObject.parentNode;
        editAtThisPoint(null);
        var o = tblProjects.childNodes[0].removeChild(tr);
        //delete o;
        addToDeleted(o);
        reIndexRowItems();
    }
    else if (timeEditor.CurrentObject) {
        var tr = timeEditor.CurrentObject.parentNode;
        editAtThisPoint(null);
        var o = tblProjects.childNodes[0].removeChild(tr);
        //delete o;
        addToDeleted(o);
        reIndexRowItems();
    }
};


function addToDeleted(o) {
    var tds = o.getElementsByTagName('TD');
    for (var i = 0; i < tds.length; i++) {
        var td = tds[i];
        if (td.ProjectTypeId)
            projectsToDelete.push(td.ProjectTypeId);
    }
};

function reIndexRowItems() {
    for (var y = 0; y < tblProjects.childNodes[0].childNodes.length; y++) {
        for (var x = 0; x < tblProjects.childNodes[0].childNodes[y].Lines.length; x++) {
            tblProjects.childNodes[0].childNodes[y].Lines[x].X = x;
            tblProjects.childNodes[0].childNodes[y].Lines[x].Y = y;
        }
    }
    SumAll();
};

function SumAll() {
    for (var x = 1; x < 9; x++) {
        sumVertical(x);
    }
};

function SaveWeek() {

    editAtThisPoint(null);
    //Is all projects chosen for days where time!=0 or a memo has been set?
    var rows = tblProjects.getElementsByTagName('TR');

    var allOkay = true;

    var ajax = new core.Ajax();
    ajax.requestFile = 'ajax/hourUsageEdit.aspx';
    ajax.encVar('save', 'true');
    ajax.encVar('linecount', rows.length);
    ajax.encVar('id', document.getElementById('txtId').value);

    ajax.encVar('txtWeekDate', document.getElementById('txtWeekDate').value);
    ajax.encVar('txtWeekNumber', document.getElementById('txtWeekNumber').value);
    ajax.encVar('txtYear', document.getElementById('txtYear').value);

    if (projectsToDelete.length > 0)
        ajax.encVar('deleteitems', projectsToDelete.toString());

    projectsToDelete.length = 0;

    for (var i = 0; i < rows.length; i++) {
        var tds = rows[i].getElementsByTagName('TD');
        var lineHasProject = true;
        if (tds[0].getAttribute('serialnoid') && tds[0].getAttribute('casenoid')) {
            ajax.encVar(i + '_serialId', tds[0].getAttribute('serialnoid'));
            ajax.encVar(i + '_caseId', tds[0].getAttribute('casenoid'));
        }
        else {
            lineHasProject = false;
        }

        var ts = '';
        for (var u = 1; u < 8; u++) {

            ajax.encVar(i + '_' + u + '_id', tds[u].ProjectTypeId);
            ajax.encVar(i + '_' + u + '_timespent', tds[u].Value);
            ajax.encVar(i + '_' + u + '_memo', tds[u].Text || '');
            ajax.encVar(i + '_' + u + '_status', tds[u].Status);

            if (tds[u].Text || tds[u].Value != 0 || tds[u].ProjectTypeId) {
                if (!lineHasProject) {
                    w.Alert('Der er indtastet data på en linje, hvor der ikke er angivet en aktivitet og et varenummer.<br><br>Angiv en aktivitet og et varenummer før der gemmes.', 'Fejl!');
                    ajax.Reset();
                    ajax.Dispose();
                    delete ajax;

                    return;
                }
            }
        }
    }

    if (document.getElementById('allowHourTimeApproval').value == "0") {
        cmdSave.Disable();
    }
    cmdReset.Disable();
    cmdDelete.Disable();
    if (cmdMove) cmdMove.Disable();
    if (cmdPrev) cmdNext.Disable();
    if (cmdPrev) cmdPrev.Disable();
    if (cmdSend) cmdSend.Disable();
    if (cmdApprove) cmdApprove.Disable();
    if (cmdDecline) cmdDecline.Disable();

    core.DOM.DisableFields(document);

    w.IsDirty = false;

    tblSave.style.display = '';
    pb.AnimateProgress(100);

    ajax.OnCompletion = function () { _loadProjectsInWeek(ajax); };
    ajax.OnError = function () { _loadProjectsInWeek(ajax); };
    ajax.OnFail = function () { _loadProjectsInWeek(ajax); };
    ajax.RunAJAX();

};

function __SaveWeek(AjaxObject) {
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    if (document.getElementById('allowHourTimeApproval').value == "0") {
        cmdSave.Enable();
    }
    cmdReset.Enable();
    cmdDelete.Enable();
    if (cmdMove) cmdMove.Enable();
    if (cmdNext) cmdNext.Enable();
    if (cmdPrev) cmdPrev.Enable();
    if (cmdSend) cmdSend.Enable();
    if (cmdApprove) cmdApprove.Enable();
    if (cmdDecline) cmdDecline.Enable();

    core.DOM.EnableFields(document);

    tblSave.style.display = 'none';
    pb.ResetTo(0);
};





function loadProjectsFromDate(date) {
    editAtThisPoint(null);

    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/hourUsageEdit.aspx';

    var theyear = date.getFullYear();
    var themonth = date.getMonth() + 1;
    var thetoday = date.getDate();
    var dsOut = theyear + '-' + themonth + '-' + thetoday;

    AjaxObject.encVar('date', dsOut);
    if (document.getElementById('allowHourTimeApproval').value == "0") {
        cmdSave.Disable();
    }
    cmdReset.Disable();
    cmdDelete.Disable();
    if (cmdMove) cmdMove.Disable();
    if (cmdNext) cmdNext.Disable();
    if (cmdPrev) cmdPrev.Disable();
    if (cmdSend) cmdSend.Disable();
    if (cmdApprove) cmdApprove.Disable();
    if (cmdDecline) cmdDecline.Disable();

    core.DOM.DisableFields(document);

    AjaxObject.OnCompletion = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.OnError = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.OnFail = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.RunAJAX();
};

function loadProjectsInWeek(id) {
    editAtThisPoint(null);

    var AjaxObject = new core.Ajax();
    AjaxObject.requestFile = 'ajax/hourUsageEdit.aspx';

    AjaxObject.encVar('id', id);

    if (document.getElementById('allowHourTimeApproval').value == "0") {
        cmdSave.Disable();
    }
    cmdReset.Disable();
    cmdDelete.Disable();
    if (cmdMove) cmdMove.Disable();
    if (cmdNext) cmdNext.Disable();
    if (cmdPrev) cmdPrev.Disable();
    if (cmdSend) cmdSend.Disable();
    if (cmdApprove) cmdApprove.Disable();
    if (cmdDecline) cmdDecline.Disable();

    core.DOM.DisableFields(document);

    AjaxObject.OnCompletion = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.OnError = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.OnFail = function () { _loadProjectsInWeek(AjaxObject); };
    AjaxObject.RunAJAX();
};

function _loadProjectsInWeek(AjaxObject) {
    tblSave.style.display = 'none';
    pb.ResetTo(0);

    // Reintialize the time register
    // deleteAllRows();
    editAtThisPoint(null);
    if (tblProjects.childNodes.length > 0) {
        for (var y = tblProjects.childNodes[0].childNodes.length - 1; y >= 0; y--) {
            tblProjects.childNodes[0].removeChild(tblProjects.childNodes[0].childNodes[y]);
        }
        reIndexRowItems();
    }
    // ended

    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var div = document.createElement('DIV');
    div.innerHTML = result;

    var headers = div.removeChild(div.firstChild);

    document.getElementById('txtUserId').value = headers.getAttribute('txtUserId');
    document.getElementById('txtWeekDate').value = headers.getAttribute('txtWeekDate');
    document.getElementById('txtWeekNumber').value = headers.getAttribute('txtWeekNumber');
    document.getElementById('txtYear').value = headers.getAttribute('txtYear');
    document.getElementById('txtId').value = headers.getAttribute('txtId');
    document.getElementById('txtUserData').value = headers.getAttribute('txtUserData');

    var isAdmin = document.getElementById('isAdmin').value == 'true';

    if (isAdmin) {
        //
        document.getElementById('txtReadOnly').value = (headers.getAttribute('approved') != '' || headers.getAttribute('declined') != '' ? 'readonly' : '');
        if (headers.getAttribute('approved') != '') {
            cmdCounterPost.Enable();
        }
        else {
            cmdCounterPost.Disable();
        }
    }
    else {
        document.getElementById('txtReadOnly').value = (headers.getAttribute('approved') != '' || headers.getAttribute('pending') != '' ? 'readonly' : '');
    }


    lblCurrentWeek.Label.innerHTML = 'Uge ' + document.getElementById('txtWeekNumber').value + ' / ' + document.getElementById('txtYear').value;
    w.SetTitle('Ugeregistrering for uge ' + document.getElementById('txtWeekNumber').value + ' / ' + document.getElementById('txtYear').value + ' for ' + document.getElementById('txtUserData').value);

    var dps = document.getElementById('txtWeekDate').value.split('-');
    myWeekDate = new Date(parseInt(dps[2], 10), parseInt(dps[1], 10) - 1, parseInt(dps[0], 10));

    //write days
    var objDays = trHeaders.getElementsByTagName('DIV');
    var day = 0;
    while (headers.firstChild) {
        var obj = headers.removeChild(headers.firstChild);
        objDays[day].innerHTML = obj.innerHTML;
        day++;
    }

    //parse data
    if (div.childNodes.length > 0) {
        while (div.firstChild) {
            var obj = div.removeChild(div.firstChild);

            var pid = parseInt(obj.getAttribute('pid'), 10);
            var serial = parseInt(obj.getAttribute('serial'), 10);
            var caseno = parseInt(obj.getAttribute('caseno'), 10);
            var dow = parseInt(obj.getAttribute('dow'), 10);
            var timespent = parseFloat(obj.getAttribute('timespent'));
            var serialname = obj.getAttribute('serialname');
            var casename = obj.getAttribute('casename');
            var serialId = obj.getAttribute('serialnoid');
            var caseId = obj.getAttribute('casenoid');

            var counterposted = obj.getAttribute('counterposted') == 'true';
            var rejected = obj.getAttribute('rejected');
            var status = obj.getAttribute('status');
            var memo = obj.innerHTML;

            var row = getRow(pid, serial, caseno, dow, timespent, serialname, casename, memo, serialId, caseId, counterposted, rejected, status); //hu.GetExistingRow(s, c, d) || hu.AddRow();

            if (row) sumRow(row);
        }
    }

    AddRow();
    SumAll();

    if (cmdNext) cmdNext.Enable();
    if (cmdPrev) cmdPrev.Enable();

    if (document.getElementById('txtReadOnly').value == '') {
        if (document.getElementById('allowHourTimeApproval').value == "0") {
            cmdSave.Enable();
        }

        cmdReset.Enable();
        cmdDelete.Enable();

        if (cmdMove) cmdMove.Enable();
        if (cmdSend) cmdSend.Enable();

        if (cmdApprove) cmdApprove.Enable();
        if (cmdDecline) cmdDecline.Enable();
    }

    var info = document.getElementById('tdInfo');
    info.innerHTML = headers.getAttribute('approved') || '';
    info.innerHTML = headers.getAttribute('declined') || info.innerHTML;
    info.innerHTML = headers.getAttribute('pending') || info.innerHTML;
    document.getElementById('trInfo').style.display = (info.innerHTML == '' ? 'none' : '');

    OnResize(w, 0, 0, w.GetInnerWidth(), w.GetInnerHeight());
    core.DOM.EnableFields(document);

    // Set start of editing!
    if (document.getElementById('txtReadOnly').value == '') {
        var o = getNextEditObjectFromCoords(1, 0);
        if (o) editAtThisPoint(o);
    }

    w.IsDirty = false;
};


function getRow(pid, serial, caseno, dow, timespent, serialName, caseName, memo, serialId, caseId, counterposted, rejected, status) {

    if (tblProjects.childNodes.length > 0) {
        for (var y = 0; y < tblProjects.childNodes[0].childNodes.length; y++) {
            if (tblProjects.childNodes[0].childNodes[y].Lines) {
                var rv = tblProjects.childNodes[0].childNodes[y]
                var pn = rv.Lines[0];

                var id = pn.getAttribute('pid');
                var pserial = pn.getAttribute('pserial');
                var cserial = pn.getAttribute('cserial');
                var cpost = pn.getAttribute('counterposted') == 'true';

                var projectname = pn.getAttribute('serialName') + ' - ' + pn.getAttribute('caseName');

                if (pserial == serial && cserial == caseno && cpost == counterposted) {
                    if (rv.Lines) {
                        if (!rv.Lines[dow].Value) {

                            rv.Lines[dow].Status = status;
                            rv.Lines[dow].IsRejected = rejected;
                            rv.Lines[dow].SerialId = serialId;
                            rv.Lines[dow].CaseId = caseId;
                            rv.Lines[dow].Value = timespent;
                            rv.Lines[dow].ProjectTypeId = pid;

                            rv.Lines[dow].innerHTML = localCore.Localization.FormatNum(timespent, floatFormat); // + '<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="' + (memo || '').replace(/"/g,"&qout;") + '">' + (memo || '&nbsp;').replace(/"/g,"&qout;") + '</div>';
                            rv.Lines[dow].title = memo || '';
                            rv.Lines[dow].Text = memo;

                            rv.Lines[dow].style.backgroundImage = memo != '' && memo != null ? 'url(images/note.png)' : '';

                            rv.Lines[dow].className = 'hourUsageDay' + (!isValueANumberNotZero(timespent) ? 'Null' : isValueANumberLessThanZero(timespent) ? 'Negative' : '');
                            rv.Lines[dow].style.backgroundRepeat = 'no-repeat';
                            rv.Lines[dow].style.backgroundPosition = 'left center';

                            if (rejected == "1") { rv.Lines[dow].style.backgroundColor = '#eea1a1'; }

                            return tblProjects.childNodes[0].childNodes[y];
                        }
                    }
                }
            }
        }
    }

    //alert(counterposted);
    var row = AddRow(counterposted);

    var o = row.Lines[0];
    o.setAttribute('pid', pid);
    o.setAttribute('pserial', serial);
    o.setAttribute('cserial', caseno);
    o.setAttribute('projectname', '[' + serial + '-' + caseno + '] ' + serialName + ' - ' + caseName);
    o.setAttribute('serialName', serialName);
    o.setAttribute('caseName', caseName);

    o.setAttribute('serialnoid', serialId);
    o.setAttribute('casenoid', caseId);
    o.setAttribute('counterposted', counterposted ? 'true' : '');

    o.innerHTML = '<DIV style="width:398px;white-space:nowrap;height:16px; ;overflow:hidden;">' + '[' + serial + '-' + caseno + '] ' + serialName + ' - ' + caseName + '</DIV>';
    o.title = '[' + serial + '-' + caseno + '] ' + serialName + ' - ' + caseName;

    fixCaseNumberDots(o, o);

    row.Lines[dow].Status = status;
    row.Lines[dow].IsRejected = rejected;
    if (rejected == "1") { row.Lines[dow].style.backgroundColor = '#eea1a1'; }

    row.Lines[dow].SerialId = serialId;
    row.Lines[dow].CaseId = caseId;
    row.Lines[dow].Value = timespent;
    row.Lines[dow].innerHTML = localCore.Localization.FormatNum(timespent, floatFormat); // + '<div style="text-align:right;border-top:solid 1px #ebebeb; overflow:hidden;width:59px;height:17px;" title="' + (memo || '').replace(/"/g,"&qout;") + '">' + (memo || '&nbsp;').replace(/"/g,"&qout;") + '</div>';
    row.Lines[dow].title = memo || '';
    row.Lines[dow].style.backgroundImage = memo != '' && memo != null ? 'url(images/note.png)' : '';
    row.Lines[dow].style.backgroundRepeat = 'no-repeat';
    row.Lines[dow].style.backgroundPosition = 'left center';


    row.Lines[dow].Text = memo;
    row.Lines[dow].ProjectTypeId = pid;
    row.Lines[dow].className = 'hourUsageDay' + (!isValueANumberNotZero(timespent) ? 'Null' : isValueANumberLessThanZero(timespent) ? 'Negative' : '');
    return row;
};
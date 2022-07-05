var ReportGenerator = function (document, container) {
    this.Document = document;
    this.Container = container;
    this.isIE = document.all ? true : false;

    //Events
    this.OnSelectedTablesChanged = null;

    this.Reports = new Array();

    this.ContextCompareText = new ContextMenu(document, container, window, '');
    this.ContextCompareOther = new ContextMenu(document, container, window, '');
    this.ContextCompareBool = new ContextMenu(document, container, window, '');

    this.ContextCompareBool.AddItem('Lig med', 'images/reportGenerator/condition_equalTo.png', null);
    this.ContextCompareBool.AddItem('Forskellig fra', 'images/reportGenerator/condition_notEqual.png', null);
    //this.ContextCompareBool.AddItem('Ikke i udtræk', 'images/reportGenerator/condition_NotIn.png', null);

    this.ContextCompareText.AddItem('Lig med', 'images/reportGenerator/condition_equalTo.png', null);
    this.ContextCompareText.AddItem('Forskellig fra', 'images/reportGenerator/condition_notEqual.png', null);
    this.ContextCompareText.AddItem('Cirka lig med', 'images/reportGenerator/condition_like.png', null);
    this.ContextCompareText.AddItem('Ikke cirka lig med', 'images/reportGenerator/condition_like.png', null);
    //this.ContextCompareText.AddItem('Ikke i udtræk', 'images/reportGenerator/condition_NotIn.png', null);

    this.ContextCompareOther.AddItem('Lig med', 'images/reportGenerator/condition_equalTo.png', null);
    this.ContextCompareOther.AddItem('Forskellig fra', 'images/reportGenerator/condition_notEqual.png', null);
    this.ContextCompareOther.AddItem('Større end', 'images/reportGenerator/condition_greater.png', null);
    this.ContextCompareOther.AddItem('Mindre end', 'images/reportGenerator/condition_smaller.png', null);
    this.ContextCompareOther.AddItem('Større end eller lig med', 'images/reportGenerator/condition_greaterOrEqual.png', null);
    this.ContextCompareOther.AddItem('Mindre end eller lig med', 'images/reportGenerator/condition_smallerOrEqual.png', null);
    //this.ContextCompareOther.AddItem('Ikke i udtræk', 'images/reportGenerator/condition_NotIn.png', null);

};

ReportGenerator.prototype.RemoveReport = function (report) {
    for (var i = this.Reports.length - 1; i >= 0; i--) {
        if (this.Reports[i] == report) {
            this.Reports.splice(i, 1);
        }
    }
    report.Container.removeChild(report.ReportView);
};

ReportGenerator.prototype.AddReport = function (name, availableTables, loadData, databaseId) {
    var r = new Report(this, name, availableTables, loadData, databaseId);
    this.Reports.push(r);
    return r;
};

var Report = function (reportGenerator, name, availableTables, loadData, databaseId) {
    this.MapsDataSourceTable = '';

    this.Tables = new Array();
    this.IsDirty = false;

    this.ReportGenerator = reportGenerator;
    this.Name = name;
    this.DatabaseId = databaseId || '';

    this.AvailableTables = availableTables;

    this.Document = reportGenerator.Document;
    this.Container = reportGenerator.Container;

    this.LoadData = this.Document.createElement('DIV');

    this.LoadData.innerHTML = loadData || '';


    this.ReportViewControl = this.Document.createElement('div');
    this.ReportViewControl.style.overflow = 'auto';


    this.ReportView = this.Document.createElement('TABLE');

    this.ReportView.border = 0;
    this.ReportView.cellPadding = 0;
    this.ReportView.cellSpacing = 0;

    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.ReportTablesArea = this.Document.createElement('TD');
    //this.VSplitter=this.Document.createElement('TD');
    this.ReportDesktopArea = this.Document.createElement('TD');

    //    this.VSplitter.className="blindsVSplit";
    //    this.VSplitter.innerHTML='<img src="images/spacer.gif" alt="" border="0" style="width:3px;height:1px;">';
    //    this.VSplitter.style.borderRight='solid 1px #576d8b';

    this.ReportView.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.ReportTablesArea);
    //tr.appendChild(this.VSplitter);
    tr.appendChild(this.ReportDesktopArea);

    this.ReportTablesArea.vAlign = 'top';

    this.ReportTablesArea.style.width = 230;
    this.ReportTablesArea.style.height = 100; //this.Container.offsetHeight;



    this.ReportDesktopArea.className = 'blinds_backgroundMDI';
    this.ReportDesktopArea.style.overflow = 'auto';
    this.ReportDesktopArea.style.width = this.Container.offsetWidth - 230;
    this.ReportDesktopArea.style.height = this.Container.offsetHeight;
    this.ReportDesktopArea.align = 'left';
    this.ReportDesktopArea.vAlign = 'top';

    this.ReportViewControl.style.width = this.ReportDesktopArea.style.width;
    this.ReportViewControl.style.height = this.ReportDesktopArea.style.height;

    this.ReportDesktopArea.appendChild(this.ReportViewControl);

    this.ReportView.style.width = this.Container.offsetWidth;
    this.ReportView.style.height = this.Container.offsetHeight;

    //this.VSplitter.style.height = this.Container.offsetHeight;



    var me = this;
    //this.VerticalSplit = new VSplitter(this.Document, this.VSplitter, this.ReportTablesArea, this.ReportDesktopArea, function(h1, h2) { me.VSplitSet(h1, h2); } );

    this.TablesHeader = this.Document.createElement('DIV');
    this.TablesHeader.innerHTML = 'Alle Datakilder';
    this.TablesHeader.style.overflow = 'hidden';
    this.TablesHeader.className = 'genericBoxHeader';

    this.TablesHeader.style.width = this.ReportGenerator.isIE ? 230 : 224;
    this.ReportTablesArea.appendChild(this.TablesHeader);

    this.Container.appendChild(this.ReportView);

    this.ReportViewControl.IsMouseDown = false;
    this.ReportViewControl.SelectedTable = null;
    this.ReportViewControl.MouseMoveEvent = null;

    this.ReportViewControl.CurrentZindex = 1;

    this.AvailableTablesControl = this.BuildAvailableTables();
    this.AvailableTablesControl.style.height = this.ReportTablesArea.offsetHeight;
    this.ReportTablesArea.appendChild(this.AvailableTablesControl);

    this.ReportViewControl.onmousemove = function () {
        if (this.IsMouseDown && this.SelectedTable) {
            autoComplete.Hide();
            if (this.MouseMoveEvent == 'move') {
                this.SelectedTable.Window.style.left = (parseInt(localCore.Mouse.X) - this.SelectedTable.Window.OffsetX - this.scrollLeft);
                this.SelectedTable.Window.style.top = (parseInt(localCore.Mouse.Y) - this.SelectedTable.Window.OffsetY - this.scrollTop);
                if (this.SelectedTable.Window.ArrowH) this.SelectedTable.Window.SetPointingArrows();
                for (var i = 0; i < this.SelectedTable.Window.ArrowsPointingToMeFrom.length; i++) {
                    if (this.SelectedTable.Window.ArrowsPointingToMeFrom[i].ArrowH)
                        this.SelectedTable.Window.ArrowsPointingToMeFrom[i].SetPointingArrows();
                }
            }
            else if (this.MouseMoveEvent == 'rz-r') {
                this.SelectedTable.Window.Resize(null, null, (parseInt(localCore.Mouse.X) - this.SelectedTable.Window.OffsetX - this.scrollLeft));
            }
            else if (this.MouseMoveEvent == 'rz-l') {
                this.SelectedTable.Window.Resize((parseInt(localCore.Mouse.X) - this.SelectedTable.Window.OffsetX - this.scrollLeft), null, this.SelectedTable.Window.OffsetW - 2 - (parseInt(localCore.Mouse.X - this.SelectedTable.Window.OffsetX - this.scrollLeft)));
            }
            else if (this.MouseMoveEvent == 'rz-b') {
                this.SelectedTable.Window.Resize(null, null, null, (parseInt(localCore.Mouse.Y - this.SelectedTable.Window.OffsetY - this.scrollTop)));
            }
            else if (this.MouseMoveEvent == 'rz-br') {
                this.SelectedTable.Window.Resize(null, null, (parseInt(localCore.Mouse.X) - this.SelectedTable.Window.OffsetX - this.scrollLeft), (parseInt(localCore.Mouse.Y - this.SelectedTable.Window.OffsetY - this.scrollTop)));
            }
            else if (this.MouseMoveEvent == 'rz-bl') {
                this.SelectedTable.Window.Resize((parseInt(localCore.Mouse.X) - this.SelectedTable.Window.OffsetX - this.scrollLeft), null, this.SelectedTable.Window.OffsetW - 2 - (parseInt(localCore.Mouse.X - this.SelectedTable.Window.OffsetX - this.scrollLeft)), (parseInt(localCore.Mouse.Y - this.SelectedTable.Window.OffsetY - this.scrollTop)));
            }
        }
    };

    this.ReportViewControl.onmouseup = function () {
        this.IsMouseDown = false;
    }
    this.ReportViewControl.onclick = function () {
        this.IsMouseDown = false;
    }

    return this;
};
Report.prototype.GetSaveData = function () {
    var updates = '';

    for (var i = 0; i < this.Tables.length; i++) {
        var tbl = this.Tables[i];
        updates += 'TBL' + tbl.Window.offsetLeft + '%' + tbl.Window.offsetTop + '%' + tbl.Window.offsetWidth + '%' + tbl.Window.offsetHeight + '%' + tbl.Table + '\n';

        for (var ii = 0; ii < tbl.Fields.length; ii++) {
            var fld = tbl.Fields[ii];
            if (fld.checked) {
                updates += 'FLD' + fld.Uniqueidentifier + '\n';

                for (var iii = 0; iii < fld.Filters.length; iii++) {
                    var fltr = fld.Filters[iii];

                    updates += 'FLT' + fltr.CompareMethod + fltr.value + '\n';
                }
            }
        }
    }

    return updates;
};

Report.prototype.Resized = function (w, h) {
    this.ReportView.style.width = w;
    this.ReportView.style.height = h;
    this.AvailableTablesControl.style.height = h - this.TablesHeader.offsetHeight;
    this.ReportTablesArea.style.height = h;
    this.ReportDesktopArea.style.height = h;
    this.ReportDesktopArea.style.width = w - this.AvailableTablesControl.offsetWidth - 4;

    this.ReportViewControl.style.width = this.ReportDesktopArea.style.width;
    this.ReportViewControl.style.height = this.ReportDesktopArea.style.height;

    this.ReportViewControl.style.position = 'absolute';

    //this.VSplitter.style.height=h;
};

Report.prototype.VSplitSet = function (h1, h2) {
    this.AvailableTablesControl.style.width = h1 + (this.ReportGenerator.isIE ? 0 : 6);
    this.TablesHeader.style.width = h1;
    this.ReportViewControl.style.width = h2 - (this.ReportGenerator.isIE ? 0 : 6);
};

Report.prototype.BuildAvailableTables = function () {
    var div = this.Document.createElement('DIV');
    div.style.overflowY = 'auto';
    div.style.overflowX = 'hidden';
    div.style.height = 100;
    div.style.width = 230;

    var parentObj = div;

    for (var i = 0; i < this.AvailableTables.childNodes.length; i++) {
        if (this.AvailableTables.childNodes[i].getAttribute('header') == 'true') {
            var h = this.BuildTableHeader(this.AvailableTables.childNodes[i]);
            parentObj = h.TableContainer || div;
            div.appendChild(h);
        }
        else
            parentObj.appendChild(this.BuildAvailableTable(this.AvailableTables.childNodes[i], this.LoadData.getElementsByTagName('b')));
    }

    div.style.backgroundColor = 'white';

    return div;
};

Report.prototype.BuildTableHeader = function (ctrl) {
    var tbl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr1 = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var td1a = this.Document.createElement('TD');
    var td1b = this.Document.createElement('TD');

    var tr2 = this.Document.createElement('TR');
    var td2 = this.Document.createElement('TD');
    td2.colSpan = 3;
    td2.colspan = 3;
    td2.setAttribute('colspan', '3');

    tbl.border = 0;
    tbl.cellPadding = 0;
    tbl.cellSpacing = 0;
    tbl.style.width = '100%';

    tbl.appendChild(tb);
    tb.appendChild(tr1);
    tb.appendChild(tr2);

    tr1.appendChild(td1b);
    tr1.appendChild(td1a);
    tr1.appendChild(td1);

    tr2.appendChild(td2);

    td1.className = 'tableHeader';
    td1a.className = 'tableHeaderIcon';
    td1b.className = 'tableHeaderCollapse';

    td1.innerHTML = ctrl.innerHTML;
    td1.style.width = '100%';
    td1a.style.width = '16px';
    td1b.style.width = '16px';

    var img = this.Document.createElement('IMG');
    img.src = ctrl.getAttribute('icon');
    td1a.appendChild(img);

    var imgCol = this.Document.createElement('IMG');
    imgCol.src = '../Images/navigatorUp.gif';
    td1b.appendChild(imgCol);

    tbl.TableContainer = td2;

    var onHeaderClick = function () {
        if (tr2.style.display == '') {
            tr2.style.display = 'none';
            imgCol.src = '../Images/navigatorDown.gif';
        }
        else {
            tr2.style.display = '';
            imgCol.src = '../Images/navigatorUp.gif';
        }
    };

    td1.onclick = onHeaderClick;
    td1a.onclick = onHeaderClick;
    td1b.onclick = onHeaderClick;

    return tbl;
};

Report.prototype.BuildAvailableTable = function (ctrl, tablesToLoad) {
    var tbl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var td2 = this.Document.createElement('TD');
    var td3 = this.Document.createElement('TD');

    var cb = this.Document.createElement('INPUT');
    cb.type = 'checkbox';

    if (!this.ReportGenerator.isIE) {
        cb.style.marginTop = 1;
        cb.style.marginBottom = 2;
    }
    var img = this.Document.createElement('IMG');
    tbl.border = 0;
    tbl.cellPadding = this.ReportGenerator.isIE ? 1 : 0;
    tbl.cellSpacing = 0;
    tbl.style.cursor = 'pointer';

    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);
    td1.appendChild(cb);
    td2.appendChild(img);

    //    td1.style.paddingTop = (this.ReportGenerator.isIE ? 2 : 2);
    //    td1.style.paddingBottom = (this.ReportGenerator.isIE ? 2 : 2);
    //td1.style.padding = 2;
    td1.style.paddingTop = (this.ReportGenerator.isIE ? 1 : 3);
    td1.style.paddingBottom = (this.ReportGenerator.isIE ? 1 : 3);

    td3.innerHTML = ctrl.innerHTML;
    td3.noWrap = true;
    td3.style.width = '100%';
    tbl.style.width = '296px';

    //cb.setAttribute('type','checkbox');
    img.src = ctrl.getAttribute('icon');

    td2.onclick = function () { if (cb.disabled) return; cb.checked = !cb.checked; cb.onclick(); };
    td3.onclick = td2.onclick;

    cb.Name = ctrl.innerHTML;
    cb.Icon = ctrl.getAttribute('icon');
    cb.Table = ctrl.getAttribute('table');
    tbl.className = 'tableRow';

    ctrl.icon = ctrl.getAttribute('icon');
    ctrl.table = ctrl.getAttribute('table');

    var me = this;
    cb.onclick = function (loadFieldsAndFilters, lockTableAndCheckItem) {
        if (this.disabled) return;

        if (this.checked) {
            this.TableControl = me.AddTable(this.Name, this.Icon, this.Table, this, loadFieldsAndFilters, lockTableAndCheckItem);
            this.parentNode.parentNode.parentNode.parentNode.className = 'tableRowSelected';
        }
        else {
            if (this.TableControl) {
                this.TableControl.Remove();
                this.TableControl = null;
            }
            this.parentNode.parentNode.parentNode.parentNode.className = 'tableRow';
        }

        //Refresh ToMaps AddressTableSources
        if (me.ReportGenerator.OnSelectedTablesChanged != null)
            me.ReportGenerator.OnSelectedTablesChanged(me);
    };

    for (var i = 0; i < tablesToLoad.length; i++) {
        if (ctrl.table.toLowerCase() == tablesToLoad[i].getAttribute('tableName').toLowerCase()) {
            cb.checked = true;
            cb.onclick(true);

            var x = parseInt(tablesToLoad[i].getAttribute('x'), 10);
            var y = parseInt(tablesToLoad[i].getAttribute('y'), 10);
            var w = parseInt(tablesToLoad[i].getAttribute('width'), 10);
            var h = parseInt(tablesToLoad[i].getAttribute('height'), 10);
            cb.TableControl.Window.Resize(x, y, w, h);
        }
    }

    return tbl;

};

Report.prototype.AddTable = function (name, icon, table, checkBox, loadFieldsAndFilters, lockTableAndCheckItem) {

    var t = new ReportTable(this, name, icon, table, checkBox, loadFieldsAndFilters, lockTableAndCheckItem);
    this.IsDirty = true;
    w.IsDirty = true;

    this.Tables.push(t);
    return t;
};

Report.prototype.RemoveTable = function (table) {
    this.IsDirty = true;
    w.IsDirty = true;

    for (var i = this.Tables.length - 1; i >= 0; i--) {
        if (this.Tables[i].Window.ArrowsPointingTo == table && this.Tables[i].Window.ArrowH) {
            this.Container.removeChild(this.Tables[i].Window.ArrowH);
            this.Container.removeChild(this.Tables[i].Window.ArrowV);
            this.Container.removeChild(this.Tables[i].Window.ArrowA);

            table.Tables[i].Window.ArrowH = null;
            table.Tables[i].Window.ArrowV = null;
            table.Tables[i].Window.ArrowA = null;
        }

        if (this.Tables[i] == table) {
            this.Tables.splice(i, 1);
        }
    }

    //Remove my own arrows and others
    if (table.Window.ArrowH) {
        this.Container.removeChild(this.Window.ArrowH);
        this.Container.removeChild(this.Window.ArrowV);
        this.Container.removeChild(this.Window.ArrowA);

        table.Window.ArrowH = null;
        table.Window.ArrowV = null;
        table.Window.ArrowA = null;
    }
    this.Container.removeChild(table.Window);
};

Report.prototype.Remove = function () {
    for (var i = this.ReportGenerator.Reports.length - 1; i >= 0; i--) {
        if (this.ReportGenerator.Reports[i] == this) {
            this.ReportGenerator.Reports.splice(i, 1);
        }
    }
    this.Container.removeChild(this.ReportView);
};

var ReportTable = function (report, name, icon, table, checkBox, loadFieldsAndFilters, lockTableAndCheckItem) {
    this.Report = report;
    this.Name = name;
    this.Icon = icon;
    this.Table = table;
    this.CheckBox = checkBox;
    this.Fields = new Array();

    this.Document = report.Document;
    this.Container = report.ReportViewControl;

    this.Window = this.Document.createElement('TABLE');

    this.Window.Uniqueidentifier = null;
    this.Window.sqlJoinBefore = null;
    this.Window.sqlObject = null;
    this.Window.htmlObject = null;
    this.Window.sqlJoinTo = null;
    this.Window.listviewTable = null;

    this.Window.sqlOrderBy = null;

    this.Window.ArrowH = null;
    this.Window.ArrowV = null;
    this.Window.ArrowA = null;

    this.Window.ArrowsPointingTo = null;
    this.Window.ArrowsPointingToMeFrom = new Array();

    this.Window.style.position = 'absolute';
    this.Window.style.left = 20 * (this.Report.Tables.length + 1);
    this.Window.style.top = 20 * (this.Report.Tables.length + 1);
    this.Window.style.width = 220;
    this.Window.style.height = 320;
    this.Window.border = 0;
    this.Window.cellPadding = 0;
    this.Window.cellSpacing = 0;



    //    this.Window.style.border='solid 1px black';

    var tb = this.Document.createElement('TBODY');
    var tr1 = this.Document.createElement('TR');
    var tr2 = this.Document.createElement('TR');
    var tr3 = this.Document.createElement('TR');

    this.Window.appendChild(tb);
    tb.appendChild(tr1);
    tb.appendChild(tr2);
    tb.appendChild(tr3);

    this.WindowElements = new Array();
    this.WindowElementsClasses = ['blinds_tl', 'blinds_tm', 'blinds_tr', 'blinds_ml', 'contentFrameBorder', 'blinds_mr', 'blinds_bl', 'blinds_bm', 'blinds_br'];
    this.WindowElementsClassesU = ['blinds_tl_u', 'blinds_tm_u', 'blinds_tr_u', 'blinds_ml_u', 'contentFrameBorder_u', 'blinds_mr_u', 'blinds_bl_u', 'blinds_bm_u', 'blinds_br_u'];

    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));

    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));

    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));
    this.WindowElements.push(this.Document.createElement('TD'));

    tr1.appendChild(this.WindowElements[0]);
    tr1.appendChild(this.WindowElements[1]);
    tr1.appendChild(this.WindowElements[2]);

    tr2.appendChild(this.WindowElements[3]);
    tr2.appendChild(this.WindowElements[4]);
    tr2.appendChild(this.WindowElements[5]);

    tr3.appendChild(this.WindowElements[6]);
    tr3.appendChild(this.WindowElements[7]);
    tr3.appendChild(this.WindowElements[8]);

    var w = parseInt(this.Window.style.width, 10);
    var h = parseInt(this.Window.style.height, 10);

    this.WindowElements[0].style.width = 6;
    this.WindowElements[0].style.height = 24;

    this.WindowElements[1].style.width = w - 12;
    this.WindowElements[1].style.height = 24;

    this.WindowElements[2].style.width = 6;
    this.WindowElements[2].style.height = 24;

    this.WindowElements[3].style.width = 6;
    this.WindowElements[3].style.height = h - 30;

    this.WindowElements[4].style.width = w - 12;
    this.WindowElements[4].style.height = h - 30;

    this.WindowElements[5].style.width = 6;
    this.WindowElements[5].style.height = h - 30;

    this.WindowElements[6].style.width = 6;
    this.WindowElements[6].style.height = 6;

    this.WindowElements[7].style.width = w - 12;
    this.WindowElements[7].style.height = 6;

    this.WindowElements[8].style.width = 6;
    this.WindowElements[8].style.height = 6;


    this.Window.Title = this.Document.createElement('DIV');
    this.Window.Title.innerHTML = this.Name;

    this.Window.Title.style.overflow = 'hidden';
    this.Window.Title.style.whiteSpace = 'nowrap';
    this.Window.Title.style.backgroundImage = 'url(' + WebsiteURL + this.Icon + ')';
    this.Window.Title.style.backgroundRepeat = 'no-repeat';
    this.Window.Title.style.paddingLeft = 19;
    this.Window.Title.style.width = w - (this.Report.ReportGenerator.isIE ? 37 : 56);
    this.Window.Title.style.height = 16;
    if (this.Report.ReportGenerator.isIE) this.Window.Title.style.marginBottom = 2; else this.Window.Title.style.marginTop = 3;
    this.Window.Title.style.display = (this.Report.ReportGenerator.isIE ? 'inline' : 'inline-block');

    this.Window.CloseButton = this.Document.createElement('DIV');
    this.Window.CloseButton.style.display = (this.Report.ReportGenerator.isIE ? 'inline' : 'inline-block');
    this.Window.CloseButton.innerHTML = '<img src="images/windows/cmdClose.png" style="margin-top:1px;margin-bottom:3px;width:25px;height:16px;">';
    this.Window.CloseButton.className = 'controller';
    this.Window.CloseButton.onmouseover = function () { this.className = 'controllerRedHover'; };
    this.Window.CloseButton.onmouseout = function () { this.className = 'controller'; };

    if (lockTableAndCheckItem) {
        this.Window.CloseButton.style.visibility = 'hidden';
    }

    //this.Window.CloseButton.align='right';

    this.Window.CloseButton.Table = this;

    this.Window.CloseButton.onclick = function () {
        this.Table.Remove();
        this.Table.CheckBox.checked = false;
        if (this.Table.CheckBox.TableControl) {
            this.Table.CheckBox.TableControl = null;
        }
        this.Table.CheckBox.parentNode.parentNode.parentNode.parentNode.className = 'tableRow';
    }

    this.WindowElements[1].vAlign = 'top';
    this.WindowElements[1].appendChild(this.Window.Title);
    this.WindowElements[1].appendChild(this.Window.CloseButton);

    for (var i = 0; i < 9; i++) {
        this.WindowElements[i].className = this.WindowElementsClasses[i];
        this.WindowElements[i].Table = this;

        if (i == 0 || i == 2 || i == 6 || i == 8) {
            var di = this.Document.createElement('IMG');
            di.style.visibility = 'hidden';
            di.style.width = this.WindowElements[i].style.width;
            di.style.height = this.WindowElements[i].style.height;
            this.WindowElements[i].appendChild(di);
        }
    }

    this.Window.BottomInfo = this.Document.createElement('DIV');
    this.Window.BottomInfo.Table = this;
    this.Window.BottomInfo.style.display = 'none';

    this.Window.Container = this.Document.createElement('DIV');
    this.Window.Container.style.width = w - 12;
    this.Window.Container.style.height = h - 30;
    this.Window.Container.style.backgroundColor = 'white';
    this.Window.Container.style.overflow = 'auto';
    this.Window.Container.className = 'blindFrameDropShadow';
    this.Window.Container.onscroll = function () { autoComplete.Hide(); };

    this.WindowElements[4].appendChild(this.Window.Container);
    this.WindowElements[4].appendChild(this.Window.BottomInfo);

    this.Window.Table = this;


    this.WindowElements[1].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetX = localCore.Mouse.X - this.Table.Window.offsetLeft - this.Table.Container.scrollLeft; // - ctPos[0];
        this.Table.Window.OffsetY = localCore.Mouse.Y - this.Table.Window.offsetTop - this.Table.Container.scrollTop; // - ctPos[1];

        this.Table.Container.MouseMoveEvent = 'move';
        this.Table.Container.IsMouseDown = true;

    };

    this.WindowElements[3].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetX = localCore.Mouse.X - (this.Table.Window.offsetLeft) - this.Table.Container.scrollLeft;
        this.Table.Window.OffsetW = this.Table.Window.offsetWidth + this.Table.Window.offsetLeft;

        this.Table.Container.MouseMoveEvent = 'rz-l';
        this.Table.Container.IsMouseDown = true;
    };
    this.WindowElements[5].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetX = localCore.Mouse.X - (this.Table.Window.offsetWidth) - this.Table.Container.scrollLeft;
        this.Table.Container.MouseMoveEvent = 'rz-r';
        this.Table.Container.IsMouseDown = true;
    };
    this.WindowElements[6].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetX = localCore.Mouse.X - (this.Table.Window.offsetLeft) - this.Table.Container.scrollLeft;
        this.Table.Window.OffsetW = this.Table.Window.offsetWidth + this.Table.Window.offsetLeft;
        this.Table.Window.OffsetY = localCore.Mouse.Y - (this.Table.Window.offsetHeight) - this.Table.Container.scrollTop;

        this.Table.Container.MouseMoveEvent = 'rz-bl';
        this.Table.Container.IsMouseDown = true;
    };
    this.WindowElements[7].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetY = localCore.Mouse.Y - (this.Table.Window.offsetHeight) - this.Table.Container.scrollTop;
        this.Table.Container.MouseMoveEvent = 'rz-b';
        this.Table.Container.IsMouseDown = true;
    };
    this.WindowElements[8].onmousedown = function () {
        this.Table.Window.Focus();
        this.Table.Window.OffsetX = localCore.Mouse.X - (this.Table.Window.offsetWidth) - this.Table.Container.scrollLeft;
        this.Table.Window.OffsetY = localCore.Mouse.Y - (this.Table.Window.offsetHeight) - this.Table.Container.scrollTop;
        this.Table.Container.MouseMoveEvent = 'rz-br';
        this.Table.Container.IsMouseDown = true;
    };


    this.Window.onmouseup = function () {
        this.Table.Container.IsMouseDown = false;
        this.Focus();
    };

    this.Window.onmousemove = function () {

    };

    this.Window.Focus = function () {
        this.Table.Container.CurrentZindex++;
        this.Table.Window.style.zIndex = this.Table.Container.CurrentZindex;

        for (var i = 0; i < 9; i++) {
            this.Table.WindowElements[i].className = this.Table.WindowElementsClasses[i];
        }

        if (this.Table.Container.SelectedTable && this.Table.Container.SelectedTable != this.Table)
            this.Table.Container.SelectedTable.Window.Blur();

        this.Table.Container.SelectedTable = this.Table;

    };

    this.Window.Blur = function () {
        for (var i = 0; i < 9; i++) {
            this.Table.WindowElements[i].className = this.Table.WindowElementsClassesU[i];
        }

    };

    this.Window.SetPointingArrows = function () {
        //Build the arrows if they dont exists!
        if (!this.ArrowH) {
            this.ArrowH = this.Table.Document.createElement('IMG');
            this.ArrowV = this.Table.Document.createElement('IMG');
            this.ArrowA = this.Table.Document.createElement('IMG');

            this.ArrowH.style.position = 'absolute';
            this.ArrowV.style.position = 'absolute';
            this.ArrowA.style.position = 'absolute';

            this.ArrowH.src = 'images/reportGenerator/point.gif';
            this.ArrowV.src = 'images/reportGenerator/point.gif';
            this.ArrowA.src = 'images/reportGenerator/left.gif';

            this.ArrowH.style.height = 1;
            this.ArrowV.style.width = 1;

            this.Table.Container.appendChild(this.ArrowH);
            this.Table.Container.appendChild(this.ArrowV);
            this.Table.Container.appendChild(this.ArrowA);

        }

        //Set the arrow correctly (Vertical)

        //Set the arrows correctly (Horizontal)
        this.Table.__FixPointingArrows();

    };

    this.Window.Resize = function (x, y, w, h) {
        if (w) if (w < 90) {
            if (x) x -= (90 - w);
            w = 90;
        }
        if (h) if (h < 32) {
            if (y) y -= (32 - h);
            h = 32;
        }

        this.style.left = x || this.style.left;
        this.style.top = y || this.style.top;

        if (w) this.Table.WindowElements[1].style.width = w - 12;
        if (h) this.Table.WindowElements[3].style.height = h - 30;

        if (w) this.Table.WindowElements[4].style.width = w - 12;
        if (h) this.Table.WindowElements[4].style.height = h - 30;

        if (h) this.Table.WindowElements[5].style.height = h - 30;

        if (w) this.Table.WindowElements[7].style.width = w - 12;

        if (w) this.Container.style.width = w - 12;
        if (h) this.Container.style.height = h - 30 - this.BottomInfo.offsetHeight;

        if (w) this.Title.style.width = w - (this.Table.Report.ReportGenerator.isIE ? 37 : 56);

        if (w) this.style.width = w;
        if (h) this.style.height = h;

        if (this.ArrowH) this.SetPointingArrows();
        for (var i = 0; i < this.ArrowsPointingToMeFrom.length; i++) {
            if (this.ArrowsPointingToMeFrom[i].ArrowH)
                this.ArrowsPointingToMeFrom[i].SetPointingArrows();
        }

    };

    this.Container.appendChild(this.Window);

    this.Window.Focus();

    this.LoadContent(this.Table, loadFieldsAndFilters, lockTableAndCheckItem);

    return this;
};

ReportTable.prototype.Remove = function () {
    autoComplete.Hide();
    for (var i = this.Report.Tables.length - 1; i >= 0; i--) {

        if (this.Report.Tables[i].Window.ArrowsPointingTo == this.Window && this.Report.Tables[i].Window) {
            this.Container.removeChild(this.Report.Tables[i].Window.ArrowH);
            this.Container.removeChild(this.Report.Tables[i].Window.ArrowV);
            this.Container.removeChild(this.Report.Tables[i].Window.ArrowA);
            this.Report.Tables[i].Window.ArrowH = null;
            this.Report.Tables[i].Window.ArrowV = null;
            this.Report.Tables[i].Window.ArrowA = null;
        }

        if (this.Report.Tables[i] == this) {
            this.Report.Tables.splice(i, 1);
        }
    }

    //Remove my own arrows
    if (this.Window.ArrowH) {
        this.Container.removeChild(this.Window.ArrowH);
        this.Container.removeChild(this.Window.ArrowV);
        this.Container.removeChild(this.Window.ArrowA);

        this.Window.ArrowH = null;
        this.Window.ArrowV = null;
        this.Window.ArrowA = null;
    }
    this.Container.removeChild(this.Window);
};

ReportTable.prototype.LoadContent = function (name, loadFieldsAndFilters, lockTableAndCheckItem) {
    while (this.Window.Container.hasChildNodes()) {
        this.Window.Container.removeChild(this.Window.Container.lastChild);
    }

    var wait = this.Document.createElement('IMG');
    wait.src = 'images/wait_small.gif';

    wait.style.marginTop = 137;
    wait.style.marginLeft = 87;
    this.Window.Container.appendChild(wait);

    var AjaxObject = new localCore.Ajax();
    AjaxObject.requestFile = 'ajax/statistics_ReportGeneratorLoadTable.aspx';

    AjaxObject.encVar('table', name);

    var me = this;
    AjaxObject.OnCompletion = function () { me._LoadContent(AjaxObject, loadFieldsAndFilters, lockTableAndCheckItem); };
    AjaxObject.OnError = function () { me._LoadContent(AjaxObject, loadFieldsAndFilters, lockTableAndCheckItem); };
    AjaxObject.OnFail = function () { me._LoadContent(AjaxObject, loadFieldsAndFilters, lockTableAndCheckItem); };
    AjaxObject.RunAJAX();
};

ReportTable.prototype._LoadContent = function (AjaxObject, loadFieldsAndFilters, lockTableAndCheckItem) {
    var result = AjaxObject.Response;

    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var div = this.Document.createElement('DIV');
    div.innerHTML = result;

    while (this.Window.Container.hasChildNodes()) {
        this.Window.Container.removeChild(this.Window.Container.lastChild);
    }



    var obj = div.firstChild;
    this.Window.Uniqueidentifier = obj.id;
    this.Window.sqlJoinBefore = obj.getAttribute('sqlJoinBefore');
    this.Window.sqlObject = obj.getAttribute('sqlObject');
    this.Window.htmlObject = obj.getAttribute('htmlObject');
    this.Window.sqlJoinTo = obj.getAttribute('sqlJoinTo');
    this.Window.mapable = obj.getAttribute('mapable');
    this.Window.sqlOrderBy = obj.getAttribute('sqlOrderBy');

    for (var i = 0; i < this.Report.Tables.length; i++) {
        if (this.Window.htmlObject) {
            if (this.Report.Tables[i].Window.Uniqueidentifier == this.Window.htmlObject) {
                //alert('point me to: ' + this.Report.Tables[i].Window.Uniqueidentifier);
                this.Window.ArrowH = null;
                this.Window.ArrowV = null;
                this.Window.ArrowA = null;
                this.Window.ArrowsPointingTo = this.Report.Tables[i].Window;
                this.Report.Tables[i].Window.ArrowsPointingToMeFrom.push(this.Window);
                this.Window.SetPointingArrows();

            }
        }

        if (this.Report.Tables[i].Window.htmlObject) {
            if (this.Report.Tables[i].Window.htmlObject == this.Window.Uniqueidentifier) {
                //alert('point to me from: ' + this.Report.Tables[i].Window.Uniqueidentifier);
                this.Report.Tables[i].Window.ArrowH = null;
                this.Report.Tables[i].Window.ArrowV = null;
                this.Report.Tables[i].Window.ArrowA = null;
                this.Report.Tables[i].Window.ArrowsPointingTo = this.Window;
                this.Window.ArrowsPointingToMeFrom.push(this.Report.Tables[i].Window);
                this.Report.Tables[i].Window.SetPointingArrows();
            }
        }

    }

    //Is this table relating to another table?

    for (var i = 0; i < this.Report.AvailableTables.childNodes.length; i++) {
        if (this.Window.htmlObject && this.Report.AvailableTables.childNodes[i].table) {
            if (this.Window.htmlObject.toLowerCase() == 'tbl_' + this.Report.AvailableTables.childNodes[i].table.toLowerCase()) {
                var tbl = this.Report.AvailableTables.childNodes[i];
                this.Window.BottomInfo.style.display = '';
                this.Window.BottomInfo.className = 'windowBottomInfoWithBorder';
                this.Window.BottomInfo.innerHTML = 'Kan linkes med <b>' + tbl.innerHTML + '</b>';
                this.Window.BottomInfo.style.backgroundImage = 'url(' + tbl.icon + ')';
                this.Window.BottomInfo.LinkingTable = tbl.table;

                this.Window.BottomInfo.onclick = function () {
                    var cbs = this.Table.Report.AvailableTablesControl.getElementsByTagName('INPUT');
                    for (var i = 0; i < cbs.length; i++) {
                        var cb = cbs[i];
                        if (cb.Table == this.LinkingTable) {
                            if (!cb.checked) {
                                cb.checked = true;
                                cb.onclick();
                            }
                            else {
                                cb.TableControl.Window.Focus();
                            }
                            return;
                        }
                    }

                };

                this.Window.Container.style.height = this.Window.Container.offsetHeight - this.Window.BottomInfo.offsetHeight;
            }
        }
    }


    while (obj.firstChild) {
        var o = obj.removeChild(obj.firstChild);
        var o2 = this._GetFilter(o, loadFieldsAndFilters, lockTableAndCheckItem);
        if (o2)
            this.Window.Container.appendChild(o2);
    }

};

ReportTable.prototype._GetFilter = function (o, loadFieldsAndFilters, lockTableAndCheckItem) {
    if (o.tagName != 'HR' && o.tagName != 'B' && o.tagName != 'U') {
        return;
    }

    var tbl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');

    tbl.border = 0;
    tbl.cellPadding = 0;
    tbl.cellSpacing = 0;

    if (o.tagName == 'HR') {
        tbl.appendChild(tb);
        tbl.style.width = '100%';
        tb.appendChild(tr);
        tr.appendChild(td1);
        td1.innerHTML = '<hr>';
        return tbl;
    }
    else if (o.tagName == 'U') {
        tbl.appendChild(tb);
        tbl.style.width = '100%';
        tb.appendChild(tr);
        tr.appendChild(td1);
        td1.innerHTML = o.innerHTML;
        td1.style.fontWeight = 'bold';
        td1.style.padding = 5;
        td1.style.borderTop = 'solid 1px #7da2ce';
        td1.style.backgroundImage = 'url(../images/tableRowSelected2.gif)';
        td1.noWrap = true;
        return tbl;
    }

    var trFilters = this.Document.createElement('TR');
    var tdFilters = this.Document.createElement('TD');
    tdFilters.colSpan = 4;
    tdFilters.className = 'tableSubRow';

    trFilters.appendChild(tdFilters);
    trFilters.style.display = 'none';

    var td2 = this.Document.createElement('TD');
    var td3 = this.Document.createElement('TD');
    var td4 = this.Document.createElement('TD');
    var iconAdd = this.Document.createElement('IMG');
    iconAdd.src = 'images/addFilter.gif';
    iconAdd.style.marginLeft = 2;
    iconAdd.style.marginRight = 2;
    td4.appendChild(iconAdd);

    var td5 = this.Document.createElement('TD');
    var iconAdd5 = this.Document.createElement('IMG');
    iconAdd5.src = 'images/eye32.png';
    iconAdd5.width = 16;
    iconAdd5.height = 16;
    td5.appendChild(iconAdd5);


    td4.title = 'Nyt filter';
    td4.style.cursor = 'pointer';
    td4.style.display = 'none';

    var cb = this.Document.createElement('INPUT');
    cb.type = 'checkbox';

    if (!this.Report.ReportGenerator.isIE) {
        cb.style.marginTop = 1;
        cb.style.marginBottom = 2;
    }

    //var img = this.Document.createElement('IMG');
    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    tr.appendChild(td2);

    // add sensitive icon if field is sensitive
    if (o.getAttribute('sensitive') == "1") {
        tr.appendChild(td5);
    }

    tr.appendChild(td3);
    tr.appendChild(td4);
    td1.appendChild(cb);
    //td2.appendChild(img);

    tb.appendChild(trFilters);

    td1.style.paddingTop = (this.Report.ReportGenerator.isIE ? 2 : 0);
    td1.style.paddingBottom = (this.Report.ReportGenerator.isIE ? 2 : 0);

    td2.innerHTML = o.innerHTML;
    td2.noWrap = true;
    td2.style.width = '100%';

    cb.AddFilterButton = td4;
    cb.Uniqueidentifier = o.id;
    cb.dbOutputAs = o.getAttribute('dbOutputAs');
    cb.dbName = o.getAttribute('dbName');
    cb.dbType = o.getAttribute('dbType');
    cb.sensitive = o.getAttribute('sensitive');
    var dbNameMatch = cb.dbName.split('.[');

    td2.onclick = function () { if (cb.disabled) return; cb.checked = !cb.checked; cb.onclick(); };
    td1.style.cursor = 'pointer';
    td2.style.cursor = 'pointer';

    tbl.className = 'tableRow';

    var me = this;
    cb.ReportTable = this;
    cb.FiltersControl = tdFilters;
    cb.Filters = new Array();

    this.Fields.push(cb);

    cb.onclick = function () {
        if (this.disabled) return;
        this.ReportTable.IsDirty = true;

        if (this.checked) {
            autoComplete.Hide();
            trFilters.style.display = tdFilters.childNodes.length > 0 ? '' : 'none';
            td4.style.display = '';
            //this.TableControl = me.AddTable(this.Name, this.Icon, this.Table, this); 
            this.parentNode.parentNode.parentNode.parentNode.className = tdFilters.childNodes.length > 0 ? 'tableRowSelectedSpaced' : 'tableRowSelected';
        }
        else {
            autoComplete.Hide();
            /*if(this.TableControl)
            {
            this.TableControl.Remove();
            this.TableControl=null;
            }*/
            trFilters.style.display = 'none';
            td4.style.display = 'none';
            this.parentNode.parentNode.parentNode.parentNode.className = 'tableRow';
        }
    };
    if (dbNameMatch.length > 0 && lockTableAndCheckItem != null) {
        var items = lockTableAndCheckItem.split('|');
        for (var i = 0; i < items.length; i++) {
            if (dbNameMatch[dbNameMatch.length - 1].toLowerCase() == items[i].toLowerCase()) {
                {
                    cb.checked = true;
                    cb.onclick();
                    cb.disabled = true;
                }
            }
        }
    }

    cb.AddFilter = function (compareMethod, value, unEditable) {
        autoComplete.Hide();
        this.ReportTable.IsDirty = true;

        var doc = this.ReportTable.Document;
        var tbl = doc.createElement('TABLE');
        var tb = doc.createElement('TBODY');
        var tr = doc.createElement('TR');
        var td1 = doc.createElement('TD');
        var td2 = doc.createElement('TD');
        var td3 = doc.createElement('TD');
        var td4 = doc.createElement('TD');
        var tdType = doc.createElement('TD');

        var typeIcon = doc.createElement('IMG');
        typeIcon.src = 'images/reportGenerator/' + cb.dbType + '.gif';
        typeIcon.style.marginRight = 2;
        tdType.appendChild(typeIcon);

        var dummyIcon = doc.createElement('IMG');
        dummyIcon.src = 'images/filter.png';
        td1.appendChild(dummyIcon);



        tbl.appendChild(tb);
        tb.appendChild(tr);
        tr.appendChild(td1);
        tr.appendChild(td2);
        tr.appendChild(tdType);
        tr.appendChild(td3);
        tr.appendChild(td4);

        tbl.cellPadding = 0;
        tbl.cellSpacing = 0;
        tbl.border = 0;
        tbl.style.width = '100%';


        var input = null;

        if (this.dbType == 'boolean' || this.dbType == 'checkbox') {
            input = doc.createElement('SELECT');
            input.options.add(new Option('Ja', '1'));
            input.options.add(new Option('Nej', '0'));
        }
        else if (this.dbType == 'datetime') {

            input = doc.createElement('INPUT');
            input.ReportTable = this.ReportTable;
            input.dateTimePicker = new DatePicker(doc, this.ReportTable.Report.Container, top.window);
            input.dateTimePicker.OnPickDate = function (value) {
                var y = value.getFullYear().toString();
                var m = (value.getMonth() + 1).toString();
                var d = value.getDate().toString();

                if (m.length == 1) m = '0' + m;
                if (d.length == 1) d = '0' + d;

                input.value = d + '-' + m + '-' + y;
            };


            input.onfocus = function () { var pos = localCore.DOM.GetObjectPosition(this); this.dateTimePicker.ShowStatic(pos[0], pos[1] + this.offsetHeight - this.ReportTable.Window.Container.scrollTop); };
        }
        else if (this.dbType == 'partnertype') {
            input = doc.createElement('SELECT');
            input.options.add(new Option('Ikke angivet', 'Ikke angivet'));
            input.options.add(new Option('Henvisningskontakt', 'Henvisningskontakt'));
            input.options.add(new Option('Mentorkontakt', 'Mentorkontakt'));
            input.options.add(new Option('Samarbejdskontakt', 'Samarbejdskontakt'));
        }
        else {
            input = doc.createElement('INPUT');
        }

        input.value = value || '';
        input.style.width = '100%';
        input.className = 'designerTextbox';
        td3.appendChild(input);
        td3.style.width = '100%';

        if (!this.ReportTable.Report.ReportGenerator.isIE) {
            input.style.marginTop = 1;
            input.style.marginBottom = 2;
        }



        td3.style.paddingTop = (this.ReportTable.Report.ReportGenerator.isIE ? 2 : 0);
        td3.style.paddingBottom = (this.ReportTable.Report.ReportGenerator.isIE ? 2 : 0);

        td4.style.cursor = 'pointer';

        if (unEditable) {
            input.disabled = true;
        }
        else {
            td4.onclick = function () {
                autoComplete.Hide();
                cb.ReportTable.IsDirty = true;

                for (var i = cb.Filters.length - 1; i >= 0; i--) {
                    if (cb.Filters[i] == input) {
                        cb.Filters.splice(i, 1);
                    }
                }
                tbl.parentNode.removeChild(tbl);
                trFilters.style.display = tdFilters.childNodes.length > 0 ? '' : 'none';
                cb.parentNode.parentNode.parentNode.parentNode.className = tdFilters.childNodes.length > 0 ? 'tableRowSelectedSpaced' : 'tableRowSelected';
            };
            td4.CloseIcon = doc.createElement('IMG');

            td4.CloseIcon.src = 'images/spacer.gif';
            td4.CloseIcon.className = 'tabPageCloseButton';
            td4.appendChild(td4.CloseIcon);
            td4.onmouseover = function () { this.CloseIcon.className = 'tabPageCloseButtonHover'; };
            td4.onmouseout = function () { this.CloseIcon.className = 'tabPageCloseButton'; };
        }

        this.FiltersControl.appendChild(tbl);

        trFilters.style.display = tdFilters.childNodes.length > 0 ? '' : 'none';
        cb.parentNode.parentNode.parentNode.parentNode.className = tdFilters.childNodes.length > 0 ? 'tableRowSelectedSpaced' : 'tableRowSelected';

        if (!input.onfocus) {
            input.onkeyup = function () {
                if (this.value != '') {
                    /*var sql = 'SELECT DISTINCT TOP 5 ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' AS RESULT FROM ' + 
                    cb.ReportTable.Window.sqlObject + 
                    ' WHERE ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' LIKE \'' + this.value.replace(/\'/g,'\'\'') + '%\' ' + 
                                                                    
                    'union ' + 
                                                                    
                    'SELECT DISTINCT TOP 5 ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' AS RESULT FROM ' + 
                    cb.ReportTable.Window.sqlObject + 
                    ' WHERE ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' LIKE \'%' + this.value.replace(/\'/g,'\'\'') + '%\' ORDER BY ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName);
                    var sql =   'SELECT DISTINCT TOP 5 ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' AS RESULT FROM ' + 
                    cb.ReportTable.Window.sqlObject + 
                    ' WHERE ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName) + 
                    ' LIKE \'%' + this.value.replace(/\'/g,'\'\'') + '%\' ORDER BY ' + 
                    cb.ReportTable.Report.FormatFieldOutput( cb.dbType , cb.dbName);*/

                    var sql = 'select TOP 5 RESULT from\n' +
	                                                              '      (\n' +
		                                                          '          select ' + cb.ReportTable.Report.FormatFieldOutput(cb.dbType, 'RESULT') + ' as RESULT from\n' +
		                                                          '          (\n' +
			                                                      '              SELECT distinct\n' +
			                                                      '              convert(nvarchar(max),[' + cb.dbName + ']) AS RESULT FROM ' + cb.ReportTable.Window.sqlObject + '\n' +
		                                                          '          ) as TBL\n' +
	                                                              '      ) as TBL2\n' +
	                                                              '      WHERE RESULT LIKE \'%' + this.value.replace(/\'/g, '\'\'') + '%\' ORDER BY RESULT';

                    var pos = localCore.DOM.GetObjectPosition(this);
                    autoComplete.Show(pos[0] - cb.ReportTable.Window.Container.scrollLeft, pos[1] + this.offsetHeight - cb.ReportTable.Window.Container.scrollTop, this, sql);
                }
                else
                    autoComplete.Hide();
            };

            input.onblur = function () {

            };
        }

        this.Filters.push(input);
        if (compareMethod == '~') compareMethod = '≃';

        input.CompareMethod = compareMethod || '=';
        if (this.ReportTable.Report.ReportGenerator.isIE)
            td2.innerText = input.CompareMethod;
        else
            td2.textContent = input.CompareMethod;

        td2.style.paddingRight = 3;
        td2.style.fontSize = '16px';

        td1.style.cursor = 'pointer';
        td2.style.cursor = 'pointer';
        tdType.style.cursor = 'pointer';

        td1.ReportTable = this.ReportTable;
        td2.ReportTable = this.ReportTable;
        tdType.ReportTable = this.ReportTable;

        td1.dbType = this.dbType;
        td2.dbType = this.dbType;
        tdType.dbType = this.dbType;

        td2.noWrap = true;

        td1.onclick = function () {
            this.ReportTable.IsDirty = true;

            switch (this.dbType) {
                case 'dropdown':
                case 'listview':
                case 'memo':
                case 'organisationid':
                case 'partnertype':
                case 'textbox':
                case 'dropdown':
                case 'emailaddress':
                case 'userid':
                case 'userinitials':
                case 'userrole':
                    var pos = localCore.DOM.GetObjectPosition(td1);
                    var ct = this.ReportTable.Report.ReportGenerator.ContextCompareText;

                    ct.Items[0].OnClick = function () { td2.innerHTML = '='; input.CompareMethod = '='; };
                    ct.Items[1].OnClick = function () { td2.innerHTML = '≠'; input.CompareMethod = '≠'; };
                    ct.Items[2].OnClick = function () { td2.innerHTML = '≃'; input.CompareMethod = '≃'; };
                    ct.Items[3].OnClick = function () { td2.innerHTML = '!≃'; input.CompareMethod = '!≃'; };
                    //ct.Items[3].OnClick = function () { td2.innerHTML = 'ǂ'; input.CompareMethod = 'ǂ'; };

                    ct.ShowStatic(pos[0], pos[1] + td1.offsetHeight - this.ReportTable.Window.Container.scrollTop);
                    break;

                case 'absfloat':
                case 'absinteger':
                case 'float':
                case 'integer':
                case 'datetime':
                    var pos = localCore.DOM.GetObjectPosition(td1);
                    var ct = this.ReportTable.Report.ReportGenerator.ContextCompareOther;

                    ct.Items[0].OnClick = function () { td2.innerHTML = '='; input.CompareMethod = '='; };
                    ct.Items[1].OnClick = function () { td2.innerHTML = '≠'; input.CompareMethod = '≠'; };
                    ct.Items[2].OnClick = function () { td2.innerHTML = '&gt;'; input.CompareMethod = '>'; };
                    ct.Items[3].OnClick = function () { td2.innerHTML = '&lt;'; input.CompareMethod = '<'; };
                    ct.Items[4].OnClick = function () { td2.innerHTML = '≥'; input.CompareMethod = '≥'; };
                    ct.Items[5].OnClick = function () { td2.innerHTML = '≤'; input.CompareMethod = '≤'; };
                    //ct.Items[6].OnClick = function () { td2.innerHTML = 'ǂ'; input.CompareMethod = 'ǂ'; };

                    ct.ShowStatic(pos[0], pos[1] + td1.offsetHeight - this.ReportTable.Window.Container.scrollTop);
                    break;


                case 'checkbox':
                case 'boolean':
                    var pos = localCore.DOM.GetObjectPosition(td1);
                    var ct = this.ReportTable.Report.ReportGenerator.ContextCompareBool;

                    ct.Items[0].OnClick = function () { td2.innerHTML = '='; input.CompareMethod = '='; };
                    ct.Items[1].OnClick = function () { td2.innerHTML = '≠'; input.CompareMethod = '≠'; };
                    //ct.Items[3].OnClick = function () { td2.innerHTML = 'ǂ'; input.CompareMethod = 'ǂ'; };

                    ct.Show(pos[0], pos[1] + td1.offsetHeight - this.ReportTable.Window.Container.scrollTop);
                    break;
            }
        };

        td2.onclick = td1.onclick;
        tdType.onclick = td1.onclick;

        if (!value) {
            var pos = localCore.DOM.GetObjectPosition(td1);
            new TinyBubble(document, document.body, pos[0] + (td1.offsetWidth / 2), pos[1] + td1.offsetHeight - this.ReportTable.Window.Container.scrollTop, 'Tryk for at vælge en anden metode<br>for sammenligning af filtret', 1500);
        }
    };

    if (loadFieldsAndFilters) {
        //alert(o.id);
        var chosenFields = this.Report.LoadData.getElementsByTagName('u');
        var chosenFilters = this.Report.LoadData.getElementsByTagName('input');
        for (var i = 0; i < chosenFields.length; i++) {
            if (o.id == chosenFields[i].getAttribute('uid') || o.getAttribute('alternateId') == chosenFields[i].getAttribute('uid')) {
                cb.checked = true;
                cb.onclick();

                //Add filters if any
                for (var u = 0; u < chosenFilters.length; u++) {
                    if (chosenFilters[u].getAttribute('fieldId') == o.id) {
                        cb.AddFilter(chosenFilters[u].getAttribute('condition'), chosenFilters[u].value);
                    }
                }
            }
        }
    }

    td4.onclick = function () { cb.AddFilter(); };

    return tbl;
};

Report.prototype.ToSQL = function (numResults, includeSensitive) {

    // assign default value
    if (includeSensitive == undefined) {
        includeSensitive = true;
    }

    var selects = new Array();
    var currentSelect = null;
    for (var i = 0; i < this.Tables.length; i++) {
        //HANDLE TABLES AND JOINS
        var tbl = this.Tables[i];

        //Check if the table should join to something, which we have selected in the tables.
        var tblJoinTo = this.GetJoiningTableFromTables(tbl);

        if (tblJoinTo) {
            currentSelect = this.GetOrCreateSelectFromTable(selects, tblJoinTo);
            //currentSelect.Joins +='\n\t-- Join before: ' + tbl.Window.sqlJoinBefore + '\n\t' + tbl.Window.sqlJoinTo;
            currentSelect.AddJoin(tbl.Window.sqlJoinTo, tbl.Window.Uniqueidentifier, tbl.Window.sqlJoinBefore);
        }
        else {
            currentSelect = this.GetOrCreateSelectFromTable(selects, tbl);
        }

        // Allow "Nøgle" field to be SQL part when sensitive field selected from report generator or it's already selected by user.
        var allowNogle = false;
        for (var u = 0; u < tbl.Fields.length; u++) {
            var field = tbl.Fields[u];
            if (field.checked) {
                if (field.dbOutputAs == 'Nøgle') { allowNogle = true; }
                if (field.sensitive != undefined && (field.sensitive == "1" && includeSensitive == true)) { allowNogle = true; }
            }
        }

        //FIELDS FOR THE CURRENT SELECT
        for (var u = 0; u < tbl.Fields.length; u++) {
            var field = tbl.Fields[u];
            var checked = field.checked;

            // Add "Nøgle" field
            if (allowNogle && field.dbOutputAs == 'Nøgle') { checked = true; }
            if (checked) {
                var allowField = true;
                if (field.sensitive != undefined && (field.sensitive == "1" && includeSensitive == false)) { allowField = false; }
                if (allowField) {
                    if (!currentSelect.FieldsArray[tbl.Window.Uniqueidentifier]) {
                        currentSelect.FieldsArray[tbl.Window.Uniqueidentifier] = '';
                    }
                    currentSelect.FieldsArray[tbl.Window.Uniqueidentifier] += (currentSelect.FieldsArray[tbl.Window.Uniqueidentifier] == '' ? '' : ',') + '\n\t' + this.FormatFieldOutput(field.dbType, field.dbName) + ' AS [' + field.dbOutputAs + '@@unify:' + tbl.Name + ']';
                    for (var y = 0; y < field.Filters.length; y++) {
                        var filter = field.Filters[y];
                        if (filter) {
                            currentSelect.Filters += (currentSelect.Filters == '' ? '' : ' AND\n\t') + this.FormatFieldFilter(field.dbType, field.dbName, filter.value, filter.CompareMethod, tbl.Window.sqlJoinTo);
                            currentSelect.FiltersArray.push(new ReportSelectFilter(filter.CompareMethod, this.FormatFieldFilter(field.dbType, field.dbName, filter.value, filter.CompareMethod, tbl.Window.sqlJoinTo), field.dbName));
                        }
                    }
                }
            }
        }
    }

    var sql = '';
    for (var i = 0; i < selects.length; i++) {
        sql += (sql == '' ? '' : '\n\n\n\n') + selects[i].ToSQL(numResults);
    }

    
    if (tbl.Window.sqlOrderBy != undefined && tbl.Window.sqlOrderBy != null && tbl.Window.sqlOrderBy != '') {
        sql = sql + '\nORDER BY ' + tbl.Window.sqlOrderBy;
    }
    return sql;
};

Report.prototype.AnythingSensitive = function () {

    var countSensitiveFields = 0;
    var selects = new Array();
    var currentSelect = null;
    for (var i = 0; i < this.Tables.length; i++) {
        //HANDLE TABLES AND JOINS
        var tbl = this.Tables[i];

        //Check if the table should join to something, which we have selected in the tables.
        var tblJoinTo = this.GetJoiningTableFromTables(tbl);

        if (tblJoinTo) {
            currentSelect = this.GetOrCreateSelectFromTable(selects, tblJoinTo);
            //currentSelect.Joins +='\n\t-- Join before: ' + tbl.Window.sqlJoinBefore + '\n\t' + tbl.Window.sqlJoinTo;
            currentSelect.AddJoin(tbl.Window.sqlJoinTo, tbl.Window.Uniqueidentifier, tbl.Window.sqlJoinBefore);
        }
        else {
            currentSelect = this.GetOrCreateSelectFromTable(selects, tbl);
        }

        //FIELDS FOR THE CURRENT SELECT
        for (var u = 0; u < tbl.Fields.length; u++) {
            var field = tbl.Fields[u];

            if (field.checked && field.sensitive == "1") {
                countSensitiveFields++;
            }
        }
    }

    return (countSensitiveFields > 0 ? true : false);
};

Report.prototype.SelectedTables = function () {
    var tbls = '';
    var fields = '';
    for (var i = 0; i < this.Tables.length; i++) {
        var tbl = this.Tables[i];
        tbls = tbls + tbl.Window.sqlObject + '\n';
    }
    return tbls;
};

Report.prototype.Logs = function (includeSensitive) {
    // assign default value
    if (includeSensitive == undefined) {
        includeSensitive = true;
    }

    var fields = '';
    for (var i = 0; i < this.Tables.length; i++) {
        var tbl = this.Tables[i];
        for (var u = 0; u < tbl.Fields.length; u++) {
            var field = tbl.Fields[u];
            if ((field.checked) && (field.sensitive != undefined) && (field.sensitive == "1" && includeSensitive == true)) {
                fields = fields + field.dbName + '##[' + field.dbOutputAs + '@@unify:' + tbl.Name + ']' + ',';
            }
        }
    }
    return fields;
};

Report.prototype.FormatFieldFilter = function (dbType, fieldName, value, CompareMethod, JoinTo) {
    var comparer = this.FormatGetFieldComparer(CompareMethod);
    var fn = this.FormatFieldOutput(dbType, fieldName);

    switch (dbType) {
        case 'float':
        case 'absfloat':
            return !isNaN(parseFloat(value.replace(/,/g, '.'))) ? fn + comparer + parseFloat(value.replace(/,/g, '.')) : fn + this.FormatGetFieldComparer(CompareMethod, true);

        case 'integer':
        case 'absinteger':
            return !isNaN(parseInt(value, 10)) ? fn + comparer + parseInt(value, 10) : fn + this.FormatGetFieldComparer(CompareMethod, true);

        case 'datetime':

            if (comparer == '=')
                return value != '' ? 'CONVERT(varchar, ' + fn + ', 105)' + comparer + '\'' + value.replace(/'/g, '\'\'') + '\'' : 'CONVERT(varchar, ' + fn + ', 105)' + this.FormatGetFieldComparer(CompareMethod, true);
            else
                return value != '' ? fn + comparer + '\'' + value.replace(/'/g, '\'\'') + '\'' : fn + this.FormatGetFieldComparer(CompareMethod, true);

        case 'boolean':
        case 'checkbox':
            return 'isnull(convert(int,' + fn + '),2) ' + comparer + ' ' + value;

        case 'dropdown':
        case 'listview':
        case 'memo':
        case 'organisationid':
        case 'partnertype':
        case 'textbox':
        case 'userid':
        case 'userinitials':

            if (comparer == ' LIKE ' || comparer == ' NOT LIKE ')
                return 'isnull(' + fn + ',\'\') ' + comparer + '\'%' + value.replace(/'/g, '\'\'') + '%\'';

                /// "NOT IN" FUNCTION ADDED TO REPORT GENERATOR
            else if (comparer == 'ǂ') {
                // Split the LEFT JOIN TABLE/FUNCTION statement into 2 sections 
                // Index 0, contains the what to join on, and the last index contains the matching field.
                var joinParts = JoinTo.split(' = ');
                if (joinParts.length > 1) {

                    // Get joining field, by splitting the last item with table field seperator.
                    var joinField = joinParts[joinParts.length - 1].split('.');

                    // Get table/function name from the join parts
                    var joinOn = joinParts[0].split(' ');

                    //     TABLE JOIN FIELD VALUE                            JOINED TABLE IDENTITY                   JOIN TABLE/FUNCTION NAME          ORIGINAL FIELD QUALIFIER
                    //     Splitted, to only get first item                  Splitted, to only get first item        Splitted, to only get first item  Parse original qualifier to match the not in select
                    return joinParts[1].split(' ')[0] + ' not in (select ' + joinField[1].split(' ')[0] + ' from ' + joinOn[2].split(' ')[0] + ' where isnull(' + fn + ',\'\') = \'' + value.replace(/'/g, '\'\'') + '\')';
                }
                else
                    // SOMETHING IS WRONG WITH THE JOIN - REVERT BACK TO STANDARD WHERE STATEMENT
                    return 'isnull(' + fn + ',\'\') <>\'' + value.replace(/'/g, '\'\'') + '\'';
            }
                // END OF "NOT IN" FUNCTION

            else
                return 'isnull(' + fn + ',\'\') ' + comparer + '\'' + value.replace(/'/g, '\'\'') + '\'';

    }

    return fn + comparer + '\'' + value.replace(/'/g, '\'\'') + '\'';
};

Report.prototype.FormatGetFieldComparer = function (CompareMethod, asNull) {
    switch (CompareMethod) {
        case '!=':
        case '≠':
            return asNull ? ' is not null ' : '<>';
            break;

        case '~':
        case '≃':
            return asNull ? ' is null ' : ' LIKE ';
            break;

        case '!~':
        case '!≃':
            return asNull ? ' is not null ' : ' NOT LIKE ';
            break;

        case '≤':
            return asNull ? ' is null ' : '<=';
            break;

        case '≥':
            return asNull ? ' is null ' : '>=';
            break;

        case '&lt;':
            return asNull ? ' is null ' : '<';
            break;

        case '&gt;':
            return asNull ? ' is null ' : '>';
            break;

        case '&lt;&gt;':
            return asNull ? ' is not null ' : '<>';
            break;
    }

    return asNull ? ' is null ' : CompareMethod;
};

Report.prototype.FormatFieldOutput = function (dbType, field) {
    switch (dbType) {
        case 'userid':
            return 'dbo.getUserInfo([' + field + '])';

        case 'userrole':
            return 'dbo.getUserRole([' + field + '])';

        case 'userinitials':
            return 'dbo.getUserInitials([' + field + '])';

        case 'organisationid':
            return 'dbo.getOrganisationInfo([' + field + '])';

        case 'partnertype':
            return 'dbo.getPartnerType([' + field + '])';

        case 'memo':
        case 'listview':
        case 'dropdown':
            return 'convert(nvarchar(max),[' + field + '])';
    }

    return '[' + field + ']';
};

var ReportSelect = function (table, uniqueidentifier, joinToIdentifier, joinBeforeIdentifier) {
    this.Uniqueidentifier = uniqueidentifier;
    this.JoinToIdentifier = joinToIdentifier;

    //apparently the join before identifier is not used.
    this.JoinBeforeIdentifier = joinBeforeIdentifier;

    //this.Fields='';
    this.Table = table;
    //this.Joins='';
    this.Filters = '';
    this.FiltersArray = new Array();

    this.JoinsArrayObject = document.createElement('DIV');

    this.FieldsArray = new Array();

};

var ReportSelectFilter = function (comparer, value, field) {
    this.Comparer = comparer;
    this.Value = value;
    this.Field = field;
};

ReportSelect.prototype.AddJoin = function (sqlJoin, Uniqueidentifier, JoinBefore) {
    var div = document.createElement('div');
    if (document.all)
        div.innerText = sqlJoin;
    else
        div.textContent = sqlJoin;

    div.Uniqueidentifier = Uniqueidentifier;
    div.JoinBefore = JoinBefore;

    var objBefore = this.GetInsertBeforeJoin(JoinBefore);
    if (!objBefore)
        this.JoinsArrayObject.appendChild(div);
    else
        this.JoinsArrayObject.insertBefore(div, objBefore);
};

ReportSelect.prototype.GetInsertBeforeJoin = function (JoinBefore) {
    if (!JoinBefore) return null;
    var before = JoinBefore.split(',');

    for (var i = 0; i < this.JoinsArrayObject.childNodes.length; i++) {
        if (this._InArray(before, this.JoinsArrayObject.childNodes[i].Uniqueidentifier)) {
            return this.JoinsArrayObject.childNodes[i];
        }
    }
    return null;
};

ReportSelect.prototype._InArray = function (arr, val) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == val) return true;
    }
    return false;
};

Report.prototype.GetOrCreateSelectFromTable = function (selects, tbl) {
    for (var i = 0; i < selects.length; i++) {
        var sel = selects[i];
        if (sel.Uniqueidentifier == tbl.Window.Uniqueidentifier)
            return sel;
    }

    var selNew = new ReportSelect(tbl.Window.sqlObject, tbl.Window.Uniqueidentifier, tbl.Window.htmlObject, tbl.Window.sqlJoinBefore);
    selects.push(selNew);
    return selNew;
};

Report.prototype.GetJoiningTableFromTables = function (tbl, failover) {
    for (var i = 0; i < this.Tables.length; i++) {
        if (this.Tables[i].Window.Uniqueidentifier == tbl.Window.htmlObject) {
            var t2 = this.Tables[i];
            if (t2.Window.htmlObject)
                return this.GetJoiningTableFromTables(t2, t2);
            else
                return t2;
        }
    }
    return failover;
};

ReportSelect.prototype.ToSQL = function (numResults, includeSensitive) {
    var joins = '';
    var fields = '';

    fields = (this.FieldsArray[this.Uniqueidentifier] ? '\n' + this.FieldsArray[this.Uniqueidentifier] : '');

    for (var i = 0; i < this.JoinsArrayObject.childNodes.length; i++) {
        var obj = this.JoinsArrayObject.childNodes[i];
        joins += '\n\t' + (document.all ? obj.innerText : obj.textContent);

        fields += (this.FieldsArray[obj.Uniqueidentifier] ? (fields == '' ? '' : ',') + this.FieldsArray[obj.Uniqueidentifier] : '');
    }

    this.FiltersArray.sort(this.SortFilters);

    var lc = '';
    var fl = '';
    var lf = null;
    var nf = null;

    for (var i = 0; i < this.FiltersArray.length; i++) {
        var f = this.FiltersArray[i];


        var nc = null;
        if (this.FiltersArray.length > i + 1) {
            nc = this.FiltersArray[i + 1].Comparer;
            nf = this.FiltersArray[i + 1].Field;
        }

        //≃=

        // multiple likes or ='s?
        if ((f.Comparer == '=' || f.Comparer == '≃')) {
            //Beginning a new multiple?
            if ((lc != '=' && lc != '≃') || lf != f.Field) {
                //Was already in a multiple, which should be ended?
                if ((lc == '=' || lc == '≃') && lf != f.Field) {
                    fl += '\n\t) AND ';
                }
                else
                    fl += (fl == '' ? '' : ' AND ');

                fl += '\n\t(';
                fl += '\n\t\t' + f.Value;

                //last item?
                if (i == this.FiltersArray.length - 1)
                    fl += '\n\t)';

            }
            else {
                fl += ' OR \n\t\t' + f.Value;

                //last item?
                if (i == this.FiltersArray.length - 1)
                    fl += '\n\t)';
            }
        }
        else {
            //Was the last statement multiple likes or ='s?
            if ((lc == '=' || lc == '≃')) {
                fl += '\n\t) AND ';
                fl += '\n\t' + f.Value;
            }
            else
                fl += (fl == '' ? '' : ' AND \n\t') + f.Value;
        }

        lc = f.Comparer;
        lf = f.Field;
    }

    return '-- ' + this.Uniqueidentifier + '\n' +
            'SET DATEFORMAT DMY\n\nSELECT ' + (numResults != '' ? 'TOP ' + numResults + ' ' : '') + (fields == '' ? 'null as [Ingen felter valgt]' : fields) + '\nFROM ' + this.Table + joins + (this.Filters != '' ? '\nWHERE ' + fl : '');

};

ReportSelect.prototype.ToSQL = function (numResults, includeSensitive) {
    var joins = '';
    var fields = '';
    fields = (this.FieldsArray[this.Uniqueidentifier] ? '\n' + this.FieldsArray[this.Uniqueidentifier] : '');
    for (var i = 0; i < this.JoinsArrayObject.childNodes.length; i++) {
        var obj = this.JoinsArrayObject.childNodes[i];
        joins += '\n\t' + (document.all ? obj.innerText : obj.textContent);
        fields += (this.FieldsArray[obj.Uniqueidentifier] ? (fields == '' ? '' : ',') + this.FieldsArray[obj.Uniqueidentifier] : '');
    }
    this.FiltersArray.sort(this.SortFilters);

    var lc = '';
    var fl = '';
    var lf = null;
    var nf = null;
    for (var i = 0; i < this.FiltersArray.length; i++) {
        var f = this.FiltersArray[i];
        var nc = null;
        if (this.FiltersArray.length > i + 1) {
            nc = this.FiltersArray[i + 1].Comparer;
            nf = this.FiltersArray[i + 1].Field;
        }

        // multiple likes or ='s?
        if ((f.Comparer == '=' || f.Comparer == '≃')) {
            //Beginning a new multiple?
            if ((lc != '=' && lc != '≃') || lf != f.Field) {
                //Was already in a multiple, which should be ended?
                if ((lc == '=' || lc == '≃') && lf != f.Field) {
                    fl += '\n\t) AND ';
                }
                else
                    fl += (fl == '' ? '' : ' AND ');

                fl += '\n\t(';
                fl += '\n\t\t' + f.Value;

                //last item?
                if (i == this.FiltersArray.length - 1) {
                    fl += '\n\t)';
                }
            }
            else {
                fl += ' OR \n\t\t' + f.Value;

                //last item?
                if (i == this.FiltersArray.length - 1)
                    fl += '\n\t)';
            }
        }
        else {
            //Was the last statement multiple likes or ='s?
            if ((lc == '=' || lc == '≃')) {
                fl += '\n\t) AND ';
                fl += '\n\t' + f.Value;
            }
            else
                fl += (fl == '' ? '' : ' AND \n\t') + f.Value;
        }

        lc = f.Comparer;
        lf = f.Field;
    }

    return '-- ' + this.Uniqueidentifier + '\n' +
            'SET DATEFORMAT DMY\n\nSELECT ' + (numResults != '' ? 'TOP ' + numResults + ' ' : '') + (fields == '' ? 'null as [Ingen felter valgt]' : fields) + '\nFROM ' + this.Table + joins + (this.Filters != '' ? '\nWHERE ' + fl : '');

};

ReportSelect.prototype.SortFilters = function (a, b) {

    var aa = a.Field == b.Field ? (a.Comparer == '=' || a.Comparer == '≃' ? -1 : 1) : a.Field < b.Field ? -1 : 1;
    var bb = b.Field == a.Field ? (b.Comparer == '=' || b.Comparer == '≃' ? -1 : 1) : a.Field > b.Field ? -1 : 1;

    return aa - bb;
};

ReportTable.prototype.inRect = function (s1, s2, d1, d2) {
    return ((s1 >= d1 && s1 <= d2) || (s2 >= d1 && s2 <= d2)) ||
           ((d1 >= s1 && d1 <= s2) || (d2 >= s1 && d2 <= s2));
};

ReportTable.prototype.getAverrageFromPoints = function (s1, s2, d1, d2) {
    var p1 = s1 > d1 ? s1 : d1;
    var e1 = s2 < d2 ? s2 : d2;

    return ((e1 - p1) / 2) + p1;
};

ReportTable.prototype.__FixPointingArrows = function () {
    this.Window.ArrowH.style.display = '';
    this.Window.ArrowV.style.display = '';
    this.Window.ArrowA.style.display = '';


    //H - Pointer only
    if (this.inRect(this.Window.offsetTop, this.Window.offsetTop + this.Window.offsetHeight, this.Window.ArrowsPointingTo.offsetTop, this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight)) {
        this.Window.ArrowV.style.display = 'none';



        this.Window.ArrowH.style.top = this.getAverrageFromPoints(this.Window.offsetTop, this.Window.offsetTop + this.Window.offsetHeight, this.Window.ArrowsPointingTo.offsetTop, this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight);

        if (this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth < this.Window.offsetLeft) {
            this.Window.ArrowH.style.left = this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth;
            this.Window.ArrowH.style.width = this.Window.offsetLeft - (this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth);

            this.Window.ArrowA.src = 'images/reportGenerator/left.gif';
            this.Window.ArrowA.style.left = this.Window.ArrowH.offsetLeft;
            this.Window.ArrowA.style.top = this.Window.ArrowH.offsetTop - 3;
        }
        else if (this.Window.offsetLeft + this.Window.offsetWidth < this.Window.ArrowsPointingTo.offsetLeft) {
            this.Window.ArrowH.style.left = this.Window.offsetLeft + this.Window.offsetWidth;
            this.Window.ArrowH.style.width = this.Window.ArrowsPointingTo.offsetLeft - (this.Window.offsetLeft + this.Window.offsetWidth);

            this.Window.ArrowA.src = 'images/reportGenerator/right.gif';
            this.Window.ArrowA.style.left = this.Window.ArrowH.offsetLeft + this.Window.ArrowH.offsetWidth - 7;
            this.Window.ArrowA.style.top = this.Window.ArrowH.offsetTop - 3;
        }
        else {
            this.Window.ArrowH.style.display = 'none';
            this.Window.ArrowA.style.display = 'none';
        }

    }

        //V - pointer only
    else if (this.inRect(this.Window.offsetLeft, this.Window.offsetLeft + this.Window.offsetWidth, this.Window.ArrowsPointingTo.offsetLeft, this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth)) {
        this.Window.ArrowH.style.display = 'none';



        this.Window.ArrowV.style.left = this.getAverrageFromPoints(this.Window.offsetLeft, this.Window.offsetLeft + this.Window.offsetWidth, this.Window.ArrowsPointingTo.offsetLeft, this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth);

        if (this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight < this.Window.offsetTop) {
            this.Window.ArrowV.style.top = this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight;
            this.Window.ArrowV.style.height = this.Window.offsetTop - (this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight);

            this.Window.ArrowA.src = 'images/reportGenerator/up.gif';
            this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft - 3;
            this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop;
        }
        else if (this.Window.offsetTop + this.Window.offsetHeight < this.Window.ArrowsPointingTo.offsetTop) {
            this.Window.ArrowV.style.top = this.Window.offsetTop + this.Window.offsetHeight;
            this.Window.ArrowV.style.height = this.Window.ArrowsPointingTo.offsetTop - (this.Window.offsetTop + this.Window.offsetHeight);

            this.Window.ArrowA.src = 'images/reportGenerator/down.gif';
            this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft + this.Window.ArrowV.offsetWidth - 4;
            this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop + this.Window.ArrowV.offsetHeight - 7;
        }
        else {
            this.Window.ArrowV.style.display = 'none';
            this.Window.ArrowA.style.display = 'none';
        }

    }

        // H & V pointers
    else {
        this.Window.ArrowH.style.display = '';
        this.Window.ArrowV.style.display = '';

        //Right
        if (this.Window.offsetLeft > this.Window.ArrowsPointingTo.offsetLeft + this.Window.ArrowsPointingTo.offsetWidth) {
            //Horizontal setup
            this.Window.ArrowH.style.top = this.Window.offsetTop + (this.Window.offsetHeight / 2);
            this.Window.ArrowH.style.left = this.Window.ArrowsPointingTo.offsetLeft + (this.Window.ArrowsPointingTo.offsetWidth / 2);
            this.Window.ArrowH.style.width = this.Window.offsetLeft - parseInt(this.Window.ArrowH.style.left);

            //Bottom
            if (this.Window.offsetTop > this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight) {
                this.Window.ArrowV.style.left = this.Window.ArrowsPointingTo.offsetLeft + (this.Window.ArrowsPointingTo.offsetWidth / 2);
                this.Window.ArrowV.style.top = this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight;
                this.Window.ArrowV.style.height = this.Window.ArrowH.offsetTop - this.Window.ArrowV.offsetTop;

                this.Window.ArrowA.src = 'images/reportGenerator/up.gif';
                this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft + this.Window.ArrowV.offsetWidth - 4;
                this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop;
            }
                //Top
            else if (this.Window.ArrowsPointingTo.offsetTop > this.Window.offsetTop + this.Window.offsetHeight) {
                this.Window.ArrowV.style.left = this.Window.ArrowsPointingTo.offsetLeft + (this.Window.ArrowsPointingTo.offsetWidth / 2);
                this.Window.ArrowV.style.top = this.Window.ArrowH.offsetTop;
                this.Window.ArrowV.style.height = this.Window.ArrowsPointingTo.offsetTop - this.Window.ArrowH.offsetTop;

                this.Window.ArrowA.src = 'images/reportGenerator/down.gif';
                this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft + this.Window.ArrowV.offsetWidth - 4;
                this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop + this.Window.ArrowV.offsetHeight - 7;
            }

        }
            //left
        else if (this.Window.ArrowsPointingTo.offsetLeft > this.Window.offsetLeft + this.Window.offsetWidth) {
            //Horizontal setup
            this.Window.ArrowH.style.top = this.Window.offsetTop + (this.Window.offsetHeight / 2);
            this.Window.ArrowH.style.left = this.Window.offsetLeft + this.Window.offsetWidth;
            this.Window.ArrowH.style.width = this.Window.ArrowsPointingTo.offsetLeft - (this.Window.offsetLeft + this.Window.offsetWidth) + (this.Window.ArrowsPointingTo.offsetWidth / 2);

            //Bottom
            if (this.Window.offsetTop > this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight) {
                this.Window.ArrowV.style.left = this.Window.ArrowsPointingTo.offsetLeft + (this.Window.ArrowsPointingTo.offsetWidth / 2);
                this.Window.ArrowV.style.top = this.Window.ArrowsPointingTo.offsetTop + this.Window.ArrowsPointingTo.offsetHeight;
                this.Window.ArrowV.style.height = this.Window.ArrowH.offsetTop - this.Window.ArrowV.offsetTop;

                this.Window.ArrowA.src = 'images/reportGenerator/up.gif';
                this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft + this.Window.ArrowV.offsetWidth - 4;
                this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop;
            }
                //Top
            else if (this.Window.ArrowsPointingTo.offsetTop > this.Window.offsetTop + this.Window.offsetHeight) {
                this.Window.ArrowV.style.left = this.Window.ArrowsPointingTo.offsetLeft + (this.Window.ArrowsPointingTo.offsetWidth / 2);
                this.Window.ArrowV.style.top = this.Window.ArrowH.offsetTop;
                this.Window.ArrowV.style.height = this.Window.ArrowsPointingTo.offsetTop - this.Window.ArrowH.offsetTop;

                this.Window.ArrowA.src = 'images/reportGenerator/down.gif';
                this.Window.ArrowA.style.left = this.Window.ArrowV.offsetLeft + this.Window.ArrowV.offsetWidth - 4;
                this.Window.ArrowA.style.top = this.Window.ArrowV.offsetTop + this.Window.ArrowV.offsetHeight - 7;
            }
        }
    }
};
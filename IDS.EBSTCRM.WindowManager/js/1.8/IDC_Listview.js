var IDC_Listview = function (documentObject, parentObject, width, height, idcCore, idcWindowManager, idcWindow) {
    this.AutomaticDeselectItemOnClick = false;
    this.WarnWhenResultsExceeds = 0;
    this.ExcessiveDataWarningText = 'The listview is about to populate a large result (%lines% items), which can take a long time to complete.<br><br>Are you sure you want to continue?'
    this.WindowManager = idcWindowManager;
    this.Window = idcWindow;
    this.AllowTextSelection = false;
    this.isIE = document.all ? true : false;
    this.Core = idcCore || new IDC_Core(documentObject, parentObject);

//    var me = this;
//    if (this.Core.Keyboard != null) {
//        this.Core.Keyboard.PrimaryKeyDownFunction = function (keynum, target, e) {
//            if (me.Core.Keyboard.KeyMap[keynum].length == 1) {
//                me.SelectItemFromKeyboard(me.Core.Keyboard.KeyMap[keynum]);
//            }
//        };
//    }

    this.LastUrl;
    this.LastArguments;

    this.PopulationTimeout = 950;
    this.PopulationSleep = 1;

    this.ContextMenu = null;

    this.KeyboardCTRLDown = false;
    this.KeyboardSHIFTDown = false;

    var me = this;

    this.Document = documentObject || document;
    this.ParentObject = parentObject || document.body;

    //keyboard firefox & chrome fix
    this.HiddenInput = this.Document.createElement('INPUT');
    this.HiddenInput.style.position = 'absolute';
    this.HiddenInput.style.top = -5000;
    this.Document.body.appendChild(this.HiddenInput);
    this.HiddenInput.focus();

    this.HighlightColor = 'black';
    this.HighlightBackgroundColor = 'yellow';
    this.HighlightSearchItems = false;
    this.HighlightSearchQuery = '';

    this.AutoRefreshFromAjax = false;
    this.AutoRefreshInterval = 30;
    this.AutoRefreshPopulateInterval = 5000;
    this.AutoRefreshPopulateCount = 10;
    this.AutoRefreshAjaxObject = null;
    this.AutoRefreshCache = this.Document.createElement('DIV');
    this.AutoRefreshAbort = false;
    this.AutoRefreshItems = new Array();

    this.CurrentURL = this.Document.location.toString().substring(0, this.Document.location.toString().lastIndexOf('/')) + '/';

    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr1 = this.Document.createElement('TR');
    var tr2 = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var td2 = this.Document.createElement('TD');

    var tr3 = this.Document.createElement('TR');
    var td3 = this.Document.createElement('TD');

    this.OwnerColumnHeaderContainer = this.Document.createElement('DIV');
    this.OwnerColumnHeaderContainer.className = 'columnHeaderBack';

    this.OwnerColumnHeaderGroupContainer = this.Document.createElement('DIV');
    this.OwnerColumnHeaderGroupContainer.className = 'columnHeaderBack';

    this.Container = this.Document.createElement('DIV');
    this.Container.className = 'listviewBack';

    td2.vAlign = 'top';

    this.Control.cellPadding = 0;
    this.Control.cellSpacing = 0;
    this.Control.border = 0;
    this.Control.style.width = (width || 300) + (this.isIE ? '' : 'px');
    this.Control.style.height = (height || 100) + (this.isIE ? '' : 'px');

    this.Control.appendChild(tb);
    tb.appendChild(tr3);
    tb.appendChild(tr1);
    tb.appendChild(tr2);
    tr1.appendChild(td1);
    tr2.appendChild(td2);
    tr3.appendChild(td3);
    td1.appendChild(this.OwnerColumnHeaderContainer);
    td3.appendChild(this.OwnerColumnHeaderGroupContainer);
    td2.appendChild(this.Container);

    this.ParentObject.appendChild(this.Control);

    this.Container.style.width = this.Control.style.width;

    this.Container.onclick = function () { if (!this.Listview.__NoDeselect) { this.Listview.ClearSelected(); }; };

    this.Container.style.height = ((height || 100) - this.OwnerColumnHeaderContainer.offsetHeight) + (this.isIE ? '' : 'px');

    this.HeaderScroller = this.Document.createElement('DIV');
    this.HeaderGroupScroller = this.Document.createElement('DIV');

    this.__NoDeselect = false;

    this.ShowLoading = false;
    this.EnsureNoScriptTimeout = true;

    var pn = this.OwnerColumnHeaderContainer.parentNode;
    var png = this.OwnerColumnHeaderGroupContainer.parentNode;

    var cn = pn.removeChild(this.OwnerColumnHeaderContainer);
    var cng = png.removeChild(this.OwnerColumnHeaderGroupContainer);

    this.HeaderScroller.appendChild(cn);
    pn.appendChild(this.HeaderScroller);
    this.ColumnHeaderContainer = cn;

    this.HeaderGroupScroller.appendChild(cng);
    png.appendChild(this.HeaderGroupScroller);
    this.ColumnHeaderGroupContainer = cng;

    this.Statusbar = null;
    this.EnableColumnFilters = false;
    this.ResizeableColumnHeaders = true;

    this.OnDoubleClickAsString = '';

    this.Container.Listview = this;
    this.ColumnHeaderContainer.style.position = 'relative';
    this.ColumnHeaderContainer.parentNode.style.overflow = 'hidden';
    this.ColumnHeaderContainer.parentNode.noWrap = true;

    this.ColumnHeaderContainer.style.left = 0;
    this.ColumnHeaderContainer.style.top = 0;
    this.ColumnHeaderContainer.parentNode.parentNode.className = 'columnHeaderOverflowedBack';

    this.ColumnHeaderGroupContainer.style.position = 'relative';
    this.ColumnHeaderGroupContainer.parentNode.style.overflow = 'hidden';
    this.ColumnHeaderGroupContainer.parentNode.noWrap = true;

    this.ColumnHeaderGroupContainer.style.left = 0;
    this.ColumnHeaderGroupContainer.style.top = 0;
    this.ColumnHeaderGroupContainer.parentNode.parentNode.className = 'columnHeaderOverflowedBack';

    this.viewStyle = 0;
    this.lastViewstyle = 0;

    this.HideColumnGroupHeaders();

    //Add default viewstyles
    this.ViewStyles = new Array();
    this.ViewStyles.push(IDC_ListviewItemDetailed);
    this.ViewStyles.push(IDC_ListviewItemSmallIcon);
    this.ViewStyles.push(IDC_ListviewItemMediumIcon);
    this.ViewStyles.push(IDC_ListviewItemLargeIcon);
    this.ViewStyles.push(IDC_ListviewItemGalleryIcon);

    this.Container.onscroll = function () {
        //if(this.Listview.viewStyle == 3)
        //{
        this.Listview.ColumnHeaderContainer.style.left = -this.scrollLeft;
        this.Listview.ColumnHeaderContainer.parentNode.style.width = this.offsetWidth;

        this.Listview.ColumnHeaderGroupContainer.style.left = this.Listview.ColumnHeaderContainer.style.left;
        this.Listview.ColumnHeaderGroupContainer.parentNode.style.width = this.Listview.ColumnHeaderContainer.parentNode.style.width;

        if (this.Listview.EnableColumnFilters) {
            for (var i = 0; i < this.Listview.ColumnHeaders.length; i++) {
                this.Listview.ColumnHeaders[i].HideFilters();
            }
        }
        //}
    };


    this.Container.oncontextmenu = function () {
        if (this.Listview.ContextMenu) {
            if (this.Listview.SelectedItems.length == 0 && this.Listview.HoveringItem)
                this.Listview.HoveringItem.Select();

            this.Listview.ContextMenu.Show(this.Listview.Core.Mouse.X, this.Listview.Core.Mouse.Y);
            return false;
        }
    };
    this.SelectedItems = new Array();
    this.Items = new Array();
    this.ColumnHeaders = new Array();
    this.ColumnGroupHeaders = new Array();

    this.HoveringItem = null;

    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position = 'absolute';
    this.__TransparentOverlay.style.zIndex = 250000;
    this.__TransparentOverlay.style.width = '100%';
    this.__TransparentOverlay.style.height = '100%';
    this.__TransparentOverlay.style.left = 0;
    this.__TransparentOverlay.style.top = 0;
    this.__TransparentOverlay.style.display = 'none';
    this.__TransparentOverlay.oncontextmenu = function (e) { return false; };
    this.__TransparentOverlay.style.cursor = 'e-resize';

    this.__TransparentOverlay.onclick = function () { };
    this.__TransparentOverlay.onmouseup = function () { this.style.display = 'none'; };

    this.__TransparentOverlay.onmousemove = null;
    this.__TransparentOverlay.Listview = this;

    if (this.isIE) {
        this.__TransparentOverlay.style.backgroundImage = 'url(' + this.CurrentURL + 'images/spacer.gif)';
    }

    this.Document.body.appendChild(this.__TransparentOverlay);

    this.__InlineBlock = (this.isIE ? 'inline' : 'inline-block');
    this.__UseDimPX = (this.isIE ? '' : 'px');

    this.DragDrop_ItemDrop = null; //dragDrop_ItemDrop || null;
    this.DropType = null; // dropType;
    this.Dropable = null; // dropable;

    this.MultiSelect = true;

    this.OnDoubleClick = null;
    this.OnSelect = null;
    this.OnLoad = null;
    this.OnLoadFailed = null;
    this.OnParseComplete = null;
    this.OnParseBegin = null;
    this.OnParsing = null;
    this.Keyboard = null;

    this.Sortable = true;
    this.SortingColumn = null;
    this.SortDirectionAsc = true;

    this.AjaxObject = null;

    //Drag drop
    //    if(dd)
    //    {
    //        if(this.Dropable) dd.AddDropZone(this.Container, this.DropType, this.DragDrop_ItemDrop);
    //    }
    this.ColumnHeaderContainer.parentNode.style.width = this.Container.offsetWidth;
    this.ColumnHeaderGroupContainer.parentNode.style.width = this.ColumnHeaderContainer.parentNode.style.width;
};

IDC_Listview.prototype.Refresh = function ()
{
    this.Container.onscroll();
};

IDC_Listview.prototype.SelectItemFromKeyboard = function (keys) {
    //Single select
    var columnIndex = 0;
    for (var i = 0; i < this.ColumnHeaders.length; i++) {
        if (this.ColumnHeaders[i].IsSortingColumn == true) {
            columnIndex = i;
            break;
        }
    }
    
    for (var i = 0; i < this.Items.length; i++) {
        var n = this.Items[i];

        keys = keys.toLowerCase();

        if (n.Items[columnIndex].toLowerCase().indexOf(keys) == 0) {
            this.Items[i].Select();

            var x = this.Items[i].Control.offsetLeft;
            var y = this.Items[i].Control.offsetTop;

            this.Container.scrollLeft = x;
            this.Container.scrollTop = y;

            break;
        }
    }
};

IDC_Listview.prototype.CopySelectedItemsToClipboard = function (ColumnIndex)
{
    var index = isNaN(ColumnIndex) ? 0 : parseInd(ColumnIndex);

    var cpData = '';

    for (var i = 0; i < this.SelectedItems.length; i++)
    {
        var dt = this.SelectedItems[i].Items[index];
        if (dt == null) dt = '';
        cpData += (cpData == '' ? dt : '\r\n' + dt);
    }

    //Add to clipboard
    if (parent.window.clipboardData)
        parent.window.clipboardData.setData('text', cpData);

    return cpData;
};

IDC_Listview.prototype.SetAutogrow = function (value)
{
    this.Container.className = value ? 'listviewBackGrow' : 'listviewBack';
};


IDC_Listview.prototype.HideColumnGroupHeaders = function ()
{
    this.ColumnHeaderGroupContainer.style.display = 'none';
};

IDC_Listview.prototype.ShowColumnGroupHeaders = function ()
{
    this.ColumnHeaderGroupContainer.style.display = '';
};

IDC_Listview.prototype.HideColumnHeaders = function ()
{
    this.ColumnHeaderContainer.style.display = 'none';
};

IDC_Listview.prototype.ShowColumnHeaders = function ()
{
    this.ColumnHeaderContainer.style.display = '';
};

IDC_Listview.prototype.GetItemById = function (Id)
{
    for (var i = 0; i < this.Items.length; i++)
    {
        if (this.Items[i].Uniqueidentifier == Id)
            return this.Items[i];
    }
    return null;
};

IDC_Listview.prototype.ClearSelected = function ()
{
    //Single select
    for (var i = 0; i < this.SelectedItems.length; i++)
    {
        var n = this.SelectedItems[i];
        n.IsSelected = false;
        n.EventHandler.SelectionChanged(n);
    }
    this.SelectedItems.length = 0;

    if (this.OnSelect)
        this.OnSelect(null);
};

IDC_Listview.prototype.ChangeView = function (newView)
{
    if (this.viewStyle == newView) return;

    this.lastViewstyle = this.viewStyle;
    this.viewStyle = newView;

    for (var i = 0; i < this.Items.length; i++)
    {
        this.Items[i].Create(this);
    }
};

IDC_Listview.prototype.SetSize = function (width, height)
{
    if (parseInt(width) < 0) parseInt(width) = 0;
    if (parseInt(height) < this.ColumnHeaderContainer.offsetHeight + 1) height = this.ColumnHeaderContainer.offsetHeight + 1;


    var h = parseInt(height) - this.ColumnHeaderContainer.offsetHeight - this.ColumnHeaderGroupContainer.offsetHeight;
    if (h < 1) h = 1;
    this.Container.style.width = parseInt(width);
    this.Container.style.height = h;


    this.ColumnHeaderContainer.style.left = -this.Container.scrollLeft;
    this.ColumnHeaderContainer.parentNode.style.width = this.Container.offsetWidth;

    this.ColumnHeaderGroupContainer.style.left = this.ColumnHeaderContainer.style.left;
    this.ColumnHeaderGroupContainer.parentNode.style.width = this.ColumnHeaderContainer.parentNode.style.width;

};

IDC_Listview.prototype.AddItem = function (Uniqueidentifier, Value, Icons, SubItems)
{
    var values = new Array();
    values.push(Value);

    if (SubItems)
    {
        values = values.concat(SubItems);
    }

    var l = new IDC_ListviewItem(this.Document, this.Container, values);
    l.Uniqueidentifier = Uniqueidentifier;

    if (Icons)
    {
        l.Icon16 = Icons.length > 0 ? Icons[0] : null;
        l.Icon32 = Icons.length > 1 ? Icons[1] : null;
        l.Icon48 = Icons.length > 2 ? Icons[2] : null;
        l.Icon64 = Icons.length > 3 ? Icons[3] : null;
        l.Icon96 = Icons.length > 4 ? Icons[4] : null;
        l.Icon128 = Icons.length > 5 ? Icons[5] : null;
    }

    var i = this.Items.push(l);
    l.Create(this);

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';

    return l;
};

IDC_Listview.prototype.AddItemAt = function (Uniqueidentifier, Value, Icons, SubItems, Index)
{
    var values = new Array();
    values.push(Value);

    if (SubItems)
    {
        values = values.concat(SubItems);
    }

    var l = new IDC_ListviewItem(this.Document, this.Container, values);
    l.Uniqueidentifier = Uniqueidentifier;

    if (Icons)
    {
        l.Icon16 = Icons.length > 0 ? Icons[0] : null;
        l.Icon32 = Icons.length > 1 ? Icons[1] : null;
        l.Icon48 = Icons.length > 2 ? Icons[2] : null;
        l.Icon64 = Icons.length > 3 ? Icons[3] : null;
        l.Icon96 = Icons.length > 4 ? Icons[4] : null;
        l.Icon128 = Icons.length > 5 ? Icons[5] : null;
    }

    if (Index < 0) Index = 0;
    if (Index > this.Items.length - 1) Index = this.Items.length - 1;

    var i = this.Items.splice(Index, 0, l);

    l.Create(this, Index);

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';

    return l;
};

IDC_Listview.prototype.AddItemObject = function (lwItem)
{
    var i = this.Items.push(lwItem);
    lwItem.Create(this);

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';
};

IDC_Listview.prototype.AddItemObjectAt = function (lwItem, Index)
{
    if (this.Items.length == 0)
    {
        this.AddItemObject(lwItem);
        return;
    }
    if (Index < 0) Index = 0;
    if (Index > this.Items.length - 1) Index = this.Items.length - 1;

    var i = this.Items.splice(Index, 0, lwItem);
    lwItem.Create(this, Index);

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';
};

IDC_Listview.prototype.AddItemsFromHTMLObject = function (sender, what)
{
    while (what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);



        if (src.tagName)
        {
            var v = new Array();

            while (src.hasChildNodes())
            {
                var ss = src.firstChild;
                ss = src.removeChild(ss);
                if (ss.tagName)
                    v.push(this.isIE ? ss.innerText : ss.textContent);
            }

            var l = new IDC_ListviewItem(sender.Document, sender.Container, v);

            l.Icon16 = src.getAttribute('icon16');
            l.Icon32 = src.getAttribute('icon32');
            l.Icon48 = src.getAttribute('icon48');
            l.Icon64 = src.getAttribute('icon64');
            l.Icon96 = src.getAttribute('icon96');
            l.Icon128 = src.getAttribute('icon128');

            //Drag drop 
            l.Dragable = src.getAttribute('drag') == '1';
            l.DragType = src.getAttribute('dragtype');
            l.Dropable = src.getAttribute('drop') == '1';
            l.DropType = src.getAttribute('droptype');
            l.Uniqueidentifier = src.getAttribute('uid');
            l.Uniqueidentifier2 = src.getAttribute('uid2');

            sender.AddItemObject(l);
        }
    }
};

IDC_Listview.prototype.AddItemsFromHTMLObjectAt = function (sender, what, index)
{
    while (what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);



        if (src.tagName)
        {
            var v = new Array();

            while (src.hasChildNodes())
            {
                var ss = src.firstChild;
                ss = src.removeChild(ss);
                if (ss.tagName)
                    v.push(this.isIE ? ss.innerText : ss.textContent);
            }

            var l = new IDC_ListviewItem(sender.Document, sender.Container, v);

            l.Icon16 = src.getAttribute('icon16');
            l.Icon32 = src.getAttribute('icon32');
            l.Icon48 = src.getAttribute('icon48');
            l.Icon64 = src.getAttribute('icon64');
            l.Icon96 = src.getAttribute('icon96');
            l.Icon128 = src.getAttribute('icon128');

            //Drag drop 
            l.Dragable = src.getAttribute('drag') == '1';
            l.DragType = src.getAttribute('dragtype');
            l.Dropable = src.getAttribute('drop') == '1';
            l.DropType = src.getAttribute('droptype');
            l.Uniqueidentifier = src.getAttribute('uid');
            l.Uniqueidentifier2 = src.getAttribute('uid2');

            sender.AddItemObjectAt(l, index);
            index++;
        }
    }
};

IDC_Listview.prototype.PrintItems = function (windowManager, forcePrint)
{
    var html = this.ItemsToPrintableHTML();

    if (!windowManager) windowManager = this.WindowManager;
    if (forcePrint) windowManager = null;

    if (windowManager)
    {
        this.WindowManagerPrintPreview(windowManager, html, 'PrintItems(null, true)');
    }
    else
    {
        var w = window.open('', '');

        w.document.write('<html><head><style type="text/css" rel="stylesheet">\ntd { font-family:arial;font-size:12px;margin:0px;}\n</style></head><body onload="window.print();window.close();">' + html + '</body></html>');
        w.document.close();
    }
};

IDC_Listview.prototype.PrintSelectedItems = function (windowManager, forcePrint)
{
    var html = this.SelectedItemsToPrintableHTML();

    if (!windowManager) windowManager = this.WindowManager;
    if (forcePrint) windowManager = null;

    if (windowManager)
    {
        this.WindowManagerPrintPreview(windowManager, html, 'PrintSelectedItems(null, true)');
    }
    else
    {
        var w = window.open('', '');
        w.document.write('<html><head><style type="text/css" rel="stylesheet">\ntd { font-family:arial;font-size:12px;margin:0px;}\n</style></head><body onload="window.print();window.close();">' + html + '</body></html>');
        w.document.close();
    }
};

IDC_Listview.prototype.FormPostItems = function (url)
{
    this.FormPostActionItems(url, this.FilteredItemsToFormPostData());
};
IDC_Listview.prototype.FormPostSelectedItems = function (url)
{
    this.FormPostActionItems(url, this.FilteredItemsToFormPostData());
};

IDC_Listview.prototype.FormPostFilteredItems = function (url)
{
    this.FormPostActionItems(url, this.FilteredItemsToFormPostData());
};

IDC_Listview.prototype.FormPostActionItems = function (url, postdata)
{
    var frm = document.createElement('FORM');
    frm.target = '_blank';
    frm.method = 'POST';
    frm.action = url;
    frm.style.display = 'none';
    frm.style.position = 'absolute';
    document.body.appendChild(frm);

    var data = document.createElement('input');
    data.type = 'hidden';
    data.name = 'data';
    data.value = postdata;

    frm.appendChild(data);
    frm.submit();
    document.body.removeChild(frm);
    delete fmr;
};

IDC_Listview.prototype.FilteredItemsToFormPostData = function (elementSep, lineSep, itemQoute)
{
    var html = '';
    var ls = lineSep || '\r\n';
    var es = elementSep || ';';
    var qt = itemQoute || '"';

    if (this.ColumnGroupHeaders.length > 0)
    {
        for (var i = 0; i < this.ColumnGroupHeaders.length; i++)
        {
            html += this.ColumnGroupHeaders[i].ToFormPostData(es, qt);
        }
        html += ls;
    }

    var colmax = this.ColumnHeaders.length - 1;
    for (var i = 0; i < this.ColumnHeaders.length; i++)
    {
        html += this.ColumnHeaders[i].ToFormPostData(es, qt) + (i < colmax ? es : ls);
    }
    for (var i = 0; i < this.Items.length; i++)
    {
        if (this.Items[i].Control.style.display != 'none')
            html += this.Items[i].ToFormPostData(es, ls, qt);
    }
    return html;
};

IDC_Listview.prototype.PrintFilteredItems = function (windowManager, forcePrint)
{
    var html = this.FilteredItemsToPrintableHTML();

    if (!windowManager) windowManager = this.WindowManager;
    if (forcePrint) windowManager = null;

    if (windowManager)
    {
        this.WindowManagerPrintPreview(windowManager, html, 'PrintFilteredItems(null, true)');
    }
    else
    {
        var w = window.open('', '');
        w.document.write('<html><head><style type="text/css" rel="stylesheet">\ntd { font-family:arial;font-size:12px;margin:0px;}\n</style></head><body onload="window.resizeTo(250,250);window.moveTo(-1000,-1000);window.print();window.close();">' + html + '</body></html>');
        w.document.close();
    }
};

IDC_Listview.prototype.WindowManagerPrintPreview = function (windowManager, content, lwPrintFunction)
{

    var html = '<html>' +
                '<head>' +
                '<link href="' + windowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<link href=<%="\"styles/" + IDS.EBSTCRM.WindowManager.CSSVersionTracker.CurrentVersion + "styles.css" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'var win=null;\n' +
                'function init()' +
                '{' +
                    'win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.CenterOwner();' +
                    'win.OnResized = OnResize;' +
                    'win.OnMaximize = OnResize;' +
                    'win.OnRestore = OnResize;' +
                    'document.getElementById("cmdOkay").focus();' +
                    'OnResize(win, 0,0, win.GetInnerWidth(),win.GetInnerHeight());' +
                '};' +
                'function OnResize(wind, l, t, w, h)' +
                '{' +
                'divContent.style.width=w;' +
                'divContent.style.height=h-trButtons.offsetHeight;' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0" style="Width:100%;height:100%;">' +
                '<tr><td style="height:100%;" valign="top"><div id="divContent" style="overflow:auto;position:absolute;">' + content + '</div></td></tr>' +
                '<tr id="trButtons"><td align="right" class="resizeableDialogButtons">' +
                '<input id="cmdOkay" class="resizeableDialogCommandButton" type="button" value="Print" onclick="win.Variables.' + lwPrintFunction + ';">' +
                '&nbsp;<input class="resizeableDialogCommandButton" type="button" value="Close" onclick="win.Close();">' +
                '</td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = windowManager.CurrentWindow.CreateWindow('');

    win.Icons = ['images/print.png', 'images/print32.png'];

    win.Name = 'Print';
    win.Resizeable = true;
    win.Minimizeable = true;
    win.Maximizeable = true;
    win.Width = 600;
    win.Height = 400;
    win.MinWidth = 250;

    win.Variables = this;
    win.CenterOwner();
    win.Show();

    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_Listview.prototype.ItemsToPrintableHTML = function ()
{
    var html = '<table border="0" cellpadding="0" cellspacing="0">';

    if (this.ColumnGroupHeaders.length > 0)
    {
        html += '<tr>';
        for (var i = 0; i < this.ColumnGroupHeaders.length; i++)
        {
            html += this.ColumnGroupHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; font-size:14px;');
        }
        html += '</tr>';
    }

    html += '<tr>';
    for (var i = 0; i < this.ColumnHeaders.length; i++)
    {
        html += this.ColumnHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; border-bottom:solid 1px #4f81bd');
    }
    html += '</tr>';

    for (var i = 0; i < this.Items.length; i++)
    {
        html += this.Items[i].ToPrintableHTML(i % 2 == 0 ? '#d3dfee' : '#ffffff', 'padding:2px;color:#365f91;');
    }
    html += '</table>';
    return html;
};

IDC_Listview.prototype.SelectedItemsToPrintableHTML = function ()
{
    var html = '<table border="0" cellpadding="0" cellspacing="0">';

    if (this.ColumnGroupHeaders.length > 0)
    {
        html += '<tr>';
        for (var i = 0; i < this.ColumnGroupHeaders.length; i++)
        {
            html += this.ColumnGroupHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; font-size:14px;');
        }
        html += '</tr>';
    }

    html += '<tr>';
    for (var i = 0; i < this.ColumnHeaders.length; i++)
    {
        html += this.ColumnHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; border-bottom:solid 1px #4f81bd');
    }
    html += '</tr>';

    for (var i = 0; i < this.SelectedItems.length; i++)
    {
        html += this.SelectedItems[i].ToPrintableHTML(i % 2 == 0 ? '#d3dfee' : '#ffffff', 'padding:2px;color:#365f91;');
    }
    html += '</table>';
    return html;
};

IDC_Listview.prototype.FilteredItemsToPrintableHTML = function ()
{
    var html = '<table border="0" cellpadding="0" cellspacing="0">';

    if (this.ColumnGroupHeaders.length > 0)
    {
        html += '<tr>';
        for (var i = 0; i < this.ColumnGroupHeaders.length; i++)
        {
            html += this.ColumnGroupHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; font-size:14px;');
        }
        html += '</tr>';
    }

    html += '<tr>';
    for (var i = 0; i < this.ColumnHeaders.length; i++)
    {
        html += this.ColumnHeaders[i].ToPrintableHTML('padding:2px;background-color:#ffffff;color:#365f91;font-weight:bold;border-top:solid 1px #4f81bd; border-bottom:solid 1px #4f81bd');
    }
    html += '</tr>';

    for (var i = 0; i < this.Items.length; i++)
    {
        if (this.Items[i].Control.style.display != 'none')
            html += this.Items[i].ToPrintableHTML(i % 2 == 0 ? '#d3dfee' : '#ffffff', 'padding:2px;color:#365f91;');
    }
    html += '</table>';
    return html;
};

IDC_Listview.prototype.GetIdsForFilteredItems = function ()
{
    var retval = new Array();

    for (var i = 0; i < this.Items.length; i++)
    {
        if (this.Items[i].Control.style.display != 'none')
            retval.push(this.Items[i].Uniqueidentifier);
    }

    return retval;
};


IDC_Listview.prototype.ItemsToHTML = function ()
{
    var html = '';
    for (var i = 0; i < this.Items.length; i++)
    {
        html += this.Items[i].ToHTML();
    }
    return html;
};

var IDC_ListviewItem = function (OwnerDocument, OwnerContainer, Values)
{
    this.SelectedCSS = '';
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.Items = Values;
    this.CoreItems = new Array();


    this.Icon16 = null;
    this.Icon32 = null;
    this.Icon48 = null;
    this.Icon64 = null;
    this.Icon96 = null;
    this.Icon128 = null;

    this.Control = null;

    this.Dragable = false;
    this.Dropable = false;
    this.DragType = null;
    this.DropType = null;

    this.Uniqueidentifier = null;
    this.Uniqueidentifier2 = null;

    this.Listview = null;
    this.IsSelected = false;

    return this;
};

IDC_ListviewItem.prototype.ToPrintableHTML = function (bgColor, style)
{
    var html = '<tr>';

    for (var i = 0; i < this.Items.length && i < this.Listview.ColumnHeaders.length; i++)
    {
        html += '<td align="' + this.Listview.ColumnHeaders[i].TextAlign + '" style="background-color:' + (bgColor || '') + ';' + (style || '') + '">' + (this.Items[i] || '&nbsp;') + '</td>';
    }

    html += '</tr>';

    return html;
};

IDC_ListviewItem.prototype.ToFormPostData = function (es, ls, qt)
{
    var html = '';
    var max = this.Listview.ColumnHeaders.length - 1;

    var r = new RegExp(qt, 'g');

    for (var i = 0; i < this.Items.length && i < this.Listview.ColumnHeaders.length; i++)
    {
        html += qt + (this.Items[i].replace(r, qt + qt) || '') + qt + (i < max ? es : ls);
    }
    return html;
};

IDC_ListviewItem.prototype.ToHTML = function ()
{
    var div = document.createElement('DIV');

    var u = document.createElement('u');

    div.appendChild(u);

    for (var i = 0; i < this.Items.length; i++)
    {
        var b = document.createElement('b');
        if (this.Listview.isIE)
            b.innerText = this.Items[i];
        else
            b.textContent = this.Items[i];

        u.appendChild(b);
    }

    if (this.Icon16) u.setAttribute('icon16', this.Icon16);
    if (this.Icon32) u.setAttribute('icon32', this.Icon32);
    if (this.Icon48) u.setAttribute('icon48', this.icon48);
    if (this.Icon64) u.setAttribute('icon64', this.icon64);
    if (this.Icon96) u.setAttribute('icon96', this.icon96);
    if (this.Icon128) u.setAttribute('ico128', this.ico128);

    //Drag drop 
    if (this.Dragable) u.setAttribute('drag', this.drag ? '1' : '');
    if (this.DragType) u.setAttribute('dragtype', this.dragtype);
    if (this.Dropable) u.setAttribute('drop', this.drop ? '1' : '');
    if (this.DropType) u.setAttribute('droptype', this.droptype);
    if (this.Uniqueidentifier) u.setAttribute('uid', this.uid);
    if (this.Uniqueidentifier2) u.setAttribute('uid2', this.uid2);

    var html = div.innerHTML;
    div.innerHTML = null;
    div = null;
    return html;
};


IDC_ListviewItem.prototype.SetColumnName = function (name, index)
{
    this.Items[index] = name;
    this.BuildCoreData();
    this.EventHandler.OnTextChanged(this);
};

IDC_ListviewItem.prototype.SetCurrentIcon = function (icon)
{
    //OBSOLETE - Wont work correctly any more
    this.Control.childNodes[0].style.backgroundImage='url(' + this.CurrentURL + icon + ')';
    this.EventHandler.OnIconChanged(this);
};

IDC_ListviewItem.prototype.SetIcons = function (Icons)
{
    if (Icons)
    {
        this.Icon16 = Icons.length > 0 ? Icons[0] : null;
        this.Icon32 = Icons.length > 1 ? Icons[1] : null;
        this.Icon48 = Icons.length > 2 ? Icons[2] : null;
        this.Icon64 = Icons.length > 3 ? Icons[3] : null;
        this.Icon96 = Icons.length > 4 ? Icons[4] : null;
        this.Icon128 = Icons.length > 5 ? Icons[5] : null;
    }

    this.EventHandler.OnIconChanged(this);
};

IDC_ListviewItem.prototype.SetName = function (name, index)
{
    this.SetColumnName(name, index || 0);

};

IDC_ListviewItem.prototype.SetNames = function (names)
{
    this.Items = names;
    this.BuildCoreData();
    this.EventHandler.OnTextChanged(this);
};

IDC_ListviewItem.prototype.getFirstSelectedIndex = function ()
{
    for (var i = 0; i < this.Listview.Items.length; i++)
    {
        if (this.Listview.Items[i].IsSelected) return i;
    }
    return 0;
};

IDC_ListviewItem.prototype.getLastSelectedIndex = function ()
{
    for (var i = this.Listview.Items.length - 1; i >= 0; i--)
    {
        if (this.Listview.Items[i].IsSelected) return i;
    }
    return this.Listview.Items.length - 1;
};

IDC_Listview.prototype.getItemByUniqueidentifier = function (uid)
{
    for (var i = this.Items.length - 1; i >= 0; i--)
    {
        if (this.Items[i].Uniqueidentifier == uid) return this.Items[i];
    }
    return null;
};

IDC_ListviewItem.prototype.getMyIndex = function ()
{
    for (var i = this.Listview.Items.length - 1; i >= 0; i--)
    {
        if (this.Listview.Items[i] == this) return i;
    }
    return null;
};

IDC_ListviewItem.prototype.Select = function ()
{
    //Single select
    var ctrl = (this.Listview.Core.Keyboard ? this.Listview.Core.Keyboard.KeyControlPressed == true : false);
    var shift = (this.Listview.Core.Keyboard ? this.Listview.Core.Keyboard.KeyShiftPressed == true : false);

    if (ctrl && this.Listview.MultiSelect)
    {
        //multiselect select
        var isInArray = false;
        var InArrayIndex = 0;
        for (var i = 0; i < this.Listview.SelectedItems.length; i++)
        {
            var n = this.Listview.SelectedItems[i];
            if (n == this)
            {
                InArrayIndex = i;
                isInArray = true;
            }
        }

        //Remove if in array:
        if (isInArray)
        {
            //this.Control.className=this.SelectedCSS;
            this.Listview.SelectedItems.splice(InArrayIndex, 1);
            this.IsSelected = false;
            this.EventHandler.SelectionChanged(this);
        }
        else
        {
            this.IsSelected = true;
            //this.Control.className=this.SelectedCSS + '_s';
            this.Listview.SelectedItems.push(this);
            this.EventHandler.SelectionChanged(this);
        }

    }
    else if (shift && this.Listview.MultiSelect && this.Listview.SelectedItems.length > 0)
    {
        var a = this.getFirstSelectedIndex();
        var b = this.getMyIndex();

        this.Listview.ClearSelected();

        var shift = a < b ? 1 : -1;
        for (var i = a; i != b; i += shift)
        {
            this.Listview.Items[i].IsSelected = true;
            this.Listview.SelectedItems.push(this.Listview.Items[i]);
            this.EventHandler.SelectionChanged(this.Listview.Items[i]);

            if (this.Listview.OnSelect)
                this.Listview.OnSelect(this.Listview.Items[i]);
        }
        this.Listview.SelectedItems.push(this);
        this.IsSelected = true;
        this.EventHandler.SelectionChanged(this);

    }
    else
    {

        var alreadySelected = false;

        for (var i = 0; i < this.Listview.SelectedItems.length; i++)
        {
            var n = this.Listview.SelectedItems[i];

            if (n == this && this.Listview.AutomaticDeselectItemOnClick)
            {
                alreadySelected = true;
            }

            n.IsSelected = false;
            n.EventHandler.SelectionChanged(n);
        }

        this.Listview.SelectedItems.length = 0;
        if (!alreadySelected)
        {
            //** Her markere den en linje
            this.Listview.SelectedItems.push(this);
            this.IsSelected = true;
        }
        this.EventHandler.SelectionChanged(this);

    }

    if (this.Listview.OnSelect)
        this.Listview.OnSelect(this);

};


IDC_ListviewItem.prototype.DisposeEvents = function ()
{
    this.Control.onclick = null;
    this.Control.onmouseover = null;
    this.EventHandler.OnMouseOver = null;
    this.Control.onmouseout = null;
    this.EventHandler.OnMouseOut = null;
    var me = this;
};

IDC_ListviewItem.prototype.BuildCoreData = function ()
{
    this.CoreItems.length = 0;

    for (var i = 0; i < this.Items.length && i < this.Listview.ColumnHeaders.length; i++)
    {
        var col = this.Listview.ColumnHeaders[i];
        if (col.DataType == 'numeric')
        {
            var va = this.Items[i].toString();
            var mask = (col.Mask || '.');

            var r = new RegExp('[^0-9' + mask + ']*', 'g');

            va = va.replace(r, '');
            this.CoreItems.push(parseFloat(va.replace(mask, '.')));
        }
        else if (col.DataType == 'date')
        {
            var va = this.Items[i].toString();
            var aa = Date.parse(va, col.Mask || 'dd-MM-yyyy');
            if (isNaN(aa)) aa = 0;
            this.CoreItems.push(aa);
        }
        else
        {
            this.CoreItems.push(this.Items[i].toString().toLowerCase());
        }
    }
};

IDC_ListviewItem.prototype.Create = function (listview, index) {


    this.Listview = listview;
    var me = this;

    if (this.Control) {
        this.DisposeEvents();
        this.Control.parentNode.removeChild(this.Control);
    }
    else {
        //reformat data, if needed.
        //add converted items to core
        this.BuildCoreData();

        if (listview.EnableColumnFilters) {
            for (var i = 0; i < listview.ColumnHeaders.length && i < this.Items.length; i++) {
                listview.ColumnHeaders[i].AddUniqueValue(this.Items[i]);
            }
        }
    }

    //Apply scripting objects
//    for (var i = 0; i < this.Items.length; i++) {
//        
//    }


    if (listview.HighlightSearchItems && listview.HighlightSearchQuery.length > 0) {
        var store = new Array();

        for (var i = 0; i < this.Items.length; i++) {
            store.push(this.Items[i].toString());
            for (var q = 0; q < listview.HighlightSearchQuery.length; q++) {
                if (listview.HighlightSearchQuery[q] != '' && listview.HighlightSearchQuery[q] != ' ') {

                    this.Items[i] = listview.Core.DOM.HighlightText(this.Items[i], listview.HighlightSearchQuery[q]);
                }
            }
        }
        this.EventHandler = new listview.ViewStyles[listview.viewStyle](listview, me);
        for (var i = 0; i < store.length; i++) {
            this.Items[i] = store[i].toString();
        }
        store.length = 0;
    }
    else
        this.EventHandler = new listview.ViewStyles[listview.viewStyle](listview, me);

    this.Control = this.EventHandler.Control;
    this.Control.ListviewItem = this;
    if (isNaN(index)) {
        this.Container.appendChild(this.Control);
    }
    else {
        this.Container.insertBefore(this.Control, this.Container.childNodes[index]);
    }

    this.Control.onclick = function () { me.Listview.__NoDeselect = true; me.Select(); setTimeout(function () { me.Listview.__NoDeselect = false; }, 50); };
    this.Control.onmouseover = function () { me.Listview.HoveringItem = this.ListviewItem; if (me.EventHandler.OnMouseOver) me.EventHandler.OnMouseOver(me); };
    this.Control.onmouseout = function () { me.Listview.HoveringItem = null; if (me.EventHandler.OnMouseOut) me.EventHandler.OnMouseOut(me); };

    if (listview.OnDoubleClick) {
        this.Control.ondblclick = function () { listview.OnDoubleClick(me); };
    }

    //Filter result!
    if (listview.EnableColumnFilters) {
        for (var i = 0; i < this.Items.length && i < listview.ColumnHeaders.length; i++) {
            if (listview.ColumnHeaders[i].HiddenValues.indexOf(this.Items[i]) > -1) {
                this.Control.style.display = 'none';
                break;
            }
        }
    }

    //Drag drop
    //    if(dd)
    //    {
    //        if(this.Dragable) dd.AttachObject(this.Control, this.Name, this.DragType, true);
    //    }

};


IDC_Listview.prototype.ReloadURLItems = function ()
{
    this.ReadURLItems(this.LastUrl, this.LastArguments);
};

IDC_Listview.prototype.ReadURLItems = function (url, formPostArguments)
{
    this.AddItemsFromAJAX(url, formPostArguments);
};

IDC_Listview.prototype.SetAutoUpdate = function (value)
{
    this.AutoRefreshFromAjax = value;

    if (this.AutoRefreshFromAjax)
    {
        this.AutoRefreshCache.innerHTML = '';
        this.AutoRefreshItems.length = 0;
        var me = this;
        setTimeout(function () { me.AutoRefreshListview(); }, (this.AutoRefreshInterval * 1000));
    }
};

IDC_Listview.prototype.AutoRefreshListview = function (fromAjax)
{
    if (fromAjax)
    {
        this.AutoRefreshCache.innerHTML = this.AutoRefreshAjaxObject.Response;
        this.AutoRefreshAjaxObject.Reset();
        this.AutoRefreshAjaxObject.Dispose();
        delete this.AutoRefreshAjaxObject;
        this.AutoRefreshAjaxObject = null;

    }

    if (!this.AutoRefreshFromAjax)
    {
        this.AutoRefreshCache.innerHTML = '';
        this.AutoRefreshItems.length = 0;
        return;
    }

    if (this.AutoRefreshCache.childNodes.length > 0)
    {

        //parse max 10 items and timeout...
        var count = 0;
        while (this.AutoRefreshCache.hasChildNodes() && count < this.AutoRefreshPopulateCount)
        {
            var src = this.AutoRefreshCache.firstChild;
            src = this.AutoRefreshCache.removeChild(src);

            if (src.tagName)
            {
                if (this.ColumnHeaders.length == 0 && src.tagName == 'I')
                {
                    if (isParsingHeaders)
                    {
                        this.ClearColumnHeaders();
                        isParsingHeaders = false;
                    }
                    this.AddColumnHeader(this.isIE ? src.innerText : src.textContent, src.getAttribute('width'), src.getAttribute('align'), src.getAttribute('datatype'), src.getAttribute('sort'), src.getAttribute('mask'));

                    //add group header?
                    if (src.getAttribute('groupColumn'))
                    {
                        if (this.ColumnGroupHeaders.length > 0)
                            groupColumn = this.ColumnGroupHeaders[this.ColumnGroupHeaders.length - 1];

                        if (groupColumn == null)
                        {
                            this.AddGroupColumnHeader(src.getAttribute('groupColumn'), this.ColumnHeaders[this.ColumnHeaders.length - 1], 1);
                        }
                        else if (groupColumn.Name != src.getAttribute('groupColumn'))
                        {

                            this.AddGroupColumnHeader(src.getAttribute('groupColumn'), this.ColumnHeaders[this.ColumnHeaders.length - 1], 1);
                        }
                        else
                        {
                            groupColumn.SetHeaderCount(groupColumn.HeaderCount + 1);
                        }
                    }
                }
                else if (src.tagName != 'I')
                {
                    var uid = src.getAttribute('uid');

                    var item = this.getItemByUniqueidentifier(uid);
                    var v = new Array();

                    while (src.hasChildNodes())
                    {
                        var ss = src.firstChild;
                        ss = src.removeChild(ss);
                        if (ss.tagName)
                            v.push(this.isIE ? ss.innerText : ss.textContent);
                    }

                    if (item)
                    {
                        if (v.toString() != item.Items.toString())
                        {
                            item.SetNames(v);
                            item.SetIcons([src.getAttribute('icon16'), src.getAttribute('icon32'), src.getAttribute('icon48'), src.getAttribute('icon64'), src.getAttribute('icon96'), src.getAttribute('icon128')]);

                            count++;
                        }
                    }
                    else
                    {
                        var l = new IDC_ListviewItem(this.Document, this.Container, v);

                        l.Icon16 = src.getAttribute('icon16');
                        l.Icon32 = src.getAttribute('icon32');
                        l.Icon48 = src.getAttribute('icon48');
                        l.Icon64 = src.getAttribute('icon64');
                        l.Icon96 = src.getAttribute('icon96');
                        l.Icon128 = src.getAttribute('icon128');

                        //Drag drop 
                        l.Dragable = src.getAttribute('drag') == '1';
                        l.DragType = src.getAttribute('dragtype');
                        l.Dropable = src.getAttribute('drop') == '1';
                        l.DropType = src.getAttribute('droptype');
                        l.Uniqueidentifier = src.getAttribute('uid');
                        l.Uniqueidentifier2 = src.getAttribute('uid2');

                        this.AddItemObject(l);

                        count++;
                    }
                    this.AutoRefreshItems.push(uid);
                }
            }
        }

        var me = this;
        if (this.AutoRefreshCache.hasChildNodes())
        {
            setTimeout(function () { me.AutoRefreshListview(); }, this.AutoRefreshPopulateInterval);
            return;
        }
        else
        {
            //Clean deleted items
            if (this.AutoRefreshItems.length > 0)
            {
                for (var i = this.Items.length - 1; i >= 0; i--)
                {
                    if (!this.InAutoUpdateList(this.Items[i].Uniqueidentifier))
                        this.Items[i].Remove();
                }
                this.AutoRefreshItems.length = 0;
            }

            if (this.OnParseComplete)
            {
                this.OnParseComplete(this.Items.length, 0);
            }
            setTimeout(function () { me.AutoRefreshListview(); }, (this.AutoRefreshInterval * 1000));
        }
        return;
    }

    if (fromAjax)
    {
        var me = this;
        setTimeout(function () { me.AutoRefreshListview(); }, (this.AutoRefreshInterval * 1000));
        return;
    }

    //do new request
    if (!this.LastUrl)
    {
        setTimeout(function () { me.AutoRefreshListview(); }, this.AutoRefreshInterval * 1000);
        return;
    }

    this.AutoRefreshItems.length = 0;
    this.AutoRefreshAjaxObject = new this.Core.Ajax();
    this.AutoRefreshAjaxObject.requestFile = this.LastUrl;

    if (this.LastArguments)
    {
        for (var i = 0; i < this.LastArguments.length; i++)
        {
            var arg = this.LastArguments[i];
            if (arg.length == 2)
                this.AutoRefreshAjaxObject.encVar(arg[0], arg[1]);
        }
    }

    var me = this;

    this.AutoRefreshAjaxObject.OnCompletion = function () { me.AutoRefreshListview(true); };
    this.AutoRefreshAjaxObject.OnError = function () { me.AutoRefreshListview(true); };
    this.AutoRefreshAjaxObject.OnFail = function () { me.AutoRefreshListview(true); };
    this.AutoRefreshAjaxObject.RunAJAX();
};

IDC_Listview.prototype.InAutoUpdateList = function (uid)
{
    for (var i = 0; i < this.AutoRefreshItems.length; i++)
    {
        if (uid == this.AutoRefreshItems[i]) return true;
    }
    return false;
};

IDC_Listview.prototype.AddItemsFromAJAX = function (url, formPostArguments) {

    if (this.ShowLoading && this.EnsureNoScriptTimeout) {
        this.Container.scrollTop = 0;
        this.BusyControl = this.Document.createElement('DIV');
        this.BusyControl.style.position = 'absolute';
        this.BusyControl.style.zIndex = 250000;
        this.BusyControl.style.width = this.Container.offsetWidth;
        this.BusyControl.style.height = this.Container.offsetHeight;
        this.BusyControl.style.backgroundColor = '#ffffff';

        if (this.isIE)
            this.BusyControl.style.filter = 'alpha(opacity=50)';
        else
            this.BusyControl.style.MozOpacity = '.50';

        this.BusyControl.innerHTML = '<center><br><br><br><br>Loading...</center>';

        if (this.Container.firstChild)
            this.Container.insertBefore(this.BusyControl, this.Container.firstChild);
        else
            this.Container.appendChild(this.BusyControl);
    }

    this.LastUrl = url;
    this.LastArguments = formPostArguments;

    //    if (this.AjaxObject)
    //    {
    //        this.AjaxObject.Reset();
    //    }
    //    else
    this.AjaxObject = new this.Core.Ajax();

    this.AjaxObject.requestFile = url;

    if (formPostArguments) {
        for (var i = 0; i < formPostArguments.length; i++) {
            var arg = formPostArguments[i];
            if (arg.length == 2)
                this.AjaxObject.encVar(arg[0], arg[1]);
        }
    }

    var me = this;

    this.AjaxObject.OnCompletion = function () { me.__AddItemsFromAJAX(); };
    this.AjaxObject.OnError = function () { me.__ListviewAjaxFailure(); };
    this.AjaxObject.OnFail = function () { me.__ListviewAjaxFailure(); };
    this.AjaxObject.RunAJAX();
};

IDC_Listview.prototype.AddItemsFromHTML = function (html)
{
    var startTicks = parseInt(new Date().getTime());

    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = html;

    var okay = this.__ToItems(tmp);

    if (this.OnLoad && okay)
    {
        this.OnLoad();
    }
};

IDC_Listview.prototype.__AddItemsFromAJAX = function () {
    var result = this.AjaxObject.Response;

    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    delete this.AjaxObject;
    this.AjaxObject = null;

    var startTicks = parseInt(new Date().getTime());

    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    delete result;
    result = null;

    var okay = this.__ToItems(tmp);

    //tmp = null;

    if (this.OnLoad && okay) {
        this.OnLoad();
    }
};

IDC_Listview.prototype.__ToItems = function (what, keepingJSAlive, originalStartTick) {

    if (!keepingJSAlive) {
        if (this.OnParseBegin)
            this.OnParseBegin(what.childNodes.length);

        this.Container.style.visibility = 'hidden';
        this.Clear();
        this.ExpectedItems = what.childNodes.length;

        var isParsingHeaders = true;
    }

    var startTicks = parseInt(new Date().getTime());
    var orgStartTick = originalStartTick || startTicks;
    var groupColumn = null;

    if (parseInt(this.WarnWhenResultsExceeds, 10) > 0) {
        if (orgStartTick == startTicks) {
            var warnAt = parseInt(this.WarnWhenResultsExceeds, 10);
            var lines = 0;
            //Count lines
            for (var i = 0; i < what.childNodes.length; i++) {
                if (what.childNodes[i].tagName) {
                    if (what.childNodes[i].tagName != 'I')
                        lines++;
                }
            }

            if (lines > warnAt) {
                var warningText = this.ExcessiveDataWarningText.replace(/%lines%/g, lines);
                warningText = warningText.replace(/%max%/g, warnAt);

                if (this.Window) {
                    //Confirm = function(Question, Title, Icon, CallBack)
                    var me = this;
                    this.Window.Confirm(warningText, null, null, function (sender, retval) { if (retval) { me.__ToItems(what, keepingJSAlive, orgStartTick); } else { me.OnParseComplete(0, 0); } });
                    return;
                }
                else {
                    if (!confirm(warningText)) {
                        this.OnParseComplete(0, 0);
                        return;
                    }
                }
            }
        }
    }

    while (what.hasChildNodes()) {
        var src = what.firstChild;
        src = what.removeChild(src);

        if (src.tagName) {


            //Column headers
            if (this.Items.length == 0 && src.tagName == 'I') {
                if (isParsingHeaders) {
                    this.ClearColumnHeaders();
                    isParsingHeaders = false;
                }
                this.AddColumnHeader(this.isIE ? src.innerText : src.textContent, src.getAttribute('width'), src.getAttribute('align'), src.getAttribute('datatype'), src.getAttribute('sort'), src.getAttribute('mask'));

                //add group header?
                if (src.getAttribute('groupColumn')) {
                    if (this.ColumnGroupHeaders.length > 0)
                        groupColumn = this.ColumnGroupHeaders[this.ColumnGroupHeaders.length - 1];

                    if (groupColumn == null) {
                        this.AddGroupColumnHeader(src.getAttribute('groupColumn'), this.ColumnHeaders[this.ColumnHeaders.length - 1], 1);
                    }
                    else if (groupColumn.Name != src.getAttribute('groupColumn')) {

                        this.AddGroupColumnHeader(src.getAttribute('groupColumn'), this.ColumnHeaders[this.ColumnHeaders.length - 1], 1);
                    }
                    else {
                        groupColumn.SetHeaderCount(groupColumn.HeaderCount + 1);
                    }
                }
            }
            else {
                var v = new Array();

                while (src.hasChildNodes()) {
                    var ss = src.firstChild;
                    ss = src.removeChild(ss);
                    if (ss.tagName)
                        v.push(this.isIE ? ss.innerText : ss.textContent);
                }

                if (v.length > 0) {
                    var l = new IDC_ListviewItem(this.Document, this.Container, v);

                    l.Icon16 = src.getAttribute('icon16');
                    l.Icon32 = src.getAttribute('icon32');
                    l.Icon48 = src.getAttribute('icon48');
                    l.Icon64 = src.getAttribute('icon64');
                    l.Icon96 = src.getAttribute('icon96');
                    l.Icon128 = src.getAttribute('icon128');

                    //Drag drop 
                    l.Dragable = src.getAttribute('drag') == '1';
                    l.DragType = src.getAttribute('dragtype');
                    l.Dropable = src.getAttribute('drop') == '1';
                    l.DropType = src.getAttribute('droptype');
                    l.Uniqueidentifier = src.getAttribute('uid');
                    l.Uniqueidentifier2 = src.getAttribute('uid2');
                    l.BgColor = src.getAttribute('bgcolor');

                    this.AddItemObject(l);
                }
                else {
                    //alert('row failed');
                }
            }
        }
        delete src;
        src = null;

        var ticks = parseInt(new Date().getTime());
        if (ticks - startTicks > this.PopulationTimeout && this.EnsureNoScriptTimeout) {
            if (this.ShowLoading && !keepingJSAlive) {
                this.Container.scrollTop = 0;
                this.BusyControl = this.Document.createElement('DIV');
                this.BusyControl.style.position = 'absolute';
                this.BusyControl.style.zIndex = 250000;
                this.BusyControl.style.width = this.Container.offsetWidth;
                this.BusyControl.style.height = this.Container.offsetHeight;
                this.BusyControl.style.backgroundColor = '#ffffff';

                if (this.isIE)
                    this.BusyControl.style.filter = 'alpha(opacity=75)';
                else
                    this.BusyControl.style.MozOpacity = '.75';

                this.BusyControl.innerHTML = '<center><br><br><br><br>The listview is containing a large amount of data, and the load is performing slower than normal.<br><br>Please be patient.<br><br><b>' + what.childNodes.length + ' of ' + this.ExpectedItems + ' remaining</b></center>';

                if (this.Container.firstChild)
                    this.Container.insertBefore(this.BusyControl, this.Container.firstChild);
                else
                    this.Container.appendChild(this.BusyControl);
            }
            else if (this.ShowLoading && keepingJSAlive) {
                this.BusyControl.innerHTML = '<center><br><br><br><br>The listview is containing a large amount of data, and the load is performing slower than normal.<br><br>Please be patient.<br><br><b>' + what.childNodes.length + ' of ' + this.ExpectedItems + ' remaining</b></center>';
            }

            if (this.OnParsing) {
                this.OnParsing(what.childNodes.length, this.ExpectedItems, ticks - orgStartTick);
            }

            var me = this;
            setTimeout(function () { me.__ToItems(what, true, orgStartTick); }, this.PopulationSleep);
            return;
        }
    }


    if (this.OnParseComplete && !what.hasChildNodes()) {
        this.OnParseComplete(this.Items.length, ticks - orgStartTick);
    }

    if (keepingJSAlive && !what.hasChildNodes()) {
        if (this.ShowLoading && this.BusyControl != null) {
            this.BusyControl.parentNode.removeChild(this.BusyControl);
            delete this.BusyControl;
            this.BusyControl = null;
        }

        if (this.OnLoad)
            this.OnLoad();
    }

    this.Container.style.visibility = '';

    delete what;
    what = null;

    if (this.isIE)
        CollectGarbage();

    return true;
};

IDC_Listview.prototype.SelectItem = function (item)
{
    //single select
    item.Select(this);
};

IDC_Listview.prototype.Clear = function ()
{
    for (var i = 0; i < this.Items.length; i++)
    {
        this.Items[i].DisposeEvents();
    }

    this.Items.length = 0;
    this.SelectedItems.length = 0;


    while (this.Container.hasChildNodes())
    {
        var o = this.Container.removeChild(this.Container.lastChild);
        delete o;
        o = null;
    }

    this.ColumnHeaderContainer.style.left = -this.Container.scrollLeft;
    this.ColumnHeaderContainer.parentNode.style.width = this.Container.offsetWidth;

    this.ColumnHeaderGroupContainer.style.left = this.ColumnHeaderContainer.style.left;
    this.ColumnHeaderGroupContainer.parentNode.style.width = this.ColumnHeaderContainer.parentNode.style.width;

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';

    if (this.EnableColumnFilters)
    {
        for (var i = 0; i < this.ColumnHeaders.length; i++)
        {
            this.ColumnHeaders[i].UniqueValues.length = 0;
            this.ColumnHeaders[i].HiddenValues.length = 0;
            this.ColumnHeaders[i].FiltersControl = null;
            this.ColumnHeaders[i].FiltersVisisble = false;
            this.ColumnHeaders[i].HideFilterButton();
        }
    }

    if (this.isIE)
        CollectGarbage();
};

IDC_ListviewItem.prototype.Remove = function ()
{
    if (this.EnableColumnFilters)
    {
        for (var i = 0; i < this.Listview.ColumnHeaders.length && i < this.Items.length; i++)
        {
            var inx = this.Listview.ColumnHeaders[i].UniqueValues.indexOf(this.Items[i]);
            this.Listview.ColumnHeaders[i].UniqueValues.splice(inx, 1);
        }
    }

    for (var i = this.Listview.SelectedItems.length - 1; i >= 0; i--)
    {
        if (this.Listview.SelectedItems[i] == this)
        {
            this.Listview.SelectedItems[i].DisposeEvents();
            this.Listview.SelectedItems.splice(i, 1);
        }
    }
    for (var i = this.Listview.Items.length - 1; i >= 0; i--)
    {
        if (this.Listview.Items[i] == this)
        {
            this.Listview.Items[i].DisposeEvents();
            this.Listview.Items.splice(i, 1);
        }
    }



    var n = this.Control.parentNode.removeChild(this.Control);
    delete n;
    n = null;

    if (this.Statusbar)
        this.Statusbar.innerHTML = this.Items.length + ' Items...';

    if (this.isIE)
        CollectGarbage();
};

IDC_Listview.prototype.AddGroupColumnHeader = function (name, beginningColumnHeader, headerCount)
{
    this.ShowColumnGroupHeaders();
    var c = new IDC_ListviewColumnGroupHeader(this, name, beginningColumnHeader, headerCount);
    this.ColumnGroupHeaders.push(c);


    c.Create();

    this.RefactorGroupHeaders();

    return c;
};

IDC_Listview.prototype.AddColumnHeader = function (name, width, textAlign, dataType, sortBy, mask)
{
    var c = new IDC_ListviewColumnHeader(this, name, width, textAlign, dataType, sortBy, mask);

    //Computed widths
    c.__Width = c.Width - 4; // (this.ColumnHeaders.length==0 ? (this.isIE ? 4 : 25) : (this.isIE ? 4 : 7));

    c.Index = this.ColumnHeaders.push(c) - 1;
    c.Create();

    //Rebuild listview
    if (this.viewStyle == 3)
    {
        for (var i = 0; i < this.Items.length; i++)
        {
            this.Items[i].Create(this);
        }
    }

    return c;
};

IDC_Listview.prototype.ClearColumnHeaders = function ()
{
    if (this.ColumnHeaders.length == 0) return;
    for (var i = this.ColumnHeaders.length - 1; i >= 0; i--)
    {
        this.ColumnHeaders[i].Remove();
    }

    if (this.viewStyle == 3)
    {
        for (var i = 0; i < this.Items.length; i++)
        {
            this.Items[i].Create(this);
        }
    }
};

IDC_ListviewColumnGroupHeader = function (listview, name, beginningColumnHeader, headerCount)
{
    this.Listview = listview;
    this.isIE = this.Listview.isIE;
    this.Document = listview.Document;
    this.Container = listview.ColumnHeaderGroupContainer;
    this.Control = null;
    this.Name = name;
    this.BeginningColumnHeader = beginningColumnHeader;
    this.HeaderCount = headerCount || 1;
};

IDC_ListviewColumnGroupHeader.prototype.SetHeaderCount = function (count)
{
    this.HeaderCount = count;

    var w = 0;
    for (var i = this.BeginningColumnHeader.Index; i < this.BeginningColumnHeader.Index + count && i < this.Listview.ColumnHeaders.length; i++)
    {
        w += parseInt(this.Listview.ColumnHeaders[i].Width, 10) - 4;
    }
    this.Control.style.width = w;
};

IDC_Listview.prototype.RefactorGroupHeaders = function ()
{
    if (this.ColumnGroupHeaders.length > 0)
    {
        this.ShowColumnGroupHeaders();
        for (var y = 0; y < this.ColumnGroupHeaders.length; y++)
        {
            var w = 0;
            var col = this.ColumnGroupHeaders[y];
            for (var i = col.BeginningColumnHeader.Index; i < col.BeginningColumnHeader.Index + col.HeaderCount && i < col.Listview.ColumnHeaders.length; i++)
            {
                w += parseInt(col.Listview.ColumnHeaders[i].Width, 10) - 4;
            }
            col.Control.style.width = w;
        }
    }
    else
    {
        this.HideColumnGroupHeaders();
    }
};

IDC_ListviewColumnGroupHeader.prototype.Create = function ()
{
    this.Control = this.Document.createElement('DIV');
    this.Control.style.display = (this.isIE ? 'inline' : 'inline-block');
    //this.Control.style.width='100' + (this.isIE ? '' : 'px');
    this.Control.className = 'columnHeaderItem';

    //this.Container.style.height=50;
    this.Container.style.whiteSpace = 'nowrap';

    this.Control.Column = this;
    this.Container.appendChild(this.Control);
    this.Control.Text = this.Document.createElement('DIV');
    this.Control.Text.innerHTML = this.Name;
    this.Control.Text.style.width = '100%';
    this.Control.Text.style.overflow = 'hidden';

    this.Control.appendChild(this.Control.Text);
};

IDC_ListviewColumnGroupHeader.prototype.ToPrintableHTML = function (style)
{
    return '<td colspan="' + this.HeaderCount + '" style="' + (style || '') + '"><div style="width:' + this.Width + 'px;overflow:hidden;">' + this.Name + '</div></td>';
};

IDC_ListviewColumnGroupHeader.prototype.ToFormPostData = function (es)
{
    var les = '';
    for (var i = 0; i < this.HeaderCount; i++)
        les += es;

    return this.Name + les;
};

IDC_ListviewColumnHeader = function (listview, name, width, textAlign, dataType, sortBy, mask)
{
    this.Listview = listview;
    this.isIE = this.Listview.isIE;
    this.Document = listview.Document;
    this.Container = listview.ColumnHeaderContainer;
    this.Control = null;
    this.Name = name;
    this.Width = width;
    this.Index = 0;
    this.IsSortingColumn = sortBy != null && sortBy != '';
    this.SortAsc = sortBy != 'desc';
    this.DataType = dataType || 'string';
    this.__Width = null;
    this.FilterControl = null;
    this.FiltersControl = null;
    this.FiltersVisisble = false;
    this.Mask = mask || 'D-M-Y';

    this.AbortSortClick = false;

    this.UniqueValues = null;
    this.HiddenValues = null;

    this.TextAlign = textAlign || 'left';
};

IDC_ListviewColumnHeader.prototype.Remove = function (refreshView)
{
    this.Container.removeChild(this.Control);
    this.Listview.ColumnHeaders.splice(this.Index, 1);

    if (refreshView && this.Listview.viewStyle == 3)
    {
        for (var i = 0; i < this.Items.length; i++)
        {
            this.Items[i].Create(this);
        }
    }
};

IDC_ListviewColumnHeader.prototype.ToPrintableHTML = function (style)
{
    return '<td style="' + (style || '') + '"><div style="width:' + this.Width + 'px;overflow:hidden;">' + this.Name + '</div></td>';
};
IDC_ListviewColumnHeader.prototype.ToFormPostData = function (es)
{
    return this.Name;
};

IDC_ListviewColumnHeader.prototype.GetOffsetLeft = function ()
{
    var x = 0;
    for (var i = 0; i < this.Listview.ColumnHeaders.length; i++)
    {
        var col = this.Listview.ColumnHeaders[i];

        if (col == this)
            break;

        x += parseInt(col.Control.style.width, 10);
    }

    return x;
};

IDC_ListviewColumnHeader.prototype.SetSortingState = function (asc)
{
    if (this.Listview.SortingColumn)
    {
        this.Listview.SortingColumn.Control.className = 'columnHeaderItem';
        this.Listview.SortingColumn.IsSortingColumn = false;
    }
    this.SortAsc = asc;
    this.IsSortingColumn = true;
    this.Listview.SortingColumn = this;
    this.Control.className = 'columnHeaderItem' + (!this.SortAsc ? 'Desc' : 'Asc');
};

IDC_ListviewColumnHeader.prototype.Create = function ()
{
    this.Control = this.Document.createElement('DIV');
    this.Control.style.display = (this.isIE ? 'inline' : 'inline-block');
    this.Control.style.width = (this.Width - 4) + (this.isIE ? '' : 'px');
    this.Control.className = 'columnHeaderItem';

    //this.Container.style.height=50;
    this.Container.style.whiteSpace = 'nowrap';

    this.Control.Column = this;
    this.Container.appendChild(this.Control);
    this.Control.Text = this.Document.createElement('DIV');
    this.Control.Text.innerHTML = this.Name;
    this.Control.Text.style.width = '100%';
    this.Control.Text.style.overflow = 'hidden';

    this.Control.appendChild(this.Control.Text);

    if (!this.isIE)
    {
        var lc = this.Listview.ColumnHeaders[this.Listview.ColumnHeaders.length - 1];
        this.Container.style.width = lc.GetOffsetLeft() + parseInt(lc.Control.style.width, 10);
    }


    if (this.Container.childNodes.length == 1 && this.Listview.Sortable)
    {
        this.Listview.SortingColumn = this;
        this.Control.className = 'columnHeaderItemAsc';
        this.SortAsc = true;
        this.IsSortingColumn = true;
    }
    else if (this.IsSortingColumn)
    {
        if (this.Listview.SortingColumn)
        {
            this.Listview.SortingColumn.Control.className = 'columnHeaderItem';
            this.Listview.SortingColumn.IsSortingColumn = false;
        }


        this.Listview.SortingColumn = this;
        this.Control.className = 'columnHeaderItem' + (!this.SortAsc ? 'Desc' : 'Asc');
    }

    if (this.Listview.Sortable)
    {
        if (this.Listview.EnableColumnFilters)
        {
            this.UniqueValues = new Array();
            this.HiddenValues = new Array();
        }

        this.Control.style.cursor = 'pointer';
        this.Control.onmouseover = function () { if (this.Column.Listview.EnableColumnFilters && this.Column.Listview.Items.length > 0) { this.Column.ShowFilterButton(); }; if (!this.Column.IsSortingColumn) this.className = 'columnHeaderItemHover'; };
        this.Control.onmouseout = function () { if (this.Column.HiddenValues) { if (this.Column.HiddenValues.length == 0) { this.Column.HideFilterButton(); } } this.className = (this.Column.IsSortingColumn ? (this.Column.SortAsc ? 'columnHeaderItemAsc' : 'columnHeaderItemDesc') : 'columnHeaderItem'); };
        this.Control.onclick = function () { if (this.Column.AbortSortClick) return; if (!this.Column.FiltersVisisble) this.Column.Listview.SortItemsBy(this.Column); else this.Column.HideFilters(); };

    }

    if (this.Listview.ResizeableColumnHeaders)
    {
        //Resize my column?
        this.Control.ResizeBox = this.Document.createElement('DIV');
        //this.Control.ResizeBox.style.backgroundColor='red';
        this.Control.ResizeBox.className = 'listviewColumnResize';
        this.Control.appendChild(this.Control.ResizeBox);
        this.Control.ResizeBox.style.top = 0;
        this.Control.ResizeBox.style.left = this.GetOffsetLeft() + parseInt(this.Control.style.width) - 1; //(this.isIE ? 1 : -3);

        this.Control.ResizeBox.Column = this;

        this.Control.ResizeBox.onmousedown = function (e)
        {
            //                                                if(this.Column.Listview.WindowManager)
            //                                                {
            //                                                    this.Column.Listview.WindowManager.Desktop.Control.appendChild(this.Column.Listview.__TransparentOverlay);
            //                                                }


            this.Column.BasePos = this.Column.Listview.Core.Mouse.X;
            //                                                if(this.Column.Listview.WindowManager && this.Column.Listview.Window)
            //                                                    this.Column.BasePos += (this.Column.Listview.Window.WindowState==1 ? this.Column.Listview.Window.Left : 0);


            this.Column.HideFilters();
            this.Column.Listview.__TransparentOverlay.ResizeColumn = this.Column;

            this.Column.Listview.__TransparentOverlay.onmousemove = function (e)
            {
                //if(!this.Listview.WindowManager)
                this.Listview.Core.Mouse.RefreshMousePosition(e);
                this.ResizeColumn.ColumnResizing();

            };

            this.Column.Listview.__TransparentOverlay.onmouseup = function (e)
            {
                this.style.display = 'none';
                if (this.ResizeColumn) this.ResizeColumn.ColumnResized();
            };

            this.Column.Listview.__TransparentOverlay.style.display = '';
        };

        this.Control.ResizeBox.onmouseup = function ()
        {
            this.Column.Listview.__TransparentOverlay.style.display = 'none';

            this.Column.BasePos = this.Column.Listview.Core.Mouse.X;
            //                                                if(this.Column.Listview.WindowManager && this.Column.Listview.Window)
            //                                                    this.Column.BasePos += (this.Column.Listview.Window.WindowState==1 ? this.Column.Listview.Window.Left : 0);

        };
    }
};

IDC_ListviewColumnHeader.prototype.ColumnResizing = function ()
{
    var nw = parseInt(this.Width);

    //    if(this.Listview.WindowManager)
    //        nw += this.Listview.WindowManager.Core.Mouse.X - this.BasePos;
    //    else
    nw += this.Listview.Core.Mouse.X - this.BasePos;

    if (nw < (this.Index == 0 ? 30 : 14)) nw = (this.Index == 0 ? 30 : 14);

    this.Control.style.width = nw - 4;

    for (var i = 0; i < this.Listview.ColumnHeaders.length; i++)
    {
        var col = this.Listview.ColumnHeaders[i];
        col.Control.ResizeBox.style.left = col.GetOffsetLeft() + parseInt(col.Control.style.width) - 1; //(this.isIE ? 1 : -3);
        if (col.FilterControl)
        {
            if (col.FilterControl.style.display == '')
                col.ShowFilterButton();
        }
    }
};

IDC_ListviewColumnHeader.prototype.ColumnResized = function ()
{
    this.Width = parseInt(this.Width);

    //    if(this.Listview.WindowManager)
    //        this.Width += this.Listview.WindowManager.Core.Mouse.X - this.BasePos;
    //    else
    this.Width += this.Listview.Core.Mouse.X - this.BasePos;

    if (this.Width < (this.Index == 0 ? 30 : 14)) this.Width = (this.Index == 0 ? 30 : 14);
    this.__Width = this.Width - 4; //(this.Index==0 ? (this.isIE ? 4 : 25) : (this.isIE ? 4 : 7));

    this.Control.style.width = this.Width - 4;

    for (var i = 0; i < this.Listview.ColumnHeaders.length; i++)
    {
        var col = this.Listview.ColumnHeaders[i];
        col.Control.ResizeBox.style.left = col.GetOffsetLeft() + parseInt(col.Control.style.width, 10) - 1; //(this.isIE ? 1 : -3);
        if (col.FilterControl)
        {
            if (col.FilterControl.style.display == '')
                col.ShowFilterButton();
        }
    }

    for (var i = 0; i < this.Listview.Items.length; i++)
    {
        this.Listview.Items[i].Create(this.Listview);
    }

    this.Listview.ColumnHeaderContainer.style.left = -this.Listview.Container.scrollLeft;
    this.Listview.ColumnHeaderContainer.parentNode.style.width = this.Listview.Container.offsetWidth;

    this.Listview.RefactorGroupHeaders();

    this.Listview.ColumnHeaderGroupContainer.style.left = this.Listview.ColumnHeaderContainer.style.left;
    this.Listview.ColumnHeaderGroupContainer.parentNode.style.width = this.Listview.ColumnHeaderContainer.parentNode.style.width;

    if (!this.isIE)
    {
        var lc = this.Listview.ColumnHeaders[this.Listview.ColumnHeaders.length - 1];
        this.Container.style.width = lc.GetOffsetLeft() + parseInt(lc.Control.style.width, 10);
    }


};

IDC_ListviewColumnHeader.prototype.AddUniqueValue = function (value)
{
    if (!Array.indexOf)
    {
        Array.prototype.indexOf = function (obj)
        {
            for (var i = 0; i < this.length; i++)
            {
                if (this[i] == obj)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    if (this.UniqueValues.indexOf(value) < 0)
        this.UniqueValues.push(value);
};

IDC_ListviewColumnHeader.prototype.ShowFilterButton = function ()
{
    if (!this.FilterControl)
    {
        this.FilterControl = this.Document.createElement('IMG');
        this.FilterControl.src = 'images/spacer.gif';
        this.FilterControl.className = 'listviewColumnFilter';
        this.FilterControl.title = 'Filtrer efter "' + this.Name + '"';
        this.FilterControl.alt = this.FilterControl.title;
        this.FilterControl.Column = this;
        this.FilterControl.setAttribute('dontcollapse', 'true');
        this.FilterControl.onclick = function ()
        {
            this.Column.AbortSortClick = true;
            var me = this.Column;
            setTimeout(function () { me.AbortSortClick = false; }, 50);

            if (!this.Column.FiltersVisisble || this.Column.Listview.WindowManager)
                this.Column.ShowFilters();
            else
                this.Column.HideFilters();
        };

        this.Control.appendChild(this.FilterControl);
    }
    this.FilterControl.style.left = this.Control.offsetLeft + parseInt(this.Control.style.width) - 18; //(this.isIE ? 18 : 14);
    this.FilterControl.style.display = '';
};

IDC_ListviewColumnHeader.prototype.ShowFilters = function ()
{
    if (!this.FiltersControl)
    {
        this.FiltersControl = this.Document.createElement('DIV');
        this.FiltersControl.Scroller = this.Document.createElement('DIV');
        this.FiltersControl.Buttons = this.Document.createElement('DIV');

        this.FiltersControl.appendChild(this.FiltersControl.Scroller);
        this.FiltersControl.appendChild(this.FiltersControl.Buttons);

        this.FiltersControl.className = 'listviewFilterFrame';
        this.FiltersControl.Scroller.className = 'listviewFilterScroller';

        this.FiltersControl.Scroller.style.width = parseInt(this.Width) + 15;
        if (parseInt(this.FiltersControl.Scroller.style.width) < 130)
            this.FiltersControl.Scroller.style.width = 130;

        this.Listview.Container.appendChild(this.FiltersControl);

        this.FiltersControl.Buttons.style.textAlign = 'right';
        this.FiltersControl.Buttons.style.whiteSpace = 'nowrap';

        this.FiltersControl.setAttribute('dontcollapse', 'true');
        this.FiltersControl.Buttons.setAttribute('dontcollapse', 'true');
        this.FiltersControl.Scroller.setAttribute('dontcollapse', 'true');


        var btnOK = this.Document.createElement('INPUT');
        btnOK.type = 'button';
        var btnCancel = this.Document.createElement('INPUT');
        btnCancel.type = 'button';

        btnOK.value = 'Filtrer';
        btnCancel.value = 'Annuller';

        btnOK.Column = this;
        btnCancel.Column = this;

        btnCancel.onclick = function () { this.Column.HideFilters(); };

        btnOK.onclick = function ()
        {
            this.Column.HiddenValues = new Array();
            for (var i = 0; i < this.Column.FiltersControl.Scroller.childNodes.length; i++)
            {
                if (this.Column.FiltersControl.Scroller.childNodes[i].className == 'listviewFilterItemHidden')
                {
                    this.Column.HiddenValues.push(this.Column.FiltersControl.Scroller.childNodes[i].title);
                }
            }
            if (this.Column.HiddenValues.length > 0)
                this.Column.ShowFilterButton();
            else
                this.Column.HideFilterButton();

            this.Column.HideFilters();

            for (var i = 0; i < this.Column.Listview.Items.length; i++)
            {
                this.Column.Listview.Items[i].Create(this.Column.Listview);
            }
        };

        this.FiltersControl.Buttons.appendChild(btnOK);
        this.FiltersControl.Buttons.appendChild(btnCancel);
    }

    for (var i = 0; i < this.Listview.ColumnHeaders.length; i++)
    {
        if (this.Listview.ColumnHeaders[i] != this) this.Listview.ColumnHeaders[i].HideFilters();
    }

    this.FiltersVisisble = true;


    if (this.DataType == 'numeric')
        this.UniqueValues.sort(this.__listviewFilterSortedByNumeric);
    else if (this.DataType == 'date')
        this.UniqueValues.sort(this.__listviewFilterSortedByDate);
    else
        this.UniqueValues.sort(this.__listviewFilterSortedBy);

    while (this.FiltersControl.Scroller.hasChildNodes())
    {
        var o = this.FiltersControl.Scroller.removeChild(this.FiltersControl.Scroller.lastChild);
        o = null;
    }

    //Fast select
    var fsel = this.Document.createElement('DIV');
    fsel.className = 'listviewFilterItemChosen';
    fsel.innerHTML = '(Alle)';
    fsel.style.whiteSpace = 'nowrap';
    fsel.title = '(Alle)';
    fsel.setAttribute('dontcollapse', 'true');

    fsel.onclick = function ()
    {
        var newCls = '';
        if (this.className == 'listviewFilterItemChosen')
        {
            newCls = 'listviewFilterItemHidden';
        }
        else
        {
            newCls = 'listviewFilterItemChosen';
        }

        for (var i = 1; i < fsel.parentNode.childNodes.length; i++)
        {
            if (fsel.parentNode.childNodes[i].className != newCls)
            {
                fsel.parentNode.childNodes[i].onclick();
            }
        }

        this.className = newCls;
    };
    this.FiltersControl.Scroller.appendChild(fsel);

    fsel.HiddenCount = 0;
    for (var i = 0; i < this.UniqueValues.length; i++)
    {
        var div = this.Document.createElement('DIV');
        div.className = this.HiddenValues.indexOf(this.UniqueValues[i]) > -1 ? 'listviewFilterItemHidden' : 'listviewFilterItemChosen';
        div.setAttribute('dontcollapse', 'true');

        if (div.className == 'listviewFilterItemHidden') fsel.HiddenCount++;
        div.UniqueValuesCount = this.UniqueValues.length;
        div.innerHTML = this.UniqueValues[i];
        div.style.whiteSpace = 'nowrap';
        div.title = this.UniqueValues[i];
        div.onclick = function ()
        {
            if (this.className == 'listviewFilterItemChosen')
            {
                fsel.HiddenCount++;
                this.className = 'listviewFilterItemHidden';
            }
            else
            {
                fsel.HiddenCount--;
                this.className = 'listviewFilterItemChosen';
            }

            if (fsel.HiddenCount == 0)
                fsel.className = 'listviewFilterItemChosen';
            else if (fsel.HiddenCount == this.UniqueValuesCount)
                fsel.className = 'listviewFilterItemHidden';
            else
                fsel.className = 'listviewFilterItemMixed';
        };
        this.FiltersControl.Scroller.appendChild(div);
    }

    if (fsel.HiddenCount == 0)
        fsel.className = 'listviewFilterItemChosen';
    else if (fsel.HiddenCount == this.UniqueValues.length)
        fsel.className = 'listviewFilterItemHidden';
    else
        fsel.className = 'listviewFilterItemMixed';

    var h = this.Listview.Container.offsetHeight / 2;
    if (h > (this.UniqueValues.length + 1.5) * 18) h = (this.UniqueValues.length + 1.5) * 18;
    this.FiltersControl.Scroller.style.height = h;



    if (!this.Listview.WindowManager)
    {
        var xPos = this.Control.offsetLeft - (this.isIE ? 0 : this.Listview.Container.scrollLeft);

        if (this.Index == this.Listview.ColumnHeaders.length - 1 && this.Index > 0)
        {
            xPos -= (parseInt(this.FiltersControl.Scroller.style.width) - this.Width + 9);
        }

        if (!this.isIE) xPos += this.Listview.Core.DOM.GetObjectPosition(this.Listview.Container)[0];

        this.FiltersControl.style.left = xPos;
        this.FiltersControl.style.top = (this.isIE ? this.Listview.Container.scrollTop : this.Listview.Core.DOM.GetObjectPosition(this.Listview.Container)[1]);

        this.FiltersControl.style.display = '';
    }
    else
    {
        this.FiltersControl.style.display = '';

        var pos = this.Listview.Core.DOM.GetObjectPosition(this.Control);
        this.Listview.WindowManager.ContextMenuManager.ShowFromWindow(this.Listview.Window, this.FiltersControl, pos[0], pos[1] + this.Control.offsetHeight, this.Listview.Core);
    }




};

IDC_ListviewColumnHeader.prototype.HideFilters = function ()
{
    this.FiltersVisisble = false;
    if (this.FiltersControl)
    {
        if (!this.Listview.WindowManager)
            this.FiltersControl.style.display = 'none';
        else
            this.Listview.WindowManager.ContextMenuManager.Hide();
    }
};

IDC_ListviewColumnHeader.prototype.HideFilterButton = function ()
{
    if (this.FilterControl)
    {
        this.FilterControl.style.display = 'none';
    }
};

IDC_Listview.prototype.__ListviewAjaxFailure = function () {
    var result = this.AjaxObject.Response;
    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    delete this.AjaxObject;
    this.AjaxObject = null;

    //alert(result);

    var re = new RegExp('<title\\b[^>]*>(.*?)</title>');
    var m = re.exec(result);
    if (m != null) {
        //alert(m[1]);
    }


    if (this.OnLoadFailed)
        this.OnLoadFailed(result);
};


/*SORT LISTVthis.isIEW BY*/

IDC_Listview.prototype.SortItemsBy = function (column)
{
    var tickStart = new Date().getTime();

    if (this.SortingColumn == column)
    {
        this.SortDirectionAsc = !this.SortDirectionAsc;
        this.SortingColumn.IsSortingColumn = true;
        this.SortingColumn.SortAsc = this.SortDirectionAsc;
        this.SortingColumn.Control.className = this.SortDirectionAsc ? 'columnHeaderItemAsc' : 'columnHeaderItemDesc';
    }
    else
    {
        if (this.SortingColumn)
        {
            this.SortingColumn.Control.className = 'columnHeaderItem';
            this.SortingColumn.IsSortingColumn = false;
            this.SortingColumn.SortAsc = true;
        }

        column.IsSortingColumn = true;
        column.SortAsc = this.SortDirectionAsc;
        column.Control.className = this.SortDirectionAsc ? 'columnHeaderItemAsc' : 'columnHeaderItemDesc';

        this.SortDirectionAsc = true;
        this.SortingColumn = column;
    }

    var me = this;


    if (this.SortDirectionAsc)
        this.Items.sort(function (a, b)
        {
            if (a.CoreItems[me.SortingColumn.Index] == b.CoreItems[me.SortingColumn.Index]) return 0;
            return a.CoreItems[me.SortingColumn.Index] > b.CoreItems[me.SortingColumn.Index] ? 1 : -1;
        }
                            );
    else
        this.Items.sort(function (a, b)
        {
            if (a.CoreItems[me.SortingColumn.Index] == b.CoreItems[me.SortingColumn.Index]) return 0;
            return a.CoreItems[me.SortingColumn.Index] > b.CoreItems[me.SortingColumn.Index] ? -1 : 1;
        }
                            );

    var tickBeforeCreate = new Date().getTime();

    //Rearrange items
    for (var i = 0; i < this.Items.length; i++)
    {
        this.Container.appendChild(this.Container.removeChild(this.Items[i].Control));
    }

    //normal create
    /*for(var i=0;i<this.Items.length;i++)
    {
    this.Items[i].Create(this);
    }*/

    var tickDone = new Date().getTime();

    /*alert('sort ticks: ' + (tickBeforeCreate - tickStart) + '\n' +
    'create ticks: ' + (tickDone - tickBeforeCreate) + '\n' +
    'total ticks: ' + (tickDone - tickStart));*/
};



IDC_ListviewColumnHeader.prototype.__listviewFilterSortedBy = function (a, b)
{
    if (a == b) return 0;
    return a.toLowerCase() > b.toLowerCase() ? 1 : -1;
};

IDC_ListviewColumnHeader.prototype.__listviewFilterSortedByNumeric = function (a, b)
{
    var aa = parseInt(a.replace(/(\.|,)/g, '')) || 0;
    var bb = parseInt(b.replace(/(\.|,)/g, '')) || 0;
    return aa - bb;
};

IDC_ListviewColumnHeader.prototype.__listviewFilterSortedByDate = function (a, b)
{


    var aa = Date.parse(a, 'dd-MM-yyyy hh:mm'); //Date.parse(a); // || new Date();
    var bb = Date.parse(b, 'dd-MM-yyyy hh:mm'); //Date.parse(b); // || new Date();

    return aa - bb;
};



IDC_Listview.prototype.MoveSelectionUp = function ()
{
    this.MoveSelection('up');
};

IDC_Listview.prototype.MoveSelectionDown = function ()
{
    this.MoveSelection('down');
};

IDC_Listview.prototype.MoveSelection = function (Direction)
{
    if (this.SelectedItems.length == 0) return;

    if (Direction == 'down')
    {
        for (var i = this.SelectedItems.length - 1; i >= 0; i--)
        {
            var s = this.SelectedItems[i];
            var after = s.GetNextItem();
            if (after)
            {
                after = after.nextSibling;
                if (after)
                {
                    var tmp = s.Container.removeChild(s.Control);
                    s.Container.insertBefore(tmp, after);
                }
                else
                {
                    var tmp = s.Container.removeChild(s.Control);
                    s.Container.appendChild(tmp);
                }
            }
        }
    }
    else
    {
        for (var i = 0; i < this.SelectedItems.length; i++)
        {
            var s = this.SelectedItems[i];
            var before = s.GetPreviousItem();
            if (before)
            {
                var tmp = s.Container.removeChild(s.Control);
                s.Container.insertBefore(tmp, before);
            }
        }
    }

    //Recompute listview items and selected items
    var objects = this.Container.childNodes;

    this.Items.length = 0;
    this.SelectedItems.length = 0;

    for (var i = 0; i < objects.length; i++)
    {
        var obj = objects[i];
        this.Items.push(obj.ListviewItem);

        if (obj.ListviewItem.IsSelected)
        {
            this.SelectedItems.push(obj.ListviewItem);
        }
    }
};

IDC_ListviewItem.prototype.GetNextItem = function ()
{
    return this.Control.nextSibling;
};

IDC_ListviewItem.prototype.GetPreviousItem = function ()
{
    return this.Control.previousSibling;
};


//Detailed listview item
var IDC_ListviewItemDetailed = function (listview, item)
{
    this.ViewName = 'Detalje liste';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = document.createElement('TABLE');
    this.Control.cellPadding = 0;
    this.Control.cellSpacing = 0;
    this.Control.border = 0;
    this.Control.className = 'listviewDTLine';


    this.Control.tr = this.Control.insertRow(-1);

    if (item.BgColor != undefined) {
        this.Control.tr.style.backgroundColor = item.BgColor;
    }

    cr = this.Control.tr.insertCell(-1).appendChild(this.createDiv(item.Items[0], listview.ColumnHeaders[0].__Width + listview.__UseDimPX, 'listviewDTIcon', 'url(' + listview.CurrentURL + item.Icon16 + ')', listview.ColumnHeaders[0].TextAlign));

    for (var i = 1; i < listview.ColumnHeaders.length && i < item.Items.length; i++)
    {
        this.Control.tr.insertCell(-1).appendChild(this.createDiv(item.Items[i], listview.ColumnHeaders[i].__Width + listview.__UseDimPX, 'listviewDTItem', '', listview.ColumnHeaders[i].TextAlign));
    }



    //EVENTS
    this.SelectionChanged = function (item)
    {
        item.Control.className = (item.IsSelected ? 'listviewDTLine_s' : 'listviewDTLine');
    };

    this.OnMouseOver = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewDTLine_h';
    };
    this.OnMouseOut = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewDTLine';
    };

    this.OnTextChanged = function (item)
    {

        for (var i = 0; i < item.Items.length && i < item.Control.tr.childNodes.length; i++)
        {
            item.Control.tr.childNodes[i].childNodes[0].innerHTML = item.Items[i];
        }
    };

    this.OnIconChanged = function (item)
    {
        item.Control.tr.childNodes[0].childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon16 + ')';
    };

    return this;
};

IDC_ListviewItemDetailed.prototype.createDiv = function (value, w, cls, bgimg, textalign)
{
    var d = document.createElement('DIV');
    d.innerHTML = value;
    d.className = cls;
    d.style.width = w;
    d.style.textAlign = textalign;

    //Clean HTML Tags
    d.title = value.replace(/<(.*?)>/g, '');

    if (bgimg)
        d.style.backgroundImage = bgimg;

    return d;
};


//Small icon item
var IDC_ListviewItemSmallIcon = function (listview, item)
{
    this.ViewName = 'Small icons';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = listview.Document.createElement('DIV');
    var span = listview.Document.createElement('SPAN');

    this.Control.style.display = listview.__InlineBlock;
    this.Control.style.width = listview.ColumnHeaders[0].__Width + (listview.isIE ? '' : 'px');

    if (listview.isIE) span.style.width = this.Control.style.width;

    this.Control.appendChild(span);
    span.innerHTML = item.Items[0];

    span.className = 'listviewSMIconText';

    if (item.Icon16 != null && item.Icon16 != '')
    {
        span.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon16 + ')';
    }
    this.Control.className = 'listviewSMIcon' + (item.IsSelected ? '_s' : '');

    this.Control.title = item.Items[0];


    //EVENTS
    this.SelectionChanged = function (item)
    {
        item.Control.className = 'listviewSMIcon' + (item.IsSelected ? '_s' : '');
    };

    this.OnMouseOver = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewSMIcon_h';
    };
    this.OnMouseOut = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewSMIcon';
    };


    this.OnTextChanged = function (item)
    {
        item.Control.childNodes[0].innerHTML = item.Items[0];
    };

    this.OnIconChanged = function (item)
    {
        item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon16 + ')';
    };

    return this;
};


//Medium icon item
var IDC_ListviewItemMediumIcon = function (listview, item)
{
    this.ViewName = 'Medium icons';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = listview.Document.createElement('DIV');
    this.Control.style.display = listview.__InlineBlock;

    this.Icon = listview.Document.createElement('IMG');
    this.Icon.className = 'listviewMDIconImage';

    this.Text = listview.Document.createElement('SPAN');
    this.Text.className = 'listviewMDIconText';

    if (listview.isIE)
        this.Text.innerText = item.Items[0];
    else
        this.Text.textContent = item.Items[0];

    this.Text.style.display = listview.__InlineBlock;

    if (item.Icon32 != null && item.Icon32 != '')
    {
        this.Icon.src = listview.CurrentURL + item.Icon32;
    }
    else
        this.Icon.style.visibility = 'hidden';

    this.Control.className = 'listviewMDIcon' + (item.IsSelected ? '_s' : '');

    this.Control.title = item.Items[0];

    this.Control.appendChild(this.Icon);
    this.Control.appendChild(this.Text);

    //EVENTS
    this.SelectionChanged = function (item)
    {
        item.Control.className = 'listviewMDIcon' + (item.IsSelected ? '_s' : '');
    };

    this.OnMouseOver = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewMDIcon_h';
    };
    this.OnMouseOut = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewMDIcon';
    };

    this.OnTextChanged = function (item)
    {
        item.Control.childNodes[1].innerHTML = item.Items[0];
    };

    this.OnIconChanged = function (item)
    {
        item.Control.childNodes[0].src = item.Icon32;
    };


    return this;
};

//Large icon item
var IDC_ListviewItemLargeIcon = function (listview, item)
{
    this.ViewName = 'Large icons';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = listview.Document.createElement('DIV');
    this.Control.style.display = listview.__InlineBlock;

    this.Icon = listview.Document.createElement('IMG');
    this.Icon.className = 'listviewLGIconImage';

    this.Text = listview.Document.createElement('SPAN');
    this.Text.className = 'listviewLGIconText';

    if (listview.isIE)
        this.Text.innerText = item.Items[0];
    else
        this.Text.textContent = item.Items[0];

    this.Text.style.display = listview.__InlineBlock;

    if (item.Icon48 != null && item.Icon48 != '')
    {
        this.Icon.src = listview.CurrentURL + item.Icon48;
    }
    else
        this.Icon.style.visibility = 'hidden';

    this.Control.className = 'listviewMDIcon' + (item.IsSelected ? '_s' : '');

    this.Control.title = item.Items[0];

    this.Control.appendChild(this.Icon);
    this.Control.appendChild(this.Text);

    //EVENTS
    this.SelectionChanged = function (item)
    {
        item.Control.className = 'listviewMDIcon' + (item.IsSelected ? '_s' : '');
    };

    this.OnMouseOver = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewMDIcon_h';
    };
    this.OnMouseOut = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewMDIcon';
    };

    this.OnTextChanged = function (item)
    {
        item.Control.childNodes[1].innerHTML = item.Items[0];
    };

    this.OnIconChanged = function (item)
    {
        item.Control.childNodes[0].src = item.Icon48;
    };

    return this;
};


//Gallery icon item
var IDC_ListviewItemGalleryIcon = function (listview, item)
{
    this.ViewName = 'Gallery view';
    this.ViewIcons = null;

    if (!listview || !item) return this;

    this.Control = listview.Document.createElement('DIV');
    this.Control.style.display = listview.__InlineBlock;

    this.Icon = listview.Document.createElement('SPAN');
    this.Icon.className = 'listviewGalleryImage';

    this.Text = listview.Document.createElement('SPAN');
    this.Text.className = 'listviewGalleryText';

    if (listview.isIE)
        this.Text.innerText = item.Items[0];
    else
        this.Text.textContent = item.Items[0];

    this.Text.style.display = listview.__InlineBlock;

    if (item.Icon96 != null && item.Icon96 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon96 + ')';
    }
    else if (item.Icon96 != null && item.Icon96 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon96 + ')';
    }
    else if (item.Icon64 != null && item.Icon64 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon64 + ')';
    }
    else if (item.Icon48 != null && item.Icon48 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon48 + ')';
    }
    else if (item.Icon32 != null && item.Icon32 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon32 + ')';
    }
    else if (item.Icon16 != null && item.Icon16 != '')
    {
        this.Icon.style.backgroundImage = 'url(' + listview.CurrentURL + item.Icon16 + ')';
    }

    this.Control.className = 'listviewIconGallery' + (item.IsSelected ? '_s' : '');

    this.Control.title = item.Items[0];

    this.Control.appendChild(this.Icon);
    this.Control.appendChild(this.Text);

    //EVENTS
    this.SelectionChanged = function (item)
    {
        item.Control.className = 'listviewIconGallery' + (item.IsSelected ? '_s' : '');
    };

    this.OnMouseOver = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewIconGallery_h';
    };
    this.OnMouseOut = function (item)
    {
        if (!item.IsSelected) item.Control.className = 'listviewIconGallery';
    };

    this.OnTextChanged = function (item)
    {
        item.Control.childNodes[1].innerHTML = item.Items[0];
    };

    this.OnIconChanged = function (item)
    {
        if (item.Icon96 != null && item.Icon96 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon96 + ')';
        }
        else if (item.Icon96 != null && item.Icon96 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon96 + ')';
        }
        else if (item.Icon64 != null && item.Icon64 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon64 + ')';
        }
        else if (item.Icon48 != null && item.Icon48 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon48 + ')';
        }
        else if (item.Icon32 != null && item.Icon32 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon32 + ')';
        }
        else if (item.Icon16 != null && item.Icon16 != '')
        {
            item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Listview.CurrentURL + item.Icon16 + ')';
        }
    };

    return this;
};

IDC_ListviewItem.prototype.ScrollIntoView = function () {
    var x = this.Control.offsetLeft;
    var y = this.Control.offsetTop;

    this.Listview.Container.scrollLeft = x;
    this.Listview.Container.scrollTop = y;
};

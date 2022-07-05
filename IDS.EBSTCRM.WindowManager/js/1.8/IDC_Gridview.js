var IDC_Gridview = function (documentObject, parentObject, width, height, idcCore, idcWindowManager, idcWindow)
{
    this.isIE = document.all?true:false;
    this.Document = documentObject || document;
    this.ParentObject = parentObject || this.Document.body;
    this.Core = idcCore || new IDC_Core(documentObject, parentObject);
    this.WindowManager = idcWindowManager;
    this.Window = idcWindow;
    
    this.FFStyle = this.isIE ? '' : 'FF';
    
    this.Control = null;
    this.HorizontalHeader = null;
    this.VerticalHeader = null;
    this.ScrollArea = null;
    this.SelectAll = null;
    this.Grid = null;
    
    this.Rows = new Array();
    this.Columns = new Array();
    
    this.ColumnWidth=60;
    this.RowHeight=20;
    
    this.LastUrl = null;
    this.LastArguments = null;
    
    this.CreateFrame();
    
    this.UniqueId = Math.floor(Math.random()*10000);
    
    this.OnDoubleClick = null;
    this.OnSelect = null;
    
    this.Width = 100;
    this.Height = 100;
    
    this.SetSize(this.Width, this.Height);
};

IDC_Gridview.prototype.CreateFrame = function()
{
    var doc = this.Document;
    this.Control = doc.createElement('TABLE');
    this.Control.border=0;
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.LayoutLocked = false;
    
    this.SelectedItems = new Array();
    
    var tr1 = this.Control.insertRow(-1);
    var tr2 = this.Control.insertRow(-1);
    
    this.SelectAll = tr1.insertCell(-1);
    this.SelectAll.className='idcGridTopLeftBox';
    var tdH = tr1.insertCell(-1);
    this.HorizontalHeader = doc.createElement('DIV');
    tdH.appendChild(this.HorizontalHeader);
    
    
    var tdV = tr2.insertCell(-1);
    this.VerticalHeader = doc.createElement('DIV');
    tdV.appendChild(this.VerticalHeader);
    
    var tdInner = tr2.insertCell(-1);
    
    
    this.ScrollArea = doc.createElement('DIV');
    tdInner.appendChild(this.ScrollArea);
    
    tdH.className='idcGridHHeader';
    tdV.className='idcGridVHeader';
    tdV.vAlign='top';
    tdV.align='right';
    
    this.HorizontalHeader.className='idcGridHHeaderInner';
    this.VerticalHeader.className='idcGridVHeaderInner';
    
    this.HorizontalHeader.Dummy = doc.createElement('IMG');
    this.HorizontalHeader.Dummy.style.width=100;
    this.HorizontalHeader.Dummy.style.height=1;
    this.HorizontalHeader.appendChild(this.HorizontalHeader.Dummy);
    
    this.VerticalHeader.Dummy = doc.createElement('IMG');
    this.VerticalHeader.Dummy.style.width=1;
    this.VerticalHeader.Dummy.style.height=100;
    this.VerticalHeader.appendChild(this.VerticalHeader.Dummy);
    
    this.ScrollArea.className='idcGridScroller';
    
    this.ParentObject.appendChild(this.Control);
    
    var img = doc.createElement('DIV');
    img.style.width=this.VerticalHeader.offsetWidth;
    img.style.height=this.HorizontalHeader.offsetHeight;
    this.SelectAll.appendChild(img);
    
    this.Grid = doc.createElement('TABLE');
    this.Grid.className='idcGridTable';
    this.Grid.border=0;
    this.Grid.cellPadding=0;
    this.Grid.cellSpacing=0;
    
    this.Grid.Gridview=this;
    this.Grid.onclick=function(e)
        {
            var target = null;
            if(!e) e = event;
            target = e.srcElement || e.target;
            if(target.tagName=='TD')
                target = target.childNodes[0];
                
            if(this.Gridview.SelectedItems.length>0)
            {
                this.Gridview.SelectedItems[0].className='idcGridItemInner' + this.Gridview.FFStyle;
                this.Gridview.SelectedItems[0].parentNode.className='idcGridItem' + this.Gridview.FFStyle;
                
            }
            this.Gridview.SelectedItems = [target];
            this.Gridview.SelectedItems[0].className='idcGridItemInnerSelected' + this.Gridview.FFStyle;
            this.Gridview.SelectedItems[0].parentNode.className='idcGridItemSelected' + this.Gridview.FFStyle;
            
            if(this.Gridview.OnSelect) this.Gridview.OnSelect(target);
        };
    
    this.Grid.ondblclick=function(e)
        {
            var target = null;
            if(!e) e = event;
            target = e.srcElement || e.target;
            if(target.tagName=='TD')
                target = target.childNodes[0];
                
            if(this.Gridview.OnDoubleClick) this.Gridview.OnDoubleClick(target);
        };
        
    this.ScrollArea.appendChild(this.Grid);
    this.ScrollArea.vAlign='top';
    this.ScrollArea.Gridview = this;
    this.ScrollArea.onscroll = function()
                                {
                                    this.Gridview.HorizontalHeader.scrollLeft = this.scrollLeft;
                                    this.Gridview.VerticalHeader.scrollTop = this.scrollTop;
                                };
};

IDC_Gridview.prototype.SetSize = function(w,h)
{
    this.Width = w;
    this.Height = h;
    
    this.ScrollArea.style.width=w-this.VerticalHeader.offsetWidth;
    this.ScrollArea.style.height=h-this.HorizontalHeader.offsetHeight;
    
    this.VerticalHeader.style.height=this.ScrollArea.style.height;
    this.HorizontalHeader.style.width=this.ScrollArea.style.width;
    
    this.Control.style.width=w;
    this.Control.style.height=h;
    
    this.RefreshHeaderSizes();
        
};

IDC_Gridview.prototype.RefreshHeaderSizes = function()
{
    this.HorizontalHeader.style.width=this.ScrollArea.offsetWidth;
    this.VerticalHeader.style.height=this.ScrollArea.offsetHeight;
};

IDC_Gridview.prototype.LockLayout = function()
{
    this.Grid.style.display='none';
    this.LayoutLocked = true;
};
IDC_Gridview.prototype.UnlockLayout = function()
{
    this.LayoutLocked = false;
    this.Grid.style.display='';
    this.RefreshHeaderSizes();
};

IDC_Gridview.prototype.AddColumn = function(Name)
{
    return this.AddColumns([Name])[0];
};

IDC_Gridview.prototype.AddColumns = function(Names)
{
    var trs = this.Grid.getElementsByTagName('TR');
    var newCols = new Array();
    
    //add actual grid cells!
    for(var i=0;i<Names.length;i++)
    {
        var nc =  this.CreateColumnHeaderControl(Names[i]);
        nc.RowHeaders = this.Rows;
        this.Columns.push(nc);
        newCols.push(nc);
        this.HorizontalHeader.insertBefore(nc, this.HorizontalHeader.Dummy );
        
        for(var y=0;y<trs.length;y++)
        {
            this.CreateCell(tr);
        }
    }

    if(!this.LayoutLocked) this.RefreshHeaderSizes();
    return newCols;
};

IDC_Gridview.prototype.AddRow = function(Name)
{
    return this.AddRows([Name])[0];
};

IDC_Gridview.prototype.AddRows = function(Names)
{
     var newRows = new Array();
        
    for(var i=0;i<Names.length;i++)
    {
        var tr = this.Grid.insertRow(-1);
        var nr = this.CreateRowHeaderControl(Names[i]);
        nr.Columns = tr;
        
        //if(trLength>0)
        //{
            for(var x=0;x<this.HorizontalHeader.childNodes.length-1;x++)
            {
                this.CreateCell(tr);
            }
        //}
        
        nr.ColumnHeaders = this.Columns;
        this.Rows.push(nr);
        
        newRows.push(nr);
        this.VerticalHeader.insertBefore(nr , this.VerticalHeader.Dummy );        
    }
    
    if(!this.LayoutLocked) this.RefreshHeaderSizes();
    return newRows;
};

IDC_Gridview.prototype.CreateCell = function(tr)
{
    var item = tr.insertCell(-1);
    item.className='idcGridItem' + this.FFStyle;
    item.style.width = this.ColumnWidth;
    item.style.height = this.RowHeight;
    item.Text = this.Document.createElement('DIV');
    item.appendChild(item.Text);
    item.Text.display='block';
    item.Text.style.width=this.ColumnWidth;
    item.Text.style.height = this.RowHeight;
    item.Text.innerHTML='';
    item.Text.className='idcGridItemInner' + this.FFStyle;
    
};

IDC_Gridview.prototype.ClearAll = function()
{
    while(this.HorizontalHeader.childNodes.length>1)
    {
        var src = this.HorizontalHeader.firstChild;
        src = this.HorizontalHeader.removeChild(src);
        delete src;
    }
    
    while(this.VerticalHeader.childNodes.length>1)
    {
        var src = this.VerticalHeader.firstChild;
        src = this.VerticalHeader.removeChild(src);
        delete src;
    }
    
    while(this.Grid.hasChildNodes())
    {
        var src = this.Grid.firstChild;
        src = this.Grid.removeChild(src);
        delete src;
    }
};

IDC_Gridview.prototype.GetRow = function(Name)
{
    var r = this.Document.getElementById('idc_gw_v_' + this.UniqueId + '_' + Name);
    return r;
};

IDC_Gridview.prototype.CreateColumnHeaderControl = function(name)
{
    
    var doc = this.Document;
    var tbl = doc.createElement('DIV');
    
    tbl.RowHeaders = new Array();
    tbl.Gridview = this;
        
    tbl.style.display=doc.all?'inline':'inline-block';
    tbl.id='idc_gw_h_' + this.UniqueId + '_' + name;
    tbl.className='idcGridHorizontalHeaderItem' + this.FFStyle;
    tbl.innerHTML=name;
    tbl.style.width = this.ColumnWidth;
    return tbl;
};

IDC_Gridview.prototype.CreateRowHeaderControl = function(name)
{
    var doc = this.Document;
    var tbl = doc.createElement('DIV');
    
    tbl.ColumnHeaders = new Array();
    tbl.Gridview = this;
    tbl.Columns = null;
    
    tbl.GetColumn = function(name)
                        {
                            var tds = this.Columns.getElementsByTagName('TD');
                            for(var i=0;i<tds.length;i++)
                            {
                                if(this.ColumnHeaders[i].id=='idc_gw_h_' + this.Gridview.UniqueId + '_' + name)
                                {
                                    return tds[i].childNodes[0];
                                }
                            }
                            return null;
                        };
    
    tbl.id='idc_gw_v_' + this.UniqueId + '_' + name;
    tbl.style.width='100%';
    tbl.style.display='block';
    tbl.className='idcGridVerticalHeaderItem' + this.FFStyle;
    tbl.style.height = this.RowHeight;
    tbl.innerHTML=name;
    return tbl;
};

IDC_Gridview.prototype.ReloadURLItems = function()
{
    this.ReadURLItems(this.LastUrl, this.LastArguments);
};

IDC_Gridview.prototype.ReadURLItems = function(url, formPostArguments)
{
    this.AddItemsFromAJAX(url, formPostArguments);
};

IDC_Gridview.prototype.AddItemsFromAJAX = function(url, formPostArguments)
{
    this.LastUrl = url;
    this.LastArguments = formPostArguments;
    
    if(this.AjaxObject)
    {
        this.AjaxObject.Reset();
    }
    else
        this.AjaxObject = new this.Core.Ajax();
        
    this.AjaxObject.requestFile = url;

    if(formPostArguments)
    {
        for(var i=0;i<formPostArguments.length;i++)
        {
            var arg = formPostArguments[i];
            if(arg.length==2)
                this.AjaxObject.encVar(arg[0],arg[1]);
        }
    }
    
    var me = this;
    this.ClearAll();
    
    this.AjaxObject.OnCompletion = function(){ me.__AddItemsFromAJAX(); }; 
    this.AjaxObject.OnError = function(){ me.__ListviewAjaxFailure();};
    this.AjaxObject.OnFail = function(){ me.__ListviewAjaxFailure();};
    this.AjaxObject.RunAJAX();
};



IDC_Gridview.prototype.__AddItemsFromAJAX = function()
{
    var result=this.AjaxObject.Response;
    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    delete this.AjaxObject;
    this.AjaxObject=null;

    var startTicks = parseInt(new Date().getTime());

    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    
    this.LockLayout();
    
    
    delete result;
    result=null;
    
    var lastRowId = '';
    var lastRow = null;
    
    while(tmp.hasChildNodes())
    {
        var src = tmp.firstChild;
        src = tmp.removeChild(src);

        if(src.tagName)
        {
            if(src.tagName=='I')
            {
                this.AddColumn(this.Document.all ? src.innerText : src.textContent);
            }
            else
            {
                var row = null;
                var rid = src.getAttribute('row')
                if(lastRowId == rid)
                    row = lastRow;
                else
                    row = this.GetRow(rid);
                    
                if(row == null)
                    row = this.AddRow(rid);
                
                lastRow=row;
                lastRowId=rid;
                
                var c = src.getAttribute('col');
                if(c!='' && c!=null)
                {
                    var col = row.GetColumn(c);
                    if(col)
                    {
                        if(this.Document.all)
                             col.innerText = src.innerText;
                        else
                            col.textContent = src.textContent;
                            
                        var bg = src.getAttribute('bgcolor');
                        var al = src.getAttribute('textalign');
                        var img = src.getAttribute('bgimg');
                        
                        if(bg) col.style.backgroundColor=bg;
                        if(img) col.style.backgroundImage='url(' + img + ')';
                        if(al) col.style.textAlign=al;
                        col.Uniqueidentifier = src.getAttribute('uid');
                        col.Row = src.getAttribute('row');
                        col.Col = src.getAttribute('col');
                        col.Tooltip = src.getAttribute('tooltip');
                        col.title = col.Tooltip;
                        
                    }
                }
            }
        }
    }
    this.UnlockLayout();
    this.SetSize(this.Width,this.Height);
};

IDC_Gridview.prototype.__ListviewAjaxFailure = function()
{
    var result=this.AjaxObject.Response;
    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    delete this.AjaxObject;
    this.AjaxObject=null;
    
    if(this.OnLoadFailed)
        this.OnLoadFailed(result);
};
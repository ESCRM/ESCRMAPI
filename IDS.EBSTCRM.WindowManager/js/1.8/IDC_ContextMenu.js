var ContextMenuJustShown=false;

var ContextMenu = function (OwnerDocument, OwnerContainer, OwnerWindow, customStyle)
{
    this.isIE = document.all?true:false;
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.Window = OwnerWindow;
    
    this.CustomStyle = customStyle || '';
    
    this.ToolbarButton = null;
    
    this.AutoHide = true;
    
    this.Items = new Array();
    
    this.OnHide = null;
    
    //OVERLAY
    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position='absolute';
    this.__TransparentOverlay.style.zIndex=250000;
    this.__TransparentOverlay.style.width='100%';
    this.__TransparentOverlay.style.height='100%';
    this.__TransparentOverlay.style.left=0;
    this.__TransparentOverlay.style.top=0;
    this.__TransparentOverlay.style.display='none';
    this.__TransparentOverlay.oncontextmenu = function(e) { this.ContextMenu.Hide(); return false; };
    if(this.isIE)
    {
        this.__TransparentOverlay.style.backgroundImage='url(images/spacer.gif)';
        //this.__TransparentOverlay.style.backgroundColor='blue';
        //this.__TransparentOverlay.style.filter = 'alpha(opacity=0)';
    }
    this.__TransparentOverlay.ContextMenu = this;
    this.__TransparentOverlay.onclick = function() { this.ContextMenu.Hide(); };
    
    //TASKMAN?
    this.__TransparentOverlayTaskman = null;

    
    this.Document.body.appendChild(this.__TransparentOverlay);
 
    //ROOT
    this.Control = this.Document.createElement('DIV');
    this.Control.style.border='solid 1px black';
    var tbl = this.Document.createElement('TABLE');
    this.Control.Items =this.Document.createElement('TBODY');
    this.Control.style.position='absolute';
    this.Control.style.top=-10000;
    tbl.border=0;
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.style.width=16;
    this.Control.style.zIndex=250001;
    this.Control.style.display='none';    
    this.Control.ContextMenu = this;
    //this.Control.onclick = function() {  if(this.ContextMenu.AutoHide) { this.ContextMenu.Hide(); } };
    tbl.className='contextMenuFrame' + this.CustomStyle;
    tbl.appendChild(this.Control.Items);
    this.Control.appendChild(tbl);
    this.Container.appendChild(this.Control);
};

ContextMenu.prototype.BindAsRightClickMenu = function(ct, bindingObject, offsetX, offsetY)
{
    bindingObject.oncontextmenu = function(e) { 
                                            if(io) localCore.Mouse.LastButton = 2;
                                            ct.ShowStatic(localCore.Mouse.GlobalX + (offsetX ? offsetX : 0), localCore.Mouse.GlobalY + (offsetY ? offsetY : 0)); 
                                            return false;
                                        };
};

ContextMenu.prototype.Dispose = function()
{
    var o = null;
    
    if(this.__TransparentOverlay)
    {
        if(this.__TransparentOverlay.parentNode)
            this.__TransparentOverlay.parentNode.removeChild(this.__TransparentOverlay);
    }   
    o=null;
    if(this.Container && this.Control)
        o = this.Container.removeChild(this.Control);
    o=null;
    this.Control = null;
    this.__TransparentOverlay = null;
    this.Items = null;
    this.Document = null;
    this.Container = null;
    this.Window = null;
    
    if(this.__TransparentOverlayTaskman)
    {
        if(this.__TransparentOverlayTaskman.parentNode)
            this.__TransparentOverlayTaskman.parentNode.removeChild(this.__TransparentOverlayTaskman);
    }
};

ContextMenu.prototype.Hide = function(dontInvokeCallback)
{
    var divs = this.Control.getElementsByTagName('DIV');
    for(var i=0;i<divs.length;i++)
    {
        if(divs[i].id==null)
            divs[i].style.display='none';
    }
    
    this.Control.style.display='none';
    this.__TransparentOverlay.style.display='none';
    
    if(this.__TransparentOverlayTaskman) this.__TransparentOverlayTaskman.style.display='none';
    
    if(this.OnHide && !dontInvokeCallback)
    {
        this.OnHide();
    }
};


ContextMenu.prototype.ShowFromOwner = function(obj)
{
    ContextMenuJustShown = true;
    setTimeout('ContextMenuJustShown=false',100);

    var xy = core.DOM.GetObjectPosition(obj);
    this.__TransparentOverlay.style.display='';
    
    if(this.__TransparentOverlayTaskman)
    {
        if(!this.__TransparentOverlayTaskman.parentNode && this.__TransparentOverlayTaskman.Document.body)
            this.__TransparentOverlayTaskman.Document.body.appendChild(this.__TransparentOverlayTaskman);
            
        this.__TransparentOverlayTaskman.style.display='';
    }
    
    var x = xy[0];
    var y = xy[1] + obj.offsetHeight;
    
    if(window.__OFFSET_X) x += window.__OFFSET_X + 7;
    if(window.__OFFSET_Y) y += window.__OFFSET_Y + 25;
    
    this.Control.style.display='';
    this.Control.style.left=x;
    this.Control.style.top=y;

};

ContextMenu.prototype.Show = function(x, y)
{
    ContextMenuJustShown = true;
    setTimeout('ContextMenuJustShown=false',100);
    
    this.__TransparentOverlay.style.display='';
    
    if(this.__TransparentOverlayTaskman)
    {
        if(!this.__TransparentOverlayTaskman.parentNode && this.__TransparentOverlayTaskman.Document.body)
            this.__TransparentOverlayTaskman.Document.body.appendChild(this.__TransparentOverlayTaskman);
            
        this.__TransparentOverlayTaskman.style.display='';
    }
    
    if(window.__WINDOW)
    {
        if(window.__WINDOW.UI.state != 1)
        {
            if(window.__OFFSET_X) x += window.__OFFSET_X;
            if(window.__OFFSET_Y) y += window.__OFFSET_Y;
        }
    }
    
    this.Control.style.display='';
    this.Control.style.left=x;
    this.Control.style.bottom=null;
    this.Control.style.top=y;
    

};

ContextMenu.prototype.ShowFromBottom = function(x, ry)
{
    ContextMenuJustShown = true;
    setTimeout('ContextMenuJustShown=false',100);
    
    this.__TransparentOverlay.style.display='';
    
    if(this.__TransparentOverlayTaskman)
    {
        if(!this.__TransparentOverlayTaskman.parentNode && this.__TransparentOverlayTaskman.Document.body)
            this.__TransparentOverlayTaskman.Document.body.appendChild(this.__TransparentOverlayTaskman);
            
        this.__TransparentOverlayTaskman.style.display='';
    }
    
    if(window.__WINDOW)
    {
        if(window.__WINDOW.UI.state != 1)
        {
            if(window.__OFFSET_X) x += window.__OFFSET_X;
            if(window.__OFFSET_Y) y += window.__OFFSET_Y;
        }
    }
    
    this.Control.style.display='';
    this.Control.style.left=x;
    this.Control.style.top=null;
    this.Control.style.bottom=ry;
};

ContextMenu.prototype.ShowStatic = function(x, y)
{
    ContextMenuJustShown = true;
    setTimeout('ContextMenuJustShown=false',100);
    
    this.__TransparentOverlay.style.display='';
    
    if(this.__TransparentOverlayTaskman)
    {
        if(!this.__TransparentOverlayTaskman.parentNode && this.__TransparentOverlayTaskman.Document.body)
            this.__TransparentOverlayTaskman.Document.body.appendChild(this.__TransparentOverlayTaskman);
            
        this.__TransparentOverlayTaskman.style.display='';
    }
    
    this.Control.style.display='';
    
    if(y + this.Control.offsetHeight > this.__TransparentOverlay.offsetHeight)
    {
        y = this.__TransparentOverlay.offsetHeight - this.Control.offsetHeight;
    }
    
    this.Control.style.left=x;
    this.Control.style.top=y;

};

ContextMenu.prototype.AddLargeItem = function(name, icon, onclick, width)
{
    var i = this.Items.push(new ContextMenuItem(this, this.Control, this.Document, name, icon, onclick, null, this.CustomStyle, true, this.ToolbarButton, width));
    var n = this.Items[i-1];
    n.Create();
    
    return n;
};

ContextMenu.prototype.AddItem = function(name, icon, onclick, width)
{
    var i = this.Items.push(new ContextMenuItem(this, this.Control, this.Document, name, icon, onclick, null, this.CustomStyle, false, this.ToolbarButton, width));
    var n = this.Items[i-1];
    n.Create();
    
    return n;
};


ContextMenu.prototype.AddSeperator = function()
{
    var i = this.Items.push(new ContextMenuSeperator(this, this.Control, this.Document, this.CustomStyle));
    var n = this.Items[i-1];
    n.Create();
    
    return n;
};

ContextMenu.prototype.AddCustomData = function()
{
    var i = this.Items.push(new ContextMenuCustomData(this, this.Control, this.Document));
    var n = this.Items[i-1];

    return n;
};

var ContextMenuCustomData = function (contextMenuBase, ParentObject, ParentDocument)
{
    
    this.ContextMenuBase = contextMenuBase;
    this.Parent = ParentObject;
    this.Document = ParentDocument;
    
    var div = this.Document.createElement('TR');
    this.CustomDataContainer = this.Document.createElement('TD');
    this.CustomDataContainer.colSpan=3;
    
    if(this.ContextMenuBase.isIE)
        this.CustomDataContainer.style.width='100%';
    
    div.appendChild(this.CustomDataContainer);
    
    this.Parent.Items.appendChild(div);
    
};

var ContextMenuSeperator = function (contextMenuBase, ParentObject, ParentDocument, customStyle)
{
    this.ContextMenuBase = contextMenuBase;
    this.Parent = ParentObject;
    this.Document = ParentDocument;
    this.CustomStyle = customStyle || '';
};
ContextMenuSeperator.prototype.Create = function()
{
    var div = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var td2 = this.Document.createElement('TD');
    
    div.appendChild(td1);
    div.appendChild(td2);
    
    td1.className='contextMenuItemSeperatorLeft' + this.CustomStyle;
    td2.className='contextMenuItemSeperatorRight' + this.CustomStyle;
    
    td2.colSpan=2;
    td1.innerHTML='<img src="images/spacer.gif" alt="" border="0">';
    td2.innerHTML='<img src="images/spacer.gif" alt="" border="0">';
    
    this.Parent.Items.appendChild(div);
};

var ContextMenuItem = function (contextMenuBase, ParentObject, ParentDocument, name, icon, onclick, ownerNode, customStyle, isLarge, toolbarButton, width)
{
    this.ContextMenuBase = contextMenuBase;
    this.ToolbarButton = toolbarButton;
    this.CustomStyle = customStyle || '';
    this.Parent = ParentObject;
    this.Items = new Array();
    this.Document = ParentDocument;
    this.Name = name;
    this.Icon = icon;
    this.OnClick = onclick;
    this.OwnerNode = ownerNode;
    
    this.__Image = null;
    this.__Text = null;
    this.__SubNodeArrow = null;
    
    this.IsLarge = isLarge;
    this.Width = width;
};

ContextMenuItem.prototype.Create = function()
{
    var div = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var tdImg = this.Document.createElement('TD');
    var tdChildren = this.Document.createElement('TD');
    
    var nodeImg = this.Document.createElement('IMG');
    var img = this.Document.createElement('IMG');
    img.border=0;
    img.alt='';
    img.src='images/navigatorRight.gif';
    img.style.visibility='hidden';
    
    nodeImg.border=0;
    nodeImg.alt='';
    tdImg.appendChild(nodeImg);
    
    if(this.Icon && this.Icon!='')
        nodeImg.src=this.Icon;
    else
        nodeImg.style.visibility='hidden';
    
    nodeImg.style.width=this.IsLarge ? 32: 16;
    nodeImg.style.height=this.IsLarge ? 32: 16;
    

    
    this.__Image = tdImg;
    this.__Text = td1;
    this.__SubNodeArrow = img;
    
    if(this.OwnerNode)
    {
        this.OwnerNode.__SubNodeArrow.style.visibility='visible';
    }

    this.__IsLarge = (this.IsLarge ? 'Large' : '');
    tdImg.className='contextMenuItemLeft' + this.CustomStyle + this.__IsLarge;
    td1.className='contextMenuItemRight' + this.CustomStyle + this.__IsLarge;
    td1.style.width='100%';
    td1.noWrap=true;
    
    if(this.Width)
    {
        var w = this.Width - (this.IsLarge ? 32: 16) - (this.ContextMenuBase.isIE ? 25 : 33);
        if(w<1) w=1;
        td1.style.width = w;
    }
    td1.innerHTML=this.Name;
    
    div.appendChild(tdImg);
    div.appendChild(td1);
    div.appendChild(tdChildren);
    
    tdChildren.vAlign='top';
    
    

        
    if(this.ContextMenuBase.isIE)
        div.style.width='100%';
    
    this.Parent.Items.appendChild(div);
    
    //CHildren
    this.Control = this.Document.createElement('DIV');
    this.Control.style.border='solid 1px black';
    var tbl = this.Document.createElement('TABLE');
    this.Control.Items =this.Document.createElement('TBODY');
    this.Control.style.position='absolute';
    tbl.border=0;
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.style.width=16;
    this.Control.style.zIndex=250001;
    this.Control.style.display='none';    
    this.Control.ContextMenu = this;
    
    div.ContextMenu = this;
    
    div.onclick = function()
                        {                            
                            if(this.ContextMenu.OnClick)
                                 this.ContextMenu.OnClick();
                            if(this.ContextMenu.Items.length==0) this.ContextMenu.ContextMenuBase.Hide();
                        };
                        
    //this.Control.onclick = function() { this.ContextMenu.Hide(); };
    this.Control.onclick = function() { 
                                        //if(this.ContextMenu.Items.length>0) return;
                                        //alert('ctrl click: ' + this.ContextMenu.Name);
                                        var divs = this.getElementsByTagName('DIV');
                                        for(var i=0;i<divs.length;i++)
                                        {
                                            divs[i].style.display='none';
                                        }
                                        
                                        this.ContextMenu.Control.style.display='none'; 
                                      };
                                      
    tbl.className='contextMenuFrame' + this.CustomStyle;
    tbl.appendChild(this.Control.Items);
    this.Control.appendChild(tbl);
    tdChildren.appendChild(this.Control);
    tdChildren.appendChild(img);
    
    tdImg.className='contextMenuItemLeft' + this.CustomStyle + this.__IsLarge;
    td1.className='contextMenuItemText' + this.CustomStyle + this.__IsLarge;
    tdChildren.className='contextMenuItemRight' + this.CustomStyle + this.__IsLarge;
    
    //EVENTS

    div.ContextMenu = this;
    
    div.className='contextMenuItem';
    div.onmouseover = function() { 
                                    
                                    tdImg.className='contextMenuItemLeft' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge + '_h';
                                    td1.className='contextMenuItemText' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge + '_h';
                                    tdChildren.className='contextMenuItemRight' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge + '_h';
                                    
                                    if(this.ContextMenu.Control.Items.childNodes.length>0)
                                    {
                                        this.ContextMenu.Control.style.display='';
                                    }
                                };
    div.onmouseout = function() { 
                                    tdImg.className='contextMenuItemLeft' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge;
                                    td1.className='contextMenuItemText' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge;
                                    tdChildren.className='contextMenuItemRight' + this.ContextMenu.CustomStyle + this.ContextMenu.__IsLarge;

                                    this.ContextMenu.Control.style.display='none';
                                };
                                
    div.onmouseup = function()
                                {
                                    if(this.ContextMenu.ToolbarButton && this.ContextMenu.Items.length==0)
                                    {
                                        this.ContextMenu.ToolbarButton.Image.src = this.ContextMenu.Icon;
                                    }
                                };
};

ContextMenuItem.prototype.AddItem = function(name, icon, onclick, width)
{
    var i = this.Items.push(new ContextMenuItem(this.ContextMenuBase, this.Control, this.Document, name, icon, onclick, this, this.CustomStyle, false, this.ToolbarButton, width));
    var n = this.Items[i-1];
    n.Create();
    
    return n;
};

ContextMenuItem.prototype.AddSeperator = function()
{
    var i = this.Items.push(new ContextMenuSeperator(this.ContextMenuBase, this.Control, this.Document, this.CustomStyle));
    var n = this.Items[i-1];
    n.Create();
    
    return n;
};

ContextMenuItem.prototype.AddCustomData = function()
{
    var i = this.Items.push(new ContextMenuCustomData(this.ContextMenuBase, this.Control, this.Document));
    var n = this.Items[i-1];

    return n;
};

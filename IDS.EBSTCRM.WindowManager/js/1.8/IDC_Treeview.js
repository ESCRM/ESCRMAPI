var IDC_Treeview = function (OwnerDocument, OwnerContainer, localCore, windowManager) {
    this.Core = localCore || new IDC_Core(OwnerDocument, OwnerContainer);
    this.WindowManager = windowManager;
    this.isIE = document.all ? true : false;

    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.Navigator = null;
    this.Control = null;

    this.AllowDrag = false;
    this.AllowDrop = false;

    this.SelectedNode = null;
    this.HoveringNode = null;
    this.OnNodeSelect = null;
    this.OnNodeDeSelect = null;
    this.OnNodeDoubleClick = null;

    this.PerformSelectionBeforeSelectEvent = false;

    this.Onload = null;
    this.OnNodeDrop = null;
    this.OnContextMenu = null;
    this.OnError = null;
    this.ShowCountTextAutomatic = true;

    this.Nodes = new Array();

    this.Ajax = null;

    this.ContextMenu = null;
    this.OnBeforeContextMenu = null;

    this.Container.Treeview = this;

    this.Container.oncontextmenu = function (e) {
        var target = e ? e.target : event.srcElement;
        if (target == this && this.Treeview.ContextMenu) {
            if (this.Treeview.SelectedNode != this.Treeview.HoveringNode && this.Treeview.HoveringNode != null)
                this.Treeview.HoveringNode.Select();

            if (this.Treeview.OnBeforeContextMenu)
                this.Treeview.OnBeforeContextMenu(this.Treeview.SelectedNode);

            this.Treeview.ContextMenu.Show(this.Treeview.ContextMenu.Core.Mouse.X, this.Treeview.ContextMenu.Core.Mouse.Y + this.Treeview.ContextMenu.WindowManager.Desktop.Control.offsetTop);
        }
        return false;
    };
};
IDC_Treeview.prototype.ShowCounter = function()
{
    this.SetCountVisible(true);
};
IDC_Treeview.prototype.HideCounter = function()
{
    this.SetCountVisible(false);
};
IDC_Treeview.prototype.SetCountVisible = function(visible, pn)
{
    if(pn==null)
        pn = this;
        
    for(var i=0;i<pn.Nodes.length;i++)
    {
        pn.Nodes[i].SetCountVisible(visible);
        this.SetCountVisible(visible, pn.Nodes[i]);
    }
    //this.__NodeCountContainer.style.display = visible ? '' : 'none';
};

IDC_Treeview.prototype.Clear = function()
{
    this.Nodes = new Array();
    while(this.Container.hasChildNodes())
    {
        this.Container.removeChild(this.Container.lastChild); 
    }
};

IDC_Treeview.prototype.RemoveChildren = function()
{
    this.Nodes = new Array();
    while(this.Container.hasChildNodes())
    {
        this.Container.removeChild(this.Container.lastChild); 
    }
};

IDC_Treeview.prototype.GetAllCheckedNodes = function()
{
    var ns = new Array();
    for(var i=0;i<this.Nodes.length;i++)
    {
        if(this.Nodes[i].Checked && this.Nodes[i].Checkbox)
            ns.push(this.Nodes[i]);
        
        if(this.Nodes[i].Nodes.length>0)
            this.Nodes[i]._GetAllCheckedNodes(ns);
    }
    
    return ns;
};

IDC_Treeview.prototype.GetNodeByUniqueidentifier = function(id)
{
    for(var i=0;i<this.Nodes.length;i++)
    {
        if(this.Nodes[i].Uniqueidentifier == id)
            return this.Nodes[i];
        
        if(this.Nodes[i].Nodes.length>0)
        {
            var retval = this.Nodes[i].GetNodeByUniqueidentifier(id);
            if(retval) return retval;
        }
    }
    
    return null;
};

IDC_Treeview.prototype.MoveNodeTo = function (sourceNode, destinationNode) {
    var d = destinationNode;
    while (d != null) {
        d = d.ParentTreeviewNode;
        if (d == sourceNode) { return; }
    }

    var n = sourceNode;
    n.__Node.parentNode.removeChild(n.__Node);
    var tn = sourceNode.ParentTreeviewNode;

    if (tn) {

        if (tn.AjaxChildCount > 0) {
            tn.AjaxChildCount--;
        }

        for (var i = tn.Nodes.length; i >= 0; i--) {
            if (tn.Nodes[i] == srcNode) {
                tn.Nodes.splice(i, 1);
            }
        }

        if (tn.Nodes.length == 0 && tn.AjaxChildCount == 0) {
            tn.Expander.Image.style.visibility = 'hidden';
        }
    }

    n.ParentTreeviewNode = destinationNode;
    n.Parent = destNode.Container;

    destinationNode.AddNodeObject(n);
    n.Select(n);
};

IDC_Treeview.prototype.ReadURLNodes = function(Url, Args, DepthsToExpand)
{
    
    var AjaxObject = new this.Core.Ajax();
    AjaxObject.requestFile = Url;

    if(Args)
        AjaxObject.encVar('args',Args);

    var me = this;

    AjaxObject.OnCompletion = function(){ me.__ReadURLNodes(AjaxObject,DepthsToExpand); }; 
    AjaxObject.OnError = function(){ me.__TreeviewAjaxFailure(AjaxObject);};
    AjaxObject.OnFail = function(){  me.__TreeviewAjaxFailure(AjaxObject);};

    AjaxObject.RunAJAX();
};


IDC_Treeview.prototype.__ReadURLNodes = function(AjaxObject,DepthsToExpand)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
    
    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    
    this.__ToNodes(tmp, DepthsToExpand);
    
    tmp = null;
     
    if(this.Onload)
    {
        this.Onload(this);
    }
};

IDC_Treeview.prototype.__ToNodes = function(what, Depths, selectThisGuid)
{
    this.RemoveChildren();

    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        if(src.tagName)
        {
            var newNode = new IDC_TreeviewNode(this, null, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'), src.getAttribute('tooltip'));
             
            newNode.ReadNodesFromURL = src.getAttribute('url');
            newNode.ReadNodesFromURLPostedArguments = src.getAttribute('args');
            
            //Drag drop 
            newNode.Dragable = src.getAttribute('drag') == '1';
            newNode.DragType = src.getAttribute('dragtype');
            newNode.Dropable = src.getAttribute('drop') == '1';
            newNode.DropType = src.getAttribute('droptype');
            newNode.Uniqueidentifier = src.getAttribute('uid');
            newNode.AjaxChildCount = src.getAttribute('childcount');
            
            newNode.Checkbox=src.getAttribute('checkbox') == '1';
            newNode.Checked=src.getAttribute('checked') == '1';
            newNode.CountText = src.getAttribute('counttext') || '';

            
            newNode = this.AddNodeObject(newNode);
            if(newNode.Uniqueidentifier == selectThisGuid && this != null)
            {
                this.Treeview.SelectNode(newNode);
            }
            if(src.getAttribute('selected') == '1')
                newNode.Select();
            
            //alt text?
            if(!this.isIE)
            {
                if(!src.firstChild.tagName)
                    newNode.SetName(this.isIE ? src.firstChild.innerText : src.firstChild.textContent);
            }
            else
            {
                var txtIndex = src.innerHTML.indexOf('<');
                if(txtIndex>-1)
                {  
                    newNode.SetName(src.innerHTML.substring(0,txtIndex));
                }
            }

            if(!isNaN(Depths) && Depths != null)
            {
                newNode.__ToNodes(src, Depths-1, selectThisGuid);
                
                if(Depths>0)
                {
                    newNode.Expand(Depths-1, selectThisGuid);
                }
            }
            else
            {
                newNode.__ToNodes(src,null, selectThisGuid);
            }
        }
    }
};


IDC_Treeview.prototype.SelectNode = function(node)
{
    if(node)
        node.Select(node);
};

IDC_Treeview.prototype.DeSelectNode = function(node)
{
    if(node)
    {
        node.DeSelect(node);
    }
};

IDC_Treeview.prototype.SetSize = function (width, height)
{
    if(width<0) width=0;
    if(height<0) height=0;
    this.Container.style.width = width;
    this.Container.style.height = height;
};

IDC_Treeview.prototype.AddNode = function (name, icon)
{
    var i = this.Nodes.push(new IDC_TreeviewNode(this, null, name, icon));
    var n = this.Nodes[i-1];
    n.Treeview = this;
    n.Create();
    
    return n;
};

IDC_Treeview.prototype.AddNodeObject = function (node)
{
    node.ParentTreeviewNode = null;
    var i = this.Nodes.push(node);
    var n = this.Nodes[i-1];
    n.Treeview = this;
    n.Create();
    
    return n;
};

var IDC_TreeviewNode = function (treeview, ParentNode, name, icon, tooltip) {
    this.Parent = null;
    this.Document = tw.Document;
    this.Name = name;
    this.CountText = '';
    this.ToolTip = tooltip || '';

    this.Icon = icon;
    this.SelectedIcon = icon;
    this.Nodes = new Array();
    this.Expander = null;
    this.Expanded = false;
    this.ReadNodesFromURL = null;
    this.ReadNodesFromURLPostedArguments = null;
    this.__Node = null;
    this.__NodeText = null;
    this.__NodeCount = null;
    this.__NodeImage = null;
    this.__NodeImageFrame = null;
    this.__NodeTextContainer = null;
    this.__NodeCountContainer = null;

    this.ShowCountTextAutomatic = treeview.ShowCountTextAutomatic;

    this.Control = null;
    this.NodesContainer = null;

    this.Dragable = false;
    this.Dropable = false;
    this.DragType = null;
    this.DropType = null;

    this.Checkbox = false;
    this.Checked = false;

    this.Uniqueidentifier = null;
    this.AjaxChildCount = 0;

    this.ParentTreeviewNode = ParentNode;
    this.Treeview = treeview;
    this.isIE = document.all ? true : false;
    this.OnSelect = null;
    this.OnDeSelect = null;

    this.ContextMenu = ParentNode != null ? ParentNode.ContextMenuForChildren : null;
    this.ContextMenuForChildren = this.ContextMenu;
};

IDC_TreeviewNode.prototype.SetToolTip = function (tooltip) {
    if (tooltip == null)
        tooltip = '';

    this.ToolTip = tooltip;
    if (this.__NodeText)
        this.__NodeText.title = tooltip;

    if (this.__NodeImage) {
        this.__NodeImage.alt = tooltip;
        this.__NodeImage.title = tooltip;
    }
};

IDC_TreeviewNode.prototype._GetAllCheckedNodes = function(ns)
{
    for(var i=0;i<this.Nodes.length;i++)
    {
        if(this.Nodes[i].Checked && this.Nodes[i].Checkbox)
            ns.push(this.Nodes[i]);
        
        if(this.Nodes[i].Nodes.length>0)
            this.Nodes[i]._GetAllCheckedNodes(ns);
    }
};
IDC_TreeviewNode.prototype.GetNodeByUniqueidentifier = function(id)
{
    for(var i=0;i<this.Nodes.length;i++)
    {
        if(this.Nodes[i].Uniqueidentifier == id)
            return this.Nodes[i];
        
        if(this.Nodes[i].Nodes.length>0)
        {
            var retval = this.Nodes[i].GetNodeByUniqueidentifier(id);
            if(retval) return retval;
        }
    }
    
    return null;
};

IDC_TreeviewNode.prototype.SetName = function(name)
{
    this.Name=name;
    if(this.__NodeText)
        this.__NodeText.innerHTML = name;
};

IDC_TreeviewNode.prototype.SetIcon = function(icon)
{
    this.Icon=icon;
    if(this.__NodeImage)
        this.__NodeImage.src = icon;
};

IDC_TreeviewNode.prototype.SetCheckbox = function(value)
{
    this.Checkbox = value;
    this.__CheckBoxContainer.style.display = (this.Checkbox ? '' : 'none');
};

IDC_TreeviewNode.prototype.SetCheckboxChecked = function(value)
{
    this.Checked = value;
    this.__Checkbox.checked = value;
};

IDC_TreeviewNode.prototype.Create = function () {
    var ct = this.Document.createElement('TABLE');
    var ttb = this.Document.createElement('TBODY');

    var ttr1 = this.Document.createElement('TR');
    var ttr2 = this.Document.createElement('TR');

    var ttd1 = this.Document.createElement('TD');
    var ttd2 = this.Document.createElement('TD');
    var ttd3 = this.Document.createElement('TD');


    //ct.style.width = '100%';
    this.__CheckBoxContainer = this.Document.createElement('TD');

    var imgE = this.Document.createElement('IMG');

    this.__Checkbox = this.Document.createElement('INPUT');
    this.__Checkbox.type = 'checkbox';
    this.__Checkbox.checked = this.Checked;
    this.__Checkbox._Node = this;
    this.__Checkbox.onclick = function () { this._Node.Checked = this.checked; };
    this.__Checkbox.onkeypress = function () { this._Node.Checked = this.checked; };

    this.__CheckBoxContainer.appendChild(this.__Checkbox);
    this.__CheckBoxContainer.style.display = (this.Checkbox ? '' : 'none');

    imgE.style.width = 16;
    imgE.style.height = 16;
    imgE.src = 'images/treeviewtreePlus.gif';

    ct.cellPadding = 0;
    ct.cellSpacing = 0;
    ct.border = 0;

    ct.appendChild(ttb);
    ttb.appendChild(ttr1);
    ttb.appendChild(ttr2);

    ttr1.appendChild(ttd1);
    ttr1.appendChild(this.__CheckBoxContainer);
    ttr1.appendChild(ttd2);

    ttr2.appendChild(ttd3);
    ttd3.colSpan = 3;
    ttd3.style.paddingLeft = 8;

    ttd1.appendChild(imgE);
    ttd1.style.width = 16;

    ttd2.noWrap = true;

    var nodetbl = this.Document.createElement('TABLE');
    var nodetb = this.Document.createElement('TBODY');
    var nodetr = this.Document.createElement('TR');
    var nodeImg = this.Document.createElement('IMG');
    var nodeImgContainer = this.Document.createElement('TD');
    var nodeText = this.Document.createElement('TD');

    //nodetbl.style.width = '100%';

    nodetbl.appendChild(nodetb);
    nodetb.appendChild(nodetr);
    nodetr.appendChild(nodeImgContainer);
    nodetr.appendChild(nodeText);

    ttd2.appendChild(nodetbl);
    nodeImgContainer.appendChild(nodeImg);

    nodetbl.cellPadding = 0;
    nodetbl.cellSpacing = 0;
    nodetbl.border = 0;

    nodeImg.border = 0;
    nodeImg.src = this.Icon || 'images/spacer.gif';
    nodeImg.alt = this.ToolTip || '';

    nodeImgContainer.className = 'nodeL';

    nodeText.noWrap = true;
    //nodeText.innerHTML = this.Name;
    nodeText.className = 'nodeR';

    nodetbl.Node = this;
    ttd2.Node = this;

    ttd2.unselectable = "on";
    ttd2.onselectstart = function () { return false };
    ttd2.style.userSelect = ttd2.style.MozUserSelect = "none";


    ttd2.onmouseover = function (e) { this.Node.Treeview.HoveringNode = this.Node; if (this.Node != this.Node.Treeview.SelectedNode) { nodeImgContainer.className = 'nodeL_h'; nodeText.className = 'nodeR_h'; } };
    ttd2.onmouseout = function (e) { this.Node.Treeview.HoveringNode = null; if (this.Node != this.Node.Treeview.SelectedNode) { nodeImgContainer.className = 'nodeL'; nodeText.className = 'nodeR'; } };
    ttd2.onclick = function (e) { this.Node.Treeview.SelectNode(this.Node); };
    ttd2.ondblclick = function (e) {
        if (this.Node.Treeview.OnNodeDoubleClick != null)
            this.Node.Treeview.OnNodeDoubleClick(this.Node);

        ttd1.onclick();
    };

    ttd2.onmouseup = function (e) {
        this.Node.MouseDown = false;
        if (this.Node.Treeview.AllowDrag && this.Node.Treeview.WindowManager) {
            this.Node.Treeview.WindowManager.DragDropManager.AbortDrag();
        }
    };

    ttd2.onmousedown = function (e) {
        this.Node.MouseDown = true;
    };

    ttd2.onmousemove = function (e) {
        if (this.Node.Treeview.AllowDrag && this.Node.Treeview.WindowManager && this.Node.MouseDown == true) {
            this.Node.MouseDown = false;
            this.Node.Treeview.WindowManager.DragDropManager.BeginDrag(this.Node, this.Node.Icon, this.Node.Name + ' ' + this.Node.Treeview.WindowManager.Core.Mouse.X);
        }
    };

    //node Text new container
    var nodeTextContainer = this.Document.createElement('SPAN');
    var nodeCountContainer = this.Document.createElement('SPAN');

    nodeText.appendChild(nodeTextContainer);
    nodeText.appendChild(nodeCountContainer);
    nodeTextContainer.innerHTML = this.Name;

    nodeTextContainer.title = this.ToolTip || '';

    nodeCountContainer.style.display = 'none';

    this.__NodeText = nodeTextContainer; // nodeText;
    this.__NodeTextContainer = nodeText;
    this.__NodeImage = nodeImg;
    this.__NodeImageFrame = nodeImgContainer;
    this.__NodeCountContainer = nodeCountContainer;

    this.SetCountText(this.CountText);

    ttd2.style.width = '100%';
    ttd3.style.width = '100%';

    ttr2.style.display = 'none';

    this.NodesContainer = ttd3;
    this.Expander = ttd1;
    this.Expander.Image = imgE;
    this.__Node = ct;

    this.Expander.Image.style.visibility = (this.AjaxChildCount > 0 ? 'visible' : 'hidden');

    if (this.ParentTreeviewNode) {
        this.ParentTreeviewNode.Expander.Image.style.visibility = 'visible';
        this.ParentTreeviewNode.NodesContainer.appendChild(ct);

    }
    else
        this.Treeview.Container.appendChild(ct);

    ct.Node = this;
    ttd1.onclick = function () { var n = this.parentNode.parentNode.parentNode.Node; if (n.Expanded) n.Collapse(n); else n.Expand(); };

    ttd1.className = 'nodeExpander';
    ttd2.Node = this;

    ttd2.oncontextmenu = function (e) {
        var target = e ? e.target : event.srcElement;
        //if(target == this)
        //{
        if (this.Node.ContextMenu) {
            if (this.Node.Treeview.SelectedNode != this.Node.Treeview.HoveringNode && this.Node.Treeview.HoveringNode != null)
                this.Node.Treeview.HoveringNode.Select();

            if (this.Node.Treeview.OnBeforeContextMenu)
                this.Node.Treeview.OnBeforeContextMenu(this.Node.Treeview.SelectedNode);

            this.Node.ContextMenu.Show(this.Node.ContextMenu.Core.Mouse.X, this.Node.ContextMenu.Core.Mouse.Y + this.Node.ContextMenu.WindowManager.Desktop.Control.offsetTop);
        }
        else if (this.Node.Treeview.ContextMenu) {
            if (this.Node.Treeview.SelectedNode != this.Node.Treeview.HoveringNode && this.Node.Treeview.HoveringNode != null)
                this.Node.Treeview.HoveringNode.Select();

            if (this.Node.Treeview.OnBeforeContextMenu)
                this.Node.Treeview.OnBeforeContextMenu(this.Node.Treeview.SelectedNode);

            this.Node.Treeview.ContextMenu.Show(this.Node.Treeview.ContextMenu.Core.Mouse.X, this.Node.Treeview.ContextMenu.Core.Mouse.Y + this.Node.Treeview.ContextMenu.WindowManager.Desktop.Control.offsetTop);
        }
        //}
        return false;
    };

};

IDC_TreeviewNode.prototype.DragDrop_ItemDrop = function(DroppedItem, DroppedHere)
{
    if(DroppedHere.DropType.indexOf(DroppedItem.DragType)>-1 && DroppedHere.Node.Treeview.OnNodeDrop)
    {
        DroppedHere.Node.Treeview.OnNodeDrop(DroppedItem, DroppedHere);
    }
};

IDC_TreeviewNode.prototype.DeSelect = function()
{      
    this.__NodeTextContainer.className='nodeR';
    this.__NodeImageFrame.className='nodeL';
    this.Treeview.SelectedNode = null;
    
    if(this.Treeview.Navigator)
        this.Treeview.Navigator.BuildButtons(this.GetPath());
        
    if(this.Treeview.OnNodeDeSelect)
    {
        this.Treeview.OnNodeDeSelect(this);
    }
    
    if(this.OnDeSelect)
    {
        this.OnDeSelect(this);
    }
    
};

IDC_TreeviewNode.prototype.ShowCounter = function()
{
    this.SetCountVisible(true);
};
IDC_TreeviewNode.prototype.HideCounter = function()
{
    this.SetCountVisible(false);
};
IDC_TreeviewNode.prototype.SetCountVisible = function(visible)
{
    this.__NodeCountContainer.style.display = visible ? '' : 'none';
};

IDC_TreeviewNode.prototype.SetCountText = function(text)
{
    this.CountText = text || '';
    
    if(this.ShowCountTextAutomatic)
        this.SetCountVisible(this.CountText!='');
    
    this.__NodeCountContainer.innerHTML = '&nbsp;(' + text + ')';
};

IDC_TreeviewNode.prototype.Select = function()
{
    if(this.Treeview.SelectedNode)
    {
        this.Treeview.SelectedNode.__NodeTextContainer.className='nodeR';
        this.Treeview.SelectedNode.__NodeImageFrame.className='nodeL';
    }
 
    if(this.Treeview.PerformSelectionBeforeSelectEvent)
    {
        this.__NodeTextContainer.className='nodeR_s';
        this.__NodeImageFrame.className='nodeL_s';
        this.Treeview.SelectedNode = this;
    }
 
    
    if(this.Treeview.Navigator)
        this.Treeview.Navigator.BuildButtons(this.GetPath());
        
    if(this.Treeview.OnNodeSelect)
    {
        this.Treeview.OnNodeSelect(this);
    }
    
    if(this.OnSelect)
    {
        this.OnSelect(this);
    }
    
    
    if(!this.Treeview.PerformSelectionBeforeSelectEvent)
    {
        this.__NodeTextContainer.className='nodeR_s';
        this.__NodeImageFrame.className='nodeL_s';
        this.Treeview.SelectedNode = this;
    }

};

IDC_TreeviewNode.prototype.ReadURLNodes = function (Depths) {
    var AjaxObject = new this.Treeview.Core.Ajax();
    AjaxObject.requestFile = this.ReadNodesFromURL;

    if (this.ReadNodesFromURLPostedArguments)
        AjaxObject.encVar('args', this.ReadNodesFromURLPostedArguments);


    var me = this;
    AjaxObject.OnCompletion = function () { me.__ReadURLNodes(AjaxObject, Depths); };
    AjaxObject.OnError = function () { me.__TreeviewAjaxFailure(AjaxObject); };
    AjaxObject.OnFail = function () { me.__TreeviewAjaxFailure(AjaxObject); };
    AjaxObject.RunAJAX();
};

IDC_TreeviewNode.prototype.__ReadURLNodes = function(AjaxObject, Depths)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
        
    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    
    this.__ToNodes(tmp, Depths);
    
    tmp = null;
    
    if(this.Treeview.Onload)
    {
        this.Treeview.Onload(this);
    }
    
};

IDC_TreeviewNode.prototype.ShowExpander = function()
{
    this.Expander.Image.style.visibility='';
};

IDC_TreeviewNode.prototype.HideExpander = function()
{
    this.Expander.Image.style.visibility='hidden';
};

IDC_TreeviewNode.prototype.__ToNodes = function(what, Depths, selectThisGuid)
{
    this.RemoveChildren();

    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        
        if(src.tagName)
        {


            var newNode = new IDC_TreeviewNode(this.Treeview, this, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'), src.getAttribute('tooltip'));
             
            newNode.ReadNodesFromURL = src.getAttribute('url');
            newNode.ReadNodesFromURLPostedArguments = src.getAttribute('args');
            
            //Drag drop 
            newNode.Dragable = src.getAttribute('drag') == '1';
            newNode.DragType = src.getAttribute('dragtype');
            newNode.Dropable = src.getAttribute('drop') == '1';
            newNode.DropType = src.getAttribute('droptype');
            newNode.Uniqueidentifier = src.getAttribute('uid');
            newNode.AjaxChildCount = src.getAttribute('childcount');
            
            newNode.Checkbox=src.getAttribute('checkbox') == '1';
            newNode.Checked=src.getAttribute('checked') == '1';
    
            newNode.CountText=src.getAttribute('counttext') || '';
            
            newNode = this.AddNodeObject(newNode);
            
            if(newNode.Uniqueidentifier == selectThisGuid)
            {
                this.Treeview.SelectNode(newNode);
            }
            if(src.getAttribute('selected') == '1')
                newNode.Select();
            
            //alt text?
            if(!this.isIE)
            {
                if(!src.firstChild.tagName)
                    newNode.SetName(this.isIE ? src.firstChild.innerText : src.firstChild.textContent);
            }
            else
            {
                var txtIndex = src.innerHTML.indexOf('<');
                if(txtIndex>-1)
                {
                    newNode.SetName(src.innerText.substring(0,txtIndex));
                }
            }
            
            
            
            if(!isNaN(Depths) && Depths != null)
            {
                newNode.__ToNodes( src, Depths-1, selectThisGuid);
                
                if(Depths>0)
                {
                    newNode.Expand(Depths-1, selectThisGuid);
                }
            }
            else
            {
                newNode.__ToNodes( src,null, selectThisGuid);
            }
        }
        
        
    }
};

IDC_TreeviewNode.prototype.__AppendToNodes = function(what)
{
    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        if(src.tagName)
        {
            var newNode = new IDC_TreeviewNode(this.Treeview, this, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'));
             
            newNode.ReadNodesFromURL = src.getAttribute('url');
            newNode.ReadNodesFromURLPostedArguments = src.getAttribute('args');
            
            //Drag drop 
            newNode.Dragable = src.getAttribute('drag') == '1';
            newNode.DragType = src.getAttribute('dragtype');
            newNode.Dropable = src.getAttribute('drop') == '1';
            newNode.DropType = src.getAttribute('droptype');
            newNode.Uniqueidentifier = src.getAttribute('uid');
            newNode.AjaxChildCount = src.getAttribute('childcount');
            
            newNode.Checkbox=src.getAttribute('checkbox') == '1';
            newNode.Checked=src.getAttribute('checked') == '1';
            
            newNode.CountText=src.getAttribute('counttext') || '';
            
            newNode = this.AddNodeObject(newNode);
            
            newNode.__AppendToNodes(newNode, src);
            
            if(src.getAttribute('selected') == '1')
                newNode.Select();
            
        }
        src=null;
    }
};

IDC_TreeviewNode.prototype.ExpandParents = function()
{
    var pt = this.ParentTreeviewNode;
    while(pt)
    {
        pt.Expanded=true;
        pt.Expander.Image.src = 'images/treeviewtreeMinus.gif';
        pt.Container.parentNode.style.display='';
        
        pt = pt.ParentTreeviewNode;
    }
};

IDC_TreeviewNode.prototype.Expand = function(Depths)
{
    this.Expanded=true;
    this.Expander.Image.src = 'images/treeviewtreeMinus.gif';
    this.NodesContainer.parentNode.style.display='';

    if(this.ReadNodesFromURL != null)
    {
        this.ReadURLNodes(Depths);
    }
};

IDC_TreeviewNode.prototype.Collapse = function()
{
    this.Expanded=false;
    this.Expander.Image.src = 'images/treeviewtreePlus.gif';
    this.NodesContainer.parentNode.style.display='none';
};

IDC_TreeviewNode.prototype.RemoveChildren = function()
{
    this.Nodes = new Array();
    while(this.NodesContainer.hasChildNodes())
    {
        this.NodesContainer.removeChild(this.Container.lastChild); 
    }
    
    if(this.AjaxChildCount==0)
        this.Expander.Image.style.visibility = 'hidden';
};
IDC_TreeviewNode.prototype.Remove = function()
{   
    var tn = this.ParentTreeviewNode;

    var n = this.__Node.parentNode.removeChild(this.__Node);
    
    if(this.Treeview.SelectedNode = this)
        this.Treeview.SelectedNode=null;
    
    if(tn)
    {
        for(var i=tn.Nodes.length;i>=0;i--)
        {
            if(tn.Nodes[i]==this)
            {
                tn.Nodes.splice(i,1);
            }
        }

        if (tn.Nodes.length == 0 && tn.AjaxChildCount == 0)
        {
            tn.Expander.Image.style.visibility = 'hidden';
        }
    }
    else
    {
        for(var i=this.Treeview.Nodes.length;i>=0;i--)
        {
            if(this.Treeview.Nodes[i]==this)
            {
                this.Treeview.Nodes.splice(i,1);
            }
        }
    }
    
        
    if(this.Treeview && tn)
        this.Treeview.SelectNode(tn);
        
    n = null;
    
};

IDC_TreeviewNode.prototype.AddNode = function (name, icon)
{
    var nd = new IDC_TreeviewNode(this.Treeview, null, name, icon);
    nd.ParentTreeviewNode = this;
    var i = this.Nodes.push(nd);
    var n = this.Nodes[i-1];
    n.Create();
    
    return n;
};

IDC_TreeviewNode.prototype.AddNodeObject = function (node)
{
    node.ParentTreeviewNode = this;
    var i = this.Nodes.push(node);
    var n = this.Nodes[i-1];
    n.Create();
    
    this.Expander.Image.style.visibility='';
    
    return n;
};

IDC_TreeviewNode.prototype.MoveToIndex = function (node, index)
{
    var pn = node.__Node.parentNode;
    var n = pn.removeChild(node.__Node);
    if(index<pn.childNodes.length)
        pn.insertBefore(n, pn.childNodes[index]);
    else
        pn.appendChild(n);
};

IDC_TreeviewNode.prototype.GetPath = function ()
{
    var nodes = new Array();
    var node = this;
    while(node)
    {
        nodes.push(node);
        node = node.ParentTreeviewNode;
    }
    return nodes.reverse();
};

IDC_TreeviewNode.prototype.GetPathAsString = function (separator)
{
    if(!separator) separator='/';
    var nodes = new Array();
    var node = this;
    while(node)
    {
        nodes.push(node.Name);
        node = node.ParentTreeviewNode;
    }
    nodes = nodes.reverse();
    
    var ns = '';
    
    for(var i=0;i<nodes.length;i++)
        ns+=separator + nodes[i];
    
    return ns + separator;
};

IDC_TreeviewNode.prototype.__TreeviewAjaxFailure = function(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
    
    var emsg = result.substring(0,result.indexOf('</title>'));
    emsg = emsg.substring(result.indexOf('<title>')+7);

    if(this.OnError)
    {
        this.OnError(emsg);
    }
};

IDC_Treeview.prototype.__TreeviewAjaxFailure = function(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
    
    var emsg = result.substring(0,result.indexOf('</title>'));
    emsg = emsg.substring(result.indexOf('<title>')+7);
    
    if(this.OnError)
    {
        this.OnError(emsg);
    }
};
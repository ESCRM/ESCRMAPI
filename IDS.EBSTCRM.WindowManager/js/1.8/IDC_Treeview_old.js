var Treeview = function (OwnerDocument, OwnerContainer)
{
    this.Core = new IDC_Core(OwnerDocument, OwnerContainer);
    this.isIE = document.all?true:false;
    
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.Navigator=null;
    
    this.SelectedNode = null;
    
    this.OnNodeSelect = null;
    
    this.Onload = null;
    
    this.OnNodeDrop = null;
    
    this.Nodes = new Array();
};
Treeview.prototype.Clear = function()
{
    this.Nodes = new Array();
    while(this.Container.hasChildNodes())
    {
        this.Container.removeChild(this.Container.lastChild); 
    }
};
Treeview.prototype.RemoveChildren = function()
{
    this.Nodes = new Array();
    while(this.Container.hasChildNodes())
    {
        this.Container.removeChild(this.Container.lastChild); 
    }
};
Treeview.prototype.GetAllCheckedNodes = function()
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

Treeview.prototype.GetNodeByUniqueidentifier = function(id)
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

Treeview.prototype.MoveNodeTo = function(srcNode, destNode)
{
    var d = destNode;
    while(d!=null)
    {
        d=d.ParentTreeviewNode;
        if(d==srcNode) return;
    }
    
    var n = srcNode;
    n.__Node.parentNode.removeChild(n.__Node);
                        t.SecondaryProjectTypeId = TypeCast.ToInt(Request.Form["txtSID"]);
                    t.SecondaryProjectTypeSerialNo = TypeCast.ToInt(Request.Form["txtSNo"]);

    var tn = srcNode.ParentTreeviewNode;
        
    if(tn)
    {
        
        if(tn.AjaxChildCount>0)
        {
            tn.AjaxChildCount--;
        }
        
        for(var i=tn.Nodes.length;i>=0;i--)
        {
            if(tn.Nodes[i]==srcNode)
            {
                tn.Nodes.splice(i,1);
            }
        }
         
        if(tn.Nodes.length==0 && tn.AjaxChildCount==0)
        {
            tn.Expander.Image.style.visibility = 'hidden';
        }
    }
    
    n.ParentTreeviewNode = destNode;
    n.Parent = destNode.Container;
    
    destNode.AddNodeObject(n);
    n.Select(n);
};

Treeview.prototype.ReadURLNodes = function(Url, Args, Depths)
{
//    if(window.__WINDOW)
//    {
//        window.__WINDOW.LockLayout();
//    }
    
    var AjaxObject = new this.Core.Ajax();
    AjaxObject.requestFile = Url;

    if(Args)
        AjaxObject.encVar('args',Args);

    var me = this;

    AjaxObject.OnCompletion = function(){ me.__ReadURLNodes(me, AjaxObject, Depths); }; 
    AjaxObject.OnError = function(){ __TreeviewAjaxFailure(AjaxObject);};
    AjaxObject.OnFail = function(){  __TreeviewAjaxFailure(AjaxObject);};
    AjaxObject.RunAJAX();
};
Treeview.prototype.__ReadURLNodes = function(sender, AjaxObject, Depths)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
    
    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    
    sender.__ToNodes(sender, tmp, Depths);
    
    tmp = null;
    
//    if(window.__WINDOW)
//    {
//        window.__WINDOW.UnLockLayout();
//    }
    
    if(this.Onload)
    {
        this.Onload(sender);
    }
};

Treeview.prototype.__ToNodes = function(sender, what, Depths, selectThisGuid)
{
    sender.RemoveChildren();

    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        if(src.tagName)
        {
            var newNode = new TreeviewNode(this.Container, this.Document, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'));
             
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
            
            newNode = sender.AddNodeObject(newNode);
            
            if(newNode.Uniqueidentifier == selectThisGuid && sender != null)
            {
                if(sender.Treeview)
                    sender.Treeview.SelectNode(newNode);
            }
            
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
                newNode.__ToNodes(newNode, src, Depths-1, selectThisGuid);
                
                if(Depths>0)
                {
                    newNode.Expand(newNode, Depths-1, selectThisGuid);
                }
            }
            else
            {
                newNode.__ToNodes(newNode, src,null, selectThisGuid);
            }
        }
        src=null;
    }
};


Treeview.prototype.SelectNode = function(node)
{
    if(this.SelectedNode)
        this.DeSelectNode(this.SelectedNode);
    
    this.SelectedNode = node;
    node.Select(node);
    
    if(this.Navigator)
        this.Navigator.BuildButtons(node.GetPath(node));
        
    if(this.OnNodeSelect)
    {
        this.OnNodeSelect(node);
    }
    
    if(node.OnSelect)
    {
        node.OnSelect(node);
    }
};

Treeview.prototype.DeSelectNode = function(node)
{
    if(node)
    {
        node.Deselect(node);
    }
};

Treeview.prototype.SetSize = function (width, height)
{
    if(width<0) width=0;
    if(height<0) height=0;
    this.Container.style.width = width;
    this.Container.style.height = height;
};

Treeview.prototype.AddNode = function (name, icon)
{
    var i = this.Nodes.push(new TreeviewNode(this.Container, this.Document, name, icon));
    var n = this.Nodes[i-1];
    n.Treeview = this;
    n.Create();
    
    return n;
};

Treeview.prototype.AddNodeObject = function (node)
{
    node.ParentTreeviewNode = null;
    var i = this.Nodes.push(node);
    var n = this.Nodes[i-1];
    n.Treeview = this;
    n.Create();
    
    return n;
};

var TreeviewNode = function (ParentObject, ParentDocument, name, icon)
{
    this.Parent = ParentObject;
    this.Document = ParentDocument;
    this.Name = name;
    this.Icon = icon;
    this.SelectedIcon = icon;
    this.Nodes = new Array();
    this.Container = null;
    this.Expander = null;
    this.Expanded=false;
    this.ReadNodesFromURL = null;
    this.ReadNodesFromURLPostedArguments = null;
    this.__Node = null;
    this.__NodeText = null;
    this.__NodeImage = null;
    this.__NodeImageFrame = null;
    
    this.Dragable=false;
    this.Dropable=false;
    this.DragType=null;
    this.DropType=null;
    
    this.Checkbox=false;
    this.Checked=false;
    
    this.Uniqueidentifier=null;
    this.AjaxChildCount = 0;
    
    this.ParentTreeviewNode = null;
    this.Treeview = null;
    this.isIE = document.all?true:false;
    this.OnSelect = null;
};
TreeviewNode.prototype._GetAllCheckedNodes = function(ns)
{
    for(var i=0;i<this.Nodes.length;i++)
    {
        if(this.Nodes[i].Checked && this.Nodes[i].Checkbox)
            ns.push(this.Nodes[i]);
        
        if(this.Nodes[i].Nodes.length>0)
            this.Nodes[i]._GetAllCheckedNodes(ns);
    }
};
TreeviewNode.prototype.GetNodeByUniqueidentifier = function(id)
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

TreeviewNode.prototype.SetName = function(name)
{
    this.Name=name;
    if(this.__NodeText)
        this.__NodeText.innerHTML = name;
};

TreeviewNode.prototype.SetIcon = function(icon)
{
    this.Icon=icon;
    if(this.__NodeImage)
        this.__NodeImage.src = icon;
};

TreeviewNode.prototype.SetCheckbox = function(value)
{
    this.Checkbox = value;
    this.__CheckBoxContainer.style.display = (this.Checkbox ? '' : 'none');
};

TreeviewNode.prototype.SetCheckboxChecked = function(value)
{
    this.Checked = value;
    this.__Checkbox.checked = value;
};

TreeviewNode.prototype.Create = function()
{
    var ct = this.Document.createElement('TABLE');
    var ttb = this.Document.createElement('TBODY');
    
    var ttr1 = this.Document.createElement('TR');
    var ttr2 = this.Document.createElement('TR');
    
    var ttd1 = this.Document.createElement('TD');
    var ttd2 = this.Document.createElement('TD');
    var ttd3 = this.Document.createElement('TD');
    
    this.__CheckBoxContainer = this.Document.createElement('TD');
    
    var imgE = this.Document.createElement('IMG');
    
    this.__Checkbox = this.Document.createElement('INPUT');
    this.__Checkbox.type='checkbox';
    this.__Checkbox.checked = this.Checked;
    this.__Checkbox._Node = this;
    this.__Checkbox.onclick=function() { this._Node.Checked = this.checked; };
    this.__Checkbox.onkeypress=function() { this._Node.Checked = this.checked; };
    
    this.__CheckBoxContainer.appendChild(this.__Checkbox);
    this.__CheckBoxContainer.style.display = (this.Checkbox ? '' : 'none');
   
    imgE.style.width=16;
    imgE.style.height=16;
    imgE.src='images/treeviewtreePlus.gif';
    
    ct.cellPadding=0;
    ct.cellSpacing=0;
    ct.border=0;
    
    ct.appendChild(ttb);
    ttb.appendChild(ttr1);
    ttb.appendChild(ttr2);
    
    ttr1.appendChild(ttd1);
    ttr1.appendChild(this.__CheckBoxContainer);
    ttr1.appendChild(ttd2);
    
    ttr2.appendChild(ttd3);
    ttd3.colSpan=3;
    ttd3.style.paddingLeft=8;
    
    ttd1.appendChild(imgE);
    ttd1.style.width=16;
    
    ttd2.noWrap=true;
       
    var nodetbl = this.Document.createElement('TABLE');
    var nodetb = this.Document.createElement('TBODY');
    var nodetr = this.Document.createElement('TR');
    var nodeImg = this.Document.createElement('IMG');
    var nodeImgContainer = this.Document.createElement('TD');
    var nodeText = this.Document.createElement('TD');
    
    nodetbl.appendChild(nodetb);
    nodetb.appendChild(nodetr);
    nodetr.appendChild(nodeImgContainer);
    nodetr.appendChild(nodeText);
    
    ttd2.appendChild(nodetbl);
    nodeImgContainer.appendChild(nodeImg);
    
    nodetbl.cellPadding=0;
    nodetbl.cellSpacing=0;
    nodetbl.border=0;
    
    nodeImg.border=0;
    nodeImg.src=this.Icon || 'images/spacer.gif';
    nodeImg.alt='';
    
    nodeImgContainer.className='nodeL';
    
    nodeText.noWrap=true;
    nodeText.innerHTML = this.Name;
    nodeText.className='nodeR';
    
    nodetbl.Node = this;
    
    nodetbl.onmouseover = function(e) { if(this.Node != this.Node.Treeview.SelectedNode) { nodeImgContainer.className='nodeL_h'; nodeText.className='nodeR_h'; }};
    nodetbl.onmouseout = function(e) { if(this.Node != this.Node.Treeview.SelectedNode) { nodeImgContainer.className='nodeL'; nodeText.className='nodeR'; }};
    nodetbl.onclick = function(e) { this.Node.Treeview.SelectNode(this.Node); };
    nodetbl.ondblclick = function(e) { ttd1.onclick(); };
    
    this.__NodeText = nodeText;
    this.__NodeImage = nodeImg;
    this.__NodeImageFrame = nodeImgContainer;
    
    ttd2.style.width='100%';
    ttd3.style.width='100%';
    
    ttr2.style.display='none';
    
    
    
    this.Container = ttd3;
    this.Expander = ttd1;
    this.Expander.Image = imgE;
    this.__Node = ct;
    
    this.Expander.Image.style.visibility = (this.AjaxChildCount>0 ? 'visible' : 'hidden');
    
    if(this.ParentTreeviewNode)
        this.ParentTreeviewNode.Expander.Image.style.visibility = 'visible';
    
    this.Parent.appendChild(ct);
    ct.Node = this;
    ttd1.onclick = function() { var n = this.parentNode.parentNode.parentNode.Node; if(n.Expanded) n.Collapse(n); else n.Expand(n); };
    ttd1.className='nodeExpander';
    
    //Drag drop
//    if(dd)
//    {
//        if(this.Dragable) dd.AttachObject(nodetbl, this.Name, this.DragType);
//        if(this.Dropable) dd.AddDropZone(nodetbl, this.DropType, this.DragDrop_ItemDrop);
//    }
};

TreeviewNode.prototype.DragDrop_ItemDrop = function(DroppedItem, DroppedHere)
{
    if(DroppedHere.DropType.indexOf(DroppedItem.DragType)>-1 && DroppedHere.Node.Treeview.OnNodeDrop)
    {
        DroppedHere.Node.Treeview.OnNodeDrop(DroppedItem, DroppedHere);
    }
};
TreeviewNode.prototype.Select = function(sender)
{
    sender.__NodeText.className='nodeR_s';
    sender.__NodeImageFrame.className='nodeL_s';
};

TreeviewNode.prototype.Deselect = function(sender)
{
    sender.__NodeText.className='nodeR';
    sender.__NodeImageFrame.className='nodeL';
};

TreeviewNode.prototype.ReadURLNodes = function(sender, Depths)
{
    if(window.__WINDOW)
    {
        window.__WINDOW.LockLayout();
    }
    
    var AjaxObject = new this.Treeview.Core.Ajax();
    AjaxObject.requestFile = sender.ReadNodesFromURL;

    if(sender.ReadNodesFromURLPostedArguments)
        AjaxObject.encVar('args',sender.ReadNodesFromURLPostedArguments);

    AjaxObject.OnCompletion = function(){ sender.__ReadURLNodes(sender, AjaxObject, Depths); }; 
    AjaxObject.OnError = function(){ __TreeviewAjaxFailure(AjaxObject);};
    AjaxObject.OnFail = function(){  __TreeviewAjaxFailure(AjaxObject);};
    AjaxObject.RunAJAX();
};

TreeviewNode.prototype.__ReadURLNodes = function(sender, AjaxObject, Depths)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    AjaxObject=null;
    
    var tmp = this.Document.createElement('DIV');
    tmp.innerHTML = result;
    
    sender.__ToNodes(sender, tmp, Depths);
    
    tmp = null;
    
    if(window.__WINDOW)
    {
        window.__WINDOW.UnLockLayout();
    }
    
    if(this.Treeview.Onload)
    {
        this.Treeview.Onload(sender);
    }
    
};

TreeviewNode.prototype.ShowExpander = function()
{
    this.Expander.Image.style.visibility='';
};
TreeviewNode.prototype.HideExpander = function()
{
    this.Expander.Image.style.visibility='hidden';
};

TreeviewNode.prototype.__ToNodes = function(sender, what, Depths, selectThisGuid)
{
    sender.RemoveChildren();

    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        if(src.tagName)
        {
            var newNode = new TreeviewNode(this.Container, this.Document, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'));
             
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
    
            
            newNode = sender.AddNodeObject(newNode);
            
            if(newNode.Uniqueidentifier == selectThisGuid)
            {
                sender.Treeview.SelectNode(newNode);
            }
            
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
                newNode.__ToNodes(newNode, src, Depths-1, selectThisGuid);
                
                if(Depths>0)
                {
                    newNode.Expand(newNode, Depths-1, selectThisGuid);
                }
            }
            else
            {
                newNode.__ToNodes(newNode, src,null, selectThisGuid);
            }
        }
        src=null;
    }
};

TreeviewNode.prototype.__AppendToNodes = function(sender, what)
{
    while(what.hasChildNodes())
    {
        var src = what.firstChild;
        src = what.removeChild(src);
        if(src.tagName)
        {
            var newNode = new TreeviewNode(this.Container, this.Document, this.isIE ? src.innerText : src.textContent, src.getAttribute('icon'));
             
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
            
            newNode = sender.AddNodeObject(newNode);
            
            newNode.__AppendToNodes(newNode, src);
            
        }
        src=null;
    }
};

TreeviewNode.prototype.ExpandParents = function()
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

TreeviewNode.prototype.Expand = function(sender, Depths)
{
    sender.Expanded=true;
    sender.Expander.Image.src = 'images/treeviewtreeMinus.gif';
    sender.Container.parentNode.style.display='';

    if(sender.ReadNodesFromURL != null)
    {
        sender.ReadURLNodes(sender, Depths);
    }
};
TreeviewNode.prototype.Collapse = function(sender)
{
    sender.Expanded=false;
    sender.Expander.Image.src = 'images/treeviewtreePlus.gif';
    sender.Container.parentNode.style.display='none';
};
TreeviewNode.prototype.RemoveChildren = function()
{
    this.Nodes = new Array();
    while(this.Container.hasChildNodes())
    {
        this.Container.removeChild(this.Container.lastChild); 
    }
    
    if(this.AjaxChildCount==0)
        this.Expander.Image.style.visibility = 'hidden';
};
TreeviewNode.prototype.Remove = function(treeview)
{   
    var tn = this.ParentTreeviewNode;

    var n = this.__Node.parentNode.removeChild(this.__Node);
    
    if(treeview.SelectedNode = this)
        treeview.SelectedNode=null;
    
    if(tn)
    {
        for(var i=tn.Nodes.length;i>=0;i--)
        {
            if(tn.Nodes[i]==this)
            {
                tn.Nodes.splice(i,1);
            }
        }
        
        if(tn.Nodes.length==0 && this.AjaxChildCount==0)
        {
            tn.Expander.Image.style.visibility = 'hidden';
        }
    }
    else
    {
        for(var i=treeview.Nodes.length;i>=0;i--)
        {
            if(treeview.Nodes[i]==this)
            {
                treeview.Nodes.splice(i,1);
            }
        }
    }
    
        
    if(treeview && tn)
        treeview.SelectNode(tn);
        
    n = null;
    
};
TreeviewNode.prototype.AddNode = function (name, icon)
{
    var nd = new TreeviewNode(this.Container, this.Document, name, icon);
    nd.ParentTreeviewNode = this;
    var i = this.Nodes.push(nd);
    var n = this.Nodes[i-1];
    n.Treeview = this.Treeview;
    n.Create();
    
    return n;
};

TreeviewNode.prototype.AddNodeObject = function (node)
{
    node.ParentTreeviewNode = this;
    var i = this.Nodes.push(node);
    var n = this.Nodes[i-1];
    n.Treeview = this.Treeview;
    n.Create();
    
    this.Expander.Image.style.visibility='';
    
    return n;
};

TreeviewNode.prototype.MoveToIndex = function (node, index)
{
    var pn = node.__Node.parentNode;
    var n = pn.removeChild(node.__Node);
    if(index<pn.childNodes.length)
        pn.insertBefore(n, pn.childNodes[index]);
    else
        pn.appendChild(n);
};

TreeviewNode.prototype.GetPath = function (node)
{
    var nodes = new Array();
    
    while(node)
    {
        nodes.push(node);
        node = node.ParentTreeviewNode;
    }
    
    nodes.reverse();
    
    return nodes;
};

function __TreeviewAjaxFailure(AjaxObject)
{
    var result=AjaxObject.response;
    AjaxObject.reset();
    AjaxObject.dispose();
    AjaxObject=null;
    
    var emsg = result.substring(0,result.indexOf('</title>'));
    emsg = emsg.substring(result.indexOf('<title>')+7);
    
    wm.Alert(window.__WINDOW, emsg, 'Error', null, null, null);
    
    if(window.__WINDOW)
    {
        window.__WINDOW.UnLockLayout();
    }
};
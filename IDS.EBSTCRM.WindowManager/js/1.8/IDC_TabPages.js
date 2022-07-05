var TabPageContainer = function (OwnerDocument, OwnerContainer)
{
    this.Orientation = 1;
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    this.TabPages = new Array();
    
    this.SelectedTabPage = null;
    this.CloseAbleTabs = false;
    
};

TabPageContainer.prototype.AddTabPage = function(name, onSelect, onDeSelect, icon, onClose)
{
    var t = new TabPage(this, this.Document, this.Container, name, onSelect, onDeSelect, this.Orientation, icon, onClose);
    
    t.TabPageContainer = this;
    
    this.TabPages.push(t);
    return t;
};

TabPageContainer.prototype.RemoveTabPage = function(tab)
{
    var abort = false;
    if(tab.OnClose) abort = tab.OnClose(tab);
    if(abort) return;
    
    for(var i = this.TabPages.length-1;i>=0;i--)
    {
        if(this.TabPages[i] == tab)
        {
            this.TabPages.splice(i,1);
        }
    }
    
    if(this.SelectedTabPage == tab)
    {
        if(this.TabPages.length>0)
        {
            this.TabPages[this.TabPages.length-1].Select();
        }
        else
        {
            this.SelectedTabPage = null;
        }
    }
       
    this.Container.removeChild(tab.TabControl);
};

var TabPage = function(tabs, OwnerDocument, OwnerContainer, name, onSelect, onDeSelect, orientation, icon, onClose)
{
    this.TabPages = tabs;
    this.Icon = icon;
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.Name = name;
    this.OnSelect = onSelect;
    this.OnDeSelect = onDeSelect;
    this.Orientation = orientation;
    this.OnClose = onClose;
    this.IsRemoved = false;
    
    this.TabPageContainer = null;
    
    this.TabControl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.__TabLeft = this.Document.createElement('TD');
    this.__TabIcon = this.Document.createElement('TD');
    this.__TabText = this.Document.createElement('TD');
    this.__TabRight = this.Document.createElement('TD');
    this.__TabClose = this.Document.createElement('TD');
    
    this.TabControl.cellPadding=0;
    this.TabControl.cellSpacing=0;
    this.TabControl.border=0;
    this.TabControl.style.display='inline';
    
    this.TabControl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.__TabLeft);
    tr.appendChild(this.__TabIcon);
    tr.appendChild(this.__TabText);
    tr.appendChild(this.__TabClose);
    tr.appendChild(this.__TabRight);
    
    if(!this.TabPages.CloseAbleTabs)
    {
        this.__TabClose.style.display='none';
    }
    
    this.__TabClose.TabPage = this;
    this.__TabClose.Icon = this.Document.createElement('IMG');
    this.__TabClose.Icon.src='images/spacer.gif';
    this.__TabClose.Icon.className='tabPageCloseButton';
    this.__TabClose.appendChild(this.__TabClose.Icon);
    this.__TabClose.onmouseover = function() { this.Icon.className='tabPageCloseButtonHover'; };
    this.__TabClose.onmouseout = function() { this.Icon.className='tabPageCloseButton'; };
    
    this.__TabClose.onclick = function()
        {
            this.TabPage.Remove();
        }
    
    if(this.Icon)
    {
        this.__TabIcon.innerHTML='<img src="' + this.Icon + '" alt="" border="0">';
        this.__TabIcon.style.display='';
    }
    else
        this.__TabIcon.style.display='none';
        
    if(this.Orientation == 0)
    {
        this.__TabLeft.className='tabPageLeft';
        this.__TabIcon.className='tabPageText';
        this.__TabText.className='tabPageText';
        this.__TabClose.className='tabPageText';
        this.__TabRight.className='tabPageRight';
    }
    else if(this.Orientation == 1)
    {
        this.__TabLeft.className='tabPageTLeft';
        this.__TabIcon.className='tabPageTText';
        this.__TabText.className='tabPageTText';
        this.__TabClose.className='tabPageTText';
        this.__TabRight.className='tabPageTRight';
    }
    
    this.__TabText.innerHTML = this.Name;
    
    this.Container.appendChild(this.TabControl);
    this.TabControl.TabPage = this;
    
    this.TabControl.onclick = function() { if(!this.TabPage.IsRemoved) this.TabPage.Select(); };
};

TabPage.prototype.Remove = function(forceRemove)
{
    var abort = false;
    if(this.OnClose && !forceRemove) abort = this.OnClose(this);
    if(abort && !forceRemove) return;
 
    this.IsRemoved = true;
    
    for(var i = this.TabPages.TabPages.length-1;i>=0;i--)
    {
        if(this.TabPages.TabPages[i] == this)
        {
            this.TabPages.TabPages.splice(i,1);
        }
    }
    
    this.Container.removeChild(this.TabControl);
    
    if(this.TabPages.SelectedTabPage == this)
    {
        if(this.TabPages.TabPages.length>0)
        {
            this.TabPages.SelectedTabPage = null;
            this.TabPages.TabPages[this.TabPages.TabPages.length-1].Select();
        }
        else
        {
            this.TabPages.SelectedTabPage = null;
        }
    }
};

TabPage.prototype.SetName = function(Name)
{
    this.__TabText.innerHTML=Name;
    this.Name = Name;
};

TabPage.prototype.Select = function (NoInvoke) {

    if (this.TabPageContainer.SelectedTabPage == this) return;

    var oldPage = this.TabPageContainer.SelectedTabPage;
    this.TabPageContainer.SelectedTabPage = this;

    if (this.OnSelect && NoInvoke != true)
        this.OnSelect(this);

    if (oldPage) {
        if (oldPage.Orientation == 0) {
            oldPage.__TabLeft.className = 'tabPageLeft';
            oldPage.__TabIcon.className = 'tabPageText';
            oldPage.__TabText.className = 'tabPageText';
            oldPage.__TabClose.className = 'tabPageText';
            oldPage.__TabRight.className = 'tabPageRight';
        }
        else if (oldPage.Orientation == 1) {
            oldPage.__TabLeft.className = 'tabPageTLeft';
            oldPage.__TabIcon.className = 'tabPageTText';
            oldPage.__TabText.className = 'tabPageTText';
            oldPage.__TabClose.className = 'tabPageTText';
            oldPage.__TabRight.className = 'tabPageTRight';
        }

        if (oldPage.OnDeSelect && NoInvoke != true) {
            oldPage.OnDeSelect(oldPage);
        }
    }

    if (this.Orientation == 0) {
        this.__TabLeft.className = 'tabPageLeft_s';
        this.__TabIcon.className = 'tabPageText_s';
        this.__TabText.className = 'tabPageText_s';
        this.__TabClose.className = 'tabPageText_s';
        this.__TabRight.className = 'tabPageRight_s';
    }
    else if (this.Orientation == 1) {
        this.__TabLeft.className = 'tabPageTLeft_s';
        this.__TabIcon.className = 'tabPageTText_s';
        this.__TabText.className = 'tabPageTText_s';
        this.__TabClose.className = 'tabPageTText_s';
        this.__TabRight.className = 'tabPageTRight_s';
    }
};
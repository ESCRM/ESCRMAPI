var Toolbar = function (OwnerDocument, OwnerContainer) {
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.isIE = document.all ? true : false;

    //this.CurrentURL = (window.location.origin + window.location.pathname).substring(0, this.Document.location.toString().lastIndexOf('/')) + '/';
    this.CurrentURL = window.location.origin + window.location.pathname;
    this.CurrentURL = this.CurrentURL.substring(0, this.CurrentURL.lastIndexOf('/') + 1);

    this.Container.className = 'toolbarBack';
    this.ButtonContainer = this.Container;

    this.Control = null;

    if (this.Container.tagName != 'TR') {
        var tbl = this.Document.createElement('TABLE');
        var tbody = this.Document.createElement('TBODY');
        var tr = this.Document.createElement('TR');
        var td = this.Document.createElement('TD');

        tbl.cellPadding = 0;
        tbl.cellSpacing = 0;
        tbl.border = 0;

        //tbl.style.width='100%';
        tbl.style.height = '100%';

        this.Container.appendChild(tbl);
        tbl.appendChild(tbody);
        tbody.appendChild(tr);
        this.ButtonContainer = tr;

        this.Control = tbl;
    }

    this.Buttons = new Array();

    this.CSSLeft = 'toolbarButtonLeft';
    this.CSSRight = 'toolbarButtonRight';
    this.CSS = 'toolbarButton';

    this.CSSLeft_Hover = 'toolbarButtonLeft_h';
    this.CSSRight_Hover = 'toolbarButtonRight_h';

    this.CSSLeft_Selected = 'toolbarButtonLeft_s';
    this.CSSRight_Selected = 'toolbarButtonRight_s';

    this.CSSLabel = 'toolbarLabel';

    this.HideDisabledButtons = false;
    this.AnimateButtons = false;

    if (window.__WINDOW) {
        if (window.__WINDOW.UI) {
            window.__WINDOW.UI.BindToolbar(this);
        }
    }
};
Toolbar.prototype.SetButtonEnabledState = function(index, enabled)
{
    this.Buttons[i].SetEnabledState(enabled);
};

Toolbar.prototype.Disable = function()
{
    this.SetEnabledState(false);
};
Toolbar.prototype.Enable = function()
{
    this.SetEnabledState(true);
};
Toolbar.prototype.SetEnabledState = function(enabled)
{
    for(var i=0;i<this.Buttons.length;i++)
    {
        this.Buttons[i].SetEnabledState(enabled);
    }
};

Toolbar.prototype.AddNewButtonRow = function()
{
    var newTr = this.Document.createElement('TR');
    this.ButtonContainer.parentNode.appendChild(newTr);
    this.ButtonContainer = newTr;
};

Toolbar.prototype.SetBackgroundImage = function(url)
{
    this.Container.style.backgroundImage = 'url(' + url + ')';
};
Toolbar.prototype.SetBackgroundClass = function(cls)
{
    this.Container.className=cls;
};

Toolbar.prototype.AddButton = function(Name, Icon, OnClick, Width, IsSelectable)
{
    var b = new ToolbarButton(this.Document, this.ButtonContainer, Name, Icon, OnClick, this.CSS, this.CSSLeft, this.CSSRight, this.CSSLeft_Hover, this.CSSRight_Hover, this.CSSLeft_Selected, this.CSSRight_Selected, Width, IsSelectable, this);
    b.Create();
    this.Buttons.push(b);
    
    return b;
};

Toolbar.prototype.AddDropdownButton = function(Name, Icon, OnClick, Width, IsSelectable)
{
    var b = new ToolbarButton(this.Document, this.ButtonContainer, Name, Icon, OnClick, this.CSS, 'toolbarDropdownButtonLeft', 'toolbarDropdownButtonRight', 'toolbarDropdownButtonLeft_h', 'toolbarDropdownButtonRight_h', 'toolbarDropdownButtonLeft_s', 'toolbarDropdownButtonRight_s', Width, IsSelectable, this);
    b.Create();
    this.Buttons.push(b);
    
    return b;
};

Toolbar.prototype.AddTinyButton = function(Name, Icon, OnClick, IsSelectable)
{
    var b = new ToolbarTinyButton(this.Document, this.ButtonContainer, Name, Icon, OnClick, IsSelectable, this);
    b.Create();
    this.Buttons.push(b);
    
    return b;
};

Toolbar.prototype.AddTinyDropdownButton = function(Name, Icon, OnClick, IsSelectable)
{
    var b = new ToolbarTinyDropdownButton(this.Document, this.ButtonContainer, Name, Icon, OnClick, IsSelectable, this);
    b.Create();
    this.Buttons.push(b);
    
    return b;
};

Toolbar.prototype.AddControl = function(control)
{
    var s = new ToolbarControl(this.Document, this.ButtonContainer, control, this);
    this.Buttons.push(s);
    
    return s;
};

var ToolbarControl = function(OwnerDocument, OwnerContainer, control, toolbar)
{
    this.Enabled=true;
    var tbl = OwnerDocument.createElement('TABLE');
    var tb = OwnerDocument.createElement('TBODY');
    var tr = OwnerDocument.createElement('TR');
    var td1 = OwnerDocument.createElement('TD');
    
    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    td1.appendChild(control);
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    tbl.style.margin=0;
    
    td1.style.padding=0;
    td1.style.display='';
    
    var tdP = OwnerDocument.createElement('TD');
    tdP.appendChild(tbl);
    OwnerContainer.appendChild(tdP);
    this.Control = tbl;
    
    this.Toolbar = toolbar;
    
    return this;
};
ToolbarControl.prototype.SetEnabledState = function(enabed) {};

Toolbar.prototype.AddLabel = function(text)
{
    var s = new ToolbarLabel(this.Document, this.ButtonContainer, text, this);
    s.className = this.CSSLabel;
    this.Buttons.push(s);
    
    return s;
};
Toolbar.prototype.AddIcon = function(icon, tooltip)
{
    var s = new ToolbarIcon(this.Document, this.ButtonContainer, icon, tooltip, this);
    this.Buttons.push(s);
    
    return s;
};

var ToolbarLabel = function(OwnerDocument, OwnerContainer, text, toolbar)
{
    this.Toolbar = toolbar;
    this.Enabled = true;
    var tbl = OwnerDocument.createElement('TABLE');
    var tb = OwnerDocument.createElement('TBODY');
    var tr = OwnerDocument.createElement('TR');
    var td1 = OwnerDocument.createElement('TD');
    
    this.Label = td1;
    
    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    td1.innerHTML = text;
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    
    tbl.className=this.CSSLabel;
    
    var tdP = OwnerDocument.createElement('TD');
    tdP.appendChild(tbl);
    OwnerContainer.appendChild(tdP);
    this.Control = tbl;
    return this;
};
ToolbarLabel.prototype.SetEnabledState = function(enabled)
{
    this.Enabled = enabled;
    
    var opa = enabled ? 100 : 60;
    
    if(this.Toolbar.isIE)
        this.Control.style.filter = 'alpha(opacity=' + opa + ')';
    else
        this.Control.style.opacity = opa/100;
};

var ToolbarIcon = function(OwnerDocument, OwnerContainer, icon, tooltip, toolbar)
{
    this.Toolbar = toolbar;
    this.Enabled = true;
    var tbl = OwnerDocument.createElement('TABLE');
    var tb = OwnerDocument.createElement('TBODY');
    var tr = OwnerDocument.createElement('TR');
    var td1 = OwnerDocument.createElement('TD');
    var img = OwnerDocument.createElement('IMG');
    
    if(tooltip)
    {
        img.alt = tooltip;
        td1.title = tooltip;
    }
    
    this.Icon = img;
    
    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    td1.appendChild(img);
    img.src = this.Toolbar.CurrentURL + icon;
    
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    
    tbl.className='toolbarIcon';
    
    var tdP = OwnerDocument.createElement('TD');
    tdP.appendChild(tbl);
    OwnerContainer.appendChild(tdP);
    
    this.Control = tbl;
    this.Image = img;
    return this;
};
ToolbarIcon.prototype.SetEnabledState = function(enabled)
{
    this.Enabled = enabled;
    
    var opa = enabled ? 100 : 60;
    
    if(this.Toolbar.isIE)
    {
        this.Control.style.filter = opa != 100 ? 'alpha(opacity=' + opa + ')' : null;
        this.Image.style.filter = opa != 100 ? 'progid:DXImageTransform.Microsoft.Emboss()' : null;
        this.Image.style.height=opa != 100 ? 14 : 16;
        this.Image.style.width=opa != 100 ? 14 : 16;
    }
    else
        this.Control.style.opacity = opa/100;
};


Toolbar.prototype.AddSeparator = function()
{
    return this.AddSeperator();
};

Toolbar.prototype.AddSeperator = function()
{
    var s = new ToolbarSeperator(this.Document, this.ButtonContainer, this);
    this.Buttons.push(s);
    
    return s;
};

Toolbar.prototype.AddBlankSeperator = function () {
    var s = new ToolbarBlankSeperator(this.Document, this.ButtonContainer, this);
    this.Buttons.push(s);

    return s;
};

var ToolbarSeperator = function (OwnerDocument, OwnerContainer, toolbar) {
    this.TypeName = 'SEP';
    this.Toolbar = toolbar;
    var div = OwnerDocument.createElement('IMG');
    div.src = 'images/spacer.gif';
    div.className = 'toolbarSeparator';

    var tdP = OwnerDocument.createElement('TD');
    tdP.appendChild(div);
    OwnerContainer.appendChild(tdP);
    this.Control = tdP;

    return this;
};

var ToolbarBlankSeperator = function (OwnerDocument, OwnerContainer, toolbar) {
    this.TypeName = 'SEP';
    this.Toolbar = toolbar;
    var div = OwnerDocument.createElement('IMG');
    div.src = 'images/spacer.gif';
    div.className = 'toolbarBlankSeparator';

    var tdP = OwnerDocument.createElement('TD');
    tdP.appendChild(div);
    OwnerContainer.appendChild(tdP);
    this.Control = tdP;

    return this;
};

ToolbarSeperator.prototype.SetEnabledState = function()
{
};

ToolbarSeperator.prototype.Hide = function()
{
    this.SetVisibility(false);
};
ToolbarSeperator.prototype.Show = function()
{
    this.SetVisibility(true);
};
ToolbarSeperator.prototype.SetVisibility = function(visible)
{
    this.Control.style.display= visible ? '' : 'none';
};

var ToolbarTinyButton = function(OwnerDocument, OwnerContainer, name, icon, onClick, isSelectable, toolbar) {
    this.TypeName = 'BTN';
    this.Toolbar = toolbar;
    this.Enabled = true;
    this.IsSelectable = isSelectable;
    this.IsSelected = false;
    this.Name = name;
    this.Icon = icon;
    this.OnClick = onClick; 
    
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    this.Control = null;
    this.Image = null;    
};
ToolbarTinyButton.prototype.Disable = function()
{
    this.SetEnabledState(false);
};
ToolbarTinyButton.prototype.Enable = function()
{
    this.SetEnabledState(true);
};
ToolbarTinyButton.prototype.SetEnabledState = function(enabled)
{
    if(!enabled) this.Control.onmouseout();
    
    this.Enabled = enabled;
    
    var opa = enabled ? 100 : 60;
    
    if(this.Toolbar.isIE)
    {
        
        this.Control.style.filter = opa != 100 ? 'alpha(opacity=' + opa + ')' : null;
        this.Image.style.filter = opa != 100 ? 'progid:DXImageTransform.Microsoft.Emboss()' : null;
        this.Image.style.height=opa != 100 ? 14 : 16;
        this.Image.style.width=opa != 100 ? 14 : 16;
    }
    else
        this.Control.style.opacity = opa/100;
};

ToolbarTinyButton.prototype.SetSelectedState = function(selected)
{
    if(this.IsSelectable) 
    {
        this.IsSelected = selected; 
        if(this.IsSelected) 
        {
            this.ButtonControl.className='toolbarButtonIco_s';
        }
        else
        {
            this.ButtonControl.className='toolbarButtonIco';
        }
    }
};

ToolbarTinyButton.prototype.Create = function()
{
    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.ButtonControl = this.Document.createElement('TD');
    var img = this.Document.createElement('IMG');
    
   
    this.Control.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.ButtonControl);
    this.ButtonControl.appendChild(img);
    
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    
    this.Control.className='toolbarButton';
    this.ButtonControl.className='toolbarButtonIco';
    
    this.Control.Button = this;
    this.Control.title = this.Name;
        
    this.Control.onmouseover=function() { if(!this.Button.Enabled) return; this.Button.ButtonControl.className='toolbarButtonIco_h';  };
    this.Control.onmouseout=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { this.Button.ButtonControl.className='toolbarButtonIco_s'; } else { this.Button.ButtonControl.className='toolbarButtonIco'; } };
    this.Control.onmousedown=function() { if(!this.Button.Enabled) return; this.Button.ButtonControl.className='toolbarButtonIco_s'; };
    this.Control.onmouseup=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { this.Button.ButtonControl.className='toolbarButtonIco_s'; } else { this.Button.ButtonControl.className='toolbarButtonIco_h'; } };
    
    this.Image = img;
    
    img.alt=this.Name;
    img.border=0;
    img.src=this.Toolbar.CurrentURL + this.Icon;
    
    var tdP = this.Document.createElement('TD');
    tdP.appendChild(this.Control);
    this.Container.appendChild(tdP);
    
    this.Control.onclick = function() {
                                 if(!this.Button.Enabled) return;
                                 
                                if(this.Button.IsSelectable) 
                                {
                                    this.Button.IsSelected = !this.Button.IsSelected; 
                                    if(this.Button.IsSelected) 
                                    {
                                        this.Button.ButtonControl.className='toolbarButtonIco_s'; 
                                    }
                                    else
                                    {
                                        this.Button.ButtonControl.className='toolbarButtonIco'; 
                                    }
                                }
                                
                                if(this.Button.OnClick) this.Button.OnClick(this.Button);
                             }        
    return this.Control;
};
ToolbarTinyButton.prototype.Hide = function()
{
    this.SetVisibility(false);
};
ToolbarTinyButton.prototype.Show = function()
{
    this.SetVisibility(true);
};
ToolbarTinyButton.prototype.SetVisibility = function(visible)
{
    this.Control.style.display= visible ? '' : 'none';
};



ToolbarTinyButton.prototype.SetTextAndIcon = function (text, icon) {
    if (icon) this.Image.src = this.Toolbar.CurrentURL + icon;
    if (text) {
        this.Image.alt = text;
        this.Name = text;
        this.Control.title = this.Name;
        if (this.Text != null)
            this.Text.innerHTML = text;
    }
};

var ToolbarButton = function(OwnerDocument, OwnerContainer, name, icon, onClick, css, cssLeft, cssRight, cssLeft_hover, cssRight_hover, cssLeft_selected, cssRight_selected, width, isSelectable, toolbar) {
    this.TypeName = 'BTN';
    this.Enabled = true;
    this.IsSelectable = isSelectable;
    this.IsSelected = false;
    this.CSSLeft = cssLeft;
    this.CSSRight = cssRight;
    this.CSS = css;
    this.Width = width;
    
    this.CSSLeft_Hover = cssLeft_hover;
    this.CSSRight_Hover = cssRight_hover;
    
    this.CSSLeft_Selected = cssLeft_selected;
    this.CSSRight_Selected = cssRight_selected;
    
    this.Name = name;
    this.Icon = icon;
    this.OnClick = onClick;
    
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    this.Control = null;
    this.Image = null;
    this.Text = null;
    
    this.Toolbar = toolbar;
};

ToolbarButton.prototype.Create = function()
{
    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var td2 = this.Document.createElement('TD');
    var img = this.Document.createElement('IMG');
    
    this.ButtonLeft = td1;
    this.ButtonRight = td2;
    
    this.Control.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(td1);
    tr.appendChild(td2);
    td1.appendChild(img);
    
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    td2.noWrap=true;
    
    this.Control.className=this.CSS;
    td1.className=this.CSSLeft;
    td2.className=this.CSSRight;
    
    this.Control.Button = this;
    
    this.Control.onmouseover=function() { if(!this.Button.Enabled) return; td1.className=this.Button.CSSLeft_Hover; td2.className=this.Button.CSSRight_Hover; };
    this.Control.onmouseout=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { td1.className=this.Button.CSSLeft_Selected; td2.className=this.Button.CSSRight_Selected; } else { td1.className=this.Button.CSSLeft; td2.className=this.Button.CSSRight; } };
    this.Control.onmousedown=function() { if(!this.Button.Enabled) return; td1.className=this.Button.CSSLeft_Selected; td2.className=this.Button.CSSRight_Selected; };
    this.Control.onmouseup=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { td1.className=this.Button.CSSLeft_Selected; td2.className=this.Button.CSSRight_Selected; } else { td1.className=this.Button.CSSLeft_Hover; td2.className=this.Button.CSSRight_Hover; } };
    td2.innerHTML=this.Name;
    
    this.Image = img;
    this.Text = td2;
    
    img.alt='';
    img.border=0;
    img.src=this.Toolbar.CurrentURL + this.Icon;
    
    var tdP = this.Document.createElement('TD');
    tdP.appendChild(this.Control);
    this.Container.appendChild(tdP);
    
    this.Control.onclick = function() {
                                 if(!this.Button.Enabled) return;
                                 
                                if(this.Button.IsSelectable) 
                                {
                                    this.Button.IsSelected = !this.Button.IsSelected; 
                                    if(this.Button.IsSelected) 
                                    {
                                        td1.className=this.Button.CSSLeft_Selected; 
                                        td2.className=this.Button.CSSRight_Selected; 
                                    }
                                    else
                                    {
                                        td1.className=this.Button.CSSLeft; 
                                        td2.className=this.Button.CSSRight; 
                                    }
                                }
                                
                                if(this.Button.OnClick) this.Button.OnClick(this.Button);
                             }    
    if(this.Width)
    {
        td2.style.width = this.Width -  td1.offsetWidth;
    }
    
    return this.Control;
};

ToolbarButton.prototype.SetSelectedState = function(selected)
{
    if(this.IsSelectable) 
    {
        this.IsSelected = selected; 
        if(this.IsSelected) 
        {
            this.ButtonLeft.className=this.CSSLeft_Selected; 
            this.ButtonRight.className=this.CSSRight_Selected; 
        }
        else
        {
            this.ButtonLeft.className=this.CSSLeft; 
            this.ButtonRight.className=this.CSSRight; 
        }
    }
};

ToolbarButton.prototype.Disable = function()
{
    this.SetEnabledState(false);
};
ToolbarButton.prototype.Enable = function()
{
    this.SetEnabledState(true);
};
ToolbarButton.prototype.SetEnabledState = function(enabled)
{
    if(!enabled) this.Control.onmouseout();
    this.Enabled = enabled;
    
    var opa = enabled ? 100 : 60;
    
    if(this.Toolbar.isIE)
    {
        this.Control.style.filter = opa != 100 ? 'alpha(opacity=' + opa + ')' : null;
        this.Image.style.filter = opa != 100 ? 'progid:DXImageTransform.Microsoft.Emboss()' : null;
        this.Image.style.height=opa != 100 ? 14 : 16;
        this.Image.style.width=opa != 100 ? 14 : 16;
    }
    else
        this.Control.style.opacity = opa/100;
        
    if(this.Toolbar.HideDisabledButtons)
    {
        if(enabled)
            this.Show();
        else
            this.Hide();
    }
};

ToolbarButton.prototype.Hide = function()
{
    this.SetVisibility(false);
};
ToolbarButton.prototype.Show = function()
{
    this.SetVisibility(true);
};
ToolbarButton.prototype.SetVisibility = function(visible)
{
    this.Control.style.display= visible ? '' : 'none';
};

ToolbarButton.prototype.SetTextAndIcon = function(text, icon)
{
    if(icon) this.Image.src = this.Toolbar.CurrentURL + icon;
    if(text)
    {
        this.Image.alt = text;
        this.Name = text;
        this.Control.title = this.Name;
        this.Text.innerHTML = text;
    }
};

//Tiny drop down
var ToolbarTinyDropdownButton = function(OwnerDocument, OwnerContainer, name, icon, onClick, isSelectable, toolbar) {
    this.TypeName = 'DPBTN';
    this.Toolbar = toolbar;
    this.Enabled = true;
    this.IsSelectable = isSelectable;
    this.IsSelected = false;
    this.Name = name;
    this.Icon = icon;
    this.OnClick = onClick;
    
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    this.Control = null;
    this.Image = null;    
};
ToolbarTinyDropdownButton.prototype.Disable = function()
{
    this.SetEnabledState(false);
};
ToolbarTinyDropdownButton.prototype.Enable = function()
{
    this.SetEnabledState(true);
};
ToolbarTinyDropdownButton.prototype.SetEnabledState = function(enabled)
{
    if(!enabled) this.Control.onmouseout();
    
    this.Enabled = enabled;
    
    var opa = enabled ? 100 : 60;
    
    if(this.Toolbar.isIE)
    {
        
        this.Control.style.filter = opa != 100 ? 'alpha(opacity=' + opa + ')' : null;
        this.Image.style.filter = opa != 100 ? 'progid:DXImageTransform.Microsoft.Emboss()' : null;
        this.Image.style.height=opa != 100 ? 14 : 16;
        this.Image.style.width=opa != 100 ? 14 : 16;
    }
    else
        this.Control.style.opacity = opa/100;
};

ToolbarTinyDropdownButton.prototype.SetSelectedState = function(selected)
{
    if(this.IsSelectable) 
    {
        this.IsSelected = selected; 
        if(this.IsSelected) 
        {
            this.ButtonControl.className='toolbarButtonIcoDropdown_s';
        }
        else
        {
            this.ButtonControl.className='toolbarButtonIcoDropdown';
        }
    }
};

ToolbarTinyDropdownButton.prototype.Create = function()
{
    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.ButtonControl = this.Document.createElement('TD');
    var img = this.Document.createElement('IMG');
    
   
    this.Control.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.ButtonControl);
    this.ButtonControl.appendChild(img);
    
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    
    this.Control.className='toolbarButton';
    this.ButtonControl.className='toolbarButtonIcoDropdown';
    
    this.Control.Button = this;
    this.Control.title = this.Name;
        
    this.Control.onmouseover=function() { if(!this.Button.Enabled) return; this.Button.ButtonControl.className='toolbarButtonIcoDropdown_h';  };
    this.Control.onmouseout=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { this.Button.ButtonControl.className='toolbarButtonIcoDropdown_s'; } else { this.Button.ButtonControl.className='toolbarButtonIcoDropdown'; } };
    this.Control.onmousedown=function() { if(!this.Button.Enabled) return; this.Button.ButtonControl.className='toolbarButtonIcoDropdown_s'; };
    this.Control.onmouseup=function() { if(!this.Button.Enabled) return; if(this.Button.IsSelectable && this.Button.IsSelected) { this.Button.ButtonControl.className='toolbarButtonIcoDropdown_s'; } else { this.Button.ButtonControl.className='toolbarButtonIcoDropdown_h'; } };
    
    this.Image = img;
    
    img.alt=this.Name;
    img.border=0;
    img.src=this.Toolbar.CurrentURL + this.Icon;
    
    var tdP = this.Document.createElement('TD');
    tdP.appendChild(this.Control);
    this.Container.appendChild(tdP);
    
    this.Control.onclick = function() {
                                 if(!this.Button.Enabled) return;
                                 
                                if(this.Button.IsSelectable) 
                                {
                                    this.Button.IsSelected = !this.Button.IsSelected; 
                                    if(this.Button.IsSelected) 
                                    {
                                        this.Button.ButtonControl.className='toolbarButtonIcoDropdown_s'; 
                                    }
                                    else
                                    {
                                        this.Button.ButtonControl.className='toolbarButtonIcoDropdown'; 
                                    }
                                }
                                
                                if(this.Button.OnClick) this.Button.OnClick(this.Button);
                             }        
    return this.Control;
};
ToolbarTinyDropdownButton.prototype.Hide = function()
{
    this.SetVisibility(false);
};
ToolbarTinyDropdownButton.prototype.Show = function()
{
    this.SetVisibility(true);
};
ToolbarTinyDropdownButton.prototype.SetVisibility = function(visible)
{
    this.Control.style.display= visible ? '' : 'none';
};



ToolbarTinyDropdownButton.prototype.SetTextAndIcon = function(text, icon)
{
    if(icon) this.Image.src = this.Toolbar.CurrentURL + icon;
    if(text)
    {
        this.Image.alt = text;
        this.Name = text;
        this.Control.title = this.Name;
        if(this.Text != null)
            this.Text.innerHTML = text;
    }
};
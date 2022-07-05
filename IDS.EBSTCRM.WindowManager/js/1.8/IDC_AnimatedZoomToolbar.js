var IDC_animatedZoomToolbar = function(Document, Container)
{
    this.Document = Document;
    this.isIE = document.all?true:false;
    this.Core = new IDC_Core(Document, Container);
    
    var center =Document.createElement('CENTER');
    var tbl = Document.createElement('TABLE');
    var tb = Document.createElement('TBODY');
    this.Container = Document.createElement('TR');
    
   
    var tdLeft = Document.createElement('TD');
    tdLeft.className='zoomToolbarBackLeft';
    tdLeft.style.width=4;
    this.Container.appendChild(tdLeft);
    
    var tdRight = Document.createElement('TD');
    tdRight.className='zoomToolbarBackRight';
    tdRight.style.width=4;
    this.Container.appendChild(tdRight);
    
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    
    tbl.appendChild(tb);
    tb.appendChild(this.Container);
    
    center.style.position='absolute';
    center.style.width='100%';
    center.appendChild(tbl);
    
    this.Control = center;
    this.Control.style.display='none';
    Container.appendChild(this.Control);
    
    this.Buttons = new Array();
};

IDC_animatedZoomToolbar.prototype.AddButton = function(name, iconSmall, iconLarge, onclick)
{
    var b = new IDC_animatedZoomToolbarButton(this, name, iconSmall, iconLarge, onclick);
    this.Buttons.push(b);
    this.Control.style.display='';
    return b;
};

IDC_animatedZoomToolbar.prototype.Clear = function()
{
    for(var i=this.Buttons.length-1;i>=0;i--)
    {
        this.Buttons[i].Remove();
    }
    this.Buttons = new Array();
    this.Control.style.display='none';
};

var IDC_animatedZoomToolbarButton = function(toolbar, name, iconSmall, iconLarge, onclick)
{
    this.Name = name;
    this.IconSmall = iconSmall;
    this.IconLarge = iconLarge;
    this.Document = toolbar.Document;
    this.Toolbar = toolbar;
    this.OnClick = onclick;
    
    this.PopupButtons = new Array();
    this.PopupButtonsContainer = this.Document.createElement('DIV');

    this.PopupButtonsContainer.className='zoomToolbarButtonPopupButtonsBack';
    this.PopupButtonsContainer.style.display='none';
    
    var td = this.Document.createElement('TD');
    var tbl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.ButtonLeft = this.Document.createElement('TD');
    this.ButtonRight = this.Document.createElement('TD');
    
    tbl.style.cursor='pointer';
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    
    td.vAlign='top';
    td.className='zoomToolbarBack';
    this.ButtonRight.className='zoomToolbarButtonBackUnZoomedRight';
    td.appendChild(tbl);
    tbl.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.ButtonLeft);
    tr.appendChild(this.ButtonRight);
    
    this.Toolbar.Control.parentNode.appendChild(this.PopupButtonsContainer);
    
    
    this.Toolbar.Container.insertBefore(td, this.Toolbar.Container.lastChild);
    
    this.Icon = this.Document.createElement('IMG');
    this.Icon.src=iconSmall;
    
    this.Label = this.Document.createElement('SPAN');
    this.Label.innerHTML = name;
    this.Label.style.fontSize = '12px';
    
    this.ButtonLeft.appendChild(this.Icon);
    this.ButtonRight.appendChild(this.Label);    
    
    this.ButtonLeft.style.paddingTop=4;
    this.ButtonRight.style.paddingTop=4;
    
    this.ButtonLeft.style.paddingBottom=4;
    this.ButtonRight.style.paddingBottom=4;
    
    this.ButtonLeft.style.paddingLeft=4;
    this.ButtonRight.style.paddingLeft=2;
    this.ButtonRight.style.paddingRight=4;
    
    this.ZoomPos = 0;
    this.ZoomReach=16;
    this.ZoomDirection=0;
    this.Animating=false;
    
    this.Control = td;
    this.Control.Button = this;
    
    this.PopupButtonsContainer.Button = this;
    
    this.HasFocus = false;
    
    this.Control.onmouseover = function()
            {
                this.Button.ZoomDirection=2;
                this.Button.ZoomReach=16;
                
                this.Button.Toolbar.Control.style.zIndex=250000;
                
                if(!this.Button.Animating)
                {
                    var me = this.Button;
                    setTimeout(function() {me.Animate(); },5);
                }
            };
            
    this.Control.onmouseout = function()
            {
                this.Button.ZoomReach=0;
                this.Button.ZoomDirection=-2;
                var me = this.Button;
                
                
                
                setTimeout(function() {
                                        if(!me.Animating && !me.HasFocus)
                                        {
                                            setTimeout(function() {me.Animate(); },5);
                                        }
                                      },15);

            };
    
    this.PopupButtonsContainer.onmouseover = function()
            {
                this.Button.HasFocus = true;
            };
            
    this.PopupButtonsContainer.onmouseout = function()
            {
                this.Button.HasFocus = false;
                var me = this.Button;
                setTimeout(function() {
                                        if(!me.Animating && !me.HasFocus)
                                        {
                                            setTimeout(function() {me.Animate(); },1);
                                        }
                                      },5);
            };
    
    this.Control.onclick = function()
            {
                if(this.Button.OnClick)
                {
                    this.Button.OnClick(this.Button);
                }
            };
    
    return this;
};

IDC_animatedZoomToolbarButton.prototype.Remove = function()
{
    this.Control.parentNode.removeChild(this.Control);
    this.PopupButtonsContainer.parentNode.removeChild(this.PopupButtonsContainer);
    
    for(var i = this.Toolbar.Buttons.length-1;i>=0;i--)
    {
        if(this.Toolbar.Buttons[i] == this)
        {
            this.Toolbar.Buttons.splice(i,1);
        }
    }
    this.Toolbar.Control.style.display=this.Toolbar.Buttons.length == 0 ? 'none' : '';
};

IDC_animatedZoomToolbarButton.prototype.Animate = function()
{
    if(this.ZoomReach != this.ZoomPos)
    {
        this.Animating=true;
        
        this.ZoomPos += this.ZoomDirection;

        this.Label.style.fontSize = parseInt(12 + (this.ZoomPos/2),10) + 'px';
        
        this.Icon.style.width = parseInt(16 + (this.ZoomPos),10) + 'px';
        this.Icon.style.height = this.Icon.style.width;
        
        this.Label.style.lineHeight=this.Icon.style.height;
        
        if(this.ZoomDirection > 0)
        {
            this.Icon.src = this.IconLarge;
            this.ButtonLeft.className='zoomToolbarButtonBackLeft';
            this.ButtonRight.className='zoomToolbarButtonBackRight';
        }
        else
        {
            this.Icon.src = this.IconSmall;
            this.ButtonLeft.className='';
            this.ButtonRight.className='zoomToolbarButtonBackUnZoomedRight';
            this.PopupButtonsContainer.style.display= 'none';
        }
        
        this.Toolbar.Control.style.zIndex=250000;        
        
        var me = this;
        setTimeout(function() {me.Animate(); },5);
        
    }
    else
    {
        if(this.ZoomDirection > 0)
        {
            this.PopupButtonsContainer.style.display=this.PopupButtons.length>0 ? '' : 'none';
            
            if(this.PopupButtons.length>0)
            {
                var pos = this.Toolbar.Core.DOM.GetObjectPosition(this.Control);
                this.PopupButtonsContainer.style.left =pos[0];
                this.PopupButtonsContainer.style.top =pos[1] + this.Control.offsetHeight-16;
                this.PopupButtonsContainer.style.width = this.Control.offsetWidth - (this.Toolbar.isIE ? 0 : 6);
                this.PopupButtonsContainer.style.zIndex=250000;
            }
            
            this.Icon.src = this.IconLarge;
            this.Toolbar.Control.style.zIndex=250000;
            
        }
        else
        {
            this.Icon.src = this.IconSmall;
            this.PopupButtonsContainer.style.display= 'none';
            
            this.Toolbar.Control.style.zIndex='';
            this.PopupButtonsContainer.style.zIndex='';
            
            for(var i=0;i<this.Toolbar.Buttons.length;i++)
            {
                if(this.Toolbar.Buttons[i].PopupButtonsContainer.style.display != 'none')
                {
                    var pos = this.Toolbar.Core.DOM.GetObjectPosition(this.Toolbar.Buttons[i].Control);
                    this.Toolbar.Buttons[i].PopupButtonsContainer.style.left = pos[0];
                }
            }
        }
        
        this.Animating=false;
    }
    
};

IDC_animatedZoomToolbarButton.prototype.AddSeperator = function()
{
    var b = new IDC_animatedZoomToolbarPopupSeperator(this);
    this.PopupButtons.push(b);
    return b;
};

IDC_animatedZoomToolbarButton.prototype.AddButton = function(name, icon, onclick)
{
    var b = new IDC_animatedZoomToolbarPopupButton(this, name, icon, onclick);
    this.PopupButtons.push(b);
    return b;
};

var IDC_animatedZoomToolbarPopupSeperator = function(button)
{
    this.ParentButton = button;
    this.Document = button.Document;
    this.Control = this.Document.createElement('DIV');
    
    this.Control.className='zoomToolbarButtonPopupSeparator';
    this.Control.innerHTML='<img style="width:1px;height:4px;" src="images/spacer.gif"><br>';
    this.ParentButton.PopupButtonsContainer.appendChild(this.Control);
    
    return this;
};

var IDC_animatedZoomToolbarPopupButton = function(button, name, icon, onclick)
{
    this.ParentButton = button;
    this.Name = name;
    this.Icon = icon;
    this.OnClick = onclick;
    
    this.Document = button.Document;
    
    this.PopupButtons = new Array();
    this.PopupButtonsContainer = this.Document.createElement('DIV');

    this.PopupButtonsContainer.className='zoomToolbarButtonPopupButtonsChildBack';
    this.PopupButtonsContainer.style.display='none';
    
    
    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr = this.Document.createElement('TR');
    this.ButtonLeft = this.Document.createElement('TD');
    this.ButtonRight = this.Document.createElement('TD');
    this.PopupParent = this.Document.createElement('TD');
    
    this.ButtonRight.noWrap=true;
    
    this.Control.appendChild(tb);
    tb.appendChild(tr);
    tr.appendChild(this.ButtonLeft);
    tr.appendChild(this.ButtonRight);
    tr.appendChild(this.PopupParent);
    
    this.PopupParent.vAlign='top';
    this.PopupParent.appendChild(this.PopupButtonsContainer);
    
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    this.Control.style.width='100%';
    
    this.IconControl = this.Document.createElement('IMG');
    this.Label = this.Document.createElement('SPAN');

    this.ButtonRight.style.width='100%';
    
    this.ButtonLeft.appendChild(this.IconControl);
    this.ButtonRight.appendChild(this.Label);
    
    this.IconControl.src = this.Icon || 'images/spacer.gif';
    this.Label.innerHTML = this.Name;
    this.Label.style.fontSize='12px';
    
    this.ButtonLeft.className='zoomToolbarButtonPopupButtonLeft';
    this.ButtonRight.className='zoomToolbarButtonPopupButtonRight';
    
    if(this.ParentButton.PopupButtons.length>0)
    {
        if(this.ParentButton.ButtonRight.className != 'zoomToolbarButtonBackUnZoomedRight' && this.ParentButton.ButtonRight.className!=null)
        this.ParentButton.ButtonRight.className='zoomToolbarButtonPopupButtonRightArrow';
    }
    
    this.Control.Button = this;
    
    this.Control.onmouseover = function() { 
                                                this.Button.ButtonLeft.className='zoomToolbarButtonPopupButtonLeftHover';
                                                
                                                if(this.Button.PopupButtons.length==0)
                                                    this.Button.ButtonRight.className='zoomToolbarButtonPopupButtonRightHover';
                                                else
                                                    this.Button.ButtonRight.className='zoomToolbarButtonPopupButtonRightArrowHover';
                                                
                                                if(this.Button.PopupButtons.length>0)
                                                {
                                                    this.Button.PopupButtonsContainer.style.display='';
                                                }
                                            };
    this.Control.onmouseout = function() { 
                                                this.Button.ButtonLeft.className='zoomToolbarButtonPopupButtonLeft';
                                                
                                                if(this.Button.PopupButtons.length==0)
                                                    this.Button.ButtonRight.className='zoomToolbarButtonPopupButtonRight';
                                                else
                                                    this.Button.ButtonRight.className='zoomToolbarButtonPopupButtonRightArrow';
                                                    
                                                this.Button.PopupButtonsContainer.style.display='none';
                                            };
    
    this.Control.onclick = function()
            {
                if(this.Button.OnClick)
                {
                    this.Button.OnClick(this.Button);
                    
                }
                
                if(this.Button.PopupButtons.length==0)
                {
                    this.Button.ParentButton.PopupButtonsContainer.style.display='none';
                    this.Button.ParentButton.HasFocus = false;
                    this.Button.ParentButton.Control.onmouseout();
                }
            };
            
    this.ParentButton.PopupButtonsContainer.appendChild(this.Control);
    
    return this;
};

IDC_animatedZoomToolbarPopupButton.prototype.AddButton = function(name, icon, onclick)
{
    var b = new IDC_animatedZoomToolbarPopupButton(this, name, icon, onclick);
    this.PopupButtons.push(b);
    return b;
};

IDC_animatedZoomToolbarPopupButton.prototype.AddSeperator = function()
{
    var b = new IDC_animatedZoomToolbarPopupSeperator(this);
    this.PopupButtons.push(b);
    return b;
};

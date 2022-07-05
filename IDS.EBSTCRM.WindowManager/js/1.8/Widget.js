var WidgetManager = function(Document, Container, IDCCore)
{
    this.Document = Document || document;
    this.Container = Document.createElement('DIV');
    this.Container.style.right=0;
    this.Container.style.top=14;
    this.Container.style.padding=4;
    this.Container.style.position='absolute';
    this.RootContainer = Container;
    this.Core = IDCCore || new IDC_Core(this.Document);
    this.RootContainer.appendChild(this.Container);
    this.Widgets = new Array();
    
    this.ControllerWindow = null;
    
    
    this.WidgetMovingArea = Document.createElement('DIV');
    this.WidgetMovingArea.WidgetManager = this;
    this.WidgetMovingArea.style.position='absolute';
    this.WidgetMovingArea.style.left=0;
    this.WidgetMovingArea.style.top=0;
    this.WidgetMovingArea.style.width='100%';
    this.WidgetMovingArea.style.height='100%';
    this.WidgetMovingArea.style.display='none';
    this.WidgetMovingArea.style.backgroundImage='url(' + core.WebsiteUrl + 'images/spacer.gif)';
    
    this.WidgetMovingArea.style.zIndex=250000;
    
    this.WidgetMovingArea.onmouseup=function()
                            {
                                this.style.display='none';
                                this.WidgetManager.SaveWidgets();
                            };
                            
    this.WidgetMovingArea.onmousemove=function()
                            {
                                
                                
                                if(this.WidgetManager.MovingWidget)
                                {
                                    if(this.WidgetManager.RootContainer.offsetWidth - this.WidgetManager.Core.Mouse.X > 120)
                                    {
                                    
                                        if(this.WidgetManager.MovingWidget.Control.parentNode==this.WidgetManager.RootContainer || Math.abs(this.WidgetManager.Core.Mouse.X - this.WidgetManager.MovingWidget.mouseX)>48 || Math.abs(this.WidgetManager.Core.Mouse.Y - this.WidgetManager.MovingWidget.mouseY)>48)
                                        {
                                            if(this.WidgetManager.MovingWidget.Control.parentNode!=this.WidgetManager.RootContainer)
                                            {
                                                this.WidgetManager.MovingWidget.Control.parentNode.removeChild(this.WidgetManager.MovingWidget.Control);
                                                this.WidgetManager.RootContainer.appendChild(this.WidgetManager.MovingWidget.Control);
                                                this.WidgetManager.MovingWidget.Control.style.position='absolute';
                                            }
                                            
                                            this.WidgetManager.MovingWidget.Control.style.left=this.WidgetManager.Core.Mouse.X + this.WidgetManager.MovingWidget.baseX;
                                            this.WidgetManager.MovingWidget.Control.style.top=this.WidgetManager.Core.Mouse.Y + this.WidgetManager.MovingWidget.baseY;
                                        }
                                    
                                    }
                                    else
                                    {
                                        if(this.WidgetManager.MovingWidget.Control.parentNode==this.WidgetManager.RootContainer)
                                        {
                                            this.WidgetManager.MovingWidget.Control.parentNode.removeChild(this.WidgetManager.MovingWidget.Control);
                                            this.WidgetManager.Container.appendChild(this.WidgetManager.MovingWidget.Control);
                                            this.WidgetManager.MovingWidget.Control.style.position='';
                                            
                                            var pos = this.WidgetManager.Core.DOM.GetObjectPosition(this.WidgetManager.MovingWidget.Control);
                                            
                                            //Reorganize widgets to and insert this at the bottom!
                                            for(var i=this.WidgetManager.Widgets.length-1;i>=0;i--)
                                            {
                                                if(this.WidgetManager.Widgets[i] == this.WidgetManager.MovingWidget)
                                                {
                                                    this.WidgetManager.Widgets.splice(i,1);
                                                    this.WidgetManager.Widgets.push(this.WidgetManager.MovingWidget);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                
                            };
                            
    this.RootContainer.appendChild(this.WidgetMovingArea);
    delete this.MovingWidge;
    this.MovingWidget=null;
};

WidgetManager.prototype.ShowMover = function()
{
    if(this.MovingWidget)
    {
        this.WidgetMovingArea.style.display='';
    }
};

var WidgetInfo = function(name, description, icon, filename)
{
    this.Name = name;
    this.Description = description;
    this.Icon = icon;
    this.Filename = filename;
};

WidgetManager.prototype.Configure = function()
{
//    this.ControllerWindow = wm.CreateWindow(0,0,390,430,'Gadgets', 'images/gadget.png', 'widgetBrowser.aspx', false, true, false, false, true, null, null, null, null); 
//    this.ControllerWindow.CenterScreen();
    
    this.ControllerWindow = windowManager.CreateWindow('widgetBrowser.aspx');
}

WidgetManager.prototype.AddWidget = function(WidgetName, updateWidthAjax)
{
    var args = WidgetName.split('@');
    
    var w = new Widget(this, this.Container, args[0], args[1], args[2]);
    this.Widgets.push(w);
    
    if(updateWidthAjax)
    {
        this.SaveWidgets();
    }
    
    return w;
};

WidgetManager.prototype.SaveWidgets = function()
{
    var AjaxObject = new this.Core.Ajax();
    AjaxObject.requestFile = 'ajax/widgetManagement.aspx';
    
    var wgds= '';
    var order = '';
    var coords = '';
    
    for(var i=0;i<this.Widgets.length;i++)
    {
        wgds += (wgds == '' ? '' : '\\') + this.Widgets[i].WidgetName;
        order +=(order=='' ? '' : ',') + i;
        coords += (coords=='' ? '' : ',') + (this.Widgets[i].Control.style.position=='absolute' ? parseInt(this.Widgets[i].Control.style.left,10) + '.' + parseInt(this.Widgets[i].Control.style.top,10) : 'NaN.NaN');
    }
    AjaxObject.encVar('add', 'true');
    AjaxObject.encVar('name', wgds);
    AjaxObject.encVar('index', order);
    AjaxObject.encVar('coords', coords);
    
    AjaxObject.OnCompletion = function(){ __WidgetManagerAjaxLoad(AjaxObject); }; 
    AjaxObject.OnError = function(){ __WidgetManagerAjaxLoad(AjaxObject);};
    AjaxObject.OnFail = function(){  __WidgetManagerAjaxLoad(AjaxObject);};
    AjaxObject.RunAJAX();
};

WidgetManager.prototype.Clear = function()
{
    for(var i=this.Widgets.length-1;i>=0;i--)
    {
        this.Widgets[i].Remove();
    }
    this.Widgets = new Array();
};

//** Fjern kun favorite widget, ESCRM-118/121
WidgetManager.prototype.ClearFavorite = function (favorite) {
    for (var i = this.Widgets.length - 1; i >= 0; i--) {
        if (this.Widgets[i].WidgetName == favorite) {
            this.Widgets[i].Remove();
        }
    }
    this.Widgets = new Array();
};

var Widget = function(manager, container, WidgetName, x, y)
{
    this.Manager = manager;
    this.WidgetName = WidgetName;
    this.Container = container;
    this.Control = document.createElement('DIV');
    this.IsMouseDown=false;
    
    this.CommandButtons = document.createElement('DIV');
    this.Control.appendChild(this.CommandButtons);
    this.Control.style.padding=4;
    
    this.CommandButtons.style.position='absolute';
    this.CommandButtons.style.right=0;
    this.CommandButtons.style.display='none';
    
    this.CommandButtons.Close = document.createElement('A');
    this.CommandButtons.Close.innerHTML='<img border="0" src="images/spacer.gif" style="width:15px;height:20px;"><br>';
    this.CommandButtons.Close.Widget = this;
    if(this.Manager.Core.isIE) this.CommandButtons.Close.href='#';
    this.CommandButtons.Close.title='Luk';
    this.CommandButtons.Close.alt='Luk';
    this.CommandButtons.Close.onclick=function() { this.Widget.Remove(true); };
    this.CommandButtons.Close.className='widgetClose';
    
    this.CommandButtons.appendChild(this.CommandButtons.Close);
    
    this.CommandButtons.Pick = document.createElement('A');
    this.CommandButtons.Pick.innerHTML='<img border="0" src="images/spacer.gif" style="width:15px;height:20px;"><br>';
    this.CommandButtons.Pick.Widget = this;
    if(this.Manager.Core.isIE) this.CommandButtons.Pick.href='#';
    this.CommandButtons.Pick.title='Vælg gadgets';
    this.CommandButtons.Pick.alt='Vælg gadgets';
    this.CommandButtons.Pick.onclick=function() { this.Widget.Manager.Configure(); };
    this.CommandButtons.Pick.className='widgetPick';
    
    this.CommandButtons.appendChild(this.CommandButtons.Pick);
    
    this.Control.Widget = this;
    
    this.CommandButtons.onmouseover = function() { this.MouseIsOver = true; };
    this.CommandButtons.onmouseout = function() { this.MouseIsOver = false; };
    
    this.WidgetFunx = 'tester';
    
    
    this.Control.onmouseover = function()
                    {
                        this.Widget.CommandButtons.style.display='';
                    }
                    
    this.Control.onmouseout = function()
                    {
                        this.Widget.CommandButtons.style.display='none';
                    }
    
    this.Control.onmousedown = function()
                    {
                        this.Widget.IsMouseDown=true;
                        
                        var pos = this.Widget.Manager.Core.DOM.GetObjectPosition(this);
                        
                        this.Widget.baseX = pos[0] - this.Widget.Manager.Core.Mouse.X;
                        this.Widget.baseY =  pos[1] - this.Widget.Manager.Core.Mouse.Y;
                        
                        this.Widget.mouseX = this.Widget.Manager.Core.Mouse.X;
                        this.Widget.mouseY =  this.Widget.Manager.Core.Mouse.Y;
                        
                        this.Widget.Manager.MovingWidget = this.Widget;
                        var me = this.Widget.Manager;
                        
                        setTimeout(function() { me.ShowMover(); },150);
                    }
                    
    this.Control.onmouseup = function()
                    {
                        this.Widget.Manager.MovingWidget = null;
                        this.Widget.IsMouseDown=false;
                    }
                    
    this.Control.onmousemove = function()
                    {
                    
                    }
    
    if(!isNaN(x) && !isNaN(y) && x!='' && y!='')
    {
        this.Control.style.position='absolute';
        this.Control.style.left=x;
        this.Control.style.top=y;
        this.Manager.RootContainer.appendChild(this.Control);
        
    }
    else
        this.Container.appendChild(this.Control);
    
    var me = this;
    
    eval('try { me.Control.appendChild(new ' + WidgetName + '(me.Manager.Document, me)) } catch(exp) {}');
    
    
    this.WidgetFunx = this.Control.childNodes[this.Control.childNodes.length-1].WidgetItem;

    return this;
};

Widget.prototype.Remove = function(updateWidthAjax)
{
    if(this.Control)
    {
        if(this.Control.parentNode)
            this.Control.parentNode.removeChild(this.Control);
    }   
    
    for(var i = this.Manager.Widgets.length-1;i>=0;i--)
    {
        if(this.Manager.Widgets[i] == this)
        {
            this.Manager.Widgets.splice(i,1);
            if(this.Manager.ControllerWindow != null)
            {
                if(this.Manager.ControllerWindow.UI != null)
                {
                    if(this.Manager.ControllerWindow.UI.container != null)
                    {
                        if(this.Manager.ControllerWindow.UI.container.contentWindow)
                        {
                            if(this.Manager.ControllerWindow.UI.container.contentWindow.WidgetClosed)
                                this.Manager.ControllerWindow.UI.container.contentWindow.WidgetClosed(this);
                        }
                    }
                }
            }
        }
    }
    
    
    
    if(updateWidthAjax)
    {
        var AjaxObject = new core.Ajax();
        AjaxObject.requestFile = 'ajax/widgetManagement.aspx';

        AjaxObject.encVar('remove', 'true');
        AjaxObject.encVar('name', this.WidgetName);
        
        AjaxObject.OnCompletion = function(){ __WidgetManagerAjaxLoad(AjaxObject); }; 
        AjaxObject.OnError = function(){ __WidgetManagerAjaxLoad(AjaxObject);};
        AjaxObject.OnFail = function(){  __WidgetManagerAjaxLoad(AjaxObject);};
        AjaxObject.RunAJAX();
    }
    
    delete this.Control;
};

function __WidgetManagerAjaxLoad(AjaxObject)
{
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject=null;
};
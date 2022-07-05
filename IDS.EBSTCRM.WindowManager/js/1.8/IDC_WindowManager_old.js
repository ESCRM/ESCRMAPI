var IDC_WindowManager = function (Document, parentObject, layout, jsVersion) {
    this.Document = Document;
    this.ParentObject = parentObject;
    this.isIE = Document.all ? true : false;
    this.BlankPage = 'blank.htm';

    //handle errors
    //    window.onerror = function(msg, url, lineno)
    //                                {
    //                                    alert(msg);
    //                                    return true;
    //                                };

    Document.oncontextmenu = function (e) { return false; };

    this.Layout = layout || 'IDControls/windowmanager/default.css';
    this.ScriptVersion = jsVersion || '';

    this.Core = new IDC_Core(Document, Document.body);

    //Attach keyboard now
    var me = this;
    this.Core.Keyboard.PrimaryKeyUpFunction = function (keyCode, keyboard) {
        if (me.CurrentWindow) {
            if (!keyCode)
                keyCode = event.keyCode;


            if (me.CurrentWindow.KeyUpEvents[keyCode])
                me.CurrentWindow.KeyUpEvents[keyCode]();

            if ((!keyboard.KeyControlPressed || !keyboard.KeyShiftPressed) && me.IsMouseDown)
                me.__TransparentOverlay.onmouseup(); //alert('WM CTRL KeyUp: ' + keyCode);
            else if ((!keyboard.KeyControlPressed || !keyboard.KeyShiftPressed) && !me.IsMouseDown) {
                me.CurrentWindow.MoveFromCenterWindow = false;
                me.__TransparentOverlay.style.display = 'none';
            }
        }

        return false;
    };

    this.Core.Keyboard.PrimaryKeyDownFunction = function (keyCode, keyboard) {
        if (me.CurrentWindow) {
            if (!keyCode)
                keyCode = event.keyCode;

            if (keyboard.KeyControlPressed && keyboard.KeyShiftPressed && !me.IsMouseDown) {

                if (me.CurrentWindow.ToolbarContextMenu) {
                    var toolbarContextMenu = me.CreateContextMenu();

                    for (var i = 0; i < me.CurrentWindow.ToolbarContextMenu.Buttons.length; i++) {
                        if (me.CurrentWindow.ToolbarContextMenu.Buttons[i].TypeName == 'SEP') {
                            if (toolbarContextMenu.Items.length > 0) {
                                toolbarContextMenu.AddSeparator();
                            }

                        }
                        else if (me.CurrentWindow.ToolbarContextMenu.Buttons[i].TypeName == 'BTN') {
                            var btn = me.CurrentWindow.ToolbarContextMenu.Buttons[i];
                            var ctxi = toolbarContextMenu.AddItem(btn.Name, btn.Icon, btn.OnClick);
                            ctxi.SetEnabledState(btn.Enabled);
                            ctxi.SetVisibility(btn.Control.style.display == '');
                        }
                    }


                    me.BindMouseContextMenuToObject(me.__TransparentOverlay, toolbarContextMenu);
                }
                else {
                    me.UnBindMouseContextMenuToObject(me.__TransparentOverlay);
                }


                me.CurrentWindow.MoveFromCenterWindow = true;
                me.__TransparentOverlay.style.display = '';
                me.__TransparentOverlay.style.backgroundColor = '';
                //                me.CurrentWindow.Titlebar.onmousedown(); // alert('WM CTRLKeyDown: ' + keyCode);
                //                me.CurrentWindow.Titlebar.onmousemove();
            }
        }

        return false;
    };

    this.Desktop = new IDC_Desktop(this, Document, parentObject);
    this.ContextMenuManager = new IDC_ContextMenuManager(this, Document.body); //this.Desktop.Control);
    this.DragDropManager = new IDS_DragDropManager(this, Document.body);
    this.VisualEffects = new IDC_Window_VisualEffects();
    this.ShowContentWhileDraggingAndResizing = false;

    this.Windows = new Array();
    this.WindowFocusHistory = new Array();

    this.ToolTipWindow = null;

    this.CurrentWindow = null;
    this.IsMouseDown = false;
    this.CurrentWindowEvent = null;

    this.__TransparentOverlay = null;
    this.__MovingAndResizingWindowOverlay = null;
    this.__MakeOverlay();

    var heads = Document.getElementsByTagName("head");
    this.__Head = heads.length > 0 ? heads[0] : null;

    this.TopWindowIndex = 1;
    this.AlwaysOnTopWindowIndex = 125000;

    this.ChangeLayout(this.Layout);

    //Events
    this.OnWindowResize = null;
    this.OnWindowResized = null;
    this.OnWindowMaximize = null;
    this.OnWindowMinimize = null;
    this.OnWindowRestore = null;
    this.OnWindowClose = null;
    this.OnWindowMove = null;
    this.OnWindowFocus = null;
    this.OnWindowBlur = null;

    this.Document.body.onresize = function () { me.DesktopResized(); };
};

IDC_WindowManager.prototype.DesktopResized = function () {

    for (var i = 0; i < this.Windows.length; i++) {
        if (this.Windows[i].WindowState == 2) {
            if (this.Windows[i].OnResized) this.Windows[i].OnResized(this.Windows[i], this.Windows[i].Left, this.Windows[i].Top, this.Windows[i].GetInnerWidth(), this.Windows[i].GetInnerHeight());
        }
    }

};

IDC_WindowManager.prototype.BindTooltipToObject = function(Object, Content)
{
    Object.WindowManager = this;
    Object.Content = Content;
    Object.onmouseover = function() 
        { 
            var me = this; 
            this.TimerId = setTimeout(function() 
                { 
                    me.TimerId=null; 
                    me.WindowManager.ShowTooltip(me.Content); 
                },1000
                ); 
         };
    
    Object.onmouseout = function() 
        { 
            var me = this; 
            setTimeout(function() 
            { 
                if(me.TimerId) 
                { 
                    clearTimeout(me.TimerId); 
                    me.TimerId=null; 
                 }; 
                 
                 if(!me.WindowManager.ToolTipWindow.HasFocus) 
                    me.WindowManager.HideTooltip();
             },10
             );         
         };
};

IDC_WindowManager.prototype.ShowTooltip = function(Content)
{
    if(!this.ToolTipWindow)
    {
        this.ToolTipWindow = this.__MakeGhostWindowFocused();
        this.ToolTipWindow.Content = this.Document.createElement('DIV');
        this.ToolTipWindow.appendChild(this.ToolTipWindow.Content);
        
        this.ToolTipWindow.WindowManager = this;
        
        this.ToolTipWindow.Content.style.whiteSpace='nowrap';
        this.ToolTipWindow.Content.style.position='absolute';
        this.Desktop.Control.appendChild(this.ToolTipWindow);
        
        this.ToolTipWindow.onmouseover = function() { this.HasFocus=true; };
        this.ToolTipWindow.onmouseout = function() { this.HasFocus=false; this.WindowManager.HideTooltip(); };
    }
    
    if(this.isIE)
        this.ToolTipWindow.Content.innerText = Content;
    else
        this.ToolTipWindow.Content.textContent = Content;
       
    this.ToolTipWindow.style.left = this.Core.Mouse.X;
    this.ToolTipWindow.style.top = this.Core.Mouse.Y;
    
    this.ToolTipWindow.style.display='';
    
    this.ToolTipWindow.style.width = this.ToolTipWindow.Content.offsetWidth;
    this.ToolTipWindow.style.height = this.ToolTipWindow.Content.offsetHeight;
   
};

IDC_WindowManager.prototype.HideTooltip = function()
{
    if(!this.ToolTipWindow) return;

    if(this.isIE)
        this.ToolTipWindow.Content.innerText = '';
    else
        this.ToolTipWindow.Content.textContent = '';
        
    this.ToolTipWindow.style.display='none';
};

IDC_WindowManager.prototype.CreateContextMenu = function(OnBeforeShow)
{
    return new IDC_ContextMenu(this, null, OnBeforeShow);
};


IDC_WindowManager.prototype.BindMouseContextMenuToObject = function (Object, ContextMenu) {
    if (!Object || !ContextMenu) return;

    Object.ContextMenu = ContextMenu;
    Object.oncontextmenu = function (e) {
        var target = e ? e.target : event.srcElement;
        if (target == this && Object.ContextMenu) {
            Object.ContextMenu.WindowManager.__TransparentOverlay.ContextIsOn = false;
            Object.ContextMenu.Show(this.ContextMenu.WindowManager.Core.Mouse.X, this.ContextMenu.WindowManager.Core.Mouse.Y);
        }
    };
};

IDC_WindowManager.prototype.BindMouseContextMenuToObjectAndChildren = function(Object, ContextMenu)
{
    if(!Object || !ContextMenu) return;
    
    Object.ContextMenu = ContextMenu;
    Object.oncontextmenu = function(e) {
                                    Object.ContextMenu.WindowManager.__TransparentOverlay.ContextIsOn = false;
                                    Object.ContextMenu.Show(this.ContextMenu.WindowManager.Core.Mouse.X, this.ContextMenu.WindowManager.Core.Mouse.Y);
                               };
};

IDC_WindowManager.prototype.UnBindMouseContextMenuToObject = function(Object)
{
    if(!Object) return;
    Object.oncontextmenu = null;
};


IDC_WindowManager.prototype.GetWindowByName = function(Name)
{
    for(var i=this.Windows.length-1;i>=0;i--)
    {
        if(this.Windows[i].Name == Name)
        {
            return this.Windows[i];
        }
    }   
    return null;
};

IDC_WindowManager.prototype.CloseWindow = function(Window)
{
    for(var i=this.WindowFocusHistory.length-1;i>=0;i--)
    {
        if(this.WindowFocusHistory[i] == Window)
        {
            this.WindowFocusHistory.splice(i,1);
        }
    }
    for(var i=this.Windows.length-1;i>=0;i--)
    {
        if(this.Windows[i] == Window)
        {
            this.Windows[i].Close();
            this.Windows[i]=null;
            
            this.Windows.splice(i,1);
            break;
        }
    }   
};

IDC_WindowManager.prototype.CloseWindows = function()
{
    
    for(var i=this.WindowFocusHistory.length-1;i>=0;i--)
    {
        this.WindowFocusHistory.splice(i,1);
    }
    for(var i=this.Windows.length-1;i>=0;i--)
    {
        this.Windows[i].Close(null, true, true);
        delete this.Windows[i];
        this.Windows[i]=null;
        
        this.Windows.splice(i,1);
    }   
};


IDC_WindowManager.prototype.ChangeLayout = function(stylesheet)
{
    if(!this.__Head) return;
    if(this.Layout == stylesheet) return;
    
    var styles = this.__Head.getElementsByTagName('link');
    for(var i=styles.length-1;i>=0;i--)
    {
        var src = styles[i].getAttribute('href');
        if(src.indexOf(this.Layout)>-1)
        {
            this.__Head.removeChild(styles[i]);
        }
    }
    
    this.Layout = stylesheet;
    
    var cssNode = this.Document.createElement('link');
    cssNode.type = 'text/css';
    cssNode.rel = 'stylesheet';
    cssNode.href = this.Layout;
    cssNode.media = 'screen';
    this.__Head.insertBefore(cssNode, this.__Head.firstChild);
};

IDC_WindowManager.prototype.__MakeGhostWindow = function()
{
    var obj = this.Document.createElement('DIV');
   obj.style.position='absolute';
   obj.style.zIndex=250000;
   obj.style.width='100';
   obj.style.height='100';
   obj.style.left=0;
   obj.style.top=0;
   obj.style.display='none';
   obj.className='IDCWMWindowFrameCenter_MV';
    
    //Inner Objects
   obj.ResizeTopLeft = this.Document.createElement('DIV');
   obj.ResizeTop = this.Document.createElement('DIV');
   obj.ResizeTopRight = this.Document.createElement('DIV');
   obj.ResizeLeft = this.Document.createElement('DIV');   
   obj.ResizeRight = this.Document.createElement('DIV');
   obj.ResizeBottomLeft = this.Document.createElement('DIV');
   obj.ResizeBottom = this.Document.createElement('DIV');
   obj.ResizeBottomRight = this.Document.createElement('DIV');
    
    //Classes
   obj.ResizeTopLeft.className='IDCWMWindowFrameRZTL_MV';
   obj.ResizeTop.className='IDCWMWindowFrameRZT_MV';
   obj.ResizeTopRight.className='IDCWMWindowFrameRZTR_MV';
   obj.ResizeLeft.className='IDCWMWindowFrameRZL_MV';
   obj.ResizeRight.className='IDCWMWindowFrameRZR_MV';
   obj.ResizeBottomLeft.className='IDCWMWindowFrameRZBL_MV';
   obj.ResizeBottom.className='IDCWMWindowFrameRZB_MV';
   obj.ResizeBottomRight.className='IDCWMWindowFrameRZBR_MV';
    
   obj.appendChild(obj.ResizeTopLeft);
   obj.appendChild(obj.ResizeTop);
   obj.appendChild(obj.ResizeTopRight);
   obj.appendChild(obj.ResizeLeft);
   obj.appendChild(obj.ResizeRight);
   obj.appendChild(obj.ResizeBottomLeft);
   obj.appendChild(obj.ResizeBottom);
   obj.appendChild(obj.ResizeBottomRight);
   
   return obj;
};

IDC_WindowManager.prototype.__MakeGhostWindowFocused = function()
{
    var obj = this.Document.createElement('DIV');
   obj.style.position='absolute';
   obj.style.zIndex=250000;
   obj.style.width='1';
   obj.style.height='1';
   obj.style.left=0;
   obj.style.top=0;
   obj.style.display='none';
   obj.className='IDCWMWindowFrameCenterFocused';
    
    //Inner Objects
   obj.ResizeTopLeft = this.Document.createElement('DIV');
   obj.ResizeTop = this.Document.createElement('DIV');
   obj.ResizeTopRight = this.Document.createElement('DIV');
   obj.ResizeLeft = this.Document.createElement('DIV');   
   obj.ResizeRight = this.Document.createElement('DIV');
   obj.ResizeBottomLeft = this.Document.createElement('DIV');
   obj.ResizeBottom = this.Document.createElement('DIV');
   obj.ResizeBottomRight = this.Document.createElement('DIV');
    
    //Classes
   obj.ResizeTopLeft.className='IDCWMWindowFrameRZTLFocused';
   obj.ResizeTop.className='IDCWMWindowFrameRZTFocused';
   obj.ResizeTopRight.className='IDCWMWindowFrameRZTRFocused';
   obj.ResizeLeft.className='IDCWMWindowFrameRZLFocused';
   obj.ResizeRight.className='IDCWMWindowFrameRZRFocused';
   obj.ResizeBottomLeft.className='IDCWMWindowFrameRZBLFocused';
   obj.ResizeBottom.className='IDCWMWindowFrameRZBFocused';
   obj.ResizeBottomRight.className='IDCWMWindowFrameRZBRFocused';
    
   obj.appendChild(obj.ResizeTopLeft);
   obj.appendChild(obj.ResizeTop);
   obj.appendChild(obj.ResizeTopRight);
   obj.appendChild(obj.ResizeLeft);
   obj.appendChild(obj.ResizeRight);
   obj.appendChild(obj.ResizeBottomLeft);
   obj.appendChild(obj.ResizeBottom);
   obj.appendChild(obj.ResizeBottomRight);
   
   return obj;
};

IDC_WindowManager.prototype.GhostAnimation = function(startX, startY, startW, startH, endX, endY, endW, endH, passes, fadeMode, OnAnimationComplete)
{
    if(!passes) passes=10;

    var ghost = this.__MakeGhostWindow();
    this.Desktop.Control.appendChild(ghost);
    
    var xstep = (startX -endX)/passes;
    var ystep = (startY -endY)/passes;
    var wstep = (startW -endW)/passes;
    var hstep = (startH -endH)/passes;
    
    this.__GhostAnimate(ghost, startX, startY, startW, startH, xstep, ystep, wstep, hstep, passes, passes, fadeMode, OnAnimationComplete);
};

IDC_WindowManager.prototype.__GhostAnimate = function(obj, x, y, w, h, sx, sy, sw, sh, passesLeft, passes, fadeMode, OnAnimationComplete)
{
    passesLeft--;
    
    x-=sx;
    y-=sy;
    w-=sw;
    h-=sh;
    
    if(w<0) w = 0;
    if(h<0) h = 0;
    
    obj.style.left = x;
    obj.style.top = y;
    obj.style.width = w;
    obj.style.height = h;
    obj.style.display='';
    
    if(!this.isIE)
    {
        if(fadeMode == -1)
            obj.style.opacity = passesLeft/passes;
        else if(fadeMode==1)
            obj.style.opacity = 1- (passesLeft/passes);
    }
    if(passesLeft>0)
    {
        var me = this;
        setTimeout(function() { me.__GhostAnimate(obj, x, y, w, h, sx, sy, sw, sh, passesLeft, passes, fadeMode, OnAnimationComplete); },15);
    }
    else
    {
        if(OnAnimationComplete) OnAnimationComplete();
        obj.parentNode.removeChild(obj);
        delete obj;
        obj = null;
    }
    
};



IDC_WindowManager.prototype.__MakeOverlay = function () {


    this.__MovingAndResizingWindowOverlay = this.__MakeGhostWindow();
    this.Desktop.Control.appendChild(this.__MovingAndResizingWindowOverlay);

    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position = 'absolute';
    this.__TransparentOverlay.style.zIndex = 900000;
    this.__TransparentOverlay.style.width = '100%';
    this.__TransparentOverlay.style.height = '100%';
    this.__TransparentOverlay.style.left = 0;
    this.__TransparentOverlay.style.top = 0;
    this.__TransparentOverlay.style.display = 'none';
    this.__TransparentOverlay.style.color = 'white';
    this.__TransparentOverlay.oncontextmenu = function (e) { alert('ctx');  return false; };
    this.__TransparentOverlay.MovementOccurred = false;

    this.__TransparentOverlay.onmousedown = function (e) {
        if (this.WindowManager.CurrentWindow != null) {
            var rightclick;
            if (e !=null && e.which) rightclick = (e.which == 3);
            else if (e != null && e.button) rightclick = (e.button == 2);

            if (rightclick) {
                this.ContextIsOn = true;
            }
            else {
                this.ContextIsOn = false;
                this.WindowManager.CurrentWindow.Titlebar.onmousedown(); // alert('WM CTRLKeyDown: ' + keyCode);
                this.WindowManager.CurrentWindow.Titlebar.onmousemove();
            }
        }
    };

    this.__TransparentOverlay.onclick = function () { };
    this.__TransparentOverlay.onmouseup = function (e) {

        if (this.ContextIsOn) return;

        this.WindowManager.IsMouseDown = false;
        
        this.style.cursor = 'arrow';
        this.style.display = 'none';


        if (!this.MovementOccurred) return;
        this.MovementOccurred = false;

        if (this.WindowManager.CurrentWindow.WindowState == 1) {
            if (this.WindowManager.CurrentWindowEvent.indexOf('r') == 0) {
                if (!this.WindowManager.ShowContentWhileDraggingAndResizing) {
                    this.WindowManager.CurrentWindow.SetPositionAndSize(this.WindowManager.__MovingAndResizingWindowOverlay.offsetLeft, this.WindowManager.__MovingAndResizingWindowOverlay.offsetTop, this.WindowManager.__MovingAndResizingWindowOverlay.offsetWidth, this.WindowManager.__MovingAndResizingWindowOverlay.offsetHeight - this.WindowManager.CurrentWindow.TitlebarTable.offsetHeight);
                    this.WindowManager.__MovingAndResizingWindowOverlay.style.display = 'none';
                }

                if (this.WindowManager.CurrentWindow.OnResized) this.WindowManager.CurrentWindow.OnResized(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.CurrentWindow.GetInnerWidth(), this.WindowManager.CurrentWindow.GetInnerHeight());
                if (this.WindowManager.OnWindowResized) this.WindowManager.OnWindowResized(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.CurrentWindow.GetInnerWidth(), this.WindowManager.CurrentWindow.GetInnerHeight());

            }
            else if (this.WindowManager.CurrentWindowEvent == 'mv' && !this.WindowManager.ShowContentWhileDraggingAndResizing) {
                this.WindowManager.CurrentWindow.MoveTo(this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY);
                if (this.WindowManager.CurrentWindow.OnMove) this.WindowManager.CurrentWindow.OnMove(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top);
                if (this.WindowManager.OnWindowMove) this.WindowManager.OnWindowMove(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top);

                this.WindowManager.__MovingAndResizingWindowOverlay.style.display = 'none';
            }
        }

    };

    if (this.isIE) {
        this.__TransparentOverlay.style.backgroundImage = 'url(' + this.Core.WebsiteUrl + 'images/spacer.gif)';
    }
    if (this.Desktop)
        this.Desktop.Control.appendChild(this.__TransparentOverlay);
    else
        this.ParentObject.appendChild(this.__TransparentOverlay);

    this.__TransparentOverlay.WindowManager = this;
    this.__TransparentOverlay.onmousemove = function () {
        this.MovementOccurred = true;

        //this.innerHTML=this.WindowManager.Core.Mouse.X + ' x ' + this.WindowManager.Core.Mouse.Y;

        if (this.WindowManager.IsMouseDown && this.WindowManager.CurrentWindow) {
            if (this.WindowManager.CurrentWindow.WindowState == 1) {
                if (this.WindowManager.CurrentWindowEvent == 'mv') {
                    if (this.WindowManager.ShowContentWhileDraggingAndResizing) {
                        this.WindowManager.CurrentWindow.MoveTo(this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY);
                        if (this.WindowManager.CurrentWindow.OnMove) this.WindowManager.CurrentWindow.OnMove(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top);
                        if (this.WindowManager.OnWindowMove) this.WindowManager.OnWindowMove(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top);
                    }
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY, this.WindowManager.CurrentWindow.Width, this.WindowManager.CurrentWindow.Height);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }

                    return;
                }

                if (this.WindowManager.CurrentWindowEvent == 'rbr') //resize bottom right
                {
                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetSize(this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rr') //resize right
                {
                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetSize(this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW, this.WindowManager.CurrentWindow.BaseOffsetH);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW, this.WindowManager.CurrentWindow.BaseOffsetH);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rb') //resize bottom
                {
                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetSize(this.WindowManager.CurrentWindow.BaseOffsetW, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.CurrentWindow.BaseOffsetW, this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rbl') //resize bottom left
                {

                    var x = this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX;
                    var w = this.WindowManager.CurrentWindow.BaseOffsetW + (this.WindowManager.CurrentWindow.BaseOffsetW - (this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW));
                    var h = this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH;

                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetPositionAndSize(x, null, w, h);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(x, this.WindowManager.CurrentWindow.Top, w, h);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rl') //resize bottom left
                {

                    var x = this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX;
                    var w = this.WindowManager.CurrentWindow.BaseOffsetW + (this.WindowManager.CurrentWindow.BaseOffsetW - (this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW));
                    var h = this.WindowManager.CurrentWindow.BaseOffsetH;
                    var y = this.WindowManager.CurrentWindow.Control.offsetTop;

                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetPositionAndSize(x, y, w, h);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(x, y, w, h);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rtl') //resize bottom left
                {

                    var x = this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseX;
                    var w = this.WindowManager.CurrentWindow.BaseOffsetW + (this.WindowManager.CurrentWindow.BaseOffsetW - (this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW));
                    var h = this.WindowManager.CurrentWindow.BaseOffsetH + (this.WindowManager.CurrentWindow.BaseOffsetH - (this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH));
                    var y = this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY;

                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetPositionAndSize(x, y, w, h);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(x, y, w, h);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rt') //resize bottom left
                {

                    var w = this.WindowManager.CurrentWindow.BaseOffsetW;
                    var h = this.WindowManager.CurrentWindow.BaseOffsetH + (this.WindowManager.CurrentWindow.BaseOffsetH - (this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH));
                    var y = this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY;

                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetPositionAndSize(null, y, w, h);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.CurrentWindow.Left, y, w, h);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }
                else if (this.WindowManager.CurrentWindowEvent == 'rtr') //resize bottom left
                {

                    var w = this.WindowManager.Core.Mouse.X - this.WindowManager.CurrentWindow.BaseMouseW;
                    var h = this.WindowManager.CurrentWindow.BaseOffsetH + (this.WindowManager.CurrentWindow.BaseOffsetH - (this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseH));
                    var y = this.WindowManager.Core.Mouse.Y - this.WindowManager.CurrentWindow.BaseMouseY;

                    if (this.WindowManager.ShowContentWhileDraggingAndResizing)
                        this.WindowManager.CurrentWindow.SetPositionAndSize(null, y, w, h);
                    else {
                        this.WindowManager.SetOverMovingAndResizingOverlay(this.WindowManager.CurrentWindow.Left, y, w, h);
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.zIndex = this.WindowManager.AlwaysOnTopWindowIndex + this.WindowManager.TopWindowIndex + 1;
                        this.WindowManager.__MovingAndResizingWindowOverlay.style.display = '';
                    }
                }

                //EVENTS
                if (this.WindowManager.CurrentWindowEvent.indexOf('r') == 0 && this.WindowManager.ShowContentWhileDraggingAndResizing) {
                    if (this.WindowManager.CurrentWindow.OnResize) this.WindowManager.CurrentWindow.OnResize(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.CurrentWindow.GetInnerWidth(), this.WindowManager.CurrentWindow.GetInnerHeight());
                    if (this.WindowManager.OnWindowResize) this.WindowManager.OnWindowResize(this.WindowManager.CurrentWindow, this.WindowManager.CurrentWindow.Left, this.WindowManager.CurrentWindow.Top, this.WindowManager.CurrentWindow.GetInnerWidth(), this.WindowManager.CurrentWindow.GetInnerHeight());
                }
            }
        }
    };
};

IDC_WindowManager.prototype.SetOverMovingAndResizingOverlay = function(x,y,w,h,ignoreMinAndMax)
{
    //make sure w and h is not less than minimum and maximum values
    if(!ignoreMinAndMax)
    {
        if(w<this.CurrentWindow.MinWidth)
        {
           if(x!=this.__MovingAndResizingWindowOverlay.offsetLeft) x-=(this.CurrentWindow.MinWidth-w);
            w=this.CurrentWindow.MinWidth;
        }           
        if(h<this.CurrentWindow.MinHeight)
        {
            if(y!=this.__MovingAndResizingWindowOverlay.offsetTop) y-=(this.CurrentWindow.MinHeight-h);
            h=this.CurrentWindow.MinHeight;
        }
        
        if(w>this.CurrentWindow.MaxWidth && this.CurrentWindow.MaxWidth>this.CurrentWindow.MinWidth)
        {
            if(x!=this.__MovingAndResizingWindowOverlay.offsetLeft) x-=(this.CurrentWindow.MaxWidth-w);
            w=this.CurrentWindow.MaxWidth;
        }
                   
        if(h>this.CurrentWindow.MaxHeight && this.CurrentWindow.MaxHeight>this.CurrentWindow.MinHeight)
        {
            if(y!=this.__MovingAndResizingWindowOverlay.offsetTop) y-=(this.CurrentWindow.MaxHeight-h);
            h=this.CurrentWindow.MaxHeight;
        }
    }

    this.__MovingAndResizingWindowOverlay.style.left = x;
    this.__MovingAndResizingWindowOverlay.style.top = y;
    this.__MovingAndResizingWindowOverlay.style.width = w;
    this.__MovingAndResizingWindowOverlay.style.height = h + this.CurrentWindow.TitlebarTable.offsetHeight;
                                                        
};

IDC_WindowManager.prototype.SetOverlayCursor = function()
{
    if(this.CurrentWindowEvent=='mv')
        this.__TransparentOverlay.style.cursor='pointer';
        
    else if(this.CurrentWindowEvent=='rbr' || this.CurrentWindowEvent=='rtl')
        this.__TransparentOverlay.style.cursor='nw-resize';
        
    else if(this.CurrentWindowEvent=='rb' || this.CurrentWindowEvent=='rt')
        this.__TransparentOverlay.style.cursor='s-resize';
        
    else if(this.CurrentWindowEvent=='rbl' || this.CurrentWindowEvent=='rtr')
        this.__TransparentOverlay.style.cursor='ne-resize';

    else if(this.CurrentWindowEvent=='rl' || this.CurrentWindowEvent=='rr')
        this.__TransparentOverlay.style.cursor='e-resize';
        
    else
        this.__TransparentOverlay.style.cursor='arrow';
};

IDC_WindowManager.prototype.CreateWindow = function(Url, DialogOf, Name, Icons, Width, Height, Left, Top, CreatedFromWindow)
{
    var w = new IDC_Window(this, Url, DialogOf, Name, Icons, Width, Height, Left, Top, CreatedFromWindow);
    this.Windows.push(w);
    return w;
};

var IDC_Window = function(windowManager, Url, DialogOf, Name, Icons, Width, Height, Left, Top, CreatedFromWindow)
{
    //Private properties
    this.Document = windowManager.Document;
    this.ParentObject = windowManager.Desktop ? windowManager.Desktop.Control : windowManager.ParentObject;
    this.WindowManager = windowManager;
    this.isIE = document.all?true:false;
    
    this.CreatedFromWindow = CreatedFromWindow;
    this.CreatedFromWindowRoot = (CreatedFromWindow != null ? CreatedFromWindow.CreatedFromWindow || CreatedFromWindow : null);
    
    this.ToolbarContextMenu = null;
    this.Keyboard = null;
    this.KeyUpEvents = new Array();
    this.KeyDownEvents = new Array();
    
    this.OnKeyDown = null;
    this.OnKeyUp = null;
    
    this.BaseMouseX=0;
    this.BaseMouseY=0; 
    
    this.BaseOffsetW=0;
    this.BaseOffsetH=0;
    
    this.BaseMouseW=0;
    this.BaseMouseH=0;
    
    this.SelectedTitleBarTab = null;
    
    //public properties
    this.UseBorder=true;
    
    this.IsPinable=true;
    
    this.AlwaysOnTop=false;
    this.DialogOwnerOf=DialogOf;
    this.IsLocked=false;
    this.IsLockedBy=null;
    
    this.Variables=null;
    
    if(this.DialogOwnerOf)
    {
        this.DialogOwnerOf.IsLocked=true;
        this.DialogOwnerOf.IsLockedBy=this;
    }
    
    this.Width=Width || 200;
    this.Height=Height || 100;
    
    this.MinWidth=160;
    this.MinHeight=60;
    this.MaxWidth=null;
    this.MaxHeight=null;
    
    this.Closeable=true;
    this.Minimizeable=true;
    this.Maximizeable=true;
    this.Resizeable=true;
    
    this.ShowInTaskbar=true;
    
    this.WindowState=1;
    this.OldWindowState=1;
    this.IsFocused = false;
    
    this.Name=Name;
    this.Icons=Icons;
    this.Url=Url;
    
    this.Left = Left || 0;
    this.Top = Top || 0;
    
    this.Width = Width || 400;
    this.Height = Height || 300;
    
    this.Control = null;
    this.TaskbarButton=null;
    this.TabControls = new Array();
    
    //Prepare iframe and load it before it's shown
    this.ContentIframe = this.Document.createElement('IFRAME');
    this.ContentIframe.className='IDCWMWindowIFrame';
    this.ContentIframe.scrolling='no';
    this.ContentIframe.frameBorder='no';
    this.ContentIframe.IDCWindow = this;
    this.ContentIframe.style.display='';
    this.Document.body.appendChild(this.ContentIframe);
    this.ContentIframe.src=this.Url || this.WindowManager.BlankPage;
    
    //Events
    this.OnResize = null;
    this.OnResized = null;
    this.OnMaximize = null;
    this.OnMinimize = null;
    this.OnRestore = null;
    this.OnClose = null;
    this.OnMove = null;
    this.OnFocus = null;
    this.OnBlur = null;
    
    //Dirty manager
    this.IsDirty = false;
    this.UseDirtyManager = false;
    this.OnDirtyDialogOK = null;
    this.OnDirtyDialogCancel = null;
    this.DirtyMessage = 'This windows has been modified.<br><br>Do you want to close it anyway?';
    this.DirtyTitle = 'Window is modified';
};

IDC_Window.prototype.CreateWindow = function(Url, DialogOf, Name, Icons, Width, Height, Left, Top)
{
    if(DialogOf == this)
    {
        
    }
    return this.WindowManager.CreateWindow(Url, DialogOf, Name, Icons, Width, Height, Left, Top, this);
};

IDC_Window.prototype.CreateContextMenu = function(LocalCore, OnBeforeShow)
{
    if(!LocalCore)
    {
        this.Alert('CreateContextMenu expects argument IDC_Core LocalCore.','ERROR!');
        return null;
    }
    return new IDC_ContextMenu(this.WindowManager, null, this, LocalCore, OnBeforeShow);
};

IDC_Window.prototype.BindMouseContextMenuToObject = function(Object, ContextMenu, LocalCore)
{
    if(!Object || !ContextMenu) return;
    
    Object.ContextMenu = ContextMenu;
    Object.LocalCore = LocalCore;
    Object.WindowControl = this;
    Object.oncontextmenu = function(e)
                               {
                                    var target=e?e.target:event.srcElement;
                                    if(target == this && Object.ContextMenu)
                                    {
                                        //var wd = (this.WindowControl.ContentIframeOuterBox.offsetWidth - this.WindowControl.ContentIframe.offsetWidth);
                                        //var hd = (this.WindowControl.ContentIframeOuterBox.offsetHeight - this.WindowControl.ContentIframe.offsetHeight);
                                        //Object.ContextMenu.Show(this.WindowControl.Left + this.LocalCore.Mouse.X + wd, this.WindowControl.Top +this.LocalCore.Mouse.Y + this.WindowControl.Titlebar.offsetHeight+hd);
                                        Object.ContextMenu.Show(this.ContextMenu.Core.Mouse.X, this.ContextMenu.Core.Mouse.Y + this.ContextMenu.WindowManager.Desktop.Control.offsetTop);
                                    }
                                    return false;
                               };
};

IDC_Window.prototype.BindMouseContextMenuToObjectAndChildren = function(Object, ContextMenu, LocalCore)
{
    if(!Object || !ContextMenu) return;
    
    Object.ContextMenu = ContextMenu;
    Object.LocalCore = LocalCore || ContextMenu.Core;
    Object.WindowControl = this;
    Object.oncontextmenu = function(e) {

                                    //frame size
                                    //var wd = (this.WindowControl.ContentIframeOuterBox.offsetWidth - this.WindowControl.ContentIframe.offsetWidth);
                                    //var hd = (this.WindowControl.ContentIframeOuterBox.offsetHeight - this.WindowControl.ContentIframe.offsetHeight);
                                    //Object.ContextMenu.Show(this.WindowControl.Left + this.LocalCore.Mouse.X + wd, this.WindowControl.Top +this.LocalCore.Mouse.Y + this.WindowControl.Titlebar.offsetHeight+hd);
                                    Object.ContextMenu.Show(this.ContextMenu.Core.Mouse.X, this.ContextMenu.Core.Mouse.Y + this.ContextMenu.WindowManager.Desktop.Control.offsetTop);
                                    return false;
                               };
};

IDC_Window.prototype.CaptureKeyboard = function()
{
    if(!this.Keyboard)
    {
        this.Keyboard = new IDC_Keyboard(this.ContentIframe.contentWindow.document,this.ContentIframe.contentWindow.document);
        var me = this;
        this.Keyboard.PrimaryKeyUpFunction = function(keyCode)
                                                {
                                                    if(!keyCode)
                                                        keyCode = me.ContentIframe.contentWindow.event.keyCode;

                                                    if(me.KeyUpEvents[keyCode])
                                                        me.KeyUpEvents[keyCode]();

                                                };
    }
};

IDC_Window.prototype.BindKeyUpEvent = function( keyCode, Event )
{
    this.CaptureKeyboard();
    this.KeyUpEvents[keyCode] = Event;
};
IDC_Window.prototype.BindKeyDownEvent = function( keyCode, Event )
{
    this.CaptureKeyboard();
    this.KeyDownEvents[keyCode] = Event;
};
IDC_Window.prototype.UnBindKeyUpEvent = function( keyCode )
{
    this.KeyUpEvents[keyCode] = null;
};
IDC_Window.prototype.UnBindKeyDownEvent = function( keyCode )
{
    this.KeyDownEvents[keyCode] = null;
};

IDC_Window.prototype.ClearHeaderTabs = function () {

    if (this.TitlebarTabs == null) {
        this.TitlebarTabs = this.Document.createElement('SPAN');
        this.TitlebarTabs.className = 'IDCWMWindowFrameTitleBarTabItems';
        this.ContentIframeOuterBox.appendChild(this.TitlebarTabs);
    }

    while (this.TitlebarTabs.hasChildNodes()) {
        this.TitlebarTabs.removeChild(this.TitlebarTabs.lastChild);
    }
};

IDC_Window.prototype.RemoveHeaderTab = function( tab )
{
    for(var i=0;i<this.TitlebarTabs.childNodes.length;i++)
    {
        var t = this.TitlebarTabs.childNodes[i];
        if(t==tab)
        {
            if(t.className == 'IDCWMWindowFrameTitleBarTabItem_s' + t.Color)
            {
                if(i>0)
                    this.TitlebarTabs.childNodes[i-1].onclick();
                else if(this.TitlebarTabs.childNodes.length>1)
                    this.TitlebarTabs.childNodes[i+1].onclick();
            }
            
            this.TitlebarTabs.removeChild(t);
            break;
        }
    }
};

IDC_Window.prototype.AddHeaderTab = function( text, onclick , color)
{
    var div = this.isIE ? this.Document.createElement('SPAN') : this.Document.createElement('DIV');
    div.Color = (color || '');
    div.className='IDCWMWindowFrameTitleBarTabItem' + div.Color;
    div.innerHTML=text;
    this.TitlebarTabs.appendChild(div);
    
    div.ClickEvent = onclick;
    div.onclick = function()
        {
            this.SelectedTitleBarTab = this;
            for(var i=0;i<this.parentNode.childNodes.length;i++)
            {
                this.parentNode.childNodes[i].className= this.parentNode.childNodes[i]==this ? 'IDCWMWindowFrameTitleBarTabItem_s' + this.parentNode.childNodes[i].Color : 'IDCWMWindowFrameTitleBarTabItem' + this.parentNode.childNodes[i].Color;
            }
            
            if(this.ClickEvent)
                this.ClickEvent(this);
        };
    
    return div;
    
};

IDC_Window.prototype.Close = function(Arguments, ignoreAnimation, ignoreDirty)
{   
    //Animation before anything else
//    this.IsDirty = false;
//    this.UseDirtyManager = false;
//    this.DirtyMessage = 'This windows has been modified.<br><br>Do you want to close it anyway?';
//    this.DirtyTitle = 'Window is modified';
    
    if(this.IsDirty && this.UseDirtyManager && !ignoreDirty)
    {
    
        var me = this;
        this.Confirm(this.DirtyMessage, this.DirtyTitle, null, function(sender, retval) 
                                                                    { 
                                                                        if(retval) 
                                                                        { 
                                                                            if(me.OnDirtyDialogOK) 
                                                                            {
                                                                                me.OnDirtyDialogOK(me);
                                                                            }  
                                                                            me.IsDirty=false; 
                                                                            me.Close( Arguments, ignoreAnimation, true ); 
                                                                        }
                                                                        else
                                                                        {
                                                                            if(me.OnDirtyDialogCancel)
                                                                            {
                                                                                me.OnDirtyDialogCancel(me);
                                                                            }
                                                                        }
                                                                     });
        return;
    }
    
    if(!ignoreAnimation && this.WindowManager.VisualEffects.AnimateOnClose)
    {
        var w1 = this.Width/1.4;
        var h1 = this.Height/1.4;
        var x2 = (this.Width - w1)/2;
        var y2 = (this.Height - h1)/2;
        
        this.WindowManager.GhostAnimation(this.Left, this.Top, this.Width, this.Height, this.Left + x2, this.Top + y2, w1, h1, 7, -1);
    }
    
    if(this.DialogOwnerOf)
    {
        this.DialogOwnerOf.IsLocked=false;
        this.DialogOwnerOf.IsLockedBy=null;
        this.DialogOwnerOf.Focus();
        this.DialogOwnerOf = null;
    }
    else
    {
        for(var i=this.WindowManager.WindowFocusHistory.length-1;i>=0;i--)
        {
            if(this.WindowManager.WindowFocusHistory[i] == this)
            {
                this.WindowManager.WindowFocusHistory.splice(i,1);
            }
            else
            {
                try
                {
                    this.WindowManager.WindowFocusHistory[i].Focus();
                    break;
                }
                catch(exp)
                {
                    this.WindowManager.WindowFocusHistory.splice(i,1);
                }
            }
        }
    }
    
    if(this.OnClose)
    {
        try
        {
            this.OnClose(this, Arguments);
        }
        catch(exp)
        {};
    }
    if(this.WindowManager.OnWindowClose)
    {
        try
        {
            this.WindowManager.OnWindowClose(this, Arguments);
        }
        catch(exp)
        {};
    }
     
                                                    
    //No current window
    if(this.WindowManager.CurrentWindow == this)
        this.WindowManager.CurrentWindow =null;
    
    //If the controls is created then dispose
    if(this.Control)
    {
        try
        {
            this.WindowManager.Core.DOM.Purge(this.Control);
        }
        catch(ex)
        { }
        
        //Clear ALL events
        this.Titlebar.onmousedown=null;
        this.Titlebar.onmouseover=null;
        this.TitlebarIcon.onerror=null;
        this.cmdClose.onmouseover=null;
        this.cmdClose.onmouseout=null;
        this.cmdClose.onclick=null;
        this.cmdMinimize.onmouseover=null;
        this.cmdMinimize.onmouseout=null;
        this.cmdMinimize.onclick=null;
        this.cmdMaximize.onmouseover=null;
        this.cmdMaximize.onmouseout=null;
        this.cmdMaximize.onclick=null;
        this.cmdRestore.onmouseover=null;
        this.cmdRestore.onmouseout=null;
        this.cmdRestore.onclick=null;
        this.Titlebar.onmousedown=null;
        this.Titlebar.onmouseover=null;
        this.Titlebar.onmousemove=null;
        this.Titlebar.onmouseup=null;
        this.Control.onmouseup=null;
        this.Titlebar.ondblclick=null;
        this.BlurredOverlay.onmousedown=null;
        this.ResizeBottomRight.onmousedown=null;
        this.ResizeRight.onmousedown=null;
        this.ResizeBottom.onmousedown=null;
        this.ResizeBottomLeft.onmousedown=null;
        this.ResizeLeft.onmousedown=null;
        this.ResizeTopLeft.onmousedown=null;
        this.ResizeTop.onmousedown=null;
        this.ResizeTopRight.onmousedown=null;
        this.ResizeBottomRight.onmouseover=null;
        this.ResizeRight.onmouseover=null;
        this.ResizeBottom.onmouseover=null;
        this.ResizeBottomLeft.onmouseover=null;
        this.ResizeLeft.onmouseover=null;
        this.ResizeTopLeft.onmouseover=null;
        this.ResizeTop.onmouseover=null;
        this.ResizeTopRight.onmouseover=null;
        
        
        //Clean up
        this.TaskbarButton.Remove();
        delete this.TaskbarButton;
        this.TaskbarButton = null;
        
        this.Icons.length=0;
        this.Icons = null;
        
        this.Control.Window = null;
        this.Titlebar.Window = null;
        this.ContentIframe.IDCWindow = null;
            
        this.cmdClose.Window = null;
        this.cmdMinimize.Window = null;
        this.cmdMaximize.Window = null;
        this.cmdRestore.Window = null;
        
        this.BlurredOverlay.Window = null;
        
        try
        {
            this.WindowManager.Core.DOM.Purge(this.ContentIframe.contentWindow.document.body);
        } catch(exp) {};
        try
        {
            this.ContentIframe.contentWindow.document.location.href=this.WindowManager.BlankPage;
        } catch(exp) {};
        
        var gc = new Array();
        
        gc.push( this.ContentIframeOuterBox.removeChild(this.ContentIframe));
        gc.push( this.ContentTableTdContent.removeChild(this.ContentIframeOuterBox));
        gc.push( this.TitlebarTableTdIcon.removeChild(this.TitlebarIcon));
        
        gc.push( this.TitlebarTextContainer.removeChild(this.TitlebarText));
        gc.push( this.TitlebarTableTdTitle.removeChild(this.TitlebarTextContainer));
        
        
        gc.push( this.TitlebarTableTr.removeChild(this.TitlebarTableTdControl));
        gc.push( this.TitlebarTableTr.removeChild(this.TitlebarTableTdTitle));
        gc.push( this.TitlebarTableTr.removeChild(this.TitlebarTableTdIcon));
        gc.push( this.TitlebarTableBody.removeChild(this.TitlebarTableTr));
        gc.push( this.TitlebarTable.removeChild(this.TitlebarTableBody));
        gc.push( this.Titlebar.removeChild(this.TitlebarTable));
        
        gc.push( this.ContentTableTdTitle.removeChild(this.Titlebar));
        gc.push( this.ContentTableTdContent.removeChild(this.BlurredOverlay));
        gc.push( this.ContentTableTrContent.removeChild(this.ContentTableTdContent));
        gc.push( this.ContentTableTrTitle.removeChild(this.ContentTableTdTitle));    
        gc.push( this.ContentTableBody.removeChild(this.ContentTableTrContent));
        gc.push( this.ContentTableBody.removeChild(this.ContentTableTrTitle));
        gc.push( this.ContentTable.removeChild(this.ContentTableBody));
        gc.push( this.Control.removeChild(this.ContentTable));
        gc.push( this.Control.removeChild(this.ResizeTopLeft));
        gc.push( this.Control.removeChild(this.ResizeTop));
        gc.push( this.Control.removeChild(this.ResizeTopRight));
        gc.push( this.Control.removeChild(this.ResizeLeft));
        gc.push( this.Control.removeChild(this.ResizeRight));
        gc.push( this.Control.removeChild(this.ResizeBottomLeft));
        gc.push( this.Control.removeChild(this.ResizeBottom));
        gc.push( this.Control.removeChild(this.ResizeBottomRight));
        
        gc.push( this.TitlebarControlButtons.removeChild(this.cmdClose));
        gc.push( this.TitlebarControlButtons.removeChild(this.cmdMaximize));
        gc.push( this.TitlebarControlButtons.removeChild(this.cmdMinimize));
        gc.push( this.TitlebarControlButtons.removeChild(this.cmdRestore));
        
        gc.push( this.Control.removeChild(this.TitlebarControlButtons));
        gc.push( this.ParentObject.removeChild(this.Control));
        
        this.Control.innerHTML='';
        
        for(var i=0;i<gc.length;i++)
        {
            delete gc[i];
            gc[i]=null;
        }
        
        gc.length=0;
        gc=null;
        
        //Clean objects
        delete this.Control;
        this.Control = null;
        delete this.ResizeTopLeft;
        this.ResizeTopLeft = null;
        delete this.ResizeTop;
        this.ResizeTop = null;
        delete this.ResizeTopRight;
        this.ResizeTopRight = null;
        delete this.ResizeLeft;
        this.ResizeLeft = null;
        delete this.ContentOuterBox;
        this.ContentOuterBox = null;
        delete this.ContentInnerBox;
        this.ContentInnerBox = null;
        delete this.ContentIframe;
        this.ContentIframe = null;
        delete this.ResizeRight;
        this.ResizeRight = null;
        delete this.ResizeBottomLeft;
        this.ResizeBottomLeft = null;
        delete this.ResizeBottom ;
        this.ResizeBottom = null;
        delete this.ResizeBottomRight;
        this.ResizeBottomRight = null;
    
    }
    
    this.Document = null;
    this.ParentObject = null;
    
    
    
    for(var i=this.WindowManager.Windows.length-1;i>=0;i--)
    {
        if(this.WindowManager.Windows[i] == this)
        {
            var win = this.WindowManager.Windows.splice(i,1);
            delete win;
            win=null;
            //w = null;
            break;
        }
    }
    
    if(this.WindowManager.isIE)
        CollectGarbage();
};

IDC_Window.prototype.ShowDialog = function(DialogOfWindow, ignoreAnimation)
{
    if(DialogOfWindow) this.DialogOwnerOf=DialogOfWindow;
    
    if(this.DialogOwnerOf)
    {
        this.DialogOwnerOf.IsLocked=true;
        this.DialogOwnerOf.IsLockedBy=this;
    }
    
    if(this.WindowManager.VisualEffects.AnimateOnCreate && !this.Control && !ignoreAnimation)
    {
        //Animation before anything else
        var w1 = this.Width/1.4;
        var h1 = this.Height/1.4;
        var x2 = (this.Width - w1)/2;
        var y2 = (this.Height - h1)/2;
        var me = this;
        this._Show(true);
        this.WindowManager.GhostAnimation(this.Left + x2, this.Top + y2, w1, h1,this.Left, this.Top, this.Width, this.Height, 7, 1, function() { me._Show(); });
    }
    else
        this._Show();
};

IDC_Window.prototype.Show = function(ignoreAnimation)
{
    if(this.DialogOwnerOf)
    {
        this.DialogOwnerOf.IsLocked=false;
        this.DialogOwnerOf.IsLockedBy=null;
        this.DialogOwnerOf=null;
    }
        
    if(this.WindowManager.VisualEffects.AnimateOnCreate && !this.Control && !ignoreAnimation)
    {
        //Animation before anything else
        var w1 = this.Width/1.4;
        var h1 = this.Height/1.4;
        var x2 = (this.Width - w1)/2;
        var y2 = (this.Height - h1)/2;
        var me = this;
        this._Show(true);
        this.WindowManager.GhostAnimation(this.Left + x2, this.Top + y2, w1, h1,this.Left, this.Top, this.Width, this.Height, 7, 1, function() { me._Show(); });
    }
    else
        this._Show();
};

IDC_Window.prototype.CenterOwner = function(Owner)
{
    if(this.WindowState!=1) return;
    
    if(!Owner) Owner = this.DialogOwnerOf;
    if(Owner)
    {
        this.Left = Owner.Left + ((Owner.Width - (this.Width || 400)) / 2);
        this.Top = Owner.Top + ((Owner.Height - (this.Height || 300)) / 2);
        
        if(this.Left + this.Width > this.WindowManager.Desktop.Control.offsetWidth)
            this.Left = this.WindowManager.Desktop.Control.offsetWidth - this.Width;
        
        if(this.Top + this.Height > this.WindowManager.Desktop.Control.offsetHeight - this.WindowManager.Desktop.Taskbar.Control.offsetHeight)
        {
            this.Top = this.WindowManager.Desktop.Control.offsetHeight - this.Height - this.WindowManager.Desktop.Taskbar.Control.offsetHeight;
        }
        
        if(this.Left<0) this.Left=0;
        if(this.Top<0) this.Top=0;
        
        
        if(this.Control)
        {
            this.SetPositionAndSize(this.Left, this.Top);
        }
    }
    else
        this.CenterScreen();
};

IDC_Window.prototype.CenterScreen = function()
{
    if(this.WindowState!=1) return;
    if(this.ParentObject)
    {
        var taskbarHeight = (this.WindowManager.Desktop.Taskbar.Control.offsetHeight);
        
        this.Left = (this.ParentObject.offsetWidth - (this.Width || 400)) / 2;
        this.Top = (this.ParentObject.offsetHeight - taskbarHeight - (this.Height || 300)) / 2;
    }
    else
    {
        var po = this.WindowManager.Desktop ? this.WindowManager.Desktop.Control : this.WindowManager.ParentObject;
        if(po)
        {
            this.Left = (po.offsetWidth - (this.Width || 400)) / 2;
            this.Top = (po.offsetHeight - (this.Height || 300)) / 2;
        }
    }
    
    if(this.Left<0) this.Left=0;
    if(this.Top<0) this.Top=0;
        
    if(this.Control)
    {
        this.SetPositionAndSize(this.Left, this.Top);
    }
};

IDC_Window.prototype._Show = function(remainHidden)
{
    if(!this.Document) return;
    if(this.Control)
    {
        this.Control.style.visibility = remainHidden ? 'hidden' : '';
        return;
    }
    
    this.OriginalTitle=this.Name;
    
    //Frame
    this.Control = this.Document.createElement('DIV');
    this.Control.className='IDCWMWindowFrame';
    this.Control.style.left= this.Left || 0;
    this.Control.style.top = this.Top || 0;
    
    this.Control.style.width= this.Width || 400;
    this.Control.style.height= this.Height || 300;
    
    //Inner Objects
    this.ResizeTopLeft = this.Document.createElement('DIV');
    this.ResizeTop = this.Document.createElement('DIV');
    this.ResizeTopRight = this.Document.createElement('DIV');
    this.ResizeLeft = this.Document.createElement('DIV');
    this.Titlebar = this.Document.createElement('DIV');
    this.TitlebarIcon = this.Document.createElement('IMG');
    this.TitlebarTextContainer = this.Document.createElement('DIV');
    this.TitlebarText = this.Document.createElement('FONT');
    this.TitlebarControlButtons = this.Document.createElement('DIV');
    
    this.ResizeRight = this.Document.createElement('DIV');
    this.ResizeBottomLeft = this.Document.createElement('DIV');
    this.ResizeBottom = this.Document.createElement('DIV');
    this.ResizeBottomRight = this.Document.createElement('DIV');
    
    //Titlebar with control, text and icon
    this.TitlebarTable = this.Document.createElement('TABLE');
    this.TitlebarTableBody = this.Document.createElement('TBODY');
    this.TitlebarTableTr = this.Document.createElement('TR');
    this.TitlebarTableTdIcon = this.Document.createElement('TD');
    this.TitlebarTableTdTitle = this.Document.createElement('TD');
    this.TitlebarTableTdControl = this.Document.createElement('TD');
    this.TitlebarTableTdControl.noWrap=true;
    
    //Tabs for titlebar
    this.TitlebarTabs = this.Document.createElement('SPAN');
    this.TitlebarTabs.className='IDCWMWindowFrameTitleBarTabItems';
    
    //Titlebar control buttons
    this.cmdClose = this.Document.createElement('SPAN');
    this.cmdMaximize = this.Document.createElement('SPAN');
    this.cmdMinimize = this.Document.createElement('SPAN');
    this.cmdRestore = this.Document.createElement('SPAN');
    
    this.TitlebarTable.cellPadding=0;
    this.TitlebarTable.cellSpacing=0;
    this.TitlebarTable.border=0;
    this.TitlebarTable.style.width='100%';
    this.TitlebarTable.style.height='100%';
    this.TitlebarTableTdTitle.style.width='100%';
    
    this.TitlebarTableTdIcon.vAlign='top';
    this.TitlebarTableTdTitle.vAlign='top';
    this.TitlebarTableTdControl.vAlign='top';
    
    
    //Title and content to match 100% in height
    
    //COntentTable er ikke 100% i top, når der maximeres i Internet Explorer!!!!
    this.ContentTable = this.Document.createElement('TABLE');
    this.ContentTableBody = this.Document.createElement('TBODY');
    this.ContentTableTrTitle = this.Document.createElement('TR');
    this.ContentTableTrContent = this.Document.createElement('TR');
    this.ContentTableTdTitle = this.Document.createElement('TD');
    this.ContentTableTdContent = this.Document.createElement('TD');
    this.ContentIframeOuterBox = this.Document.createElement('DIV');
    
    this.ContentTableTdTitle.vAlign='top';
    
    this.ContentTable.cellPadding=0;
    this.ContentTable.cellSpacing=0;
    this.ContentTable.border=0;
    this.ContentTable.style.width='100%';
    this.ContentTable.style.height='100%';
    
    //this.ContentTableTdContent.style.height='100%';
    this.ContentTableTdContent.vAlign='top';
    
    //blurred overlay for focus on click
    this.BlurredOverlay = this.Document.createElement('DIV');
    this.BlurredOverlay.style.position='absolute';
    this.BlurredOverlay.style.display='';
    this.BlurredOverlay.style.width=100;
    this.BlurredOverlay.style.height=100;
    //this.BlurredOverlay.style.backgroundColor='red';
    if(this.isIE)
    {
        this.BlurredOverlay.style.backgroundImage='url(' + this.WindowManager.Core.WebsiteUrl + 'images/spacer.gif)';
    } 
    
    //Classes
    this.ResizeTopLeft.className='IDCWMWindowFrameRZTL';
    this.ResizeTop.className='IDCWMWindowFrameRZT';
    this.ResizeTopRight.className='IDCWMWindowFrameRZTR';
    this.ResizeLeft.className='IDCWMWindowFrameRZL';
    this.ResizeRight.className='IDCWMWindowFrameRZR';
    this.ResizeBottomLeft.className='IDCWMWindowFrameRZBL';
    this.ResizeBottom.className='IDCWMWindowFrameRZB';
    this.ResizeBottomRight.className='IDCWMWindowFrameRZBR';
    
    this.Titlebar.className='IDCWMWindowFrameTitleBar';
    this.TitlebarTextContainer.className='IDCWMWindowFrameTitleBarTextContainer';
    this.TitlebarText.className='IDCWMWindowFrameTitleBarText';
    this.TitlebarIcon.className='IDCWMWindowFrameTitleBarIcon';
    this.TitlebarTableTdControl.className='IDCWMWindowFrameTitleBarControlsContainer';
    this.TitlebarControlButtons.className='IDCWMWindowFrameTitleBarControls';
    
    //Titlebar control buttons
    this.cmdClose.className='IDCWMWindowFrameTitleBarControlsButtonClose';
    this.cmdMinimize.className='IDCWMWindowFrameTitleBarControlsMinimize';
    this.cmdMaximize.className='IDCWMWindowFrameTitleBarControlsMaximize';
    this.cmdRestore.className='IDCWMWindowFrameTitleBarControlsRestore';
    
    this.ContentTableTdContent.className='IDCWMWindowOuterTdContentFrame';
    this.ContentIframeOuterBox.className='IDCWMWindowIFrameOuterBox';
            
    //Add borders
    this.Control.style.visibility = remainHidden ? 'hidden' : '';
    
    this.ParentObject.appendChild(this.Control);
    this.Control.appendChild(this.ResizeTopLeft);
    
    this.Control.appendChild(this.ResizeTopRight);
    this.Control.appendChild(this.ResizeLeft);
    
    this.Control.appendChild(this.ResizeRight);
    this.Control.appendChild(this.ResizeBottomLeft);
    this.Control.appendChild(this.ResizeBottom);
    this.Control.appendChild(this.ResizeBottomRight);
    
    
    //add content table
    
    this.Control.appendChild(this.ContentTable);
    this.ContentTable.appendChild(this.ContentTableBody);
    this.ContentTableBody.appendChild(this.ContentTableTrTitle);
    this.ContentTableBody.appendChild(this.ContentTableTrContent);
    this.ContentTableTrTitle.appendChild(this.ContentTableTdTitle);
    this.ContentTableTrContent.appendChild(this.ContentTableTdContent);
    
    //add titlebar
    this.ContentTableTdTitle.appendChild(this.Titlebar);
    this.Titlebar.appendChild(this.TitlebarTable);
    this.TitlebarTable.appendChild(this.TitlebarTableBody);
    this.TitlebarTableBody.appendChild(this.TitlebarTableTr);
    this.TitlebarTableTr.appendChild(this.TitlebarTableTdIcon);
    this.TitlebarTableTr.appendChild(this.TitlebarTableTdTitle);
    this.TitlebarTableTr.appendChild(this.TitlebarTableTdControl);
    
    this.TitlebarTableTdIcon.appendChild(this.TitlebarIcon);
    this.TitlebarTableTdTitle.appendChild(this.TitlebarTextContainer);
    this.TitlebarTextContainer.appendChild(this.TitlebarText);
    
    
    
    //add content
    this.ContentTableTdContent.appendChild(this.BlurredOverlay);
    this.ContentTableTdContent.appendChild(this.ContentIframeOuterBox);
    
    if(this.ContentIframe.parentNode)
    {
        this.ContentIframe=this.ContentIframe.parentNode.removeChild(this.ContentIframe);
        this.ContentIframe.style.display='';
    }
    //Tabs for titlebar
    this.ContentIframeOuterBox.appendChild(this.TitlebarTabs);
    this.ContentIframeOuterBox.appendChild(this.ContentIframe);

    this.Control.appendChild(this.ResizeTop);
    this.Control.appendChild(this.TitlebarControlButtons);  
    
    this.TitlebarControlButtons.appendChild(this.cmdMinimize);
    this.TitlebarControlButtons.appendChild(this.cmdRestore);
    this.TitlebarControlButtons.appendChild(this.cmdMaximize);
    this.TitlebarControlButtons.appendChild(this.cmdClose);
    
    
    

    //EVENT HANDLERS
//
    this.Control.Window=this;
    this.Titlebar.Window = this;
    this.ResizeBottomRight.Window = this;
    this.ResizeRight.Window = this;
    this.ResizeBottom.Window = this;
    this.ResizeBottomLeft.Window = this;
    this.ResizeLeft.Window = this;
    this.ResizeTopLeft.Window = this;
    this.ResizeTop.Window = this;
    this.ResizeTopRight.Window = this;
    this.TitlebarIcon.Window = this;
    
    this.cmdClose.Window = this;
    this.cmdMinimize.Window = this;
    this.cmdMaximize.Window = this;
    this.cmdRestore.Window = this;
    
    this.BlurredOverlay.Window = this;
        
    this.TitlebarIcon.onerror = function() { this.style.visibility='hidden'; };
    this.TitlebarIcon.onload = function() { this.style.visibility='visible'; };
    this.TitlebarIcon.ondblclick = function() { if(this.Window.Closeable && !this.Window.IsLocked) this.Window.Close(); };
    
    this.cmdClose.onmouseover = function() 
                                    { 
                                        if(this.Window.Closeable && !this.Window.IsLocked) 
                                        {
                                            if(!this.Window.Minimizeable && !this.Window.Maximizeable)
                                                this.parentNode.className='IDCWMWindowFrameTitleBarControlsCloseOnlyHover' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); 
                                            else
                                                this.parentNode.className='IDCWMWindowFrameTitleBarControlsButton3Hover' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); 
                                        }
                                    };
    this.cmdClose.onmouseout = function(){ 
                                        if(this.Window.Closeable && !this.Window.IsLocked) 
                                        {
                                            if(!this.Window.Minimizeable && !this.Window.Maximizeable)
                                                this.parentNode.className='IDCWMWindowFrameTitleBarControlsCloseOnly' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); 
                                            else
                                                this.parentNode.className='IDCWMWindowFrameTitleBarControls' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); 
                                        }
                                    };
    
    this.cmdClose.onclick = function() { if(this.Window.Closeable && !this.Window.IsLocked) this.Window.Close(); };
    
    this.cmdMinimize.onmouseover = function() { if(this.Window.Minimizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControlsButton1Hover' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdMinimize.onmouseout = function() { if(this.Window.Minimizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControls' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdMinimize.onclick = function() { if(this.Window.Minimizeable && !this.Window.IsLocked) this.Window.Minimize(); };
    
    this.cmdMaximize.onmouseover = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControlsButton2Hover' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdMaximize.onmouseout = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControls' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdMaximize.onclick = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.Window.Maximize(); };
    
    this.cmdRestore.onmouseover = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControlsButton2Hover' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdRestore.onmouseout = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.parentNode.className='IDCWMWindowFrameTitleBarControls' + (this.Window.IsFocused ? 'Focused' : '') + (this.Window.WindowState==1 ? '' : 'Maximized'); };
    this.cmdRestore.onclick = function() { if(this.Window.Maximizeable && !this.Window.IsLocked) this.Window.Restore(); };
    
    this.Titlebar.onmousedown=function() { this.Window.__MouseDown('mv'); };
    this.Titlebar.onmouseover=function() { this.Window.__SetWindowCursor(this,'mv'); };
    this.Titlebar.onmousemove=function() {  this.Window.__MouseBeginMove(); };
    this.Titlebar.onmouseup=function() { this.Window.__MouseUp(); };
    this.Control.onmouseup=function() { this.Window.__MouseUp(); };
    
    this.Titlebar.ondblclick=function() { if(this.Window.WindowState==1) this.Window.cmdMaximize.onclick(); else this.Window.cmdRestore.onclick(); };
    
    this.BlurredOverlay.NewMouseX = 0;
    this.BlurredOverlay.NewMouseY = 0;
    
    this.BlurredOverlay.StartMouseX = 0;
    this.BlurredOverlay.StartMouseY = 0;
    
    this.BlurredOverlay.CoreStartMouseX = 0;
    this.BlurredOverlay.CoreStartMouseY = 0;
    
    this.BlurredOverlay.onmousedown = function() { this.Window.Focus(); };
    this.BlurredOverlay.onmousemove = function() { 
                if(!this.StartMouseX) this.StartMouseX = this.Window.WindowManager.Core.Mouse.X;
                if(!this.StartMouseY) this.StartMouseY = this.Window.WindowManager.Core.Mouse.Y;
                
                if(this.CoreOut)
                {
                    this.CoreOut.Mouse.X = this.CoreStartMouseX + (this.Window.WindowManager.Core.Mouse.X - this.StartMouseX);
                    this.CoreOut.Mouse.Y = this.CoreStartMouseY + (this.Window.WindowManager.Core.Mouse.Y - this.StartMouseY);
                }
                };
    
    this.ResizeBottomRight.onmousedown=function() { this.Window.__MouseDown('rbr'); this.Window.__MouseBeginMove(); };
    this.ResizeRight.onmousedown=function() { this.Window.__MouseDown('rr'); this.Window.__MouseBeginMove(); };
    this.ResizeBottom.onmousedown=function() { this.Window.__MouseDown('rb'); this.Window.__MouseBeginMove(); };
    this.ResizeBottomLeft.onmousedown=function() { this.Window.__MouseDown('rbl'); this.Window.__MouseBeginMove(); };
    this.ResizeLeft.onmousedown=function() { this.Window.__MouseDown('rl'); this.Window.__MouseBeginMove(); };
    this.ResizeTopLeft.onmousedown=function() { this.Window.__MouseDown('rtl'); this.Window.__MouseBeginMove(); };
    this.ResizeTop.onmousedown=function() { this.Window.__MouseDown('rt'); this.Window.__MouseBeginMove(); };
    this.ResizeTopRight.onmousedown=function() { this.Window.__MouseDown('rtr'); this.Window.__MouseBeginMove(); };                                

    this.ResizeBottomRight.onmouseover=function() { this.Window.__SetWindowCursor(this,'rbr'); };
    this.ResizeRight.onmouseover=function() { this.Window.__SetWindowCursor(this,'rr'); };
    this.ResizeBottom.onmouseover=function() { this.Window.__SetWindowCursor(this,'rb'); };
    this.ResizeBottomLeft.onmouseover=function() { this.Window.__SetWindowCursor(this,'rbl'); };
    this.ResizeLeft.onmouseover=function() { this.Window.__SetWindowCursor(this,'rl'); };
    this.ResizeTopLeft.onmouseover=function() { this.Window.__SetWindowCursor(this,'rtl'); };
    this.ResizeTop.onmouseover=function() { this.Window.__SetWindowCursor(this,'rt'); };
    this.ResizeTopRight.onmouseover=function() { this.Window.__SetWindowCursor(this,'rtr'); };
        
    this.SetTitle(this.Name || '');
    this.SetSize(this.Width, this.Height);
    
    this.BlurredOverlay.style.width='100%';
   
    this.SetWindowIcons(this.Icons);
   
    //Create taskbar item
    var icon = '';
    var ctxicon = '';
    if(this.Icons)
    {
        if(this.Icons.length>0)
            icon =this.Icons[this.Icons.length-1];
        
        ctxicon = this.Icons[0];
    }
    this.TaskbarButton=this.WindowManager.Desktop.Taskbar.CreateTaskbarButton(icon, ctxicon, this.Name, null, null, null, null, this); 
   
    this.Focus();
    this.SetSize(this.Width, this.Height);
    this.RefreshClasses();
    this.RefreshControlButtons();
    
    //this.FixTabHandler();
    
    if(this.WindowState==2)
    {
        this.Maximize();
    }
    else if(this.WindowState==0)
    {
        this.Minimize();
    }
    
};


IDC_Window.prototype.__SetWindowCursor = function(object, WindowEvent)
{
    WindowEvent = this.Resizeable ? WindowEvent : 'mv';
    
    if(WindowEvent=='mv')
        object.style.cursor='pointer';
        
    else if(WindowEvent=='rbr' || WindowEvent=='rtl')
        object.style.cursor='nw-resize';
        
    else if(WindowEvent=='rb' || WindowEvent=='rt')
        object.style.cursor='s-resize';
        
    else if(WindowEvent=='rbl' || WindowEvent=='rtr')
        object.style.cursor='ne-resize';

    else if(WindowEvent=='rl' || WindowEvent=='rr')
        object.style.cursor='e-resize';
        
    else
        object.style.cursor='arrow';
};

IDC_Window.prototype.SetWindowOpacity = function(Opacity)
{
    this.Opacity = Opacity;
    if(this.isIE)
        this.Control.style.filter = 'alpha(opacity=' + this.Opacity + ')';
    else
        this.Control.style.opacity = this.Opacity/100;
    
};

IDC_Window.prototype.SetWindowIcons = function(Icons)
{
    if(Icons!=null)
        this.Icons = Icons;
    else
        this.Icons = new Array();
        
    if(this.Icons.constructor.toString().indexOf('Array')==-1)
    {
        this.Icons = new Array();
        if(Icons!='' && Icons!=null)
            this.Icons.push(Icons);
    }
    
    this.TitlebarIcon.src = this.Icons.length>0 ? this.Icons[0] : 'images/spacer.gif';
};

IDC_Window.prototype.GetInnerWidth = function() {
    return this.ContentIframe.offsetWidth - 2;
    try
    {
        return this.isIE ? this.ContentIframe.contentWindow.document.body.offsetWidth : this.ContentIframe.contentWindow.innerWidth;
    }
    catch(ex)
    {
        return this.ContentIframe.offsetWidth;
    }
};

IDC_Window.prototype.GetInnerHeight = function() {
    return this.ContentIframe.offsetHeight - 2;
    try
    {
        return this.isIE ? this.ContentIframe.contentWindow.document.body.offsetHeight : this.ContentIframe.contentWindow.innerHeight;
    }
    catch(ex)
    {
        return this.ContentIframe.offsetHeight - 2;
    }
};


IDC_Window.prototype.GetInnerDimensions = function()
{
    var retval = new Array();
    retval[0] = this.GetInnerWidth();
    retval[1] = this.GetInnerHeight();
    
    return retval;
};

IDC_Window.prototype.RefreshControlButtons = function()
{
    if(!this.Control) return;

    if(!this.Closeable && !this.Minimizeable && !this.Closeable && !this.Maximizeable && this.Name=='')
    {
        var changed = this.ContentTableTdTitle.style.display!='none';
        this.ContentTableTdTitle.style.display='none';
        this.TitlebarControlButtons.style.display='none';
        this.TitlebarTextContainer.style.display='none';
        this.ContentTableTrTitle.style.display='none';
        
        if(this.TaskbarButton)
        {
            this.TaskbarButton.Control.style.display='none';
        }
        
                
        if(changed)
        {
            this.SetSize(this.Width, this.Height);
            this.RefreshClasses();
        }
        return;
    }
    else
    {
        this.ContentTableTdTitle.style.display='';
        this.TitlebarControlButtons.style.display='';
        this.TitlebarTextContainer.style.display='';
        this.ContentTableTrTitle.style.display='';
        if(this.TaskbarButton)
        {
            this.TaskbarButton.Control.style.display='';
        }
    }
    
    //Classes
    var addCls = '';
    if(this.IsFocused && this.WindowState == 2)
        addCls = 'FocusedMaximized';
    else if(this.IsFocused && this.WindowState == 1)
        addCls = 'Focused';
    else if(!this.IsFocused && this.WindowState == 2)
        addCls = 'Maximized';
    else if(this.WindowState == 0)
        addCls = 'Minimized';
        
    this.cmdClose.className = (this.Closeable ? 'IDCWMWindowFrameTitleBarControlsButtonClose' : 'IDCWMWindowFrameTitleBarControlsButtonCloseDisabled')  + addCls;
    this.cmdMinimize.className = (this.Minimizeable ? 'IDCWMWindowFrameTitleBarControlsMinimize' : 'IDCWMWindowFrameTitleBarControlsMinimizeDisabled')  + addCls;
    this.cmdMaximize.className = (this.Maximizeable ? 'IDCWMWindowFrameTitleBarControlsMaximize' : 'IDCWMWindowFrameTitleBarControlsMaximizeDisabled')  + addCls;
    this.cmdRestore.className = (this.Maximizeable ? 'IDCWMWindowFrameTitleBarControlsRestore' : 'IDCWMWindowFrameTitleBarControlsRestoreDisabled')  + addCls;
    
    //Close only
    if(!this.Minimizeable && !this.Maximizeable)
    {
        this.TitlebarControlButtons.className='IDCWMWindowFrameTitleBarControlsCloseOnly' + addCls;
        this.cmdMinimize.style.display='none';
        this.cmdMaximize.style.display='none';
        this.cmdRestore.style.display='none';
    }
    else
    {
        this.TitlebarControlButtons.className='IDCWMWindowFrameTitleBarControls' + addCls;
        this.cmdMinimize.style.display='inline-block';
        if(this.WindowState==1)
        {
            this.cmdMaximize.style.display='inline-block';
            this.cmdRestore.style.display='none';
        }
        else
        {
            this.cmdRestore.style.display='inline-block';
            this.cmdMaximize.style.display='none';
        }
    }
    
};

IDC_Window.prototype.SetTitle = function(Title)
{
    this.Name = Title || '';
    
    if(!this.Control) return;
    if(this.isIE)
    {
        this.TitlebarText.innerText = Title;
    }
    else
    {
        this.TitlebarText.textContent = Title;
    }   
    
    if(this.TaskbarButton)
    {
        this.TaskbarButton.Name = Title;
        this.TaskbarButton.TextControl.innerHTML = Title;
        this.TaskbarButton.Control.title = Title;
    }
        
    this.TitlebarText.title=Title;
    
    this.TitlebarTabs.style.marginLeft = this.TitlebarText.offsetWidth + this.TitlebarIcon.offsetWidth;
};

IDC_Window.prototype.__MouseDown = function (eventToUse) {
    if (this.IsLocked) {
        if (this.IsLockedBy) {
            this.IsLockedBy.FlashMe(6);
        }
        return;
    }

    if (this.MoveFromCenterWindow) {
        this.MoveFromCenterWindow = false;
        this.BaseMouseX = this.Control.offsetWidth / 2; // this.Control.offsetLeft;
        this.BaseMouseY = this.Control.offsetHeight / 2; //this.Control.offsetTop;

        this.BaseMouseW = 0;
        this.BaseMouseH = 0;

        this.BaseOffsetW = this.Control.offsetWidth;
        this.BaseOffsetH = this.Control.offsetHeight - this.ContentTableTdTitle.offsetHeight;
    }
    else {
        this.BaseMouseX = this.WindowManager.Core.Mouse.X - this.Control.offsetLeft;
        this.BaseMouseY = this.WindowManager.Core.Mouse.Y - this.Control.offsetTop;

        this.BaseMouseW = this.WindowManager.Core.Mouse.X - this.Control.offsetWidth;
        this.BaseMouseH = this.WindowManager.Core.Mouse.Y - this.Control.offsetHeight + this.ContentTableTdTitle.offsetHeight;

        this.BaseOffsetW = this.Control.offsetWidth;
        this.BaseOffsetH = this.Control.offsetHeight - this.ContentTableTdTitle.offsetHeight;
    }

    this.WindowManager.CurrentWindowEvent = this.Resizeable ? eventToUse : 'mv';
    this.WindowManager.SetOverlayCursor();
    this.Focus();

    this.WindowManager.IsMouseDown = true;
};
IDC_Window.prototype.__MouseUp = function()
{    
    this.WindowManager.CurrentWindowEvent=null;
    this.WindowManager.IsMouseDown = false; 
    this.WindowManager.__TransparentOverlay.style.display='none';   
};
IDC_Window.prototype.__MouseBeginMove = function()
{
    if(this.WindowManager.IsMouseDown)
        this.WindowManager.__TransparentOverlay.style.display='';        
};


IDC_Window.prototype.MoveTo = function(x, y)
{
    this.Control.style.left = x;
    this.Control.style.top = y;
    
    this.Left=x;
    this.Top=y;
};

IDC_Window.prototype.SetPositionAndSize = function(x, y, w, h, ignoreMinAndMax)
{
    //make sure w and h is not less than minimum and maximum values
    if(!ignoreMinAndMax)
    {
        if(w<this.MinWidth)
        {
            if(x) x-=(this.MinWidth-w);
            w=this.MinWidth;
        }           
        if(h<this.MinHeight)
        {
            if(y) y-=(this.MinHeight-h);
            h=this.MinHeight;
        }
        
        if(w>this.MaxWidth && this.MaxWidth>this.MinWidth)
        {
            if(x) x-=(this.MaxWidth-w);
            w=this.MaxWidth;
        }
                   
        if(h>this.MaxHeight && this.MaxHeight>this.MinHeight)
        {
            if(y) y-=(this.MaxHeight-h);
            h=this.MaxHeight;
        }
    }
    
    this.MoveTo(x || this.Control.offsetLeft,y || this.Control.offsetTop);
    this.SetSize(w,h, ignoreMinAndMax);
};

IDC_Window.prototype.SetSize = function(w, h, ignoreMinAndMax)
{
    if(this.WindowState!=1) return;
    
    if(!ignoreMinAndMax)
    {
        //make sure w and h is not less than minimum and maximum values
        if(w<this.MinWidth)
            w=this.MinWidth;
                    
        if(h<this.MinHeight)
            h=this.MinHeight;

        if(w>this.MaxWidth && this.MaxWidth>this.MinWidth)
            w=this.MaxWidth;
                    
        if(h>this.MaxHeight && this.MaxHeight>this.MinHeight)
            h=this.MaxHeight;
    }

    if(!w) w = this.Width;
    if(!h) h = this.Height;
    
    this.Width=w;
    this.Height=h;
    
    this.Control.style.width=w + 'px';
    this.Control.style.height=(h + this.ContentTableTdTitle.offsetHeight) + 'px';
    
    this.TitlebarTable.style.widht=w;
    this.TitlebarTextContainer.style.width = (w - (this.TitlebarTableTdIcon.offsetWidth + this.TitlebarTableTdControl.offsetWidth)) + 'px';
    if(this.isIE)
    {
        this.ContentIframe.style.height = h + 'px'; // - this.ContentTableTdTitle.offsetHeight;
        //this.BlurredOverlay.style.height='100%';
        this.BlurredOverlay.style.height=h + 'px';
    }
    else
    {
        this.ContentIframeOuterBox.style.height = h + 'px'; // - this.ContentTableTdTitle.offsetHeight;
        this.BlurredOverlay.style.height=this.ContentIframeOuterBox.style.height + 'px';
    }

    this.BlurredOverlay.style.height = h + 'px';
    this.BlurredOverlay.style.width = w + 'px';
        
};

IDC_Window.prototype.FlashMe = function(count)
{
    if(this.IsLockedBy)
    {
        this.IsLockedBy.FlashMe(count);
        return;
    }
    
    count--;
    if(count>0)
    {
        if(!this.IsFocused)
            this.Focus();
        else
            this.Blur();
            
        var me = this;
        setTimeout(function() { me.FlashMe(count); me=null; },75);
    }
    else
    {
        this.Focus();
    }
};

IDC_Window.prototype.Focus = function()
{
    if(!this.Control) return;
    if(this.IsLocked)
    {
        if(this.IsLockedBy)
        {
            this.IsLockedBy.FlashMe(6);
        }
        return;
    }
    
    this.WindowManager.WindowFocusHistory.push(this);
    
    if(this.WindowState==0)
    {
        this.Restore();
    }
    
    if(!this.IsFocused)
    {
        if(this.WindowManager.CurrentWindow)
        {
            this.WindowManager.CurrentWindow.Blur();
        }
        this.IsFocused=true;
        
        this.WindowManager.TopWindowIndex++;
        this.Control.style.zIndex = (this.AlwaysOnTop ? this.WindowManager.AlwaysOnTopWindowIndex : 0) + this.WindowManager.TopWindowIndex;
        this.WindowManager.CurrentWindow = this;
        
        this.RefreshClasses();
        this.TaskbarButton.Focus();
        
        if(this.WindowState==1) this.SetSize(this.Width, this.Height);
    }
    this.BlurredOverlay.style.display='none';
    if(this.WindowManager.ContextMenuManager) this.WindowManager.ContextMenuManager.Hide();
    
    if(this.OnFocus) this.WindowManager.CurrentWindow.OnFocus(this);
    if(this.WindowManager.OnWindowFocus) this.WindowManager.OnWindowFocus(this);
    
    if(this.ContentIframe)
    {
        try
        {
            if(this.ContentIframe.contentWindow)
            {
                this.ContentIframe.contentWindow.focus();
                if(this.ContentIframe.contentWindow.document)
                {
                    if(this.ContentIframe.contentWindow.document.body)
                        this.ContentIframe.contentWindow.document.body.focus();
                }
            }
        }
        catch(err)
        {}
    }
};

IDC_Window.prototype.Blur = function()
{
    if(!this.Control) return;
    
    this.IsFocused=false;
    this.BlurredOverlay.style.display='';
    this.WindowManager.CurrentWindow = null;
    
    this.RefreshClasses();
    this.TaskbarButton.Blur();
    
    if(this.WindowState==1) this.SetSize(this.Width, this.Height);
    if(this.WindowManager.ContextMenuManager) this.WindowManager.ContextMenuManager.Hide();
    
    if(this.OnBlur) this.WindowManager.CurrentWindow.OnBlur(this);
    if(this.WindowManager.OnWindowBlur) this.WindowManager.OnWindowBlur(this);
};

IDC_Window.prototype.RefreshClasses = function()
{
    if(!this.Control) return;
    
    //Classes
    var addCls = '';
    if(this.IsFocused && this.WindowState == 2)
        addCls = 'FocusedMaximized';
    else if(this.IsFocused && this.WindowState == 1)
        addCls = 'Focused';
    else if(!this.IsFocused && this.WindowState == 2)
        addCls = 'Maximized';
    else if(this.WindowState == 0)
        addCls = 'Minimized';
    
    this.Control.className='IDCWMWindowFrame' + addCls;
    
    this.ResizeTopLeft.className='IDCWMWindowFrameRZTL' + addCls;
    this.ResizeTop.className='IDCWMWindowFrameRZT' + addCls;
    this.ResizeTopRight.className='IDCWMWindowFrameRZTR' + addCls;
    this.ResizeLeft.className='IDCWMWindowFrameRZL' + addCls;
    this.ResizeRight.className='IDCWMWindowFrameRZR' + addCls;
    this.ResizeBottomLeft.className='IDCWMWindowFrameRZBL' + addCls;
    this.ResizeBottom.className='IDCWMWindowFrameRZB' + addCls;
    this.ResizeBottomRight.className='IDCWMWindowFrameRZBR' + addCls;
    
    this.Titlebar.className='IDCWMWindowFrameTitleBar' + addCls;
    this.TitlebarTextContainer.className='IDCWMWindowFrameTitleBarTextContainer' + addCls;
    this.TitlebarText.className='IDCWMWindowFrameTitleBarText' + addCls;
    this.TitlebarIcon.className='IDCWMWindowFrameTitleBarIcon' + addCls;
       
    this.TitlebarTableTdControl.className='IDCWMWindowFrameTitleBarControlsContainer' + addCls;
    this.TitlebarControlButtons.className='IDCWMWindowFrameTitleBarControls' + addCls;
    
    //Titlebar control buttons
    this.RefreshControlButtons();
    
    this.ContentTableTdContent.className='IDCWMWindowOuterTdContentFrame' + addCls;
    this.ContentIframeOuterBox.className='IDCWMWindowIFrameOuterBox' + addCls;
    this.ContentIframe.className='IDCWMWindowIFrame' + addCls;
};

IDC_Window.prototype.SetCloseable = function(Value)
{
    this.Closeable=Value;
    this.RefreshControlButtons();
};

IDC_Window.prototype.SetMinimizeable = function(Value)
{
    this.Minimizeable=Value;
    this.RefreshControlButtons();
};

IDC_Window.prototype.SetMaximizeable = function(Value)
{
    this.Maximizeable=Value;
    this.RefreshControlButtons();
};

IDC_Window.prototype.SetResizeable = function(Value)
{
    this.Resizeable=Value;
    this.RefreshControlButtons();
};
    
IDC_Window.prototype.SetAlwaysOnTop = function(Value)
{
    if(this.AlwaysOnTop == Value) return;
    
    this.AlwaysOnTop=Value;
    
    if(this.AlwaysOnTop)
        this.Control.style.zIndex = parseInt(this.Control.style.zIndex) + this.WindowManager.AlwaysOnTopWindowIndex;
    else
        this.Control.style.zIndex = parseInt(this.Control.style.zIndex) - this.WindowManager.AlwaysOnTopWindowIndex;   
};

IDC_Window.prototype.__OnMouseUp = function()
{
    this.WindowManager.IsMouseDown = false;
    this.WindowManager.__TransparentOverlay.style.display = 'none';
};

IDC_Window.prototype.Restore = function()
{

    this.WindowState=this.OldWindowState || 1;
    this.OldWindowState=1;
    this.RefreshClasses();
    
    if(this.WindowState==1)
        this.SetPositionAndSize(this.Left,this.Top,this.Width,this.Height);
        
    if(this.OnRestore) this.OnRestore(this, this.Left,this.Top,this.GetInnerWidth(),this.GetInnerHeight());
    if(this.WindowManager.OnWindowRestore) this.WindowManager.OnWindowRestore(this, this.Left,this.Top,this.GetInnerWidth(),this.GetInnerHeight());
};

IDC_Window.prototype.Maximize = function()
{
    this.OldWindowState=1;
    this.WindowState=2;
    this.RefreshClasses();
    
    this.Control.style.width='';
    this.Control.style.height='';
    this.Control.style.left='';
    this.Control.style.top='';
    
    var w = this.Control.offsetWidth;
    var h = this.Control.offsetHeight;
    
    this.TitlebarTable.style.widht='100%'; // 
    this.TitlebarTextContainer.style.width = '100%'; // (w - (this.TitlebarTableTdIcon.offsetWidth + this.TitlebarTableTdControl.offsetWidth));
    if(this.isIE)
    {
        this.ContentIframe.style.height = '100%'; // h - this.ContentTableTdTitle.offsetHeight;
        this.BlurredOverlay.style.height='100%';
    }
    else
    {
        this.ContentIframeOuterBox.style.height = '100%'; // h - this.ContentTableTdTitle.offsetHeight;
        this.BlurredOverlay.style.height='100%'; // this.ContentIframeOuterBox.style.height;
    }   
    this.TitlebarTextContainer.style.width = '100%';
    
    if(this.OnMaximize) this.OnMaximize(this, this.Left,this.Top,this.GetInnerWidth(),this.GetInnerHeight());
    if(this.WindowManager.OnWindowMaximize) this.WindowManager.OnWindowMaximize(this, this.Left,this.Top,this.GetInnerWidth(),this.GetInnerHeight());
                                                    
    
};

IDC_Window.prototype.Minimize = function()
{
    this.OldWindowState = this.WindowState;
    this.WindowState=0;
    this.RefreshClasses();
    
    if(this.OnMinimize) this.OnMinimize(this);
    if(this.WindowManager.OnWindowMinimize) this.WindowManager.OnWindowMinimize(this);
};

var IDC_Window_MessageBoxButton = function(text, callbackValue)
{
    this.Text = text;
    this.CallbackValue = callbackValue;
};

IDC_Window.prototype.MessageBox = function(Text, Title, Icon, Buttons, CallBack)
{
    var htmlbtns = '';
    
    for(var i=0;i<Buttons.length;i++)
    {
        htmlbtns += (htmlbtns=='' ? '':'&nbsp;') + '<input onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="' + Buttons[i].Text + '" onclick="window.frameElement.IDCWindow.Close("' + Buttons[i].CallbackValue + '");">';
    }
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;">' + htmlbtns + '</td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.OnClose = CallBack;
    
    win.Icons = this.Icons.slice(0,this.Icons.length);

    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.ShowDialog();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_Window.prototype.PickDateAndTime = function (Question, Title, Date, Time, Icon, CallBack) {

    var dtHtml = '<table border="0" cellpadding="0" cellspacing="0">' +
            '<tr>' +
                '<td style="padding-right:4px;">Dato:</td>' +
                '<td><input class="designerTextbox" value="' + (Date != null ? Date : '') + '" style="width:90px;height:' + (this.WindowManager.isIE ? 18 : 20) + 'px;" type="text" name="dtPicker" id="dtPicker"></td>' +
                '<td><DIV class="designerDatePickerButton" onmouseover="this.className=\'designerDatePickerButtonHover\';" onmouseout="this.className=\'designerDatePickerButton\';" onclick="pickDateToControl(\'dtPicker\');"></DIV></td>' +
                '<td style="padding-left:12px;padding-right:4px;">Klokken:</td>' +
                '<td><input type="text" name="dtTime" value="' + (Time != null ? Time : '') + '" id="dtTime" class="designerTextbox" style="width:40px;height:' + (this.WindowManager.isIE ? 18 : 20) + 'px;"></td>' +
            '</tr>' +
            '<tr>' +
            '<td><i></i></td>' +
            '<td colspan="2" align="center"><i>dd-mm-åååå</i></td>' +
            '<td></td>' +
            '<td align="center"><i>tt:mm</i></td>' + 
            '</tr>' +
            '</table>';

    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + 'styles/styles.css" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript" src="' + this.WindowManager.Core.WebsiteUrl + 'js/' + this.WindowManager.ScriptVersion + 'js.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.WindowManager.Core.WebsiteUrl + 'js/' + this.WindowManager.ScriptVersion + 'IDC_Core.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.WindowManager.Core.WebsiteUrl + 'js/' + this.WindowManager.ScriptVersion + 'DatePicker.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.WindowManager.Core.WebsiteUrl + 'js/' + this.WindowManager.ScriptVersion + 'IDC_ContextMenu.js"></script>' +
                '' +
                '<script language="javascript" type="text/javascript">\n' +
                'var datePicker=null;\n' +
                'var localCore = null;\n' +
                'var core = null;\n' +
                'var currentDateObject = null;\n' +

                'function init() { setTimeout(function() {_init();},50); };' +

                'function _init()' +
                '{' +
                    'w = window.frameElement.IDCWindow;' +
                    'core = new IDC_Core(document, document.body); //w.WindowManager.Core;' +
                    'localCore = new IDC_Core(document, document.body);' +
                    'wm = w.WindowManager;' +

                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterScreen();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +

                    'datePicker = new DatePicker(document, parent.document.body, top.window);' +
                    'datePicker.OnPickDate = OnDatePicked;' +

                '};' +

                'function pickDateToControl(id) {' +
                '    currentDateObject = document.getElementById(id);' +
                '' +
                '    var sl = document.body;' +
                '    var pos = localCore.DOM.GetObjectPosition(currentDateObject);' +
                '' +
                '    var dateparts = currentDateObject.value.split(/[.,\/ -]/);' +
                '    var dt = new Date();' +
                '    var day = dt.getDate();' +
                '    var month = dt.getMonth();' +
                '    var year = dt.getFullYear();' +
                '' +
                '    if (dateparts.length == 3) {' +
                '        day = parseInt(dateparts[0], 10);' +
                '        month = parseInt(dateparts[1], 10) - 1;' +
                '        year = parseInt(dateparts[2], 10);' +
                '    }' +
                '' +
                '    datePicker.SetCurrentDate(day, month, year);' +
                '' +
                '    if (w.WindowState != 2)' +
                '        datePicker.Show(pos[0] + w.Left - sl.scrollLeft, pos[1] + w.Top + 74 - sl.scrollTop);' +
                '    else' +
                '        datePicker.Show(pos[0] - sl.scrollLeft, pos[1] + 74 - sl.scrollTop);' +
                '};' +
                '' +
                '    function OnDatePicked(value) {' +
                '' +
                '        if (currentDateObject != null) {' +
                '            var y = value.getFullYear().toString();' +
                '            var m = (value.getMonth() + 1).toString();' +
                '            var d = value.getDate().toString();' +
                '' +
                '            if (m.length == 1) m = \'0\' + m;' +
                '            if (d.length == 1) d = \'0\' + d;' +
                '' +
                '            currentDateObject.value = d + \'-\' + m + \'-\' + y;' +
                '        }' +
                '    };' +

                '    function Okay() {' +
                '        var dtime = document.getElementById(\'dtTime\').value.match(/\\d{1,2}:\\d{1,2}/g);' +
                '        var ddate = document.getElementById(\'dtPicker\').value.match(/\\d{1,2}\\-\\d{1,2}\\-\\d{1,4}/g);' +
                '        if(ddate == null || dtime == null) {' +
                '            window.frameElement.IDCWindow.Alert("Dato og tidsformaterne er ikke angivet korrekt.<br><br>Kontroller de indtastede værdier og prøv igen");' +
                '            return;' +
                '        }' +
                '' +
                '       window.frameElement.IDCWindow.Close([document.getElementById(\'dtPicker\').value, document.getElementById(\'dtTime\').value]);' +
                '    };' +

                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + dtHtml + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="Okay();">&nbsp;<input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close(false);"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.OnClose = CallBack;

    win.Icons = this.Icons.slice(0, this.Icons.length);

    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width = 60;
    win.Height = 20;

    //win.HTML = html;
    win.CenterOwner();
    win.ShowDialog();

    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_Window.prototype.Confirm = function(Question, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(true);">&nbsp;<input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close(false);"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';
                
    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.OnClose = CallBack;

    win.Icons = this.Icons.slice(0,this.Icons.length);

    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    //win.HTML = html;
    win.CenterOwner();
    win.ShowDialog();

    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_Window.prototype.Alert = function(Message, Title, Icon, CallBack, Variables)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };' +
                'function _init(noImg)'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    '' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init(true);" align="left" style="margin-right:8px;">' + Message + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close();"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.Variables = Variables;
    win.OnClose = CallBack;
    win.OnKeyDown = function(key, target, e) { if(key==27) win.Close(); };
    
    win.Icons = this.Icons.slice(0,this.Icons.length);

    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.ShowDialog();
    
    //win.ContentIframe.contentWindow.document.open();
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

IDC_Window.prototype.PromptMultiline = function(Message, Text, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Message + '<br><textarea id="txtValue" name="txtValue" style="width:250px;height:150px;font-family:arial;font-size:12px;">' + Text + '</textarea></td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(txtValue.value);">&nbsp;<input id="cmdCancel" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close();"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.OnClose = CallBack;
    win.Icons = this.Icons.slice(0,this.Icons.length);;
    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.ShowDialog();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

IDC_Window.prototype.Prompt = function(Message, Text, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.WindowManager.Core.WebsiteUrl + this.WindowManager.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.WindowManager.Core.WebsiteUrl + (Icon || (this.Icons.length > 1 ? this.Icons[1] : '')) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Message + '<br><input type="text" id="txtValue" name="txtValue" style="width:100%;" value="' + Text + '"></td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(txtValue.value);">&nbsp;<input id="cmdCancel" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close();"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.WindowManager.BlankPage, this);
    win.OnClose = CallBack;
    win.Icons = this.Icons.slice(0,this.Icons.length);;
    win.Name = Title || this.Name;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.ShowDialog();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

var IDC_Window_VisualEffects = function()
{
    this.AnimateOnClose=false;
    this.AnimateOnCreate=false;
    this.AnimateOnMaximize=false;
    this.AnimateOnMinimize=false;
    this.AnimateOnDesktopBackgroundChange=false;   
};

var IDC_Desktop = function(WindowManager, Document, parentObject)
{
    this.WindowManager = WindowManager;
    this.Document = Document;
    this.ParentObject = parentObject;
    this.isIE = document.all?true:false;
    
    this.Control = Document.createElement('DIV');
    this.OuterControl = this.Control;

    
    this.Control.className='IDCWMDesktop';
    
    this.Background = Document.createElement('DIV');
    this.Background.className='IDCWMDesktopBackgroundImage';
    this.Background.style.position='absolute';
    this.Background.style.display='block';
    this.Background.style.width='100%';
    this.Background.style.height='100%';
    
    this.__BackgroundFadeObject = null;
    
    this.ParentObject.appendChild(this.Control);
    
    var obj = this.OuterControl.parentNode;
    
    while(obj.tagName!='BODY' && obj.tagName!='DIV' && obj.tagName!='P' && obj.tagName!='SPAN')
    {
        obj =obj.parentNode;
    }
    
    this.OuterControl = obj;
    
    this.Control.appendChild(this.Background);

    this.Taskbar = new IDC_Taskbar(this, this.Document, this.Control);
    
    this.Control.Desktop=this;
    
    return this;
};


IDC_Desktop.prototype.SetBackground = function(color, imageUrl, repeat, position)
{
    this.Background.style.backgroundColor=color || '';
    
    if(imageUrl)
    {
        if(imageUrl.toLowerCase().indexOf('https:')==-1)
            imageUrl = this.WindowManager.Core.WebsiteUrl + imageUrl;
    }
    this.Background.style.backgroundImage=imageUrl ? 'url(' + imageUrl + ')' : '';
};

IDC_Desktop.prototype.FadeToColor = function(color, onDoneEvent)
{
    if(!this.__BackgroundFadeObject)
    {
        this.__BackgroundFadeObject = this.Document.createElement('DIV');
        this.__BackgroundFadeObject.style.position='absolute';
        this.__BackgroundFadeObject.style.zIndex=250000;
        this.__BackgroundFadeObject.style.width='100%';
        this.__BackgroundFadeObject.style.height='100%';
        this.__BackgroundFadeObject.style.left=0;
        this.__BackgroundFadeObject.style.top=0;
        this.__BackgroundFadeObject.style.backgroundColor=color;
        if(this.isIE)
            this.__BackgroundFadeObject.style.filter='alpha(opacity=0)';
        else
            this.__BackgroundFadeObject.style.opacity = 0;
            
        this.Control.appendChild(this.__BackgroundFadeObject);
    }
    else
    {
        if(this.isIE)
            this.__BackgroundFadeObject.style.filter='alpha(opacity=100)';
        else
            this.__BackgroundFadeObject.style.opacity = 100;
    }
    
    this.__FadeAnimation(color, true, 0, onDoneEvent);
};

IDC_Desktop.prototype.UnFadeFromColor = function(color, onDoneEvent)
{
    if(!this.__BackgroundFadeObject)
    {
        this.__BackgroundFadeObject = this.Document.createElement('DIV');
        this.__BackgroundFadeObject.style.position='absolute';
        this.__BackgroundFadeObject.style.zIndex=250000;
        this.__BackgroundFadeObject.style.width='100%';
        this.__BackgroundFadeObject.style.height='100%';
        this.__BackgroundFadeObject.style.left=0;
        this.__BackgroundFadeObject.style.top=0;
        this.__BackgroundFadeObject.style.backgroundColor=color;
        if(this.isIE)
            this.__BackgroundFadeObject.style.filter='alpha(opacity=100)';
        else
            this.__BackgroundFadeObject.style.opacity = 100;
            
        this.Control.appendChild(this.__BackgroundFadeObject);
    }
    else
    {
        if(this.isIE)
            this.__BackgroundFadeObject.style.filter='alpha(opacity=100)';
        else
            this.__BackgroundFadeObject.style.opacity = 100;
    }
    
    this.__FadeAnimation(color, false, 100, onDoneEvent);
};

IDC_Desktop.prototype.__FadeAnimation = function(color, fadeIn, opacity, onDoneEvent)
{
    opacity = opacity + (fadeIn ? 7 : -7);
    if(opacity<0) opacity=0;
    if(opacity>100) opacity=100;
   
        if(this.isIE)
            this.__BackgroundFadeObject.style.filter='alpha(opacity=' + opacity + ')';
        else
            this.__BackgroundFadeObject.style.opacity = opacity/10;
    
    if(opacity>0 && opacity<100)
    {
        var me = this;
        setTimeout(function(){ me.__FadeAnimation(color, fadeIn, opacity, onDoneEvent); }, 15);
    }
    else
    {
        if(!fadeIn)
        {
            this.__BackgroundFadeObject.parentNode.removeChild(this.__BackgroundFadeObject);
            delete this.__BackgroundFadeObject;
            this.__BackgroundFadeObject=null;
        }
        if(onDoneEvent)
        {
            try
            {
                onDoneEvent();
            }
            catch(exp) {}
        }
    }
    
};

var IDC_Taskbar = function(Desktop, Document, parentObject)
{

    this.TextPinToTaskbar = null;
    this.TextUnPinToTaskbar = null;
    this.TextCloseWindows = null;
    
    this.AutogroupButtons = false;
    this.AutogroupByIcon = true;
    this.AutogroupByURL = true;
    this.AutogroupByOriginWindow=true;

    this.Desktop = Desktop;
   
    this.ThumbNailURL = this.Desktop.WindowManager.Core.WebsiteUrl;

    
    
    
    this.Document = Document;
    this.ParentObject = parentObject;
    this.isIE = document.all?true:false;
    
    this.EnableGrouping=true;
    this.GroupAlways=true;
    
    this.Control = Document.createElement('DIV');
    this.Control.className='IDCWMDesktopTaskbar';
    this.ParentObject.appendChild(this.Control);
    
    this.NotifyControl = Document.createElement('DIV');
    this.NotifyControl.className='IDCWMDesktopTaskbarNotifyArea';
    this.ParentObject.appendChild(this.NotifyControl);
    
    this.MultipleWindowsOuterControl = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr1 = this.Document.createElement('TR');
    var tr2 = this.Document.createElement('TR');
    var tr3 = this.Document.createElement('TR');
    
    var td11 = this.Document.createElement('TD');
    var td12 = this.Document.createElement('TD');
    var td13 = this.Document.createElement('TD');
    
    var td21 = this.Document.createElement('TD');
    this.MultipleWindowsControl = this.Document.createElement('TD');
    var td23 = this.Document.createElement('TD');
    
    var td31 = this.Document.createElement('TD');
    var td32 = this.Document.createElement('TD');
    var td33 = this.Document.createElement('TD');
    
    this.MultipleWindowsOuterControl.appendChild(tb);
    tb.appendChild(tr1);
    tb.appendChild(tr2);
    tb.appendChild(tr3);
    
    tr1.appendChild(td11);
    tr1.appendChild(td12);
    tr1.appendChild(td13);
    
    tr2.appendChild(td21);
    tr2.appendChild(this.MultipleWindowsControl);
    tr2.appendChild(td23);
    
    tr3.appendChild(td31);
    tr3.appendChild(td32);
    tr3.appendChild(td33);
    
    td11.className='IDCWMDesktopTaskbarMultipleWindowsTL';
    td12.className='IDCWMDesktopTaskbarMultipleWindowsT';
    td13.className='IDCWMDesktopTaskbarMultipleWindowsTR';
    
    td21.className='IDCWMDesktopTaskbarMultipleWindowsL';
    td23.className='IDCWMDesktopTaskbarMultipleWindowsR';
    
    td31.className='IDCWMDesktopTaskbarMultipleWindowsBL';
    td32.className='IDCWMDesktopTaskbarMultipleWindowsB';
    td33.className='IDCWMDesktopTaskbarMultipleWindowsBR';
    
    this.MultipleWindowsOuterControl.cellPadding=0;
    this.MultipleWindowsOuterControl.cellSpacing=0;
    this.MultipleWindowsOuterControl.border=0;
    
    this.MultipleWindowsControl.className='IDCWMDesktopTaskbarMultipleWindows';
    
    this.Buttons = new Array();
    
    //Events
    this.OnPinToTaskbar = null;
    this.OnUnPinToTaskbar = null;
    
};

IDC_Taskbar.prototype.SetAutogroupButtons = function(value)
{
    if(this.AutogroupButtons == value) return;
    this.AutogroupButtons = value;
    for(var i=0;i<this.Buttons.length;i++)
    {
        this.Buttons[i].RefreshStyles();
        //this.Buttons[i].RefreshGrouping();
        
    }
};

IDC_Taskbar.prototype.Hide = function()
{
    this.Control.style.display='none';
    this.NotifyControl.style.display='none';
};

IDC_Taskbar.prototype.Show = function()
{
    this.Control.style.display='';
    this.NotifyControl.style.display='';
};

IDC_Taskbar.prototype.ClearButtons = function()
{
    for(var i=this.Buttons.length-1;i>=0;i--)
    {
        if(!this.Buttons[i].BoundWindow)
            this.Buttons[i].Remove();
    }
};

IDC_Taskbar.prototype.FillMultipleWindowsControl = function(button)
{
    
    //this.Desktop.WindowManager.Core.DOM.Purge(this.MultipleWindowsControl);
    
    while(this.MultipleWindowsControl.hasChildNodes())
    {
        var o = this.MultipleWindowsControl.lastChild;
        
        if(o.OriginalParent)
        {
            o.OriginalParent.appendChild(o);
        }
        else
        {
            this.MultipleWindowsControl.removeChild(o);
            delete o;
        }
        
    }
    this.MultipleWindowsControl.innerHTML='';
    
    if(button.BoundWindow)
        this.AddWindowPreviewButton(button);
    for(var i=0;i<button.ChildGroupedButtons.length;i++)
    {
        if(button.ChildGroupedButtons[i].BoundWindow)
            this.AddWindowPreviewButton(button.ChildGroupedButtons[i]);
    }    
};

IDC_Taskbar.prototype.AddWindowPreviewButton = function(button)
{
    var sp = this.Document.createElement('SPAN');
    if(button.BoundWindow)
        sp.className='IDCWMDesktopTaskbarMultipleWindowsButton' + (button.BoundWindow.IsFocused ? 'Focused' : '');
    else
        sp.className='IDCWMDesktopTaskbarMultipleWindowsButton';
        
    sp.Button = button;
    sp.onclick=function(){ if(this.Button.BoundWindow) this.Button.BoundWindow.Focus(); };
    sp.onmouseover=function() 
                        { 
                            if(this.Button.BoundWindow)
                                this.className='IDCWMDesktopTaskbarMultipleWindowsButtonHover' + (this.Button.BoundWindow.IsFocused ? 'Focused' : ''); 
                        };
    sp.onmouseout=function() 
                        { 
                            if(this.Button.BoundWindow)
                                this.className='IDCWMDesktopTaskbarMultipleWindowsButton' + (this.Button.BoundWindow.IsFocused ? 'Focused' : ''); 
                        };
    
    
    
    this.MultipleWindowsControl.appendChild(sp);
    var spTitle = this.Document.createElement('DIV');
    var spThumbnail = this.Document.createElement('DIV');
    
    
    //spThumbnail.style.backgroundImage = 'url(' + button.LastThumbNailUrl + ')';
    
    spTitle.innerHTML = button.Name;
    spTitle.style.backgroundImage = 'url(' + this.ThumbNailURL + button.BoundWindow.Icons[0] + ')';
    spThumbnail.style.backgroundImage = 'url(' + (button.LastThumbNailUrl || this.ThumbNailURL + button.Icon) + ')';
    //spThumbnail.innerHTML = spThumbnail.style.backgroundImage;
    
    spTitle.className='IDCWMDesktopTaskbarMultipleWindowsButtonTitle';
    spThumbnail.className='IDCWMDesktopTaskbarMultipleWindowsButtonThumbnail';
    
    sp.appendChild(spTitle);
    sp.appendChild(spThumbnail);
    
    //Disabled image thumbnail of window
    return;
    
    var html = '';
    var url = button.BoundWindow.Url;
    try
    {
        html = '<html>' +
            '<head>' + button.BoundWindow.ContentIframe.contentWindow.document.getElementsByTagName('HEAD')[0].innerHTML + '</head>' +
            '<body>' + button.BoundWindow.ContentIframe.contentWindow.document.body.innerHTML + '</body>' +
            '</html>';
        url = button.BoundWindow.ContentIframe.contentWindow.document.location.href;
    }
    catch(exp)
    {
    }
    
    if(!button.Ajax)
        button.Ajax = new this.Desktop.WindowManager.Core.Ajax();
    else
        button.Ajax.Reset();
        
    button.Ajax.requestFile = this.ThumbNailURL + 'htmlToJpeg.aspx?w=' + button.BoundWindow.Control.offsetWidth + '&h=' + button.BoundWindow.Control.offsetHeight + '&nw=' + (spThumbnail.offsetWidth||190) + '&nh=' + (spThumbnail.offsetHeight||99);
    button.Ajax.encVar('html', html);
    button.Ajax.encVar('url', url);
    button.Ajax.encVar('cbId', spThumbnail.id);

    button.Ajax.OnCompletion = function() 
                                    { 
                                        button.__GetThumbNail(spThumbnail);
                                    };
    button.Ajax.OnError = function() 
                                    { 
                                        button.__GetThumbNail(spThumbnail, true);
                                    };
    button.Ajax.OnFail = function() 
                                    { 
                                        button.__GetThumbNail(spThumbnail, true);
                                    };
    button.Ajax.RunAJAX();    
};

IDC_Taskbar.prototype.CreateTaskbarButton = function(icon, contextMenuIcon, name, command, insertBeforeButton, dontRefreshNow, uniqueidentifier, boundWindow)
{
    var b = new IDC_TaskbarButton(this, icon, contextMenuIcon, name, command, insertBeforeButton, dontRefreshNow, uniqueidentifier, boundWindow);
    this.Buttons.push(b);
    return b;
};

var IDC_TaskbarButton = function(Taskbar, icon, contextMenuIcon, name, command, insertBeforeButton, dontRefreshNow, uniqueidentifier, boundWindow)
{
    //Animation
    this.AnimationEnabled=true;
    this.AnimationFrame=0;
    this.AnimationMaxFrames=14;
    this.AnimationFrameHeight=40;
    this.AnimationDelay=35;
    this.AnimationClassBefore='';
    
    this.AnimationMouseOverY=40;
    this.AnimationMouseOverDirection=null;
    
    this.Document = Taskbar.Document;
    this.ParentObject = Taskbar.Control;
    this.Taskbar = Taskbar;
    
    this.CustomHoveringColor='';
    
    this.AJAX = null;
    
    this.LastThumbNailUrl=null;
    
    this.ChildOfGroupButton = null;
    this.ChildGroupedButtons = new Array();
    
    this.Command = command;
    this.WindowUrl = null;
    
    this.Name = name;
    this.Icon = icon ? icon.toString() : '';
    this.ContextMenuIcon = contextMenuIcon ? contextMenuIcon.toString() : '';
    
    this.Uniqueidentifier = uniqueidentifier;
    
    this.Control = this.Document.createElement('SPAN');
    this.Control.title = name;
    
    this.IsFocused = false;
    
    this.AutoBindWithWindows = command == null;
    this.BoundWindow = boundWindow;
    this.CommandButton = null;
    
    this.MouseOverControl = this.Document.createElement('SPAN');
    this.MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver';
    
    this.IconControl = this.Document.createElement('SPAN');
    this.TextControl = this.Document.createElement('SPAN');
    
    this.Control.className='IDCWMDesktopTaskbarButton';
    this.Control.style.whiteSpace='nowrap';
    this.IconControl.className='IDCWMDesktopTaskbarButtonIcon';
    this.IconControl.style.backgroundImage='url(' + this.Taskbar.Desktop.WindowManager.Core.WebsiteUrl  + this.Icon + ')';
    
    this.MouseOverControl.style.display='none';
    
    if(!insertBeforeButton)
        this.ParentObject.appendChild(this.Control);
    else
        this.ParentObject.insertBefore( this.Control,insertBeforeButton.Control);
        
    this.Control.appendChild(this.MouseOverControl);
    this.Control.appendChild(this.IconControl);
    this.Control.appendChild(this.TextControl);
    
    this.TextControl.className='IDCWMDesktopTaskbarButtonTextItem';
    this.TextControl.style.display='none';

    this.Control.Button = this;
    
    this.Control.onclick = function() { this.Button.OnClick(); };
    this.Control.onmouseover = function() { this.Button.OnMouseOver(); };
    this.Control.onmousemove = function() { this.Button.RefreshMouseOver(true); };
    this.Control.onmouseout = function() { this.Button.OnMouseOut(); this.Button.RefreshMouseOver(false); };
    
    this.Control.oncontextmenu = function() { this.Button.OnContextMenu(); };
    
    this.RefreshStyles();
    
    if(!dontRefreshNow)
    {        
        this.RefreshGrouping();
    }
    
    return this;
};

IDC_TaskbarButton.prototype.__GetThumbNail = function(obj, failed)
{
    var result=this.Ajax.Response;
    this.Ajax.Reset();
    this.Ajax.Dispose();
    delete this.Ajax;
    this.Ajax=null;
    
    this.LastThumbNailUrl=result;
    if(failed)
    {
        this.LastThumbNailUrl=this.WindowManager.Core.WebsiteUrl + this.Icon;
        obj.style.backgroundImage='url(' + this.LastThumbNailUrl + ')';
    }
    else
    {
        this.LastThumbNailUrl=result;
        this.__SetLastThumbNail(obj);

    }   
    
    
};

IDC_TaskbarButton.prototype.__SetLastThumbNail = function(obj)
{
    obj.style.backgroundImage='url(' + this.Taskbar.Desktop.WindowManager.Core.WebsiteUrl + 'htmlToJpegGet.aspx?file=' + escape(this.LastThumbNailUrl) + ')';
};

IDC_TaskbarButton.prototype.SetHoveringColor = function(color)
{
    this.CustomHoveringColor = color;
   
    if(this.CustomHoveringColor != null && this.CustomHoveringColor != '')
        this.CustomHoveringColor = '_' + this.CustomHoveringColor;
    else
        this.CustomHoveringColor='';
        
    var cls = this.MouseOverControl.className.split('_')[0];
    
    this.MouseOverControl.className = cls + this.CustomHoveringColor;
    
};

IDC_TaskbarButton.prototype.RefreshMouseOverAnimationEntry = function()
{
    if(this.AnimationMouseOverDirection==0) return;
    if(this.AnimationMouseOverY>2)
    {
        this.AnimationMouseOverY-=3;
        if(this.AnimationMouseOverY<2) this.AnimationMouseOverY=2;
        this.MouseOverControl.style.display='';
        var bgL = (this.Taskbar.Desktop.WindowManager.Core.Mouse.X) - this.Control.offsetLeft - 50; 
        this.MouseOverControl.style.backgroundPosition=bgL + 'px ' + this.AnimationMouseOverY + 'px';
        var me = this;
        setTimeout(function() { me.RefreshMouseOverAnimationEntry(); },15);
    }
    
};
IDC_TaskbarButton.prototype.RefreshMouseOverAnimationExit = function()
{
    if(this.AnimationMouseOverDirection==1) return;
    if(this.AnimationMouseOverY<40)
    {
        this.AnimationMouseOverY+=1;
        var pos = this.MouseOverControl.style.backgroundPosition.split(' ')[0];
        this.MouseOverControl.style.backgroundPosition=pos + ' ' + this.AnimationMouseOverY + 'px';
        var me = this;
        setTimeout(function() { me.RefreshMouseOverAnimationExit(); },15);
    }
    else
    {
        this.AnimationMouseOverY=40;
        this.MouseOverControl.style.display='none';
    }
    
};
IDC_TaskbarButton.prototype.RefreshMouseOver = function(visible)
{
    if(this.ChildOfGroupButton)
    {
        if(this.ChildOfGroupButton.Command == null)
        {
            this.ChildOfGroupButton.RefreshMouseOver(visible);
            return;
        }
    }
    
    if(visible && !this.Command)
    {
        if(this.MouseOverControl.style.display=='none' || this.AnimationMouseOverDirection==0)
        {
            this.AnimationMouseOverDirection=1;
            this.RefreshMouseOverAnimationEntry();
        }
        this.MouseOverControl.style.display='';
        var bgL = (this.Taskbar.Desktop.WindowManager.Core.Mouse.X) - this.Control.offsetLeft - 50; // - (this.MouseOverControl.offsetWidth/2));
        this.MouseOverControl.style.backgroundPosition=bgL + 'px ' + this.AnimationMouseOverY + 'px';
    }
    else
    {
        this.AnimationMouseOverDirection=0;
        this.RefreshMouseOverAnimationExit();
    }
};

IDC_TaskbarButton.prototype.RefreshGrouping = function()
{
    //if(!this.Taskbar.AutogroupButtons) return;
    
    for(var i=this.Taskbar.Buttons.length-1;i>=0;i--)
    {
        var b = this.Taskbar.Buttons[i];
        var burl = b.WindowUrl || '';
        var murl = this.WindowUrl || '';
        
        var mw = b.BoundWindow;
        var bw = this.BoundWindow;
        
        if(bw)
        {
            if(bw.CreatedFromWindowRoot)
                bw = bw.CreatedFromWindowRoot;
        }

        if(mw)
        {
            if(mw.CreatedFromWindowRoot)
                mw = mw.CreatedFromWindowRoot;
        }
      
        if(burl.indexOf('?') >-1) burl = burl.substring(0, burl.indexOf('?'));
        if(murl.indexOf('?') >-1) murl = murl.substring(0, murl.indexOf('?'));
                
        if(
            (
                (this.Taskbar.AutogroupByIcon && b.Icon == this.Icon && b != this && b.ChildOfGroupButton == null && b.AutoBindWithWindows) ||
                (this.Taskbar.AutogroupByURL && murl == burl && burl!='' && b != this && b.ChildOfGroupButton == null && b.AutoBindWithWindows) ||
                (this.Taskbar.AutogroupByOriginWindow && mw == bw && mw != null && b != this && b.AutoBindWithWindows)
            )
            )
        {
           
            if(b.ChildOfGroupButton)
            {
                b.ChildOfGroupButton.ChildGroupedButtons.push(this);
                this.ChildOfGroupButton = b.ChildOfGroupButton;
                b.ChildOfGroupButton.RefreshStyles();
                this.RefreshStyles();
                //Insert just after the primary button
                if(b.ChildOfGroupButton.Control.nextSibling)
                {
                    this.ParentObject.removeChild(this.Control);
                    this.ParentObject.insertBefore(this.Control,b.ChildOfGroupButton.Control.nextSibling);
                }
            }
            else
            {
                b.ChildGroupedButtons.push(this);
                this.ChildOfGroupButton = b;
                b.RefreshStyles();
                this.RefreshStyles();
                //Insert just after the primary button
                if(b.Control.nextSibling)
                {
                    this.ParentObject.removeChild(this.Control);
                    this.ParentObject.insertBefore(this.Control,b.Control.nextSibling);
                }
            }
            break;
        }
    }
};

IDC_TaskbarButton.prototype.OnContextMenu = function()
{
    while(this.Taskbar.MultipleWindowsControl.hasChildNodes())
    {
        this.Taskbar.MultipleWindowsControl.removeChild(this.Taskbar.MultipleWindowsControl.lastChild);
    }
    
    var ctxBack = document.createElement('DIV');
    ctxBack.className='IDCWMDesktopTaskbarContextBackground';
    this.Taskbar.MultipleWindowsControl.appendChild(ctxBack);
    
    var ctx = document.createElement('DIV');
    
    ctxBack.appendChild(ctx);
    
    if(!this.Command)
    {
        
        ctx.Button = this.ChildOfGroupButton || this;
        
        var wi = ctx.Button.BoundWindow;
        if(wi)
        {
            if(wi.IsPinable)
            {
                var ctPin = document.createElement('DIV');
                ctxBack.appendChild(ctPin);
                
                
                ctPin.Button = this.ChildOfGroupButton || this;
                ctPin.innerHTML= this.Taskbar.TextPinToTaskbar || 'Pin to taskbar';
                
                ctPin.className='IDCWMDesktopTaskbarContextMenuPinItem';
                
                ctPin.onmouseover = function() { this.className='IDCWMDesktopTaskbarContextMenuPinItemHover'; };
                ctPin.onmouseout = function() { this.className='IDCWMDesktopTaskbarContextMenuPinItem'; };
                
                ctPin.onclick = function()
                                        {
                                            var w = this.Button.BoundWindow;
                                            
                                            var ic1 = w.Icons.length>0 ?  w.Icons[0] : '';
                                            var ic2 = w.Icons.length>1 ?  w.Icons[1] : '';

                                            var cmd = this.Button.Taskbar.CreateTaskbarButton(ic2,ic1,this.Button.Name,function() { var wn = w.CreateWindow(w.Url); }, this.Button,true );
                                            cmd.AutoBindWithWindows = true;
                                            var anyFocused = false;
                                            
                                            for(var i=0;i<this.Button.ChildGroupedButtons.length;i++)
                                            {
                                                this.Button.ChildGroupedButtons[i].ChildOfGroupButton=cmd;
                                                cmd.ChildGroupedButtons.push(this.Button.ChildGroupedButtons[i]);
                                                if(this.Button.ChildGroupedButtons[i].IsFocused) anyFocused=true;
                                            }
                                            
                                            if(!anyFocused) anyFocused = this.Button.IsFocused;
                                            
                                            //find pin index number
                                            var pinIndex=0;
                                            for(var i=0;i<this.Button.ParentObject.childNodes.length;i++)
                                            {
                                                var b = this.Button.ParentObject.childNodes[i].Button;
                                                if(b.Control.oncontextmenu && b.Command)
                                                    pinIndex++;
                                                    
                                                    if(b.Control == this.Button.Control) 
                                                    {
                                                        pinIndex-=1;
                                                        break;
                                                    }                                                
                                            }
                                            
                                            this.Button.IsFocused = anyFocused;
                                            this.Button.ChildGroupedButtons = new Array();
                                            this.Button.RefreshGrouping();
                                            this.Button.RefreshStyles();
                                            

                                            
                                            if(this.Button.Taskbar.OnPinToTaskbar)
                                            {
                                                var btn = this.Button.ChildOfGroupButton ? this.Button.ChildOfGroupButton : this.Button;
                                                this.Button.Taskbar.OnPinToTaskbar(btn, w, pinIndex);
                                            }
                                            
                                        };
            }
        }
        
        var ctClose = document.createElement('DIV');
        ctClose.Button = this.ChildOfGroupButton || this;
        ctClose.ActualButton = this;
        ctClose.innerHTML= this.Taskbar.TextCloseWindows || 'Close windows';
        
        ctxBack.appendChild(ctClose);
        
        ctClose.className='IDCWMDesktopTaskbarContextMenuCloseItem';
        
        ctClose.onmouseover = function() { this.className='IDCWMDesktopTaskbarContextMenuCloseItemHover'; };
        ctClose.onmouseout = function() { this.className='IDCWMDesktopTaskbarContextMenuCloseItem'; };
        ctClose.onclick = function()
            {
                if(this.Button.ChildGroupedButtons.length>0 && this.Button.Taskbar.AutogroupButtons)
                {
                    for(var i=this.Button.ChildGroupedButtons.length-1;i>=0;i--)
                    {
                        var w = this.Button.ChildGroupedButtons[i].BoundWindow;
                        if(w)
                        {
                            if(w.Closeable && !w.IsLocked) 
                                w.Close();
                            else
                                w.Focus();
                        }
                            
                    }
                    
                    if(this.Button.BoundWindow)
                    {
                        var w = this.Button.BoundWindow;
                        if(w.Closeable && !w.IsLocked) 
                            w.Close();
                        else
                            w.Focus();
                    }
                }
                

                
                if(!this.Button.Taskbar.AutogroupButtons)
                {
                    if(this.ActualButton.BoundWindow)
                    {
                        var w = this.ActualButton.BoundWindow;
                        if(w.Closeable && !w.IsLocked) 
                            w.Close();
                        else
                            w.Focus();
                    }
                }
            };
        
    }
    else
    {
        ctx.Button = this;      
    }    
    
    ctx.innerHTML = ctx.Button.Name || '';
    ctx.className='IDCWMDesktopTaskbarContextMenuItem';
    ctx.style.backgroundImage='url(' + this.Taskbar.Desktop.WindowManager.Core.WebsiteUrl + this.ContextMenuIcon + ')';
    
    ctx.onclick = function() { if(this.Button.Command) this.Button.Command(this.Button); };
    ctx.onmouseover = function() { this.className='IDCWMDesktopTaskbarContextMenuItemHover'; };
    ctx.onmouseout = function() { this.className='IDCWMDesktopTaskbarContextMenuItem'; };
    
    if(!ctx.Button.Command)
    {
        ctx.parentNode.removeChild(ctx);
    }
    
    if(ctx.Button.Command)
    {
        var ctUnPin = document.createElement('DIV');
        if(ctxBack.lastChild.className=='IDCWMDesktopTaskbarContextMenuCloseItem')
            ctxBack.insertBefore(ctUnPin, ctxBack.lastChild);
        else
            ctxBack.appendChild(ctUnPin);
        
        ctUnPin.Button = this.ChildOfGroupButton || this;
        ctUnPin.innerHTML= this.Taskbar.TextUnPinToTaskbar || 'Unpin from taskbar';
        
        ctUnPin.className='IDCWMDesktopTaskbarContextMenuUnPinItem';
        
        ctUnPin.onmouseover = function() { this.className='IDCWMDesktopTaskbarContextMenuUnPinItemHover'; };
        ctUnPin.onmouseout = function() { this.className='IDCWMDesktopTaskbarContextMenuUnPinItem'; };
        ctUnPin.onclick = function()
                            {
                                if(this.Button.Taskbar.OnUnPinToTaskbar)
                                {
                                    var btn = this.Button.ChildGroupedButtons.length>0 ? this.Button.ChildGroupedButtons[0] : this.Button;
                                    if(!btn.Uniqueidentifier) btn = this.Button;
                                    if(!btn.Uniqueidentifier) btn = this.ChildOfGroupButton;
                                    this.Button.Taskbar.OnUnPinToTaskbar(btn);
                                }
                                this.Button.Remove();
                            };
    }
    
    var ctxMPCTRL = this.Taskbar.MultipleWindowsOuterControl;
    this.Taskbar.Desktop.WindowManager.ContextMenuManager.Show(this.Taskbar.MultipleWindowsOuterControl, this.Taskbar.Desktop.WindowManager.Core.Mouse.X - this.Taskbar.Desktop.Control.offsetLeft, null, null,this.Taskbar.Desktop.Control.offsetHeight-this.Taskbar.Desktop.WindowManager.Core.Mouse.Y + this.Taskbar.Desktop.Control.offsetTop,
                function() { return (ctxMPCTRL.offsetWidth - ctxBack.offsetWidth)/2; } );
  

    
};

IDC_TaskbarButton.prototype.OnMouseOver = function()
{
    if(this.Command && this.ChildGroupedButtons.length==0 && this.AnimationFrame==0)
    {
        this.Control.className='IDCWMDesktopTaskbarCommandButtonHover';
    }
};
IDC_TaskbarButton.prototype.OnMouseOut = function()
{
    if(this.Command && this.ChildGroupedButtons.length==0 && this.AnimationFrame==0)
    {
        this.Control.className='IDCWMDesktopTaskbarCommandButton';
    }
};

IDC_TaskbarButton.prototype.ExecuteAnimate = function()
{
    if(!this.Control) return;
    if(this.Control.className!='IDCWMDesktopTaskbarCommandButtonAnimating')
    {
        this.AnimationClassBefore = this.Control.className;
        this.Control.className='IDCWMDesktopTaskbarCommandButtonAnimating';
    }
       
    this.Control.style.backgroundPosition = '0px ' + (-this.AnimationFrame * this.AnimationFrameHeight) + 'px';
    this.AnimationFrame++;
    if(this.AnimationFrame<this.AnimationMaxFrames)
    {
        var me = this;
        setTimeout(function() { me.ExecuteAnimate(); }, this.AnimationDelay);
    }
    else
    {
        this.Control.className=this.AnimationClassBefore.replace('Hover','');
        this.Control.style.backgroundPosition = '';
        this.AnimationFrame=0;
    }
};

IDC_TaskbarButton.prototype.OnClick = function()
{
    if(this.Command && this.ChildGroupedButtons.length==0)
    {
        if(this.AnimationEnabled)
        {
            this.AnimationFrame=0;
            this.ExecuteAnimate();
        }
        this.Command(this);
    }
    else
    {
        if(this.ChildOfGroupButton && this.Taskbar.AutogroupButtons)
        {
            this.ChildOfGroupButton.OnClick();
            return;
        }
        
        if(this.BoundWindow)
        {
            if(this.ChildGroupedButtons.length>0 && this.Taskbar.AutogroupButtons)
            {
                this.Taskbar.Desktop.WindowManager.ContextMenuManager.Show(this.Taskbar.MultipleWindowsOuterControl, null,null,null, this.Taskbar.Control.offsetHeight);
                this.Taskbar.FillMultipleWindowsControl(this);
            }
            else
                this.BoundWindow.Focus();
        }
        else
        {
            if(this.ChildGroupedButtons.length>1 && this.Taskbar.AutogroupButtons)
            {
                this.Taskbar.Desktop.WindowManager.ContextMenuManager.Show(this.Taskbar.MultipleWindowsOuterControl, null,null,null, this.Taskbar.Control.offsetHeight);
                this.Taskbar.FillMultipleWindowsControl(this);
            }
            else
            {
                if(this.ChildGroupedButtons[0].BoundWindow)
                    this.ChildGroupedButtons[0].BoundWindow.Focus();
            }
        }
    }
};

IDC_TaskbarButton.prototype.Focus = function()
{
    this.IsFocused = true;
    this.RefreshStyles();
};

IDC_TaskbarButton.prototype.Blur = function()
{
    this.IsFocused = false;
    this.RefreshStyles();
};

IDC_TaskbarButton.prototype.RefreshStyles = function()
{
    var style = this.IsFocused ? 'Focused' : '';
    var childClass = (this.Taskbar.AutogroupButtons ? 'IDCWMDesktopTaskbarButtonGroupChild' : 'IDCWMDesktopTaskbarButtonText');
    var primaryClass = (this.Taskbar.AutogroupButtons ? 'IDCWMDesktopTaskbarButtonGroupPrimary' : 'IDCWMDesktopTaskbarButtonText');

    if(this.ChildOfGroupButton)
    {
        if(this.ChildOfGroupButton.Command==null)
        {
            if(this.Taskbar.AutogroupButtons)
                this.ChildOfGroupButton.Control.className=primaryClass + style;
                
            for(var i=0;i<this.ChildOfGroupButton.ChildGroupedButtons.length;i++)
            {
                if(!this.Taskbar.AutogroupButtons)
                {
                    this.ChildOfGroupButton.ChildGroupedButtons[i].Control.className=childClass + (this.ChildOfGroupButton.ChildGroupedButtons[i] == this || this.Taskbar.AutogroupButtons ? style : '');
                    
                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.innerHTML = this.ChildOfGroupButton.ChildGroupedButtons[i].Name;
                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.style.display='';
                    //this.ChildOfGroupButton.ChildGroupedButtons[i].Control.style.width=180;
                }
                else
                {
                    this.ChildOfGroupButton.ChildGroupedButtons[i].Control.className=childClass + style;
                    //this.ChildOfGroupButton.ChildGroupedButtons[i].Control.style.width='';
                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.style.display='none';
                }   
                this.ChildOfGroupButton.ChildGroupedButtons[i].MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? 'Primary' : 'Text') + (this.ChildOfGroupButton.ChildGroupedButtons[i] == this ? style: '') +  + this.CustomHoveringColor;
            }
        }
        else
        {
            if(this.Taskbar.AutogroupButtons)
                this.ChildOfGroupButton.Control.className=primaryClass + style;
                
            for(var i=0;i<this.ChildOfGroupButton.ChildGroupedButtons.length;i++)
            {
                
                if(!this.Taskbar.AutogroupButtons)
                {
                    this.ChildOfGroupButton.ChildGroupedButtons[i].Control.className=childClass + (this.ChildOfGroupButton.ChildGroupedButtons[i] == this || this.Taskbar.AutogroupButtons ? style: '');

                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.innerHTML = this.ChildOfGroupButton.ChildGroupedButtons[i].Name;
                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.style.display='';
                    //this.ChildOfGroupButton.ChildGroupedButtons[i].Control.style.width=300;
                    
                }
                else
                {
                    this.ChildOfGroupButton.ChildGroupedButtons[i].Control.className=childClass + style;
                    this.ChildOfGroupButton.ChildGroupedButtons[i].TextControl.style.display='none';
                    //this.ChildOfGroupButton.ChildGroupedButtons[i].Control.style.width='';
                }   
                
                this.ChildOfGroupButton.ChildGroupedButtons[i].MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? 'Primary' : 'Text') + (this.ChildOfGroupButton.ChildGroupedButtons[i] == this || this.Taskbar.AutogroupButtons ? style: '') + this.CustomHoveringColor;
            }
            var obj = this.ChildOfGroupButton.ChildGroupedButtons[this.ChildOfGroupButton.ChildGroupedButtons.length-1];
            var mix = this.ChildOfGroupButton.ChildGroupedButtons.length>1 && this.Taskbar.AutogroupButtons ? 'GroupPrimary' : (this.Taskbar.AutogroupButtons ? '' : 'Text');
            obj.Control.className='IDCWMDesktopTaskbarButton' + mix + (obj == this || this.Taskbar.AutogroupButtons ? style: '');
            obj.MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? '' : 'Text') + style + obj.CustomHoveringColor;
            this.ChildOfGroupButton.Control.style.display='none';
        }
    }
    else if(this.ChildGroupedButtons.length>0)
    {
        this.Control.className=primaryClass + style;
        this.MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? 'Primary' : 'Text') + style + this.CustomHoveringColor;
        
        for(var i=0;i<this.ChildGroupedButtons.length;i++)
        {               
            if(!this.Taskbar.AutogroupButtons)
            {
                this.ChildGroupedButtons[i].TextControl.innerHTML = this.ChildGroupedButtons[i].Name;
                this.ChildGroupedButtons[i].TextControl.style.display='';
                //this.ChildGroupedButtons[i].Control.style.width=180;
                this.ChildGroupedButtons[i].Control.className=childClass + (this.ChildGroupedButtons[i] == this || this.Taskbar.AutogroupButtons ? style: '');
            }
            else
            {
                //this.ChildGroupedButtons[i].Control.style.width='';
                this.ChildGroupedButtons[i].TextControl.style.display='none';
                this.ChildGroupedButtons[i].Control.className=childClass +  style;
            }   
            this.ChildGroupedButtons[i].MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? 'Primary' : 'Text') + (this.ChildGroupedButtons[i] == this || this.Taskbar.AutogroupButtons ? style: '') + this.ChildGroupedButtons[i].CustomHoveringColor;
        }
    }
    else
    {
        if(this.Command)
        {
            this.Control.className='IDCWMDesktopTaskbarCommandButton' + style;
            this.TextControl.style.display='none';
            this.MouseOverControl.className='IDCWMDesktopTaskbarCommandButtonMouseOver' + (this.Taskbar.AutogroupButtons ? '' : 'Text') + style + this.CustomHoveringColor;
        }
        else
        {
            if(!this.Taskbar.AutogroupButtons)
            {
                this.Control.className='IDCWMDesktopTaskbarButtonText' + style;
                this.MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? '' : 'Text') + style + this.CustomHoveringColor;
                //this.Control.style.width=180;
                this.TextControl.innerHTML = this.Name;
                this.TextControl.style.display='';
            }
            else
            {
                
                this.Control.className='IDCWMDesktopTaskbarButton' + style;
                this.MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? '' : 'Text') + style + this.CustomHoveringColor;
                this.TextControl.style.display='none';
                
            }
        }
    }


};


IDC_TaskbarButton.prototype.Remove = function()
{
    for(var i=this.Taskbar.Buttons.length-1;i>=0;i--)
    {
        if(this.Taskbar.Buttons[i] == this)
        {           
            this.Taskbar.Buttons.splice(i,1);
            break;
        }
    } 
    
    this.Control.onmousemove = null;
    this.Control.onmouseout = null;
    this.Control.onclick = null;
    
    this.BoundWindow = null;
    this.Control.Button = null;
    this.Control.removeChild(this.IconControl);
    this.Control.removeChild(this.TextControl);
    this.Control.removeChild(this.MouseOverControl);
    this.ParentObject.removeChild(this.Control);
    
    //If im a child of a grouping
    if(this.ChildOfGroupButton)
    {
        for(var i=this.ChildOfGroupButton.ChildGroupedButtons.length-1;i>=0;i--)
        {
            var b = this.ChildOfGroupButton.ChildGroupedButtons[i];
            if(b == this)
            {                           
                this.ChildOfGroupButton.ChildGroupedButtons.splice(i,1);
                this.ChildOfGroupButton.RefreshStyles();
                break;
            }
        }  
        
        if(this.ChildOfGroupButton.Command!=null)
        {
            if(this.ChildOfGroupButton.ChildGroupedButtons.length==0)
                this.ChildOfGroupButton.Control.style.display='';
            else
            {
                var index = this.ChildOfGroupButton.ChildGroupedButtons.length-1;
                
                var clsType = index==0 ? (this.Taskbar.AutogroupButtons ? '' : 'Text') : (this.Taskbar.AutogroupButtons ? 'GroupPrimary' : 'Text');
                var clsType2 = index==0 ? (this.Taskbar.AutogroupButtons ? '' : 'Text') : (this.Taskbar.AutogroupButtons ? 'Primary' : 'Text');
                
                var style = this.ChildOfGroupButton.ChildGroupedButtons[index].IsFocused ? 'Focused' : '';
                this.ChildOfGroupButton.ChildGroupedButtons[index].Control.className='IDCWMDesktopTaskbarButton' + clsType + style;
                this.ChildOfGroupButton.ChildGroupedButtons[index].MouseOverControl.className='IDCWMDesktopTaskbarButtonMouseOver' + (this.Taskbar.AutogroupButtons ? '' : 'Text') + clsType2 + style;
            }
            
               
        }
    }
    
    //If im an owner of a group
    else if(this.ChildGroupedButtons.length>0)
    {
        var newOwner = this.ChildGroupedButtons[this.ChildGroupedButtons.length-1];
        this.ChildGroupedButtons.splice(this.ChildGroupedButtons.length-1,1);
        
        newOwner.ChildOfGroupButton=null;
        newOwner.ChildGroupedButtons = this.ChildGroupedButtons;
                
        if(newOwner.ChildGroupedButtons.length>0)
        {
            for(var i=0;i<newOwner.ChildGroupedButtons.length;i++)
            {
                newOwner.ChildGroupedButtons[i].ChildOfGroupButton = newOwner;
            }
        }
        newOwner.RefreshStyles();
    }
        
    delete this.Control;
    this.Control = null;
    this.ParentObject = null;
    this.ChildOfGroupButton = null;
    var ie = this.Taskbar.isIE
    this.Taskbar = null;
    
    if(ie)
        CollectGarbage();
};



var IDC_ContextMenuManager = function(WindowManager, ParentObject)
{
    this.WindowManager = WindowManager;
    this.Document = WindowManager.Document;
    this.ParentObject = ParentObject;
    this.Control = this.Document.createElement('SPAN');
    this.Control.style.position='absolute';
    this.Control.style.display='block';
    this.Control.style.zIndex=990000;
    
    this.ParentObject.appendChild(this.Control);
    this.ParentObject.ContextMenuManager = this;
    
    this.ParentObject.onmouseup=function(e) { this.ContextMenuManager.__OnMouseUp(e); };
    this.OnHide = null;
    
};

IDC_ContextMenuManager.prototype.ShowFromWindow = function(Window, MenuDOMObject, Left, Top, CoreOut)
{
    var x = Left;
    var y = Top;
    if(Window)
    {       
        var wd = (Window.ContentIframeOuterBox.offsetWidth - Window.ContentIframe.offsetWidth);
        var hd = (Window.ContentIframeOuterBox.offsetHeight - Window.ContentIframe.offsetHeight);
        
        
        if(Window.WindowState==2)
        {
            x+=wd;
            y+=Window.Titlebar.offsetHeight;
            
        }
        else
        {
            x+=Window.Left + wd;
            y+=Window.Top + hd + Window.Titlebar.offsetHeight;
        }  
    }

    y+=this.WindowManager.Desktop.Control.offsetTop;

    this.Show(MenuDOMObject, x, y, null, null, null, CoreOut);
};

IDC_ContextMenuManager.prototype.Show = function(MenuDOMObject, Left, Top, Right, Bottom, offsetFunction, CoreOut)
{
    for(var i=this.Control.childNodes.length-1;i>=1;i--)
    {
        var o = this.Control.removeChild(this.Control.childNodes[i]);
        delete o;
    }
    
     if(this.WindowManager.CurrentWindow)
     {
        this.WindowManager.CurrentWindow.BlurredOverlay.CoreStartMouseX = CoreOut ? CoreOut.Mouse.X : 0;
        this.WindowManager.CurrentWindow.BlurredOverlay.CoreStartMouseY = CoreOut ? CoreOut.Mouse.Y : 0;
        
        this.WindowManager.CurrentWindow.BlurredOverlay.CoreOut = CoreOut;
        this.WindowManager.CurrentWindow.BlurredOverlay.style.display='';       
    }
    while(this.Control.hasChildNodes())
    {
        this.Control.removeChild(this.Control.lastChild);
    }
    
    if(MenuDOMObject)
        this.Control.appendChild(MenuDOMObject);
       
    this.Control.style.left = isNaN(Left) ? '' : Left; 
    this.Control.style.top = isNaN(Top) ? '' : Top;
    this.Control.style.bottom = isNaN(Bottom) ? '' : Bottom;
    this.Control.style.right = isNaN(Right) ? '' : Right;
    
    this.Control.style.visibility='visible';
    
     if(this.WindowManager.CurrentWindow)
     {
        this.WindowManager.CurrentWindow.BlurredOverlay.StartMouseX = this.Control.offsetLeft;
        this.WindowManager.CurrentWindow.BlurredOverlay.StartMouseY = this.Control.offsetTop;
    }
    
    if(offsetFunction)
    {
        var off = offsetFunction();
        if(Left) this.Control.style.left = Left-off || '';
        if(Top) this.Control.style.top = Top-off || '';
        if(Bottom) this.Control.style.bottom = Bottom-off || '';
        if(Right) this.Control.style.right = Right-off || '';
    }
    
};

IDC_ContextMenuManager.prototype.Hide = function()
{
    this.Control.style.visibility='hidden';
    
    if(this.OnHide)
        this.OnHide();
        
     if(this.WindowManager.CurrentWindow)
        this.WindowManager.CurrentWindow.BlurredOverlay.style.display='none';
    
};

IDC_ContextMenuManager.prototype.__OnMouseUp = function(e)
{
    var target=e?e.target:event.srcElement;

    if(this.Control.style.visibility!='hidden' && target != this.Control)
    {
        if(target.tagName!='INPUT' && target.tagName!='SELECT' && target.tagName!='OPTION' && target.tagName!='TEXTAREA' && target.getAttribute('dontcollapse')!='true')
        { 
            this.Control.style.visibility='hidden';
            
            if(this.OnHide)
                this.OnHide();
        }
    }
};

var IDC_ContextMenu = function (windowManager, parentMenuItem, Window, Core, OnBeforeShow)
{
    this.WindowManager = windowManager;
    this.Window = Window;
    this.Core = Core;
    this.Document = this.WindowManager.Document;
    
    this.OnBeforeShow = OnBeforeShow;
    
    this.Control = this.Document.createElement('DIV');
    this.Control.MenuItems = this.Document.createElement('TABLE');
    this.Control.MenuItems.cellPadding=0;
    this.Control.MenuItems.cellSpacing=0;
    this.Control.MenuItems.border=0;

    this.Control.appendChild(this.Control.MenuItems);
    
    this.Control.className='IDCWMWindowContextMenuFrame';
    
    this.ParentMenuItem = parentMenuItem;
    this.Items = new Array();
    
    this.ExtraItems = null;
    
    this.ScrollUp = null;
    this.ScrollDown = null;
};

IDC_ContextMenu.prototype.ShowBelowControl = function(ctrl)
{
    var pos = this.Core.DOM.GetObjectPosition(ctrl);
            
    pos[1]+=this.WindowManager.ParentObject.offsetTop;
    pos[0]+=this.WindowManager.ParentObject.offsetLeft;
        
    this.Show(pos[0],pos[1] + ctrl.offsetHeight);
};

IDC_ContextMenu.prototype.ShowBelowControlLeanLeft = function (ctrl) {
    var pos = this.Core.DOM.GetObjectPosition(ctrl);

    //alert(this.Control.offsetWidth);
    if (this.Control.offsetWidth == 0) {
        this.Show(-1000, -1000);
    }

    pos[1] += this.WindowManager.ParentObject.offsetTop;
    pos[0] += this.WindowManager.ParentObject.offsetLeft + ctrl.offsetWidth - this.Control.offsetWidth;

    this.Show(pos[0], pos[1] + ctrl.offsetHeight);
};

IDC_ContextMenu.prototype.Show = function(x,y)
{

        
    
    if(this.Window && this.Core)
    {       
        var wd = (this.Window.ContentIframeOuterBox.offsetWidth - this.Window.ContentIframe.offsetWidth);
        var hd = (this.Window.ContentIframeOuterBox.offsetHeight - this.Window.ContentIframe.offsetHeight);
        
        if(this.Window.WindowState==2)
        {
            x+=wd;
            y+=this.Window.Titlebar.offsetHeight;
            
        }
        else
        {
            x+=this.Window.Left + wd;
            y+=this.Window.Top + hd + this.Window.Titlebar.offsetHeight;
        }
    }
    
    if(this.OnBeforeShow)
    {
        this.OnBeforeShow(this);
    }
    this.ShowAutoHidden();
    

    
    this.WindowManager.ContextMenuManager.Show(this.Control, x, y,null,null,null,this.Core);

    var mt = isNaN(this.Control.style.marginTop) ? 0 : parseInt(this.Control.style.marginTop);
    var oh = this.Control.offsetHeight;
    
    var ml = isNaN(this.Control.style.marginLeft) ? 0 : parseInt(this.Control.style.marginLeft);
    var ow = this.Control.offsetWidth;
    
    if(y + oh > this.WindowManager.Desktop.Control.offsetHeight + this.WindowManager.Desktop.Control.offsetTop)
    {
        y += (this.WindowManager.Desktop.Control.offsetHeight + this.WindowManager.Desktop.Control.offsetTop) - (y + oh);

        if(y<0)
        {
          if(!this.ScrollUp == null || this.ScrollDown == null)
          {
            this.ScrollUp = this.Document.createElement('DIV');
            this.ScrollUp.innerHTML='&uarr;';
            
            this.ScrollUp.className='IDCWMWindowContextMenuScrollUp';
            this.Document.body.appendChild(this.ScrollUp);
            
            this.ScrollDown = this.Document.createElement('DIV');
            this.ScrollDown.innerHTML='&darr;';
            
            this.ScrollDown.className='IDCWMWindowContextMenuScrollDown';
            this.Document.body.appendChild(this.ScrollDown);
            
            this.ScrollUp.setAttribute('dontcollapse','true');
            this.ScrollDown.setAttribute('dontcollapse','true');
            
            this.ScrollDown.ContextMenu = this;
            this.ScrollDown.WindowManager = this.WindowManager;
            
            this.ScrollUp.ContextMenu = this;
            this.ScrollUp.WindowManager = this.WindowManager;
                        
            this.ScrollDown.doScroll=function()
                                            {
                                                var y = this.WindowManager.ContextMenuManager.Control.offsetTop;
                                                y-=5;
                                                var oh = this.ContextMenu.Control.offsetHeight;
                                                if(y + oh < this.WindowManager.Desktop.Control.offsetHeight + this.WindowManager.Desktop.Control.offsetTop)
                                                {
                                                    y=this.WindowManager.Desktop.Control.offsetHeight + this.WindowManager.Desktop.Control.offsetTop-oh;
                                                    this.style.display='none';
                                                }
                                                else
                                                {
                                                    this.WindowManager.ContextMenuManager.Control.style.top=y;
                                                    this.style.display='';
                                                    this.ContextMenu.ScrollUp.style.display='';
                                                }
                                                
                                                if(this.scrolling)
                                                {
                                                    var me = this;
                                                    setTimeout(function() { me.doScroll(); },25);
                                                }
                                            };
                                            
            this.ScrollDown.onmouseover = function(){this.scrolling=true;this.doScroll();};
            this.ScrollUp.onmouseover = function(){this.scrolling=true;this.doScroll();};
                                                
            this.ScrollDown.onmouseout = function(){this.scrolling=false;};           
            this.ScrollUp.onmouseout = function(){this.scrolling=false;};
                                                    
            this.ScrollUp.doScroll=function()
                                            {
                                                var y = this.WindowManager.ContextMenuManager.Control.offsetTop;
                                                y+=5;
                                                var oh = this.ContextMenu.Control.offsetHeight;
                                                if(y>0)
                                                {
                                                    y=0;
                                                    this.style.display='none';
                                                }
                                                else
                                                {
                                                    this.WindowManager.ContextMenuManager.Control.style.top=y;
                                                    this.style.display='';
                                                    this.ContextMenu.ScrollDown.style.display='';
                                                }
                                                
                                                if(this.scrolling)
                                                {
                                                    var me = this;
                                                    setTimeout(function() { me.doScroll(); },25);
                                                }
                                            };
            
          }
          
            this.ScrollDown.style.display='';
            this.ScrollUp.style.display='none';
        
            this.ScrollDown.style.left = x;
            this.ScrollUp.style.left = x;

            y = this.ScrollUp.offsetHeight;

            this.ScrollDown.style.width=this.Control.offsetWidth;
            this.ScrollUp.style.width=this.Control.offsetWidth;

            var ctx = this;
            this.WindowManager.ContextMenuManager.OnHide = function() { ctx.ScrollUp.style.display='none'; ctx.ScrollDown.style.display='none';};
        }
        
        this.WindowManager.ContextMenuManager.Show(this.Control, x, y);
    }
        
    if(x + ow > this.WindowManager.Desktop.Control.offsetWidth)
    {       
        x = this.WindowManager.Desktop.Control.offsetWidth - this.Control.offsetWidth;
        this.WindowManager.ContextMenuManager.Show(this.Control, x, y);
    }

};

IDC_ContextMenu.prototype.CloneFromItem = function(dest, src)
{

    if(src.SourceNode)
    {
        //src = src.SourceNode;
        //alert(src.Name);
    }    
    var node = dest.AddItem(src.Name, src.Icon, null, src.ToolbarButton);

        
    node.SourceNode = src.SourceNode || src;
    node.OnClick = function() { if(this.SourceNode.OnClick) { this.SourceNode.OnClick(this.SourceNode); } };
    if(src.SubMenu)
    {
        for(var i=0;i<src.SubMenu.Items.length;i++)
        {
            dest.ContextMenu.CloneFromItem(node, src.SubMenu.Items[i]);
        }
    }
};

IDC_ContextMenu.prototype.ShowAutoHidden = function()
{
    for(var i=0;i<this.Items.length;i++)
    {
        if(this.Items[i].AutoHiddden)
        {
            this.Items[i].AutoHiddden=false;
            this.Items[i].Show();
        }
        
    }
    
    if(this.ExtraItems)
    {
        this.ExtraItems.Clear();
        this.ExtraItems.Hide();
    }
};

IDC_ContextMenu.prototype.Hide = function()
{
    if(this.ScrollDown) this.ScrollDown.style.display='none';
    if(this.ScrollUp) this.ScrollUp.style.display='none';
    this.WindowManager.ContextMenuManager.Hide();
};

IDC_ContextMenu.prototype.AddItem = function(Name, Icon, OnClick, ToolbarButton)
{
    var l = new IDC_ContextMenuItem(this, Icon, Name, OnClick, ToolbarButton)
    this.Items.push(l);
    return l;
};

IDC_ContextMenu.prototype.AddSeparator = function()
{
    return this.AddItem(null, null, null);
    
};


IDC_ContextMenu.prototype.Clear = function()
{
    for(var i=this.Items.length-1;i>=0;i--)
    {
        this.Items[i].Control.parentNode.removeChild(this.Items[i].Control);
        this.Items[i]=null;
        this.Items.splice(i,1);
    }
};

var IDC_ContextMenuItem = function(ContextMenu, Icon, Name, OnClick, ToolbarButton)
{
    this.ContextMenu = ContextMenu;
    this.Document = this.ContextMenu.Document;
    
    this.Icon = Icon;
    this.Name = Name;
    this.OnClick = OnClick;
    this.Enabled = true;
    this.AutoHiddden=false;
    
    this.ToolbarButton = ToolbarButton;
    
    this.Control = this.ContextMenu.Control.MenuItems.insertRow(-1);
    this.Control.Icon = this.Control.insertCell(-1);
    this.Control.Text = this.Control.insertCell(-1);
    this.Control.SubContainer = this.Control.insertCell(-1);
    this.Control.SubContainer.vAlign='top';
    
    this.Control.SubContainerIcon = this.Document.createElement('IMG');
    this.Control.SubContainerIcon.border=0;
    this.Control.SubContainerIcon.src=this.ContextMenu.WindowManager.Core.WebsiteUrl + 'images/spacer.gif';
    this.Control.SubContainer.appendChild(this.Control.SubContainerIcon);
        
    if(this.Icon==null && this.Name == null && this.OnClick==null)
    {

        this.Control.Text.className='IDCWMWindowContextMenuSepatator';
        this.Control.Icon.className='IDCWMWindowContextMenuSepatatorLeft';
        this.Control.SubContainer.className='IDCWMWindowContextMenuSepatatorRight';
        
        this.Control.Text.innerHTML='&nbsp;';
        this.Control.Icon.innerHTML='&nbsp;';
        
        this.Control.setAttribute('dontcollapse','true');
        this.Control.SubContainerIcon.setAttribute('dontcollapse','true');
        this.Control.Text.setAttribute('dontcollapse','true');
        this.Control.Icon.setAttribute('dontcollapse','true');
    }
    else
    {
        

        if(Icon)
        {
            this.Control.Icon.style.backgroundImage = 'url(' + this.ContextMenu.WindowManager.Core.WebsiteUrl + Icon + ')';
        }
        
        this.Control.Text.innerHTML = Name;
        this.Control.className='IDCWMWindowContextMenuItem';
        this.Control.Text.className='IDCWMWindowContextMenuText';
        this.Control.Icon.className='IDCWMWindowContextMenuIcon';
        this.Control.SubContainer.className='IDCWMWindowContextMenuSubPointer';
        this.Control.SubContainerIcon.className='IDCWMWindowContextMenuSubPointerIconNull';
        
        this.Control.Icon.innerHTML='&nbsp;';
        
        this.Control.ContextMenu = this;
        this.Control.onmouseover=function() { this.ContextMenu.__mouseover(); };
        this.Control.onmouseout=function() { this.ContextMenu.__mouseout(); };
        
        this.Control.onclick = function() {
                                            if(this.ContextMenu.OnClick && !this.ContextMenu.SubMenu && this.ContextMenu.Enabled)
                                            { 
                                                
                                                if(this.ContextMenu.ToolbarButton)
                                                {
                                                    this.ContextMenu.ToolbarButton.SetTextAndIcon(this.ContextMenu.Name , this.ContextMenu.Icon);
                                                }
                                                this.ContextMenu.OnClick(this.ContextMenu); 
                                                
                                            } 
//                                            if(this.ContextMenu.ContextMenu.ParentMenuItem)
//                                                this.ContextMenu.ContextMenu.ParentMenuItem.ContextMenu.Hide();

                                            if(this.ContextMenu.ContextMenu.ParentMenuItem)
                                                this.ContextMenu.ContextMenu.ParentMenuItem.SubMenu.Control.style.visibility='hidden';

                                            this.ContextMenu.ContextMenu.Hide(); 
                                            };
    }
    
    this.SubMenu = null;
    
    return this;
};

IDC_ContextMenuItem.prototype.Hide = function(AutoHiddden)
{
    this.Control.style.display='none';
    this.AutoHiddden = AutoHiddden || false;
};


IDC_ContextMenuItem.prototype.SetVisibility = function (visible) {
    this.Control.style.display = visible ? '' : 'none';
};

IDC_ContextMenuItem.prototype.Show = function()
{
    this.Control.style.display='';
};

IDC_ContextMenuItem.prototype.Disable = function()
{
    this.SetEnabledState(false);
};

IDC_ContextMenuItem.prototype.Enable = function()
{
    this.SetEnabledState(true);
};

IDC_ContextMenuItem.prototype.SetEnabledState = function(state)
{
    this.Enabled = state;
    
    var opa = state ? 100 : document.all ? 10 : 60;
    
    if(document.all)
    {
        this.Control.Text.style.filter = opa != 100 ? 'alpha(opacity=' + (opa*6) + ')' : null;
        this.Control.Icon.style.filter = opa != 100 ? 'alpha(opacity=' + (opa) + ')' : null; // = opa != 100 ? 'progid:DXImageTransform.Microsoft.Emboss()' : null;
        //this.Control.Icon.style.height=opa != 100 ? 14 : 16;
        //this.Control.Icon.style.width=opa != 100 ? 14 : 16;
    }
    else
        this.Control.style.opacity = opa/100;
    
};

IDC_ContextMenuItem.prototype.__mouseover = function()
{
    if(!this.Enabled) return;
    
    this.Control.className='IDCWMWindowContextMenuItemHover';
    this.Control.Text.className='IDCWMWindowContextMenuTextHover';
    this.Control.Icon.className='IDCWMWindowContextMenuIconHover';
    this.Control.SubContainer.className='IDCWMWindowContextMenuSubPointerHover';
    this.Control.SubContainerIcon.className= this.SubMenu? 'IDCWMWindowContextMenuSubPointerIconHover' : 'IDCWMWindowContextMenuSubPointerIconNullHover';
    
    if(this.SubMenu)
    {
        //this.SubMenu.ShowAutoHidden();
        this.SubMenu.Control.style.visibility='visible';   
        this.SubMenu.Control.style.marginTop = '';
        this.SubMenu.Control.style.marginLeft = '';
        
        if(this.SubMenu.Control.offsetHeight > this.ContextMenu.WindowManager.Desktop.Control.offsetHeight)
        {
            if(!this.SubMenu.ScrollUp == null || this.SubMenu.ScrollDown == null)
            {
//                this.SubMenu.ScrollUp = this.Document.createElement('DIV');
//                this.SubMenu.ScrollUp.innerHTML='5';

//                this.SubMenu.ScrollUp.className='IDCWMWindowContextMenuScrollUp';
//                this.Document.body.appendChild(this.SubMenu.ScrollUp);

//                this.SubMenu.ScrollDown = this.Document.createElement('DIV');
//                this.SubMenu.ScrollDown.innerHTML='6';

//                this.SubMenu.ScrollDown.className='IDCWMWindowContextMenuScrollDown';
//                this.Document.body.appendChild(this.SubMenu.ScrollDown);

//                this.SubMenu.ScrollUp.setAttribute('dontcollapse','true');
//                this.SubMenu.ScrollDown.setAttribute('dontcollapse','true');

//                this.SubMenu.ScrollDown.ContextMenu = this.SubMenu;
//                this.SubMenu.ScrollDown.WindowManager = this.WindowManager;

//                this.SubMenu.ScrollUp.ContextMenu = this.SubMenu;
//                this.SubMenu.ScrollUp.WindowManager = this.WindowManager;
//                
//                this.SubMenu.ScrollDown.style.display='';
//                this.SubMenu.ScrollUp.style.display='none';
//            
//                this.SubMenu.ScrollDown.style.left = this.SubMenu.Control.offsetLeft;
//                this.SubMenu.ScrollUp.style.left = this.SubMenu.Control.offsetLeft;

//                this.SubMenu.ScrollDown.style.width=this.SubMenu.Control.offsetWidth;
//                this.SubMenu.ScrollUp.style.width=this.SubMenu.Control.offsetWidth;

//                var ctx = this;
                
            }
        }
        
        var oh = this.SubMenu.Control.offsetHeight;//-mt;
        var y = this.ContextMenu.WindowManager.Core.DOM.GetObjectPosition(this.SubMenu.Control)[1];
        var dh = this.ContextMenu.WindowManager.Desktop.Control.offsetHeight + this.ContextMenu.WindowManager.Desktop.Control.offsetTop;

        if(y + oh > dh)
        {
            this.SubMenu.Control.style.marginTop = dh - (y + oh + 4);
        }
        
        var ow = this.SubMenu.Control.offsetWidth;//-mt;
        var x = this.ContextMenu.WindowManager.Core.DOM.GetObjectPosition(this.SubMenu.Control)[0];
        var dw = this.ContextMenu.WindowManager.Desktop.Control.offsetWidth;
        
        if(x + ow > dw)
        {
            var mx = (this.SubMenu.Control.offsetWidth + this.Control.offsetWidth);
            if(x-mx < 0) mx += (x-mx);
            this.SubMenu.Control.style.marginLeft=  -mx;
//            alert(this.SubMenu.ScrollDown.offsetLeft- mx);
//            if(this.SubMenu.ScrollDown) this.SubMenu.ScrollDown.style.left = this.SubMenu.ScrollDown.offsetLeft- mx;
//            if(this.SubMenu.ScrollUp) this.SubMenu.ScrollUp.style.left = this.SubMenu.ScrollDown.offsetLeft- mx;
            
        }
        

    }
};  

IDC_ContextMenuItem.prototype.__mouseout = function()
{
    this.Control.className='IDCWMWindowContextMenuItem';
    this.Control.Text.className='IDCWMWindowContextMenuText';
    this.Control.Icon.className='IDCWMWindowContextMenuIcon';
    this.Control.SubContainer.className='IDCWMWindowContextMenuSubPointer';
    this.Control.SubContainerIcon.className=this.SubMenu?'IDCWMWindowContextMenuSubPointerIcon':'IDCWMWindowContextMenuSubPointerIconNull';
    
    if(this.SubMenu)
    {
        this.SubMenu.Control.style.visibility='hidden';

    }
};

IDC_ContextMenuItem.prototype.AddItem = function(Name, Icon, OnClick, ToolbarButton)
{
    if(!this.SubMenu)
    {
        this.SubMenu = new IDC_ContextMenu(this.ContextMenu.WindowManager, this);
        this.Control.SubContainer.appendChild(this.SubMenu.Control);
        this.SubMenu.Control.style.visibility='hidden';
        this.Control.SubContainerIcon.className = 'IDCWMWindowContextMenuSubPointerIcon';
        
        this.Control.setAttribute('dontcollapse','true');
        this.Control.SubContainerIcon.setAttribute('dontcollapse','true');
        this.Control.Text.setAttribute('dontcollapse','true');
        this.Control.Icon.setAttribute('dontcollapse','true');
    }
    
    var l = new IDC_ContextMenuItem(this.SubMenu, Icon, Name, OnClick, ToolbarButton);
    this.SubMenu.Items.push(l);
    return l;
};

IDC_ContextMenuItem.prototype.AddSeparator = function()
{
    return this.AddItem(null, null, null);
    
};

IDC_ContextMenuItem.prototype.Clear = function()
{
    for(var i=this.SubMenu.Items.length-1;i>=0;i--)
    {
        this.SubMenu.Items[i].Control.parentNode.removeChild(this.SubMenu.Items[i].Control);
        this.SubMenu.Items[i]=null;
        this.SubMenu.Items.splice(i,1);
    }
};



IDC_WindowManager.prototype.MessageBox = function(Text, Title, Icon, Buttons, CallBack)
{
    var htmlbtns = '';
    
    for(var i=0;i<Buttons.length;i++)
    {
        htmlbtns += (htmlbtns=='' ? '':'&nbsp;') + '<input onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="' + Buttons[i].Text + '" onclick="window.frameElement.IDCWindow.Close("' + Buttons[i].CallbackValue + '");">';
    }
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + Icon + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;">' + htmlbtns + '</td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;
    win.Name = Title || null;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.Show();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
};

IDC_WindowManager.prototype.PickDateAndTime = function (Question, Title, Date, Time, Icon, CallBack) {

    var dtHtml = '<table border="0" cellpadding="0" cellspacing="0">' +
            '<tr>' +
                '<td style="padding-right:4px;">Dato:</td>' +
                '<td><input class="designerTextbox" value="' + (Date != null ? Date : '') + '" style="width:90px;height:' + (this.isIE ? 18 : 20) + 'px;" type="text" name="dtPicker" id="dtPicker"></td>' +
                '<td><DIV class="designerDatePickerButton" onmouseover="this.className=\'designerDatePickerButtonHover\';" onmouseout="this.className=\'designerDatePickerButton\';" onclick="pickDateToControl(\'dtPicker\');"></DIV></td>' +
                '<td style="padding-left:12px;padding-right:4px;">Klokken:</td>' +
                '<td><input type="text" name="dtTime" value="' + (Time != null ? Time : '') + '" id="dtTime" class="designerTextbox" style="width:40px;height:' + (this.isIE ? 18 : 20) + 'px;"></td>' +
            '</tr>' +
            '</table>';

    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Core.WebsiteUrl + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<link href="' + this.Core.WebsiteUrl + 'styles/styles.css" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript" src="' + this.Core.WebsiteUrl + 'js/' + this.ScriptVersion + 'js.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.Core.WebsiteUrl + 'js/' + this.ScriptVersion + 'IDC_Core.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.Core.WebsiteUrl + 'js/' + this.ScriptVersion + 'DatePicker.js"></script>' +
                '<script language="javascript" type="text/javascript" src="' + this.Core.WebsiteUrl + 'js/' + this.ScriptVersion + 'IDC_ContextMenu.js"></script>' +
                '' +
                '<script language="javascript" type="text/javascript">\n' +
                'var datePicker=null;\n' +
                'var localCore = null;\n' +
                'var core = null;\n' +
                'var currentDateObject = null;\n' +

                'function init() { setTimeout(function() {_init();},50); };' +

                'function _init()' +
                '{' +
                    'w = window.frameElement.IDCWindow;' +
                    'core = new IDC_Core(document, document.body); //w.WindowManager.Core;' +
                    'localCore = new IDC_Core(document, document.body);' +
                    'wm = w.WindowManager;' +

                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterScreen();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +

                    'datePicker = new DatePicker(document, parent.document.body, top.window);' +
                    'datePicker.OnPickDate = OnDatePicked;' +

                '};' +

                'function pickDateToControl(id) {' +
                '    currentDateObject = document.getElementById(id);' +
                '' +
                '    var sl = document.body;' +
                '    var pos = localCore.DOM.GetObjectPosition(currentDateObject);' +
                '' +
                '    var dateparts = currentDateObject.value.split(/[.,\/ -]/);' +
                '    var dt = new Date();' +
                '    var day = dt.getDate();' +
                '    var month = dt.getMonth();' +
                '    var year = dt.getFullYear();' +
                '' +
                '    if (dateparts.length == 3) {' +
                '        day = parseInt(dateparts[0], 10);' +
                '        month = parseInt(dateparts[1], 10) - 1;' +
                '        year = parseInt(dateparts[2], 10);' +
                '    }' +
                '' +
                '    datePicker.SetCurrentDate(day, month, year);' +
                '' +
                '    if (w.WindowState != 2)' +
                '        datePicker.Show(pos[0] + w.Left - sl.scrollLeft, pos[1] + w.Top + 74 - sl.scrollTop);' +
                '    else' +
                '        datePicker.Show(pos[0] - sl.scrollLeft, pos[1] + 74 - sl.scrollTop);' +
                '};' +
                '' +
                '    function OnDatePicked(value) {' +
                '' +
                '        if (currentDateObject != null) {' +
                '            var y = value.getFullYear().toString();' +
                '            var m = (value.getMonth() + 1).toString();' +
                '            var d = value.getDate().toString();' +
                '' +
                '            if (m.length == 1) m = \'0\' + m;' +
                '            if (d.length == 1) d = \'0\' + d;' +
                '' +
                '            currentDateObject.value = d + \'-\' + m + \'-\' + y;' +
                '        }' +
                '    };' +

                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + this.Core.WebsiteUrl + (Icon) + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + dtHtml + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close([document.getElementById(\'dtPicker\').value, document.getElementById(\'dtTime\').value]);">&nbsp;<input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close(false);"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';


    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;

    win.Name = Title;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width = 60;
    win.Height = 20;

    //win.HTML = html;
    win.CenterOwner();
    win.ShowDialog();

    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_WindowManager.prototype.Confirm = function(Question, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(document.body.scrollWidth, document.body.scrollHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + Icon + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Question + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(true);">&nbsp;<input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close(false);"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;
    win.Name = Title || null;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.Show();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();

};

IDC_WindowManager.prototype.Alert = function(Message, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + Icon + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Message + '</td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(true);"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;
    win.Name = Title || null;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.Show();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

IDC_WindowManager.prototype.PromptMultiline = function(Message, Text, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'var tbl = document.getElementById("tblFrame");' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="' + Icon + '" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Message + '<br><textarea id="txtValue" name="txtValue" style="width:250px;height:150px;font-family:arial;font-size:12px;">' + Text + '</textarea></td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(txtValue.value);">&nbsp;<input id="cmdCancel" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close();"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;
    win.Name = Title || null;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.Show();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

IDC_WindowManager.prototype.Prompt = function(Message, Text, Title, Icon, CallBack)
{
    var html = '<html>' +
                '<head>' +
                '<link href="' + this.Layout + '" type="text/css" rel="stylesheet" />' +
                '<script language="javascript" type="text/javascript">\n' +
                'function init() { setTimeout(function() {_init();},50); };'+
                'function _init()'+
                '{' +
                    'var win = window.frameElement.IDCWindow;' +
                    'win.SetSize(tblFrame.offsetWidth, tblFrame.offsetHeight);' +
                    'win.CenterOwner();' +
                    'win.ShowDialog();' +
                    'document.getElementById("cmdOkay").focus();' +
                '};' +
                '</script>' +
                '</head>' +
                '<body class="IDCWMDialogBack" onload="_init();">' +
                '<table id="tblFrame" border="0" cellpadding="0" cellspacing="0">' +
                '<tr><td class="IDCWMDialogContentBackground"><img src="" onload="init();" onerror="this.style.display=\'none\';init();" align="left" style="margin-right:8px;">' + Message + '<br><input type="text" id="txtValue" name="txtValue" style="width:100%;" value="' + Text + '"></td></tr>' +
                '<tr><td nowrap class="IDCWMDialogButtonsBackground" style="padding-left:160px;"><input id="cmdOkay" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="OK" onclick="window.frameElement.IDCWindow.Close(txtValue.value);">&nbsp;<input id="cmdCancel" onfocus="this.className=\'IDCWMDialogButtonFocused\';" onblur="this.className=\'IDCWMDialogButton\';" class="IDCWMDialogButton" type="button" value="Annuller" onclick="window.frameElement.IDCWindow.Close();"></td></tr>' +
                '</table>' +
                '</body>' +
                '</html>';

    var win = this.CreateWindow(this.BlankPage);
    win.OnClose = CallBack;
    win.Name = Title || null;
    win.Resizeable = false;
    win.Minimizeable = false;
    win.Maximizeable = false;
    win.Width=60;
    win.Height=20;
    
    win.CenterOwner();
    win.Show();
    
    win.ContentIframe.contentWindow.document.write(html);
    win.ContentIframe.contentWindow.document.close();
    
};

/*
    Function: IDC_WindowManager.CreateDatePicker
    Description: Converts a html input textbox to a datepicker
    Argument: <INPUT> InputControl
    Argument: function OnNewDateSelected
    Argument: string DateFormat (yyyy-mm-dd (default), dd-mm-yyyy, mm-dd-yyyy, etc.)
*/
IDC_WindowManager.prototype.CreateDatePicker = function(InputControl, OnNewDateSelected, DateFormat)
{
    InputControl.DateFormat = DateFormat || 'yyyy-mm-dd';
    var dp = this.Document.createElement('DIV');
    dp.innerHTML='Pick Date here';
    
    InputControl.DatePicker = dp;
    InputControl.WindowManager = this;
    
    InputControl.onfocus = function()
        {
            this.WindowManager.ContextMenuManager.Show(this.DatePicker, 0,0);
        };
    
};


/*
    Function: IDC_Window.CreateDatePicker
    Description: Converts a html input textbox to a datepicker
    Argument: <INPUT> InputControl
    Argument: string DateFormat (DD-MM-YYYY (default), YYYY-MM-DD, MM-DD-YYYY, etc.)
    Argument: function OnNewDateSelected
    Argument: Array Weekday short names ['Mo','Tu','We','Th','Fr','Sa','Su']
    Argument: Array Month names ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec']
*/
IDC_Window.prototype.CreateDatePicker = function (InputControl, DateFormat, OnNewDateSelected, ShortWeekDays, MonthNames, DropDownButton, ScrollArea) {
    var dp = this.Document.createElement('DIV');

    InputControl.DateFormat = DateFormat || 'YYYY-MM-DD';
    InputControl.DatePicker = dp;
    InputControl.Window = this;
    InputControl.CurrentDate = null;
    InputControl.DayControls = new Array();
    InputControl.ShortWeekDays = ShortWeekDays || ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];
    InputControl.MonthNames = MonthNames || ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    InputControl.OnNewDateSelected = OnNewDateSelected;
    InputControl.OnChange = InputControl.onchange;
    InputControl.onchange = function () { if (this.OnNewDateSelected) { this.OnNewDateSelected(this); } if (this.OnChange) this.OnChange(); };
    InputControl.ScrollArea = ScrollArea;

    var ipFocuser = this.Document.createElement('INPUT');
    //dp.appendChild(ipFocuser);
    dp.FocusObject = ipFocuser;

    //build the table
    var tbl = this.Document.createElement('TABLE');
    //tbl.style.backgroundColor='white';
    //tbl.style.border='solid 1px black';
    tbl.border = 0;
    tbl.cellPadding = 0;
    tbl.cellSpacing = 0;
    tbl.className = 'IDCWMWindowContextMenuFrame';

    var trH = tbl.insertRow(-1);
    var tdL = trH.insertCell(-1);
    var tdM = trH.insertCell(-1);
    tdM.colSpan = 5;
    tdM.innerHTML = 'Month';
    var tdR = trH.insertCell(-1);

    InputControl.CurrentMonthAndYear = tdM;

    tdL.innerHTML = '«';
    tdR.innerHTML = '»';

    tdL.setAttribute('dontcollapse', 'true');
    tdM.setAttribute('dontcollapse', 'true');
    tdR.setAttribute('dontcollapse', 'true');

    tdL.Control = InputControl;
    tdM.Control = InputControl;
    tdR.Control = InputControl;

    tdL.style.cursor = 'pointer';
    tdR.style.cursor = 'pointer';

    tdL.align = 'left';
    tdM.align = 'center';
    tdR.align = 'right';

    tdL.onclick = function () { this.Control.CurrentDate = this.Control.Window.WindowManager.Core.Localization.DateAdd(this.Control.CurrentDate, -1, 'months'); this.Control.value = this.Control.CurrentDate.format(this.Control.DateFormat); this.Control.focus(); this.Control.RefreshDateView(); this.Control.onchange(); };
    tdM.onclick = function () { this.Control.focus(); };
    tdR.onclick = function () { this.Control.CurrentDate = this.Control.Window.WindowManager.Core.Localization.DateAdd(this.Control.CurrentDate, 1, 'months'); this.Control.value = this.Control.CurrentDate.format(this.Control.DateFormat); this.Control.focus(); this.Control.RefreshDateView(); this.Control.onchange(); };

    trH.Control = InputControl;
    trH.onmousedown = function () { this.Control.MouseDown = true; };
    trH.onmouseup = function () { this.Control.MouseDown = false; };



    var trHD = tbl.insertRow(-1);
    InputControl.DayHeaders = new Array();
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));
    InputControl.DayHeaders.push(trHD.insertCell(-1));

    var i = 0;
    for (var y = 0; y < 6; y++) {
        var tr = tbl.insertRow(-1);
        for (var x = 0; x < 7; x++) {
            i++;
            var td = tr.insertCell(-1);

            InputControl.DayControls.push(td);

            td.innerHTML = i;
            td.style.cursor = 'pointer';
            td.Control = InputControl;
            td.className = 'IDCWMWindowContextMenuText';
            td.onmouseover = function () { this.className = 'IDCWMWindowContextMenuTextHover'; };
            td.onmouseout = function () { this.className = 'IDCWMWindowContextMenuText'; };
            td.onmousedown = function () { this.Control.CurrentDate = this.Date; this.Control.value = this.Date.format(this.Control.DateFormat); this.Control.focus(); this.Control.select(); this.Control.onchange(); };
        }
    }

    dp.appendChild(tbl);

    InputControl.ParseDate = function (stringDt) {
        if (!stringDt) stringDt = this.value;
        return Date.parse(stringDt, this.DateFormat || 'D-M-YYYY') || new Date();
    };

    InputControl.RefreshDateView = function () {
        //set to the first og the month, and keep going back til day is monday.
        var dt = new Date(this.CurrentDate.getFullYear(), this.CurrentDate.getMonth(), 1);


        //alert(dt + " off " + this.CurrentDate);

        while (dt.getDay() != 1) {
            dt = this.Window.WindowManager.Core.Localization.DateAdd(dt, -1, 'days');
        }

        //month header
        this.CurrentMonthAndYear.innerHTML = this.MonthNames[this.CurrentDate.getMonth()] + ' ' + this.CurrentDate.getFullYear();

        //day headers
        for (var d = 0; d < this.DayHeaders.length && d < this.ShortWeekDays.length; d++) {
            this.DayHeaders[d].innerHTML = this.ShortWeekDays[d];
        }

        var i = 0;
        for (var y = 0; y < 6; y++) {
            for (var x = 0; x < 7; x++) {
                this.DayControls[i].innerHTML = dt.getDate(); // +"/" + dt.getMonth();
                this.DayControls[i].Date = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate()); //new dt;
                dt = this.Window.WindowManager.Core.Localization.DateAdd(dt, 1, 'days');

                //handle daylight savings
                var h = dt.getHours();
                if (h == 23)
                    dt = this.Window.WindowManager.Core.Localization.DateAdd(dt, 1, 'hours');
                else if (h == 1)
                    dt = this.Window.WindowManager.Core.Localization.DateAdd(dt, -1, 'hours');

                i++;
            }
        }

    };


    InputControl.onclick = function () {

        if (this.disabled) return;

        if (this.TimerId) {
            clearTimeout(this.TimerId);
            this.TimerId = null;
        }
        //parseFormat = function(CurDate, Mask, DefaultTo)

        this.CurrentDate = this.ParseDate();
        this.RefreshDateView();


        var pos = this.Window.WindowManager.Core.DOM.GetObjectPosition(this);

        if (this.ScrollArea) {
            pos[0] -= this.ScrollArea.scrollLeft;
            pos[1] -= this.ScrollArea.scrollTop;
        }

        this.select();

        this.Window.WindowManager.ContextMenuManager.ShowFromWindow(this.Window, this.DatePicker, pos[0], pos[1] + this.offsetHeight);
        //this.DatePicker.FocusObject.focus();
    };

    if (DropDownButton) {
        DropDownButton.Input = InputControl;
        DropDownButton.onclick = function () { this.Input.onclick(); };
    }

    InputControl.onmousedown = function () { this.MouseDown = true; };
    InputControl.onmouseup = function () { this.MouseDown = false; };
    InputControl.onfocus = function () {
        if (!this.MouseDown) this.onclick();
    };

    InputControl.onblur = function () {
        var me = this;
        me.TimerId = setTimeout(function () {
            if (me.MouseDown) {
                me.onblur();
            }
            else {
                me.TimerId = null;
                me.Window.WindowManager.ContextMenuManager.Hide();
            }
        }, 50);


    };

};



IDC_Window.prototype.BindTooltipToObject = function(Core, Object, Content)
{
    Object.Window = this;
    Object.Core = Core;
    Object.Content = Content;
    Object.onmouseover = function() 
        { 
            
            var me = this; 
            this.TimerId = setTimeout(function() 
                { 
                    me.TimerId=null; 
                    me.Window.ShowTooltip(me.Content, me.Core); 
                },1000
                ); 
         };
    
    Object.onmouseout = function() 
        { 
            var me = this; 
            setTimeout(function() 
            { 
                if(me.TimerId) 
                { 
                    clearTimeout(me.TimerId); 
                    me.TimerId=null; 
                 }; 
                 
                 if(!me.Window.WindowManager.ToolTipWindow.HasFocus) 
                    me.Window.HideTooltip();
             },10
             );         
         };
};

IDC_Window.prototype.ShowTooltip = function(Content, Core)
{

    if(!this.WindowManager.ToolTipWindow)
    {
        this.WindowManager.ToolTipWindow = this.WindowManager.__MakeGhostWindowFocused();
        this.WindowManager.ToolTipWindow.Content = this.WindowManager.Document.createElement('DIV');
        this.WindowManager.ToolTipWindow.appendChild(this.WindowManager.ToolTipWindow.Content);
        
        this.WindowManager.ToolTipWindow.WindowManager = this.WindowManager;
        
        this.WindowManager.ToolTipWindow.Content.style.whiteSpace='nowrap';
        this.WindowManager.ToolTipWindow.Content.style.position='absolute';
        this.WindowManager.Desktop.Control.appendChild(this.WindowManager.ToolTipWindow);
        
        this.WindowManager.ToolTipWindow.onmouseover = function() { this.HasFocus=true; };
        this.WindowManager.ToolTipWindow.onmouseout = function() { this.HasFocus=false; this.WindowManager.HideTooltip(); };
        
        //TODO: Selection to copy paste?
        this.WindowManager.ToolTipWindow.Content.onclick = function()
            {
		        if (document.selection) 
		        {
		            var range = document.body.createTextRange();
 	                range.moveToElementText(this);
		            range.select();
		        }
		        else if (window.getSelection) 
		        {
		            var range = document.createRange();
		            range.selectNode(this);
		            window.getSelection().addRange(range);
		        }

            };
        
    }
    
    if(this.isIE)
        this.WindowManager.ToolTipWindow.Content.innerText = Content;
    else
        this.WindowManager.ToolTipWindow.Content.textContent = Content;
       
    var x = Core.Mouse.X;
    var y = Core.Mouse.Y;
    
    //this.WindowManager.ToolTipWindow.Content.innerHTML= x + "x" + y;
    
    var wd = (this.ContentIframeOuterBox.offsetWidth - this.ContentIframe.offsetWidth);
    var hd = (this.ContentIframeOuterBox.offsetHeight - this.ContentIframe.offsetHeight);
    
    
    if(this.WindowState==2)
    {
        x+=wd;
        y+=this.Titlebar.offsetHeight;
        
    }
    else
    {
        x+=this.Left + wd;
        y+=this.Top + hd + this.Titlebar.offsetHeight;
    }  

    y+=this.WindowManager.Desktop.Control.offsetTop;
       
    this.WindowManager.ToolTipWindow.style.left = x; 
    this.WindowManager.ToolTipWindow.style.top = y; 
    
    this.WindowManager.ToolTipWindow.style.display='';
    
    this.WindowManager.ToolTipWindow.style.width = this.WindowManager.ToolTipWindow.Content.offsetWidth;
    this.WindowManager.ToolTipWindow.style.height = this.WindowManager.ToolTipWindow.Content.offsetHeight;
   
};

IDC_Window.prototype.HideTooltip = function()
{
    if(!this.WindowManager.ToolTipWindow) return;

    if(this.WindowManager.isIE)
        this.WindowManager.ToolTipWindow.Content.innerText = '';
    else
        this.WindowManager.ToolTipWindow.Content.textContent = '';
        
    this.WindowManager.ToolTipWindow.style.display='none';
};


var IDS_DragDropManager = function (windowManager, ParentObject) {
    this.WindowManager = windowManager;
    this.DragConent = null;
    this.Document = this.WindowManager.Document;
    this.ParentObject = ParentObject;

    //Overlay
    this.__Focusable = null;
    this.__TransparentOverlay = null;
    this.__DragContainerContentView = null;
    this.__MakeOverlay();
};

IDS_DragDropManager.prototype.BeginDrag = function (DragContent, DragIcon, DragText) {
    this.__DragContainerContentView.innerHTML = DragText;
    this.__DragContainerContentView.style.display = '';
    this.__TransparentOverlay.style.display = '';

    this.__DragContainerContentView.style.left = this.WindowManager.Core.Mouse.X - (this.__DragContainerContentView.offsetWidth / 2);
    this.__DragContainerContentView.style.top = this.WindowManager.Core.Mouse.Y + 16;

};

IDS_DragDropManager.prototype.DoDrop = function () {

};


IDS_DragDropManager.prototype.AbortDrag = function () {
    this.__DragContainerContentView.style.display = 'none';
    this.__TransparentOverlay.style.display = 'none';
};

IDS_DragDropManager.prototype.__MakeOverlay = function () {

    //Drag Container
    this.__DragContainerContentView = this.Document.createElement('DIV');
    this.__DragContainerContentView.style.position = 'absolute';
    this.__DragContainerContentView.style.zIndex = 899999;
    this.__DragContainerContentView.style.left = 0;
    this.__DragContainerContentView.style.top = 0;
    this.__DragContainerContentView.style.display = 'none';
    this.__DragContainerContentView.style.color = 'white';
    this.__DragContainerContentView.style.backgroundColor = 'red';

    //Add object
    this.ParentObject.appendChild(this.__DragContainerContentView);

    //Overlay
    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position = 'absolute';
    this.__TransparentOverlay.style.zIndex = 900001;
    this.__TransparentOverlay.style.width = '100%';
    this.__TransparentOverlay.style.height = '100%';
    this.__TransparentOverlay.style.left = 0;
    this.__TransparentOverlay.style.top = 0;
    this.__TransparentOverlay.style.display = 'none';
    this.__TransparentOverlay.style.color = 'white';
    this.__TransparentOverlay.style.backgroundColor = '';
    this.__TransparentOverlay.style.opacity = '0.5';

    this.__TransparentOverlay.oncontextmenu = function (e) { alert('ctx2'); return false; };

    var dd = this;
    this.__TransparentOverlay.DragDropManager = dd;

    this.__TransparentOverlay.onmouseup = function () {

        this.DragDropManager.__TransparentOverlay.style.display = 'none';
        this.DragDropManager.__DragContainerContentView.style.display = 'none';
    };

    this.__TransparentOverlay.onmousemove = function () {
        this.DragDropManager.__DragContainerContentView.style.left = this.DragDropManager.WindowManager.Core.Mouse.X - (this.DragDropManager.__DragContainerContentView.offsetWidth / 2);
        this.DragDropManager.__DragContainerContentView.style.top = this.DragDropManager.WindowManager.Core.Mouse.Y + 16;
    };

    if (this.WindowManager.isIE) {
        this.__TransparentOverlay.style.backgroundImage = 'url(' + this.WindowManager.Core.WebsiteUrl + 'images/spacer.gif)';
    }

    //Add object
    this.ParentObject.appendChild(this.__TransparentOverlay);
};
var HSplitter = function (OwnerDocument, OwnerContainer, objectTop, objectBottom, callbackAfterSplit)
{
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.ObjectTop = objectTop;
    this.ObjectBottom = objectBottom;
    this.CallbackAfterSplit = callbackAfterSplit;
    
    this.SplitterObject = this.Document.createElement('DIV');
    this.SplitterObject.className='HSplitter';
    this.SplitterObject.style.position='absolute';
    this.SplitterObject.style.width = this.Container.offsetWidth;
    this.SplitterObject.style.height = this.Container.offsetHeight;
    this.SplitterObject.style.display='none';
    this.SplitterObject.style.zIndex=249999;
    this.Container.appendChild(this.SplitterObject);
    
    //OVERLAY
    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position='absolute';
    this.__TransparentOverlay.style.zIndex=250000;
    this.__TransparentOverlay.style.width='100%';
    this.__TransparentOverlay.style.height='100%';
    this.__TransparentOverlay.style.left=0;
    this.__TransparentOverlay.style.top=0;
    this.__TransparentOverlay.style.display='none';
    this.__TransparentOverlay.oncontextmenu = function(e) { this.HSplitter.Hide(); return false; };
    this.__TransparentOverlay.style.cursor='s-resize';
    
    if(document.all)
    {
        this.__TransparentOverlay.style.backgroundImage='url(' + WebsiteURL + 'images/spacer.gif)';
        //this.__TransparentOverlay.style.backgroundColor='blue';
        //this.__TransparentOverlay.style.filter = 'alpha(opacity=0)';
    }
    this.__TransparentOverlay.HSplitter = this;
    this.__TransparentOverlay.onclick = function() { this.HSplitter.Hide(); };
    this.__TransparentOverlay.onmouseup = function() { this.HSplitter.Hide(); };
    this.__TransparentOverlay.onmousemove = function() { this.HSplitter.Move(); };
    
    this.Document.body.appendChild(this.__TransparentOverlay);
 
    //EVENTS
    this.Container.HSplitter = this;
    this.Container.onmousedown = function() { this.HSplitter.Show(); };   
    this.Container.onmouseup = function() { this.HSplitter.Hide(); };   
    
    
};

HSplitter.prototype.Move = function()
{
    this.SplitterObject.style.top = localCore.Mouse.Y - parseInt(this.SplitterObject.offsetHeight/2);
};

HSplitter.prototype.Show = function()
{
    
    this.__TransparentOverlay.style.display='';
    if(document.all)
    {
        this.__TransparentOverlay.style.backgroundImage='url(' + WebsiteURL + 'images/spacer.gif)';
        //this.__TransparentOverlay.style.backgroundColor='blue';
        //this.__TransparentOverlay.style.filter = 'alpha(opacity=0)';
    }
    
    this.StartY = localCore.Mouse.Y;
    
    this.SplitterObject.style.display='';
    this.SplitterObject.style.left = getGlobalPosition(this.Container)[0];
    this.SplitterObject.style.top = localCore.Mouse.Y - parseInt(this.SplitterObject.offsetHeight/2);
    this.SplitterObject.style.width = this.Container.offsetWidth;
    this.SplitterObject.style.height = this.Container.offsetHeight;
};

HSplitter.prototype.Hide = function()
{
    
    this.__TransparentOverlay.style.display='none';
    var diff = this.StartY -parseInt(localCore.Mouse.Y-this.SplitterObject.offsetHeight/2);
    
    this.SplitterObject.style.display='none';
    
    var oLW = this.ObjectTop.offsetHeight - diff;
    var oRW = this.ObjectBottom.offsetHeight + diff;

    if(oLW<10)
    {
        var tmp = oLW - 10;
        oRW += tmp;
        oLW = 10;
    }
    
    if(oRW<10)
    {
        var tmp = oRW - 10;
        oLW += tmp;
        oRW = 10;
    }
    
    this.ObjectTop.style.height= oLW;
    this.ObjectBottom.style.height= oRW;
    
    if(this.CallbackAfterSplit)
    {
        this.CallbackAfterSplit(oLW,oRW);
    }
};



var VSplitter = function (OwnerDocument, OwnerContainer, objectLeft, objectRight, callbackAfterSplit)
{
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.ObjectLeft = objectLeft;
    this.ObjectRight = objectRight;
    this.CallbackAfterSplit = callbackAfterSplit;
    
    this.SplitterObject = this.Document.createElement('DIV');
    this.SplitterObject.className='VSplitter';
    this.SplitterObject.style.position='absolute';
    this.SplitterObject.style.width = this.Container.offsetWidth;
    this.SplitterObject.style.height = this.Container.offsetHeight;
    this.SplitterObject.style.display='none';
    this.SplitterObject.style.zIndex=249999;
    this.Container.appendChild(this.SplitterObject);
    
    //OVERLAY
    this.__TransparentOverlay = this.Document.createElement('DIV');
    this.__TransparentOverlay.style.position='absolute';
    this.__TransparentOverlay.style.zIndex=250000;
    this.__TransparentOverlay.style.width='100%';
    this.__TransparentOverlay.style.height='100%';
    this.__TransparentOverlay.style.left=0;
    this.__TransparentOverlay.style.top=0;
    this.__TransparentOverlay.style.display='none';
    this.__TransparentOverlay.oncontextmenu = function(e) { this.HSplitter.Hide(); return false; };
    this.__TransparentOverlay.style.cursor='w-resize';
    
    if(document.all)
    {
        this.__TransparentOverlay.style.backgroundImage='url(' + WebsiteURL + 'images/spacer.gif)';
        //this.__TransparentOverlay.style.backgroundColor='blue';
        //this.__TransparentOverlay.style.filter = 'alpha(opacity=0)';
    }
    this.__TransparentOverlay.VSplitter = this;
    this.__TransparentOverlay.onclick = function() { this.VSplitter.Hide(); };
    this.__TransparentOverlay.onmouseup = function() { this.VSplitter.Hide(); };
    this.__TransparentOverlay.onmousemove = function() { this.VSplitter.Move(); };
    
    this.Document.body.appendChild(this.__TransparentOverlay);
 
    //EVENTS
    this.Container.VSplitter = this;
    this.Container.onmousedown = function() { this.VSplitter.Show(); };   
    this.Container.onmouseup = function() { this.VSplitter.Hide(); };   
    
    
};

VSplitter.prototype.Move = function()
{
    this.SplitterObject.style.left = localCore.Mouse.X - parseInt(this.SplitterObject.offsetWidth/2);
};

VSplitter.prototype.Show = function()
{
    
    this.__TransparentOverlay.style.display='';
    if(document.all)
    {
        this.__TransparentOverlay.style.backgroundImage='url(' + WebsiteURL + 'images/spacer.gif)';
        //this.__TransparentOverlay.style.backgroundColor='blue';
        //this.__TransparentOverlay.style.filter = 'alpha(opacity=0)';
    }
    
    this.StartX = localCore.Mouse.X;
    
    this.SplitterObject.style.display='';
    this.SplitterObject.style.left = localCore.Mouse.X - parseInt(this.SplitterObject.offsetWidth/2);
    this.SplitterObject.style.top = getRealPosition(this.Container)[1];
    this.SplitterObject.style.width = this.Container.offsetWidth;
    this.SplitterObject.style.height = this.Container.offsetHeight;
};

VSplitter.prototype.Hide = function()
{
    
    this.__TransparentOverlay.style.display='none';
    var diff = this.StartX -parseInt(localCore.Mouse.X-this.SplitterObject.offsetWidth/2);
    
    this.SplitterObject.style.display='none';
    
    var oLW = this.ObjectLeft.offsetWidth - diff;
    var oRW = this.ObjectRight.offsetWidth + diff;

    if(oLW<10)
    {
        var tmp = oLW - 10;
        oRW += tmp;
        oLW = 10;
    }
    
    if(oRW<10)
    {
        var tmp = oRW - 10;
        oLW += tmp;
        oRW = 10;
    }
    
    this.ObjectLeft.style.width= oLW;
    this.ObjectRight.style.width= oRW;
    
    if(this.CallbackAfterSplit)
    {
        this.CallbackAfterSplit(oLW, oRW);
    }
};
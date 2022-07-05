var Progressbar = function (OwnerDocument, OwnerContainer, Width)
{
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    var div = this.Document.createElement('DIV');
    div.className='progressbarBack';
    div.style.width=Width || 266;
    
    this.Progressbar = this.Document.createElement('IMG');
    this.Progressbar.src='images/spacer.gif';
    this.Progressbar.className='progressbarPosition';
    
    this.Container.appendChild(div);
    div.appendChild(this.Progressbar);
    
    this.SetProgress(0);
    
    this.Percent=0;
    this.ReachPercent=0;
    this.Step=1;
    
    this.OnAnimateEvent = null;
    this.OnAnimateDoneEvent = null;
    
};

Progressbar.prototype.IsBad = function()
{
    return this.Progressbar.className=='progressbarPositionBad';
};

Progressbar.prototype.IsGood = function()
{
    return this.Progressbar.className=='progressbarPosition';
};

Progressbar.prototype.ProgressIsBad = function()
{
    this.Progressbar.className='progressbarPositionBad';
};
Progressbar.prototype.ProgressIsGood = function()
{
    this.Progressbar.className='progressbarPosition';
};

Progressbar.prototype.AnimateProgress = function(percent)
{
    this.ReachPercent=percent;
    this.Step = (this.ReachPercent - this.Percent) / 25;
    
    if(this.Step<1) this.Step=1;
    if(this.Step>20) this.Step=20;
    
    this._AnimateStep();
};

Progressbar.prototype._AnimateStep = function()
{
    this.Percent += this.Step;
    if(this.Percent > this.ReachPercent) this.Percent = this.ReachPercent;
    
    this.SetProgress(this.Percent);
    
    if(this.OnAnimateEvent)
    {
        this.OnAnimateEvent(this.Percent);
    }
    
    if(this.Percent < this.ReachPercent)
    {
        var me = this;
        setTimeout( function() { me._AnimateStep(); } ,10);
    }
    else
    {
        if(this.OnAnimateDoneEvent) this.OnAnimateDoneEvent();
    }
    
};

Progressbar.prototype.SetProgress = function(value)
{
    value = parseInt(value);
    
    if(value>100) value=100;
    if(value<0) value=0;
    
    if(value==0)
        this.Progressbar.style.visibility='hidden';
    else
    {
        this.Progressbar.style.width= value + '%';
        this.Progressbar.style.visibility='visible';
    }
};

Progressbar.prototype.ResetTo = function(value)
{
    
    value = parseInt(value);
    
    if(value>100) value=100;
    if(value<0) value=0;
    
    this.Percent=value;
    this.ReachPercent=value;
    this.Step=1;
    
    if(value==0)
        this.Progressbar.style.visibility='hidden';
    else
    {
        this.Progressbar.style.width= value + '%';
        this.Progressbar.style.visibility='visible';
    }
    
    if(this.OnAnimateEvent)
    {
        this.OnAnimateEvent(this.Percent);
    }
};
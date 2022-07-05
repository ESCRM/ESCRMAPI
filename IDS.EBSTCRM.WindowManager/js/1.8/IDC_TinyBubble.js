var TinyBubble = function(document, container, x, y, text, hideInMS)
{
    this.Document = document;
    this.Container = container;
    this.X = x;
    this.Y = y;
    this.Text = text;
    this.HideInMS = hideInMS;
    this.isIE = document.all?true:false;
    
    this.Control = this.Document.createElement('TABLE');
    var tb = this.Document.createElement('TBODY');
    var tr1 = this.Document.createElement('TR');
    var td1 = this.Document.createElement('TD');
    var tr2 = this.Document.createElement('TR');
    
    td1.className='tinyBubbleTop';
    
    this.TextControl = this.Document.createElement('TD');
    this.TextControl.className='tinyBubbleText';
    
    this.Control.appendChild(tb);
    tb.appendChild(tr1);
    tr1.appendChild(td1);
    
    tb.appendChild(tr2);
    tr2.appendChild(this.TextControl);
    
    this.Control.style.position='absolute';
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    this.Control.style.visibility='hidden';
    this.TextControl.innerHTML = this.Text;
    
    this.Container.appendChild(this.Control);
    
    this.Control.style.left = x - (this.Control.offsetWidth/2);
    this.Control.style.top = y;
    this.Control.style.zIndex=250000;
    
    var me = this;
    
    this.Visibility=0;
    this.Show();
    
};

TinyBubble.prototype.Hide = function()
{
    this.Visibility-=10;
    if(this.Visibility<=0)
    {
        this.Container.removeChild(this.Control);
        delete this.Control;
        var me = this;
        delete me;
    }
    else
    {
        if(this.isIE)
            this.Control.style.filter = 'alpha(opacity=' + this.Visibility + ')'; 
        else
            this.Control.style.MozOpacity = (this.Visibility / 100); 
            
        var me = this;
        setTimeout(function() { me.Hide(); }, 25);
    }
};

TinyBubble.prototype.Show = function()
{
    this.Visibility+=10;
    if(this.Visibility>=110)
    {
        this.Visibility=100;
        
        if(this.HideInMS>0)
        {
            var me = this;
            setTimeout(function() { me.Hide(); }, me.HideInMS);
        }
        else
        {   
            this.Control.TinyBubble = this;   
            this.Control.onclick = function()
                                        {
                                            this.TinyBubble.Hide();
                                        };
        }
    }
    else
    {
        if(this.isIE)
            this.Control.style.filter = 'alpha(opacity=' + this.Visibility + ')'; 
        else
            this.Control.style.MozOpacity = (this.Visibility / 100); 
        
        this.Control.style.visibility='';
        
        var me = this;
        setTimeout(function() { me.Show(); }, 25);
    }
};
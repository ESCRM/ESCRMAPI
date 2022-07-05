//Detailed listview item
var CRM_TimeUsageOverviewItem = function(listview, item)
{
    this.ViewName='Timeoversigts liste';
    if(!listview || !item) return this;
    
    this.Control = document.createElement('TABLE');
    this.Control.cellPadding=0;
    this.Control.cellSpacing=0;
    this.Control.border=0;
    this.Control.className='listviewDTLine';
    
    
    this.Control.tr = this.Control.insertRow(-1);
    
    cr = this.Control.tr.insertCell(-1).appendChild(this.createDiv(item.Items[0], listview.ColumnHeaders[0].__Width + listview.__UseDimPX, 'listviewDTIcon', 'url(' + item.Icon16 + ')', listview.ColumnHeaders[0].TextAlign));
    
    for(var i=1;i<listview.ColumnHeaders.length && i<item.Items.length;i++)
    {
        this.Control.tr.insertCell(-1).appendChild(this.createDiv(item.Items[i], listview.ColumnHeaders[i].__Width + listview.__UseDimPX, 'listviewDTItem', '', listview.ColumnHeaders[i].TextAlign ));
    }
    
        
        
    //EVENTS
    this.SelectionChanged = function(item)
                                {
                                    item.Control.className=(item.IsSelected ? 'listviewDTLine_s' : 'listviewDTLine');
                                };
                                
    this.OnMouseOver = function(item) 
                                { 
                                    if(!item.IsSelected) item.Control.className='listviewDTLine_h';
                                };
    this.OnMouseOut = function(item)
                                { 
                                    if(!item.IsSelected) item.Control.className='listviewDTLine'; 
                                };
                                
    this.OnTextChanged = function(item)
                                {
                                    
                                    for(var i=0;i<item.Items.length && i<item.Control.tr.childNodes.length;i++)
                                    {
                                        item.Control.tr.childNodes[i].childNodes[0].innerHTML=item.Items[i];
                                    }
                                };
                                
    this.OnIconChanged = function(item)
                                {
                                     item.Control.tr.childNodes[0].childNodes[0].style.backgroundImage = 'url(' + item.Icon16 + ')';
                                };
    
    return this;
};

CRM_TimeUsageOverviewItem.prototype.createDiv = function(value, w, cls, bgimg, textalign)
{
    var myValues = value.split('#');
    
    var d = document.createElement('DIV');
    d.innerHTML=myValues[0];
    d.style.color= myValues.length>1 ? '#' + myValues[1] : '';
    d.className=cls;
    d.style.width=w;
    d.style.textAlign=textalign;
    
    if(bgimg) 
        d.style.backgroundImage=bgimg;
        
    return d;
};

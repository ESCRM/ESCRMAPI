//Small icon item
var CRM_SMVContactItem = function(listview, item)
{
    this.ViewName='SMV Detaljevisning';
    if(!listview || !item) return this;
    
    this.Control = listview.Document.createElement('DIV');
    //var span = listview.Document.createElement('SPAN');
        
    this.Control.style.display=listview.__InlineBlock;
    this.Control.style.whiteSpace='nowrap';
    this.Green = (item.Icon16.indexOf('pot.png')>0 ? 'Green' : '');
    this.Control.className='listviewContactType' + this.Green; 
    
    for(var i=0;i<listview.ColumnHeaders.length;i++)
    {

        if(item.Items.length>i)
        {
            var ndv = listview.Document.createElement('DIV');
               
            if(i>0)
            {
                var ndd = listview.Document.createElement('DIV');
                ndd.style.display=listview.__InlineBlock;
                ndd.style.whiteSpace='nowrap';
                 
                ndd.innerHTML = listview.ColumnHeaders[i].Name + ':';
                ndd.className='listviewContactType' + this.Green + 'Text';
                
                ndd.style.width = listview.ColumnHeaders[0].__Width  + (listview.isIE ? '' : 'px'); 
                this.Control.appendChild(ndd);
                
                ndv.style.width = listview.ColumnHeaders[0].__Width  + (listview.isIE ? '' : 'px'); 
                ndv.className='listviewContactType' + this.Green + 'Text';
                
                if(listview.SortingColumn == listview.ColumnHeaders[i])
                {
                    ndv.className='listviewContactTypeSorted' + this.Green + 'Text';
                    ndd.className='listviewContactTypeSorted' + this.Green + 'Text';
                }
            }
            else
            {
                ndv.className='listviewContactTypeHeader' + this.Green + 'Text';
                ndv.style.width = (listview.ColumnHeaders[0].__Width*2)  + (listview.isIE ? '' : 'px');     
                ndv.style.backgroundImage='url(' + item.Icon16 + ')';
            }
            
            ndv.style.display=listview.__InlineBlock;
            ndv.style.whiteSpace='nowrap';
            
            ndv.innerHTML = item.Items[i];
            
            
            
            
            
            
            
            
            this.Control.appendChild(ndv);
            this.Control.appendChild(listview.Document.createElement('BR'));
        }
        

    }
    
    this.Control.style.width=(listview.ColumnHeaders[0].__Width *2) + (listview.isIE ? '' : 'px'); 
  
    
    

    //EVENTS
    this.SelectionChanged = function(item)
                                {
                                    item.Control.className='listviewContactType' + (item.Icon16.indexOf('pot.png')>0 ? 'Green' : '') + (item.IsSelected ? '_s' : '');
                                };
                                
    this.OnMouseOver = function(item) 
                                { 
                                    if(!item.IsSelected) item.Control.className='listviewContactType' + (item.Icon16.indexOf('pot.png')>0 ? 'Green' : '') + '_h';
                                };
    this.OnMouseOut = function(item)
                                { 
                                    if(!item.IsSelected) item.Control.className='listviewContactType' + (item.Icon16.indexOf('pot.png')>0 ? 'Green' : ''); 
                                };
                                
                                
    this.OnTextChanged = function(item)
                                {
                                    item.Control.childNodes[0].innerHTML = item.Items[0];
                                };
                                
    this.OnIconChanged = function(item)
                                {
                                     item.Control.childNodes[0].style.backgroundImage = 'url(' + item.Icon16 + ')';
                                };
    
    return this;
};

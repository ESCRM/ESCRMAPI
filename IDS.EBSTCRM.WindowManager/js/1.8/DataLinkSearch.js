function DataLinkBuild(parentObject, onfocus, onblur)
{
    _DataLinkBuild(parentObject,parentObject.getElementsByTagName('INPUT'), onfocus, onblur);
    _DataLinkBuild(parentObject,parentObject.getElementsByTagName('TEXTAREA'), onfocus, onblur);
    _DataLinkBuild(parentObject,parentObject.getElementsByTagName('SELECT'), onfocus, onblur);
};

function _DataLinkBuild(parentObject, arr, onfocus, onblur)
{
    for(var i=0;i<arr.length;i++)
    {
        var o = arr[i];
        
        var op = null;
             
        try
        {
            if(!o.getAttribute('fieldname') && !o.getAttribute('FieldName') && !o.getAttribute('dbName'))
            {
                op = o.parentNode.parentNode.parentNode.parentNode;
            }
            else
            {
                op = o;
            }
            //op = o.parentNode.parentNode.parentNode.parentNode;
        }
        catch(err)
        {
            op = o;
        }
        if(!op) op = o;
        
        var dl = op.getAttribute('DataLink');

        if (dl == 'CompanyCPR' || dl == 'NNECompanyPhone' || dl == 'NNECompanyAddress1' || dl == 'NNECompanyName' || dl == 'NNECompanyCVR' || dl == 'NNECompanyPNR' || dl == 'DGSCOntactPhone' || dl == 'DGSContactFirstname' || dl == 'DGSCOntactLastname') {
            o.parentObject = parentObject;
            o.PositionObject = op;
            o.onfocus = __DataLinkFocus;
            o.onblur = __DataLinkBlur;
        }
        else {
            o.onfocus = __showDataType; // function () { __showDataType(o); };
            o.onblur = __hideDataType; // function () { __hideDataType(o); };
        }
    }
};

function __showDataType() {
    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = '';
    }
}
function __hideDataType() {
    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = 'none';
    }
}

function __DataLinkFocus() {

    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = '';
    }
    
    if(!this.DataLinkSearchButton)
    {
        this.DataLinkSearchButton = __DataLinkBuildButton(this.parentObject, this.PositionObject.tagName=='INPUT');
        this.DataLinkSearchButton.OwnerInput = this;
    }
    
    if(this.PositionObject.tagName=='INPUT')
    {
        var pos = localCore.DOM.GetObjectPosition(this.PositionObject);
        this.DataLinkSearchButton.style.display='block';
        this.DataLinkSearchButton.style.position='absolute';
        this.DataLinkSearchButton.style.left = pos[0] + 3 + this.offsetWidth - (this.DataLinkSearchButton.offsetWidth/2);
        this.DataLinkSearchButton.style.top =  pos[1] + 3 + (this.offsetHeight/2) + (localCore.isIE ? 0 : 1);

    }
    else
    {
    
        this.DataLinkSearchButton.style.display='block';
        this.DataLinkSearchButton.style.left = this.PositionObject.offsetLeft + 3 + this.offsetWidth - (this.DataLinkSearchButton.offsetWidth/2);
        this.DataLinkSearchButton.style.top =  this.PositionObject.offsetTop + 3 + (this.offsetHeight/2) + (localCore.isIE ? 0 : 1);
    }
};

function __DataLinkBlur() {
    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = 'none';
    }

    if(!this.DataLinkSearchButton)
    {
        this.DataLinkSearchButton = __DataLinkBuildButton();
    }
    this.DataLinkSearchButton.style.display='none';
};

function __DataLinkBuildButton(parentObject, atBody)
{
    var div = document.createElement('A');
    div.innerHTML='&nbsp;';
    div.style.position='absolute';
    div.style.display='none';
    div.className='nneInlineSearch';
    div.href="javascript:void(0);";
        
    if(atBody)
    {
        div.onmousedown=function() {
                                        var nw = w.CreateWindow('SMVInstantSearch.aspx?dbType=' + __DataLinkGetDBObjectType() + '&searchType=companies&query=' + escape(this.OwnerInput.value) + '&name=' + escape(this.OwnerInput.PositionObject.getAttribute('dbName')) , w); 
                                        nw.Variables = this.OwnerInput.value;
                                        nw.OnClose =_instantSearch;
                                   };
    }
    else
    {
        div.onmousedown=function() {
                                        var nw = w.CreateWindow('SMVInstantSearch.aspx?dbType=' + __DataLinkGetDBObjectType() + '&searchType=' + (tw.SelectedNode.ParentTreeviewNode ? 'contacts' : 'companies')+ '&query=' + escape(this.OwnerInput.value) + '&name=' + escape(this.OwnerInput.PositionObject.getAttribute('dbName')) , w); 
                                        nw.Variables = this.OwnerInput.value;
                                        nw.OnClose =_instantSearch;
                                   };
    }

    
    div.style.zIndex=2500000;
    
    if(atBody)
        document.body.appendChild(div);
    else
        parentObject.appendChild(div);
    
    return div;
};


function __DataLinkGetDBObjectType()
{
    var fn = document.location.href;
    if(fn.indexOf('SAMContactsEdit')>-1) return 'SAM';
    if(fn.indexOf('SMVContactsEdit')>-1) return 'SMVPOT';
    if(fn.indexOf('earlyWarningCompanyEdit')>-1) return 'EW';
    return '';
};



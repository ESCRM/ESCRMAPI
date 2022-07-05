var IDC_SqlAutoComplete = function(Document, ParentObject)
{
    this.Document = Document || document;
    this.ParentObject = ParentObject || document.body;
    this.isIE = document.all?true:false;
    
    this.Control = this.Document.createElement('DIV');
    this.Control.style.position='absolute';
    this.Control.style.display='none';
    this.Control.className='autoCompleteBox';
    
    this.ParentObject.appendChild(this.Control);
    
    this.AjaxObject = null;
    
    this.PendingSQL=null;
    
    this.BoundInput = null;
    

    this.SelectedItem = null;
    this.SelectedIndex=-1;
    
};

IDC_SqlAutoComplete.prototype.Show = function(x,y,parent,sql)
{
    this.Control.style.left=x;
    this.Control.style.top=y;
    this.Control.style.zIndex=250000;
    this.BoundInput = parent;

    if(sql!=this.PendingSQL) {
        //Get rid of prevoius controls
        while (this.Control.hasChildNodes()) {
            this.Control.removeChild(this.Control.lastChild);
        }
        var btn = this.Document.createElement('DIV');
        btn.className = 'cmdAutoComplete';
        btn.innerHTML = '...S&oslash;ger...';
        this.Control.appendChild(btn);
        this.Control.style.display = '';

        this.PendingSQL=sql;
        var me = this;
        setTimeout(function() { me.Execute(sql);  me = null;},750);
    
    }
};

IDC_SqlAutoComplete.prototype.Hide = function()
{
    this.Control.style.display='none';
};

IDC_SqlAutoComplete.prototype.Execute = function(sql)
{
    if (sql == this.PendingSQL) {


        if(this.AjaxObject)
        {
                this.AjaxObject.xmlhttp.abort();
        }
           
        this.AjaxObject = new localCore.Ajax();
        this.AjaxObject.requestFile = 'ajax/ReportGeneratorAutoComplete.aspx';
        this.AjaxObject.encVar('sql',sql);

        var me = this;
        
        this.AjaxObject.OnCompletion = function(){ me.__Execute(); me = null; }; 
        this.AjaxObject.OnError = function(){ me.__Execute(); me = null;};
        this.AjaxObject.OnFail = function(){  me.__Execute(); me = null;};
        this.AjaxObject.RunAJAX();
    }
};

IDC_SqlAutoComplete.prototype.__Execute = function () {
    var result = this.AjaxObject.Response;
    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    delete this.AjaxObject;
    this.AjaxObject = null;

    while (this.Control.hasChildNodes()) {
        this.Control.removeChild(this.Control.lastChild);
    }


    var div = this.Document.createElement('DIV');
    div.innerHTML = result;

    this.SelectedItem = null;
    this.SelectedIndex = -1;

    if (this.BoundInput) {
        this.BoundInput.AutoComplete = this;
        this.BoundInput.onkeydown = function (e) {
            if (!e) e = event;

            var keynum = null;
            if (this.AutoComplete.isIE) // IE
                keynum = e.keyCode;
            else if (e.which) // Netscape/Firefox/Opera
                keynum = e.which;

            if (keynum == 40) {
                this.AutoComplete.SelectNext();
            }
            else if (keynum == 38) {
                this.AutoComplete.SelectPrevious();
            }
            else if (keynum == 13 && this.AutoComplete.SelectedItem) {
                this.AutoComplete.SelectedItem.onclick();
            }
        };
    }

    var cts = div.getElementsByTagName('b');

    if (cts.length > 0) {

        for (var i = 0; i < cts.length; i++) {
            var btn = this.Document.createElement('DIV');
            btn.className = 'cmdAutoComplete';
            btn.onmouseover = function () {
                this.className += 'Hover';
            };

            btn.onmouseout = function () {
                this.className = this.className.replace('Hover', '');
            };

            if (this.isIE)
                btn.innerText = cts[i].innerText;
            else
                btn.textContent = cts[i].textContent;

            btn.AutoComplete = this;
            btn.onclick = function () {
                if (this.AutoComplete.BoundInput) {
                    this.AutoComplete.BoundInput.value = (this.AutoComplete.isIE ? this.innerText : this.textContent);
                }
                this.AutoComplete.Hide();
            };

            this.Control.appendChild(btn);
        }
    }
    else {
        var noData = this.Document.createElement('DIV');
        noData.innerHTML = 'Ingen resultater';
        this.Control.appendChild(noData);
    }

    this.Control.style.display = '';

};

IDC_SqlAutoComplete.prototype.SelectNext = function()
{
    if(this.Control.childNodes.length>0)
    {
        if(this.SelectedItem)
            this.SelectedItem.className='cmdAutoComplete';
            
        this.SelectedIndex++;
        if(this.SelectedIndex == this.Control.childNodes.length)
        {
            this.SelectedIndex=0;
        }
        
        this.SelectedItem = this.Control.childNodes[this.SelectedIndex];
        this.SelectedItem.className='cmdAutoCompleteSelected';
        
    }
};

IDC_SqlAutoComplete.prototype.SelectPrevious = function()
{
    if(this.Control.childNodes.length>0)
    {
        if(this.SelectedItem)
            this.SelectedItem.className='cmdAutoComplete';
            
        this.SelectedIndex--;
        if(this.SelectedIndex < 0)
        {
            this.SelectedIndex=this.Control.childNodes.length-1;
        }
        
        this.SelectedItem = this.Control.childNodes[this.SelectedIndex];
        this.SelectedItem.className='cmdAutoCompleteSelected';
        
    }
};


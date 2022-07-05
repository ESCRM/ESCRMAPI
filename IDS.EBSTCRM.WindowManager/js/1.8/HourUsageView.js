function HourUsageView(OwnerDocument, OwnerContainer, projectAndActivitiesContainer)
{
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    
    this.IsDirty = false;
    this.OnDirtyChanged = null;
    this.IsReadOnly = false;
    
    this.Rows = new Array();
    this.DeletedRows = new Array();
    
    this.SelectedRow = null;
    
    this.Listview = new IDC_Listview(OwnerDocument, OwnerContainer, 100,100);
    this.Listview.Sortable = false;
    this.Listview.ResizeableColumnHeaders = false;
    this.Listview.EnableColumnFilters = false;
    
    this.Listview.OnSelect = function(node) { node.HourRow.HourUsageView.SelectedRow = node.HourRow; };
    
    this.FocusedObject = null;
    
    this.Listview.AddColumnHeader('Aktivitet',250);
    this.Listview.AddColumnHeader('Man',80,'', 'numeric');
    this.Listview.AddColumnHeader('Tir',80,'', 'numeric');
    this.Listview.AddColumnHeader('Ons',80,'', 'numeric');
    this.Listview.AddColumnHeader('Tor',80,'', 'numeric');
    this.Listview.AddColumnHeader('Fre',80,'', 'numeric');
    this.Listview.AddColumnHeader('Lør',80,'', 'numeric');
    this.Listview.AddColumnHeader('Søn',80,'', 'numeric');
    this.Listview.AddColumnHeader('Total',75,'right', 'numeric');
    
    this.Listview.ChangeView(0);
    
    var values = new Array();
    values.push('');
    
    for(var i=0;i<8;i++)
    {
        values.push('0,00');
    }   
    
    this.SumLine = new IDC_ListviewItem(this.Document, this.Container, values);
    this.SumLine.Icon16 = 'images/spacer.gif';   
    this.Listview.AddItemObject(this.SumLine);
    this.SumLine.Control.onmouseover=null;
    this.SumLine.Control.onmouseout=null;
    this.SumLine.Control.onclick=null;
    this.SumLine.Control.IsSumLine = true;
    for(var i=0;i<this.SumLine.Control.childNodes.length;i++)
    {
        this.SumLine.Control.childNodes[i].style.fontWeight='bold';
        this.SumLine.Control.childNodes[i].style.backgroundColor='white';
        this.SumLine.Control.childNodes[i].style.color='black';
        this.SumLine.Control.childNodes[i].style.textAlign='right';
        this.SumLine.Control.childNodes[i].style.borderBottom='double 3px #d5d5d5';
        this.SumLine.Control.childNodes[i].style.borderTop='solid 1px #d5d5d5';
        this.SumLine.Control.childNodes[i].style.fontSize='14px';
        
        this.SumLine.Control.childNodes[i].FixedValue = 0;
    }
    
    this.ProjectSelector = new HourUsageAvailableProjects(this, OwnerDocument, projectAndActivitiesContainer);
    
    this.ProjectCommentControl = this.Document.createElement('DIV');
    this.ProjectCommentControl.style.marginLeft=320 - (document.all ? 4 : 0);
    
    this.ProjectCommentControl.Memo = this.Document.createElement('TEXTAREA');
    this.ProjectCommentControl.Memo.style.width=240;
    this.ProjectCommentControl.Memo.style.height=64;
    this.ProjectCommentControl.Memo.style.border='solid 1px #d5d5d5';
    
    this.ProjectCommentControl.appendChild(this.ProjectCommentControl.Memo);
    this.ProjectCommentControl.style.display='none';
    this.Document.body.appendChild(this.ProjectCommentControl);
    
    this.ProjectCommentControl.Memo.onchange = function() { this.DayControl.Row.HourUsageView.IsDirty=true; if(this.DayControl.Row.HourUsageView.OnDirtyChanged) { this.DayControl.Row.HourUsageView.OnDirtyChanged(true); }; if(this.DayControl) this.DayControl.Memo = this.value || ''; };
    this.ProjectCommentControl.Memo.onfocus = function(forced)
    {
        if(!document.all) return;
        if(forced && !document.all) this.blur(true);
        
        if(this.DayControl)
        {
            if(!this.DayControl.Row.HourUsageView.IsReadOnly)
            {
                this.DayControl.Row.HourUsageView.FocusedObject = this;
                if(document.all) this.DayControl.Row.ListviewItem.Select(); 
                this.DayControl.className='listviewEditDTItemEditing'; 
                this.DayControl.parentNode.className='listviewDTItemEdit'; 
            }
        }   
    }
    this.ProjectCommentControl.Memo.onblur = function(forced)
    {
            
        if(this.DayControl)
        {
            this.DayControl.Row.HourUsageView.FocusedObject = null;
            this.DayControl.className='listviewEditDTItem'; 
            this.DayControl.parentNode.className='listviewDTItem'; 
            //find next control
            var n = this.DayControl.parentNode.nextSibling;
            
            if(n)
            {
                if(n.firstChild)
                    n = n.firstChild;
                
                if(n.tagName!='INPUT')
                    n = null;
            }
            
            
            if(n)
            {
                n.focus();
            }
            else
            {
                n = this.DayControl.parentNode.parentNode.nextSibling;
                
                if(n)
                {
                    if(n.firstChild.tagName == 'TEXTAREA') 
                        n = n.nextSibling;
                    
                }
                
                if(n.IsSumLine) n = null;
                
                if(!n && this.DayControl.Row.HourUsageView.Rows.length>0)
                {
                    this.DayControl.Row.HourUsageView.Rows[0].ListviewItem.Control.firstChild.firstChild.focus();
                }
                
            }
            
            if(!document.all)
                this.Control.style.display='none';
        }
    }
};

HourUsageView.prototype.SetReadOnly = function(value)
{
    this.IsReadOnly = value;
    this.ProjectCommentControl.Memo.readOnly = value;
    for(var i=0;i<this.Rows.length;i++)
    {
        for(var ii=0;ii<this.Rows[i].EditObjects.length;ii++)
        {
            this.Rows[i].EditObjects[ii].readOnly = value;
        }
    }
    
};

HourUsageView.prototype.SetDayHeader = function(day, header)
{
    this.Listview.ColumnHeaders[day].Control.innerHTML = header;
};

HourUsageView.prototype.Clear = function()
{
    this.ProjectCommentControl.style.display='none';
    this.ProjectSelector.Hide();
    
    for(var i=this.Rows.length-1;i>=0;i--)
    {
        this.Rows[i].ListviewItem.Remove();
        this.Rows.splice(i,1);
    }
    
    this.DeletedRows = new Array();
    this.IsDirty = false;
    if(this.OnDirtyChanged) { this.OnDirtyChanged(false); }
    this.SumAll();
};

HourUsageView.prototype.GetExistingRow = function(serialno, caseno, dayOfWeek)
{
    
    for(var i=0;i<this.Rows.length;i++)
    {
        var r = this.Rows[i];
        if(r.EditObjects.length>0)
        {
            if(r.EditObjects[0].SerialNo == serialno && r.EditObjects[0].CaseNo == caseno)
            {
                if(!r.EditObjects[dayOfWeek].pId)
                {
                    return r;
                }
            }
        }
    }
    
    return null;
};

function HourUsageAvailableProjects(listview, OwnerDocument, projectAndActivitiesContainer)
{
    this.Listview = listview;
    this.Projects = new Array();
    
    this.Document = OwnerDocument;
    
    this.Control = this.Document.createElement('DIV');
    this.Control.ContainerControl = this.Document.createElement('DIV');
    
    this.Control.style.position='absolute';
    this.Control.style.border='solid 1px #d5d5d5';
    this.Control.style.backgroundColor='white';
    this.Control.style.padding='0px';
    this.Control.style.overflowY='auto';
    this.Control.style.overflowX='hidden';
    
    this.Document.body.appendChild(this.Control);
    this.Control.appendChild(this.Control.ContainerControl);
    this.Control.ContainerControl.style.padding='2px';
    
    this.Control.style.left=0;
    this.Control.style.top=0;
    this.Control.style.display='none';
    this.SelectedSender = null;
    this.SelectedHoverProject = null;
    
    while(projectAndActivitiesContainer.firstChild)
    {
        var o = projectAndActivitiesContainer.removeChild(projectAndActivitiesContainer.firstChild);
        this.Projects.push(new HourUsageAvailableProject(OwnerDocument, o, this));
        this.Control.ContainerControl.appendChild(this.Projects[this.Projects.length-1].Control);
    }
    
    return this;
};

HourUsageAvailableProjects.prototype.Show = function(sender)
{
    if(this.Listview)
        if(this.Listview.IsReadOnly) return;
    
    this.SelectedSender = sender;
    this.SelectedHoverProject = null;
    var opos = core.DOM.GetObjectPosition(sender);
    this.Control.style.display='';
    this.Control.style.width=350;
    this.Control.style.left = opos[0];
    this.Control.style.top = opos[1] + sender.offsetHeight + 2;
        
    this.Filter();
    
    var h = this.Control.ContainerControl.offsetHeight + 2;
    this.Control.style.height = h > 250 ? 250 : h;
};


HourUsageAvailableProjects.prototype.Filter = function()
{
    if(this.Listview)
        if(this.Listview.IsReadOnly) return;
    
    var txt = (this.SelectedSender ? this.SelectedSender.value : '') || '';
    txt = txt.toUpperCase().replace(/\s\s/g,' ');
    var words = txt.split(' ');
   
    var visibleCount = 0;
    for(var i=0;i<this.Projects.length;i++)
    {
        if(txt.length>0 && words.length>0)
        {
            if(this.ValueInArray(words, unescape(this.Projects[i].ProjectName.toUpperCase())) ||
                this.ValueInArray(words, this.Projects[i].SerialNo.toUpperCase()) ||
                this.ValueInArray(words, this.Projects[i].CaseNo.toUpperCase()))
            {
                this.Projects[i].Control.style.display='';
                visibleCount++;
            }
            else
            {
                this.Projects[i].Control.style.display='none';
            }
        }
        else
        {
            this.Projects[i].Control.style.display='';
            visibleCount++;
        }
        
    }

};

HourUsageAvailableProjects.prototype.ValueInArray = function(words, val)
{
    var allWords = true;
    var html = '';
    for(var i=0;i<words.length;i++)
    {
        
        if(words[i] != ' ' && words[i]!=null && words[i].length>0)
        {
            var index = val.toUpperCase().indexOf(words[i]);

            if(index>-1)
            {
                html += words[i] + '=' + index + ' , ';
            }
            else
            {
                allWords = false;
                break;
            }
        }
    }
    return allWords;
};

HourUsageAvailableProjects.prototype.Hide = function()
{
    this.Control.style.display='none';
    if(this.SelectedHoverProject) this.SelectedHoverProject.Control.className='listviewDTLine';
    this.SelectedHoverProject = null;
};

function HourUsageAvailableProject(OwnerDocument, sender, projects)
{
    this.Document = OwnerDocument;
    this.Projects = projects;
    this.ProjectName = document.all ? sender.innerText : sender.textContent;
    this.SerialNo = sender.getAttribute('pserial');
    this.CaseNo = sender.getAttribute('cserial');
    this.Pid = sender.getAttribute('pid');
    this.Cid = sender.getAttribute('cid');
    
    this.Control = this.Document.createElement('DIV');
    this.Control.Project = this;
    this.Control.className='listviewDTLine';
    this.Control.style.fontFamily='arial';
    this.Control.style.fontSize='12px';
    this.Control.style.height=22;
    this.Control.style.lineHeight='22px';
    this.Control.onmouseover = function() { this.Project.SelectedHoverProject = this.Project; this.className='listviewDTLine_h'; };
    this.Control.onmouseout = function() { this.SelectedHoverProject = null; this.className='listviewDTLine'; };
    this.Control.onmousedown = function() { 
                                            if(this.Project.SelectedHoverProject) this.Project.SelectedHoverProject.Control.className='listviewDTLine';
                                            this.Project.Projects.SelectedSender.value = this.Project.ProjectName; 
                                            this.Project.Projects.SelectedSender.SerialNo = this.Project.SerialNo; 
                                            this.Project.Projects.SelectedSender.CaseNo = this.Project.CaseNo; 
                                            this.Project.Projects.SelectedSender.ProjectType = this.Project; 
                                            
                                            if(this.Project.Projects.SelectedSender.onchange)
                                                this.Project.Projects.SelectedSender.onchange(); 
                                            this.Project.Projects.SelectedSender.focus(); 
                                            
                                            if(!document.all)
                                                this.Project.Projects.Hide();
                                            
                                            };
    
    this.Control.innerHTML = '<span style="font-size:9px;color:gray;">' + this.SerialNo + '-' + this.CaseNo + '</span> ' + this.ProjectName;
};

HourUsageView.prototype.SetSize = function(w,h)
{
    this.Listview.SetSize(w,h);
};

HourUsageView.prototype.AddRow = function(project)
{
    var r = new HourUsageRow(this.Document, this.Container, this, project); 
    this.Rows.push(r);
    
    this.SumLine.Control.parentNode.removeChild(this.SumLine.Control);
    
    this.Listview.Container.appendChild(this.SumLine.Control);
    
    return r;
};

function HourUsageDayValue(document, control)
{
    this.TimeSpent = 0;
    this.ContactId  = null;
    this.Date = null;
    this.ProjectId = 0;
    this.ActivityId = 0;
    this.ProjectName = 'pj';
    this.ActivityName = 'ac';
    this.EditObject = null;
    this.Document = document;
    this.Control = control;
    
};
function HourUsageRow(ownerDocument, ownerContainer, hourUsageView, project)
{
    this.Document = ownerDocument;
    this.Container = ownerContainer;
    this.HourUsageView = hourUsageView;
    
    this.Sum = 0;
    
    this.Project = null;
    this.Activity = null;
    this.Date = null;
    
    var values = new Array();
    values.push(project || '');
    
    for(var i=0;i<8;i++)
    {
        values.push('0,00');
    }   
    
    this.ListviewItem = new IDC_ListviewItem(this.Document, this.Container, values);
    this.ListviewItem.Icon16 = 'images/itemNumber.png';
    
    this.ListviewItem.HourRow = this;
    
    this.HourUsageView.Listview.AddItemObject(this.ListviewItem);

    /*this.ListviewItem.Control.onmouseover=null;
    this.ListviewItem.Control.onmouseout=null;
    this.ListviewItem.Control.onclick=null;*/
    
    
    this.EditObjects = new Array();    
    
    for(var i=0;i<this.ListviewItem.Control.childNodes.length-1;i++)
    {
        alert(this.ListviewItem.Control.outerHTML);
        var c = this.ListviewItem.Control.childNodes[0].childNodes[0].childNodes[i].childNodes[0];
        c.Row = this;
        var eo = this.Document.createElement('INPUT');
        this.EditObjects.push(eo);
        c.EditObject = eo;
        c.EditObject.Row = this;
        c.EditObject.className='listviewEditDTItem';
        
        if(i>0)
        {
            c.EditObject.style.textAlign='right';
            c.EditObject.onfocus = function() { 
   
                                                this.Row.HourUsageView.FocusedObject = this;
                                                this.Row.ListviewItem.Select(); 
                                                
                                                if(!this.Row.HourUsageView.IsReadOnly) 
                                                {
                                                    this.className='listviewEditDTItemEditing'; 
                                                    this.parentNode.className='listviewDTItemEdit'; 
                                                }
                                                
                                                var hv = this.Row.HourUsageView;
                                                var obj = hv.ProjectCommentControl.parentNode.removeChild(hv.ProjectCommentControl);
                                                
                                                if(this.Row.ListviewItem.Control.nextSibling)
                                                {
                                                    this.Row.ListviewItem.Control.parentNode.insertBefore(obj, this.Row.ListviewItem.Control.nextSibling);
                                                }
                                                else
                                                {
                                                    this.Row.ListviewItem.Control.parentNode.appendChild(obj);
                                                }
                                                
                                                obj.style.display='';
                                                obj.style.marginLeft=core.DOM.GetObjectPosition(this)[0] - 4;
                                                obj.Memo.DayControl = this;
                                                obj.Memo.value = this.Memo || '';
                                                
                                               };
            c.EditObject.onblur = function() {  
                                                if(!this.Row.HourUsageView.IsReadOnly)
                                                {
                                                    if(document.all)
                                                    {
                                                    var me = this;
                                                    setTimeout(function() {
                                                                            if(!me.Row.HourUsageView.FocusedObject)
                                                                            {
                                                                                me.Row.HourUsageView.ProjectCommentControl.style.display='none';
                                                                            }
                                                    
                                                                            },10);
                                                    }
                                                }
                                                this.Row.HourUsageView.FocusedObject = null;
                                                this.className='listviewEditDTItem'; 
                                                hourUsageFormatNumber(this); 
                                                this.parentNode.className='listviewDTItem';  
                                             };
            
            if(i==7)
            {
                c.EditObject.onkeydown = function(e) {                                                  
                                                        if(!e) e=event;
                                                        
                                                        var keynum=null;
                                                        if(document.all) // IE
                                                            keynum = e.keyCode;
                                                        else if(e.which) // Netscape/Firefox/Opera
                                                            keynum = e.which;
                                                            
                                                        if(keynum == 13 || keynum == 9)
                                                        {
                                                                this.Row.HourUsageView.ProjectCommentControl.Memo.focus(true);
                                                        }
                                                        else if(keynum == 40)
                                                            this.Row.HourUsageView.ProjectCommentControl.Memo.focus();
                                                        
                                                        };
            }
            else
            {
                c.EditObject.onkeydown = function(e) {  
                                                        if(this.Row.HourUsageView.IsReadOnly) return;
                                                
                                                        if(!e) e=event;
                                                        
                                                        var keynum=null;
                                                        if(document.all) // IE
                                                            keynum = e.keyCode;
                                                        else if(e.which) // Netscape/Firefox/Opera
                                                            keynum = e.which;
                                                            
                                                        if(keynum == 40)
                                                            this.Row.HourUsageView.ProjectCommentControl.Memo.focus();                                         
                                                        };
            }
            
            c.EditObject.onchange = function() { this.Row.HourUsageView.IsDirty=true; if(this.Row.HourUsageView.OnDirtyChanged) { this.Row.HourUsageView.OnDirtyChanged(true); }; hourUsageFormatNumber(this); this.Row.HourUsageView.SumAll(); };
            c.EditObject.onclick = function() { 
                                                this.select(); };
                                                
            hourUsageFormatNumber(c.EditObject);
        }
        else
        {
            c.EditObject.onfocus = function() { 
                                                this.Row.HourUsageView.ProjectCommentControl.style.display='none'; 
                                                                                                
                                                
                                                this.Row.HourUsageView.ProjectSelector.Show(this); 
                                                this.Row.ListviewItem.Select(); 
                                                
                                                if(this.Row.HourUsageView.IsReadOnly) return;
                                                
                                                this.className='listviewEditDTItemEditing'; 
                                                this.parentNode.style.backgroundColor='white'; 
                                                };
            c.EditObject.onblur = function() { 
                                                this.Row.HourUsageView.ProjectSelector.Hide(); this.className='listviewEditDTItem'; this.parentNode.style.backgroundColor=''; 
                                                };
                                                
            c.EditObject.onchange = function() { 
                                                
                                                    this.Row.HourUsageView.IsDirty=true; if(this.Row.HourUsageView.OnDirtyChanged) { this.Row.HourUsageView.OnDirtyChanged(true); }; if(this.Row.HourUsageView.Rows[this.Row.HourUsageView.Rows.length-1].ListviewItem.Control.childNodes[0].EditObject.value != '') this.Row.HourUsageView.AddRow(); 
                                               };
                                               
            c.EditObject.onkeydown = function(e) {                                                 
                                                
                                                    if(!e) e=event;
                                                    
                                                    var keynum=null;
                                                    if(document.all) // IE
                                                        keynum = e.keyCode;
                                                    else if(e.which) // Netscape/Firefox/Opera
                                                        keynum = e.which;
                                                                                                        
                                                    if(keynum == 13 || keynum == 9)
                                                    {
                                                        if(this.Row.HourUsageView.ProjectSelector.SelectedHoverProject)
                                                        {
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.onmousedown();
                                                            this.Row.HourUsageView.ProjectSelector.Hide();
                                                        }
                                                    }
                                                    
                                                    if(keynum == 38)
                                                    {
                                                        if(!this.Row.HourUsageView.ProjectSelector.SelectedHoverProject)
                                                        {
                                                            var o=this.Row.HourUsageView.ProjectSelector.Projects[this.Row.HourUsageView.ProjectSelector.Projects.length-1].Control;
                                                            var isVisible = o!= null ? o.style.display!='none' : false;
                                                            while(o && isVisible == false)
                                                            {
                                                                o = o.previousSibling;
                                                                if(o)
                                                                    isVisible = o.style.display!='none';
                                                            }
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = o.Project;
                                                        }
                                                        else
                                                        {
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.onmouseout();
                                                            var o=this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.previousSibling;
                                                            var isVisible = o!= null ? o.style.display!='none' : false;
                                                            while(o && isVisible == false)
                                                            {
                                                                o = o.previousSibling;
                                                                if(o)
                                                                    isVisible = o.style.display!='none';
                                                            }
                                                            
                                                            if(!o)
                                                            {
                                                                this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = this.Row.HourUsageView.ProjectSelector.Projects[this.Row.HourUsageView.ProjectSelector.Projects.length-1];
                                                            }
                                                            else
                                                                this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = o.Project;
                                                            
                                                        }
                                                        if(this.Row.HourUsageView.ProjectSelector.SelectedHoverProject)
                                                        {
                                                            this.Row.HourUsageView.ProjectSelector.Control.scrollTop = this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.offsetTop;
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.onmouseover();
                                                            
                                                        }
                                                    }
                                                    
                                                    if(keynum == 40)
                                                    {
                                                        if(!this.Row.HourUsageView.ProjectSelector.SelectedHoverProject)
                                                        {
                                                            var o = this.Row.HourUsageView.ProjectSelector.Projects[0].Control;
                                                            var isVisible = o!= null ? o.style.display!='none' : false;
                                                            while(o && isVisible == false)
                                                            {
                                                                o = o.nextSibling;
                                                                if(o)
                                                                    isVisible = o.style.display!='none';
                                                            }
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = o.Project;
                                                        }
                                                        else
                                                        {
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.onmouseout();
                                                            var o=this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.nextSibling;
                                                            var isVisible = o!= null ? o.style.display!='none' : false;
                                                            while(o && isVisible == false)
                                                            {
                                                                o = o.nextSibling;
                                                                if(o)
                                                                    isVisible = o.style.display!='none';
                                                            }
                                                            
                                                            if(!o)
                                                            {
                                                                isVisible = false;
                                                                o = this.Row.HourUsageView.ProjectSelector.Projects[0];
                                                                while(o && isVisible == false)
                                                                {
                                                                    o = o.nextSibling;
                                                                    if(o)
                                                                        isVisible = o.style.display!='none';
                                                                }
                                                                this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = o;
                                                            }
                                                            else
                                                                this.Row.HourUsageView.ProjectSelector.SelectedHoverProject = o.Project;
                                                            
                                                        }
                                                        if(this.Row.HourUsageView.ProjectSelector.SelectedHoverProject)
                                                        {
                                                            this.Row.HourUsageView.ProjectSelector.Control.scrollTop = this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.offsetTop - 224;
                                                            this.Row.HourUsageView.ProjectSelector.SelectedHoverProject.Control.onmouseover();
                                                        }
                                                    }
                                                    
                                                    this.Row.HourUsageView.ProjectSelector.Filter(); 
                                                    };
        
            c.EditObject.onkeyup = function(e)
                                                {
                                                    if(this.Row.HourUsageView.IsReadOnly) return;
                                                
                                                    if(!e) e=event;
                                                    
                                                    var keynum=null;
                                                    if(document.all) // IE
                                                        keynum = e.keyCode;
                                                    else if(e.which) // Netscape/Firefox/Opera
                                                        keynum = e.which;
                                                        

                                                                                                        
                                                    this.Row.HourUsageView.ProjectSelector.Filter();
                                                };
                                                
                                                
        }
        
        c.innerHTML = '';
        c.appendChild(c.EditObject);
        c.appendChild(this.Document.createElement('br'));
        
    }
    
    hourUsageSumRowHorizontal(this);

    return this;
};


HourUsageRow.prototype.SetProjectType = function(serialno, caseno, projectName, caseName)
{
    if(this.EditObjects.length==0) return;
    var hr = this.HourUsageView;
    for(var i=0;i<hr.ProjectSelector.Projects.length;i++)
    {
        var p = hr.ProjectSelector.Projects[i];
        if(p.SerialNo == serialno && p.CaseNo == caseno)
        {
            this.EditObjects[0].value = p.ProjectName; 
            this.EditObjects[0].SerialNo = p.SerialNo; 
            this.EditObjects[0].CaseNo = p.CaseNo; 
            this.EditObjects[0].ProjectType = p; 
            return;
        }
    }
    this.EditObjects[0].value = projectName + ' - ' + caseName; 
    this.EditObjects[0].SerialNo = serialno; 
    this.EditObjects[0].CaseNo = caseno; 
};



HourUsageRow.prototype.SetWeekDayData = function(pid, dayOfWeek, timespent, memo)
{
    if(this.EditObjects.length==0) return;
    this.EditObjects[dayOfWeek].value = timespent;
    this.EditObjects[dayOfWeek].DayValue = parseFloat(timespent.replace(/,/g,'.')).toFixed(2);
    this.EditObjects[dayOfWeek].pId = pid;
    this.EditObjects[dayOfWeek].Memo = memo;
};

HourUsageRow.prototype.SumUp = function()
{
    var val =0;
    for(var i=1;i<this.EditObjects.length;i++)
    {
        var v = parseFloat(this.EditObjects[i].value.replace(/,/g,'.'));
        if(!v) v = 0;
        
        this.EditObjects[i].FixedValue = v;
        this.HourUsageView.SumLine.Control.childNodes[i].FixedValue +=v;
        val += v;
    }
    
    this.Sum = val;
    this.HourUsageView.SumLine.Control.childNodes[0].childNodes[0].childNodes[8].FixedValue += val;
    this.ListviewItem.Control.childNodes[0].childNodes[0].childNodes[8].childNodes[0].innerHTML = '<b>' + hourUsageFormatNumberVal(val) + '</b>';
};

HourUsageView.prototype.SumAll = function()
{
    for(var i = 1;i<this.SumLine.Control.childNodes.length; i++)
    {
        this.SumLine.Control.childNodes[0].childNodes[0].childNodes[i].childNodes[0].FixedValue = 0;
    }
    
    for(var i = 0;i<this.Rows.length;i++)
    {
        this.Rows[i].SumUp();
    }
    
    for(var i = 1;i<this.SumLine.Control.childNodes.length; i++)
    {
        this.SumLine.Control.childNodes[0].childNodes[0].childNodes[i].title = hourUsageFormatNumberVal(this.SumLine.Control.childNodes[i].FixedValue);
        this.SumLine.Control.childNodes[0].childNodes[0].childNodes[i].childNodes[0].innerHTML = hourUsageFormatNumberVal(this.SumLine.Control.childNodes[i].FixedValue);
    }
};

HourUsageRow.prototype.Delete  = function()
{
    var Items = new Array();
    for(var i=0;i<this.EditObjects.length;i++)
    {
        if(this.EditObjects[i].pId)
            Items.push(this.EditObjects[i].pId);
    }
    this.HourUsageView.DeletedRows.push(Items);
    
    this.ListviewItem.Remove();
    
    for(var i = this.HourUsageView.Rows.length-1;i>=0;i--)
    {
        if(this.HourUsageView.Rows[i] == this)
        {
            this.HourUsageView.Rows.splice(i,1);
            break;
        }
    }
    this.HourUsageView.IsDirty = true;
    this.HourUsageView.SumAll();
    if(this.HourUsageView.OnDirtyChanged) { this.HourUsageView.OnDirtyChanged(true); };
};

function hourUsageSumRowHorizontal(row)
{
    var val =0;
    for(var i=1;i<row.EditObjects.length;i++)
    {
        var v = parseFloat(row.EditObjects[i].value.replace(/,/g,'.'));
        if(!v) v = 0;
        
        row.EditObjects[i].FixedValue = v;
        
        val += v;
    }
    
    row.Sum = val;
    row.ListviewItem.Control.childNodes[0].childNodes[0].childNodes[8].childNodes[0].title = hourUsageFormatNumberVal(val);
    row.ListviewItem.Control.childNodes[0].childNodes[0].childNodes[8].childNodes[0].innerHTML = '<b>' + hourUsageFormatNumberVal(val) + '</b>';
};


function hourUsageFormatNumber(sender)
{
    var tmp = sender.value.replace(/,/g,'.');
    var val = parseFloat(tmp);
    if(!val) val = 0;
    sender.DayValue = val.toFixed(2);
    sender.value = val.toFixed(2).toString().replace(/\./g,',');
};

function hourUsageFormatNumberVal(value)
{
    var val = parseFloat(value);
    if(!val) val = 0;
    return val.toFixed(2).toString().replace(/\./g,',');
};


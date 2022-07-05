var CalendarView = function (OwnerDocument, OwnerContainer, MonthPickerContainer, MultiUsersContainer, appointmentsURL)
{
    this.Document = OwnerDocument;
    this.Container = OwnerContainer;
    this.MonthContainer = MonthPickerContainer;
    this.UsersContainer = MultiUsersContainer;
    
    this.OnCalendarLoad = null;
    this.OnCalendarFailure = null;
    
    if(this.UsersContainer)
        this.UsersContainer.Control=null;
    
    this.AppointmentsURL = appointmentsURL;
    
    this.MonthNames = new Array();
    this.MonthNames.push('January');
    this.MonthNames.push('February');
    this.MonthNames.push('March');
    this.MonthNames.push('April');
    this.MonthNames.push('May');
    this.MonthNames.push('June');
    this.MonthNames.push('July');
    this.MonthNames.push('August');
    this.MonthNames.push('September');
    this.MonthNames.push('October');
    this.MonthNames.push('November');
    this.MonthNames.push('December');
    
    this.SelectedDate = new Date();
    
    this.CurrentYear = this.SelectedDate.getFullYear();
    this.CurrentMonth = this.SelectedDate.getMonth();
    this.CurrentDay = this.SelectedDate.getDate();
    
    var tbl = this.Document.createElement('TABLE');
    var tbody = this.Document.createElement('TBODY');
    this.CalendarControls = this.Document.createElement('TR');
    
    
    this.LoadingControl = this.Document.createElement('DIV');
    this.LoadingControl.style.position='absolute';
    this.LoadingControl.style.width='100%';
    this.LoadingControl.style.height=24*66;
    this.LoadingControl.style.left=0;
    this.LoadingControl.style.top=0;
    this.LoadingControl.innerHTML='&nbsp;';
    this.LoadingControl.style.zIndex=250000;
    this.LoadingControl.style.backgroundImage='url(images/WindowBusy.gif)';
    this.LoadingControl.style.backgroundRepeat='no-repeat';
    this.LoadingControl.style.backgroundPosition='2px right';
    this.LoadingControl.style.marginRight='6px';
    this.LoadingControl.style.marginTop='6px';
    this.LoadingControl.style.display='none';
    this.Document.body.appendChild(this.LoadingControl);
    
    this.SelectedAppointment = null;
    
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    tbl.style.width='100%';
    tbl.style.height=10;
    
    this.CalendarViews = new Array();
    this.DaysInMonth = new Array();
    this.CurrentMonthYear = null;
    
    this.Container.appendChild(tbl);
    tbl.appendChild(tbody);
    tbody.appendChild(this.CalendarControls);
    
    this.AjaxObject = null;
    this.AjaxObjectMonthly = null;
    
    this.__BuildHours();
    this.__BuildDatePicker();
    
    this.SetCurrentDate(this.CurrentDay, this.CurrentMonth, this.CurrentYear, true);
    
    this.OnAppointmentDoubleClick=null;
    
    this.OnSelectAppointment=null;
    
};

CalendarView.prototype.Refresh = function()
{
    this.SetCurrentDate(this.CurrentDay, this.CurrentMonth, this.CurrentYear, true);
};

CalendarView.prototype.IsSelectedDateMatching = function(date)
{
    if(!date) return false;
    var dt = this.LeadingZeros(this.SelectedDate.getDate(),2) + '-' + 
             this.LeadingZeros(this.SelectedDate.getMonth()+1,2) + '-' + 
             this.SelectedDate.getFullYear();
     
    return date.indexOf(dt)>-1;
    
};

CalendarView.prototype.LeadingZeros = function LeadingZeros(val, size)
{
    var v = val.toString();
    while(v.length<size)
        v = '0' + v;
        
    return v;
};

CalendarView.prototype.GetSelectedCalendarView = function()
{
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        if(this.CalendarViews[i].MeetingPointer.style.visibility!='hidden')
            return this.CalendarViews[i];
    }
    
    if(this.CalendarViews.length>0)
        return this.CalendarViews[0];
    else 
        return null;
};

CalendarView.prototype.ScrollHour = function(hour)
{
    this.Container.scrollTop=hour*46;
};

CalendarView.prototype.SetCurrentDateFromDT = function(date, forcedLoadOfMontlyDates)
{
    if(!date) return;
    this.SetCurrentDate(date.getDate(), date.getMonth(), date.getFullYear(), forcedLoadOfMontlyDates);
};
CalendarView.prototype.SetCurrentDate = function(day, month, year, forcedLoadOfMontlyDates)
{   
    var oldDate = this.SelectedDate.getFullYear() + '-' + this.SelectedDate.getMonth();
    
    this.SelectedDate.setFullYear(year);
    this.SelectedDate.setMonth(month);
    this.SelectedDate.setDate(day);
        
    var dt = new Date();
    dt.setFullYear(year,month, day);
    
    dt = dt.dateAdd('d',-dt.getDate());
    
    while(dt.getDay()!=1)
    {
        dt = dt.dateAdd('d',-1);
    }
    
   
    this.CurrentMonthYear.innerHTML = this.MonthNames[month] + ' ' + year;
    
    for(var i=0;i<this.DaysInMonth.length;i++)
    {
        var tdDay = this.DaysInMonth[i];
        tdDay.innerHTML=dt.getDate();
        
        tdDay.Date = new Date();
        tdDay.Date.setFullYear(dt.getFullYear());
        tdDay.Date.setMonth(dt.getMonth());
        tdDay.Date.setDate(dt.getDate());
        
        tdDay.className =  dt.getMonth() == this.SelectedDate.getMonth() && dt.getDate() == this.SelectedDate.getDate() ? 'calendarDaySelected' : this.SelectedDate.getMonth() == dt.getMonth() ? 'calendarDay' : 'calendarDayOutOfMonth';
        
        dt = dt.dateAdd('d',1);
    }
    
    //Load appointments from new date
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        this.CalendarViews[i].Clear();
        this.CalendarViews[i].MeetingPointer.style.visibility='hidden';
    }
    this.FixHeaders();
    
    if(this.AppointmentsURL)
    {
        var calendars = '';
        for(var i=0;i<this.CalendarViews.length;i++)
        {
            calendars += (calendars == '' ? '' : ',') + this.CalendarViews[i].Uniqueidentifier;
        }
        if(calendars!='')
            this.__LoadAppointmentsForView(calendars);
        
        if(forcedLoadOfMontlyDates || oldDate != this.SelectedDate.getFullYear() + '-' + this.SelectedDate.getMonth())
        {
            for(var i=0;i<this.DaysInMonth.length;i++)
            {
                this.DaysInMonth[i].style.fontWeight='';
            }
            
            if(this.AjaxObjectMonthly)
                this.AjaxObjectMonthly.abort();
                
            this.AjaxObjectMonthly = new core.Ajax();
            this.AjaxObjectMonthly.requestFile = this.AppointmentsURL;

            this.AjaxObjectMonthly.encVar('monthlyview', 'true');
            this.AjaxObjectMonthly.encVar('date_month', this.SelectedDate.getMonth()+1);
            this.AjaxObjectMonthly.encVar('date_year', this.SelectedDate.getFullYear());
            
            this.LoadingControl.style.display='';
            var cal = this;
            
            this.AjaxObjectMonthly.OnCompletion = function(){ cal.__LoadOfMontlyDatesFromAjax(); }; 
            this.AjaxObjectMonthly.OnError = function(){ cal.__LoadOfMontlyDatesFromAjax();};
            this.AjaxObjectMonthly.OnFail = function(){  cal.__LoadOfMontlyDatesFromAjax();};
            this.AjaxObjectMonthly.RunAJAX();
        }
    }
};

CalendarView.prototype.__LoadOfMontlyDatesFromAjax = function()
{
    var result=this.AjaxObjectMonthly.Response;
    this.AjaxObjectMonthly.Reset();
    this.AjaxObjectMonthly.Dispose();
    this.AjaxObjectMonthly=null;  

    this.LoadingControl.style.display='none';
    
    if(result == 'failed')
    {       
        //if(this.OnCalendarFailure) this.OnCalendarFailure();
    }
    else
    {
        if(this.OnCalendarLoad) this.OnCalendarLoad();
        
        var div = this.Document.createElement('DIV');
        div.innerHTML=result;
            
        while(div.hasChildNodes())
        {
            var src = div.firstChild;
            src = div.removeChild(src);
            
            for(var i=0;i<this.DaysInMonth.length;i++)
            {
                if(parseInt(this.DaysInMonth[i].innerHTML) == parseInt(src.innerHTML,10) && this.DaysInMonth[i].className != 'calendarDayOutOfMonth')
                {
                    this.DaysInMonth[i].style.fontWeight='bold';
                }
            }
            
        }
    }
};

CalendarView.prototype.__LoadAppointmentsForView = function(views)
{
    if(this.AppointmentsURL)
    {
        if(views=='') return;
        
        if(this.AjaxObject)
        {
            this.AjaxObject.Abort();
        }

        this.AjaxObject = new core.Ajax();
        this.AjaxObject.requestFile = this.AppointmentsURL;

        this.AjaxObject.encVar('date_day', this.SelectedDate.getDate());
        this.AjaxObject.encVar('date_month', this.SelectedDate.getMonth()+1);
        this.AjaxObject.encVar('date_year', this.SelectedDate.getFullYear());
        this.AjaxObject.encVar('calendars', views);
        
        var cal = this;
        
        this.LoadingControl.style.display='';
        
        this.AjaxObject.OnCompletion = function(){ cal.__CalendarsLoadedFromAjax(views); }; 
        this.AjaxObject.OnError = function(){ cal.__CalendarsLoadedFromAjax(views);};
        this.AjaxObject.OnFail = function(){  cal.__CalendarsLoadedFromAjax(views);};
        this.AjaxObject.RunAJAX();
    }
};

CalendarView.prototype.__CalendarsLoadedFromAjax = function (views) {
    var result = this.AjaxObject.Response;
    this.AjaxObject.Reset();
    this.AjaxObject.Dispose();
    this.AjaxObject = null;

    this.LoadingControl.style.display = 'none';

    if (result == 'failed') {
        if (this.OnCalendarFailure) this.OnCalendarFailure(views);
    }
    else {
        if (this.OnCalendarLoad) this.OnCalendarLoad(views);

        var div = this.Document.createElement('DIV');
        div.innerHTML = result;

        while (div.hasChildNodes()) {
            var src = div.firstChild;
            src = div.removeChild(src);
            if (src.tagName) {
                var view = this.GetViewFromId(src.getAttribute('uid'));

                if (view) {
                    //add appointments
                    while (src.hasChildNodes()) {
                        var app = src.firstChild;
                        app = src.removeChild(app);

                        if (app.tagName) {
                            view.AddAppointment(app.getAttribute('dtstart'), app.getAttribute('dtend'), app.innerHTML, app.getAttribute('location'), app.getAttribute('uid'));
                        }
                    }
                }
            }
        }

        this.FixHeaders();
    }
};

CalendarView.prototype.__BuildDatePicker = function()
{
    if(!this.MonthContainer) return;
    
    var dayNames = new Array();
    dayNames.push('Ma');
    dayNames.push('Ti');
    dayNames.push('On');
    dayNames.push('To');
    dayNames.push('Fr');
    dayNames.push('Lø');
    dayNames.push('Sø');
    
    
    var tbl = this.Document.createElement('TABLE');
    
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.border=0;
    tbl.style.width='100%';
    
    var tb = this.Document.createElement('TBODY');
    
    var trTop = this.Document.createElement('TR');
    
    var tdBack = this.Document.createElement('TD');
    this.CurrentMonthYear = this.Document.createElement('TD');
    var tdNext = this.Document.createElement('TD');
    
    trTop.appendChild(tdBack);
    trTop.appendChild(this.CurrentMonthYear);
    trTop.appendChild(tdNext);

    this.CurrentMonthYear.innerHTML='month year';

    tdNext.align='right';
    
    tdBack.CalendarView = this;
    tdBack.onclick=function() { this.CalendarView.SetCurrentDateFromDT(this.CalendarView.SelectedDate.dateAdd('M',-1), true); };
    tdBack.className='calendarMonthBack';
    
    tdNext.CalendarView = this;
    tdNext.onclick=function() { this.CalendarView.SetCurrentDateFromDT(this.CalendarView.SelectedDate.dateAdd('M',1), true); };
    tdNext.className='calendarMonthNext';
    
    
    this.CurrentMonthYear.align='center';
    this.CurrentMonthYear.colSpan=5;
    
    tbl.appendChild(tb);
    tb.appendChild(trTop);
    
    
    for(var y=0;y<7;y++)
    {
        var tr = this.Document.createElement('TR');
        tb.appendChild(tr);
        for(var x = 0;x<7;x++)
        {
            var td = this.Document.createElement('TD');
            tr.appendChild(td);
            
            td.style.width=(100/7) + '%';
            if(y>0)
            {
                td.align='right';
                this.DaysInMonth.push(td);
                td.innerHTML='x';
                td.onmouseover=function() { if(this.className == 'calendarDaySelected') return; this.normalClassName=this.className; this.className='calendarDayHover'; };
                td.onmouseout=function() { if(this.className == 'calendarDaySelected') return; this.className=this.normalClassName; };
                td.CalendarView = this;
                td.onclick = function() { this.CalendarView.SetCurrentDateFromDT(this.Date); };
            }
            else
            {
                td.align='center';
                td.className='calendarDayHeader';
                td.innerHTML=dayNames[x];
            }
        }
    }
    
    this.MonthContainer.appendChild(tbl);
};

CalendarView.prototype.FixHeaders = function()
{
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        //this.CalendarViews[i].ReOrganizeAppointments();
        this.CalendarViews[i].HeaderControl.firstChild.style.width=(this.CalendarViews[i].Control.offsetWidth) -(document.all ? 1 : 1);
    }
};

CalendarView.prototype.__BuildHours = function()
{
    if(this.UsersContainer)
    {
        var tbl = this.Document.createElement('TABLE');
        var tb = this.Document.createElement('TBODY');
        var tr = this.Document.createElement('TR');
        var td = this.Document.createElement('TD');
        var tdEnd = this.Document.createElement('TD');
        
        td.style.width=50;
        
        
        tbl.cellPadding=0;
        tbl.cellSpacing=0;
        tbl.border=0;
        
        this.UsersContainer.appendChild(tbl);
        tbl.appendChild(tb);
        tb.appendChild(tr);
        tr.appendChild(td);
        tr.appendChild(tdEnd);
        
        td.innerHTML='<img src="images/spacer.gif" style="width:50px;height:1px;visbility:hidden;">';
        
        var endWidth = this.UsersContainer.offsetWidth - this.Container.offsetWidth;
        
        tdEnd.innerHTML='<img src="images/spacer.gif" style="width:17px;height:1px;visbility:hidden;">';
        
        this.UsersContainer.Control = tr;
        
    }
    
    var tdHours = this.Document.createElement('TD');
    this.CalendarControls.appendChild(tdHours);
    tdHours.vAlign='top';
    
    for(var i=0;i<24;i++)
    {
        var div = this.Document.createElement('DIV');
        div.style.width=50;
        div.style.height=46;
        div.className='calendarHoursFrame';
        div.innerHTML = (i<10 ? '0' : '') + i + '<span class="calendarSubTime">00</span>';
        tdHours.appendChild(div);
    }
};

CalendarView.prototype.GetViewFromId = function(uniqueidentifier)
{
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        if(this.CalendarViews[i].Uniqueidentifier == uniqueidentifier)
            return this.CalendarViews[i];
    }
    
    return;
};

CalendarView.prototype.AddView = function(name, color, uniqueidentifier)
{
    var v = new CalendarListview(this, name, color, uniqueidentifier);
    this.CalendarViews.push(v);
    
    var newW = (100 / this.CalendarViews.length).toFixed(2);
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        this.CalendarViews[i].Control.style.width=newW + '%';
        this.CalendarViews[i].ReOrganizeAppointments();
        this.CalendarViews[i].__setMeetingPointerBase();
    }
    
    this.FixHeaders();
    this.__LoadAppointmentsForView(uniqueidentifier);
    
    return v;
};

CalendarView.prototype.ReOrganizeAppointments = function()
{
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        this.CalendarViews[i].ReOrganizeAppointments();
    }
    this.FixHeaders();
};

CalendarView.prototype.RemoveView = function(name)
{
    for(var i=this.CalendarViews.length-1;i>=0;i--)
    {
        if(this.CalendarViews[i].Name == name)
        {
            this.CalendarViews[i].Clear();
            this.CalendarControls.removeChild(this.CalendarViews[i].Control);
            
            if(this.UsersContainer)
            {
                this.UsersContainer.Control.removeChild(this.CalendarViews[i].HeaderControl);
            }
            this.CalendarViews.splice(i,1);
        }
    }
    
    var newW = (100 / this.CalendarViews.length).toFixed(2);
    for(var i=0;i<this.CalendarViews.length;i++)
    {
        this.CalendarViews[i].Control.style.width=newW + '%';
        this.CalendarViews[i].ReOrganizeAppointments();
        this.CalendarViews[i].__setMeetingPointerBase();
    }
    this.FixHeaders();
};


var CalendarListview = function(CalView, name, color, uniqueidentifier)
{
    this.Name  = name;
    this.Appointments = new Array();
    this.Color = color;
    this.Uniqueidentifier = uniqueidentifier;
    
    this.HeaderControl = CalView.Document.createElement('TD');
    this.HeaderControl.innerHTML='<img src="images/spacer.gif" alt="" border="0" style=width:1px;height:1px;visibility:hidden;"><br>' + name;
    this.HeaderControl.align='center';
    this.HeaderControl.className='calendarUserHeader';
    this.HeaderControl.style.backgroundColor=color;
     
    this.CalendarView = CalView;

    if(CalView.UsersContainer)
    {
        var lc = CalView.UsersContainer.Control.lastChild;
        CalView.UsersContainer.Control.insertBefore(this.HeaderControl, lc);
    }
    
    this.Control = CalView.Document.createElement('TD');
    this.Control.vAlign='top';
    this.Control.style.backgroundColor=color;
    this.Control.className='calendarNoDayBackground';
    this.CalendarView.CalendarControls.appendChild(this.Control);
    
    var di = CalView.Document.createElement('DIV');
    di.style.backgroundColor=color;
    di.className='calendarDayBackground';
    di.style.position='relative';
    di.style.height=460;
    di.style.top=368;
    this.Control.appendChild(di);
    di.style.width='100%';

    
    this.MeetingPointer = CalView.Document.createElement('DIV');
    
    this.MeetingPointer.style.backgroundColor=color;
    this.MeetingPointer.style.position='relative';
    this.MeetingPointer.innerHTML='meeting';
    
    this.Control.appendChild(this.MeetingPointer);
    this.__setMeetingPointerBase();
    
    this.MeetingPointer.innerHTML=this.MeetingPointer.BaseY ;
    this.MeetingPointer.StartHour = '00:00';
    this.MeetingPointer.EndHour = '00:00';
    
    this.MeetingPointer.startY = 0;
    
    this.MouseIsDown = false;
    
    this.Control.Listview = this;
    this.Control.onmousemove = function() { this.Listview.MouseMove(); };
    this.Control.onmousedown = function() { this.Listview.MouseIsDown=true; this.Listview.MouseDown(); };
    this.Control.onmouseup = function() { this.Listview.MouseIsDown=false; };
    
    this.MeetingPointer.style.visibility='hidden';
};

CalendarListview.prototype.__setMeetingPointerBase = function()
{
    if(this.MeetingPointer.BaseY) return;
    var gpos = null; 
    gpos = core.DOM.GetObjectPosition(this.Control);
    this.MeetingPointer.BaseY = -this.MeetingPointer.offsetTop + (document.all ? 0 :gpos[1]);
};

CalendarListview.prototype.MouseDown = function()
{
    for(var i=0;i<this.CalendarView.CalendarViews.length;i++)
    {
        this.CalendarView.CalendarViews[i].MeetingPointer.style.visibility= (this.CalendarView.CalendarViews[i] == this ? '' : 'hidden');
    }
    
    var Y = parseInt((localCore.Mouse.Y + this.CalendarView.Container.scrollTop-23) / 23) * 23;
    this.MeetingPointer.startY = parseInt((localCore.Mouse.Y + this.CalendarView.Container.scrollTop-23) / 23) * 23;
    var Uh = this.CalendarView.UsersContainer.offsetHeight;
    this.MeetingPointer.style.height=23;
    
    this.MeetingPointer.StartHour = CoordsToTime(Y - (Uh > 0 ? 23 : 0));
    this.MeetingPointer.EndHour = CoordsToTime(Y+23 - (Uh > 0 ? 23 : 0));

    this.MeetingPointer.style.top = this.MeetingPointer.BaseY + Y - (Uh > 2 ? Uh+2 : Uh) - (document.all ? 1 : 0);
    
    this.MeetingPointer.innerHTML = this.MeetingPointer.StartHour + '-' + this.MeetingPointer.EndHour;
};

CalendarListview.prototype.__FixMeetingPointer = function(diff)
{
    if(this.MeetingPointer == 'hidden' || !this.MeetingPointer.style.top) return;
    this.MeetingPointer.style.top = parseInt(this.MeetingPointer.style.top) + diff;
};

CalendarListview.prototype.MouseMove = function()
{
    if(!this.MouseIsDown) return;
    var Uh = this.CalendarView.UsersContainer.offsetHeight;
    
    var Y = parseInt((localCore.Mouse.Y + this.CalendarView.Container.scrollTop) / 23) * 23;
    var nY = Y-this.MeetingPointer.startY;
    if(nY<23) nY=23;
    this.MeetingPointer.style.height=nY;
    
    this.MeetingPointer.EndHour = CoordsToTime(Y - (Uh > 0 ? 23 : 0));
    
    this.MeetingPointer.innerHTML = this.MeetingPointer.StartHour + '-' + this.MeetingPointer.EndHour;
};

CalendarListview.prototype.Clear = function()
{
    for(var i=this.Appointments.length-1;i>=0;i--)
    {
        if(this.Appointments[i] == this.CalendarView.SelectedAppointment && this.Appointments[i]!=null)
        {
            this.CalendarView.OnSelectAppointment(null);
        }
        
        this.MeetingPointer.BaseY += this.Appointments[i].Control.offsetHeight;
        this.__FixMeetingPointer(this.Appointments[i].Control.offsetHeight);
        this.Control.removeChild(this.Appointments[i].Control);
    }
    this.Appointments = new Array();
};

CalendarListview.prototype.RemoveAppointment = function(appointment)
{
    if(appointment == this.CalendarView.SelectedAppointment && appointment!=null)
    {
        this.CalendarView.OnSelectAppointment(null);
    }
    
    var offHeight = appointment.Control.offsetHeight;
    
    this.MeetingPointer.BaseY += appointment.Control.offsetHeight;
    this.__FixMeetingPointer(appointment.Control.offsetHeight);
    this.Control.removeChild(appointment.Control);
    for(var i=this.Appointments.length-1;i>=0;i--)
    {
        if(this.Appointments[i] == appointment)
        {
            this.Appointments.splice(i,1);
            break;
        }
        else
        {
            var al = 'new baseY: (old: ' + this.Appointments[i].Control.BaseY + ') new: ';
            
            this.Appointments[i].Control.BaseY +=offHeight;
            
            al += this.Appointments[i].Control.BaseY;
        }
    }
    this.reArrange();
};

CalendarListview.prototype.GetAppointmentFromId = function(id)
{
    
    for(var i=this.Appointments.length-1;i>=0;i--)
    {
        if(this.Appointments[i].Uniqueidentifier == id)
        {
            return this.Appointments[i];
        }
    }
};


CalendarListview.prototype.AddAppointment = function(startDate, endDate, Subject, location, uniqueidentifier)
{
    /*

    var offHeight = appointment.Control.offsetHeight;
    
    this.MeetingPointer.BaseY += appointment.Control.offsetHeight;
    this.__FixMeetingPointer(appointment.Control.offsetHeight);
    this.Control.removeChild(appointment.Control);

    this.reArrange();
    */
    var app = new CalendarAppointment(this, startDate, endDate, Subject, location, uniqueidentifier);
    var offHeight = app.Control.offsetHeight;
    this.MeetingPointer.BaseY -= app.Control.offsetHeight;
    this.__FixMeetingPointer(-app.Control.offsetHeight);
    this.Appointments.push(app);
    this.Appointments.sort(__CalendarListviewSortAppointmentsByStartPos);
    
    for(var i=this.Appointments.length-1;i>=0;i--)
    {
        if(this.Appointments[i] == app)
        {
            break;
        }
        else
        {
            var al = 'new baseY: (old: ' + this.Appointments[i].Control.BaseY + ') new: ';
            
            this.Appointments[i].Control.BaseY -=offHeight;
            
            al += this.Appointments[i].Control.BaseY;
        }
    }
    
    this.reArrange();
    
    return app;
};
function __CalendarListviewSortAppointmentsByStartPos(a,b)
{
    var aa = parseInt(a.Top,10);
    var bb = parseInt(b.Top,10);
    return aa-bb;
};

CalendarListview.prototype.reArrange = function()
{
    var maxRow = 0;
    for(var i=0;i<this.Appointments.length;i++)
    {
        var colObj = this.Collides(this.Appointments[i]);
        
        if(colObj)
        {
            this.Appointments[i].Row++;
            maxRow = maxRow < this.Appointments[i].Row ? this.Appointments[i].Row : maxRow;
        }
        this.Appointments[i].FixPosition();
    }
    
    if(maxRow>0)
    {
        var percentage = 90/(maxRow+1);
        for(var i=0;i<this.Appointments.length;i++)
        {
            this.Appointments[i].Control.style.width=percentage + '%';
            this.Appointments[i].Control.style.left = this.Appointments[i].Control.BaseX + ( this.Appointments[i].Control.offsetWidth * this.Appointments[i].Row) - (!document.all && this.Appointments[i].Row>0 ? 4:0);
        }
    }
};

CalendarListview.prototype.ReOrganizeAppointments = function()
{
    for(var i=0;i<this.Appointments.length;i++)
    {
        this.Appointments[i].Control.style.left = this.Appointments[i].Control.BaseX + ( this.Appointments[i].Control.offsetWidth * this.Appointments[i].Row) - (!document.all && this.Appointments[i].Row>0 ? 4:0);
    }
};

CalendarListview.prototype.Collides = function(app)
{
    
    for(var i=0;i<this.Appointments.length;i++)
    {
        if(app !=this.Appointments[i])
        {
            var app2 = this.Appointments[i];
            
            if(app.Top>=app2.Top && app.Top< app2.Bottom && app.Row == app2.Row)
            {
                return app2;
                return;
            }
        }
    }
    
    return null;
};

var CalendarAppointment = function(calendarListview, startDate, endDate, subject, location, uniqueidentifier)
{
    this.Listview = calendarListview;
    this.CalendarView = calendarListview.CalendarView
    this.StartDate = startDate;
    this.EndDate = endDate;
    this.Subject = subject;
    this.Location = location;
    this.Uniqueidentifier = uniqueidentifier;
    
    this.Control = this.CalendarView.Document.createElement('DIV');
    this.Control.innerHTML=subject;
    
    this.Control.className='calendarAppointment';
    this.Control.style.backgroundColor=this.Listview.Color;

    var Y1 = TimeToCalendarCoords(startDate);
    var Y2 = TimeToCalendarCoords(endDate);

    this.Top = Y1;
    this.Bottom = Y2;
    this.Row = 0;
    
    //Figure out where to put me
    var ibefore = this.Listview.MeetingPointer;
    for(var i=0;i<this.Listview.Appointments.length;i++)
    {
        if(this.Listview.Appointments[i].Top>this.Top) 
        {
            ibefore = this.Listview.Appointments[i].Control;
            break;
        }
    }
    
    this.Listview.Control.insertBefore(this.Control,ibefore);
    
    var gpos = null;
    
    if(!document.all)
    {
        gpos = core.DOM.GetObjectPosition(this.Listview.Control);
    } 
    
    this.Control.BaseX = -this.Control.offsetLeft + (document.all ? this.Listview.Control.offsetLeft : gpos[0]+2);
    this.Control.BaseY = -this.Control.offsetTop + (document.all ? -1 : gpos[1]);
    
   
    this.Control.style.left = this.Control.BaseX;
    this.Control.style.top = this.Control.BaseY + Y1;
    this.Control.style.height= Y2 - Y1 - (document.all ? 0 : 2);
    this.Control.style.width='90%';
    this.Control.style.border='solid 1px ' + this.Listview.Color;
    
    this.Control.Appointment = this;
    
    this.Control.ondblclick = function() {
                                            if(this.Appointment.CalendarView.OnAppointmentDoubleClick)
                                            {
                                                this.Appointment.CalendarView.OnAppointmentDoubleClick(this.Appointment.Uniqueidentifier);
                                            }
                                          }
    
    this.Control.onclick = function() {
                                                if(this.Appointment.CalendarView.SelectedAppointment)
                                                {
                                                    this.Appointment.CalendarView.SelectedAppointment.Control.style.border='solid 1px ' + this.Appointment.CalendarView.SelectedAppointment.Listview.Color;
                                                    //this.Appointment.CalendarView.SelectedAppointment.Control.style.height = this.Appointment.CalendarView.SelectedAppointment.Control.offsetHeight+2;
                                                }
                                                //this.style.height = this.offsetHeight-2;
                                                this.style.border='solid 2px black';
                                                
                                                this.Appointment.CalendarView.SelectedAppointment = this.Appointment;
                                                this.className='calendarAppointmentSelected'; 
                                                
                                                this.Appointment.CalendarView.OnSelectAppointment(this.Appointment);
                                            };
};

CalendarAppointment.prototype.FixPosition = function()
{
    var gpos = null;
    
    if(!document.all)
    {
        gpos = core.DOM.GetObjectPosition(this.Listview.Control);
    } 
    
    var Y1 = TimeToCalendarCoords(this.StartDate);
    this.Top = Y1;
    this.Control.style.top = this.Control.BaseY + Y1;
};

function TimeToCalendarCoords(time)
{
    var args = time.split(' ');
    args = args[args.length-1];
    args = args.split(':');

    args[0] = ((parseInt(args[0],10) * 60) + parseInt(args[1],10)) /60 * 46;
    
    return args[0];
};

function CoordsToTime(ypos)
{
    var timeA = Math.floor(ypos/46);
    var timeB = (ypos/46) - timeA;
    if(timeB!=0)
        timeB = 60*timeB;
    else
        timeB = '00';
        
    timeA = timeA.toString();
    while(timeA.length<2)
        timeA = '0' + timeA;
        
    return timeA + ':' + timeB;
    
};
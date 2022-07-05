function DatePicker(ownerDocument, ownerContainer, ownerWindow)
{
    this.ContextMenu = new ContextMenu(ownerDocument, ownerContainer, ownerWindow);
    this.ContextMenu.AutoHide = false;
    
    this.Document = ownerDocument;
    this.Container = ownerContainer;
    this.Window = ownerWindow;
    
    this.OnPickDate = null;
    
    this.Control = this.ContextMenu.AddCustomData().CustomDataContainer;
    
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
    this.DaysInMonth = new Array();
    
    this.CurrentYear = this.SelectedDate.getFullYear();
    this.CurrentMonth = this.SelectedDate.getMonth();
    this.CurrentDay = this.SelectedDate.getDate();
    
    this.__BuildDatePicker();
    this.SetCurrentDate(this.CurrentDay , this.CurrentMonth , this.CurrentYear);
};

DatePicker.prototype.ShowFromOwner = function(obj)
{
    this.ContextMenu.ShowFromOwner(obj);
};
DatePicker.prototype.Show = function(x, y)
{
    this.ContextMenu.Show(x, y);
};
DatePicker.prototype.ShowStatic = function(x, y)
{
    this.ContextMenu.ShowStatic(x, y);
};
DatePicker.prototype.Hide = function()
{
    this.ContextMenu.Hide();
};

DatePicker.prototype.__BuildDatePicker = function () {
    if (!this.Control) return;

    var dayNames = new Array();
    dayNames.push('Ma');
    dayNames.push('Ti');
    dayNames.push('On');
    dayNames.push('To');
    dayNames.push('Fr');
    dayNames.push('Lø');
    dayNames.push('Sø');


    var tbl = this.Document.createElement('TABLE');

    tbl.cellPadding = 0;
    tbl.cellSpacing = 0;
    tbl.border = 0;
    tbl.style.width = '100%';
    tbl.style.backgroundColor = '#f1f6fb';

    var tb = this.Document.createElement('TBODY');

    var trTop = this.Document.createElement('TR');

    var tdBack = this.Document.createElement('TD');
    this.CurrentMonthYear = this.Document.createElement('TD');
    var tdNext = this.Document.createElement('TD');

    trTop.appendChild(tdBack);
    trTop.appendChild(this.CurrentMonthYear);
    trTop.appendChild(tdNext);

    this.CurrentMonthYear.innerHTML = 'month year';

    tdNext.align = 'right';

    tdBack.CalendarView = this;
    tdBack.onclick = function () { this.CalendarView.SetCurrentDateFromDT(this.CalendarView.SelectedDate.dateAdd('M', -1), true); };
    tdBack.className = 'calendarMonthBack';

    tdNext.CalendarView = this;
    tdNext.onclick = function () { this.CalendarView.SetCurrentDateFromDT(this.CalendarView.SelectedDate.dateAdd('M', 1), true); };
    tdNext.className = 'calendarMonthNext';


    this.CurrentMonthYear.align = 'center';
    this.CurrentMonthYear.colSpan = 5;

    tbl.appendChild(tb);
    tb.appendChild(trTop);


    for (var y = 0; y < 7; y++) {
        var tr = this.Document.createElement('TR');
        tb.appendChild(tr);
        for (var x = 0; x < 7; x++) {
            var td = this.Document.createElement('TD');
            tr.appendChild(td);

            td.style.width = (100 / 7) + '%';
            if (y > 0) {
                td.align = 'right';
                this.DaysInMonth.push(td);
                td.innerHTML = 'x';
                td.onmouseover = function () { if (this.className == 'calendarDaySelected') return; this.normalClassName = this.className; this.className = 'calendarDayHover'; };
                td.onmouseout = function () { if (this.className == 'calendarDaySelected') return; this.className = this.normalClassName; };
                td.CalendarView = this;
                td.onclick = function () {

                    if (this.CalendarView.OnPickDate) {
                        this.CalendarView.OnPickDate(this.Date);
                    }
                    this.CalendarView.SetCurrentDateFromDT(this.Date);
                    this.CalendarView.Hide();
                };
            }
            else {
                td.align = 'center';
                td.className = 'calendarDayHeader';
                td.innerHTML = dayNames[x];
            }
        }
    }

    this.Control.appendChild(tbl);
};

DatePicker.prototype.SetCurrentDateFromYMD = function(val)
{
    var dt = Date.parse(val) || new Date();
    this.SetCurrentDate(dt.getDate(), dt.getMonth(), dt.getFullYear());
};

DatePicker.prototype.GetDateFromYMD = function(val)
{
    return Date.parse(val) || new Date();
};

DatePicker.prototype.SetCurrentDate = function (day, month, year) {
    var dt = new Date();

    if (isNaN(day) || isNaN(month) || isNaN(year)) {
        dt = new Date();
        this.SetCurrentDate(dt.getDate(), dt.getMonth(), dt.getFullYear());
        return;
    }

    try {
        this.SelectedDate.setFullYear(year);
        this.SelectedDate.setMonth(month);
        this.SelectedDate.setDate(day);

        dt.setFullYear(year, month, day);
    }
    catch (err) {
        dt = new Date();
        this.SetCurrentDate(dt.getDate(), dt.getMonth(), dt.getFullYear());
        return;
    }

    dt = dt.dateAdd('d', -dt.getDate());

    if (dt.getDay() != 1) {
        while (dt.getDay() != 1) {
            dt = dt.dateAdd('d', -1);
        }
    }

    this.CurrentMonthYear.innerHTML = this.MonthNames[month] + ' ' + year;



    for (var i = 0; i < this.DaysInMonth.length; i++) {
        var tdDay = this.DaysInMonth[i];


        tdDay.Date = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());

        tdDay.innerHTML = tdDay.Date.getDate();

        tdDay.className = dt.getMonth() == this.SelectedDate.getMonth() && dt.getDate() == this.SelectedDate.getDate() ? 'calendarDaySelected' : this.SelectedDate.getMonth() == dt.getMonth() ? 'calendarDay' : 'calendarDayOutOfMonth';

        dt = dt.dateAdd('d', 1);
    }
};

DatePicker.prototype.SetCurrentDateFromDT = function(date)
{
    this.SetCurrentDate(date.getDate(), date.getMonth(), date.getFullYear());
};
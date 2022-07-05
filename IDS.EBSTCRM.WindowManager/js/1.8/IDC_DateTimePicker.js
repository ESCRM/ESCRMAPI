var activeOwner=null;
var callBack=null;

function dateTime_Hide()
{
    var dt = dateTime_getDTPicker(owner, doc);
    dt.blocker.style.display='none';
    dt.style.display='none';
};

function dateTime_pickDate(owner, callback, doc, X, Y)
{
    callBack=callback;
    
    var dt = dateTime_getDTPicker(owner, doc);
    var aId= activeOwner == null ? '' : activeOwner.id;
    
    var hp = dateTime_getHPicker(owner);
    hp.style.display='none';
    
    if(dt.style.display!='none' && aId == owner.id)
    {
        dt.blocker.style.display='none';
        dt.style.display='none';
    }
    else
    {
    
        dateTime_populate(dt,owner, dateTime_parseDate(owner.value), doc);
        
        var pos = getGlobalPosition(owner);
        
        if(X){ pos[0]=X; }
        if(Y){ pos[1]=Y - owner.offsetHeight; }
        
        var internalTop=0;
        var tmp = owner;
        while(tmp.tagName!='DIV' && tmp.tagName != 'BODY')
        {
            tmp=tmp.parentNode;
            if(!tmp)
                break;
        }
        if(tmp)
        {
            if(tmp.tagName=='DIV')
            {
                internalTop = tmp.scrollTop;
                pos[0]-=25;
            }
        }
        
        dt.blocker.style.display='';
        dt.style.display='';
        dt.style.left = pos[0];
        dt.style.top  = pos[1] + parseInt(owner.offsetHeight)+2 - internalTop;
    
        try
        {
            if(ignoreClick)
            {
                ignoreClick= true;
                setTimeout('ignoreClick=false;',100);
                clickedPopup = dt;
            }
        }
        catch(err)
        {
        }
    
    }
    
    activeOwner = owner;
    
};

function dateTime_pickHour(owner, callback)
{
    callBack=callback;
    
    var dt = dateTime_getHPicker(owner);
    var aId= activeOwner == null ? '' : activeOwner.id;
    
    var dp = dateTime_getDTPicker(owner);
    dp.style.display='none';
    
    if(dt.style.display!='none' && aId == owner.id)
    {
        dt.style.display='none';
    }
    else
    {
        dateTime_populateHours(owner, dt);
        
        var pos = getGlobalPosition(owner);
        
        dt.style.display='';
        
        
        dt.style.left = pos[0]-85;
        dt.style.top  = pos[1] + parseInt(owner.offsetHeight)+2;
        
    }
    
    activeOwner = owner;
};

function dateTime_getHPicker(owner)
{
    var dt = document.getElementById('dtHourPicker');
    if(dt == null)
        dt= dateTime_createHPicker();
        
    return dt;
};

function dateTime_populateHours(owner, tbl)
{
    var tds=tbl.getElementsByTagName('TD');
    
    for(var i=0;i<tds.length;i++)
    {
        var td = tds[i];
        td.onclick=function() { owner.value=this.innerHTML ; dateTime_pickHour(owner,callBack); if(callBack != null) callBack(); };
        td.onmouseover=function() {this.style.backgroundColor='orange';};
        td.onmouseout=function() {this.style.backgroundColor= (this.innerHTML==owner.value ? 'orange' : (this.isGray == 1 ? '#f1f1f1' : 'white')) ;};
        
        td.style.backgroundColor=(td.innerHTML==owner.value ? 'orange' : (td.isGray == 1 ? '#f1f1f1' : 'white'));
    }
};

function dateTime_createHPicker()
{
    var d = document;
    var tbl = d.createElement('TABLE');
    var tbody = d.createElement('TBODY');
    
    tbl.id='dtHourPicker';
    tbl.style.position='absolute';
    tbl.style.display='none';
    tbl.style.zIndex='3000';
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.className='calendarShell';
    
    tbl.appendChild(tbody);
    
    var hr=0;
    var mn=0;
    

    var row =0;
    
    for(var h=0;h<48;h++)
    {
        var tr = tbody.childNodes[row];
        if(tr == null)
        {
            var tr = d.createElement('TR');
            tbody.appendChild(tr);
        }
        
        var td = d.createElement('TD');  
        tr.appendChild(td);     
        td.innerHTML = formatNumber(parseInt(hr),2) + ':' + formatNumber(parseInt(mn),2);
        td.align='right';
        td.className='calendarHour';
        
        if(h<16 || h>32)
        {
            td.isGray='1';
        }
        
        if(h<36)
        {
            td.style.borderRight='solid 1px #e1e1e1';
        }
        
        mn+=30;
        if(mn==60)
        {
            mn=0;
            hr++;
        }
        
        row++;
        if(row==12) row=0;
    }
    
    d.body.appendChild(tbl);
    
    return tbl;
};



function dateTime_getDTPicker(owner, doc)
{
    if(!doc) doc = document;
    
    var dt = doc.getElementById('dtDatePicker');
    if(dt == null)
        dt= dateTime_createDTPicker(doc);
        
    return dt;
    
};

function dateTime_createDTPicker(doc)
{
    var d = (doc == null ? document : doc);
    
    
    
    
    var tbl = d.createElement('TABLE');
    var tbody = d.createElement('TBODY');
    
    tbl.id='dtDatePicker';
    tbl.style.position='absolute';
    tbl.style.zIndex='250001';
    tbl.cellPadding=0;
    tbl.cellSpacing=0;
    tbl.className='calendarShell';
    
    tbl.appendChild(tbody);
    
    var day=0;
    
    var dayNames = new Array();
    dayNames[0]='Mo';
    dayNames[1]='Tu';
    dayNames[2]='We';
    dayNames[3]='Th';
    dayNames[4]='Fr';
    dayNames[5]='Sa';
    dayNames[6]='Su';
    
    var trTop = d.createElement('TR');
    var tdTopL = d.createElement('TD');
    var tdTopM = d.createElement('TD');
    var tdTopR = d.createElement('TD');
    
    tdTopM.colSpan=5;
    tdTopM.innerHTML = 'Month';
    tdTopL.innerHTML ='<img src="images/tinyArrowLeft.gif" alt="" border="0">';
    tdTopR.innerHTML ='<img src="images/tinyArrowRight.gif" alt="" border="0">';
    
    tdTopM.className='calendarMonth';
    tdTopL.className='calendarMonthClickAble';
    tdTopR.className='calendarMonthClickAble';
    
    tdTopM.align='center';
    tdTopL.align='center';
    tdTopR.align='center';
    
    trTop.appendChild(tdTopL);
    trTop.appendChild(tdTopM);
    trTop.appendChild(tdTopR);
    
    tbody.appendChild(trTop);
    
    for(var y=0;y<7;y++)
    {
        var tr=d.createElement('TR');
        tbody.appendChild(tr);
        for(var x=0;x<7;x++)
        {
            var td = d.createElement('TD');
            tr.appendChild(td);
            
            td.innerHTML=dayNames[x];
            td.className='calendarDayHeader';
            day++;
        }
    }
    
    

    var blocker = d.createElement('DIV');
    blocker.style.position='absolute';
    blocker.style.zIndex=250000;
    blocker.style.display='none';
    
    d.body.appendChild(blocker);
    d.body.appendChild(tbl);
    
    blocker.style.left=0;
    blocker.style.top=0;
    blocker.style.width='100%';
    blocker.style.height='100%';
    blocker.picker = tbl;
    blocker.onclick=function() { this.picker.style.display='none'; this.style.display='none'; };
    
    if(document.all)
    {
        blocker.style.backgroundColor='blue';
        blocker.style.filter = 'alpha(opacity=0)';
    }
    tbl.blocker = blocker;
    return tbl;
};

function dateTime_parseDate(string)
{
    if(string != '')
    {
        var ds = string.split('-');
        var dt = new Date();
        try
        {
            dt.setFullYear(ds[2],ds[1]-1,ds[0]);
        }
        catch(exp)
        {
            dt = new Date();
        }
    }
    else
        dt = new Date();
        
    dt = dt.dateAdd('h',-dt.getHours());
    dt = dt.dateAdd('n',-dt.getMinutes());
        
    return dt;
};

function dateTime_populate(tbl,owner, date, doc)
{
    
    var months = new Array();
    months[0] = 'January';
    months[1] = 'February';
    months[2] = 'March';
    months[3] = 'April';
    months[4] = 'May';
    months[5] = 'June';
    months[6] = 'July';
    months[7] = 'August';
    months[8] = 'September';
    months[9] = 'October';
    months[10] = 'November';
    months[11] = 'December';
    
    var dt = new Date();
    var currentDate = new Date();
    var ad = new Date();
    
    dt.setFullYear(date.getFullYear(),date.getMonth(), date.getDate());
    currentDate.setFullYear(date.getFullYear(), date.getMonth(), date.getDate());

    
    dt = dt.dateAdd('d',-dt.getDate());
    
    while(dt.getDay()!=1)
    {
        dt = dt.dateAdd('d',-1);
    }
    
    var tbody = tbl.childNodes[0];
    
    tbody.childNodes[0].childNodes[1].innerHTML=months[currentDate.getMonth()] + ' ' + currentDate.getFullYear();
    
    tbody.childNodes[0].childNodes[0].onclick=function() {
                                                            dateTime_populate(tbl,owner, currentDate.dateAdd('m',-1), doc);
                                                         }

    tbody.childNodes[0].childNodes[2].onclick=function() {
                                                            dateTime_populate(tbl,owner, currentDate.dateAdd('m',1), doc);
                                                         }
    
    for(var y=2;y<tbody.childNodes.length;y++)
    {
        var tr=tbody.childNodes[y];
        for(var x=0;x<tr.childNodes.length;x++)
        {
            var td=tr.childNodes[x];
            
            td.innerHTML=dt.getDate();
            td.className = (dt.getMonth() == currentDate.getMonth() ? (dt.getFullYear() == currentDate.getFullYear() && dt.getMonth() == currentDate.getMonth() && dt.getDate() == currentDate.getDate() ? 'calendarDayChosen' : (dt.getFullYear() == ad.getFullYear() && dt.getMonth() == ad.getMonth() && dt.getDate() == ad.getDate() ? 'calendarDayToDay' : 'calendarDay')) : 'calendarDayOutOfMonth');
            td.align='right';
            td.myDate=dateTime_dateToString(dt);
            td.onclick= function () { owner.value = this.myDate; dateTime_pickDate(owner, callBack, doc); if(callBack!=null) callBack(owner); };
            
            dt = dt.dateAdd('d',1);
        }
    }
    
};

function dateTime_dateToString(dt)
{
    var day = dt.getDate();
    var month = dt.getMonth()+1;
    var year = dt.getFullYear();
    
    day=formatNumber(day,2);
    month=formatNumber(month,2);
    
    return day + '-' + month + '-' + year;

};

function formatNumber(num, len)
{
    num = num.toString();
    
    
    while(num.length<len)
    {
        num = '0' + num;
    }
    
    return num;
};

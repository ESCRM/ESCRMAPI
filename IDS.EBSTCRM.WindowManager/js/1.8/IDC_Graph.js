var IDC_Graph = function(Document, ParentObject, idcCore)
{
    this.Document = Document || document;
    this.ParentObject = ParentObject || document.body;
    this.Core = idcCore || new IDC_Core(this.Document, ParentObject);
    
    this.Orientation = 0;
    this.AxisXName = 'X Axis';
    this.AxisYName = 'X Axis';
    
    this.BarColors = ["#576d8b", "#578b57", "#8b7157", "#8b5757", "#8b5787", "#312a90", "#2f902a", "#79902a", "#21c9cb"];
    
    this.Bars = new Array();
    
    this.Legends = new Array();
    
    this.Width='100%';
    this.Height='100%';
    
    this.Control=null;
};

IDC_Graph.prototype.LoadGraph = function(url, formPostArguments)
{
    var AjaxObject = new this.Core.Ajax();
    AjaxObject.requestFile = url;
    
    if(formPostArguments)
    {
        for(var i=0;i<formPostArguments.length;i++)
        {
            var arg = formPostArguments[i];
            if(arg.length==2)
                AjaxObject.encVar(arg[0],arg[1]);
        }
    }
    
   
    var me = this;
    AjaxObject.OnCompletion = function(){ me._LoadGraph(AjaxObject); }; 
    AjaxObject.OnError = function(){ me._LoadGraph(AjaxObject);};
    AjaxObject.OnFail = function(){ me._LoadGraph(AjaxObject);};
    AjaxObject.RunAJAX();
};

IDC_Graph.prototype._LoadGraph = function(AjaxObject)
{
    var result=AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject=null;
        
    this.ParseHTML(result);
};

IDC_Graph.prototype.ParseHTML = function(html)
{

    
    this.Bars.length=0;
    this.Bars=new Array();
    
    this.Legends.length=0;
    this.Legends=new Array();
    
    if(html != null && html!='')
    {
        var div = this.Document.createElement('DIV');
        div.innerHTML=html;
        
        for(var i=0;i<div.childNodes.length;i++)
        {
                var n = div.childNodes[i];
                
                if(n.tagName=='I')
                {
                    //legends
                    this.Legends.push(n.innerHTML);
                }
                else
                {
                
                    var items = new Array();
                    
                    for(var ii=0;ii<n.childNodes.length;ii++)
                    {
                        items.push(n.childNodes[ii].innerHTML);
                    }
                    
                    
                    this.Add(n.getAttribute('identifier'), items, n.getAttribute('suburl'), n.getAttribute('gobackurl'), n.getAttribute('title'), n.getAttribute('gobacktext'));
                }
        }
        
        div.innerHTML='';
        div=null;
    }
    
    this.Render();    
};

IDC_Graph.prototype.Render = function()
{
    if(!this.Control)
    {
        this.Control = this.Document.createElement('P');
        var tbl = this.Document.createElement('TABLE');
        
        tbl.cellPadding=0;
        tbl.cellSpacing=0;
        tbl.border=0;
        
        tbl.style.width='100%';
        tbl.style.height='100%';
        
        this.Control.appendChild(tbl);
        this.Control.className='IDCGraph';
        
        this._body = this.Document.createElement('TBODY');
        
        this.Control.style.width=this.Width;
        this.Control.style.height=this.Height;
        this.Control.style.display='inline-block';
        
        this.ParentObject.appendChild(this.Control);
        tbl.appendChild(this._body);
    }

    while(this._body.hasChildNodes())
    {
        this._body.removeChild(this._body.lastChild);
    }
    
    //Calculate max values
    var myMax=0;
    var graphMax=0;
    var graphs = 0;
    
    for(var i=0;i<this.Bars.length;i++)
    {
        var bar = this.Bars[i];
        for(var ii=0;ii<bar.Items.length;ii++)
        {
            if(myMax<parseInt(bar.Items[ii],10))
                myMax = parseInt(bar.Items[ii],10);
            graphs++;
        }
    }
    
    var diminus= myMax.toString().length-1;
    
    do
    {
        var di = myMax.toString().length - diminus;
        var tmp = '1';
        for(var i=0;i<di;i++)
            tmp+='0';
            
        
        
        tmp = parseInt(tmp,10);
        graphMax = Math.ceil( myMax / tmp ) * tmp;
        
        diminus--;
        
    } while(graphMax / tmp >10)
    
    if((graphMax / tmp)<5)
    {
        tmp/=2;
    }
    
    
    
    if(tmp == graphMax) tmp = 1;
    if(tmp<1) tmp=1;
    if(graphMax<1) graphMax=1;
    if(tmp>graphMax) tmp = graphMax;

    if(this.Orientation==1)
    {
        var barColor = 0;

        //Create the legends
        var trTitle = this.Document.createElement('TR');
        this._body.appendChild(trTitle);
        var tdTitle = this.Document.createElement('TD');
        trTitle.appendChild(tdTitle);
        tdTitle.style.height=1;
        tdTitle.align='center';
        tdTitle.className='IDCGraph_Header';
        
        var trLegend = this.Document.createElement('TR');
        this._body.appendChild(trLegend);
        var tdLegend = this.Document.createElement('TD');
        trLegend.appendChild(tdLegend);
        tdLegend.style.height=1;
        tdLegend.align='center';
        
        for(var i=0;i<this.Legends.length;i++)
        {
            var spl = this.Document.createElement('SPAN');
            spl.className='IDCGraph_Legend';
            tdLegend.appendChild(spl);
            spl.style.backgroundColor = this.BarColors[barColor];
            
            var splt = this.Document.createElement('SPAN');
            splt.className='IDCGraph_LegendText';
            splt.innerHTML = this.Legends[i];
            tdLegend.appendChild(splt);
            
            barColor++;
            if(barColor==this.BarColors.length) barColor=0;
            
        }
        
        
        var grfColspan=0;
        for(var i=0;i<graphMax;i+=tmp)
        {
            grfColspan++;
        }
        
        for(var i=0;i<this.Bars.length;i++)
        {           
        
            if(bar.Title)
            {
                tdTitle.innerHTML = bar.Title;
            }
            
            if(bar.GoBackUrl && i==0)
            {
                var goBack = this.Document.createElement('SPAN');
                tdLegend.insertBefore(goBack,tdLegend.firstChild);
                goBack.style.cursor='pointer';
                goBack.className='IDCGraph_GoBack';
                goBack.Graph = this;
                goBack.GoBackUrl = bar.GoBackUrl;
                goBack.onclick=function() { this.Graph.LoadGraph(this.GoBackUrl); };
                goBack.innerHTML = bar.GoBackText;
                
            }
            
            var bar = this.Bars[i];
            barColor=0;
            
            var trs = this.Document.createElement('TR');
            var tds = this.Document.createElement('TD');
            
            this._body.appendChild(trs);
            
            tds.className='IDCGraph_HorizontalBarSpacer';
            tds.innerHTML='&nbsp';
            tds.colSpan=grfColspan;
            for(var ii=0;ii<bar.Items.length;ii++)
            {
                var tr = this.Document.createElement('TR');
                this._body.appendChild(tr);
                
                //Identifier
                if(ii==0)
                {
                    var tdI = this.Document.createElement('TD');
                    tdI.innerHTML=bar.Identifier;
                    tdI.noWrap=true;
                    trs.appendChild(tdI);
                    trs.appendChild(tds);
                    tdI.rowSpan = bar.Items.length+2;
                    tdI.className='IDCGraph_YAxis';
                }
                
                var td = this.Document.createElement('TD');
                tr.appendChild(td);
                td.style.width='100%';
                td.colSpan=grfColspan;
                
                if(bar.SubUrl)
                {
                    td.style.cursor='pointer';
                    td.Graph = this;
                    td.SubUrl = bar.SubUrl;
                    td.onclick=function() { this.Graph.LoadGraph(this.SubUrl); };
                }
                
                var percentage = parseInt(Math.ceil(parseInt(bar.Items[ii],10) / graphMax * 100),10);
                if(isNaN(percentage)) percentage=0;
                if(percentage>100) percentage=100;
                
                var div = this.Document.createElement('SPAN');
                div.style.backgroundColor = this.BarColors[barColor];
                div.style.width=parseInt(percentage,10) + '%';
                div.title = bar.Items[ii];
                
                div.className='IDCGraph_HorizontalBar';

                td.appendChild(div);
                
                barColor++;
                if(barColor==this.BarColors.length) barColor=0;                
            }
            
            var trs2 = this.Document.createElement('TR');
            var tds2 = this.Document.createElement('TD');
            
            this._body.appendChild(trs2);
            trs2.appendChild(tds2);
            tds2.className='IDCGraph_HorizontalBarSpacer';
            tds2.innerHTML='&nbsp';
            tds2.colSpan=grfColspan;
        }
        
        //add identifiers
        var trI = this.Document.createElement('TR');
        var tdZ = this.Document.createElement('TD');
        tdZ.innerHTML='0';
        tdZ.className='IDCGraph_YAxisZero';
        tdZ.align='right';
        
        this._body.appendChild(trI);
        trI.appendChild(tdZ);
        
        //Create X Identifiers
        var legendColspan=1;
        
        //alert(graphMax + ' / ' + tmp + ' = ');
        var xw = tmp/graphMax*100;
        
        for(var i=0;i<graphMax;i+=tmp)
        {            
            var tdXi = this.Document.createElement('TD');
            trI.appendChild(tdXi);
            
            tdXi.innerHTML = i+tmp;
            tdXi.style.width=xw + '%';
            
            tdXi.className='IDCGraph_YAxis';
            tdXi.align='right';
            
            legendColspan++;
            
        }
        tdLegend.colSpan=legendColspan;
        tdTitle.colSpan = legendColspan;
    }
    else if(this.Orientation==0)
    {
        var barColor = 0;

        //Create the legends
        var trTitle = this.Document.createElement('TR');
        this._body.appendChild(trTitle);
        var tdTitle = this.Document.createElement('TD');
        trTitle.appendChild(tdTitle);
        tdTitle.style.height=1;
        tdTitle.align='center';
        tdTitle.className='IDCGraph_Header';
        
        var trLegend = this.Document.createElement('TR');
        this._body.appendChild(trLegend);
        var tdLegend = this.Document.createElement('TD');
        trLegend.appendChild(tdLegend);
        tdLegend.style.height=1;
        tdLegend.align='center';
        
        
        for(var i=0;i<this.Legends.length;i++)
        {
            var spl = this.Document.createElement('SPAN');
            spl.className='IDCGraph_Legend';
            tdLegend.appendChild(spl);
            spl.style.backgroundColor = this.BarColors[barColor];
            
            var splt = this.Document.createElement('SPAN');
            splt.className='IDCGraph_LegendText';
            splt.innerHTML = this.Legends[i];
            tdLegend.appendChild(splt);
            
            barColor++;
            if(barColor==this.BarColors.length) barColor=0;
            
        }
        
        //Create framework for vertical bars
        var trY = this.Document.createElement('TR');
        var trX = this.Document.createElement('TR');
        
        this._body.appendChild(trY);
        
        
        //Create Y Identifiers
        for(var i=0;i<graphMax;i+=tmp)
        {
            var trYi = i==0 ? trY : this.Document.createElement('TR');
            if(!trYi.parentNode)
                this._body.appendChild(trYi);
            
            var tdYi = this.Document.createElement('TD');
            trYi.appendChild(tdYi);
            
            tdYi.innerHTML = graphMax-i;
            
            tdYi.className='IDCGraph_YAxis';
            tdYi.vAlign='top';
            tdYi.align='right';
            
        }
        
        var tdXi = this.Document.createElement('TD');
        tdXi.innerHTML = '0';
        tdXi.vAlign='top';
        tdXi.align='right';
        tdXi.style.height='1px';
        tdXi.className='IDCGraph_YAxisZero';
        trX.appendChild(tdXi);

        

        this._body.appendChild(trX);
        
        //Write out bars vertically
        var itemW = (100/this.Bars.length) + '%';
        var graphWidth = (100/graphs) + '%';
        
        var legendColspan=1;
        
        for(var i=0;i<this.Bars.length;i++)
        {
            legendColspan+=2;
            
            var bar = this.Bars[i];
            barColor=0;
            
            var tdSpacer = this.Document.createElement('TD');
            var rowspan = graphMax/tmp;
            if(rowspan<1) rowspan=1;
            
            tdSpacer.rowSpan = rowspan;
            tdSpacer.className='IDCGraph_VerticalBarSpacer';
            trY.appendChild(tdSpacer);
            
            if(bar.Title)
            {
                tdTitle.innerHTML = bar.Title;
            }
            
            if(bar.GoBackUrl && i==0)
            {
                var goBack = this.Document.createElement('SPAN');
                tdLegend.insertBefore(goBack,tdLegend.firstChild);
                goBack.style.cursor='pointer';
                goBack.className='IDCGraph_GoBack';
                goBack.Graph = this;
                goBack.GoBackUrl = bar.GoBackUrl;
                goBack.onclick=function() { this.Graph.LoadGraph(this.GoBackUrl); };
                goBack.innerHTML = bar.GoBackText;
                
            }
                
            for(var ii=0;ii<bar.Items.length;ii++)
            {
                legendColspan++;
                
                var td = this.Document.createElement('TD');
                td.rowSpan = rowspan;
                
                td.vAlign='bottom';
                td.align='center';
                td.style.backgroundColor='white';
                

                if(bar.SubUrl)
                {
                    td.style.cursor='pointer';
                    td.Graph = this;
                    td.SubUrl = bar.SubUrl;
                    td.onclick=function() { this.Graph.LoadGraph(this.SubUrl); };
                }
                
                trY.appendChild(td);
                td.style.height='100%';
                td.style.verticalAlign='bottom';
                td.style.width  =  document.all ? graphWidth : '';
                
                var percentage = parseInt(Math.ceil(parseInt(bar.Items[ii],10) / graphMax * 100),10);
                
                if(percentage>100) percentage=100;
                var div = this.Document.createElement('DIV');
                div.style.backgroundColor = this.BarColors[barColor];
                div.style.height=parseInt(percentage,10) + '%';
                div.title = bar.Items[ii];
                div.className='IDCGraph_VerticalBar';
                var divSpacer = this.Document.createElement('DIV');
                divSpacer.style.height=(100-parseInt(percentage,10)) + '%';
                td.appendChild(divSpacer);
                td.appendChild(div);
                
                barColor++;
                if(barColor==this.BarColors.length) barColor=0;
            }
            
            tdSpacer = this.Document.createElement('TD');
            tdSpacer.rowSpan = rowspan;
            tdSpacer.className='IDCGraph_VerticalBarSpacer';
            trY.appendChild(tdSpacer);
            
            var tdI = this.Document.createElement('TD');
            var divI =this.Document.createElement('DIV');
            var spanI =this.Document.createElement('SPAN');

            divI.className='IDCGraph_XAxisTextContainer';
            
            tdI.appendChild(divI);
            divI.appendChild(spanI);
            
            spanI.innerHTML = bar.Identifier;
            spanI.className='IDCGraph_XAxisText';
            
            tdI.className='IDCGraph_XAxis';
            tdI.style.width=itemW;
            tdI.colSpan=bar.Items.length+2;
            trX.appendChild(tdI);
        }
        
        tdLegend.colSpan=legendColspan;
        tdTitle.colSpan=legendColspan;
    }
    
};

IDC_Graph.prototype.Add = function(Identifier, Items, SubUrl, GoBackUrl, Title, GoBackText)
{
    this.Bars.push(new IDC_GraphValue(Identifier, Items, SubUrl, GoBackUrl, Title, GoBackText));
};




var IDC_GraphValue = function(Identifier, Items, SubUrl, GoBackUrl, Title, GoBackText)
{
    this.Identifier = Identifier;
    this.Items = Items;
    this.SubUrl = SubUrl;
    this.GoBackUrl = GoBackUrl;
    this.Title = Title;
    this.GoBackText = GoBackText || 'Back';
};

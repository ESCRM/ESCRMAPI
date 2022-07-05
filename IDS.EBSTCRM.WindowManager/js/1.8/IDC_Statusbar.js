var IDC_Statusbar = function (documentObject, parentObject)
{
    this.Document = documentObject;
    this.Parent = parentObject;
    this.Control = this.Document.createElement('DIV');
    this.Control.className='IDC_Statusbar';
    
    this.Parent.appendChild(this.Control);
    this.Labels = new Array();
    
};

IDC_Statusbar.prototype.AddIcon = function(icon, width, height)
{
    var l = this.Document.createElement('SPAN');
    l.style.backgroundImage='url(' + icon + ')';
    l.style.width=width;
    l.style.height=height;
    l.className='IDC_StatusbarLabel_Icon';
    this.Control.appendChild(l);
    this.Labels.push(l);
    return l;
};

IDC_Statusbar.prototype.AddLabel = function(text)
{
    var l = new IDC_Statusbar_Label(this, text);
    this.Labels.push(l);
    return l;
};

IDC_Statusbar.prototype.AddLabelAtRight = function (text) {
    var l = new IDC_Statusbar_Label(this, text);
    l.Control.style = "float: right;padding-right: 15px;";
    this.Labels.push(l);
    return l;
};

IDC_Statusbar.prototype.AddProgressBar = function () {
    var l = new IDC_Statusbar_ProgressBar(this);
    this.Labels.push(l);
    return l;
};

var IDC_Statusbar_Label = function (statusbar, text)
{
    this.Statusbar = statusbar;
    this.Control = statusbar.Document.createElement('SPAN');
    
    this.Control.innerHTML = text;
    
    this.Statusbar.Control.appendChild(this.Control);
    this.Control.className='IDC_StatusbarLabel';
    
    return this;
};

IDC_Statusbar_Label.prototype.SetText = function (Text) {
    this.Control.innerHTML = Text;
};

IDC_Statusbar_Label.prototype.SetNoBorder = function()
{
    this.Control.className='IDC_StatusbarLabel';
};

IDC_Statusbar_Label.prototype.SetSunkenBorder = function()
{
    this.Control.className='IDC_StatusbarLabel_Sunken';
};

IDC_Statusbar_Label.prototype.SetRaisedBorder = function()
{
    this.Control.className='IDC_StatusbarLabel_Raised';
};


var IDC_Statusbar_ProgressBar = function (statusbar) {
    this.Statusbar = statusbar;
    this.Control = statusbar.Document.createElement('SPAN');

    this.Statusbar.Control.appendChild(this.Control);
    this.Control.className = 'IDC_StatusbarLabel_Sunken';
    this.Control.style.width = '150px';

    this.ProgressBar = statusbar.Document.createElement('IMG');
    this.ProgressBar.style.height = '18px';
    this.ProgressBar.style.width = '0%';
    this.ProgressBar.src = '../images/progressbar.gif';
    this.SetProgress(0);
    this.Control.appendChild(this.ProgressBar);

    return this;
};

IDC_Statusbar_ProgressBar.prototype.Hide = function () {
    this.Control.style.display = 'none';
};
IDC_Statusbar_ProgressBar.prototype.Show = function () {
    this.Control.style.display = '';
};

IDC_Statusbar_ProgressBar.prototype.SetProgress = function (value) {

    this.Control.className = 'IDC_StatusbarLabel_Raised';

    var v = parseInt(value, 10);
    if (v < 0) v = 0;
    if (v > 100) v = 100;

    this.ProgressBar.style.width = v + '%';
    this.ProgressBar.style.display = v == 0 ? 'none' : '';
};
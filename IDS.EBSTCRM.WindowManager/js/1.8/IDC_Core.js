var IDC_Core = function (Document, parentObject) {
    this.Document = Document || document;
    this.ParentObject = parentObject || document.body;
    this.isIE = document.all ? true : false;
    this.IsIE = this.isIE;

    this.Mouse = new IDC_Mouse(this.Document, this.ParentObject);
    this.Keyboard = new IDC_Keyboard(this.Document, this.ParentObject);
    this.DOM = new IDC_DOM(this.Document);
    this.Localization = new IDC_Localization();
    this.WebsiteUrl = window.location.origin + window.location.pathname;

    this.WebsiteUrl = this.WebsiteUrl.substring(0, this.WebsiteUrl.lastIndexOf('/') + 1);

    this.InArray = function (Array, value) {
        if (!Array) return false;
        for (var i = 0; i < Array.length; i++) {
            if (value == Array[i]) return true;
        }
        return false;
    };
};

var IDC_Mouse = function(document, parentObject)
{
    this.isIE = document.all?true:false;
    this.Document = document;
    this.ParentObject = parentObject;
    
    this.X=0; 
    this.Y=0;
    
    this.CaptureMouse(parentObject);
};

IDC_Mouse.prototype.CaptureMouse = function (parentObject) {
    if (!this.isIE) {
        document.captureEvents(Event.MOUSEMOVE);
    }
    else {
        parentObject.onselectstart = function () {
            //check if in listview
            var obj = event.srcElement;
            if (obj.tagName == 'DIV' && obj.childNodes.length == 0) {
                while (obj.parentNode != null) {

                    if (obj.Listview != null) {
                        return obj.Listview.AllowTextSelection;
                    }
                    obj = obj.parentNode;
                }
            }
            else if (obj.className == 'avnSQLLabel_Td' || obj.className=='avnSQLLabel_TdHeader' || obj.className == 'avnSQLLabel_Table') return true;

            return event.srcElement.tagName == 'INPUT' || event.srcElement.tagName == 'TEXTAREA';
        };
        parentObject.ondragstart = function () { return false; };
    }

    parentObject.IDCMouse = this;
    parentObject.ommouseover = function (e) {
        if (this.IDCMouse.isIE) {

            this.IDCMouse.X = event.clientX;
            this.IDCMouse.Y = event.clientY;

        } else {
            if (e) {
                this.IDCMouse.X = e.pageX;
                this.IDCMouse.Y = e.pageY;
            }
        }
    };

    parentObject.onmousemove = function (e) {
        if (this.IDCMouse.isIE) {

            this.IDCMouse.X = event.clientX;
            this.IDCMouse.Y = event.clientY;

        } else {
            if (e) {
                this.IDCMouse.X = e.pageX;
                this.IDCMouse.Y = e.pageY;
            }
        }
    };

    parentObject.onmousedown = function (e) {
        if (!e)
            e = window.event;

        if (e) {
            var tg = e.srcElement || e.target;

            //check if in listview
            var obj = tg;
            if (obj.tagName == 'DIV') {
                while (obj.parentNode != null) {

                    if (obj.Listview != null) {
                        return obj.Listview.AllowTextSelection;
                    }
                    obj = obj.parentNode;
                }
            }
            else if (obj.className == 'avnSQLLabel_Td' || obj.className == 'avnSQLLabel_TdHeader' || obj.className == 'avnSQLLabel_Table') return true;

            return tg.tagName == 'INPUT' || tg.tagName == 'TEXTAREA' || tg.tagName == 'SELECT';
        }
        else
            return false;
    };
};

IDC_Mouse.prototype.RefreshMousePosition = function(e)
{
    if (this.isIE) {
        
        this.X = event.clientX;
        this.Y = event.clientY; 
        
    } else { 
        if(e)
        { 
            this.X = e.pageX;
            this.Y = e.pageY;
        }
    } 
};


var IDC_Keyboard = function(Document, parentObject, iframe)
{
    this.KeyMap = ["", "", "", "CANCEL", "", "", "HELP", "", "BACK_SPACE", "TAB", "", "", "CLEAR", "ENTER", "RETURN", "", "SHIFT", "CONTROL", "ALT", "PAUSE", "CAPS_LOCK", "KANA", "EISU", "JUNJA", "FINAL", "HANJA", "", "ESCAPE", "CONVERT", "NONCONVERT", "ACCEPT", "MODECHANGE", "SPACE", "PAGE_UP", "PAGE_DOWN", "END", "HOME", "LEFT", "UP", "RIGHT", "DOWN", "SELECT", "PRINT", "EXECUTE", "PRINTSCREEN", "INSERT", "DELETE", "", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "COLON", "SEMICOLON", "LESS_THAN", "EQUALS", "GREATER_THAN", "QUESTION_MARK", "AT", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "WIN", "", "CONTEXT_MENU", "", "SLEEP", "NUMPAD0", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6", "NUMPAD7", "NUMPAD8", "NUMPAD9", "MULTIPLY", "ADD", "SEPARATOR", "SUBTRACT", "DECIMAL", "DIVIDE", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24", "", "", "", "", "", "", "", "", "NUM_LOCK", "SCROLL_LOCK", "WIN_OEM_FJ_JISHO", "WIN_OEM_FJ_MASSHOU", "WIN_OEM_FJ_TOUROKU", "WIN_OEM_FJ_LOYA", "WIN_OEM_FJ_ROYA", "", "", "", "", "", "", "", "", "", "CIRCUMFLEX", "EXCLAMATION", "DOUBLE_QUOTE", "HASH", "DOLLAR", "PERCENT", "AMPERSAND", "UNDERSCORE", "OPEN_PAREN", "CLOSE_PAREN", "ASTERISK", "PLUS", "PIPE", "HYPHEN_MINUS", "OPEN_CURLY_BRACKET", "CLOSE_CURLY_BRACKET", "TILDE", "", "", "", "", "VOLUME_MUTE", "VOLUME_DOWN", "VOLUME_UP", "", "", "", "", "COMMA", "", "PERIOD", "SLASH", "BACK_QUOTE", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "OPEN_BRACKET", "BACK_SLASH", "CLOSE_BRACKET", "QUOTE", "", "META", "ALTGR", "", "WIN_ICO_HELP", "WIN_ICO_00", "", "WIN_ICO_CLEAR", "", "", "WIN_OEM_RESET", "WIN_OEM_JUMP", "WIN_OEM_PA1", "WIN_OEM_PA2", "WIN_OEM_PA3", "WIN_OEM_WSCTRL", "WIN_OEM_CUSEL", "WIN_OEM_ATTN", "WIN_OEM_FINISH", "WIN_OEM_COPY", "WIN_OEM_AUTO", "WIN_OEM_ENLW", "WIN_OEM_BACKTAB", "ATTN", "CRSEL", "EXSEL", "EREOF", "PLAY", "ZOOM", "", "PA1", "WIN_OEM_CLEAR", ""];

    this.Document = Document || document;
    this.ParentObject = parentObject || this.Document.body;
    
    if(this.Document.IDC_Keyboard) return;
    
    this.isIE = this.Document.all?true:false;
    
    this.KeyControlPressed = false;
    this.KeyAltPressed = false;
    this.KeyShiftPressed = false;
    
    this.KeyUpFunctions = new Array();
    this.KeyDownFunctions = new Array();
    this.KeyPressFunctions = new Array();
    
    this.PrimaryKeyUpFunction = null;
    this.PrimaryKeyDownFunction = null;
    
    this.LastKeyCode = null;
    this.IFrame = iframe;
    this.CaptureKeyboard(this.Document, iframe);
};

IDC_Keyboard.prototype.BindKeyUp = function(KeyIndex, Callback)
{
    this.KeyUpFunctions[KeyIndex] = Callback;
};

IDC_Keyboard.prototype.BindKeyDown = function(KeyIndex, Callback)
{
    this.KeyDownFunctions[KeyIndex] = Callback;
};

IDC_Keyboard.prototype.BindKeyPress = function(KeyIndex, Callback)
{
    this.KeyPressFunctions[KeyIndex] = Callback;
};

IDC_Keyboard.prototype.BindKeyUpToObject = function(Object, KeyIndex, Callback)
{
    if(!Object.KeyUpCallBackEvents) Object.KeyUpCallBackEvents = new Array();
    Object.KeyUpCallBackEvents[KeyIndex] = Callback;
    
    if(this.isIE)
    {
        Object.onkeyup = function() { if(this.KeyUpCallBackEvents[event.keyCode]) return this.KeyUpCallBackEvents[event.keyCode](event); };
    }
    else
    {
        Object.onkeyup = function(e) { if(this.KeyUpCallBackEvents[e.which]) return this.KeyUpCallBackEvents[e.which](e); };
    }
};

IDC_Keyboard.prototype.BindKeyDownToObject = function(Object, KeyIndex, Callback)
{
    if(!Object.KeyDownCallBackEvents) Object.KeyDownCallBackEvents = new Array();
    Object.KeyDownCallBackEvents[KeyIndex] = Callback;   
    var iframe = this.IFrame;
    if(this.isIE)
        Object.onkeydown = function(e) { var keynum=e?e.which:iframe?iframe.event.keyCode:event.keyCode; if(this.KeyDownCallBackEvents[keynum]) return this.KeyDownCallBackEvents[keynum](e||iframe?iframe.event:event); };
    else
        Object.onkeydown = function(e) { var keynum=e.which; if(this.KeyDownCallBackEvents[keynum]) return this.KeyDownCallBackEvents[keynum](e); };
};

IDC_Keyboard.prototype.BindKeyPressToObject = function(Object, KeyIndex, Callback)
{
    if(!Object.KeyPressCallBackEvents) Object.KeyPressCallBackEvents = new Array();
    Object.KeyPressCallBackEvents[KeyIndex] = Callback;
    if(this.isIE)
    {
        Object.onkeypress = function() { if(this.KeyPressCallBackEvents[event.keyCode]) return this.KeyPressCallBackEvents[event.keyCode](event);  };
    }
    else
    {
        Object.onkeypress = function(e) { if(this.KeyPressCallBackEvents[e.which]) return this.KeyPressCallBackEvents[e.which](e); };
    }
};

IDC_Keyboard.prototype.CaptureKeyboard = function(parentObject, iframe)
{
    parentObject.IDC_Keyboard = this;
    parentObject.onkeydown = function(e) { return this.IDC_Keyboard.KeyDownEvent(e?e:iframe?iframe.event:event); };
    parentObject.onkeyup = function(e) { return this.IDC_Keyboard.KeyUpEvent(e?e:iframe?iframe.event:event); };

};

IDC_Keyboard.prototype.KeyDownEvent = function (e) {

    if (e) {
        var o = this.frameElement ? this.frameElement.IDCWindow.Keyboard : this;

        var keynum = e.which ? e.which : e.keyCode;
        var target = e.target ? e.target : e.srcElement;

        o.KeyAltPressed = e.altKey;
        o.KeyControlPressed = e.ctrlKey;
        o.KeyShiftPressed = e.shiftKey;

        o.LastKeyCode = keynum;
        var retval = null;
        if (o.PrimaryKeyDownFunction)
            retval = o.PrimaryKeyDownFunction(keynum, target, e);

        //Raise to window manager test
        if (window.frameElement && window.frameElement.IDCWindow) {
            if (window.frameElement.IDCWindow.WindowManager.Core.Keyboard.PrimaryKeyDownFunction)
                window.frameElement.IDCWindow.WindowManager.Core.Keyboard.PrimaryKeyDownFunction(keynum, o);
        }
    }

    return retval;
};
IDC_Keyboard.prototype.KeyUpEvent = function (e) {
    if (e) {
        var o = this.frameElement ? this.frameElement.IDCWindow.Keyboard : this;

        var keynum = e.which ? e.which : e.keyCode;
        var target = e.target ? e.target : e.srcElement;

        o.KeyAltPressed = e.altKey;
        o.KeyControlPressed = e.ctrlKey;
        o.KeyShiftPressed = e.shiftKey;

        o.LastKeyCode = keynum;
        var retval = null;
        if (o.PrimaryKeyUpFunction)
            retval = o.PrimaryKeyUpFunction(keynum, target, e);

        //Raise to window manager test
        if (window.frameElement && window.frameElement.IDCWindow) {
            if (window.frameElement.IDCWindow.WindowManager.Core.Keyboard.PrimaryKeyUpFunction)
                window.frameElement.IDCWindow.WindowManager.Core.Keyboard.PrimaryKeyUpFunction(keynum, o);
        }
    }
    return retval;
};


IDC_Core.prototype.Ajax = function(url) {
	this.xmlhttp = null;

    this.Dispose = function() {
  		this.OnLoading = null;
  		this.OnLoaded = null;
  		this.OnInteractive = null;
  		this.OnCompletion = null;
  		this.OnError = null;
		this.OnFail = null;
		
		this.method = null;
  		this.queryStringSeparator = null;
		this.argumentSeparator = null;
		this.URLString = null;
		this.encodeURIString = null;
  		this.execute = null;
  		this.element = null;
		this.elementObj = null;
		this.requestFile = null;
		this.vars.length=0;
		this.vars =null;
		this.ResponseStatus =null;
		this.xmlhttp = null;
		this.Dispose = null;
    };

	this.ResetData = function() {
		this.method = "POST";
  		this.queryStringSeparator = "?";
		this.argumentSeparator = "&";
		this.URLString = "";
		this.encodeURIString = true;
  		this.execute = false;
  		this.element = null;
		this.elementObj = null;
		this.requestFile = url;
		this.vars = new Object();
		this.ResponseStatus = new Array(2);
  	};

	this.ResetFunctions = function() {
  		this.OnLoading = function() { };
  		this.OnLoaded = function() { };
  		this.OnInteractive = function() { };
  		this.OnCompletion = function() { };
  		this.OnError = function() { };
		this.OnFail = function() { };
	};

	this.Reset = function() {
		this.ResetFunctions();
		this.ResetData();
	};

	this.CreateAJAX = function() {
		try {
			this.xmlhttp = new ActiveXObject("Msxml2.XMLHTTP");
		} catch (e1) {
			try {
				this.xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
			} catch (e2) {
				this.xmlhttp = null;
			}
		}

		if (! this.xmlhttp) {
			if (typeof XMLHttpRequest != "undefined") {
				this.xmlhttp = new XMLHttpRequest();
			} else {
				this.failed = true;
			}
		}
	};

	this.setVar = function(name, value){
		this.vars[name] = Array(value, false);
	};

	this.encVar = function(name, value, returnvars) {
		if (true == returnvars) {
			return Array(encodeURIComponent(name), encodeURIComponent(value));
		} else {
			this.vars[encodeURIComponent(name)] = Array(encodeURIComponent(value), true);
		}
	};

	this.processURLString = function(string, encode) {
		encoded = encodeURIComponent(this.argumentSeparator);
		regexp = new RegExp(this.argumentSeparator + "|" + encoded);
		varArray = string.split(regexp);
		for (i = 0; i < varArray.length; i++){
			urlVars = varArray[i].split("=");
			if (true == encode){
				this.encVar(urlVars[0], urlVars[1]);
			} else {
				this.setVar(urlVars[0], urlVars[1]);
			}
		}
	};

	this.createURLString = function(urlstring) {
		if (this.encodeURIString && this.URLString.length) {
			this.processURLString(this.URLString, true);
		}

		if (urlstring) {
			if (this.URLString.length) {
				this.URLString += this.argumentSeparator + urlstring;
			} else {
				this.URLString = urlstring;
			}
		}

		// prevents caching of URLString
		this.setVar("rndval", new Date().getTime());

		urlstringtemp = new Array();
		for (key in this.vars) {
			if (false == this.vars[key][1] && true == this.encodeURIString) {
				encoded = this.encVar(key, this.vars[key][0], true);
				delete this.vars[key];
				this.vars[encoded[0]] = Array(encoded[1], true);
				key = encoded[0];
			}

			urlstringtemp[urlstringtemp.length] = key + "=" + this.vars[key][0];
		}
		if (urlstring){
			this.URLString += this.argumentSeparator + urlstringtemp.join(this.argumentSeparator);
		} else {
			this.URLString += urlstringtemp.join(this.argumentSeparator);
		}
	};

	this.runResponse = function() {
		eval(this.Response);
	};

    this.RunAJAX = function (urlstring) {
		if (this.failed) {
			this.OnFail();
		} else {
			this.createURLString(urlstring);
			if (this.element) {
				this.elementObj = document.getElementById(this.element);
			}
			if (this.xmlhttp) {
				try
				{
				    var self = this;
				    if (this.method == "GET") {
					    totalurlstring = this.requestFile + this.queryStringSeparator + this.URLString;
					    this.xmlhttp.open(this.method, totalurlstring, true);
				    } else {
					    this.xmlhttp.open(this.method, this.requestFile, true);
					    try {
						    this.xmlhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded")
					    } catch (e) { }
				    }

				    this.xmlhttp.onreadystatechange = function() {
					    switch (self.xmlhttp.readyState) {
						    case 1:
							    self.OnLoading();
							    break;
						    case 2:
							    self.OnLoaded();
							    break;
						    case 3:
						        var tempText = '';
						        try
						        {
						            tempText = self.xmlhttp.responseText;
						        }
						        catch(exp)
						        { tempText = ''; }
    						    
						        self.Response = tempText;
							    self.OnInteractive();
							    break;
						    case 4:
							    self.Response = self.xmlhttp.responseText;
							    self.ResponseXML = self.xmlhttp.responseXML;
							    self.ResponseStatus[0] = self.xmlhttp.status;							
							    self.ResponseStatus[1] = self.xmlhttp.statusText;

							    if (self.execute) {
								    if(self.runResponse) self.runResponse();
							    }

							    if (self.elementObj) {
								    elemNodeName = self.elementObj.nodeName;
								    elemNodeName.toLowerCase();
								    if (elemNodeName == "input"
								    || elemNodeName == "select"
								    || elemNodeName == "option"
								    || elemNodeName == "textarea") {
									    self.elementObj.value = self.Response;
								    } else {
									    self.elementObj.innerHTML = self.Response;
								    }
							    }
							    try
							    {
							        if (self.ResponseStatus[0] == "200" || self.ResponseStatus[0] == "0") {
								        if(self.OnCompletion) self.OnCompletion();
							        } else {
								        if(self.OnError) self.OnError();
							        }
							    }
							    catch(exp) {}

							    self.URLString = "";
							    break;
					    }
				    };

				    this.xmlhttp.send(this.URLString);
				}
				catch(exp) {}
			}
		}
	};

	this.Reset();
	this.CreateAJAX();
};

var IDC_DOM = function(document)
{
    
};

IDC_DOM.prototype.HighlightText = function(bodyText, searchTerm, color, backgroundColor)
{
    // the highlightStartTag and highlightEndTag parameters are optional
    if(!color) color='blue';
    if(!backgroundColor) backgroundColor='yellow';
    
    var highlightStartTag = '<font style="color:' + color + '; background-color:' + backgroundColor + ';">';
    var highlightEndTag = '</font>';

    // find all occurences of the search term in the given text,
    // and add some "highlight" tags to them (we're not using a
    // regular expression search, because we want to filter out
    // matches that occur within HTML tags and script blocks, so
    // we have to do a little extra validation)
    var newText = "";
    var i = -1;
    var lcSearchTerm = searchTerm.toLowerCase();    
    var lcBodyText = bodyText.toLowerCase();

    while (bodyText.length > 0) {
    i = lcBodyText.indexOf(lcSearchTerm, i+1);
    if (i < 0) {
      newText += bodyText;
      bodyText = "";
    } else {
      // skip anything inside an HTML tag
      if (bodyText.lastIndexOf(">", i) >= bodyText.lastIndexOf("<", i)) {
        // skip anything inside a <script> block
        if (lcBodyText.lastIndexOf("/script>", i) >= lcBodyText.lastIndexOf("<script", i)) {
          newText += bodyText.substring(0, i) + highlightStartTag + bodyText.substr(i, searchTerm.length) + highlightEndTag;
          bodyText = bodyText.substr(i + searchTerm.length);
          lcBodyText = bodyText.toLowerCase();
          i = -1;
        }
      }
    }
    }

    return newText;
};

IDC_DOM.prototype.Purge = function(obj)
{    
    try
    {
        var a = obj.attributes, i, l, n;    
        if (a) 
        {
            l = a.length;
            for (i = 0; i < l; i += 1) 
            {
                n = a[i].name;
                if (typeof obj[n] === 'function')
                {
                    delete obj[n];
                    obj[n] = null;
                }
            }    
        }

        a = obj.childNodes;    
        if (a)
        {
            l = a.length;
            for (i = 0; i < l; i += 1)
            {
                this.Purge(obj.childNodes[i]);
            }    
        }
    }
    catch(ex)
    { }
};

IDC_DOM.prototype.GetElementsByTagNames = function(arrayOfTagNames, parentObject)
{
    var obj = parentObject || document;
    var tagNames = arrayOfTagNames;
    var resultArray = new Array();
    for (var i=0;i<tagNames.length;i++)
    {
            var tags = obj.getElementsByTagName(tagNames[i]);
            for (var j=0;j<tags.length;j++)
            {
                    resultArray.push(tags[j]);
            }
    }
    var testNode = resultArray[0];
    if (testNode.sourceIndex)
    {
            resultArray.sort(function (a,b) {
                            //if(a.tabIndex && b.tabIndex)
                                return (a.tabIndex || a.sourceIndex)- (b.tabIndex || b.sourceIndex);
//                            else if(a.tabIndex)
//                                return a.tabIndex - b.sourceIndex;
//                            else
//                                return a.sourceIndex - b.sourceIndex;
            });
    }
    else if (testNode.compareDocumentPosition)
    {
            resultArray.sort(function (a,b) {
                            return 3 - (a.compareDocumentPosition(b) & 6);
            });
    }
    return resultArray;

};
IDC_DOM.prototype.GetObjectPosition = function(obj)
{
    var x=0;
    var y=0;
    
    if (obj.offsetParent)
    {
        do
        {
			x += obj.offsetLeft;
			y += obj.offsetTop;
        } 
        while (obj = obj.offsetParent);
    }
    
    var retval=new Array();
    
    retval[0] = x;
    retval[1] = y;
    
    return retval;
};

IDC_DOM.prototype.DisableFields = function(obj)
{
    this._DisableFields(obj, 'INPUT');
    this._DisableFields(obj, 'SELECT');
    this._DisableFields(obj, 'TEXTAREA');
};

IDC_DOM.prototype._DisableFields = function(obj, type)
{
    var arr = obj.getElementsByTagName(type);
    for(var i=0;i<arr.length;i++)
    {
        arr[i].setAttribute('OrgState',arr[i].disabled ? '1' : '0');
        arr[i].disabled = true;
    }
};

IDC_DOM.prototype.EnableFields = function(obj)
{
    this._EnableFields(obj, 'INPUT');
    this._EnableFields(obj, 'SELECT');
    this._EnableFields(obj, 'TEXTAREA');
};

IDC_DOM.prototype._EnableFields = function(obj, type)
{
    var arr = obj.getElementsByTagName(type);
    
    for(var i=0;i<arr.length;i++)
    {
        arr[i].disabled = arr[i].getAttribute('OrgState')=='1';
    }
};


var IDC_Localization = function()
{
    this.DateAdd = IDC_DateAdd;
    this.ParseDateFormat = function(value, mask) {return date.parse(value, mask); };
    this.DateFormat = function(date, mask) { return date.format(mask); };
    
    /*this.DateToSortableString = function(date)
        {
            var y = date.getFullYear().toString();
            var m = (date.getMonth()+1).toString();
            var d = date.getDate().toString();
            var h = date.getHours().toString();
            var mm =date.getMinutes().toString();
            var s = date.getSeconds().toString();
            var mmm=date.getMilliseconds().toString();
            
            if(m.length<2) m='0' + m;
            if(d.length<2) d='0' + d;
            if(h.length<2) h='0' + h;
            if(mm.length<2) mm='0' + mm;
            if(s.length<2) s='0' + s;
            
            while(mmm.length<4)
                mmm='0' + mmm;
                
            //return y+'-'+m+'-'+d+'-'+h+':'+mm+':'+s+':'+mmm;
            return y+'-'+m+'-'+d+'-'+h+':'+mm+':'+s+':'+mmm;
        };*/
};

var IDC_LocalizationSettings = function(decimalPoint, thousandSep, fracDigits)
{
    this.decimalPoint = new String(decimalPoint);
    this.thousandSep = new String(thousandSep);
    this.fracDigits = fracDigits;
};

IDC_Localization.prototype.RoundFloat = function(num, fracDigits)
{
    var factor = Math.pow(10, fracDigits);
    return (Math.round(num*factor)/factor);
};

IDC_Localization.prototype.ToLcString = function(num, lc)
{
    var str = new String(num);
    var aParts = str.split(".");
    return (aParts.join(lc.decimalPoint));
};

IDC_Localization.prototype.FormatNum = function(num, lc)
{
    var sNum = new String(this.RoundFloat(num, lc.fracDigits));

    if(lc.fracDigits>0)
    {
        if(sNum.indexOf(".")<0)
            sNum = sNum+".";
            
        while(sNum.length < 1+sNum.indexOf(".")+lc.fracDigits)
            sNum = sNum+"0";
    }
    
    return(this.ToLcString(sNum, lc));
};
  
IDC_Localization.prototype.ParseLcNum = function(str, lc)
{
    var sNum = new String(str);
    var aParts = sNum.split(lc.thousandSep);
    sNum = aParts.join("");
    aParts = sNum.split(lc.decimalPoint);
    return(parseFloat(aParts.join(".")));
};



Date.is = function(Ob) {
	try {
		if ( typeof Ob == "object" ) {
			if ( Ob.constructor == Date ) {
				return true;
			};
		};
	} catch (CurError) { };
	return false;
};


	// parseFormat
	// Accepts a date/time format and a string and returns either a date (if the format matches) or null
var IDC_DateAdd = function (DateObj, Amount, DatePart, Destructive) {

    DatePart = DatePart.toLowerCase();

    var ReturnDate = new Date(DateObj);
    var CurAbsAmount = Math.abs(Amount);

    // Set the Multiplication factors for unambigiuos times (MS times these will result in the appropriate data part)
    var Factors = new Object();
    Factors.milliseconds = 1; // 1 ms to the ms (1 * 1000)
    Factors.seconds = 1000; 	// 1000 ms to the second (1 * 1000)
    Factors.minutes = 60000; // 60 seconds to the minute (1 * 1000 * 60)
    Factors.quarterhours = 900000; // 15 minutes to the quarter hour (1 * 1000 * 60 * 15)
    Factors.warhols = 900000; // 15 minutes of fame (1 * 1000 * 60 * 15)
    Factors.halfhours = 1800000; // 30 minutes to the half hour (1 * 1000 * 60 * 15)
    Factors.hours = 3600000; // 60 minutes to the hour (1 * 1000 * 60 * 60)
    Factors.days = 86400000; // 24 hours to the day (1 * 1000 * 60 * 60 * 24)
    Factors.weeks = 604800000; // 7 days per week (1 * 1000 * 60 * 60 * 24 * 7)

    // Do the math
    switch (DatePart) {
        // The following are all unambigously convertable to ms equivalents 
        case "milliseconds":
        case "seconds":
        case "minutes":
        case "quarterhours":
        case "warhols":
        case "halfhours":
        case "hours":
        case "days":
        case "weeks":
            ReturnDate = new Date(DateObj.getTime() + (Amount * Factors[DatePart]));
            break;
        case "businessdays":
            if (CurAbsAmount > 5) {
                var CurWeeks = Math.floor(CurAbsAmount / 5);
                var CurDays = CurAbsAmount % 5;
                if (Amount < 0) {
                    CurWeeks = -CurWeeks;
                    CurDays = -CurDays;
                };
            } else {
                var CurWeeks = 0;
                var CurDays = Amount;
            };
            // Add the number of weeks to the date
            ReturnDate = IDC_DateAdd(ReturnDate, CurWeeks, "weeks");
            // Now add the days
            ReturnDate = IDC_DateAdd(ReturnDate, CurDays, "days");
            // If we've landed on a weekend push us
            if (ReturnDate.getDay() == 0) {
                if (Amount < 0) {
                    ReturnDate = IDC_DateAdd(ReturnDate, -2, "days");
                } else {
                    ReturnDate = IDC_DateAdd(ReturnDate, 1, "days");
                };
            };
            if (ReturnDate.getDay() == 6) {
                if (Amount < 0) {
                    ReturnDate = IDC_DateAdd(ReturnDate, -1, "days");
                } else {
                    ReturnDate = IDC_DateAdd(ReturnDate, 2, "days");
                };
            };
            break;
        case "businessweeks":
            ReturnDate = IDC_DateAdd(ReturnDate, Amount * 5, "businessdays");
            break;
        case "wholeweeks":
            // Move to the nearest Sunday
            if (Amount < 0) {
                ReturnDate = IDC_DateAdd(ReturnDate, -(ReturnDate.getDay()), "days");
            } else {
                ReturnDate = IDC_DateAdd(ReturnDate, ReturnDate.getDay() + (6 - ReturnDate.getDay()), "days");
            };
            // Now add the weeks
            ReturnDate = IDC_DateAdd(ReturnDate, Amount, "weeks");
            break;
        case "months":
            // Months are tricky - they have different number of days
            // First split the amount into the number of years and months
            if (CurAbsAmount > 11) {
                var CurYears = Math.floor(CurAbsAmount / 12);
                var CurMonths = CurAbsAmount % 12;
                if (Amount < 0) {
                    CurYears = -CurYears;
                    CurMonths = -CurMonths;
                };
            } else {
                var CurYears = 0;
                var CurMonths = Amount;
            };
            // Add the number of years to the date
            ReturnDate = IDC_DateAdd(ReturnDate, CurYears, "years");
            // Now add the months
            var TempReturnDate = new Date(ReturnDate);
            TempReturnDate.setDate(1);
            TempReturnDate = new Date(new Date(TempReturnDate).setMonth(TempReturnDate.getMonth() + CurMonths));
            ReturnDate = new Date(new Date(ReturnDate).setMonth(ReturnDate.getMonth() + CurMonths));
            // Determine if the months got thrown off (due to too many days in the current month compared to the target)
//            if (ReturnDate.getMonth() != TempReturnDate.getMonth()) {
//                // Set the date to the last day of the previous month
//                alert('month off');
//                ReturnDate.setDate(0)
            //            };

            if (ReturnDate.getMonth() > TempReturnDate.getMonth()) {
                while (ReturnDate.getMonth() > TempReturnDate.getMonth()) {
                    ReturnDate.setDate(ReturnDate.getDate() - 1);
                }
            }
            if (ReturnDate.getMonth() < TempReturnDate.getMonth()) {
                while (ReturnDate.getMonth() < TempReturnDate.getMonth()) {
                    ReturnDate.setDate(ReturnDate.getDate() + 1);
                }
            }

            break;
        case "years":
            // February 29th may cause problems
            var Feb29 = false;
            if (ReturnDate.getMonth() == 1 && ReturnDate.getDate() == 29) {
                Feb29 = true;
            };
            // Add Years directly as a data part
            ReturnDate = new Date(new Date(DateObj).setFullYear(DateObj.getFullYear() + Amount));
            // If Feb29th then check to ensure that the date hasn't changed the month
            if (Feb29) {
                if (ReturnDate.getMonth != 1) {
                    ReturnDate.setDate(0);
                };
            };
            break;
    };

    // Return the time
    if (!Destructive) {
        return ReturnDate;
    } else {
        DateObj.setTime(ReturnDate.getTime());
        return DateObj;
    };

};

Date.prototype.add = function(Amount, DatePart, Destructive)
{
    return IDC_DateAdd(this, Amount, DatePart, Destructive);
};

(
    function (d, dp) 
    {
        d.i18n = function (l) {
          return (typeof l == 'string')
               ? (l in Date.i18n ? Date.i18n[l] : Date.i18n(l.substr(0, l.lastIndexOf('-'))))
               : (l || Date.i18n(navigator.language || navigator.browserLanguage || ''));
        };
        
        d.i18n.inherit = function (l, o) {
          l = Date.i18n(l);
          for (var k in l) if (typeof o[k] == 'undefined') o[k] = l[k];
          return o;
        };
        d.i18n[''] = // default
        d.i18n['en'] = 
        d.i18n['en-US'] = {
        months: {
            abbr: [ 'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec' ],
            full: [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December' ]
          },
          days: {
            abbr: [ 'Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat' ],
            full: [ 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday' ]
          },
          week: {   // Used by date pickers
            abbr: 'Wk',
            full: 'Week'
          },
          ad: 'AD',
          am: 'AM',
          pm: 'PM',
          gmt: 'GMT',
          z: ':',   // Hour - minute separator
          Z: '',    // Hour - minute separator
          fdow: 0,  // First day of week
          mdifw: 1  // Minimum days in first week
        };
        d.i18n['iso'] = d.i18n.inherit('en', {
        Z: ':',
        fdow: 1,
        mdifw: 4
    }
);

d.WEEK = 6048e5;
d.DAY = 864e5;
d.HOUR = 36e5;
d.MINUTE = 6e4
d.SECOND = 1000;
d.today = function () {
  return new Date().datePart();
};
dp.clone = function() {
  return new Date(+this);
};
dp.datePart = function () {
  with (this) return new Date(getFullYear(), getMonth(), getDate());
};

dp.timePart = function () {
  with (this) return new Date(1970, 0, 1, getHours(), getMinutes(), getSeconds(), getMilliseconds());
};

dp.setDay = function (d) {
  with (this) setDate((getDate() - getDay()) + d);
};

dp.getDayOfWeek = function (o) {
  if (typeof o != 'number') o = Date.i18n(o).fdow;
  var d = this.getDay() - o;
  if (d < 0) d += 7;
  return d + 1;
};

dp.setDayOfWeek = function (d, o) {
  with (this) setDate((getDate() - getDayOfWeek(o)) + d);
};

dp.getDaysInMonth = function () {
  with (this.clone()) {
    setDate(32);
    return 32 - getDate();
  }
};

dp.getDaysInYear = function () {
  var y = this.getFullYear();
  return Math.floor((Date.UTC(y+1, 0, 1) - Date.UTC(y, 0, 1)) / Date.DAY);
};

dp.getDayOfYear = function () {
  return Math.floor((this - new Date(this.getFullYear(), 0, 1)) / Date.DAY) + 1;
};

dp.setDayOfYear = function (d) {
  this.setMonth(0, d); 
};

dp.getWeekOfMonth = function (l) {
  l = Date.i18n(l);
  with (this.clone()) {
    setDate(1);
    var d = (7 - (getDay() - l.fdow)) % 7;
    d = (d < l.mdifw) ? -d : (7 - d);
    return Math.ceil((this.getDate() + d) / 7);
  }
};

dp.setWeekOfMonth = function (w, l) {
  l = Date.i18n(l);
  with (this.clone()) {
    setDate(1);
    var d = (7 - (getDay() - l.fdow)) % 7;
    d = (d < l.mdifw) ? -d : (7 - d);
    setDate(d);
  }
};

dp.getWeekOfYear = function (l) {
  l = Date.i18n(l);
  with (this.clone()) {
    setMonth(0, 1);
    var d = (7 - (getDay() - l.fdow)) % 7;
    if (l.mdifw < d) d -= 7;
    setDate(d);
    var w = Math.ceil((+this - valueOf()) / Date.WEEK);
    return (w <= getWeeksInYear()) ? w : 1;
  }
};

dp.setWeekOfYear = function (w, l) {
  l = Date.i18n(l);
  with (this) {
    setMonth(0, 1);
    var d = (7 - (getDay() - l.fdow)) % 7;
    if (l.mdifw < d) d -= 7;
    d += w * 7;
    setDate(d);
  }
};

dp.getWeeksInYear = function () {
  var y = this.getFullYear();
  return 52 + (new Date(y, 0, 1).getDay() == 4 || new Date(y, 11, 31).getDay() == 4);
};

dp.setTimezoneOffset = function (o) {
  with (this) setTime(valueOf() + ((getTimezoneOffset() + -o) * Date.MINUTE));
};

dp.format = function (p, l) {
  var i18n = Date.i18n(l);
  var d = this;
  var pad = function (n, l) {
    for (n = String(n), l -= n.length; --l >= 0; n = '0'+n);
    return n;
  };
  var tz = function (n, s) {
    return ((n<0)?'+':'-')+pad(Math.abs(n/60),2)+s+pad(Math.abs(n%60),2);
  };
  return p.replace(/([aDdEFGHhKkMmSsWwyZz])\1*|'[^']*'|"[^"]*"/g, function (m) {
    l = m.length;
    switch (m.charAt(0)) {
      case 'a': return (d.getHours() < 12) ? i18n.am : i18n.pm;
      case 'D': return pad(d.getDayOfYear(), l);
      case 'd': return pad(d.getDate(), l);
      case 'E': return i18n.days[(l > 3)?'full':'abbr'][d.getDay()];
      case 'F': return pad(d.getDayOfWeek(i18n), l);
      case 'G': return i18n.ad;
      case 'H': return pad(d.getHours(), l);
      case 'h': return pad(d.getHours() % 12 || 12, l);
      case 'K': return pad(d.getHours() % 12, l);
      case 'k': return pad(d.getHours() || 24, l);
      case 'M': return (l < 3) 
                     ? pad(d.getMonth() + 1, l)
                     : i18n.months[(l > 3)?'full':'abbr'][d.getMonth()];
      case 'm': return pad(d.getMinutes(), l);
      case 'S': return pad(d.getMilliseconds(), l);
      case 's': return pad(d.getSeconds(), l);
      case 'W': return pad(d.getWeekOfMonth(i18n), l);
      case 'w': return pad(d.getWeekOfYear(i18n), l);
      case 'y': return (l == 2) 
                     ? String(d.getFullYear()).substr(2)
                     : pad(d.getFullYear(), l);
      case 'Z': return tz(d.getTimezoneOffset(), i18n.Z);
      case 'z': return i18n.gmt+tz(d.getTimezoneOffset(), i18n.z);
      case "'":
      case '"': return m.substr(1, l - 2);
      default:  throw new Error('Illegal pattern');
    }
  });
}

d.parse = function (s, p, l) {
  if (!p) return arguments.callee.original.call(this);
  var i18n = Date.i18n(l), d = new Date(1970,0,1,0,0,0,0);
  var pi = 0, si = 0, i, j, k, c;
  var num = function (x) {
    if (x) l = x;
    else if (!/[DdFHhKkMmSsWwy]/.test(p.charAt(pi))) l = Number.MAX_VALUE;
    for (i = si; --l >= 0 && /[0-9]/.test(s.charAt(si)); si++);
    if (i == si) throw 1;
    return parseInt(s.substring(i, si), 10);
  };
  var cmp = function (x) {
    if (s.substr(si, x.length).toLowerCase() != x.toLowerCase()) return false;
    si += x.length;
    return true;
  };
  var idx = function (x) {
    for (i = x.length; --i >= 0;) if (cmp(x[i])) return i+1;
    return 0;
  };
  try {
    while (pi < p.length) {
      c = p.charAt(l = pi);
      if (/[aDdEFGHhKkMmSsWwyZz]/.test(c)) {
        while (p.charAt(++pi) == c);
        l = pi - l;
        switch (c) {
          case 'a': if (cmp(i18n.pm)) d.setHours(12 + d.getHours());
                    else if (!cmp(i18n.am)) throw 2;
                    break;
          case 'D': d.setDayOfYear(num()); break;
          case 'd': d.setDate(num()); break;
          case 'E': if (i = idx(i18n.days.full)) d.setDay(i - 1);
                    else if (i = idx(i18n.days.abbr)) d.setDay(i - 1);
                    else throw 3;
                    break;
          case 'F': d.setDayOfWeek(num(), i18n); break;
          case 'G': if (!cmp(i18n.ad)) throw 4;
                    break;
          case 'H': 
          case 'k': d.setHours((i = num()) < 24 ? i : 0); break;
          case 'K':
          case 'h': d.setHours((i = num()) < 12 ? i : 0); break;
          case 'M': if (l < 3) d.setMonth(num() - 1); 
                    else if (i = idx(i18n.months.full)) d.setMonth(i - 1);
                    else if (i = idx(i18n.months.abbr)) d.setMonth(i - 1);
                    else throw 5;
                    break;
          case 'm': d.setMinutes(num()); break;
          case 'S': d.setMilliseconds(num()); break;
          case 's': d.setSeconds(num()); break;
          case 'W': d.setWeekOfMonth(num(), i18n); break;
          case 'w': d.setWeekOfYear(num(), i18n); break;
          case 'y': d.setFullYear((l == 2) ? 2000 + num() : num()); break;
          case 'z': if (!cmp(i18n.gmt)) throw 6;
          case 'Z': if (!/[+-]/.test(j = s.charAt(si++))) throw 6;
                    k = num(2) * 60;
                    if (!cmp(i18n[c])) throw 7;
                    k += num(2);
                    d.setTimezoneOffset((j == '+') ? -k : k);
        }
      }
      else if (/["']/.test(c)) {
        while (++pi < p.length && p.charAt(pi) != c);
        if (!cmp(p.substring(l+1, pi++))) throw 8;
      }
      else {
        while (pi < p.length && !/[aDdEFGHhKkMmSsWwyZz"']/.test(p.charAt(pi))) pi++;
        if (!cmp(p.substring(l, pi))) throw 9;
      }
    }
    return d;
  }
  catch (e) {
    if (e > 0) return Number.NaN;
    throw e;      
  }
};
d.parse.original = d.parse;

})(Date, Date.prototype);
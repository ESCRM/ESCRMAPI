var floatFormat = null;

function RequeredFieldsBuild() {
    _RequeredFieldsBuild(document.getElementsByTagName('INPUT'));
    _RequeredFieldsBuild(document.getElementsByTagName('TEXTAREA'));
    _RequeredFieldsBuild(document.getElementsByTagName('SELECT'));

    floatFormat = new IDC_LocalizationSettings(',', '.', 2);

    //custom autofills



};

function AutolookUpDataGetTable(obj) {
    try {

        if (obj != null) {
            while (obj.tagName != 'TABLE') {
                if (obj.parentNode == null) return null;
                obj = obj.parentNode;
            }
            return obj;

        }
        else
            return null;

    }
    catch (err) {

    }

    return null;
};

function AutoLookUpData(sender) {

    var op = null;
    var getTable = false;
    try {
        if (!sender.getAttribute('fieldname') && !sender.getAttribute('FieldName') && !sender.getAttribute('dbName')) {
            op = sender.parentNode.parentNode.parentNode.parentNode;
            getTable = true;
        }
        else {
            op = sender;
        }
    }
    catch (err) {
        op = sender;
    }
    if (!op) op = sender;

    var otherFields = new Array();

    var otherFieldsSource = document.getElementsByTagName('INPUT');
    for (var i = 0; i < otherFieldsSource.length; i++) {
        otherFields.push(getTable ? AutolookUpDataGetTable(otherFieldsSource[i]) : otherFieldsSource[i]);
    }

    otherFieldsSource = document.getElementsByTagName('SELECT');
    for (var i = 0; i < otherFieldsSource.length; i++) {
        otherFields.push(getTable ? AutolookUpDataGetTable(otherFieldsSource[i]) : otherFieldsSource[i]);
    }

    otherFieldsSource = document.getElementsByTagName('TEXTAREA');
    for (var i = 0; i < otherFieldsSource.length; i++) {
        otherFields.push(getTable ? AutolookUpDataGetTable(otherFieldsSource[i]) : otherFieldsSource[i]);
    }


    if (op.getAttribute('DataLink') == 'NNECompanyZipcode') {
        var corrObj = null;
        var corrObjB = null;

        for (var i = 0; i < otherFields.length; i++) {



            if (otherFields[i] != null) {

                if (otherFields[i].getAttribute('DataLink') == 'NNECompanyCity') {


                    corrObj = otherFields[i];
                    if (corrObj.tagName == 'TABLE') {
                        corrObj = corrObj.getElementsByTagName('INPUT')[0];
                    }

                    var AjaxObject = new core.Ajax();
                    AjaxObject.requestFile = 'ajax/get_city.aspx';
                    AjaxObject.encVar('zipcode', sender.value);

                    AjaxObject.OnCompletion = function () { _AutoLookUpData(corrObj, AjaxObject); };
                    AjaxObject.OnError = function () { _AutoLookUpData(corrObj, AjaxObject); };
                    AjaxObject.OnFail = function () { _AutoLookUpData(corrObj, AjaxObject); };
                    AjaxObject.RunAJAX();
                }

                if (otherFields[i].getAttribute('DataLink') == 'Counties') {
                    corrObjB = otherFields[i];
                    if (corrObjB.tagName == 'TABLE') {
                        corrObjB = corrObjB.getElementsByTagName('SELECT')[0];
                    }

                    if (corrObjB.selectedIndex == 0) {
                        var AjaxObjectB = new core.Ajax();
                        AjaxObjectB.requestFile = 'ajax/get_county.aspx';
                        AjaxObjectB.encVar('zipcode', sender.value);

                        AjaxObjectB.OnCompletion = function () { _AutoLookUpDataMultiple(corrObjB, AjaxObjectB); };
                        AjaxObjectB.OnError = function () { _AutoLookUpDataMultiple(corrObjB, AjaxObjectB); };
                        AjaxObjectB.OnFail = function () { _AutoLookUpDataMultiple(corrObjB, AjaxObjectB); };
                        AjaxObjectB.RunAJAX();
                    }
                }
            }
            //if(corrObj || corrObjB) break;
        }


    }

};

function _AutoLookUpData(destObj, AjaxObject) {
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    destObj.value = result;

    if (destObj.onchange)
        destObj.onchange();

    updateTWNDValue(destObj);
};

function _AutoLookUpDataMultiple(destObj, AjaxObject) {
    var result = AjaxObject.Response;
    AjaxObject.Reset();
    AjaxObject.Dispose();
    delete AjaxObject;
    AjaxObject = null;

    var div = document.createElement('DIV');
    div.innerHTML = result;

    if (div.childNodes.length == 0) {

    }
    else if (div.childNodes.length == 1) {
        destObj.value = div.childNodes[0].innerHTML;

        if (destObj.onchange)
            destObj.onchange();

        updateTWNDValue(destObj);

    }
    else {
        var nw = w.CreateWindow('PickCounty.aspx', w);
        nw.Variables = [div, destObj];
        nw.OnClose = __AutoLookUpDataMultiple;
    }
};

function __AutoLookUpDataMultiple(sender, retval) {
    if (retval) {
        retval[0].value = retval[1];
        updateTWNDValue(retval[0]);
    }
};


function _RequeredFieldsBuild(arr) {
    for (var i = 0; i < arr.length; i++) {
        var o = arr[i];

        var op = null;

        try {
            if (!o.getAttribute('fieldname') && !o.getAttribute('FieldName') && !o.getAttribute('dbName')) {
                if (o.parentNode.parentNode.parentNode.parentNode)
                    op = o.parentNode.parentNode.parentNode.parentNode;
                else
                    op = o;
            }
            else
                op = o;
        }
        catch (err) {
            op = o;
        }



        if (!op) op = o;
        if (!op.tagName)
            op = o;

        if (op.getAttribute('RequiredState') == '1' && op.style.visibility != 'hidden') {
            o.RequiredObject = document.createElement('IMG');
            o.RequiredObject.src = 'images/requiredField.png';
            o.RequiredObject.style.position = 'absolute';

            if (op == o) {
                //o.RequiredObject.style.left = 0;
                //o.RequiredObject.style.top = -14;// - 8;
                o.RequiredObject.style.marginTop = -4;
                o.RequiredObject.style.marginLeft = -4;

                o.RequiredObject.style.zIndex = 2500000;
                op.parentNode.insertBefore(o.RequiredObject, op);
            }
            else {
                o.RequiredObject.style.left = parseInt(op.style.left || -4) - 4;
                o.RequiredObject.style.top = parseInt(op.style.top || -24);// - 8;
                o.RequiredObject.style.zIndex = 2500000;
                op.parentNode.appendChild(o.RequiredObject);
            }
        }
        else if (op.getAttribute('RequiredState') == '2' && op.style.visibility != 'hidden') {
            o.RequiredObject = document.createElement('IMG');
            o.RequiredObject.src = 'images/requiredFieldEnc.png';
            o.RequiredObject.style.position = 'absolute';

            o.RequiredObject.style.left = parseInt(op.style.left || op.offsetLeft) - 4;
            o.RequiredObject.style.top = parseInt(op.style.top || op.offsetTop);// - 8;
            o.RequiredObject.style.zIndex = 2500000;

            op.parentNode.appendChild(o.RequiredObject);
        }

        if (op.style.visibility != 'hidden' && op.getAttribute('dbContentType')) {
            var ip = op.getElementsByTagName('INPUT');


            if (ip.length > 0 && ip[0].getAttribute('type') == 'text') {
                o.DataInvalidObject = document.createElement('IMG');
                o.DataInvalidObject.src = 'images/requiredFieldDataWarning.png';
                o.DataInvalidObject.style.position = 'absolute';

                o.DataInvalidObject.style.left = parseInt(op.style.left || op.offsetLeft) - 4;
                o.DataInvalidObject.style.top = parseInt(op.style.top || op.offsetTop) + parseInt(op.style.height || op.offsetHeight) - 8;
                o.DataInvalidObject.style.zIndex = 2500000;
                o.DataInvalidObject.style.display = 'none';

                op.parentNode.appendChild(o.DataInvalidObject);


                //Data type object
                o.DataTypeObject = document.createElement('IMG');
                o.DataTypeObject.src = 'images/designFields/' + op.getAttribute('dbContentType') + '.gif';
                o.DataTypeObject.style.position = 'absolute';
                o.DataTypeObject.alt = op.getAttribute('dbContentType');

                o.DataTypeObject.style.left = parseInt(op.style.left || op.offsetLeft) - (o.RequiredObject != null ? 20 : 16);
                o.DataTypeObject.style.top = parseInt(op.style.top || op.offsetTop);
                o.DataTypeObject.style.zIndex = 2500000;
                o.DataTypeObject.style.display = 'none';

                if (!o.onfocus)
                    o.onfocus = __reqShowDataType; // function () { __reqShowDataType(o); };

                if (!o.onblur)
                    o.onblur = __reqHideDataType; // function () { __reqHideDataType(o); };

                op.parentNode.appendChild(o.DataTypeObject);
            }
        }
    }
};

function __reqShowDataType() {
    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = '';
    }
}
function __reqHideDataType() {
    if (this.DataTypeObject) {
        this.DataTypeObject.style.display = 'none';
    }
}

function getFieldByDataLink(o, link) {
    var tbls = o.parentNode.getElementsByTagName('TABLE');
    for (var i = 0; i < tbls.length; i++) {
        if (tbls[i].getAttribute('DataLink') == link) {
            return tbls[i];
        }
    }

    return null;
};

function getInputFromObject(o) {
    var oo = o.getElementsByTagName('INPUT');
    if (oo.length == 1) return oo[0];

    oo = o.getElementsByTagName('SELECT');
    if (oo.length == 1) return oo[0];

    oo = o.getElementsByTagName('TEXTAREA');
    if (oo.length == 1) return oo[0];
};

function RequeredFieldTestField(o) {
    var op = null;
    
    try {
        if (!o.getAttribute('fieldname') && !o.getAttribute('FieldName'))
            op = o.parentNode.parentNode.parentNode.parentNode;
        else
            op = o;
    }
    catch (err) {
        op = o;
    }

    //If contact is Unknown, disable all fields
    if (op.getAttribute('DataLink') == 'Ukendt kontaktperson') {
        var tbls = op.parentNode.getElementsByTagName('TABLE');


        //Test for save data?
        if (tw != null && tw.Nodes != null && tw.SelectedNode != null) {
            if (tw.SelectedNode.Uniqueidentifier != '' && !o.checked)
                o.disabled = true;
            else
                o.disabled = false;
        }


        for (var i = 0; i < tbls.length; i++) {
            var ot = tbls[i];
            var oi = getInputFromObject(ot);

            if (oi != null && ot.getAttribute('dbId') != op.getAttribute('dbId')) {
                if (o.checked) {
                    if (ot.getAttribute('DataLink') == 'DGSContactFirstname') {
                        oi.value = 'Ukendt';
                        oi.onchange();
                        //                    else if(ot.getAttribute('DataLink') == 'DGSCOntactLastname')
                        //                        oi.value='Ukendt';
                    }
                    else
                        oi.value = '';

                    ot.setAttribute('orgReqState', ot.getAttribute('RequiredState'));
                    ot.setAttribute('RequiredState', '0');

                    if (oi.RequiredObject != null)
                        oi.RequiredObject.style.display = 'none';

                    oi.disabled = true;
                }
                else {
                    ot.setAttribute('RequiredState', ot.getAttribute('orgReqState'));

                    if (oi.RequiredObject != null)
                        oi.RequiredObject.style.display = '';

                    oi.disabled = false;
                }
            }
        }
    }

    //If country is not denmark - pnr and cvr is not required and should be disabled.
    if (op.getAttribute('DataLink') == 'Countries') {
        var ocvr = getFieldByDataLink(op, 'NNECompanyCVR');
        var opnr = getFieldByDataLink(op, 'NNECompanyPNR');
        var ocounty = getFieldByDataLink(op, 'Counties');

        if (o.value != 'Denmark' && o.value != 'Danmark') {
            //disable cvr + pnr (NNECompanyCVR | NNECompanyPNR)
            if (ocvr) {
                var ocvrf = getInputFromObject(ocvr);//.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (ocvrf.RequiredObject) {

                    ocvr.setAttribute('orgReqState', ocvr.getAttribute('RequiredState'));
                    ocvr.setAttribute('RequiredState', '0');
                    ocvrf.RequiredObject.style.display = 'none';
                    ocvrf.disabled = true;
                }
            }

            if (opnr) {
                var opnrf = getInputFromObject(opnr); //opnr.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (opnrf.RequiredObject) {
                    opnr.setAttribute('orgReqState', opnr.getAttribute('RequiredState'));
                    opnr.setAttribute('RequiredState', '0');
                    opnrf.RequiredObject.style.display = 'none';
                    opnrf.disabled = true;
                }
            }

            if (ocounty) {
                var oocountyf = getInputFromObject(ocounty); //opnr.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (oocountyf.RequiredObject) {
                    ocounty.setAttribute('orgReqState', ocounty.getAttribute('RequiredState'));
                    ocounty.setAttribute('RequiredState', '0');
                    oocountyf.RequiredObject.style.display = 'none';
                    oocountyf.disabled = true;
                }
            }
        }
        else {
            //enable cvr + pnr
            if (ocvr) {
                var ocvrf = getInputFromObject(ocvr); //ocvr.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (ocvrf.RequiredObject) {
                    if (ocvr.getAttribute('RequiredState') == '0')
                        ocvr.setAttribute('RequiredState', ocvr.getAttribute('orgReqState') || '1');
                    ocvrf.RequiredObject.style.display = '';
                    ocvrf.disabled = false;

                    RequeredFieldTestField(ocvrf);
                }
            }

            if (opnr) {
                var opnrf = getInputFromObject(opnr); //opnr.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (opnrf.RequiredObject) {
                    if (opnr.getAttribute('RequiredState') == '0')
                        opnr.setAttribute('RequiredState', opnr.getAttribute('orgReqState') || '1');
                    opnrf.RequiredObject.style.display = '';
                    opnrf.disabled = false;

                    RequeredFieldTestField(opnrf);
                }
            }

            if (ocounty) {
                var oocountyf = getInputFromObject(ocounty); //opnr.childNodes[0].childNodes[0].childNodes[0].lastChild;
                if (oocountyf.RequiredObject) {
                    if (ocounty.getAttribute('RequiredState') == '0')
                        ocounty.setAttribute('RequiredState', ocounty.getAttribute('orgReqState') || '1');
                    oocountyf.RequiredObject.style.display = '';
                    oocountyf.disabled = false;

                    RequeredFieldTestField(oocountyf);
                }
            }
        }
    }

    //Test data integrety
    if (op.getAttribute('dbContentType') && op.style.visibility != 'hidden' && o.style.display != 'none') {
        if (!TestField(o, op) && o.value != '') {
            if (o.DataInvalidObject != null)
                o.DataInvalidObject.style.display = '';
        }
        else {
            if (o.DataInvalidObject != null)
                o.DataInvalidObject.style.display = 'none';
        }
    }

    //Test required state
    if (op.getAttribute('RequiredState') == '1' && op.style.visibility != 'hidden' && o.style.display != 'none') {
        if (o.RequiredObject) {
            if (!TestField(o, op)) {
                o.RequiredObject.src = 'images/requiredField.png';
                return false;
            }
            else {
                o.RequiredObject.src = 'images/requiredFieldOK.png';
                return true;
            }
        }
        else
            return true;
    }
    else if (op.getAttribute('RequiredState') == '2' && op.style.visibility != 'hidden' && o.style.display != 'none') {
        if (o.RequiredObject) {
            if (!TestField(o, op)) {
                o.RequiredObject.src = 'images/requiredFieldEnc.png';
                return false;
            }
            else {
                o.RequiredObject.src = 'images/requiredFieldOK.png';
                return true;
            }
        }
        else
            return true;
    }
    else
        return true;
};

function RequeredFieldTestFieldManual(fieldId, value) {
    var o = document.getElementById(fieldId);
    if (o) {
        var op = null;

        try {
            if (!o.getAttribute('fieldname') && !o.getAttribute('FieldName'))
                op = o.parentNode.parentNode.parentNode.parentNode;
            else
                op = o;
        }
        catch (err) {
            op = o;
        }

        if (op.getAttribute('RequiredState') == '1' && op.style.visibility != 'hidden' && o.style.display != 'none') {
            if (!TestField(o, op, value)) {
                return op.getAttribute('dbName') || o.getAttribute('fieldname') || o.getAttribute('FieldName');
            }
            else
                return null;
        }
        else if (op.getAttribute('RequiredState') == '2' && op.style.visibility != 'hidden' && o.style.display != 'none') {
            if (!TestField(o, op, value)) {
                return op.getAttribute('dbName') || o.getAttribute('fieldname') || o.getAttribute('FieldName');
            }
            else
                return null;
        }
        else {

            if (o.tagName != 'SELECT' && value != '' && value != '-' && !TestField(o, op, value) && op.style.visibility != 'hidden' && o.style.display != 'none') {
                if (op.getAttribute('dbContentType') == 'integer' && TestField(o, op, value) == false && value.replace("-", "").replace("+", "").trim().length > 18) {
                    return (op.getAttribute('dbName') || o.getAttribute('fieldname') || o.getAttribute('FieldName')) + ' (Tallet er for stort. Max 18 cifre.)';
                }
                //Tallet er for stort. Max 18 cifre.
                return op.getAttribute('dbName') || o.getAttribute('fieldname') || o.getAttribute('FieldName');
            }
            else
                return null;
        }
    }
    else
        return null;
};

function IsFieldOnlySuggested(fieldId) {
    var o = document.getElementById(fieldId);
    if (o) {
        var op = null;

        try {
            if (!o.getAttribute('fieldname') && !o.getAttribute('FieldName'))
                op = o.parentNode.parentNode.parentNode.parentNode;
            else
                op = o;
        }
        catch (err) {
            op = o;
        }

        return op.getAttribute('RequiredState') == '2';
    }
    return false;
};

function TestField(f, tbl, optionalValue) {

    var val = optionalValue || f.value;

    //if((val=='' || val==' ') && (tbl.getAttribute('RequiredState')=='1' || tbl.getAttribute('RequiredState')=='2') && tbl.style.display=='') return false;
    //if(val=='' && (tbl.getAttribute('RequiredState')=='1' || tbl.getAttribute('RequiredState')=='2') && tbl.style.display=='') return true;


    switch (tbl.getAttribute('dbContentType')) {
        case 'dropdown':
            if (val == '' || val == ' ' || val == '-') return false;
            break;

        case 'listview':
            if (val == '' || val == ' ' || val == '-') return false;
            break;

        case 'memo':
        case 'textbox':

            switch (tbl.getAttribute('DataLink')) {
                case 'NNECompanyCVR':
                    if (!optionalValue) {
                        f.value = cleanValue(f.value);
                        return isCVR(f.value);
                    }
                    else
                        return isCVR(optionalValue || f.value);
                    break;

                case 'NNECompanyPNR':
                    if (!optionalValue) {
                        f.value = cleanValue(f.value);
                        return isPNR(f.value);
                    }
                    else
                        return isPNR(optionalValue || f.value);
                    break;

                case 'CompanyCPR':
                    if (!optionalValue) {
                        f.value = cleanValue(f.value);
                        if (f.value.length == 10 && !isNaN(f.value)) {
                            f.value = f.value.substring(0, 6) + '-' + f.value.substring(6);
                        }
                        var cpr = isCPR(f.value);
                        return cpr;
                    }
                    else {
                        var cpr = isCPR(optionalValue);
                        return cpr;
                    }
                    break;

                default:
                    //               if(tbl.getAttribute('dbName').indexOf('Fax') >-1)
                    //                    alert("X" + val + "X // " + tbl.getAttribute('RequiredState') + " // " + tbl.outerHTML);
                    ////                    
                    return !(val == '' || val == ' ');
                    break;
            }
            break;

        case 'datetime':
            //            if (!optionalValue)
            //                f.value = cleanValue(f.value);
            var x = optionalValue || f.value;

            if (x == '') return false;

            var filter = /^\d{2}[./-]\d{2}[./-]\d{4}$/;

            if (filter.test(x))
                return isDate(x);
            else
                return false;
            break;

        case 'emailaddress':

            //                if(!optionalValue)
            //                    f.value = cleanValue(f.value);
            var x = optionalValue || f.value;

            //alert(optionalValue + '/' + f.value + '=' + x);



            //var filter  = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            var filter = /^([a-zæøåA-ZÆØÅ0-9_\.\-])+\@(([a-zæøåA-ZÆØÅ0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

            if (x == '' || x == ' ') return false;
            return filter.test(x);

            break;


        case 'absinteger':
            if (val == '' || val == ' ') return false;
            var tmp = optionalValue || f.value;
            if (!isNaN(tmp)) {
                tmp = parseInt(tmp);
                var filter = /^(([1-9]\d*)|0)?$/;
                if (filter.test(val) && val.trim().length <= 18) {
                    return true;
                }
                return false;
            }
            return false;
            break;

        case 'integer':
            if (val == '' || val == ' ') return false;
            var tmp = optionalValue || f.value;
            if (!isNaN(tmp)) {
                tmp = parseInt(tmp);
                var filter = /^-?(([1-9]\d*)|0)?$/;

                if (filter.test(val) && val.replace("-", "").replace("+", "").trim().length <= 18) {
                    return true;
                }
                return false;
            }
            return false;
            break;

        case 'absfloat':
            if (val == '' || val == ' ') return false;
            //var tmp=displayNumberToComputerNumber(f.value);
            var tmp = localCore.Localization.ParseLcNum(optionalValue || f.value, floatFormat);

            if (!isNaN(tmp)) {
                tmp = parseFloat(tmp);

                if (val.indexOf('.') > -1) {
                    var filter = /^([0-9]{1,3}(\.[0-9]{3})*(,[0-9]+)?|,[0-9]+)$/;
                    if (filter.test(val)) {
                        return true;
                    }
                }
                else {
                    var filter = /^(([0-9]\d*)|0)(,0*[0-9](0*[0-9])*)?$/;
                    if (filter.test(val)) {
                        return true;
                    }
                }
                return false;
            }
            return false;
            break;

        case 'float':
            if (val == '' || val == ' ') return false;
            var tmp = localCore.Localization.ParseLcNum(optionalValue || f.value, floatFormat);


            if (!isNaN(tmp)) {
                tmp = Math.abs(parseFloat(tmp));
                //var filter = /^(([0-9]\d*)|0)(\.0*[0-9](0*[0-9])*)?$/;
                //var filter =/^(?!0+\.00)(?=.{1,9}(\.|$))(?!0(?!\.))\d{1,3}(,\d{3})*(\.\d+)?$/;

                if (val.indexOf('.') > -1) {
                    var filter = /^([-]{0,1}[0-9]{1,3}(\.[0-9]{3})*(,[0-9]+)?|,[0-9]+)$/;
                    if (filter.test(val)) {
                        return true;
                    }
                }
                else {
                    var filter = /^([-]{0,1}([0-9]\d*)|0)(,0*[0-9](0*[0-9])*)?$/;
                    if (filter.test(val)) {
                        return true;
                    }
                }
            }
            return false;
            break;
    }

    //Anything else fails - return true for now
    return !(val == '' || val == ' ');

};

function isDate(str) {
    var parms = str.split(/[\.\-\/]/);
    var yyyy = parseInt(parms[2], 10);
    var mm = parseInt(parms[1], 10);
    var dd = parseInt(parms[0], 10);
    var date = new Date(yyyy, mm - 1, dd, 0, 0, 0, 0);
    return mm === (date.getMonth() + 1) &&
         dd === date.getDate() &&
       yyyy === date.getFullYear();
};


function cleanValue(value) {
    while (value.indexOf(' ') > -1) {
        value = value.replace(' ', '');
    }

    return value;
};

function extractNumbers(value) {
    var r = value.match(/[\d]+/g);
    return r;
};


function displayNumberToComputerNumber(value) {
    value = cleanValue(value);
    while (value.indexOf('.') > -1) {
        value = value.replace('.', '');
    }

    return value.replace(',', '.');
};

function computerFloatNumberToDisplayNumber(value) {
    if (IsFloatNumeric(value) == false) return '';

    var tmp = value.toString();
    if (tmp.indexOf('.') < 0)
        return computerNumberToDisplayNumber(tmp + '.00');
    else
        return computerNumberToDisplayNumber(tmp);
};

function computerNumberToDisplayNumber(value) {
    if (IsFloatNumeric(value) == false) return '';

    var tmp = (parseFloat(value)).toFixed(2).toString();
    value = '';

    var counter = 0;

    var hasDecimal = tmp.indexOf('.') > -1 ? true : false;
    var passedDecimal = !hasDecimal;

    for (var i = tmp.length - 1; i >= 0; i--) {

        if (tmp.charAt(i) == '.') {
            passedDecimal = true;
            value = ',' + value;
        }
        else {
            if (passedDecimal == true) counter++;
            value = tmp.charAt(i) + value;
            if (counter % 3 == 0 && i > 0 && counter > 0) value = '.' + value;
        }
    }

    if (value.indexOf(',') > -1) {
        while (value.length - value.indexOf(',') < 3) {
            value = value.toString() + '0';
        }

        //Manual Round Up if more than 2 digits (Math error in javascript)
        if (value.length - value.indexOf(',') > 3) {
            var rounder = value.substring(value.indexOf(',') + 3, value.indexOf(',') + 4);

            if (parseInt(rounder) > 4) {
                var digi = parseInt(value.substring(value.indexOf(',') + 1, value.indexOf(',') + 4));
                digi = digi + (10 - rounder);
                value = value.substring(0, value.indexOf(',')) + ',' + digi;
            }

        }
        return value.substring(0, value.indexOf(',') + 3);
    }
    else
        return value;
};

function isEmail(text) {
    var filter = /^([a-zæøåA-ZÆØÅ0-9_\.\-])+\@(([a-zæøåA-ZÆØÅ0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;

    return filter.test(text);
};


function isCVR(value) {
    if (value.length != 8) return false;
    if (value == '00000000') return false;

    var map = '27654321';
    var sum = 0;

    for (var i = 0; i < value.length; i++) {
        sum += parseInt(parseInt(map.charAt(i)) * parseInt(value.charAt(i)));
    }

    return sum % 11 == 0;
};

function isPNR(value) {
    //** Sætter tjek til på PNR, ESCRM-190/191
    //** Er det kun et 0
    if (parseInt(value.charAt(0)) == 0 && value.length == 1)
        return true;
    //** Er længden ikke 10
    if (value.length != 10) return false;
    //** Er værdien 10 x 0
    if(value == '0000000000') return false;

    return true;

//    var map = '2765432143';
//    var sum=0;

//    for(var i=0;i<value.length;i++)
//    {
//        sum += parseInt(parseInt(map.charAt(i))*parseInt(value.charAt(i)));
//    }
            
//    return sum % 11 == 0;
//    return !isNaN(value) && value != '' && value != ' ';
};

function isCPR(value) {

    var isValid = true;
    if (value.length != 11) { isValid = false; };

    /*if (value == '000000-0000') { isValid = false; }
    var map = '432765-4321';
    var sum = 0;
    for (var i = 0; i < value.length; i++) {
        if (map.charAt(i) != '-'){
            sum += parseInt(parseInt(map.charAt(i)) * parseInt(value.charAt(i)));
        }
    }
    if (sum % 11 != 0) { isValid = false;}*/

    var cprRegex = /^\d{6}-\d{4}$/;
    isValid = cprRegex.test(value)
    return isValid;
};
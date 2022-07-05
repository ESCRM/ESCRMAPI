using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.DBManager
{
    public enum FieldViewType
    {
        PlainTextBox=0,
        EmailTextBox=1,
        CVRTextBox=2,
        CPRTextBox=3,
        AbsIntTextBox=4,
        IntTextBox=5,
        AbsFloatTextBox=6,
        FloatTextBox=7,
        MemoField=8,
        CheckBox=9

    }

    public enum FieldRequirement
    {
        NotRequired=0,
        Required=1,
        RequiredNow=2
    }

    public enum FieldShowOnlyHere
    {
        any=0,
        POT=1,
        SMV=2
    }

    /// <summary>
    /// OBSOLETE
    /// </summary>
    class DBField
    {
        public string Name="";
        public object Value=null;
        public int X = 0;
        public int Y = 0;
        public string Width = "";
        public string Height = "";
        public FieldViewType HTMLDisplayType = FieldViewType.PlainTextBox;
        public string DBDataType = "varchar";
        public FieldRequirement IsRequired = FieldRequirement.NotRequired;
        public FieldShowOnlyHere UseOnlyHere = FieldShowOnlyHere.any;


        public DBField()
        {

        }

        public DBField(ref System.Data.SqlClient.SqlDataReader dr, int currentIndex)
        {

        }
    }
}

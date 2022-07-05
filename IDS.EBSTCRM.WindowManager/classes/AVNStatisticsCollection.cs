using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDS.EBSTCRM.Base;
using System.Web.UI.HtmlControls;

namespace IDS.EBSTCRM.WindowManager
{
    public class AVNStatisticsCollection
    {
        List<StatisticsItem> statistics = new List<StatisticsItem>();

        public void Add(AVNField Field, SQLLabelDataItem Data)
        {
            string valFormatted = Data.ValueFormatted;

            if (Field.Statistics == "count")
            {
                for (int i = 0; i < statistics.Count; i++)
                {
                    if (statistics[i].Key == Field.DatabaseColumn && statistics[i].Group == valFormatted)
                    {
                        statistics[i].Value++;
                        return;
                    }
                }
                statistics.Add(new StatisticsItem(Field.DatabaseColumn, valFormatted, Field.Statistics, 1, Data.DataType));
            }
            else if (Field.Statistics == "countcontent")
            {
                if (valFormatted != "")
                {
                    for (int i = 0; i < statistics.Count; i++)
                    {
                        if (statistics[i].Key == Field.DatabaseColumn)
                        {
                            statistics[i].Value++;
                            return;
                        }
                    }
                    statistics.Add(new StatisticsItem(Field.DatabaseColumn, "Har indhold", Field.Statistics, 1, Data.DataType));
                }
            }
            else if (Field.Statistics != "" && (Data.DataType == "float" || Data.DataType == "int")) // && Data.Value != null && Data.Value != DBNull.Value
            {
                //for (int i = 0; i < statistics.Count; i++)
                //{
                //    if (statistics[i].Key == Field.DatabaseColumn && statistics[i].Group == valFormatted)
                //    {
                //        statistics[i].Value+=TypeCast.ToDecimal(Data.Value);
                //        return;
                //    }
                //}
                statistics.Add(new StatisticsItem(Field.DatabaseColumn, valFormatted, Field.Statistics, TypeCast.ToDecimal(Data.Value), Data.DataType));
            }
            //else if (Field.Statistics == "avg" && (Data.DataType == "float" || Data.DataType == "int"))
            //{
                 
            //}
            //else if (Field.Statistics == "sum" && Data.DataType == "float" && Data.Value != null && Data.Value != DBNull.Value)
            //{
            //    statVal[Field.DatabaseColumn + "\t" + Data.ValueFormatted] = (TypeCast.ToDecimal(statVal[Field.DatabaseColumn + "\t" + Data.ValueFormatted]) + TypeCast.ToDecimal(Data.Value)).ToString();
            //}
            //else if (Field.Statistics == "sum" && Data.DataType == "int" && Data.Value != null && Data.Value != DBNull.Value)
            //{
            //    statVal[Field.DatabaseColumn + "\t" + Data.ValueFormatted] = (TypeCast.ToInt(statVal[Field.DatabaseColumn + "\t" + Data.ValueFormatted]) + TypeCast.ToInt(Data.Value)).ToString();
            //}
            //else if (Field.Statistics == "min.avg.max" && Data.Value != null && Data.Value != DBNull.Value && (Data.DataType == "float" || Data.DataType == "int"))
            //{

        //}
            else if (Field.Statistics == "year" && Data.DataType == "datetime" && Data.Value != null && Data.Value != DBNull.Value)
            {
                valFormatted = TypeCast.ToDateTime(Data.Value).ToString("yyyy");
                for (int i = 0; i < statistics.Count; i++)
                {
                    if (statistics[i].Key == Field.DatabaseColumn && statistics[i].Group == valFormatted)
                    {
                        statistics[i].Value++;
                        return;
                    }
                }
                statistics.Add(new StatisticsItem(Field.DatabaseColumn, valFormatted, Field.Statistics, 1, Data.DataType));
            }
            else if (Field.Statistics == "quarter" && Data.DataType == "datetime" && Data.Value != null && Data.Value != DBNull.Value)
            {
                DateTime dt =TypeCast.ToDateTime(Data.Value);
                valFormatted = dt.ToString("yyyy") + " Q" + (dt.Month <=3 ? 1 : dt.Month <=6 ? 2 : dt.Month <=9 ? 3 : 4);
                for (int i = 0; i < statistics.Count; i++)
                {
                    if (statistics[i].Key == Field.DatabaseColumn && statistics[i].Group == valFormatted)
                    {
                        statistics[i].Value++;
                        return;
                    }
                }
                statistics.Add(new StatisticsItem(Field.DatabaseColumn, valFormatted, Field.Statistics, 1, Data.DataType));
            }
            else if (Field.Statistics == "month" && Data.DataType == "datetime" && Data.Value != null && Data.Value != DBNull.Value)
            {
                valFormatted = TypeCast.ToDateTime(Data.Value).ToString("yyyy MM");
                for (int i = 0; i < statistics.Count; i++)
                {
                    if (statistics[i].Key == Field.DatabaseColumn && statistics[i].Group == valFormatted)
                    {
                        statistics[i].Value++;
                        return;
                    }
                }
                statistics.Add(new StatisticsItem(Field.DatabaseColumn, valFormatted, Field.Statistics, 1, Data.DataType));
            }
        }

        public HtmlGenericControl ToHTML()
        {
            HtmlGenericControl root = new HtmlGenericControl("DIV");
            if (statistics == null || statistics.Count == 0)
            {
                HtmlGenericControl divNoStat = new HtmlGenericControl("DIV");
                divNoStat.InnerText = "Ingen statistikker for denne projektnote";
                divNoStat.Attributes["style"] = "display:block;text-align:center;padding:4px;";
                root.Controls.Add(divNoStat);
                return root;
            }

            HtmlGenericControl tHead = new HtmlGenericControl("h2");
            tHead.InnerText = "Statistik*";

            root.Controls.Add(tHead);

            statistics = statistics.OrderBy(o => o.Key).ThenBy(o => o.Group).ThenBy(o => o.Value).ToList();

            string oldKey = "";
            string oldGroup = "";
            string oldStats = "";
            string oldType = "";
            int keyCounter = 0;
            decimal maxValue = decimal.MinValue;
            decimal minValue = decimal.MaxValue;
            decimal sumValue = 0;

            HtmlTable tbl = new HtmlTable();
            tbl.Attributes["class"] = "AVNStatsSumTable";
            root.Controls.Add(tbl);

            foreach(StatisticsItem sim in statistics)
            {
                if (oldKey != sim.Key)
                {
                    string maskFromField = "#,##0";
                    if(oldStats == "float")
                        maskFromField = "#,##0.00";

                    if (oldStats == "sum")
                    {
                        tbl.Controls.Add(CreateRow("Sum (af " + keyCounter.ToString("#,##0") + ")", sumValue, maskFromField));
                    }
                    else if (oldStats == "avg")
                    {
                        tbl.Controls.Add(CreateRow("Snit (af " + keyCounter.ToString("#,##0") + ")", (sumValue / keyCounter), "#,##0.00"));
                    }
                    else if (oldStats == "min.avg.max")
                    {
                        tbl.Controls.Add(CreateRow("Minimum", minValue, maskFromField));
                        tbl.Controls.Add(CreateRow("Gennemsnit", (sumValue / keyCounter), "#,##0"));
                        tbl.Controls.Add(CreateRow("Maksimum", maxValue, maskFromField));
                        tbl.Controls.Add(CreateRow("Antal", keyCounter, "##,##0"));
                    }
                    else if (oldStats == "min.avg.max.sum")
                    {
                        tbl.Controls.Add(CreateRow("Minimum", minValue, maskFromField));
                        tbl.Controls.Add(CreateRow("Gennemsnit", (sumValue / keyCounter), "#,##0"));
                        tbl.Controls.Add(CreateRow("Maksimum", maxValue, maskFromField));
                        tbl.Controls.Add(CreateRow("Antal", keyCounter, "##,##0"));
                        tbl.Controls.Add(CreateRow("Sum", sumValue, "##,##0"));
                    }
                    else if (oldStats == "min.max")
                    {
                        tbl.Controls.Add(CreateRow("Minimum", minValue, maskFromField));
                    }

                    oldType = sim.DataType;
                    oldKey = sim.Key;
                    oldGroup = sim.Group;
                    oldStats = sim.StatisticsType;
                    keyCounter = 0;
                    maxValue = decimal.MinValue;
                    minValue = decimal.MaxValue;
                    sumValue = 0;

                    //Add header html controls
                    HtmlTableRow trH = new HtmlTableRow();
                    HtmlTableCell tdH = new HtmlTableCell();
                    tdH.ColSpan = 2;
                    HtmlGenericControl divH = new HtmlGenericControl("DIV");
                    divH.Attributes["class"] = "AVNStatsSumTableHeader";
                    divH.InnerText = sim.Key;
                    tdH.Controls.Add(divH);
                    trH.Controls.Add(tdH);
                    tbl.Controls.Add(trH);
                }
                keyCounter++;
                sumValue += sim.Value;

                //Set min and max values for group
                if (maxValue < sim.Value) maxValue = sim.Value;
                if (minValue > sim.Value) minValue = sim.Value;

                //Write row (if multiline statistics)
                if (oldStats == "count" || oldStats == "countcontent" || oldStats == "year" || oldStats == "quarter" || oldStats == "month")
                {
                    tbl.Controls.Add(CreateRow(sim.Group, sim.Value, "#,##0"));
                }
            }

            HtmlGenericControl tNote = new HtmlGenericControl("div");
            tNote.Attributes["style"] = "margin:10px 3px 3px 3px;font-style:italic;";
            tNote.InnerText = "*) Ovenstående statistikker er beregnet ud fra hele resultatet og der er ikke taget højde for eventuelle brugerdefinerede filtre.";

            root.Controls.Add(tNote);

            return root;

        }

        private HtmlTableRow CreateRow(string text, decimal value, string mask)
        {
            HtmlTableRow tr = new HtmlTableRow();
            HtmlTableCell tdText = new HtmlTableCell();
            HtmlTableCell tdValue = new HtmlTableCell();
            tdValue.Align = "right";
            tdValue.VAlign = "bottom";

            tdText.Attributes["class"] = "AVNStatsSumTableText";
            tdValue.Attributes["class"] = "AVNStatsSumTableValue";

            if (text == "" || text == null)
                text = "-Blank-";

            tdText.InnerText = text;

            if (mask != "" && mask != null)
                tdValue.InnerText = value.ToString(mask);
            else
                tdValue.InnerText = value.ToString();

            tr.Controls.Add(tdText);
            tr.Controls.Add(tdValue);

            return tr;
        }

        public class StatisticsItem
        {
            public string Key { get; set; }
            public string Group { get; set; }
            public string StatisticsType { get; set; }
            public decimal Value { get; set; }
            public string DataType { get; set; }

            public StatisticsItem()
            {

            }

            public StatisticsItem(string key, string group, string statisticsType, decimal value, string datatype)
            {
                this.Key = key;
                this.Group = group;
                this.Value = value;
                this.StatisticsType = statisticsType;
                this.DataType = datatype;
            }
        }
    }
}
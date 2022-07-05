using System;
using System.Collections.Generic;
using System.Text;
using IDS.EBSTCRM.Base.NNE;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Static class for searching for NNO Data
    /// NNO = Private Person Section / White Pages
    /// OBSOLETE
    /// </summary>
    public static class NNOSearcher
    {
        public static List<NNO.Subscriber> NNOFindContacts(ref SQLDB sql, ref User U, string name, string phone, int maxResults)
        {
            try
            {
                string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];
                NNO.NavneNumreBasis nno = new IDS.EBSTCRM.Base.NNO.NavneNumreBasis();
                NNO.SearchQuestion question = new IDS.EBSTCRM.Base.NNO.SearchQuestion();
                question.phone = phone;
                question.startswith="y";
                question.phonetics = "y";
                question.username = NNESerial;
                
                List<NNO.Subscriber> result = new List<IDS.EBSTCRM.Base.NNO.Subscriber>();
                NNO.SubscriberResult tmp = nno.lookupSubscribers(question);

                if (tmp.subscribers != null)
                    result.AddRange(tmp.subscribers);

                question.phone = null;
                question.name = name;

                tmp = nno.lookupSubscribers(question);
                if (tmp.subscribers != null)
                    result.AddRange(tmp.subscribers);

                nno = null;

                if (result.Count > maxResults)
                {
                    result.RemoveRange(maxResults, result.Count - maxResults);
                }

                return result;
            }
            catch (Exception e)
            {
                ExceptionMail.SendException(e, U);
                throw e;
            }
        }

        public static NNO.Subscriber NNOGetContact(ref SQLDB sql, ref User U, int NNEID)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];
            NNO.NavneNumreBasis nno = new IDS.EBSTCRM.Base.NNO.NavneNumreBasis();
            NNO.SearchIDQuestion question = new IDS.EBSTCRM.Base.NNO.SearchIDQuestion();
            question.TDC_PID = NNEID;
            question.username = NNESerial;
            NNO.SubscriberResult tmp = nno.lookupSubscribersByID(question);

            if (tmp != null)
            {
                if (tmp.subscribers != null)
                {
                    return tmp.subscribers[0];
                }
                return null;
            }
            return null;

        }
    }

    /// <summary>
    /// Static class for searching for NNE Data
    /// NNE = Company Section / Yellow Pages
    /// </summary>
    public static class NNESearcher
    {
        public static List<NNE.CompanyBasic> NNEFindCompany(ref SQLDB sql, ref User U, string name, int CVR, int maxResults)
        {
            try
            {
                string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];

                NNE.NNE nne = new IDS.EBSTCRM.Base.NNE.NNE();
                NNE.Question question = new IDS.EBSTCRM.Base.NNE.Question();
                IDS.EBSTCRM.Base.NNE.CompanyBasic[] tmp = null;
               
                question.nameStartsWith = true;
                question.name = name;

                List<NNE.CompanyBasic> result = new List<IDS.EBSTCRM.Base.NNE.CompanyBasic>();

                int InclAdProtected = 0;

                tmp = nne.search(question, maxResults, 1, InclAdProtected, NNESerial).companyBasic;
                if(tmp!=null)
                {
                    foreach (NNE.CompanyBasic cb in tmp)
                    {
                        if (!result.Contains(cb))
                            result.Add(cb);
                    }
                }

                question.nameStartsWith = false;
                question.name = null;
                question.cvrNo = CVR;

                tmp = nne.search(question, maxResults, 1, InclAdProtected, NNESerial).companyBasic;

                if (tmp != null)
                {
                    foreach (NNE.CompanyBasic cb in tmp)
                    {
                        if (!result.Contains(cb))
                            result.Add(cb);
                    }
                }


                question.nameStartsWith = false;
                question.name = null;
                question.cvrNo = 0;
                question.phone = name;

                tmp = nne.search(question, maxResults, 1, InclAdProtected, NNESerial).companyBasic;

                if (tmp != null)
                {
                    foreach (NNE.CompanyBasic cb in tmp)
                    {
                        if (!result.Contains(cb))
                            result.Add(cb);
                    }
                }


                question = new Question();
                question.tdcId = TypeCast.ToInt(name);
                if (question.tdcId > 0)
                {
                    tmp = nne.search(question, maxResults, 1, InclAdProtected, NNESerial).companyBasic;

                    if (tmp != null)
                    {
                        foreach (NNE.CompanyBasic cb in tmp)
                        {
                            if (!result.Contains(cb))
                                result.Add(cb);
                        }
                    } 
                    
                    tmp = nne.search(question, maxResults, 1, 1, NNESerial).companyBasic;

                    if (tmp != null)
                    {
                        foreach (NNE.CompanyBasic cb in tmp)
                        {
                            if (!result.Contains(cb))
                                result.Add(cb);
                        }
                    }

                }

                //If not results were found, try searching within ad protected companies.
                //if (result.Count == 0 && InclAdProtected == 0)
                //{
                    InclAdProtected = 1;

                    NNE.QuestionTargetGroup questionTarget = new NNE.QuestionTargetGroup();
                    questionTarget.cvrNo = CVR;
                    questionTarget.legalEntitiesOnly = true;

                    tmp = nne.search(questionTarget, maxResults, 1, InclAdProtected, NNESerial).companyBasic;

                    if (tmp != null && tmp.Length > 0)
                    {
                        //Cowboy hack - use linq instead
                        bool Exists = false;
                        foreach (NNE.CompanyBasic cb in result)
                        {
                            if (cb.tdcId == tmp[0].tdcId)
                            {
                                Exists = true;
                                break;
                            }
                        }

                        if (!Exists)
                            result.Add(tmp[0]);
                    }


                    questionTarget = new NNE.QuestionTargetGroup();
                    questionTarget.cvrNo = CVR;
                    questionTarget.legalEntitiesOnly = false;

                    tmp = nne.search(questionTarget, maxResults, 1, InclAdProtected, NNESerial).companyBasic;

                    

                    if (tmp != null && tmp.Length > 0)
                    {
                        //Cowboy hack - use linq instead
                        bool Exists = false;
                        foreach (NNE.CompanyBasic cb in result)
                        {
                            if (cb.tdcId == tmp[0].tdcId)
                            {
                                Exists = true;
                                break;
                            }
                        }

                        if (!Exists)
                            result.Add(tmp[0]);
                    }

                //}

                nne = null;

                if (result.Count > maxResults)
                {
                    result.RemoveRange(maxResults, result.Count - maxResults);
                }

                return result;
            }
            catch (Exception e)
            {
                ExceptionMail.SendException(e, U);
                throw e;
            }
        }

        // Find only a single company by searching for CVR. Exceptions are just thrown, not sent by mail.
        // Searches by PNo if that is != 0, otherwise by CVR
        // (note that while PNo is defined as 10 digit, it seems that all valid PNos fit in an int)
        public static NNE.CompanyBasic NNEFindCompanyFromCvrOrPNo(int CVR, int PNo)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];

            NNE.NNE nne = new NNE.NNE();

            NNE.QuestionTargetGroup q = new NNE.QuestionTargetGroup();
            if (PNo > 0 && PNo.ToString().Length == 10)    // we don't use any obviously invalid PNos
                q.PNo = PNo;
            else if (CVR > 10000000)
            {
                q.cvrNo = CVR;
                q.legalEntitiesOnly = true;
            }
            else
                return null;    // Neither CVR nor PNo contains useful data

            NNE.Answer a = nne.search(q, 1/*results per page*/, 1/*page no*/, 1/*InclAdProtected*/, NNESerial);
            // When InclAdProtected = 1, NNE will never return more than one result no matter what.

            nne.Dispose();

            if (a.errorMessage != null && a.errorMessage != "")
                throw new NNE.NNEException(a.errorMessage + " CVR=" + CVR.ToString() + " PNr=" + PNo.ToString());

            if (a.companyBasic != null && a.companyBasic.Length > 0)
                return a.companyBasic[0];
            else
                return null;
        }

        public static NNE.Company NNEGetCompany(ref SQLDB sql, ref User U, int NNEID)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];
            NNE.NNE nne = new IDS.EBSTCRM.Base.NNE.NNE();
            NNE.Company c;
            try
            {
                c = nne.fetchCompany(NNEID, 0, "", NNESerial);
            }
            finally
            {
                nne.Dispose();
            }
            return c;
        }

        public static NNE.Trade NNEGetCompanyTrade(ref SQLDB sql, ref User U, int NNEID)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];
            NNE.NNE nne = new IDS.EBSTCRM.Base.NNE.NNE();
            NNE.Trade[] Trades = nne.fetchCompanyTrade(NNEID, 0, "", NNESerial);
            nne.Dispose();
            
            if (Trades.Length > 0)
                return Trades[0];
            else
                return null;
        }


        // Return all Danish and Greenlandish municipalities, with value = the number used in Company.geography.municipalityCode
        // Note that this is danish "kommuner", which other parts of EBSTCRM call Counties (but NNE uses counties = danish "region").
        // Note also, that Københavns Kommune is returned with text = "Københavns", whereas the rest of EBSTCRM uses "København".
        private static NNE.ValueText[] NNEMunicipalitiesCache;
        public static NNE.ValueText[] NNEMunicipalities()
        {
            if (NNEMunicipalitiesCache == null)
            {
                NNE.NNE nne = new IDS.EBSTCRM.Base.NNE.NNE();
                NNEMunicipalitiesCache = nne.lookupMunicipalityCodes();
                nne.Dispose();
            }
            return NNEMunicipalitiesCache;
        }

        /// <summary>
        /// Get additional names from NNE Company
        /// </summary>
        /// <param name="NNEID"></param>
        /// <returns></returns>
        public static string[] NNEGetCompanyAdditionalNames(int NNEID)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];

            NNE.NNE nne = new NNE.NNE();
            string[] rv = nne.fetchCompanyAdditionalNames(NNEID, 0, "", NNESerial);
            nne.Dispose();
            nne = null;

            return rv;
        }

        public static void NNE_GetCompanyTurnOvers(int NNEID)
        {
            string NNESerial = System.Configuration.ConfigurationManager.AppSettings["NNESerial"];
            NNE.NNE nne = new IDS.EBSTCRM.Base.NNE.NNE();

            //IDS.EBSTCRM.Base.NNE.Company cp = new NNE.Company();
            //cp.geography.
            
            //Finance[] fin = nne.fetchCompanyFinance(NNEID, 0, "", NNESerial);
            //fin[0].
        }
    }
}

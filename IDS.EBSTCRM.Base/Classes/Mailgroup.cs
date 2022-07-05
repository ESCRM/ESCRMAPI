using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Deleted mailgroup (interressegruppe)
    /// </summary>
    [Serializable()]
    public class MailgroupDeleted : Mailgroup
    {
        public string DeletedByUser = "";
        public DateTime DateDeleted;

        public MailgroupDeleted()
        {
        }

        public MailgroupDeleted(ref SqlDataReader dr)
            : base(ref dr)
        {
            DeletedByUser = TypeCast.ToString(dr["DeletedByUser"]);
            DateDeleted = TypeCast.ToDateTime(dr["DateDeleted"]);
        }
    }

    /// <summary>
    /// Mailgroup (interressegruppe)
    /// Can be applied to a SMV/POT Contact
    /// </summary>
   [Serializable()]
    public class Mailgroup : EventLogBase
   {
       
       private int Id;
       private string Name;
       private string CreatedBy;
        public  string CreatedByUsername;
       private object DateStamp;
       private int OrganisationId;
        public int FolderId;

       public int id
       {
           get { return Id; }
           set { Id = value; }
       }

       public string name
       {
           get { return Name; }
           set { Name = value; }
       }

       public string createdBy
       {
           get { return CreatedBy; }
           set { CreatedBy = value; }
       }

       public object dateStamp
       {
           get { return DateStamp; }
           set { DateStamp = value; }
       }
       
       public int organisationId
       {
           get { return OrganisationId; }
           set { OrganisationId = value; }
       }

       public DateTime? DeactivatedDate { get; set; }
       public string DeactivatedBy { get; set; }
       public string DeactivatedByUsername { get; set; }
       public bool SharedWith { get; set; }

       public bool Writeable { get; set; }

       public string SharedWithText { get; set; }

       public Mailgroup()
       { 
       }
       
       public Mailgroup(ref SqlDataReader dr)
       {
           Populate(ref dr);
       }

       private void Populate(ref SqlDataReader dr)
       {
           id = TypeCast.ToInt(dr["Id"]);
           name = TypeCast.ToString(dr["Name"]);
           createdBy = TypeCast.ToString(dr["CreatedBy"]);
           dateStamp = TypeCast.makeDateOrNothing(dr["DateStamp"]);
           organisationId = TypeCast.ToInt(dr["organisationId"]);
           FolderId = TypeCast.ToInt(dr["folderId"]);
           CreatedByUsername = TypeCast.ToString(dr["CreatedByUsername"]);

           DeactivatedDate = TypeCast.ToDateTimeLoose(dr["DeactivatedDate"]);
           DeactivatedBy = TypeCast.ToString(dr["DeactivatedBy"]);
           DeactivatedByUsername = TypeCast.ToString(dr["DeactivatedByUsername"]);
           SharedWith = TypeCast.ToBool(dr["SharedWith"]);

           Writeable = TypeCast.ToBool(dr["Writeable"]);

           SharedWithText = TypeCast.ToString(dr["SharedWithText"]);
       }   
   }

   /// <summary>
   /// Mailgroup (interressegruppe)
   /// Added to a SMV/POT Contact with Created By UserId
   /// </summary>
    [Serializable()]
    public class MailgroupOnContact : Mailgroup
    {
       public string RelationCreatedByUsername;
       public bool FromBatchJob;

       public MailgroupOnContact() : base()
       { 
       }

        public MailgroupOnContact(ref SqlDataReader dr) :base(ref dr)
       {
           Populate(ref dr);
       }

       private void Populate(ref SqlDataReader dr)
       {

           RelationCreatedByUsername = TypeCast.ToString(dr["RelationCreatedByUsername"]);
           FromBatchJob = TypeCast.ToBool(dr["FromBatchJob"]);
       }  
    }
}

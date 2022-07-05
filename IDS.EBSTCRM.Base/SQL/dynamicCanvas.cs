using System;
using System.Data;
using System.Configuration;
using System.Web;
using IDS.EBSTCRM.Base;

namespace IDS.EBSTCRM.Base
{
    /// <summary>
    /// Handles the Canvas of SMV/POT Contact and Company
    /// OBSOLETE
    /// </summary>
    public class dynamicCanvas
    {
        public int Id;
        public int OrganisationId;
        public string CanvasType;
        public int Width;
        public int Height;

        public dynamicCanvas()
        {

        }

        public dynamicCanvas(int id, int organisationId, string canvasType, int width, int height)
        {
            Id = id;
            OrganisationId = organisationId;
            CanvasType = canvasType;
            Width=width;
            Height = height;
        }

        public dynamicCanvas(ref System.Data.SqlClient.SqlDataReader dr)
        {
            Id = TypeCast.ToInt(dr["id"]);
            OrganisationId = TypeCast.ToInt(dr["OrganisationId"]);
            CanvasType = TypeCast.ToString(dr["CanvasType"]);
            Width = TypeCast.ToInt(dr["Width"]);
            Height = TypeCast.ToInt(dr["Height"]);
        }

    }
}

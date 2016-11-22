using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class WayRoute
    {
        private int wayRouteId;

        public int WayRouteId
        {
            get { return wayRouteId; }
            set { wayRouteId = value; }
        }
        private string code;

        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        private string lastModifyAccount;

        public string LastModifyAccount
        {
            get { return lastModifyAccount; }
            set { lastModifyAccount = value; }
        }
        private DateTime lastModifyDate;

        public DateTime LastModifyDate
        {
            get { return lastModifyDate; }
            set { lastModifyDate = value; }
        }
        private string remark;

        public string Remark
        {
            get { return remark; }
            set { remark = value; }
        }
        private int airportId;

        public int AirportId
        {
            get { return airportId; }
            set { airportId = value; }
        }
        private string designation;

        public string Designation
        {
            get { return designation; }
            set { designation = value; }
        }
        private string flyType;

        public string FlyType
        {
            get { return flyType; }
            set { flyType = value; }
        }
        private int isRNAV;

        public int IsRNAV
        {
            get { return isRNAV; }
            set { isRNAV = value; }
        }
        public int Num { get; set; }
        public int Is_AIP { get; set; }
        public WayRoute() { }
        //public WayRoute(string code, string lastModifyAccount, DateTime lastModifyDate, string remark, int airportId, string designation, string flyType, int isRNAV) 
        public WayRoute(string code, string lastModifyAccount, string remark, int airportId, string designation, string flyType, int isRNAV,int num,int is_AIP) 
        {
            this.Code = code;
            this.LastModifyAccount = lastModifyAccount;
            //this.LastModifyDate = lastModifyDate;
            this.Remark = remark;
            this.AirportId = airportId;
            this.Designation = designation;
            this.FlyType = flyType;
            this.IsRNAV = isRNAV;
            this.Num = num;
            this.Is_AIP = is_AIP;
        }

        //public WayRoute(int wayRouteId,string code, string lastModifyAccount, DateTime lastModifyDate, string remark, int airportId, string designation, string flyType, int isRNAV)
        public WayRoute(int wayRouteId, string code, string lastModifyAccount, string remark, int airportId, string designation, string flyType, int isRNAV,int num,int is_AIP)
        {
            this.WayRouteId = wayRouteId;
            this.Code = code;
            this.LastModifyAccount = lastModifyAccount;
            //this.LastModifyDate = lastModifyDate;
            this.Remark = remark;
            this.AirportId = airportId;
            this.Designation = designation;
            this.FlyType = flyType;
            this.IsRNAV = isRNAV;
            this.Num = num;

            this.Is_AIP = is_AIP;
        }
    }
}

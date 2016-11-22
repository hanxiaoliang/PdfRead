using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class WayRoutePoint
    {
        private int _WayRoutePoint_Id;
        public int WayRoutePoint_Id
        {
            get { return _WayRoutePoint_Id; }
            set { _WayRoutePoint_Id = value; }
        }
        private int wayPointId;
        public int WayPointId
        {
            get { return wayPointId; }
            set { wayPointId = value; }
        }
        private int wayRouteId;
        public int WayRouteId
        {
            get { return wayRouteId; }
            set { wayRouteId = value; }
        }
        private int sortId;

        public int SortId
        {
            get { return sortId; }
            set { sortId = value; }
        }
        private string altitude;

        public string Altitude
        {
            get { return altitude; }
            set { altitude = value; }
        }
        private string maxAltitude;

        public string MaxAltitude
        {
            get { return maxAltitude; }
            set { maxAltitude = value; }
        }
        private string confinerate;

        public string Confinerate
        {
            get { return confinerate; }
            set { confinerate = value; }
        }
        private string property;

        public string Property
        {
            get { return property; }
            set { property = value; }
        }
        private DateTime lastModifyDate;

        public DateTime LastModifyDate
        {
            get { return lastModifyDate; }
            set { lastModifyDate = value; }
        }
        private string lastModifyAccount;

        public string LastModifyAccount
        {
            get { return lastModifyAccount; }
            set { lastModifyAccount = value; }
        }
        //20141106 geyafeng insert start
        public string SUGGESTEDALTITUDE { get; set; }
        public string ISBYATC { get; set; }
        public string MAGNETICHEAD { get; set; }
        public string TURNINDICATOR { get; set; }
        public int? NAVPERFORMANCEID { get; set; }
        public int? TRACKDESCRIBEDID { get; set; }
        public int? REFERENCEPOINT { get; set; }
        public string RAD_LENGTH { get; set; }
        public string VPATCH { get; set; }
        //20141106 geyafeng insert end

        public WayRoutePoint() { }
        //public wayRoute_Point(int wayPointId, int wayRouteId, int sortId, int altitude, int maxAltitude, string confinerate, string property, DateTime lastModifyDate, string lastModifyAccount) 

        //20141106 geyafeng update start
        //public WayRoutePoint(int wayPointId, int wayRouteId, int sortId, int altitude, int maxAltitude, string confinerate, string property, string lastModifyAccount) 
        //{
        //    this.WayPointId = wayPointId;
        //    this.WayRouteId = wayRouteId;
        //    this.SortId = sortId;
        //    this.Altitude = altitude;
        //    this.MaxAltitude = maxAltitude;
        //    this.Confinerate = confinerate;
        //    this.Property = property;
        //    //this.LastModifyDate = lastModifyDate;
        //    this.LastModifyAccount = lastModifyAccount;
        //}
        public WayRoutePoint(int wayPointId, int wayRouteId, int sortId, string  altitude, string  maxAltitude, string confinerate, string property, string lastModifyAccount, string SUGGESTEDALTITUDE, string ISBYATC, string MAGNETICHEAD, string TURNINDICATOR, int NAVPERFORMANCEID, int TRACKDESCRIBEDID, int REFERENCEPOINT, string RAD_LENGTH, string VPATCH)
        {
            this.WayPointId = wayPointId;
            this.WayRouteId = wayRouteId;
            this.SortId = sortId;
            this.Altitude = altitude;
            this.MaxAltitude = maxAltitude;
            this.Confinerate = confinerate;
            this.Property = property;
            this.LastModifyAccount = lastModifyAccount;
            this.SUGGESTEDALTITUDE = SUGGESTEDALTITUDE;
            this.ISBYATC = ISBYATC;
            this.MAGNETICHEAD = MAGNETICHEAD;
            this.TURNINDICATOR = TURNINDICATOR;
            this.NAVPERFORMANCEID = NAVPERFORMANCEID;
            this.TRACKDESCRIBEDID = TRACKDESCRIBEDID;
            this.REFERENCEPOINT = REFERENCEPOINT;
            this.RAD_LENGTH = RAD_LENGTH;
            this.VPATCH = VPATCH;

        }
        ////public wayRoute_Point(int wayRoute_Point_Id, int wayPointId, int wayRouteId, int sortId, int altitude, int maxAltitude, string confinerate, string property, DateTime lastModifyDate, string lastModifyAccount)
        //public WayRoutePoint(int wayRoute_Point_Id, int wayPointId, int wayRouteId, int sortId, int altitude, int maxAltitude, string confinerate, string property, string lastModifyAccount, int SUGGESTEDALTITUDE, int ISBYATC, string MAGNETICHEAD, string TURNINDICATOR, int NAVPERFORMANCEID, int TRACKDESCRIBEDID, int REFERENCEPOINT, int RAD_LENGTH, string VPATCH)
        //{
        //    this.WayRoutePoint_Id = wayRoute_Point_Id;
        //    this.WayPointId = wayPointId;
        //    this.WayRouteId = wayRouteId;
        //    this.SortId = sortId;
        //    this.Altitude = altitude;
        //    this.MaxAltitude = maxAltitude;
        //    this.Confinerate = confinerate;
        //    this.Property = property;
        //    //this.LastModifyDate = lastModifyDate;
        //    this.LastModifyAccount = lastModifyAccount;
        //}
        public WayRoutePoint(int wayRoute_Point_Id, int wayPointId, int wayRouteId, int sortId, string  altitude, string  maxAltitude, string confinerate, string property, string lastModifyAccount, string  SUGGESTEDALTITUDE, string ISBYATC, string MAGNETICHEAD, string TURNINDICATOR, int NAVPERFORMANCEID, int TRACKDESCRIBEDID, int REFERENCEPOINT, string RAD_LENGTH, string VPATCH)
        {
            this.WayRoutePoint_Id = wayRoute_Point_Id;
            this.WayPointId = wayPointId;
            this.WayRouteId = wayRouteId;
            this.SortId = sortId;
            this.Altitude = altitude;
            this.MaxAltitude = maxAltitude;
            this.Confinerate = confinerate;
            this.Property = property;
            this.LastModifyAccount = lastModifyAccount;
            this.SUGGESTEDALTITUDE = SUGGESTEDALTITUDE;
            this.ISBYATC = ISBYATC;
            this.MAGNETICHEAD = MAGNETICHEAD;
            this.TURNINDICATOR = TURNINDICATOR;
            this.NAVPERFORMANCEID = NAVPERFORMANCEID;
            this.TRACKDESCRIBEDID = TRACKDESCRIBEDID;
            this.REFERENCEPOINT = REFERENCEPOINT;
            this.RAD_LENGTH = RAD_LENGTH;
            this.VPATCH = VPATCH;
        }
    }
}

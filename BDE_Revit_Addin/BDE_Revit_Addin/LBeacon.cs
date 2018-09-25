using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;
using Point = GeoJSON.Net.Geometry.Point;

namespace BDE
{
    class LBeacon
    {
        /*
         * Initialize beacon object from FamilyInstance and LocationPoint
         * 
         */
        public LBeacon(FamilyInstance fi, XYZ lp, Level level)
        {
            CategoryName = fi.Category.Name;
            BeaconType = fi.Name;
            ElementId = fi.Id;
            GUID = Guid.NewGuid().ToString();
            Level = level.Name;
            XLocation = lp.X;
            YLocation = lp.Y;
            ZLocation = lp.Z;
            Parameter mark = fi.LookupParameter("Mark");
            Mark = mark.AsString();
        }

        public string Mark
        {
            get; private set;
        }

        /*
        * Getter for beacon's beamwidths
        * Example: 60 degree, 30 degree
        */
        public double Beamwidths
        {
            get; private set;
        }

        /*
        * Getter for beacon's Anteena Type
        */
        public String AnteenaType
        {
            get; private set;
        }

        /*
        * Getter for beacon's Cable
        */
        public String Cable
        {
            get; private set;
        }

        /*
        * Getter for beacon's Connector
        */
        public String Connector
        {
            get; private set;
        }

        /*
        * Getter for beacon's Emergency Instructions
        */
        public String EmergencyInstructions
        {
            get; private set;
        }

        /*
        * Getter for beacon's Frequency Coverage
        */
        public String FrequencyCoverage
        {
            get; private set;
        }

        /*
        * Getter for beacon's GUID
        */
        public String GUID
        {
            get; private set;
        }

        /*
         * Getter for beacon's category name
         */
        public String CategoryName
        {
            get; private set;
        }

        /*
         * Getter for beacon's type
         */
        public String BeaconType
        {
            get; private set;
        }

        /*
         * Getter for beacon's element id in Revit project
         */
        public ElementId ElementId
        {
            get; private set;
        }

        /*
         * Getter for beacon's latitude
         */
        public double Latitude
        {
            get; private set;
        }

        /*
        * Getter for beacon's longitude
        */
        public double Longitude
        {
            get; private set;
        }

        /*
        * Getter for beacon's level
        */
        public String Level
        {
            get; private set;
        }

        /*
        * Getter for beacon's peak gain
        */
        public String PeakGain
        {
            get; private set;
        }

        /*
        * Getter for beacon's polarization
        */
        public String Polarization
        {
            get; private set;
        }

        /*
        * Getter for beacon's Returns Loss
        */
        public String ReturnsLoss
        {
            get; private set;
        }

        /*
         * Getter for beacon's x location coordinate
         */
        public double XLocation
        {
            get; private set;
        }

        /*
         * Getter for beacon's y location coordinate
         */
        public double YLocation
        {
            get; private set;
        }

        /*
         * Getter for beacon's z location coordinate
         */
        public double ZLocation
        {
            get; private set;
        }

        /*
         * Getter for a beacon's coordinates in Point format
         * Geographic Position seems to use Y,X,Z format so change accordingly
         */
        public Point BeaconCoordinates
        {
            get
            {
                return new Point(new GeographicPosition(YLocation, XLocation, ZLocation));
            }
        }

        /*
         * GeoJSON Feature Representation of a beacon
         */
        public Feature ToGeoJSONFeature()
        {
            var properties = new Dictionary<String, object>();
            properties.Add("Type", BeaconType);
            properties.Add("Element Id", ElementId);
            properties.Add("Level", Level);
            properties.Add("GUID", GUID);
            properties.Add("Mark", Mark);

            var feature = new Feature(BeaconCoordinates, properties);
            return feature;
        }

        /*
        * Set origin point of the buliding to geojson
        */
        public static Feature setOriginPointToGeoJSON(double latitude, double longitude, double altitude)
        {
            var properties = new Dictionary<String, object>();
            properties.Add("Origin Point", "OriginPoint");
            Point OriginCoordinate = new Point(new GeographicPosition(latitude, longitude, altitude));

            var feature = new Feature(OriginCoordinate, properties);
            return feature;
        }

        /*
         * Simple string representation of a beacon
         */
        public String toString()
        {
            return "Category: " + CategoryName +
                ", Beacon Type: " + BeaconType +
                ", Level: " + Level +
                ", GUID: " + GUID +
                ", (" + XLocation + "," + YLocation + "," + ZLocation + ")\n";
        }
    }
}

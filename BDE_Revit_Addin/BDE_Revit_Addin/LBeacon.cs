using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Xml;

namespace BDE
{
    public class LBeacon
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
            Level = level.Name;
            XLocation = lp.X;
            YLocation = lp.Y;
            ZLocation = 0;
            //ZLocation = Convert.ToSingle(level.Name);
            Mark = fi.LookupParameter("Mark").AsString();
            NodeType = fi.LookupParameter("NodeType").AsString();
            Neighbor = fi.LookupParameter("Neighbor").AsString().Split('/');
            UUID = Converter.ToUUID(this).ToUpper();
        }

        public string NodeType
        {
            get; private set;
        }

        public string[] Neighbor
        {
            get; private set;
        }

        public string Region
        {
            get; private set;
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
        public String UUID
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
        public float ZLocation
        {
            get; private set;
        }

        /*
         * Xml Representation of a beacon
         */
        public XmlElement ToXmlElement(XmlDocument xml, string category)
        {
            XmlElement node = xml.CreateElement("node");
            node.SetAttribute("id", this.UUID);
            node.SetAttribute("name", this.Mark);
            node.SetAttribute("region", this.Region);
            node.SetAttribute("category", category);
            return node;
        }

        public XmlElement ToXmlElement(XmlDocument xml)
        {
            XmlElement node = xml.CreateElement("node");
            node.SetAttribute("id", this.UUID);
            node.SetAttribute("name", this.Mark);
            node.SetAttribute("region", this.Region);
            node.SetAttribute("lat", this.YLocation.ToString("##.000000"));
            node.SetAttribute("lon", this.XLocation.ToString("##.000000"));
            for (int i = 0; i < this.Neighbor.Length - 1; i++)
            {
                node.SetAttribute("neighbor" + (i + 1).ToString(), this.Neighbor[i]);
            }
            node.SetAttribute("category", "");
            node.SetAttribute("nodeType", this.NodeType);
            node.SetAttribute("connectPointID", "");
            node.SetAttribute("groupID", "");
            node.SetAttribute("elevation", this.Level);
            return node;
        }

    }
}

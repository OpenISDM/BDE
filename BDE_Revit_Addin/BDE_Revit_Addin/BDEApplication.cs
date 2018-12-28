/*
License and copyright Statement: to be added

Module Name:
 Extract XYZ

Abstract:
This file contains the source code of the Beacon Selection addin for Autodesk Revit 2016.
The following contains code to select and filter beacons, record the coordinates,
and publish technical specifications into a .csv file.

Authors:
Your name (your email address) 15-Oct-2012

Major Revisions:
 None

Environment:
 User mode only.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace BDE
{
    public class BDEApplication : IExternalApplication
    {

        // Both OnStartup and OnShutdown must be implemented as public method
        public Result OnStartup(UIControlledApplication application)
        {
            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("BDE");

            // Create a push button in panel
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Specify a info data for the push button
            PushButtonData buttonData = new PushButtonData("BDE",
               "BeDIPS Development Environment", thisAssemblyPath, "BDE.XYZFamily");

            // Set BDE icon to large image
            buttonData.LargeImage = convertFromBitmap(BDE.Properties.Resources.BDE);

            // Set BED icon to normal image
            buttonData.Image = convertFromBitmap(BDE.Properties.Resources.BDE);

            // Add the button data to push button
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;

            // Add a tip for BDE addin
            pushButton.ToolTip = "Open BDE addin for managing LBeacons in the building";

            return Result.Succeeded;
        }

        public static BitmapSource convertFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // To-do clean up something after addin close

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class XYZFamily : IExternalCommand
    {
        public List<LBeacon> LBeacons { get; set; }

        public class FamilyFilter : ISelectionFilter //implement filter
        {
            bool ISelectionFilter.AllowElement(Element elem)
            {
                // Allow selecting if type Beacon/LaserPointer Family
                if (elem.Name == "30Degree")
                    return true;
                else if (elem.Name == "60Degree")
                    return true;
                else if (elem.Name == "LaserPointer")
                    return true;
                else
                    return false;
                /* Now only the Beacon Family is allowed to be selected       
                If more beacons types are desired, insert above */
            }

            bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            FamilyFilter ff = new FamilyFilter();

            IList<Reference> sel = uidoc.Selection.PickObjects(ObjectType.Element, ff);

            SiteLocation site = doc.SiteLocation;

            // Angles are in radians when coming from Revit API, so we 
            // convert to degrees for display
            const double angleRatio = Math.PI / 180;   // angle conversion factor

            // Get real-word coordinates of the building
            double projectLongitude = site.Longitude / angleRatio;
            double projectLatitude = site.Latitude / angleRatio;

            // Store the output data 
            string pathLBeacon = doc.PathName.Remove(doc.PathName.Length - 4) + ".xml";

            // Create a new list of LBeacons
            LBeacons = new List<LBeacon>();

            foreach (Reference r in sel)
            {
                try
                {
                    Element e = doc.GetElement(r);
                    FamilyInstance fi = e as FamilyInstance;
                    LocationPoint lp = fi.Location as LocationPoint;
                    Level level = e.Document.GetElement(e.LevelId) as Level;

                    // Create a new XYZ for Laser Pointer using in Revit coordinate system
                    XYZ revitXYZ = new XYZ(lp.Point.X, lp.Point.Y, lp.Point.Z);

                    // Create a new beacon and add it to the feature collection as a feature
                    LBeacon beacon = new LBeacon(fi, revitXYZ, level);

                    using (Transaction t = new Transaction(doc, "TextNote"))
                    {
                        t.Start("Create");
                        TextNote.Create(doc, uidoc.ActiveView.Id, revitXYZ, beacon.Mark, doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType));
                        t.Commit();
                    }

                    // Translate the Revit coordinate to Real World coordinate
                    Transform TrueNorthTransform = GetTrueNorthTransform(doc);
                    XYZ TrueNorthCoordinates = TrueNorthTransform.OfPoint(lp.Point);

                    // Convert feet to meter(Revit coordinate system unit is feet.)
                    double xMeter = Utilities.feetToMeters(TrueNorthCoordinates.X);
                    double yMeter = Utilities.feetToMeters(TrueNorthCoordinates.Y);
                    double zMeter = Utilities.feetToMeters(TrueNorthCoordinates.Z);

                    // Create new latitude/longitude
                    double newLatitude = projectLatitude + Utilities.MeterToDecimalDegress(yMeter);
                    double newLongitude = projectLongitude + Utilities.MeterToDecimalDegress(xMeter);

                    // Create a new XYZ for LBeacon using in real-world map
                    XYZ geoXYZ = new XYZ(newLongitude, newLatitude, zMeter);

                    // Create a new beacon and add it to the feature collection as a feature
                    LBeacons.Add(new LBeacon(fi, geoXYZ, level));
                    ReNameNeighbor();
                    LBeacons.Sort((x, y) => { return x.ZLocation.CompareTo(y.ZLocation); });
                    
                    WriteXml(pathLBeacon, doc.Title);
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Revit", e.ToString());
                }
            }

            return Result.Succeeded;
        }

        private Transform GetTrueNorthTransform(Document doc)
        {
            ProjectPosition projectPosition = doc.ActiveProjectLocation.get_ProjectPosition(XYZ.Zero);

            Transform rotationTransform = Transform.CreateRotation(XYZ.BasisZ, projectPosition.Angle);

            return rotationTransform;
        }

        private void ReNameNeighbor()
        {
            for (int i = 0; i < LBeacons.Count; i++)
            {
                for (int j = 0; j < LBeacons[i].Neighbor.Length; j++)
                {
                    foreach (LBeacon other in LBeacons)
                    {
                        if (LBeacons[i].Neighbor[j] == other.Mark)
                        {
                            LBeacons[i].Neighbor[j] = other.UUID;
                        }
                    }
                }
            }
        }

        private void WriteXml(string path, string buildingName)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement building = xmlDocument.CreateElement("Building");
            building.SetAttribute("name", buildingName);
            xmlDocument.AppendChild(building);
            XmlElement region = xmlDocument.CreateElement("region");
            building.AppendChild(region);
            foreach (LBeacon beacon in LBeacons)
            {
                region.AppendChild(beacon.ToXmlElement(xmlDocument, ""));
                building.AppendChild(beacon.ToXmlElement(xmlDocument));
            }
            xmlDocument.Save(path);
        }
    }
}
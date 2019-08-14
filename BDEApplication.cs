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

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

using Newtonsoft.Json;
using GeoJSON.Net.Feature;
using System.Xml;

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

            // Store the output data on desktop
            string pathLBeacon = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + "\\"
                + doc.Title.Remove(doc.Title.Length - 4)
                + "_ForLBeacon"
                + ".xml";

            // Store the output data on desktop
            string pathLaserPointer = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                + "\\"
                + doc.Title.Remove(doc.Title.Length - 4)
                + "_ForLaserPointer"
                + ".xml";

            // Create a new feature collections object, beacons will be represented by features
            FeatureCollection featuresForLBeacon = new FeatureCollection();

            // Create a new feature collections object, beacons and laser pointers will be represented by features
            FeatureCollection featuresForLaserPointer = new FeatureCollection();

            // Set origin point for Laser pointer
            // Origin point also known as Reference Point or Startup Location,
            // which is the internal project origin and fixed on the plate. 
            featuresForLaserPointer.Features.Add(LBeacon.setOriginPointToGeoJSON(0, 0, 0));

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
                    featuresForLaserPointer.Features.Add(new LBeacon(fi, revitXYZ, level).ToGeoJSONFeature());

                    if (fi.Name != "LaserPointer")
                    {
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
                        featuresForLBeacon.Features.Add(new LBeacon(fi, geoXYZ, level).ToGeoJSONFeature());
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("Revit", e.ToString());
                }
            }

            //Overwrite the original file if action is duplicated
            using (StreamWriter sw = new StreamWriter(pathLBeacon, false))
            {
                // Convert the features collection to GeoJSON and output to external file                
                string jsonStr = JsonConvert.SerializeObject(featuresForLBeacon);
               
                XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr, "abc");
                

                using (StringWriter strWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(strWriter))
                    {
                        xmlDoc.WriteTo(xmlWriter);
                        xmlWriter.Flush();
                        sw.WriteLine("1: " + strWriter.GetStringBuilder().ToString());
                    }
                }
                 // sw.WriteLine(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(featuresForLBeacon)).OuterXml);

            }

            //Overwrite the original file if action is duplicated
            using (StreamWriter sw = new StreamWriter(pathLaserPointer, false))
            {
                // Convert the features collection to GeoJSON and output to external file

                string jsonStr = JsonConvert.SerializeObject(featuresForLaserPointer);

                XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr, "abc");


                using (StringWriter strWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(strWriter))
                    {
                        xmlDoc.WriteTo(xmlWriter);
                        xmlWriter.Flush();
                        sw.WriteLine("1: " + strWriter.GetStringBuilder().ToString());
                    }
                }
                //sw.WriteLine(JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(featuresForLaserPointer)).OuterXml);

            }
            return Result.Succeeded;
        }

        Transform GetTrueNorthTransform(Document doc)
        {
            ProjectPosition projectPosition = doc.ActiveProjectLocation.get_ProjectPosition(XYZ.Zero);

            Transform rotationTransform = Transform.CreateRotation(XYZ.BasisZ, projectPosition.Angle);

            return rotationTransform;
        }
    }
}

using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using Genellestirme;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

namespace KadirSahbaz
{
    /// <summary>Bu sınıf binanın hangi küme ve voronoide olduğu bilgisini tutar.</summary>
    public class BinaKumeVoronoi
    {
        #region Değişkenler
        private int voronoiID;
        private int kumeID;
        private int binaID;
        private double length;
        private double area;
        private string type;
        private double compactness;
        private double convexity;
        private double elongation;
        private double orientation;
        private double rectangularity;
        private double granularity;

        public int KumeID { get { return kumeID; } set { kumeID = value; } }
        public int VoronoiID { get { return voronoiID; } set { voronoiID = value; } }
        public int BinaID { get { return binaID; } set { binaID = value; } }
        public double Length { get { return length; } set { length = value; } }
        public double Area { get { return area; } set { area = value; } }
        public string Type { get { return type; } set { type = value; } }
        public double Compactness { get { return compactness; } set { compactness = value; } }
        public double Convexity { get { return convexity; } set { convexity = value; } }
        public double Elongation { get { return elongation; } set { elongation = value; } }
        public double Orientation { get { return orientation; } set { orientation = value; } }
        public double Rectangularity { get { return rectangularity; } set { rectangularity = value; } }
        public double Granularity { get { return granularity; } set { granularity = value; } }
        #endregion

        public BinaKumeVoronoi(IFeature voronoi, IFeature kume, Bina bina)
        {
            this.voronoiID = voronoi.OID;
            this.kumeID = kume.OID;
            this.BinaID = bina.BinaID;
            this.Length = bina.Length;
            this.Area = bina.Area;
            this.Type = bina.Type;
            this.Compactness = bina.Compactness;
            this.Convexity = bina.Convexity;
            this.Elongation = bina.Elongation;
            this.Orientation = bina.Orientation;
            this.Rectangularity = bina.Rectangularity;
            this.Granularity = bina.Granularity;
        }

        public override string ToString()
        {
            string str;
            str = this.binaID.ToString() + ";" +
                  this.length.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.area.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.type + ";" +
                  this.compactness.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.convexity.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.elongation.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.orientation.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.rectangularity.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.granularity.ToString(CultureInfo.InvariantCulture);
            return str;
        }
    } 
    
}

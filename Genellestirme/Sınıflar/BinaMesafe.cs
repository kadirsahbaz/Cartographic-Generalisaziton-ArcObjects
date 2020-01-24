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
    /// <summary>İki bina arasındaki mesafe bilgisini tutar.</summary>
    public class BinaMesafe
    {
        #region Değişkenler
        private int bina1_ID;
        private int bina2_ID;
        private double mesafe;
        private int voronoiID;
        private int kumeID;
        
        public int Bina1_ID { get { return bina1_ID; } set { bina1_ID = value; } }
        public int Bina2_ID { get { return bina2_ID; } set { bina2_ID = value; } }
        public double Mesafe { get { return mesafe; } set { mesafe = value; } }
        public int VoronoiID { get { return voronoiID; } set { voronoiID = value; } }
        public int KumeID { get { return kumeID; } set { kumeID = value; } }        
        #endregion


        /// <param name="b1">Birinci bina</param>
        /// <param name="b2">İkinci bina</param>
        public BinaMesafe(Bina b1, Bina b2)
        {
            IProximityOperator mesafeAnalizi = b1.Shape as IProximityOperator;
            this.Mesafe = mesafeAnalizi.ReturnDistance(b2.Shape as IGeometry);

            this.Bina1_ID = b1.BinaID;
            this.Bina2_ID = b2.BinaID;
        }

        /// <param name="id1">ilk bina id'si</param>
        /// <param name="id2">ikinci bina id'si</param>
        /// <param name="m">İki bina arasındaki mesafe</param>
        /// <param name="v_ID">Binaları içeren voronoi ID</param>
        /// <param name="k_ID">Binaları içeren küme ID</param>
        public BinaMesafe(int id1, int id2, double m, int v_ID, int k_ID)
        {
            this.Bina1_ID = id1;
            this.Bina2_ID = id2;
            this.Mesafe = m;
            this.VoronoiID = v_ID;
            this.KumeID = k_ID;
        }

        /// <summary>İki bina arasındaki en kısa mesafe bilgisini ve binanın ait 
        /// olduğu küme ve voronoi bilgisini tutar</summary>
        /// <param name="b1">Birinci bina</param>
        /// <param name="b2">İkinci bina</param>
        /// <param name="voronoi">Binayı içeren voronoi Feature'ı</param>
        /// <param name="kume">Binayı içeren kume (tampon) Feature'ı</param>
        public BinaMesafe(Bina b1, Bina b2, IFeature voronoi, IFeature kume)
        {
            IProximityOperator mesafeAnalizi = b1.Shape as IProximityOperator;
            this.Mesafe = mesafeAnalizi.ReturnDistance(b2.Shape as IGeometry);

            this.Bina1_ID = b1.BinaID;
            this.Bina2_ID = b2.BinaID;
            this.VoronoiID = voronoi.OID;
            this.KumeID = kume.OID;
        }

        public override string ToString()
        {
            string str = this.bina1_ID.ToString() + ";" +
                this.bina2_ID.ToString() + ";" +
                this.mesafe.ToString(CultureInfo.InvariantCulture);
            return str;
        }
    }
   
}

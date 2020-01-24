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
    public class Bina
    {
        #region Değişkenler

        private int binaID;
        private IPolygon shape;//iki bina toplandığında değişir
        private double length;//Çevre
        private double area;//Alan
        private IPoint centroid;//iki bina toplandığında değişir
        private string type;//bina tipi (ev, resmi, sağlık..)
        private double compactness;
        private double convexity;//Karışık şekilli olma durumu
        private double elongation;//uzanım
        private double orientation;//iki bina toplandığında değişir
        private double rectangularity;//
        private double granularity;//en kısa kenar
        private IFeature feature;

        public int BinaID { get { return binaID; } set { binaID = value; } }
        public IPolygon Shape { get { return shape; } set { shape = value; } }
        public double Length { get { return length; } set { length = value; } }
        public double Area { get { return area; } set { area = value; } }
        public IPoint Centroid { get { return centroid; } set { centroid = value; } }
        public string Type { get { return type; } set { type = value; } }
        public double Compactness { get { return compactness; } set { compactness = value; } }
        public double Convexity { get { return convexity; } set { convexity = value; } }
        public double Elongation { get { return elongation; } set { elongation = value; } }
        public double Orientation { get { return orientation; } set { orientation = value; } }
        public double Granularity { get { return granularity; } set { granularity = value; } }
        public double Rectangularity { get { return rectangularity; } set { if (value == 0) rectangularity = 1; else rectangularity = value; } }
        public IFeature Feature { get { return feature; } set { feature = value; } }
        #endregion

        public Bina(IFeature bina)
        {
            if (bina != null)
            {
                this.binaID = bina.OID;
                this.shape = bina.ShapeCopy as IPolygon;
                this.length = this.Shape.Length;
                this.area = (this.Shape as IArea).Area;
                this.centroid = (this.Shape as IArea).Centroid;
                this.type = Convert.ToString(bina.get_Value(bina.Table.FindField("Type")));
                this.compactness = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Compctness")));
                this.convexity = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Convexity")));
                this.elongation = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Elongation")));
                this.orientation = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Orienttion")));
                this.rectangularity = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Rectnglrty")));
                this.granularity = Convert.ToDouble(bina.get_Value(bina.Table.FindField("Granulrity")));
                this.feature = bina;
            }
        }

        public Bina() { }

        public override string ToString()
        {
            string str;
            str = this.BinaID.ToString() + ";" +
                  this.Length.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Area.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Type + ";" +
                  this.Compactness.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Convexity.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Elongation.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Orientation.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Rectangularity.ToString(CultureInfo.InvariantCulture) + ";" +
                  this.Granularity.ToString(CultureInfo.InvariantCulture);
            return str;
        }

        public static Bina operator |(Bina b1, Bina b2)
        {
            //İki binanın toplamı:
            //İlk binayı, iki binanın ağırlık merkezlerinin ortalama koordinatına
            //ve doğrultusu iki binanın doğrultuları ortalaması olacak şekilde taşıyacağız.

            // ŞARTLAR
            // 1.1. İlk bina kare, ikinci bina kare ise ilk binayı koru.
            //   2. İkinci bina kare değilse ikinci binayı koru.
            // 2.1. İlk bina dikdörtgen, ikinci bina konveks ise ikinci binayı koru.
            //   2. İkinci bina konveks değilse ilk binayı koru.
            // 3.1. İki bina da konveks ise iki binayı da koru.

            //Alttaki satırı kaldırırsam, konveks olan iki binayı da tipikleştirir.
            //if (b1.KonveksMi() && b2.KonveksMi()) return null;

            Bina korunacakBina = b1, silinecekBina = b2;
            if (b1.KareMi() && !b2.KareMi()) { korunacakBina = b2; silinecekBina = b1; }
            else if (b1.DikdortgenMi() && b2.KonveksMi()) { korunacakBina = b2; silinecekBina = b1; }

            if (!korunacakBina.KonveksMi()) // Korunacak bina konveks değilse taşıma işlemi yap.
            {
                IPoint otelemeMiktarı = new PointClass();
                otelemeMiktarı.X = (silinecekBina.Centroid.X - korunacakBina.Centroid.X) / 2; //negatif olabilir.
                otelemeMiktarı.Y = (silinecekBina.Centroid.Y - korunacakBina.Centroid.Y) / 2; //negatif olabilir.
                (korunacakBina.Shape as ITransform2D).Move(otelemeMiktarı.X, otelemeMiktarı.Y);//Centroid değişiyor.
                korunacakBina.Centroid = (korunacakBina.Shape as IArea).Centroid;
            }

            //if (b1.KareMi() && b2.KareMi())
            //{   //Eğer iki bina da kare ise döndürme yapıyoruz.
            //    double donmeMiktari = (silinecekBina.Orientation - korunacakBina.Orientation) / 2; //negatif olabilir.
            //    if (donmeMiktari < 20 && donmeMiktari > -20)
            //    {
            //        (korunacakBina.Shape as ITransform2D).Rotate(korunacakBina.Centroid, donmeMiktari);//Orientation değişiyor.
            //        korunacakBina.Orientation = (korunacakBina.Orientation + silinecekBina.Orientation) / 2;
            //    }
            //}

            return korunacakBina;
        }
        

        public static double EnKisaMesafe(Bina b1, Bina b2)
        {
            IProximityOperator mesafeAnalizi = b1.Shape as IProximityOperator;
            double mesafe = mesafeAnalizi.ReturnDistance(b2.Shape as IGeometry);

            return mesafe;
        }
        public bool KareMi()
        {
            if (this.Convexity == 1 && this.Elongation == 1) return true;
            else return false;
        }
        public bool DikdortgenMi()
        {
            //Convex değilse ve Uzanım 1'den küçükse dikdörtgendir.
            if (this.Convexity == 1 && this.Elongation < 1) return true;
            else return false;
        }
        public bool KonveksMi()
        {
            //Convexity 1'den küçükse konvekstir
            if (this.Convexity < 1) return true;
            else return false;
        }
    }
}

#region Namspaces
using System;
using System.Linq;
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
using System.Xml.Linq;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoDatabaseDistributed;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.CartographyTools;
#endregion


namespace Genellestirme.Buttons
{
    class Class1
    {
        public void DNSTH()
        {
            int binaSayısı = 0, w, koseSayısı = 0;

            double toplamTamponAlan, tamponAlanı = 0, final_mp_spc2_geom_area = 0, w_std_dist = 0, total_square_dist = 0, standartMesafe, ortalamaBinaAlanı, binaCentroidX = 0, binaCentroidY = 0, GBCentroidX = 0, GBCentroidY = 0, mesafe = 0, binalarAgırlıkMerkeziX = 0, binalarAgırlıkMerkeziY = 0, binalarAgırlıkMerkeziXToplam = 0, binalarAgırlıkMerkeziYToplam = 0, toplamBinaAlani = 0, binaAlani = 0, standartMesafeOranı = 0, new_gen_zone_dns, thresh_dns = 0, GBKoseCentroidX=0, GBKoseCentroidY=0;

            w = 1;
            toplamTamponAlan = 0.0;
            binalarAgırlıkMerkeziXToplam = 0.0;
            binalarAgırlıkMerkeziYToplam = 0.0;
            toplamBinaAlani = 0.0;

            while (w <= binaSayısı)
            {
                toplamTamponAlan += tamponAlanı;

                if (binaSayısı > 1)
                {
                    binalarAgırlıkMerkeziXToplam += binaCentroidX * binaAlani;
                    binalarAgırlıkMerkeziYToplam += binaCentroidY * binaAlani;
                    toplamBinaAlani += binaAlani;
                }
                else ortalamaBinaAlanı = binaAlani;
            }

            if (binaSayısı > 1)
            {
                binalarAgırlıkMerkeziX = binalarAgırlıkMerkeziXToplam / toplamBinaAlani;
                binalarAgırlıkMerkeziY = binalarAgırlıkMerkeziYToplam / toplamBinaAlani;
                ortalamaBinaAlanı = toplamBinaAlani / binaSayısı;
                w = 1;
                total_square_dist = 0.0;

                while (w <= binaSayısı)
                {
                    // GB_BinaAgirlikliMesafe
                    mesafe = Math.Sqrt(Math.Pow(binaCentroidX - binalarAgırlıkMerkeziX, 2) + Math.Pow(binaCentroidY - binalarAgırlıkMerkeziY, 2));
                }


                w_std_dist = total_square_dist / Math.Sqrt(binaSayısı);

                total_square_dist = 0.0;

                //# Köşe noktalarını elde etme #
                while (w <= koseSayısı)
                {                   
                    mesafe = Math.Sqrt(Math.Pow(GBKoseCentroidX - GBCentroidX, 2) + Math.Pow(GBKoseCentroidY - GBCentroidY, 2));
                }

                standartMesafe = total_square_dist / Math.Sqrt(koseSayısı);
                standartMesafeOranı = standartMesafe / w_std_dist;
            }
            else standartMesafeOranı = 1.0;

            //# 25 m buffers and map space2 intersection for density calculation #
            ortalamaBinaAlanı = 1;
            new_gen_zone_dns = toplamTamponAlan / final_mp_spc2_geom_area;
            thresh_dns = (95 + 2 * (standartMesafeOranı - toplamTamponAlan / ortalamaBinaAlanı)) / 100.0;

        }






    }

}


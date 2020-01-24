using System;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using KadirSahbaz;

namespace Genellestirme
{
    public class YerlesimAlaniGenislet : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public YerlesimAlaniGenislet()
        {
        }

        public void Calistir()
        {
            OnClick();
        }

        protected override void OnClick()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDoc.FocusMap;
            IActiveView activeView = mxDoc.ActiveView;
            map.ClearSelection();

            IGeoProcessor2 gp = new GeoProcessorClass();
            gp.AddOutputsToMap = true;
            gp.OverwriteOutput = true;

            try
            {
                gp.AddToolbox(@"C:\TEZ\MyTools.tbx");
                IVariantArray parametreler = new VarArrayClass();
                parametreler.Add("Blok_Ada");
                parametreler.Add("Binalar_Bst");
                parametreler.Add(@"C:\TEZ\Default.gdb\Binalar_Buf_2");
                gp.Execute("YerlesimAlaniGenislet1", parametreler, null);
                BinaGEN.Layer("Binalar_Buf_2").Visible = false;
                map.ClearSelection();
                                
                //Yerleşim Alanı ve yakın binaları birleştir
                parametreler = new VarArrayClass();
                parametreler.Add("Yerlesim");
                parametreler.Add("Binalar_Buf_2");
                parametreler.Add("Binalar_Bst");
                parametreler.Add(@"C:\TEZ\Default.gdb\Yerlesim_Bina");
                gp.Execute("YerlesimAlaniGenislet2", parametreler, null);
                BinaGEN.Layer("Yerlesim_Bina").Visible = false;
                map.ClearSelection();

                //Yerlesim+Bina tabakasından 10 m aralıklı noktalar oluştur
                BinaGEN.AraNoktaOlustur(BinaGEN.Layer("Yerlesim_Bina"), 10);
                BinaGEN.Layer("Yerlesim_Bina_N").Visible = false;
                
                //TIN Oluştur
                parametreler = new VarArrayClass();
                parametreler.Add("Yerlesim_Bina_N");
                parametreler.Add(@"C:\TEZ\Default.gdb\Yerlesim_TINEdge");
                gp.Execute("YerlesimAlaniGenislet3", parametreler, null);
                BinaGEN.Layer("Yerlesim_TINEdge").Visible = false;
                map.ClearSelection();

                //Gereksiz TIN'leri sil
                parametreler = new VarArrayClass();
                parametreler.Add("YolOrta");
                parametreler.Add("Yerlesim_TINEdge");
                parametreler.Add(@"C:\TEZ\Default.gdb\Yerlesim_Bina_TINEdge");
                gp.Execute("YerlesimAlaniGenislet4", parametreler, null);
                BinaGEN.Layer("Yerlesim_Bina_TINEdge").Visible = false;
                map.ClearSelection();

                //Kalan TIN kenarlarından Yerleşim Alanı oluştur
                parametreler = new VarArrayClass();
                parametreler.Add("Yerlesim_Bina");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Yerlesim_Ara");
                gp.Execute("YerlesimAlaniGenislet5", parametreler, null);                
                
                //Yerleşim alanına eklenen binaları sil
                parametreler = new VarArrayClass();
                parametreler.Add("Binalar_Bst");
                parametreler.Add("Yerlesim_Ara");
                parametreler.Add(@"C:\TEZ\TezGDB.gdb\Binalar_Yeni");
                gp.Execute("YerlesimAlanindakiBinalariSil", parametreler, null);

                map.ClearSelection();
                mxDoc.UpdateContents();
                activeView.Refresh();
            }
            catch (Exception hata)
            {
                BinaGEN.Mesaj(hata.Source, hata.Message);
            }
            //BinaGEN.Mesaj(" ", "İşlem tamamlandı.");
        }

        protected override void OnUpdate()
        {
        }
    }
}

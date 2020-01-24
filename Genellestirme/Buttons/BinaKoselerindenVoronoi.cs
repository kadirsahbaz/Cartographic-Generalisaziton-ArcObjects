using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using KadirSahbaz;



namespace Genellestirme
{
    public class BinaKoselerindenVoronoi : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public BinaKoselerindenVoronoi()
        {
        }

        public void Calistir()
        {
            OnClick();
        }

        protected override void OnClick()
        {
            IMxDocument mxDoc = ArcMap.Application.Document as IMxDocument;
            ILayer layer = BinaGEN.Layer("Binalar_Yeni");
            mxDoc.FocusMap.ClearSelection();

            BinaGEN.PoligondanVoronoiOlustur(layer);
        }

        protected override void OnUpdate()
        {
        }
    }
}

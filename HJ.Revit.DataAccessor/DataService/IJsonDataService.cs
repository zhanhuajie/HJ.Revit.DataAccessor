namespace HJ.Revit
{
    public interface IJsonDataService
    {
        string GetJsonData(Element elem);
        void SetJsonData(Element elem, string json);
    }
}

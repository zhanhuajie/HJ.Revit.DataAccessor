using Newtonsoft.Json.Linq;

namespace HJ.Revit
{
    public interface IJsonDataService
    {
        JObject GetJsonData(Element elem);
        void SetJsonData(Element elem, JObject jObject);
    }
}

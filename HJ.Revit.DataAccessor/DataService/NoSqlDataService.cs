using Newtonsoft.Json.Linq;

namespace HJ.Revit
{
    class NoSqlDataService : IJsonDataService
    {
        public JObject GetJsonData(Element elem)
        {
            throw new NotImplementedException();
        }

        public void SetJsonData(Element elem, JObject jObject)
        {
            throw new NotImplementedException();
        }
    }
}

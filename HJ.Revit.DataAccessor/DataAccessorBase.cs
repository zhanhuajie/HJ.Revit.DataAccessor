using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Runtime.CompilerServices;

namespace HJ.Revit
{
    public abstract class DataAccessorBase(Element elem)
    {
        protected virtual IJsonDataService DataService { get; } = new EntityDataService();
        protected virtual JsonSerializer Serializer { get; } = JsonSerializer.CreateDefault();
       
        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            try
            {
                var jsonData = JObject.Parse(DataService.GetJsonData(elem));
                return jsonData[name].ToObject<T>(Serializer ?? JsonSerializer.CreateDefault());
            }
            catch
            {
                return default;
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            JObject jsonData;
            try
            {
                jsonData = JObject.Parse(DataService.GetJsonData(elem));
            }
            catch
            {
                jsonData = [];
            }
            jsonData[name] = value == null ? null : JToken.FromObject(value, Serializer ?? JsonSerializer.CreateDefault());
            DataService.SetJsonData(elem, jsonData.ToString());
        }
    }
}

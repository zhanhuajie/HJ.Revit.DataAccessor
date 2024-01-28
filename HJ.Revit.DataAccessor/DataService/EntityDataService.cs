using Autodesk.Revit.DB.ExtensibleStorage;

using Newtonsoft.Json.Linq;

using System.Diagnostics;


namespace HJ.Revit
{
    internal class EntityDataService : IJsonDataService
    {

        private Schema _shcema;
        public Schema Schema
        {
            get
            {
                if (_shcema == null)
                {
                    var builder = new SchemaBuilder(new Guid("{773DDF50-BB10-4CE7-A447-271BA46C8214}"));
                    builder.SetSchemaName("Revit_DataAccessor");
                    builder.SetApplicationGUID(new Guid("{198247C7-51EA-490F-B0EC-C1A5CEFEBF58}"));
                    builder.SetVendorId("Revit_DataAccessor");
                    builder.SetReadAccessLevel(AccessLevel.Public);
                    builder.SetWriteAccessLevel(AccessLevel.Public);
                    builder.SetDocumentation("Revit.DataAccessor Storge");

                    builder.AddSimpleField("Data", typeof(string));

                    _shcema = builder.Finish();
                }
                return _shcema;
            }
        }

        /// <summary>
        /// Get or create entity from element
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        private Entity GetOrCreateEntity(Element elem, Schema schema)
        {
            var entity = elem.GetEntity(schema);
            return entity.IsValid() ? entity : new Entity(schema);
        }

        /// <summary>
        /// Get json data from element
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public string GetJsonData(Element elem)
        {
            _ = elem ?? throw new ArgumentNullException(nameof(elem));
            var callerClassName = GetCallerClassName();
            var entity = elem.GetEntity(Schema);
            if (!entity.IsValid())
            {
                return null;
            }
            try
            {
                return JObject.Parse(entity.Get<string>("Data"))[callerClassName].ToString();
            }
            catch
            {
                return null;
            }
        }
        public void SetJsonData(Element elem, string json)
        {
            _ = elem ?? throw new ArgumentNullException(nameof(elem));
            var callerClassName = GetCallerClassName();
            var entity = GetOrCreateEntity(elem, Schema);
            JObject jObject;
            try
            {
                jObject = JObject.Parse(entity.Get<string>("Data"));
            }
            catch
            {
                jObject = [];
            }
            try
            {
                jObject[callerClassName] = JObject.Parse(json);
            }
            catch
            {
                throw new ArgumentException("Invalid json string", nameof(json));
            }
            entity.Set("Data", jObject.ToString());
            elem.SetEntity(entity);
        }
        private string GetCallerClassName()
        {
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(2);  // 获取调用当前方法的方法的堆栈帧
            var method = stackFrame.GetMethod();
            var declaringType = method.DeclaringType;
            return declaringType != null ? declaringType.FullName : "Unknown";
        }
    }
}

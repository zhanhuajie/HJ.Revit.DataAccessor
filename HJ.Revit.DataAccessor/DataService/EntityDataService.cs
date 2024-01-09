using Autodesk.Revit.DB.ExtensibleStorage;

using Newtonsoft.Json.Linq;


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
                    builder.SetSchemaName("Revit.DataAccessor");
                    builder.SetApplicationGUID(new Guid("{198247C7-51EA-490F-B0EC-C1A5CEFEBF58}"));
                    builder.SetVendorId("Revit.DataAccessor");
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
            // Check arguments
            _ = elem ?? throw new ArgumentNullException(nameof(elem));

            // Get entity
            var entity = elem.GetEntity(Schema);
            // Check entity
            if (!entity.IsValid())
            {
                return null;
            }
            // Get json data
            try
            {
                return entity.Get<string>("Data");
            }
            catch
            {
                return null;
            }
        }
        public void SetJsonData(Element elem, string json)
        {
            _ = elem ?? throw new ArgumentNullException(nameof(elem));
            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch
            {
                throw new ArgumentException("JsonData is not valid");
            }

            // 标记UniqueId
            if (jObject["UniqueId"] == null)
            {
                jObject.AddFirst(new JProperty("UniqueId", elem.UniqueId));
            }
            else
            {
                jObject["UniqueId"] = elem.UniqueId;
            }
            //标记clr-type
            if (jObject["Clr-Type"] == null)
            {

                jObject.AddFirst(new JProperty("Clr-Type", elem.GetType().FullName));
            }
            else
            {
                jObject["Clr-Type"] = elem.GetType().FullName;
            }
            var entity = GetOrCreateEntity(elem, Schema);
            entity.Set("Data", jObject.ToString());
            elem.SetEntity(entity);
        }
    }
}

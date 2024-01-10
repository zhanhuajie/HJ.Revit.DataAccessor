using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Runtime.CompilerServices;

namespace HJ.Revit
{
    public abstract class DataAccessorBase
    {
        private static readonly Dictionary<Guid, Dictionary<string, object>> _globalCache = new();
        private static readonly Dictionary<Guid, int> _counter = new();

        private readonly Guid _guid;
        readonly Document _document;
        readonly ElementId _elementId;
        private Element Element => _document.GetElement(_elementId) ?? throw new InvalidOperationException("Element is not valid");
        private Dictionary<string, object> Cache
        {
            get
            {
                if (!_globalCache.TryGetValue(_guid, out var storage))
                {
                    storage = new();
                    _globalCache.Add(_guid, storage);
                }
                return storage;
            }
        }
        public bool AutoSave { get; set; } = false;
        protected virtual IJsonDataService DataService { get; } = new EntityDataService();
        protected virtual JsonSerializer Serializer { get; } = JsonSerializer.CreateDefault();

        protected DataAccessorBase(Element elem)
        {
            _document = elem?.Document ?? throw new ArgumentNullException(nameof(elem));
            _elementId = elem.Id;
            _guid = elem.GetGuid().Xor(Guid.Parse(MD5Helper.GetMD5Hash(this.GetType().FullName)));
            if (_counter.TryGetValue(_guid, out var count))
            {
                _counter[_guid] = count + 1;
            }
            else
            {
                _counter.Add(_guid, 1);
            }
        }

        ~DataAccessorBase()
        {
            if (_counter.TryGetValue(_guid, out var count))
            {
                if (count == 1)
                {
                    _globalCache.Remove(_guid);
                    _counter.Remove(_guid);
                }
                else
                {
                    _counter[_guid] = count - 1;
                }
            }
        }

        public void ReRead()
        {
            Cache.Clear();
        }
        public void Save()
        {
            JObject jsonData;
            try
            {
                jsonData = JObject.Parse(DataService.GetJsonData(Element));
            }
            catch
            {
                jsonData = new();
            }
            var jObject = jsonData[this.GetType().FullName] as JObject ?? new();
            foreach (var key in Cache.Keys)
            {
                jObject[key] = Cache[key] == null ? null : JToken.FromObject(Cache[key], Serializer ?? JsonSerializer.CreateDefault());
            }
            jsonData[this.GetType().FullName] = jObject;
            DataService.SetJsonData(Element, jsonData.ToString());
            ReRead();
        }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            try
            {
                return (T)Cache[name];
            }
            catch
            {
                JObject jsonData;
                try
                {
                    jsonData = JObject.Parse(DataService.GetJsonData(Element));
                    var t = jsonData[this.GetType().FullName][name].ToObject<T>(Serializer ?? JsonSerializer.CreateDefault());
                    Cache[name] = t;
                    return t;
                }
                catch
                {
                    Cache[name] = default;
                    return default;
                }
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            Cache[name] = value;
            if (AutoSave)
            {
                Save();
            }
        }
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is DataAccessorBase accessor && accessor._guid == _guid;
        }

    }
}

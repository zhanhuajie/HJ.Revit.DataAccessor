using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Runtime.CompilerServices;

namespace HJ.Revit
{
    public abstract class DataAccessorBase
    {
        private static readonly Dictionary<Guid, Dictionary<string, object>> _globalStorage = new();
        private static readonly Dictionary<Guid, int> _counter = new();

        private static Guid GetGuid(Element elem)
        {
            var g1 = new Guid(MD5Helper.GetMD5Hash(elem.Document.GetHashCode().ToString()));
            var g2 = new Guid(MD5Helper.GetMD5Hash(elem.UniqueId));
            return g1.Xor(g2);
        }
        private readonly Guid _guid;
        readonly Document _document;
        readonly ElementId _elementId;
        private Element Element => _document.GetElement(_elementId) ?? throw new InvalidOperationException("Element is not valid");
        private Dictionary<string, object> Storage
        {
            get
            {
                if (!_globalStorage.TryGetValue(_guid, out var storage))
                {
                    storage = new();
                    _globalStorage.Add(_guid, storage);
                }
                return storage;
            }
        }
        public bool AutoSave { get; set; } = false;
        protected virtual IJsonDataService DataService { get; } = new EntityDataService();
        protected virtual JsonSerializer Serializer => JsonSerializer.CreateDefault();

        protected DataAccessorBase(Element elem)
        {
            _document = elem?.Document ?? throw new ArgumentNullException(nameof(elem));
            _elementId = elem.Id;
            _guid = GetGuid(elem).Xor(this.GetType().GUID);
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
                    _globalStorage.Remove(_guid);
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
            Storage.Clear();
        }
        public void Save()
        {
            var jsonData = DataService.GetJsonData(Element) ?? new();
            var jObject = jsonData[this.GetType().FullName] as JObject ?? new();
            foreach (var key in Storage.Keys)
            {
                jObject[key] = Storage[key] == null ? null : JToken.FromObject(Storage[key], Serializer ?? JsonSerializer.CreateDefault());
            }
            jsonData[this.GetType().FullName] = jObject;
            DataService.SetJsonData(Element, jsonData);
            ReRead();
        }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            if (!Storage.TryGetValue(name, out object value))
            {
                var jsonData = DataService.GetJsonData(Element);
                try
                {
                    var t = jsonData[this.GetType().FullName][name].ToObject<T>(Serializer ?? JsonSerializer.CreateDefault());
                    Storage[name] = t;
                    return t;
                }
                catch
                {
                    Storage[name] = default;
                    return default;
                }
            }
            else
            {
                return (T)value;
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            Storage[name] = value;
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

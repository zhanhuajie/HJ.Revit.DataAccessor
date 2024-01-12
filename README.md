# Usage
```c#
using HJ.Revit;

namespace DataAccessor
{
    internal class WallData:DataAccessorBase
    {
        public WallData(Wall wall) : base(wall) { }
        public DateTime BuildDate
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }
        public decimal Cost
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }
    }
}
```

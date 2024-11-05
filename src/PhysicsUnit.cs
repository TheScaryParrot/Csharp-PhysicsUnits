using System.Numerics;

[Serializable]
public class UnitException : Exception
{
    public UnitException() { }
    public UnitException(string message) : base(message) { }
    public UnitException(string message, Exception inner) : base(message, inner) { }
} 

public class PhysicsUnit<T> where T : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>
{
    private class Unit
    {
        public string name;
        public Unit? SIbaseUnit;

        public Unit(string name)
        {
            this.name = name;
        }

        public Unit(string name, Unit SIbaseUnit)
        {
            this.name = name;
            this.SIbaseUnit = SIbaseUnit;
        }

        public bool IsEqual(Unit unit)
        {
            // TODO: Add support for derived units
            return this.name == unit.name;
        }
    }

    private class UnitPower
    {
        public Unit unit;
        public int power;

        public UnitPower(Unit unit, int power)
        {
            this.unit = unit;
            this.power = power;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is not UnitPower other) return false;

            return this.power == other.power && this.unit.IsEqual(other.unit);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    private class UnitList
    {
        public List<UnitPower> units;

        public UnitList()
        {
            units = new List<UnitPower>();
        }

        public UnitList(string unit)
        {
            units = ParseUnit(unit);
        }

        public UnitList(List<UnitPower> units)
        {
            this.units = units;
        }

        private static List<UnitPower> ParseUnit(string unit)
        {
            List<UnitPower> units = new();

            string unitName = "";
            int unitPower = 1;
            int unitSign = 1; 

            for(var i = 0; i < unit.Length; i++)
            {
                char c = unit[i];
                if (Char.IsDigit(c))
                {
                    unitPower = c - '0';
                    c = unit[++i];
                    while (Char.IsDigit(c))
                    {
                        unitPower = unitPower * 10 + (c - '0');
                        c = unit[++i];
                    }
                }
                

                if (c == '*' || c == '/')
                {
                    if (unitName == "") throw new Exception("Invalid unit string. Unit name is empty");

                    units.Add(new UnitPower(new Unit(unitName), unitPower * unitSign));
                    unitName = "";
                    unitPower = 1;
                    unitSign = c == '*' ? 1 : -1; // Negative power for division
                    continue;
                }

                unitName += c;
            }

            if (unitName == "") throw new Exception("Invalid unit string. Unit name is empty. Did you end the string with a '*' or '/'?");
            units.Add(new UnitPower(new Unit(unitName), unitPower * unitSign));

            return units;
        }

        /// <summary>
        /// Combines two UnitLists into one, adding the powers of the same units
        /// </summary>
        /// <param name="invert">Whether the powers of the units of b should be negated (used for division of units)</param>
        public static UnitList Combine(UnitList a, UnitList b, bool invert = false)
        {
            var result = a.units;

            foreach (var unit in b.units)
            {
                var index = result.FindIndex(x => x.unit.IsEqual(unit.unit));

                if (index == -1)
                {
                    if (invert) result.Add(new UnitPower(unit.unit, -unit.power));
                    else result.Add(unit);
                }
                else
                {
                    var power = invert ? -unit.power : unit.power;
                    result[index] = new UnitPower(unit.unit, result[index].power + power );
                
                }
            }

            return new UnitList(result);
        }

        public static bool operator ==(UnitList a, UnitList b)
        {
            if (a.units.Count != b.units.Count) return false;

            foreach (var unit in a.units)
            {
                if (!b.units.Contains(unit)) return false;
            }
            
            return true;
        }

        public static bool operator !=(UnitList a, UnitList b)
        {
            if (a.units.Count != b.units.Count) return true;

            foreach (var unit in a.units)
            {
                if (!b.units.Contains(unit)) return true;
            }
            
            return false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is not UnitList other) return false;

            if (this.units.Count != other.units.Count) return false;

            foreach (var unit in this.units)
            {
                if (!other.units.Contains(unit)) return false;
            }
            
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (units.Count == 0) return "";

            var result = units[0].unit.name + "^" + units[0].power;

            for (int i = 1; i < units.Count; i++)
            {
                result += "*" + units[i].unit.name + "^" + units[i].power;
            }

            return result;
        }
    }

    private T value;
    private UnitList units;

    public PhysicsUnit(T value, string unit)
    {
        this.value = value;
        units = new UnitList(unit);
    }

    private PhysicsUnit(T value, UnitList units)
    {
        this.value = value;
        this.units = units;
    }

    public static PhysicsUnit<T> operator +(PhysicsUnit<T> a, PhysicsUnit<T> b)
    {
        if (a.units != b.units) throw new UnitException("Units " + a.units.ToString() + " and " + b.units.ToString() + " are not the same");

        return new PhysicsUnit<T>(a.value + b.value, a.units);
    }

    public static PhysicsUnit<T> operator -(PhysicsUnit<T> a, PhysicsUnit<T> b)
    {
        if (a.units != b.units) throw new UnitException("Units " + a.units.ToString() + " and " + b.units.ToString() + " are not the same");

        return new PhysicsUnit<T>(a.value - b.value, a.units);
    }

    public static PhysicsUnit<T> operator *(PhysicsUnit<T> a, PhysicsUnit<T> b)
    {
        return new PhysicsUnit<T>(a.value * b.value, UnitList.Combine(a.units, b.units));
    }

    public static PhysicsUnit<T> operator /(PhysicsUnit<T> a, PhysicsUnit<T> b)
    {
        return new PhysicsUnit<T>(a.value / b.value, UnitList.Combine(a.units, b.units, true));
    }

    public static PhysicsUnit<T> operator +(PhysicsUnit<T> a, T b)
    {
        return new PhysicsUnit<T>(a.value + b, a.units);
    }

    public static PhysicsUnit<T> operator -(PhysicsUnit<T> a, T b)
    {
        return new PhysicsUnit<T>(a.value - b, a.units);
    }

    public static PhysicsUnit<T> operator *(PhysicsUnit<T> a, T b)
    {
        return new PhysicsUnit<T>(a.value * b, a.units);
    }

    public static PhysicsUnit<T> operator /(PhysicsUnit<T> a, T b)
    {
        return new PhysicsUnit<T>(a.value / b, a.units);
    }

    public static implicit operator T(PhysicsUnit<T> unit)
    {
        return unit.value;
    }

    public override string ToString()
    {
        return value.ToString() + units.ToString();
    }
}
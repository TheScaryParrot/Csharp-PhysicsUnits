[AttributeUsage(AttributeTargets.All)]
[Obsolete("This attribute is obsolete. Use the PhysicsUnit class instead.")]
public class UnitAttribute : Attribute
{
    private string unit;

    public UnitAttribute(string unit)
    {
        this.unit = unit;
    }
}
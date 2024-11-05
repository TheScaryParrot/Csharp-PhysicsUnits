class MainClass
{
    public static PhysicsUnit<float> distance = new PhysicsUnit<float>(1.0f, "m");

    static void Main()
    {
        distance = distance + 1.0f;
        Console.WriteLine(distance.ToString());
        distance *= new PhysicsUnit<float>(1.0f, "m");
        Console.WriteLine(distance.ToString());

        try
        {
            distance = distance + new PhysicsUnit<float>(1.0f, "s");
        }
        catch (UnitException e)
        {
            Console.WriteLine(e.Message);
        }

        distance /= new PhysicsUnit<float>(5.0f, "m/s");
        Console.WriteLine(distance.ToString());

        distance *= new PhysicsUnit<float>(1.0f, "m");
        distance += new PhysicsUnit<float>(2.0f, "m2*s");
        Console.WriteLine(distance.ToString());
    }
}
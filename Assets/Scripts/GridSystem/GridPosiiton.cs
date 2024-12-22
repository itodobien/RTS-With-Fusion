

public struct GridPosiiton
{
    public int x;
    public int z;
    
    public GridPosiiton(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return $"x: ({x}, z: {z})";
    }
}

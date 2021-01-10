namespace Ganymed.Utils.Structures
{
    public struct Vector4Int
    {
        public int x { get; }
        public int y { get; }
        public int z { get; }
        public int w { get; }
    
    
        public Vector4Int(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    
        public Vector4Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = 0;
        }
    
        public Vector4Int(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
            this.w = 0;
        }
    }
}
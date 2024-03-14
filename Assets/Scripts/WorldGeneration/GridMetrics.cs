
// Stores values that will be used all over the project
using System.Numerics;

public static class GridMetrics {
    public const int NumThreads = 8;
    public const int Scale = 32;
    public const int EdgeDistance = 5;

    public static int[] LODs = {
        8,
        16,
        24,
        32,
        40
    };

    public static int LastLod = LODs.Length - 1;

    public static int PointsPerChunk(int lod) {
        return LODs[lod];
    }

    public static int ThreadGroups(int lod) {
        return LODs[lod] / NumThreads;
    }

    public static int chunk3Dto1D(int X, int Y, int Z, int chunkRadius) {
        chunkRadius *= 2;
        return chunkRadius * chunkRadius * X + chunkRadius * Y + Z;
    }

    public static Vector3 chunk1Dto3D(int n, int chunkRadius) {
        chunkRadius *= 2;
        int X = n / (chunkRadius * chunkRadius);
        int Y = (n % (chunkRadius * chunkRadius)) / chunkRadius;
        int Z = (n % (chunkRadius * chunkRadius)) % chunkRadius;
        return new Vector3(X, Y, Z);
    }
}
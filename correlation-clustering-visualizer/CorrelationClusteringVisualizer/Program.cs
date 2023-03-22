using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class Program {

    static void Main(string[] args) {
        string inputProblemImage = args[0];
        string directory = Directory.GetParent(inputProblemImage).FullName;
        string cnfDirectory = "P:\\Stuff\\School\\gradu\\correlation-clustering\\correlation-clustering-encoder\\local";

        double[,] matrix = FromBitmap(inputProblemImage);
        byte[] bytes = Serializer.Serialize(matrix);
        File.WriteAllBytes($"{inputProblemImage}.matrix", bytes);

        DeletePreviousSolutions(cnfDirectory);

        Console.WriteLine("Waiting for solution...");
        while (!SolutionsExist(cnfDirectory)) {
            Thread.Sleep(500);
        }

        foreach (string file in Directory.GetFiles(cnfDirectory)) {
            if (Path.GetFileName(file).EndsWith(".solution")) {
                Console.WriteLine("Found solution: " + file);
                Visualize(inputProblemImage, file, directory);
            }
        }

    }

    private static void DeletePreviousSolutions(string dir) {
        foreach (string file in Directory.GetFiles(dir)) {
            if (Path.GetFileName(file).EndsWith(".results.txt")) {
                File.Delete(file);
            }
            if (Path.GetFileName(file).EndsWith(".solution")) {
                File.Delete(file);
            }
            if (Path.GetFileName(file).EndsWith(".wcnf")) {
                File.Delete(file);
            }
            if (Path.GetFileName(file).EndsWith(".protowcnf")) {
                File.Delete(file);
            }
        }
    }

    private static bool SolutionsExist(string dir) {
        foreach (string file in Directory.GetFiles(dir)) {
            if (Path.GetFileName(file).EndsWith(".results.txt")) {
                return true;
            }
        }

        return false;
    }

    public static double[,] FromBitmap(string bmpFile) {
        Bitmap img = new Bitmap(bmpFile);
        Console.WriteLine(img.Width);
        Console.WriteLine(img.Height);

        List<Coord> points = new List<Coord>();

        for (int x = 0; x < img.Width; x++) {
            for (int y = 0; y < img.Height; y++) {
                if (img.GetPixel(x, y).ToArgb() == Color.White.ToArgb()) {
                    points.Add(new Coord(x, y));
                }
            }
        }

        double[,] distanceMatrix = new double[points.Count, points.Count];
        double maxDistance = 0;

        for (int i = 0; i < points.Count; i++) {
            distanceMatrix[i, i] = double.PositiveInfinity;

            for (int j = 0; j < points.Count; j++) {
                if (j == i) {
                    continue;
                }
                double distance = Coord.Distance(points[i], points[j]);
                if (distance > maxDistance) {
                    maxDistance = distance;
                }
                //distanceMatrix[i, j] = distance;
                distanceMatrix[i, j] = distance < 10 ? 1 : -1;
            }
        }

        double diff = maxDistance / 2;

        Console.WriteLine("Max distance: " + maxDistance);

        for (int i = 0; i < points.Count; i++) {
            for (int j = 0; j < points.Count; j++) {
                if (j == i) {
                    continue;
                }

                //distanceMatrix[i, j] -= diff;
                // distanceMatrix[i, j] += 7;
            }
        }

        return distanceMatrix;
    }

    private struct Coord {
        public int X;
        public int Y;

        public Coord(int x, int y) {
            X = x;
            Y = y;
        }

        public static double Distance(Coord a, Coord b) {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        override public string ToString() => $"({X}, {Y})";
    }

    public static void Visualize(string imageFile, string solutionFile, string outputDir) {
        Console.WriteLine("Visualize " + solutionFile);
        Bitmap img = new Bitmap(imageFile);

        if (!Serializer.Deserialize<int[]>(File.ReadAllBytes(solutionFile), out int[] solution)) {
            throw new Exception("Unable to load solution");
        }

        Console.WriteLine("Solution length: " + solution.Length);

        int index = 0;
        for (int x = 0; x < img.Width; x++) {
            for (int y = 0; y < img.Height; y++) {
                if (img.GetPixel(x, y).ToArgb() == Color.White.ToArgb()) {
                    img.SetPixel(x, y, UniqueColor.GetColor(solution[index] + 2));
                    index++;
                }
                // img.SetPixel(x, y, Color.FromArgb(x, y, 0));
            }
        }

        string path = $"{outputDir}/{Path.GetFileName(solutionFile)}.bmp";
        Console.WriteLine("Save solution: " + path);
        img.Save(path);
    }
}

public static class UniqueColor {
    public static Color GetColor(int i) {
        int[] p = getPattern(i);
        return Color.FromArgb(getElement(p[0]), getElement(p[1]), getElement(p[2]));
    }

    public static int getElement(int index) {
        int value = index - 1;
        int v = 0;
        for (int i = 0; i < 8; i++) {
            v = v | (value & 1);
            v <<= 1;
            value >>= 1;
        }
        v >>= 1;
        return v & 0xFF;
    }

    public static int[] getPattern(int index) {
        int n = (int)Math.Pow(index, (double)1 / 3);
        index -= (n * n * n);
        int[] p = new int[3] { n, n, n };
        if (index == 0) {
            return p;
        }
        index--;
        int v = index % 3;
        index = index / 3;
        if (index < n) {
            p[v] = index % n;
            return p;
        }
        index -= n;
        p[v] = index / n;
        p[++v % 3] = index % n;
        return p;
    }
}

public static class Serializer {
    public static byte[] Serialize<T>(T o) {
        if (!typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)))) {
            throw new InvalidOperationException("A serializable Type is required");
        }
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream()) {
            bf.Serialize(ms, o);
            return ms.ToArray();
        }
    }

    public static bool Deserialize<T>(byte[] bytes, out T target) {
        using (var memStream = new MemoryStream()) {
            var binForm = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            try {
                target = (T)obj;
                return true;
            } catch (System.Exception e) {
                Console.WriteLine("Error deserializing to type: " + e.Message + "\n\n" + e.StackTrace);
                target = default;
                return false;
            }
        }
    }

    public static T Copy<T>(T o) {
        T target;
        Deserialize<T>(Serialize(o), out target);
        return target;
    }

    public static bool IsSerializable<T>(T o) {
        try {
            Serialize(o);
            return true;
        } catch {
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CorrelationClusteringEncoder;

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
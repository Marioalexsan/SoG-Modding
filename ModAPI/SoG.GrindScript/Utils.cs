using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding
{

    public static class Utils
    {
        public static void TryCreateDirectory(string name)
        {
            try
            {
                Directory.CreateDirectory(name);
            }
            catch (Exception) { }
        }
    }

    public static partial class TypeExtension
    {
        public static MethodInfo GetPublicInstanceMethod(this TypeInfo t,string name)
        {
            return t.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
        }

        public static MethodInfo[] GetPublicInstanceOverloadedMethods(this TypeInfo t, string name)
        {
            return t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == name).ToArray();
        }

        public static MethodInfo GetPrivateInstanceMethod(this TypeInfo t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static MethodInfo GetPrivateStaticMethod(this TypeInfo t, string name)
        {
            return t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static T GetPrivateInstanceField<T>(this TypeInfo t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance);
        }

        public static T GetPublicInstanceField<T>(this TypeInfo t, object instance, string field)
        {
            return (T)t.GetField(field, BindingFlags.Instance | BindingFlags.Public)?.GetValue(instance);
        }

        public static T GetPublicStaticField<T>(this TypeInfo t, string field)
        {
            return (T) t.GetField(field, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }

        public static dynamic GetPublicStaticField(this TypeInfo t, string field)
        {
            return t.GetField(field, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }
    }
}

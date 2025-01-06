using UnityEngine;
using System;

namespace Higo.UI
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CreateAssetAttribute : PropertyAttribute
    {
        public Type Type;
        public string Ext;
        public string Path;
        public CreateAssetAttribute(Type type, string ext)
        {
            Type = type;
            Ext = ext;
        }
    }

}
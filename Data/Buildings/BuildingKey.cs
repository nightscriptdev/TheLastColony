using Enums;
using System;

namespace Data.Buildings
{
    public readonly struct BuildingKey : IEquatable<BuildingKey>
    {
        public readonly BuildingType Type;
        public readonly int Level;
    
        public BuildingKey(BuildingType type, int level)
        {
            Type = type;
            Level = level;
        }
    
        // 实现IEquatable接口，确保字典能正确比较
        public bool Equals(BuildingKey other)
        {
            return Type == other.Type && Level == other.Level;
        }
    
        public override bool Equals(object obj)
        {
            return obj is BuildingKey other && Equals(other);
        }
    
        // 重写GetHashCode，确保字典性能
        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Level);
        }
    
        public override string ToString()
        {
            return $"{Type}_Level{Level}";
        }
    
        // 便捷的操作符重载
        public static bool operator ==(BuildingKey left, BuildingKey right)
        {
            return left.Equals(right);
        }
    
        public static bool operator !=(BuildingKey left, BuildingKey right)
        {
            return !left.Equals(right);
        }
    }
}
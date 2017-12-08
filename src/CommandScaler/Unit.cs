using System;
using System.Threading.Tasks;

namespace CommandScaler
{
    public struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        public static readonly Unit Value = new Unit();

        public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

        public int CompareTo(Unit other) => 0;

        int IComparable.CompareTo(object obj) => 0;

        public override int GetHashCode() => 0;

        public bool Equals(Unit other) => true;

        public override bool Equals(object obj)=>obj is Unit;

        public static bool operator ==(Unit first, Unit second) => true;

        public static bool operator !=(Unit first, Unit second) => false;

        public override string ToString() => "()";
    }
}

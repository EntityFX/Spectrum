namespace EntityFX.Core.Common.Cpu
{
    public interface ILookupTables
    {
        byte[] HalfcarryAdd { get; }
        byte[] HalfcarrySub { get; }
        byte[] OverflowAdd { get; }
        byte[] OverflowSub { get; }
        byte[] Sz53 { get; }
        byte[] Parity { get; }
        byte[] Sz53P { get; }
        void Init();
    }
}
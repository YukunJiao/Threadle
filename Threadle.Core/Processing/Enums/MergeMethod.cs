namespace Threadle.Core.Processing.Enums
{
    /// <summary>
    /// Enum specifying how two layers of the same mode are combined when merged into a new layer.
    /// Used by the mergelayers command.
    /// For 1-mode layers, the method fully determines the resulting layer's edge type:
    /// And/Or/Xor always produce a binary layer; Sum/Average always produce a valued layer;
    /// Max/Min/Product produce a binary layer only if both input layers are binary, valued otherwise.
    /// For 2-mode layers, only Or (union of hyperedges and their nodes) and And (intersection) are valid.
    /// </summary>
    public enum MergeMethod
    {
        And,
        Or,
        Xor,
        Sum,
        Average,
        Max,
        Min,
        Product
    }
}
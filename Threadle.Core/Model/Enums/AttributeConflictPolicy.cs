namespace Threadle.Core.Model.Enums
{
    /// <summary>
    /// Declares how to resolve a node attribute that has different values set in both sources
    /// being merged (see <see cref="Nodeset.Merge"/> and <see cref="Network.Merge"/>). Only applies
    /// when both sources actually have a value set for the same node and attribute, and those
    /// values differ; a value present in only one source is never a conflict.
    /// </summary>
    public enum AttributeConflictPolicy
    {
        /// <summary>
        /// Fail the merge, reporting the first node/attribute found with conflicting values.
        /// </summary>
        Fail,

        /// <summary>
        /// Keep the value from the first source.
        /// </summary>
        KeepFirst,

        /// <summary>
        /// Keep the value from the second source.
        /// </summary>
        KeepLast
    }
}
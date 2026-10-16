using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Model;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'duplicate' CLI command.
    /// </summary>
    public class Duplicate : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "duplicate(structure = [var:structure], type = ['layer','attribute'], from = [str], to = [str])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Duplicates either a layer or a node attribute under a new name, leaving the original untouched. When type is 'layer', 'structure' must be a Network variable, and 'from'/'to' are the existing layer's name and the new layer's name: all edges and layer properties (directionality, value type, selfties for 1-mode; hyperedges for 2-mode) are copied. When type is 'attribute', 'structure' is a Nodeset (or the Nodeset of the provided Network), and 'from'/'to' are the existing node attribute's name and the new attribute's name: the attribute's type and all currently set values are copied. Duplicating to a name already in use, or duplicating a layer/attribute that does not exist, fails without changing anything.";

        /// <summary>
        /// Gets a value indicating whether this command produces output that must be assigned to a variable.
        /// </summary>
        public bool ToAssign => false;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="command">The parsed <see cref="CommandPackage"/> to be executed.</param>
        /// <param name="context">The <see cref="CommandContext"/> providing shared console variable memory.</param>
        public CommandResult Execute(CommandPackage command, CommandContext context)
        {
            string structureName = command.GetArgumentThrowExceptionIfMissingOrNull("structure", "arg0");
            string type = command.GetArgumentThrowExceptionIfMissingOrNull("type", "arg1");
            string from = command.GetArgumentThrowExceptionIfMissingOrNull("from", "arg2");
            string to = command.GetArgumentThrowExceptionIfMissingOrNull("to", "arg3");

            switch (type.ToLowerInvariant())
            {
                case "layer":
                    if (CommandHelpers.TryGetVariable<Network>(context, structureName, out var network) is CommandResult networkFail)
                        return networkFail;
                    return CommandResult.FromOperationResult(network.DuplicateLayer(from, to));
                case "attribute":
                    if (CommandHelpers.TryGetNodesetFromIStructure(context, structureName, out var nodeset) is CommandResult nodesetFail)
                        return nodesetFail;
                    return CommandResult.FromOperationResult(nodeset!.DuplicateNodeAttribute(from, to));
                default:
                    return CommandResult.Fail("InvalidArgument", $"Unknown duplicate type '{type}': expected 'layer' or 'attribute'.");
            }
        }
    }
}
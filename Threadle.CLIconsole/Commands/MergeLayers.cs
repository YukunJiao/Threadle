using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Model;
using Threadle.Core.Processing;
using Threadle.Core.Processing.Enums;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'mergelayers' CLI command.
    /// </summary>
    public class MergeLayers : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "mergelayers(network = [var:network], layer1 = [str], layer2 = [str], *method = ['and','or','xor','sum','average','max','min','product'], *newlayername = [str])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Merges two layers of the same mode into a new layer of that mode by the provided method: the original layers remain unchanged. A 1-mode layer cannot be merged with a 2-mode layer. Merging two 1-mode layers results in a new 1-mode layer. Both layers must have the same directionality (both directed or both undirected): symmetrize the directed one first with symmetrize() if they don't match. All methods can be used (default 'max'), and the method fully determines the resulting layer's edge type: 'and'/'or'/'xor' always produce a binary layer (does a tie exist, ignoring its strength); 'sum'/'average' always produce a valued layer; 'max'/'min'/'product' produce a binary layer only if both source layers are binary, valued otherwise. For binary layers, 'and' is equivalent to 'product' and 'or' is equivalent to 'max'; 'xor' (true only if exactly one layer has the tie) has no equivalent among the others. Merging two 2-mode layers results in a new 2-mode layer, where hyperedges are matched by name. Only 'or' (default) and 'and' can be used: 'or' takes the union, i.e. all hyperedges that exist in either layer, where a hyperedge that exists in both layers gets the nodes it has in either layer; 'and' takes the intersection, i.e. only hyperedges that exist in both layers, each with the nodes it has in both layers. An optional name for the new layer can be specified: otherwise, it will be automatically named.";

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
            if (CommandHelpers.TryGetVariable<Network>(context, command.GetArgumentThrowExceptionIfMissingOrNull("network", "arg0"), out var network) is CommandResult commandResult)
                return commandResult;
            string layerName1 = command.GetArgumentThrowExceptionIfMissingOrNull("layer1", "arg1");
            string layerName2 = command.GetArgumentThrowExceptionIfMissingOrNull("layer2", "arg2");
            // No method given: the default depends on the layers' mode ('max' for 1-mode, 'or' for 2-mode).
            MergeMethod? method = Enum.TryParse<MergeMethod>(command.GetArgument("method"), true, out var parsedMethod) ? parsedMethod : null;
            string newLayerName = network.GetNextAvailableLayerName(command.GetArgumentParseString("newlayername", layerName1 + "-" + layerName2 + "-merged"));
            return CommandResult.FromOperationResult(NetworkProcessor.MergeLayers(network, layerName1, layerName2, method, newLayerName));
        }
    }
}
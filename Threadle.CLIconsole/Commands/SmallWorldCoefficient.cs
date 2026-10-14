using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Analysis;
using Threadle.Core.Model;
using Threadle.Core.Model.Enums;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'smallworldcoefficient' CLI command.
    /// </summary>
    public class SmallWorldCoefficient : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "smallworldcoefficient(network=[var:network], layername=[str], *nulltrials=[int(default:10)], *traversal=['out'(default),'in','both'])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Computes the small-world coefficient sigma (Humphries & Gurney 2008) for a single 1-mode layer: (observed clustering / random clustering) / (observed average path length / random average path length). sigma > 1 indicates small-world structure — markedly more clustered than a comparable random graph, without a correspondingly longer average path length. The random reference is the mean over 'nulltrials' independent degree-preserving rewirings of the layer (same mechanism as rewire(), default 2-edge swap; each trial is discarded after measuring, never added to the network). Clustering uses whichever formula clusteringcoefficient() would auto-select for the layer's type; average path length is diameter()'s AvgDistance, restricted to the largest weakly connected component in both the observed and each random-reference network for a like-for-like comparison. For directed layers, 'traversal' controls which direction BFS follows for the path-length measurements. Returns Sigma, ObservedClustering, ObservedAvgDistance, RandomClustering, RandomAvgDistance and NullTrials. Reports only sigma, not Telesford et al. (2011)'s omega, which additionally needs a ring-lattice reference Threadle does not currently generate. References: Watts & Strogatz (1998) doi:10.1038/30918; Humphries & Gurney (2008) doi:10.1371/journal.pone.0002051.";

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
            string layerName = command.GetArgumentThrowExceptionIfMissingOrNull("layername", "arg1");
            int nullTrials = command.GetArgumentParseInt("nulltrials", 10);
            string traversalStr = command.GetArgumentParseString("traversal", "out");
            EdgeTraversal traversal = traversalStr.ToLowerInvariant() switch
            {
                "in" => EdgeTraversal.In,
                "both" => EdgeTraversal.Both,
                _ => EdgeTraversal.Out
            };
            var result = Analyses.SmallWorldCoefficient(network, layerName, nullTrials, traversal);
            return CommandResult.FromOperationResult(result, result.Value);
        }
    }
}
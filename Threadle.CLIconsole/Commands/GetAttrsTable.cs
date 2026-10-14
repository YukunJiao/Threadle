using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Utilities;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'getattrstable' CLI command.
    /// </summary>
    public class GetAttrsTable : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "[table] = getattrstable(structure = [var:structure], *nodes = [semicolon-separated uints], *attrnames = [semicolon-separated str], *offset = [int(default:0)], *limit = [int(default:1000)])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Returns a table with one row per node and one column per node attribute - the bulk-export counterpart to importnodeattributes(). The first column is 'node' (the node id); the remaining columns are named after the requested attributes. 'nodes' selects which nodes to include (semicolon-separated); if omitted, nodes are taken from the Nodeset itself, paginated with 'offset'/'limit' exactly like getallnodes(). 'attrnames' selects which attributes to include as columns (semicolon-separated); if omitted, all currently defined node attributes are included. A node that is not in the Nodeset, or that lacks a value for a given attribute, gets a null entry for that attribute rather than failing the whole call - a left join, same spirit as importnodeattributes(). In JSON output, the payload has one array per column, directly usable as an R data frame.";

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
            if (CommandHelpers.TryGetNodesetFromIStructure(context, command.GetArgumentThrowExceptionIfMissingOrNull("structure", "arg0"), out var nodeset) is CommandResult commandResult)
                return commandResult;

            string nodesStr = command.GetArgumentParseString("nodes", "");
            uint[] nodeIds;
            if (nodesStr.Length > 0)
            {
                if (Misc.SplitStringToUintArray(nodesStr) is not uint[] parsedNodeIds)
                    return CommandResult.Fail("ParseError", "Could not parse semicolon-separated list with nodes.");
                nodeIds = parsedNodeIds;
            }
            else
            {
                int offset = command.GetArgumentParseInt("offset", 0);
                int limit = command.GetArgumentParseInt("limit", 1000);
                nodeIds = nodeset!.GetAllNodes(offset, limit).Value?.ToArray() ?? [];
            }

            string attrNamesStr = command.GetArgumentParseString("attrnames", "");
            string[]? attrNames = attrNamesStr.Length > 0 ? attrNamesStr.Split(';') : null;

            var result = nodeset!.GetNodeAttributesTable(nodeIds, attrNames);
            if (!result.Success)
                return CommandResult.FromOperationResult(result);

            var table = new TablePayload().AddColumn("node", nodeIds);
            foreach (var (columnName, values) in result.Value!)
                table.AddColumn(columnName, values);
            return CommandResult.FromOperationResult(result, table);
        }
    }
}
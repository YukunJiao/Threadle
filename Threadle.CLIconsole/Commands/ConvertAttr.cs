using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'convertattr' CLI command.
    /// </summary>
    public class ConvertAttr : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "convertattr(structure = [var:structure], from = [str], to = [str], attrtype = ['int','char','float','bool','string'])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Converts a node attribute to a new type, storing the result under a new name 'to' and leaving the original attribute 'from' untouched. Each node's value is converted independently - formatted as a string and re-parsed as the new type, the same way a string CLI argument is parsed for that type - so a value that doesn't convert (e.g. a non-numeric string to 'int', or a string longer than one character to 'char') is simply left unset on the new attribute for that node, rather than failing the whole command. Converting a string to 'float' or 'int' keeps the parser's native handling of 'NaN'/'Infinity' as real values, not as missing data. Fails without changing anything if 'from' doesn't exist, 'to' is already in use, or 'attrtype' isn't recognized.";

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
            string from = command.GetArgumentThrowExceptionIfMissingOrNull("from", "arg1");
            string to = command.GetArgumentThrowExceptionIfMissingOrNull("to", "arg2");
            string attrType = command.GetArgumentThrowExceptionIfMissingOrNull("attrtype", "arg3");
            return CommandResult.FromOperationResult(nodeset!.ConvertNodeAttributeType(from, to, attrType));
        }
    }
}
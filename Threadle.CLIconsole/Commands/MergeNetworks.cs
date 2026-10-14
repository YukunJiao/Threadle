using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Model;
using Threadle.Core.Model.Enums;
using Threadle.Core.Utilities;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'mergenetworks' CLI command.
    /// </summary>
    public class MergeNetworks : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "[var:network] = mergenetworks(network1 = [var:network], network2 = [var:network], *name = [str], *onvalueconflict = ['fail'(default),'keepfirst','keeplast'])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Merges two networks into a new one, stored in [var:network]: their nodesets are merged exactly as mergenodesets() does (see that command for how attribute definitions and values are resolved, including the 'onvalueconflict' argument), and every layer from both networks is copied into the merged network over that unified nodeset. A layer name occurring in both networks is kept side-by-side rather than combined: the one from the second network is suffixed with its source network's name (e.g. 'kinship_net2024') so both survive distinctly, allowing year-over-year comparison afterward. Consider renaming a layer with a more meaningful period/source label (see rename()) before merging rather than relying on this automatic suffix - the same convention recommended for per-period node attributes before merging nodesets.";

        /// <summary>
        /// Gets a value indicating whether this command produces output that must be assigned to a variable.
        /// </summary>
        public bool ToAssign => true;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="command">The parsed <see cref="CommandPackage"/> to be executed.</param>
        /// <param name="context">The <see cref="CommandContext"/> providing shared console variable memory.</param>
        public CommandResult Execute(CommandPackage command, CommandContext context)
        {
            string variableName = command.GetAssignmentVariableNameThrowExceptionIfNull();
            if (CommandHelpers.TryGetVariable<Network>(context, command.GetArgumentThrowExceptionIfMissingOrNull("network1", "arg0"), out var network1) is CommandResult fail1)
                return fail1;
            if (CommandHelpers.TryGetVariable<Network>(context, command.GetArgumentThrowExceptionIfMissingOrNull("network2", "arg1"), out var network2) is CommandResult fail2)
                return fail2;
            string name = command.GetArgumentParseString("name", variableName);
            if (!Misc.IsNameWithinBinaryLimit(name))
                return CommandResult.Fail("NameTooLong", $"The network name - '{name}' - is too long; can max be 255 UTF8 bytes.");
            AttributeConflictPolicy onValueConflict = command.GetArgumentParseEnum<AttributeConflictPolicy>("onvalueconflict", AttributeConflictPolicy.Fail);

            var result = Network.Merge(network1, network2, name, onValueConflict);
            if (!result.Success)
                return CommandResult.FromOperationResult(result);
            context.SetVariable(variableName, result.Value!);
            return CommandResult.Ok(
                message: result.Message,
                assignments: CommandResult.Assigning(variableName, typeof(Network))
                );
        }
    }
}
using Threadle.CLIconsole.Parsing;
using Threadle.CLIconsole.Results;
using Threadle.CLIconsole.Runtime;
using Threadle.Core.Model;
using Threadle.Core.Model.Enums;
using Threadle.Core.Utilities;

namespace Threadle.CLIconsole.Commands
{
    /// <summary>
    /// Class representing the 'mergenodesets' CLI command.
    /// </summary>
    public class MergeNodesets : ICommand
    {
        /// <summary>
        /// Gets the command syntax definition as shown in help and usage output.
        /// </summary>
        public string Syntax => "[var:nodeset] = mergenodesets(nodeset1 = [var:nodeset], nodeset2 = [var:nodeset], *name = [str], *onvalueconflict = ['fail'(default),'keepfirst','keeplast'])";

        /// <summary>
        /// Gets a human-readable description of what the command does.
        /// </summary>
        public string Description => "Merges two nodesets into a new one, stored in [var:nodeset]: the union of their node ids, and the union of their defined node attributes. An attribute name defined in both with a different type always fails the merge - that signals two genuinely different attributes sharing a name by accident. For each node and attribute: if only one source has a value set, that value is used; if both have a value set and they agree, that value is used; if both have a value set and disagree, 'onvalueconflict' decides ('fail' by default, or 'keepfirst'/'keeplast' to prefer nodeset1/nodeset2). A value missing in one source is never treated as a conflict - only two different values for the same node and attribute are. Attributes expected to vary between the two sources (e.g. a per-period measure like income) should be renamed with their period beforehand (see rename()), so that only genuinely static attributes (e.g. gender, birthyear) remain under a shared name and risk a value conflict.";

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
            if (CommandHelpers.TryGetVariable<Nodeset>(context, command.GetArgumentThrowExceptionIfMissingOrNull("nodeset1", "arg0"), out var nodeset1) is CommandResult fail1)
                return fail1;
            if (CommandHelpers.TryGetVariable<Nodeset>(context, command.GetArgumentThrowExceptionIfMissingOrNull("nodeset2", "arg1"), out var nodeset2) is CommandResult fail2)
                return fail2;
            string name = command.GetArgumentParseString("name", variableName);
            if (!Misc.IsNameWithinBinaryLimit(name))
                return CommandResult.Fail("NameTooLong", $"The nodeset name - '{name}' - is too long; can max be 255 UTF8 bytes.");
            AttributeConflictPolicy onValueConflict = command.GetArgumentParseEnum<AttributeConflictPolicy>("onvalueconflict", AttributeConflictPolicy.Fail);

            var result = Nodeset.Merge(nodeset1, nodeset2, name, onValueConflict);
            if (!result.Success)
                return CommandResult.FromOperationResult(result);
            context.SetVariable(variableName, result.Value!);
            return CommandResult.Ok(
                message: result.Message,
                assignments: CommandResult.Assigning(variableName, typeof(Nodeset))
                );
        }
    }
}
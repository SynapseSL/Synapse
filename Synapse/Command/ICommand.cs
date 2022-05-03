namespace Synapse.Command
{
    public interface ICommand
    {
        CommandResult Execute(CommandContext context);

        string Name
        {
            get;
            set;
        }
        
        string[] Aliases
        {
            get;
            set;
        }
        
        string Permission
        {
            get;
            set;
        }

        string Usage
        {
            get;
            set;
        }

        string[] Arguments
        {
            get;
            set;
        }
        
        string Description
        {
            get;
            set;
        }

        Platform[] Platforms
        {
            get;
            set;
        }
    }
    

}
